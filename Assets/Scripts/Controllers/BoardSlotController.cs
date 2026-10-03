using UnityEngine;

public class BoardSlotController : MonoBehaviour, IInteractable
{
    [SerializeField] private int _slotIndex;

    private BoardController _boardController;

    private void Awake()
    {
        _boardController = GetComponentInParent<BoardController>();
    }

    /// <summary>
    /// 연결된 게시판에 슬롯 상호작용을 전달한다.
    /// _slotIndex로 보관하거나 다시 들 명령서의 슬롯을 선택한다.
    /// </summary>
    public void Interact()
    {
        _boardController.InteractSlot(_slotIndex);
    }
}
