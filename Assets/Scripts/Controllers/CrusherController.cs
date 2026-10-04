using UnityEngine;

[RequireComponent(typeof(Outline))]
public class CrusherController : MonoBehaviour, IInteractable
{
    private const float RULE_FAIL_PENALTY = 10f;

    [Header("Day End")]
    [SerializeField]
    private GameObject _text;

    private Outline _outline;

    private void Awake()
    {
        _outline = GetComponent<Outline>();
        _outline.SetOutline(false);
        _outline.enabled = false;
    }

    private void OnEnable()
    {
        Managers.Date.OnDayEnd += HandleDayEnd;
        Managers.Date.OnDayStart += HandleDayStart;
    }

    private void OnDisable()
    {
        Managers.Date.OnDayEnd -= HandleDayEnd;
        Managers.Date.OnDayStart -= HandleDayStart;
    }

    /// <summary>
    /// 들고 있는 명령서를 파쇄하고 진짜 명령서이면 정신력 실패 연출을 실행한다.
    /// 플레이어 카메라 아래의 FaxInstructionController를 확인하고 IsReal이 true일 때 정신력을 감소시킨다.
    /// </summary>
    public void Interact()
    {
        if (!_outline.enabled)
            return;

        FaxInstructionController instruction = Camera.main.GetComponentInChildren<FaxInstructionController>();
        if (instruction == null)
            return;

        bool isReal = instruction.IsReal;
        Managers.Fax.RemoveInstruction(instruction);
        Destroy(instruction.gameObject);

        Managers.Sound.ShredderSound();

        if (isReal)
            Managers.Game.PunchMentality(RULE_FAIL_PENALTY);
    }

    /// <summary>
    /// 하루 종료 시 Crusher 안내 텍스트를 표시한다.
    /// _text를 활성화한다.
    /// </summary>
    private void HandleDayEnd()
    {
        _text.SetActive(true);
        _outline.enabled = true;
    }

    /// <summary>
    /// 다음 날 시작 시 Crusher 안내 텍스트를 숨긴다.
    /// _text를 비활성화한다.
    /// </summary>
    private void HandleDayStart()
    {
        _text.SetActive(false);
        _outline.SetOutline(false);
        _outline.enabled = false;
    }
}
