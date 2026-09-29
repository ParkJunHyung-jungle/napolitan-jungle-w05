using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 1인칭 플레이어. Player 루트에 붙인다.
/// WASD 이동(중력 포함, 점프 없음)과 좌클릭 상호작용을 담당한다.
/// 좌클릭하면 조준선(카메라 정면)으로 Raycast해서 맞은 IInteractable을 바로 조작한다.
/// 회전과 커서는 FirstPersonCamera가 한다.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    // 접지 중에도 살짝 아래로 눌러야 CharacterController.isGrounded가 안정적으로 유지된다
    private const float GroundedVerticalVelocity = -2f;

    [Header("References")]
    [Tooltip("비어 있으면 같은 오브젝트에서 찾는다")]
    [SerializeField] private CharacterController characterController;

    [Tooltip("비어 있으면 자식에서 찾는다")]
    [SerializeField] private FirstPersonCamera firstPersonCamera;

    [Header("Movement")]
    [Tooltip("m/초")]
    [SerializeField] private float moveSpeed = 3f;

    [SerializeField] private float gravity = -9.81f;

    [Header("Interaction")]
    [Tooltip("조작 가능 거리 (m)")]
    [SerializeField] private float interactDistance = 2f;

    [Tooltip("상호작용 Raycast 대상 레이어 (Button, 추후 Slider)")]
    [SerializeField] private LayerMask interactLayerMask;

    private Vector2 _moveInput;
    private float _verticalVelocity;
    private int _lockCount;

    public bool IsInputLocked => _lockCount > 0;

    // 컴포넌트를 처음 붙일 때 기본 레이어 마스크를 Button으로 채운다
    private void Reset()
    {
        interactLayerMask = LayerMask.GetMask("Button");
    }

    private void Awake()
    {
        if (characterController == null) characterController = GetComponent<CharacterController>();
        if (firstPersonCamera == null) firstPersonCamera = GetComponentInChildren<FirstPersonCamera>();
    }

    // ---------- Input (PlayerInput Unity Events) ----------

    public void OnMove(InputAction.CallbackContext context)
    {
        // canceled 때는 zero가 들어와 멈춘다
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        // 커서가 풀린 상태의 클릭은 커서 재잠금만 하고 상호작용하지 않는다
        if (!firstPersonCamera.IsCursorLocked)
        {
            firstPersonCamera.SetCursorLocked(true);
            return;
        }

        if (IsInputLocked) return;

        TryInteract();
    }

    // ---------- Movement ----------

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        // WASD 컴포지트는 이미 정규화되어 있다. 게임패드 스틱 대비로 길이만 1로 제한한다
        Vector2 input = IsInputLocked ? Vector2.zero : Vector2.ClampMagnitude(_moveInput, 1f);
        Vector3 direction = transform.forward * input.y + transform.right * input.x;

        if (characterController.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = GroundedVerticalVelocity;
        else
            _verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = direction * moveSpeed + Vector3.up * _verticalVelocity;
        characterController.Move(velocity * Time.deltaTime);
    }

    // ---------- Interaction ----------

    private void TryInteract()
    {
        Ray ray = firstPersonCamera.GetAimRay();

        if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayerMask, QueryTriggerInteraction.Ignore))
            return;

        // 버튼은 루트와 자식(Clickable, FoucsIndicator) 모두 콜라이더가 있으므로 부모 방향으로 찾는다
        IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
        if (interactable != null) interactable.Interact();
    }

    // ---------- Lock ----------

    /// <summary>
    /// 이동·시점·상호작용 잠금. 요청 수를 세므로 잠근 쪽이 각자 한 번씩 풀어야 한다.
    /// (슬라이더 드래그, 매뉴얼 확대 보기용)
    /// </summary>
    public void SetInputLocked(bool locked)
    {
        if (locked) _lockCount++;
        else if (_lockCount > 0) _lockCount--;

        firstPersonCamera.SetLookLocked(IsInputLocked);
    }
}
