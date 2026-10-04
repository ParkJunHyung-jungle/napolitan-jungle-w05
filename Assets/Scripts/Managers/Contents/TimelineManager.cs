using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using UnityEngine;

using Newtonsoft.Json;

public class TimelineManager
{
    private const string TEXTS_PATH = "Datas/Texts";
    private const string TIMELINE_PATH = "Datas/Timeline";
    private const string PREFAB_FOLDER = "Prefabs";
    private const string FAX_HEADER_ID = "FAX_HEADER";
    private const string PHONE_RING_CHECK_ID = "PHONE_RING";
    private const string TIME_FORMAT = @"hh\:mm";
    private const string BROKE_FLAG_FORMAT = "BROKE_{0}";
    private const float RULE_FAIL_PENALTY = 10f;

    [Header("Data")]
    private Dictionary<string, string> _texts;
    private Dictionary<string, InstructionData> _instructions;
    private ILookup<int, TimelineEvent> _events;

    public event Action OnPhoneRing;
    public event Action OnPhoneRingStop;
    public event Action OnPhoneCryingStop;
    public event Action OnDoorKnock;

    [Header("Device State")]
    private bool _isLightOn = true;
    private bool _isLightBlinking;
    private bool _isDoorOpen;
    private bool _isKnocking;
    private PhoneState _phoneState;
    private bool _isCrying;

    [Header("Check")]
    private readonly List<TimelineCheck> _checks = new();
    private readonly List<Coroutine> _delayedErrorFaxCoroutines = new();

    [Header("Record")]
    private readonly HashSet<string> _flags = new();
    private readonly List<CheckRecord> _checkRecords = new();

    public IReadOnlyList<CheckRecord> CheckRecords => _checkRecords;
    public bool HasActiveAnomalies => _checks.Count > 0;
    // 기한을 넘긴 구간과 지금 Keep이 깨진 반복 구간을 정신력 지속 감소 대상으로 센다.
    public int OverdueAnomalyCount => _checks.Count(check => check.IsOverdue || (check.IsRepeating && check.IsKeepBroken(IsStateHeld)));

    /// <summary>
    /// Resources의 Texts.json과 Timeline.json을 읽어 문구, 명령서 구성, 시각별 이벤트를 준비한다.
    /// 이벤트를 분 단위로 묶어 _events에 저장하고, 분이 바뀔 때와 하루가 끝날 때 실행되도록 DateManager에 등록한다.
    /// </summary>
    public void Init()
    {
        _texts = JsonConvert.DeserializeObject<Dictionary<string, string>>(Resources.Load<TextAsset>(TEXTS_PATH).text);
        TimelineData timeline = JsonConvert.DeserializeObject<TimelineData>(Resources.Load<TextAsset>(TIMELINE_PATH).text);

        _instructions = timeline.Instructions.ToDictionary(instruction => instruction.Id);
        _events = timeline.Events.ToLookup(timelineEvent => ToMinute(timelineEvent.Time));

        Managers.Date.OnMinuteChange += TriggerEvents;
        Managers.Date.OnDayEnd += ExpireAllChecks;
    }

    /// <summary>
    /// 씬 오브젝트가 구독한 이벤트를 모두 해제하고 예약된 에러 팩스 출력을 멈춘다.
    /// 판정 구간, 플래그, 기록, 장치 상태를 처음 상태로 되돌려 다음 판에 이전 판의 결과가 남지 않게 한다.
    /// </summary>
    public void Clear()
    {
        OnPhoneRing = null;
        OnPhoneRingStop = null;
        OnPhoneCryingStop = null;
        OnDoorKnock = null;

        foreach (Coroutine coroutine in _delayedErrorFaxCoroutines)
            Managers.Instance.StopCoroutine(coroutine);
        _delayedErrorFaxCoroutines.Clear();

        _checks.Clear();
        _flags.Clear();
        _checkRecords.Clear();
        _isLightOn = true;
        _isLightBlinking = false;
        _isDoorOpen = false;
        _isKnocking = false;
        _phoneState = PhoneState.Idle;
        _isCrying = false;
    }

    /// <summary>
    /// minute에 끝나는 판정 구간을 마무리하고 반복 구간을 확인한 뒤, minute에 등록된 이벤트를 Timeline.json에 적힌 순서대로 실행한다.
    /// If, IfNot 조건을 만족하지 않는 이벤트는 건너뛰고, Check가 있는 이벤트는 판정 구간을 시작한다.
    /// </summary>
    public void TriggerEvents(int minute)
    {
        ExpireChecks(minute);
        RepeatChecks(minute);

        foreach (TimelineEvent timelineEvent in _events[minute])
        {
            if (!IsConditionMet(timelineEvent))
                continue;

            Execute(timelineEvent);
            if (timelineEvent.Check != null)
                StartCheck(timelineEvent, minute);
        }
    }

