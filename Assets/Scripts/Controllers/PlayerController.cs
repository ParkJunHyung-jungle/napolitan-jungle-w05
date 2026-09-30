using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;
    public Camera mainCamera;
    public float moveSpeed;
    private Vector2 moveInput;

    private float xMove = 0f;
    private float zMove = 0f;

    private Vector3 rayStartPoint;
    private Vector3 rayDirection;
    public float maxCursorDistance = 3f;
    public LayerMask movingLayerMask;

    public Vector2 mousePosition;
    public LayerMask fixedLayerMask;

    private GameObject cursorItem = null;
    public GameObject selectedItem = null;
    private Renderer targetRenderer;
    private Material originMaterial;
    public Material highlightedMaterial;
    public Material selectedMaterial;

    public event Action<GameObject> playerInteracted;
    public bool canMove = true;

    public void OnEnable()
    {

    }

    public void OnDestroy()
    {

    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        // 하이라이트 된 오브젝트와 상호작용(현재 e키)해서 UI로 진입하면.. canMove가 false가 되면서 Move, ShootCursor를 멈춘다
        if (canMove == true)
        {
            if (cursorItem == null) return;

            if (!CheckIsSelectable()) return;

            if (selectedItem == null)
            {
                selectedItem = cursorItem;
                targetRenderer.material = selectedMaterial;
                Debug.Log($"Selected Item: {selectedItem}");
                canMove = false;

                ReturnCursorItemMaterial();

                // 아이템을 선택했다는 이벤트를 invoke해서 카메라 뷰 이동이나 오브젝트 활성화 같은 데에 쓸 수 있도록 한다
                playerInteracted?.Invoke(selectedItem);
            }
        }

        else
        {
            return;
        }
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (canMove || !context.performed) return;

        Vector2 clickPosition = Mouse.current.position.ReadValue();

        // cursorItem이 Button 레이어일 경우 ButtonManager를 가져온 뒤, 거기 있는 ClickButton을 실행시킨다.
        if (cursorItem.layer == LayerMask.NameToLayer("Button") && cursorItem.TryGetComponent<IInteractable>(out var interactable))
        {
            interactable.Interact();
        }
    }

    public void OnPoint(InputAction.CallbackContext context)
    {
        if (canMove) return;
        mousePosition = context.ReadValue<Vector2>();
    }

    public void OnEscape(InputAction.CallbackContext context)
    {
        // UI에서 벗어나면 (현재 esc키 입력) canMove를 true로 바꾸고, Move와 ShootCursor가 다시 작동한다
        if (canMove == false)
        {
            Debug.Log("Escaped");
            canMove = true;
            // 벗어날 때도 이벤트를 invoke해서 이벤트 구독자들이 상태 변화를 인지하도록 한다
            playerInteracted?.Invoke(selectedItem);

            // 벗어나면 selectedItem을 null로 바꾼다
            selectedItem = null;
        }
    }

    void Update()
    {
        // 별다른 조건 없으면 이동 & 상호작용 오브젝트 선택
        if (canMove)
        {
            Move();
            ShootCursorFromCamera();
        }

        // 뷰 고정 상태일 때는 마우스에서 raycast 시키기
        else
        {
            ShootCursorFromMouse();
        }
    }

    private void Move()
    {
        // 입력값의 y축, x축 반대로 생각
        xMove = moveInput.y * moveSpeed * Time.deltaTime;
        zMove = moveInput.x * moveSpeed * Time.deltaTime;
        Vector3 moveDirection = transform.forward * xMove + transform.right * zMove;

        controller.Move(moveDirection);
    }

    // 이동 가능할 때 카메라 중앙에서 Raycast
    private void ShootCursorFromCamera()
    {
        // 커서 위치 업데이트
        rayStartPoint = transform.position;
        rayDirection = mainCamera.transform.forward;

        RaycastHit hitInfo;


        // raycast로 인식할 layerMask: 앞뒤왼오
        bool rayHit = Physics.Raycast(rayStartPoint, rayDirection, out hitInfo, maxCursorDistance, movingLayerMask, QueryTriggerInteraction.Ignore);

        // raycast에 맞은 오브젝트가 있으면 하이라이트할 오브젝트 저장
        GameObject nextItem = rayHit ? hitInfo.collider.gameObject : null;

        // 이전꺼랑 같으면 넘어가고 (null 포함)
        if (cursorItem == nextItem) return;

        // 아니면 이전꺼 머티리얼 돌려준 뒤에 새 아이템 지정 & 머티리얼 바꿔주기
        ReturnCursorItemMaterial();

        cursorItem = nextItem;
        if (CheckIsSelectable()) ChangeCursorItemMaterial();
    }

    // 오브젝트를 선택해서 뷰가 고정됐을 때 마우스 위치에서 Raycast
    private void ShootCursorFromMouse()
    {
        // 커서 위치 업데이트. 커서 위치에 맞춰서 Ray를 쏴줘야 한다.
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        rayStartPoint = ray.origin;
        rayDirection = ray.direction;

        RaycastHit hitInfo;

        // raycast로 인식할 layerMask: Button, Slider
        bool rayHit = Physics.Raycast(rayStartPoint, rayDirection, out hitInfo, maxCursorDistance * 5, fixedLayerMask, QueryTriggerInteraction.Ignore);

        // raycast에 맞은 오브젝트가 있으면 하이라이트할 오브젝트 저장
        GameObject nextItem = rayHit ? hitInfo.collider.gameObject : null;

        // 이전꺼랑 같으면 넘어가고 (null 포함)
        if (cursorItem == nextItem) return;

        // 아니면 이전꺼 머티리얼 돌려준 뒤에 새 아이템 지정 & 머티리얼 바꿔주기
        // ReturnCursorItemMaterial();

        cursorItem = nextItem;
        // if (CheckIsSelectable()) ChangeCursorItemMaterial();
    }

    private bool CheckIsSelectable()
    {
        if (cursorItem == null) return false;
        // 이동 가능한 상태일 때
        if (canMove)
        {
            if (cursorItem.layer != LayerMask.NameToLayer("Front") && cursorItem.layer != LayerMask.NameToLayer("Back")
                && cursorItem.layer != LayerMask.NameToLayer("Left") && cursorItem.layer != LayerMask.NameToLayer("Right")) return false;
            else return true;
        }

        else
        {
            if (cursorItem.layer != LayerMask.NameToLayer("Button") && cursorItem.layer != LayerMask.NameToLayer("Slider")) return false;
            else return true;
        }

    }

    public void ChangeCursorItemMaterial()
    {
        if (cursorItem == null) return;
        targetRenderer = cursorItem.GetComponentInChildren<Renderer>();

        if (targetRenderer != null && originMaterial == null)
        {
            originMaterial = targetRenderer.material;
            targetRenderer.material = highlightedMaterial;
        }
    }

    public void ReturnCursorItemMaterial()
    {
        if (targetRenderer != null && originMaterial != null) targetRenderer.material = originMaterial;
        cursorItem = null;
        targetRenderer = null;
        originMaterial = null;
    }


    // 화면 중앙 커서에 포커스된 오브젝트를 매니저에 보낸다
    public GameObject SendCursorItem()
    {
        if (cursorItem != null) return cursorItem;
        else return null;
    }

    // 상호작용 키로 선택한 아이템을 게임 매니저에 보낸다
    public GameObject SendSelectedItem()
    {
        if (selectedItem != null) return selectedItem;
        else return null;
    }
}
