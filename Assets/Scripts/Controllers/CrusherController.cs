using UnityEngine;

public class CrusherController : MonoBehaviour, IInteractable
{
    private const float RULE_FAIL_PENALTY = 10f;

    /// <summary>
    /// 들고 있는 명령서를 파쇄하고 진짜 명령서이면 정신력 실패 연출을 실행한다.
    /// 플레이어 카메라 아래의 FaxInstructionController를 확인하고 IsReal이 true일 때 정신력을 감소시킨다.
    /// </summary>
    public void Interact()
    {
        FaxInstructionController instruction = Camera.main.GetComponentInChildren<FaxInstructionController>();
        if (instruction == null)
            return;

        bool isReal = instruction.IsReal;
        Destroy(instruction.gameObject);

        if (isReal)
            Managers.Game.PunchMentality(RULE_FAIL_PENALTY);
    }
}