    /// <summary>
    /// 컨트롤러가 알린 장치 동작 action으로 장치 상태를 갱신하고 진행 중인 판정 구간에 전달한다.
    /// 해결된 구간은 결과를 기록하고 제거하며, 기한을 넘긴 실패 구간은 늦게 해결될 때까지 유지한다.
    /// </summary>
    public void Report(DeviceAction action)
    {
        UpdateDeviceState(action);
        int minute = Managers.Date.CurrentMinute;

        // 판정이 끝난 구간을 바로 제거하기 위해 뒤에서부터 순회한다.
        for (int i = _checks.Count - 1; i >= 0; i--)
        {
            TimelineCheck check = _checks[i];
            if (check.IsOverdue && check.IsResolved(IsStateHeld))
            {
                _checks.RemoveAt(i);
                continue;
            }

            CheckState state = check.Apply(action, IsStateHeld);
            if (check.IsOverdue)
            {
                if (state == CheckState.Succeeded)
                    _checks.RemoveAt(i);
                continue;
            }

            if (state == CheckState.Running)
            {
                if (check.IsRepeating)
                    RunRepeat(check, minute);
                continue;
            }

            _checks.RemoveAt(i);
            Resolve(check, state, minute);
        }
    }

    /// <summary>
    /// flag가 판정 결과나 에러 팩스 처리로 기록되어 있는지 확인해 반환한다.
    /// </summary>
    public bool HasFlag(string flag)
    {
        return _flags.Contains(flag);
    }

    /// <summary>
    /// id 판정 구간 중 결과가 isSuccess인 기록의 수를 반환한다.
    /// PHONE_RING의 성공은 받은 전화, 실패는 놓친 전화 수이다.
    /// </summary>
    public int CountCheckRecords(string id, bool isSuccess)
    {
        return _checkRecords.Count(record => record.Id == id && record.IsSuccess == isSuccess);
    }

    /// <summary>
    /// timelineEvent의 Type에 맞는 기존 기능을 호출한다.
    /// 문과 전화기처럼 씬 오브젝트가 처리하는 이벤트는 구독자에게 알리고, 깜빡임, 노크, 울음 상태를 함께 갱신한다.
    /// </summary>
    private void Execute(TimelineEvent timelineEvent)
    {
        switch (timelineEvent.Type)
        {
            case TimelineEventType.Instruction:
                PrintInstruction(timelineEvent.Arg, timelineEvent.Time);
                break;
            case TimelineEventType.ErrorFax:
                Managers.Fax.PrintErrorFax(_texts[timelineEvent.Arg]);
                break;
            case TimelineEventType.LightBlink:
                Managers.Light.RoomLightBlink();
                _isLightBlinking = true;
                break;
            case TimelineEventType.PhoneRing:
                OnPhoneRing?.Invoke();
                break;
            case TimelineEventType.PhoneRingStop:
                HandlePhoneRingStop();
                OnPhoneRingStop?.Invoke();
                break;
            case TimelineEventType.PhoneCrying:
                Managers.Sound.CryingSound();
                _isCrying = true;
                break;
            case TimelineEventType.PhoneCryingStop:
                OnPhoneCryingStop?.Invoke();
                _isCrying = false;
                break;
            case TimelineEventType.DoorKnock:
                OnDoorKnock?.Invoke();
                _isKnocking = true;
                break;
            case TimelineEventType.CompositeCheck:
                // 여러 장치에 걸친 판정 구간만 시작하는 이벤트라 실행할 동작이 없다.
                break;
            case TimelineEventType.Spawn:
                SpawnPrefab(timelineEvent.Arg);
                break;
        }
    }

    /// <summary>
    /// Resources/Prefabs의 prefabName 프리팹을 프리팹에 저장된 위치와 회전 그대로 씬에 생성한다.
    /// 경비원 편지처럼 팩스가 아닌 방식으로 나타나는 오브젝트에 사용한다.
    /// </summary>
    private void SpawnPrefab(string prefabName)
    {
        UnityEngine.Object.Instantiate(Resources.Load<GameObject>($"{PREFAB_FOLDER}/{prefabName}"));
    }

    /// <summary>
    /// timelineEvent의 Check로 판정 구간을 만들어 minute부터 시작한다.
    /// 시작 시점에 실패하면 바로 기록하고, 아니면 _checks에 추가한 뒤 반복 구간은 상태가 이미 깨졌는지 확인한다.
    /// </summary>
    private void StartCheck(TimelineEvent timelineEvent, int minute)
    {
        TimelineCheck check = new TimelineCheck(timelineEvent.Check, timelineEvent.Time, minute);
        CheckState state = check.Begin(IsStateHeld);
        if (state != CheckState.Running)
        {
            Resolve(check, state, minute);
            return;
        }

        _checks.Add(check);
        if (check.IsRepeating)
            RunRepeat(check, minute);
    }

