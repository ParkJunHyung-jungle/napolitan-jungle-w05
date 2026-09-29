using UnityEngine;

/// <summary>
/// 누른 채 드래그해서 조작하는 오브젝트 (슬라이더 등).
/// FirstPersonController가 잡는 순간 BeginDrag, 잡고 있는 동안 매 프레임 Drag, 놓는 순간 EndDrag를 부른다.
/// </summary>
public interface IDraggable
{
    /// <summary>잡는 순간 한 번. cam은 화면 좌표 계산용 플레이어 카메라.</summary>
    void BeginDrag(Camera cam);

    /// <summary>잡고 있는 동안 매 프레임. 픽셀 단위 마우스 이동량.</summary>
    void Drag(Vector2 mouseDelta);

    /// <summary>놓는 순간 한 번.</summary>
    void EndDrag();
}
