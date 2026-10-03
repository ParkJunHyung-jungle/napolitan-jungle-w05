/// <summary>
/// 판정 구간 하나의 결과 기록이다. Id는 Timeline.json의 check id이고 시각은 "HH:mm" 형식이다.
/// PHONE_RING의 성공은 받은 전화, 실패는 놓친 전화처럼 Id와 IsSuccess로 결과를 구분하며, RepeatCount는 WhileBroken으로 출력한 횟수이다.
/// </summary>
public class CheckRecord
{
    public string Id { get; }
    public string StartTime { get; }
    public string EndTime { get; }
    public bool IsSuccess { get; }
    public int RepeatCount { get; }

    /// <summary>
    /// 판정 결과 기록을 만든다.
    /// id, startTime, endTime, isSuccess, repeatCount를 그대로 저장한다.
    /// </summary>
    public CheckRecord(string id, string startTime, string endTime, bool isSuccess, int repeatCount)
    {
        Id = id;
        StartTime = startTime;
        EndTime = endTime;
        IsSuccess = isSuccess;
        RepeatCount = repeatCount;
    }
}

/// <summary>
/// 출력된 명령서 한 장의 기록이다. 미니게임에서 진짜/가짜 명령서 판정의 정답으로 사용한다.
/// </summary>
public class InstructionRecord
{
    public string Id { get; }
    public string Time { get; }
    public bool IsReal { get; }

    /// <summary>
    /// 명령서 출력 기록을 만든다.
    /// id, time, isReal을 그대로 저장한다.
    /// </summary>
    public InstructionRecord(string id, string time, bool isReal)
    {
        Id = id;
        Time = time;
        IsReal = isReal;
    }
}

/// <summary>
/// 출력된 에러 팩스 한 장의 기록이다. 명령서가 아니므로 미니게임 판정에는 포함하지 않는다.
/// Cause는 출력을 일으킨 check id이며, 타임라인 이벤트로 바로 출력한 경우 null이다.
/// </summary>
public class ErrorFaxRecord
{
    public string Id { get; }
    public string Time { get; }
    public string Cause { get; }

    /// <summary>
    /// 에러 팩스 출력 기록을 만든다.
    /// id, time, cause를 그대로 저장한다.
    /// </summary>
    public ErrorFaxRecord(string id, string time, string cause)
    {
        Id = id;
        Time = time;
        Cause = cause;
    }
}
