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

