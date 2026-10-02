using System.Collections.Generic;

using Newtonsoft.Json;

public enum TimelineEventType
{
    Fax,
    LightBlink,
    PhoneRing,
    PhoneStop,
    Knock,
    DoorOpen,
    Crying,
    CryingStop,
    Watch,
    Spawn,
}

/// <summary>
/// 컨트롤러가 타임라인에 알리는 장치 동작이다. 판정 구간의 Require, Forbid, Keep에 사용한다.
/// Keep에서는 DoorOpened, DoorClosed, LightOn, LightOff를 현재 상태로 해석한다.
/// </summary>
public enum DeviceAction
{
    DoorOpened,
    DoorClosed,
    LightOn,
    LightOff,
    PhoneAnswered,
}

/// <summary>
/// 팩스 한 장의 구성이다. Lines에는 Texts.json의 문구 id를 출력 순서대로 적는다.
/// IsReal은 미니게임에서 진짜 명령서인지 판정하는 값이며 false이면 가짜 명령서이다.
/// </summary>
public class FaxData
{
    [JsonProperty]
    public string Id { get; private set; }
    [JsonProperty]
    public bool Header { get; private set; }
    [JsonProperty]
    public List<string> Lines { get; private set; }
    [JsonProperty(Required = Required.Always)]
    public bool IsReal { get; private set; }
}

/// <summary>
/// 판정 구간이 끝났을 때 실행할 결과이다.
/// Fax는 Delay초 뒤 출력할 팩스 id, Flag는 기록할 플래그, Penalty는 정신력 감소 여부이다.
/// </summary>
public class WatchOutcome
{
    [JsonProperty]
    public string Fax { get; private set; }
    [JsonProperty]
    public float Delay { get; private set; }
    [JsonProperty]
    public string Flag { get; private set; }
    [JsonProperty]
    public bool Penalty { get; private set; }
}

/// <summary>
/// 이벤트에 붙는 판정 구간이다. 이벤트 시각부터 Minutes분 동안 장치 동작을 판정한다.
/// Require는 순서대로 해야 할 동작, Forbid는 하면 안 되는 동작, Keep은 구간 내내 유지할 상태이다.
/// </summary>
public class WatchData
{
    [JsonProperty]
    public string Id { get; private set; }
    [JsonProperty]
    public int Minutes { get; private set; }
    [JsonProperty]
    public List<DeviceAction> Require { get; private set; } = new();
    [JsonProperty]
    public List<DeviceAction> Forbid { get; private set; } = new();
    [JsonProperty]
    public List<DeviceAction> Keep { get; private set; } = new();
    [JsonProperty]
    public WatchOutcome Success { get; private set; }
    [JsonProperty]
    public WatchOutcome Fail { get; private set; }
}

/// <summary>
/// 특정 시각에 실행할 이벤트 하나이다. Time은 "HH:mm" 형식이고 Arg는 Type에 따라 의미가 다르다(Fax는 팩스 id, Spawn은 Resources/Prefabs의 프리팹 이름).
/// If가 있으면 해당 플래그가 있을 때만, IfNot이 있으면 해당 플래그가 없을 때만 실행하며, Watch가 있으면 판정 구간을 시작한다.
/// </summary>
public class TimelineEvent
{
    [JsonProperty]
    public string Time { get; private set; }
    [JsonProperty]
    public TimelineEventType Type { get; private set; }
    [JsonProperty]
    public string Arg { get; private set; }
    [JsonProperty]
    public string If { get; private set; }
    [JsonProperty]
    public string IfNot { get; private set; }
    [JsonProperty]
    public WatchData Watch { get; private set; }
}

/// <summary>
/// Timeline.json 전체 구조이다.
/// </summary>
public class TimelineData
{
    [JsonProperty]
    public List<FaxData> Faxes { get; private set; }
    [JsonProperty]
    public List<TimelineEvent> Events { get; private set; }
}
