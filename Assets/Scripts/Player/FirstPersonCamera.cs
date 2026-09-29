using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 1인칭 카메라. Player 자식의 Main Camera에 붙인다.
/// 마우스로 좌우(몸) / 상하(카메라) 회전, 커서 잠금, 조준 Ray 제공을 담당한다.
/// 이동과 상호작용 판정은 FirstPersonController가 한다.
/// </summary>
[DisallowMultipleComponent]
public class FirstPersonCamera : MonoBehaviour
{
    [Header("Rotation")]
    [Tooltip("좌우 회전 대상. 비어 있으면 부모 Transform")]
    [SerializeField] private Transform playerBody;

    [Tooltip("마우스 delta(픽셀) 배율")]
    [SerializeField] private float sensitivity = 0.1f;

    [Tooltip("상하 각도 제한 (아래로 볼 때 양수)")]
    [SerializeField] private float minPitch = -85f;
    [SerializeField] private float maxPitch = 85f;

    [Header("Cursor")]
    [Tooltip("활성화 시 커서 잠금")]
    [SerializeField] private bool lockCursorOnEnable = true;

    private Vector2 _lookInput;
    private float _pitch;
    private bool _isLookLocked;
    private bool _isCursorLockRequested;

    public bool IsCursorLocked => Cursor.lockState == CursorLockMode.Locked;

    private void Awake()
    {
        if (playerBody == null) playerBody = transform.parent;

        // 카메라가 몸의 자식이 아니면 좌우 회전과 이동이 화면에 반영되지 않는다
        if (playerBody == null || !transform.IsChildOf(playerBody))
            Debug.LogWarning($"[FirstPersonCamera] {name}이(가) Player Body의 자식이 아닙니다. 카메라를 Player 아래로 옮기세요.", this);

        // 씬에 배치된 카메라 각도에서 시작한다. (localEulerAngles는 0~360이므로 -180~180으로 변환)
        _pitch = Mathf.Clamp(Mathf.DeltaAngle(0f, transform.localEulerAngles.x), minPitch, maxPitch);
    }

    private void OnEnable()
    {
        if (lockCursorOnEnable) SetCursorLocked(true);
    }

    private void OnDisable()
    {
        SetCursorLocked(false);
    }

    // 창 포커스를 잃으면 커서 잠금이 풀리므로, 돌아왔을 때 요청 상태를 다시 적용한다
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus) ApplyCursorState(_isCursorLockRequested);
    }

    // ---------- Input (PlayerInput Unity Events) ----------

    public void OnLook(InputAction.CallbackContext context)
    {
        // Value 액션은 첫 입력 때 started와 performed가 같은 값으로 연달아 오므로 performed만 받는다.
        // 한 프레임에 여러 번 올 수 있어 대입하지 않고 누적한다.
        if (!context.performed) return;
        _lookInput += context.ReadValue<Vector2>();
    }

    public void OnEscape(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        SetCursorLocked(false);
    }

    // ---------- Rotation ----------

    private void Update()
    {
        if (_isLookLocked || !IsCursorLocked)
        {
            _lookInput = Vector2.zero;
            return;
        }

        // 마우스 delta는 이미 프레임 이동량이므로 deltaTime을 곱하지 않는다
        Vector2 delta = _lookInput * sensitivity;
        _lookInput = Vector2.zero;

        // 상하 : 카메라
        _pitch = Mathf.Clamp(_pitch - delta.y, minPitch, maxPitch);
        transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);

        // 좌우 : 몸
        if (playerBody != null) playerBody.Rotate(Vector3.up, delta.x);
    }

    // ---------- Public API ----------

    /// <summary>카메라 위치에서 카메라 정면 방향. 화면 중앙 조준선과 같은 선이다.</summary>
    public Ray GetAimRay()
    {
        return new Ray(transform.position, transform.forward);
    }

    public void SetLookLocked(bool locked)
    {
        _isLookLocked = locked;
        if (locked) _lookInput = Vector2.zero;
    }

    public void SetCursorLocked(bool locked)
    {
        _isCursorLockRequested = locked;
        ApplyCursorState(locked);
    }

    private static void ApplyCursorState(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