    /// <summary>
    /// 종료 시각이 minute 이하인 판정 구간을 마무리한다.
    /// 성공 구간과 반복 구간은 제거하고, 그 외 실패 구간은 기록한 뒤 늦은 해결까지 _checks에 유지한다.
    /// </summary>
    private void ExpireChecks(int minute)
    {
        for (int i = _checks.Count - 1; i >= 0; i--)
        {
            TimelineCheck check = _checks[i];
            if (check.IsOverdue || check.EndMinute > minute)
                continue;

            CheckState state = check.Expire();
            // 반복 구간은 깨져 있는 동안 이미 정신력이 감소했으므로 실패해도 Overdue로 남기지 않는다.
            if (state == CheckState.Failed && !check.IsRepeating)
                check.MarkOverdue();
            else
                _checks.RemoveAt(i);

            Resolve(check, state, check.EndMinute, state != CheckState.Failed);
        }
    }

    /// <summary>
    /// 하루가 끝났을 때 남은 판정 구간을 모두 마무리한다.
    /// DateManager는 하루가 끝나면 분을 0으로 되돌리므로 05:00에 끝나는 구간은 여기서 처리된다.
    /// </summary>
    private void ExpireAllChecks()
    {
        ExpireChecks(int.MaxValue);
    }

    /// <summary>
    /// 진행 중인 반복 구간을 minute 시각 기준으로 확인한다.
    /// 깨진 상태가 이어지는 구간은 WhileBroken.Every분마다 반복 결과를 실행한다.
    /// </summary>
    private void RepeatChecks(int minute)
    {
        foreach (TimelineCheck check in _checks)
        {
            if (check.IsRepeating)
                RunRepeat(check, minute);
        }
    }

    /// <summary>
    /// 반복 구간 check의 Keep 상태를 minute 시각으로 확인해 차례가 되면 WhileBroken 결과를 실행한다.
    /// 처음 깨졌을 때 BROKE 플래그를 기록하고, 차례마다 에러 팩스를 출력한다
    /// </summary>
    private void RunRepeat(TimelineCheck check, int minute)
    {
        if (!check.CheckRepeat(minute, IsStateHeld))
            return;

        CheckRepeat repeat = check.Data.WhileBroken;
        if (check.RepeatCount == 1)
            _flags.Add(string.Format(BROKE_FLAG_FORMAT, check.Data.Id));

        Managers.Fax.PrintErrorFax(_texts[repeat.ErrorFax]);
    }

    /// <summary>
    /// 응답 없이 벨이 멈춘 PHONE_RING 체크를 실패 처리하고 관련 효과를 실행한다.
    /// 이미 만료 처리된 체크는 기록을 중복하지 않고, 정신력 감소와 비상등 및 비네트 효과를 한 번 적용한다.
    /// </summary>
    private void HandlePhoneRingStop()
    {
        int minute = Managers.Date.CurrentMinute;
        for (int i = _checks.Count - 1; i >= 0; i--)
        {
            TimelineCheck check = _checks[i];
            if (check.Data.Id != PHONE_RING_CHECK_ID)
                continue;

            _checks.RemoveAt(i);
            if (!check.IsOverdue)
                Resolve(check, CheckState.Failed, minute, false);

            Managers.Game.PunchMentality(RULE_FAIL_PENALTY);
            return;
        }
    }

    /// <summary>
    /// 끝난 판정 구간 check의 결과 state를 minute 시각으로 _checkRecords에 기록한다.
    /// 성공이면 Success, 실패면 Fail 결과를 실행한다.
    /// </summary>
    private void Resolve(TimelineCheck check, CheckState state, int minute, bool applyPenalty = true)
    {
        bool isSuccess = state == CheckState.Succeeded;

        _checkRecords.Add(new CheckRecord(check.Data.Id, check.StartTime, ToTimeText(minute), isSuccess, check.RepeatCount));
        ApplyOutcome(isSuccess ? check.Data.Success : check.Data.Fail, applyPenalty);
    }

    /// <summary>
    /// 판정 결과 outcome에 적힌 플래그 기록, 선택적 정신력 감소, 에러 팩스 출력을 실행한다.
    /// applyPenalty가 true일 때만 정신력 감소를 적용하고, 에러 팩스는 Delay가 있으면 그만큼 기다린 뒤 출력한다.
    /// </summary>
    private void ApplyOutcome(CheckOutcome outcome, bool applyPenalty)
    {
        // 결과가 필요 없는 쪽은 Timeline.json에서 생략할 수 있다.
        if (outcome == null)
            return;

        if (outcome.Flag != null)
            _flags.Add(outcome.Flag);
        if (applyPenalty && outcome.Penalty)
            Managers.Game.ChangeMentality(-RULE_FAIL_PENALTY);
        if (outcome.ErrorFax == null)
            return;

        if (outcome.Delay > 0f)
            _delayedErrorFaxCoroutines.Add(Managers.Instance.StartCoroutine(PrintErrorFaxAfterDelay(outcome.ErrorFax, outcome.Delay)));
        else
            Managers.Fax.PrintErrorFax(_texts[outcome.ErrorFax]);
    }

