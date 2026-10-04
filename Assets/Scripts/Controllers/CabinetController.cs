using UnityEngine;

[RequireComponent(typeof(Outline))]
public class CabinetController : MonoBehaviour, IInteractable
{
    private const float RULE_FAIL_PENALTY = 10f;

    [Header("Day End")]
    [SerializeField]
    private GameObject _text;
    [SerializeField]
    private GameObject _floorSecond;

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
    /// 들고 있는 명령서를 보관하고 가짜 명령서이면 정신력 실패 연출을 실행한다.
    /// 플레이어 카메라 아래의 FaxInstructionController를 확인하고 IsReal이 false일 때 정신력을 감소시킨다.
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

        if (!isReal)
            Managers.Game.PunchMentality(RULE_FAIL_PENALTY);
    }

    /// <summary>
    /// 하루 종료 시 안내 텍스트를 표시하고 FloorSecond를 이동한다.
    /// _text를 활성화하고 _floorSecond의 로컬 x 좌표를 0.8로 변경한다.
    /// </summary>
    private void HandleDayEnd()
    {
        _text.SetActive(true);
        _outline.enabled = true;
        Vector3 localPosition = _floorSecond.transform.localPosition;
        localPosition.x = 0.8f;
        _floorSecond.transform.localPosition = localPosition;
    }

    /// <summary>
    /// 다음 날 시작 시 안내 텍스트와 FloorSecond 위치를 초기화한다.
    /// _text를 비활성화하고 _floorSecond의 로컬 x 좌표를 0으로 변경한다.
    /// </summary>
    private void HandleDayStart()
    {
        _text.SetActive(false);
        _outline.SetOutline(false);
        _outline.enabled = false;

        Vector3 localPosition = _floorSecond.transform.localPosition;
        localPosition.x = 0f;
        _floorSecond.transform.localPosition = localPosition;
    }
}
