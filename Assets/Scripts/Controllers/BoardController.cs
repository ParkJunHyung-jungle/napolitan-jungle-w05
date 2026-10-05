using UnityEngine;

public class BoardController : MonoBehaviour
{
    [Header("Instruction Slots")]
    [SerializeField] private Transform _slot1;
    [SerializeField] private Transform _slot2;
    [SerializeField] private Transform _slot3;
    [SerializeField] private Transform _slot4;
    [SerializeField] private Transform _slot5;
    [SerializeField] private Transform _slot6;

    private FaxInstructionController[] _instructions = new FaxInstructionController[6];

    /// <summary>
    /// 지정된 슬롯에 들고 있는 명령서를 보관하거나 이미 보관된 명령서를 다시 든다.
    /// slotIndex로 슬롯과 보관된 명령서를 선택하고 명령서의 물리, 상호작용, 아웃라인 상태를 변경한다.
    /// </summary>
    public void InteractSlot(int slotIndex)
    {
        if (_instructions[slotIndex] != null)
        {
            FaxInstructionController instruction = _instructions[slotIndex];
            FaxInstructionController swapInstruction = Camera.main.GetComponentInChildren<FaxInstructionController>();

            if (swapInstruction != null)
            {
                Transform swapSlot = GetSlot(slotIndex);
                swapInstruction.transform.SetParent(null, true);
                swapInstruction.transform.SetPositionAndRotation(swapSlot.position, swapSlot.rotation);
                Rigidbody swapRigidbody = swapInstruction.GetComponent<Rigidbody>();
                swapRigidbody.isKinematic = true;
                swapRigidbody.useGravity = false;
                swapRigidbody.detectCollisions = false;
                swapInstruction.enabled = false;
                Outline swapOutline = swapInstruction.GetComponent<Outline>();
                swapOutline.SetOutline(false);
                swapOutline.enabled = false;
                _instructions[slotIndex] = swapInstruction;

                instruction.enabled = true;
                Outline instructionOutline = instruction.GetComponent<Outline>();
                instructionOutline.enabled = true;
                instruction.PickUp();
                return;
            }

            Rigidbody rigidbody = instruction.GetComponent<Rigidbody>();
            Outline outline = instruction.GetComponent<Outline>();

            rigidbody.detectCollisions = true;
            instruction.enabled = true;
            outline.enabled = true;
            instruction.PickUp();
            _instructions[slotIndex] = null;
            return;
        }

        FaxInstructionController heldInstruction = Camera.main.GetComponentInChildren<FaxInstructionController>();
        if (heldInstruction == null)
            return;

        Transform slot = GetSlot(slotIndex);
        heldInstruction.transform.SetParent(null, true);
        heldInstruction.transform.SetPositionAndRotation(slot.position, slot.rotation);
        Rigidbody heldRigidbody = heldInstruction.GetComponent<Rigidbody>();
        heldRigidbody.isKinematic = true;
        heldRigidbody.useGravity = false;
        heldRigidbody.detectCollisions = false;
        heldInstruction.enabled = false;
        Outline heldOutline = heldInstruction.GetComponent<Outline>();
        heldOutline.SetOutline(false);
        heldOutline.enabled = false;
        _instructions[slotIndex] = heldInstruction;

        Managers.Sound.PaperSound();
    }

    /// <summary>
    /// 슬롯 번호에 해당하는 Inspector 연결 Transform을 반환한다.
    /// slotIndex를 사용해 여섯 슬롯 중 하나를 선택한다.
    /// </summary>
    private Transform GetSlot(int slotIndex)
    {
        switch (slotIndex)
        {
            case 0: return _slot1;
            case 1: return _slot2;
            case 2: return _slot3;
            case 3: return _slot4;
            case 4: return _slot5;
            default: return _slot6;
        }
    }
}
