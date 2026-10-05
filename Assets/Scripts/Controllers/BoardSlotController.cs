using UnityEngine;

public class BoardSlotController : MonoBehaviour, IInteractable
{
    [SerializeField] 
    private int _slotIndex;
    
    [SerializeField]
    private FaxInstructionController _pastMessage;

    private BoardController _boardController;

    private void Awake()
    {
        _boardController = GetComponentInParent<BoardController>();
    }

    private void Start()
    {
        if (_slotIndex != 5)
            return;

        _pastMessage.transform.SetPositionAndRotation(
            transform.position,
            transform.rotation * Quaternion.Euler(0f, 180f, 0f));

        _pastMessage.enabled = false;

        Rigidbody rigidbody = _pastMessage.GetComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;
        rigidbody.detectCollisions = false;

        Outline outline = _pastMessage.GetComponent<Outline>();
        outline.SetOutline(false);
        outline.enabled = false;

        _boardController.InitializeSlot(_slotIndex, _pastMessage);
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
