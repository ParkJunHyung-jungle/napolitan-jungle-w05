using UnityEngine;
/// <summary>
/// 1인칭 플레이어. Player 루트에 붙인다.
/// WASD 이동(중력 포함, 점프 없음)과 좌클릭 상호작용을 담당한다.
/// 좌클릭하면 조준선(카메라 정면)으로 Raycast해서 맞은 IInteractable을 바로 조작한다.
/// 맞은 대상이 IDraggable이면 누르고 있는 동안 잡아서 마우스 이동량을 넘기고, 그동안 이동을 멈추고 시점 감도를 낮춘다.
/// 시점 회전은 FirstPersonCamera가 한다.
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
    [Tooltip("상호작용 Raycast 대상 레이어 (Button, Slider)")]
    [SerializeField] private LayerMask interactLayerMask;

    [Header("Drag")]
    [Tooltip("드래그 중 시점 감도 배율. 0이면 시점이 완전히 멈춘다. 가장 무거운 슬라이더(weight 0.3)를 끌 때 손잡이가 멈춰 보이지 않도록 낮게 둔다")]
    [SerializeField, Range(0f, 1f)] private float dragLookScale = 0.1f;

    private float _verticalVelocity;
    public FirstPersonCamera FirstPersonCamera => firstPersonCamera;

    // 컴포넌트를 처음 붙일 때 기본 레이어 마스크를 Button, Slider로 채운다
    private void Reset()
    {
        interactLayerMask = LayerMask.GetMask("Button", "Slider");
    }

    private void Awake()
    {
        if (characterController == null) characterController = GetComponent<CharacterController>();
        if (firstPersonCamera == null) firstPersonCamera = GetComponentInChildren<FirstPersonCamera>();
        Managers.Game.Player = this;
    }

    // ---------- Movement ----------

    private void Update()
    {
        UpdateInput();
        Move();
    }

    private void Move()
    {
        // WASD 컴포지트는 이미 정규화되어 있다. 게임패드 스틱 대비로 길이만 1로 제한한다
        // 드래그 중에는 손잡이와의 거리·각도가 바뀌지 않도록 걷지 않는다 (시점은 낮은 감도로 돈다)
        Vector2 input = Vector2.ClampMagnitude(Managers.Input.MoveInput, 1f);
        Vector3 direction = transform.forward * input.y + transform.right * input.x;

        if (characterController.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = GroundedVerticalVelocity;
        else
            _verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = direction * moveSpeed + Vector3.up * _verticalVelocity;
        characterController.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// 중앙 입력 매니저에서 이동, 시점, 상호작용 상태를 읽는다.
    /// 현재 프레임 입력을 사용해 드래그 이동량을 누적하고 상호작용 시작·해제를 처리한다.
    /// </summary>
    private void UpdateInput()
    {
        if (!Managers.Input.InteractPressed)
            return;
        TryInteract();
    }

    // ---------- Interaction ----------
    // 아웃라인과 클릭이 함께 사용하는 거리 검사
    /// <summary>
    /// 카메라 중앙 조준 Ray로 3m 이내 상호작용 대상을 찾는다.
    /// 입력 잠금과 PlayerMap 상태를 확인하고 적중 정보를 hit에 반환한다.
    /// </summary>
    private bool TryGetTarget(out RaycastHit hit)
    {
        hit = default;

        if (!firstPersonCamera.TryGetAimHit(out hit))
            return false;

        int hitLayerMask = 1 << hit.collider.gameObject.layer;
        return (interactLayerMask.value & hitLayerMask) != 0;
    }

    /// <summary>
    /// 조준 Ray에 맞은 대상의 드래그 또는 상호작용을 시작한다.
    /// 적중 Collider의 부모에서 IDraggable 또는 IInteractable을 찾아 상태를 변경한다.
    /// </summary>
    private void TryInteract()
    {
        FaxInstructionController heldFax = firstPersonCamera.gameObject.GetComponentInChildren<FaxInstructionController>();
        bool hasTarget = TryGetTarget(out RaycastHit hit);
        IInteractable interactable = hasTarget ? hit.collider.GetComponentInParent<IInteractable>() : null;

        if (heldFax != null && !(interactable is BoardSlotController))
        {
            heldFax.Drop();
            return;
        }

        if (interactable != null) interactable.Interact();
    }
}
