/// <summary>
/// 판정 구간 하나의 결과 기록이다. Id는 Timeline.json의 watch id이고 시각은 "HH:mm" 형식이다.
/// CALL의 성공은 받은 전화, 실패는 놓친 전화처럼 Id와 IsSuccess로 결과를 구분한다.
/// </summary>
public class WatchRecord
{
    public string Id { get; }
    public string StartTime { get; }
    public string EndTime { get; }
    public bool IsSuccess { get; }

    /// <summary>
    /// 판정 결과 기록을 만든다.
    /// id, startTime, endTime, isSuccess를 그대로 저장한다.
    /// </summary>
    public WatchRecord(string id, string startTime, string endTime, bool isSuccess)
    {
        Id = id;
        StartTime = startTime;
        EndTime = endTime;
        IsSuccess = isSuccess;
    }
}

/// <summary>
/// 출력된 팩스 한 장의 기록이다. 미니게임에서 진짜/가짜 명령서 판정에 사용한다.
/// </summary>
public class FaxRecord
{
    public string Id { get; }
    public string Time { get; }
    public bool IsReal { get; }

    /// <summary>
    /// 팩스 출력 기록을 만든다.
    /// id, time, isReal을 그대로 저장한다.
    /// </summary>
    public FaxRecord(string id, string time, bool isReal)
    {
        Id = id;
        Time = time;
        IsReal = isReal;
    }
}