    /// <summary>
    /// instructionId 명령서를 머리글과 함께 출력하고 진짜 명령서 여부를 함께 전달한다.
    /// time은 머리글에 들어갈 "HH:mm" 시각이다.
    /// </summary>
    private void PrintInstruction(string instructionId, string time)
    {
        InstructionData instruction = _instructions[instructionId];
        string body = string.Join("\n", instruction.Lines.Select(id => _texts[id]));
        string message = string.Format(_texts[FAX_HEADER_ID], time) + "\n" + body;

        Managers.Fax.InstantiateFaxMessage(message, instruction.IsReal);
    }

    /// <summary>
    /// delay초를 기다린 뒤 textId 문구로 에러 팩스를 출력한다.
    /// </summary>
    private IEnumerator PrintErrorFaxAfterDelay(string textId, float delay)
    {
        yield return new WaitForSeconds(delay);

        Managers.Fax.PrintErrorFax(_texts[textId]);
    }

    /// <summary>
    /// timelineEvent의 If, IfNot 조건을 _flags와 비교한다.
    /// 두 조건을 모두 만족하거나 조건이 없으면 true를 반환한다.
    /// </summary>
    private bool IsConditionMet(TimelineEvent timelineEvent)
    {
        // If와 IfNot은 Timeline.json에서 생략할 수 있어 비어 있으면 조건 없음으로 본다.
        if (timelineEvent.If != null && !_flags.Contains(timelineEvent.If))
            return false;
        if (timelineEvent.IfNot != null && _flags.Contains(timelineEvent.IfNot))
            return false;

        return true;
    }

    /// <summary>
    /// 컨트롤러가 보고한 action으로 장치 상태를 갱신한다.
    /// 조명 조작은 깜빡임을, 문 조작은 노크를 함께 멈추며, 전화기 동작은 _phoneState를 바꾼다.
    /// </summary>
    private void UpdateDeviceState(DeviceAction action)
    {
        switch (action)
        {
            case DeviceAction.DoorOpened:
                _isDoorOpen = true;
                _isKnocking = false;
                break;
            case DeviceAction.DoorClosed:
                _isDoorOpen = false;
                _isKnocking = false;
                break;
            case DeviceAction.LightOn:
                _isLightOn = true;
                _isLightBlinking = false;
                break;
            case DeviceAction.LightOff:
                _isLightOn = false;
                _isLightBlinking = false;
                break;
            case DeviceAction.PhoneRinging:
                _phoneState = PhoneState.Ringing;
                break;
            case DeviceAction.PhoneAnswered:
                _phoneState = PhoneState.InCall;
                break;
            case DeviceAction.PhoneIdle:
                _phoneState = PhoneState.Idle;
                break;
        }
    }

    /// <summary>
    /// state를 현재 장치 상태로 해석해 지금 유지되고 있는지 반환한다.
    /// 문, 조명, 전화기 동작을 현재 상태와 비교하며, 상태로 해석하지 않는 동작은 항상 유지된 것으로 본다.
    /// </summary>
    private bool IsStateHeld(DeviceAction state)
    {
        switch (state)
        {
            case DeviceAction.DoorOpened:
                return _isDoorOpen;
            case DeviceAction.DoorClosed:
                return !_isDoorOpen;
            case DeviceAction.LightOn:
                return _isLightOn;
            case DeviceAction.LightOff:
                return !_isLightOn;
            case DeviceAction.PhoneRinging:
                return _phoneState == PhoneState.Ringing;
            case DeviceAction.PhoneAnswered:
                return _phoneState == PhoneState.InCall;
            case DeviceAction.PhoneIdle:
                return _phoneState == PhoneState.Idle;
            default:
                return true;
        }
    }

    /// <summary>
    /// "HH:mm" 형식의 time을 하루 시작부터 지난 분으로 바꾼다.
    /// DateManager.CurrentMinute와 같은 기준의 정수 분을 반환한다.
    /// </summary>
    private int ToMinute(string time)
    {
        return (int)TimeSpan.ParseExact(time, TIME_FORMAT, CultureInfo.InvariantCulture).TotalMinutes;
    }

    /// <summary>
    /// 하루 시작부터 지난 분 minute을 "HH:mm" 형식 문자열로 바꿔 반환한다.
    /// </summary>
    private string ToTimeText(int minute)
    {
        return TimeSpan.FromMinutes(minute).ToString(TIME_FORMAT, CultureInfo.InvariantCulture);
    }
}
