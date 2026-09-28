using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;
    public Camera camera;
    public float moveSpeed;
    private Vector2 moveInput;

    private float xMove = 0f;
    private float zMove = 0f;

    private Vector3 rayStartPoint;
    private Vector3 rayDirection;
    public float maxCursorDistance = 3f;
    public LayerMask layerMask;

    private GameObject cursorItem = null;
    private GameObject selectedItem = null;
    private Renderer targetRenderer;
    private Material originMaterial;
    public Material highlightedMaterial;
    public Material selectedMaterial;

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (cursorItem != null) selectedItem = cursorItem;
        targetRenderer.material = selectedMaterial;
        Debug.Log($"Selected Item: {selectedItem}");
        // gameManager.SetSelectedItem(selectedItem);
    }

    void Update()
    {
        // 별다른 조건 없으면 이동 가능
        Move();
        ShootCursor();
    }

    private void Move()
    {
        // 입력값의 y축, x축 반대로 생각
        xMove = moveInput.y * moveSpeed * Time.deltaTime;
        zMove = moveInput.x * moveSpeed * Time.deltaTime;
        Vector3 moveDirection = transform.forward * xMove + transform.right * zMove;

        controller.Move(moveDirection);
    }

    private void ShootCursor()
    {
        // 커서 위치 업데이트
        rayStartPoint = transform.position;
        rayDirection = camera.transform.forward;

        RaycastHit hitInfo;

        bool rayHit = Physics.Raycast(rayStartPoint, rayDirection, out hitInfo, maxCursorDistance, layerMask, QueryTriggerInteraction.Ignore);

        if (rayHit)
        {
            if (cursorItem != null && cursorItem.name != hitInfo.collider.name) ReturnCursorItemMaterial();
            cursorItem = hitInfo.collider.gameObject;
            ChangeCursorItemMaterial();
        }

        else
        {
            cursorItem = null;
            ReturnCursorItemMaterial();
        }
    }

    public void ChangeCursorItemMaterial()
    {
        if (cursorItem == null) return;
        targetRenderer = cursorItem.GetComponent<Renderer>();

        if (targetRenderer != null && originMaterial == null)
        {
            originMaterial = targetRenderer.material;
            targetRenderer.material = highlightedMaterial;
        }
    }

    public void ReturnCursorItemMaterial()
    {
        Debug.Log($"target: {targetRenderer}, origin: {originMaterial}");
        if (targetRenderer == null || originMaterial == null) return;
        targetRenderer.material = originMaterial;
        targetRenderer = null;
        originMaterial = null;
    }


    // 화면 중앙 커서에 포커스된 오브젝트를 매니저에 보낸다
    public GameObject SendCursorItem()
    {
        if (cursorItem != null) return cursorItem;
        else return null;
    }

    // 상호작용 키로 선택한 아이템을 매니저에 보낸다
    public GameObject SendSelectedItem()
    {
        if (selectedItem != null) return selectedItem;
        else return null;
    }
}
