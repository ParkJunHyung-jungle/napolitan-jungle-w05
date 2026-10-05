using UnityEngine;
/// <summary>
/// 1인칭 카메라. Player 자식의 Main Camera에 붙인다.
/// 입력에 따른 좌우(몸) / 상하(카메라) 회전과 조준 Ray 제공을 담당한다.
/// 이동과 상호작용 판정은 FirstPersonController가 한다.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class FirstPersonCamera : MonoBehaviour
{
    [Header("Rotation")]
    [Tooltip("좌우 회전 대상. 비어 있으면 부모 Transform")]
    [SerializeField] private Transform playerBody;

    [Header("Look")]
    [SerializeField] private float sensitivity = 0.1f;
    [SerializeField] private float minPitch = -85f;
    [SerializeField] private float maxPitch = 85f;

    [Header("Interact")]
    [SerializeField] private float _aimDistance = 4f;

    private Camera _camera;
    private float _pitch;
    private bool _isLookLocked;
    private float _lookScale = 1f;
    private int _aimLayerMask;
    private bool _hasAimHit;
    private RaycastHit _aimHit;
    private Outline _outlinedTarget;

    /// <summary>플레이어 카메라. 드래그 대상이 화면 좌표를 계산할 때 쓴다.</summary>
    public Camera Camera => _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (playerBody == null) playerBody = transform.parent;
        _aimLayerMask = Physics.DefaultRaycastLayers & ~LayerMask.GetMask("Player");

        // 카메라가 몸의 자식이 아니면 좌우 회전과 이동이 화면에 반영되지 않는다
        if (playerBody == null || !transform.IsChildOf(playerBody))
            Debug.LogWarning($"[FirstPersonCamera] {name}이(가) Player Body의 자식이 아닙니다. 카메라를 Player 아래로 옮기세요.", this);

        // 씬에 배치된 카메라 각도에서 시작한다. (localEulerAngles는 0~360이므로 -180~180으로 변환)
        _pitch = Mathf.Clamp(Mathf.DeltaAngle(0f, transform.localEulerAngles.x), minPitch, maxPitch);
    }

    // ---------- Rotation ----------

    private void Update()
    {
        if (!_isLookLocked)
        {
            // 마우스 delta는 이미 프레임 이동량이므로 deltaTime을 곱하지 않는다
            Vector2 delta = Managers.Input.LookInput * (sensitivity * _lookScale);

            _pitch = Mathf.Clamp(_pitch - delta.y, minPitch, maxPitch);

            playerBody.Rotate(Vector3.up, delta.x);
            transform.rotation = playerBody.rotation * Quaternion.Euler(_pitch, 0f, 0f);
            transform.position = playerBody.position + new Vector3(0, 1.5f, 0);
        }

        UpdateAimTarget();
    }

    // ---------- Public API ----------

    /// <summary>카메라 위치에서 카메라 정면 방향. 화면 중앙 조준선과 같은 선이다.</summary>
    public Ray GetAimRay()
    {
        return new Ray(transform.position, transform.forward);
    }

    /// <summary>
    /// 현재 프레임의 조준 Raycast 결과를 반환한다.
    /// 카메라 Update에서 갱신한 적중 상태와 정보를 hit에 전달한다.
    /// </summary>
    public bool TryGetAimHit(out RaycastHit hit)
    {
        hit = _aimHit;
        return _hasAimHit;
    }

    /// <summary>
    /// 중앙 조준 Ray를 3m까지 검사하고 맞은 Outline을 갱신한다.
    /// 적중 정보를 저장해 아웃라인과 상호작용에서 같은 대상을 사용하게 한다.
    /// </summary>
    private void UpdateAimTarget()
    {
        _hasAimHit = Physics.Raycast(
            GetAimRay(),
            out _aimHit,
            _aimDistance,
            _aimLayerMask,
            QueryTriggerInteraction.Ignore);

        Outline nextTarget = _hasAimHit
            ? _aimHit.collider.GetComponentInParent<Outline>()
            : null;

        if (nextTarget != null && !nextTarget.isActiveAndEnabled)
            nextTarget = null;

        if (_outlinedTarget == nextTarget)
            return;

        if (_outlinedTarget != null)
            _outlinedTarget.SetOutline(false);

        _outlinedTarget = nextTarget;

        if (_outlinedTarget != null)
            _outlinedTarget.SetOutline(true);
    }

    /// <summary>시점 감도 배율. 1이면 원래 감도, 0이면 돌지 않는다. (슬라이더 드래그 중 저감도용)</summary>
    public void SetLookScale(float scale)
    {
        _lookScale = Mathf.Max(0f, scale);
    }
}
