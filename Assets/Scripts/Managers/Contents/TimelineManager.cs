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
    private const string TIME_FORMAT = @"hh\:mm";
    private const float RULE_FAIL_PENALTY = 10f;

    [Header("Data")]
    private Dictionary<string, string> _texts;
    private Dictionary<string, FaxData> _faxes;
    private ILookup<int, TimelineEvent> _events;

    public event Action OnPhoneRing;
    public event Action OnPhoneStop;
    public event Action OnKnock;
    public event Action OnDoorOpen;
    public event Action OnCryingStop;

    [Header("Device State")]
    private bool _isLightOn = true;
    private bool _isDoorOpen;

    [Header("Watch")]
    private readonly List<TimelineWatch> _watches = new();
    private readonly List<Coroutine> _delayedFaxCoroutines = new();

    [Header("Record")]
    private readonly HashSet<string> _flags = new();
    private readonly List<WatchRecord> _watchRecords = new();
    private readonly List<FaxRecord> _faxRecords = new();

    public IReadOnlyList<WatchRecord> WatchRecords => _watchRecords;
    public IReadOnlyList<FaxRecord> FaxRecords => _faxRecords;

    /// <summary>
    /// Resources의 Texts.json과 Timeline.json을 읽어 문구, 팩스 구성, 시각별 이벤트를 준비한다.
    /// 이벤트를 분 단위로 묶어 _events에 저장하고, 분이 바뀔 때와 하루가 끝날 때 실행되도록 DateManager에 등록한다.
    /// </summary>
    public void Init()
    {
        _texts = JsonConvert.DeserializeObject<Dictionary<string, string>>(Resources.Load<TextAsset>(TEXTS_PATH).text);
        TimelineData timeline = JsonConvert.DeserializeObject<TimelineData>(Resources.Load<TextAsset>(TIMELINE_PATH).text);

        _faxes = timeline.Faxes.ToDictionary(fax => fax.Id);
        _events = timeline.Events.ToLookup(timelineEvent => ToMinute(timelineEvent.Time));

        Managers.Date.OnMinuteChange += TriggerEvents;
        Managers.Date.OnDayEnd += ExpireAllWatches;
    }

    /// <summary>
    /// 씬 오브젝트가 구독한 이벤트를 모두 해제하고 예약된 팩스 출력을 멈춘다.
    /// 판정 구간, 플래그, 기록, 장치 상태를 처음 상태로 되돌려 다음 판에 이전 판의 결과가 남지 않게 한다.
    /// </summary>
    public void Clear()
    {
        OnPhoneRing = null;
        OnPhoneStop = null;
        OnKnock = null;
        OnDoorOpen = null;
        OnCryingStop = null;

        foreach (Coroutine coroutine in _delayedFaxCoroutines)
            Managers.Instance.StopCoroutine(coroutine);
        _delayedFaxCoroutines.Clear();

        _watches.Clear();
        _flags.Clear();
        _watchRecords.Clear();
        _faxRecords.Clear();
        _isLightOn = true;
        _isDoorOpen = false;
    }

    /// <summary>
    /// minute에 끝나는 판정 구간을 먼저 마무리한 뒤, minute에 등록된 이벤트를 Timeline.json에 적힌 순서대로 실행한다.
    /// If, IfNot 조건을 만족하지 않는 이벤트는 건너뛰고, Watch가 있는 이벤트는 판정 구간을 시작한다.
    /// </summary>
    public void TriggerEvents(int minute)
    {
        ExpireWatches(minute);

        foreach (TimelineEvent timelineEvent in _events[minute])
        {
            if (!IsConditionMet(timelineEvent))
                continue;

            Execute(timelineEvent);
            if (timelineEvent.Watch != null)
                StartWatch(timelineEvent, minute);
        }
    }

    /// <summary>
    /// 컨트롤러가 알린 장치 동작 action으로 장치 상태를 갱신하고 진행 중인 판정 구간에 전달한다.
    /// 판정이 끝난 구간은 현재 시각으로 결과를 기록하고 _watches에서 제거한다.
    /// </summary>
    public void Report(DeviceAction action)
    {
        UpdateDeviceState(action);

        // 판정이 끝난 구간을 바로 제거하기 위해 뒤에서부터 순회한다.
        for (int i = _watches.Count - 1; i >= 0; i--)
        {
            TimelineWatch watch = _watches[i];
            WatchState state = watch.Apply(action, IsStateHeld);
            if (state == WatchState.Running)
                continue;

            _watches.RemoveAt(i);
            Resolve(watch, state, Managers.Date.CurrentMinute);
        }
    }

    /// <summary>
    /// flag가 판정 결과로 기록되어 있는지 확인해 반환한다.
    /// </summary>
    public bool HasFlag(string flag)
    {
        return _flags.Contains(flag);
    }

    /// <summary>
    /// id 판정 구간 중 결과가 isSuccess인 기록의 수를 반환한다.
    /// CALL의 성공은 받은 전화, 실패는 놓친 전화 수이다.
    /// </summary>
    public int CountWatchRecords(string id, bool isSuccess)
    {
        return _watchRecords.Count(record => record.Id == id && record.IsSuccess == isSuccess);
    }

    /// <summary>
    /// timelineEvent의 Type에 맞는 기존 기능을 호출한다.
    /// 문과 전화기처럼 씬 오브젝트가 처리하는 이벤트는 구독자에게 알린다.
    /// </summary>
    private void Execute(TimelineEvent timelineEvent)
    {
        switch (timelineEvent.Type)
        {
            case TimelineEventType.Fax:
                PrintFax(timelineEvent.Arg, timelineEvent.Time);
                break;
            case TimelineEventType.LightBlink:
                Managers.Light.RoomLightBlink();
                break;
            case TimelineEventType.PhoneRing:
                OnPhoneRing?.Invoke();
                break;
            case TimelineEventType.PhoneStop:
                OnPhoneStop?.Invoke();
                break;
            case TimelineEventType.Knock:
                OnKnock?.Invoke();
                break;
            case TimelineEventType.DoorOpen:
                OnDoorOpen?.Invoke();
                break;
            case TimelineEventType.Crying:
                Managers.Sound.CryingSound();
                break;
            case TimelineEventType.CryingStop:
                OnCryingStop?.Invoke();
                break;
            case TimelineEventType.Watch:
                // 판정 구간만 시작하는 이벤트라 실행할 동작이 없다.
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
    /// timelineEvent의 Watch로 판정 구간을 만들어 minute부터 시작한다.
    /// 시작 시점에 Keep 상태가 이미 깨져 있으면 바로 실패로 기록하고, 아니면 _watches에 추가한다.
    /// </summary>
    private void StartWatch(TimelineEvent timelineEvent, int minute)
    {
        TimelineWatch watch = new TimelineWatch(timelineEvent.Watch, timelineEvent.Time, minute);
        WatchState state = watch.Begin(IsStateHeld);
        if (state == WatchState.Running)
        {
            _watches.Add(watch);
            return;
        }

        Resolve(watch, state, minute);
    }

    /// <summary>
    /// 종료 시각이 minute 이하인 판정 구간을 마무리한다.
    /// 각 구간의 최종 결과를 종료 시각 기준으로 기록하고 _watches에서 제거한다.
    /// </summary>
    private void ExpireWatches(int minute)
    {
        for (int i = _watches.Count - 1; i >= 0; i--)
        {
            TimelineWatch watch = _watches[i];
            if (watch.EndMinute > minute)
                continue;

            _watches.RemoveAt(i);
            Resolve(watch, watch.Expire(), watch.EndMinute);
        }
    }

    /// <summary>
    /// 하루가 끝났을 때 남은 판정 구간을 모두 마무리한다.
    /// DateManager는 하루가 끝나면 분을 0으로 되돌리므로 05:00에 끝나는 구간은 여기서 처리된다.
    /// </summary>
    private void ExpireAllWatches()
    {
        ExpireWatches(int.MaxValue);
    }

    /// <summary>
    /// 끝난 판정 구간 watch의 결과 state를 minute 시각으로 _watchRecords에 기록한다.
    /// 성공이면 Success, 실패면 Fail 결과를 실행한다.
    /// </summary>
    private void Resolve(TimelineWatch watch, WatchState state, int minute)
    {
        bool isSuccess = state == WatchState.Succeeded;
        string time = ToTimeText(minute);

        _watchRecords.Add(new WatchRecord(watch.Data.Id, watch.StartTime, time, isSuccess));
        ApplyOutcome(isSuccess ? watch.Data.Success : watch.Data.Fail, time);
    }

    /// <summary>
    /// 판정 결과 outcome에 적힌 플래그 기록, 정신력 감소, 팩스 출력을 실행한다.
    /// 팩스는 Delay가 있으면 그만큼 기다린 뒤, 없으면 time 시각으로 바로 출력한다.
    /// </summary>
    private void ApplyOutcome(WatchOutcome outcome, string time)
    {
        // 결과가 필요 없는 쪽은 Timeline.json에서 생략할 수 있다.
        if (outcome == null)
            return;

        if (outcome.Flag != null)
            _flags.Add(outcome.Flag);
        if (outcome.Penalty)
            Managers.Game.ChangeMentality(-RULE_FAIL_PENALTY);
        if (outcome.Fax == null)
            return;

        if (outcome.Delay > 0f)
            _delayedFaxCoroutines.Add(Managers.Instance.StartCoroutine(PrintFaxAfterDelay(outcome.Fax, outcome.Delay)));
        else
            PrintFax(outcome.Fax, time);
    }

    /// <summary>
    /// faxId 팩스 구성으로 문구를 만들어 출력하고 진짜 명령서 여부를 함께 전달한다.
    /// time은 Header가 true일 때 머리글에 들어갈 "HH:mm" 시각이며, 출력한 팩스를 _faxRecords에 기록한다.
    /// </summary>
    private void PrintFax(string faxId, string time)
    {
        FaxData fax = _faxes[faxId];
        Managers.Fax.InstantiateFaxMessage(BuildFaxMessage(fax, time), fax.IsReal);
        _faxRecords.Add(new FaxRecord(faxId, time, fax.IsReal));
    }

    /// <summary>
    /// fax의 Lines 문구를 줄바꿈으로 이어 붙인다.
    /// Header가 true이면 FAX_HEADER 문구에 time을 넣어 맨 위에 붙이고, 완성된 문자열을 반환한다.
    /// </summary>
    private string BuildFaxMessage(FaxData fax, string time)
    {
        string body = string.Join("\n", fax.Lines.Select(id => _texts[id]));

        if (!fax.Header)
            return body;

        return string.Format(_texts[FAX_HEADER_ID], time) + "\n" + body;
    }

    /// <summary>
    /// delay초를 기다린 뒤 faxId 팩스를 현재 시각으로 출력한다.
    /// </summary>
    private IEnumerator PrintFaxAfterDelay(string faxId, float delay)
    {
        yield return new WaitForSeconds(delay);

        PrintFax(faxId, ToTimeText(Managers.Date.CurrentMinute));
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
    /// 문과 조명 동작 action으로 _isDoorOpen, _isLightOn을 갱신한다.
    /// 상태가 없는 동작은 아무것도 바꾸지 않는다.
    /// </summary>
    private void UpdateDeviceState(DeviceAction action)
    {
        switch (action)
        {
            case DeviceAction.DoorOpened:
                _isDoorOpen = true;
                break;
            case DeviceAction.DoorClosed:
                _isDoorOpen = false;
                break;
            case DeviceAction.LightOn:
                _isLightOn = true;
                break;
            case DeviceAction.LightOff:
                _isLightOn = false;
                break;
        }
    }

    /// <summary>
    /// state를 현재 장치 상태로 해석해 지금 유지되고 있는지 반환한다.
    /// 문과 조명 동작만 상태로 보며, 그 외 동작은 항상 유지된 것으로 본다.
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
