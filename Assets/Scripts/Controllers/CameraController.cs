using UnityEngine;

public class CameraController : MonoBehaviour
{
    // 임시로 직접 연결하기 위한 용도. Manager 넣으면 삭제하자
    public PlayerController playerController;

    public float cameraSpeed = 0.4f;
    private Vector2 mouseDelta;

    public Transform playerBody;

    private float xRotation = 0f;
    private float yRotation = 0f;

    private bool isFixed = false;
    private GameObject selectedItem;
    public float viewDistance = 4f;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Quaternion startCameraRotation;
    private float movingTime = 0.6f;
    private float currentMovingTime = 0f;

    // 첫 값이 튀는 것을 무시한다
    private bool ignoreFirstLook = true;

    void Awake()
    {
        // 임시로 playercontroller랑 직접 연결해서 이벤트 구독. 나중에 매니저를 통해서 연결하자
        playerController.playerInteracted += ToggleView;
    }

    void OnDestroy()
    {
        playerController.playerInteracted -= ToggleView;
    }

    // New Input system의 Look을 가져온다
    void Update()
    {
        ProcessLookInput();

        // 플레이어가 물체와 상호작용한 상태인지 playerInteracted 이벤트 구독해서 확인, bool 값 토글해서 카메라 및 플레이어 위치 고정/해제
        if (isFixed) FixedItem();
        else MouseRotate();
    }

    /// <summary>
    /// InputManager에서 시점 입력을 읽어 카메라 회전량을 갱신한다.
    /// 최초 입력은 무시하고 고정 시점이 아닐 때 mouseDelta를 설정한다.
    /// </summary>
    private void ProcessLookInput()
    {
        Vector2 lookInput = Managers.Input.LookInput;
        if (lookInput == Vector2.zero) return;

        if (ignoreFirstLook)
        {
            ignoreFirstLook = false;
            mouseDelta = Vector2.zero;
            return;
        }

        if (!isFixed) mouseDelta = lookInput * cameraSpeed;
    }

    // 상호작용하는 물체가 없을 때 카메라를 회전시키는 함수
    private void MouseRotate()
    {
        // Vector2 입력값 x, y랑 실제 움직임은 반대로 묶어야 한다
        xRotation -= mouseDelta.y;

        // Clamp를 넣어야 카메라 각도를 제한할 수 있다
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // 상하 회전
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 좌우 회전
        yRotation = mouseDelta.x;
        if (playerBody != null) playerBody.Rotate(Vector3.up * yRotation);

        // 마우스가 멈춰있을 때 회전하지 않도록 값 초기화
        mouseDelta = Vector2.zero;
    }

    private void FixedItem()
    {
        Vector3 inwardNormal;

        // 각 벽의 레이어에 소속된 아이템인가? 체크하고 그에 맞춰 플레이어가 바라볼 방향 맞추기 (카메라가 플레이어의 child이므로)
        if (selectedItem.layer == LayerMask.NameToLayer("Front")) inwardNormal = Vector3.left;
        else if (selectedItem.layer == LayerMask.NameToLayer("Back")) inwardNormal = Vector3.right;
        else if (selectedItem.layer == LayerMask.NameToLayer("Left")) inwardNormal = Vector3.back;
        else if (selectedItem.layer == LayerMask.NameToLayer("Right")) inwardNormal = Vector3.forward;
        else return;

        // 방향, 위치 설정
        Vector3 targetPosition = selectedItem.transform.position - inwardNormal * viewDistance;
        targetPosition.y = 2f;
        Quaternion targetRotation = Quaternion.LookRotation(inwardNormal, Vector3.up);
        mouseDelta = Vector2.zero;

        currentMovingTime += Time.deltaTime;

        // 0~1까지 진행도를 계산하는 progress.
        float progress = Mathf.Clamp01(currentMovingTime / movingTime);

        // 부드러운 움직임을 만드는 데 많이 사용하는 식: SmoothStep -> 3t^2 - 2t^3 (t는 진행도)
        progress = 3f * Mathf.Pow(progress, 2) - 2f * Mathf.Pow(progress, 3);

        // 플레이어 위치 설정
        playerBody.localPosition = Vector3.Lerp(startPosition, targetPosition, progress);
        playerBody.localRotation = Quaternion.Slerp(startRotation, targetRotation, progress);

        // 카메라 y값 설정. 없으면 카메라 y값이 초기화되지 않아서 바라본 방향으로 간다
        transform.localRotation = Quaternion.Slerp(startCameraRotation, Quaternion.identity, progress);
    }

    private void ToggleView(GameObject item)
    {
        if (isFixed == false)
        {
            selectedItem = item;
            isFixed = true;

            // FixedItem 뷰로 부드럽게 이동시키기 위해 위치, 방향 값들을 기억하고, 애니메이션 진행 시간을 초기화한다
            startPosition = playerBody.localPosition;
            startRotation = playerBody.localRotation;
            startCameraRotation = transform.localRotation;
            xRotation = 0f;
            currentMovingTime = 0f;
        }

        else
        {
            selectedItem = null;
            isFixed = false;
            // transform.localPosition = Vector3.zero;
        }
    }
}
