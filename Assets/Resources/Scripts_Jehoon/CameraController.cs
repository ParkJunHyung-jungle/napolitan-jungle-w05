using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float cameraSpeed = 0.4f;
    private Vector2 mouseDelta;

    public Transform playerBody;

    private float xRotation = 0f;
    private float yRotation = 0f;


    // New Input system의 Look을 가져온다
    public void OnLook(InputAction.CallbackContext context)
    {
        mouseDelta = context.ReadValue<Vector2>() * cameraSpeed;
    }

    void Update()
    {
        // 별다른 조건 없으면 마우스에 맞춰서 카메라 회전
        mouseRotate();

        // 특정 조건을 이벤트 구독해서 확인, 그 때는 카메라 및 플레이어 위치 고정시키기
    }

    private void mouseRotate()
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
}
