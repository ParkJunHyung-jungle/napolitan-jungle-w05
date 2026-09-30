using UnityEngine;

public class SelectColliderController : MonoBehaviour
{
    public PlayerController playerController;
    public Collider selectCollider;

    private bool isActive = true;

    void OnEnable()
    {
        playerController.playerInteracted += ToggleCollider;
    }

    void OnDisable()
    {
        playerController.playerInteracted -= ToggleCollider;
    }

    private void ToggleCollider(GameObject selectedItem)
    {
        // 이 오브젝트를 선택했을 때
        if (selectedItem.name == gameObject.name)
        {
            // 활성화 된 상태였다면 collider를 끈다
            if (isActive)
            {
                selectCollider.enabled = false;
                isActive = false;
            }

            // 비활성화된 상태였으면 다시 collider를 킨다
            else
            {
                selectCollider.enabled = true;
                isActive = true;
            }
        }

        // 선택된 오브젝트가 다른거면 그냥 활성화
        else selectCollider.enabled = true;
    }
}
