using System;
using System.Linq;

public enum CheckState
{
    Running,
    Succeeded,
    Failed,
}

/// <summary>
/// Timeline.json의 check 하나가 진행 중인 판정 구간이다.
/// 보고된 장치 동작으로 Require 순서 진행, Forbid 위반, Keep 상태 유지를 판정한다.
/// </summary>
public class TimelineCheck
{
    private int _requireIndex;
    private bool _isBroken;
    private int _nextRepeatMinute;

    public CheckData Data { get; }
    public string StartTime { get; }
    public int EndMinute { get; }
    public int RepeatCount { get; private set; }
    public bool IsRepeating => Data.WhileBroken != null;

    /// <summary>
    /// data 판정 구간을 startTime 시각에 만든다.
    /// startMinute에 data.Minutes를 더해 EndMinute에 저장한다.
    /// </summary>
    public TimelineCheck(CheckData data, string startTime, int startMinute)
    {
        Data = data;
        StartTime = startTime;
        EndMinute = startMinute + data.Minutes;
    }

    /// <summary>
    /// 판정 구간을 시작하며 Keep 상태가 이미 깨져 있는지 확인한다.
    /// 반복 구간이 아닌데 깨져 있으면 Failed, 그 외에는 Running을 반환한다.
    /// </summary>
    public CheckState Begin(Func<DeviceAction, bool> isStateHeld)
    {
        if (!IsRepeating && IsKeepBroken(isStateHeld))
            return CheckState.Failed;

        return CheckState.Running;
    }

    /// <summary>
    /// 보고된 action으로 판정을 진행하고 _requireIndex를 갱신한다.
    /// Forbid에 있거나 반복 구간이 아닌데 Keep 상태가 깨지면 Failed, Require를 순서대로 모두 채우면 Succeeded, 그 외에는 Running을 반환한다.
    /// </summary>
    public CheckState Apply(DeviceAction action, Func<DeviceAction, bool> isStateHeld)
    {
        if (Data.Forbid.Contains(action) || (!IsRepeating && IsKeepBroken(isStateHeld)))
            return CheckState.Failed;

        if (_requireIndex < Data.Require.Count && Data.Require[_requireIndex] == action)
            _requireIndex++;

        // Require가 없는 구간은 끝날 때까지 위반이 없어야 성공이므로 여기서 성공 처리하지 않는다.
        if (Data.Require.Count > 0 && _requireIndex == Data.Require.Count)
            return CheckState.Succeeded;

        return CheckState.Running;
    }

    /// <summary>
    /// 반복 구간의 Keep 상태를 minute 시각 기준으로 확인해 반복 결과를 실행할 차례인지 반환한다.
    /// 새로 깨진 순간이거나 깨진 상태로 WhileBroken.Every분이 지났으면 true를 반환하고 RepeatCount와 다음 실행 시각을 갱신한다.
    /// </summary>
    public bool CheckRepeat(int minute, Func<DeviceAction, bool> isStateHeld)
    {
        if (!IsKeepBroken(isStateHeld))
        {
            _isBroken = false;
            return false;
        }

        // 이미 깨진 상태가 이어지는 중이면 다음 반복 시각이 되었을 때만 실행한다.
        if (_isBroken && minute < _nextRepeatMinute)
            return false;

        _isBroken = true;
        _nextRepeatMinute = minute + Data.WhileBroken.Every;
        RepeatCount++;
        return true;
    }

    /// <summary>
    /// 구간 종료 시각이 되었을 때 최종 결과를 정한다.
    /// Require를 다 채우지 못했거나 반복 구간에서 한 번이라도 깨졌으면 Failed, 그 외에는 Succeeded를 반환한다.
    /// </summary>
    public CheckState Expire()
    {
        if (_requireIndex < Data.Require.Count || RepeatCount > 0)
            return CheckState.Failed;

        return CheckState.Succeeded;
    }

    /// <summary>
    /// Keep에 적힌 상태 중 하나라도 유지되지 않는지 확인한다.
    /// isStateHeld로 각 상태를 조회하고, 깨진 상태가 있으면 true를 반환한다.
    /// </summary>
    private bool IsKeepBroken(Func<DeviceAction, bool> isStateHeld)
    {
        return Data.Keep.Any(state => !isStateHeld(state));
    }
}
