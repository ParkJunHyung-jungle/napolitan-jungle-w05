using System.Collections.Generic;

using Newtonsoft.Json;

/// <summary>
/// 타임라인 이벤트 종류이다. 장치 이벤트는 플레이어가 다뤄야 하는 장치 이름을 앞에 붙이고, 멈춤 이벤트는 Stop을 뒤에 붙인다.
/// CompositeCheck는 실행할 동작 없이 여러 장치에 걸친 판정 구간만 시작하는 복합 이벤트이다.
/// </summary>
public enum TimelineEventType
{
    Instruction,
    ErrorFax,
    LightBlink,
    PhoneRing,
    PhoneRingStop,
    PhoneCrying,
    PhoneCryingStop,
    DoorKnock,
    CompositeCheck,
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
/// 명령서 한 장의 구성이다. Lines에는 Texts.json의 문구 id를 출력 순서대로 적고, 머리글은 항상 붙는다.
/// IsReal은 미니게임에서 진짜 명령서인지 판정하는 값이며 false이면 가짜 명령서이다.
/// </summary>
public class InstructionData
{
    [JsonProperty]
    public string Id { get; private set; }
    [JsonProperty]
    public List<string> Lines { get; private set; }
    [JsonProperty(Required = Required.Always)]
    public bool IsReal { get; private set; }
}

/// <summary>
/// 판정 구간이 끝났을 때 실행할 결과이다.
/// ErrorFax는 Delay초 뒤 출력할 에러 팩스 문구 id, Flag는 기록할 플래그, Penalty는 정신력 감소 여부이다.
/// </summary>
public class CheckOutcome
{
    [JsonProperty]
    public string ErrorFax { get; private set; }
    [JsonProperty]
    public float Delay { get; private set; }
    [JsonProperty]
    public string Flag { get; private set; }
    [JsonProperty]
    public bool Penalty { get; private set; }
}

/// <summary>
/// Keep 상태가 깨져 있는 동안 반복할 결과이다.
/// 깨지는 순간 ErrorFax를 출력하고 계속 깨져 있으면 Every분마다 다시 출력하며, Penalty는 처음 깨질 때 한 번만 적용한다.
/// </summary>
public class CheckRepeat
{
    [JsonProperty]
    public int Every { get; private set; }
    [JsonProperty]
    public string ErrorFax { get; private set; }
    [JsonProperty]
    public bool Penalty { get; private set; }
}

/// <summary>
/// 이벤트에 붙는 판정 구간이다. 이벤트 시각부터 Minutes분 동안 장치 동작을 판정한다.
/// Require는 순서대로 해야 할 동작, Forbid는 하면 안 되는 동작, Keep은 구간 내내 유지할 상태이다.
/// WhileBroken이 있으면 Keep이 깨져도 바로 실패하지 않고 구간 끝까지 감시하며 반복 결과를 실행한다.
/// </summary>
public class CheckData
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
    public CheckOutcome Success { get; private set; }
    [JsonProperty]
    public CheckOutcome Fail { get; private set; }
    [JsonProperty]
    public CheckRepeat WhileBroken { get; private set; }
}

/// <summary>
/// 특정 시각에 실행할 이벤트 하나이다. Time은 "HH:mm" 형식이고 Arg는 Type에 따라 의미가 다르다.
/// Instruction은 명령서 id, ErrorFax는 에러 팩스 문구 id, Spawn은 Resources/Prefabs의 프리팹 이름이다.
/// If가 있으면 해당 플래그가 있을 때만, IfNot이 있으면 해당 플래그가 없을 때만 실행하며, Check가 있으면 판정 구간을 시작한다.
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
    public CheckData Check { get; private set; }
}

/// <summary>
/// Timeline.json 전체 구조이다.
/// </summary>
public class TimelineData
{
    [JsonProperty]
    public List<InstructionData> Instructions { get; private set; }
    [JsonProperty]
    public List<TimelineEvent> Events { get; private set; }
}
