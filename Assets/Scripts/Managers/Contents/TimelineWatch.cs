using System;
using System.Linq;

public enum WatchState
{
    Running,
    Succeeded,
    Failed,
}

/// <summary>
/// Timeline.json의 watch 하나가 진행 중인 판정 구간이다.
/// 보고된 장치 동작으로 Require 순서 진행, Forbid 위반, Keep 상태 유지를 판정한다.
/// </summary>
public class TimelineWatch
{
    private int _requireIndex;

    public WatchData Data { get; }
    public string StartTime { get; }
    public int EndMinute { get; }

    /// <summary>
    /// data 판정 구간을 startTime 시각에 만든다.
    /// startMinute에 data.Minutes를 더해 EndMinute에 저장한다.
    /// </summary>
    public TimelineWatch(WatchData data, string startTime, int startMinute)
    {
        Data = data;
        StartTime = startTime;
        EndMinute = startMinute + data.Minutes;
    }

    /// <summary>
    /// 판정 구간을 시작하며 Keep 상태가 이미 깨져 있는지 확인한다.
    /// isStateHeld로 현재 장치 상태를 조회하고, 깨져 있으면 Failed, 아니면 Running을 반환한다.
    /// </summary>
    public WatchState Begin(Func<DeviceAction, bool> isStateHeld)
    {
        return IsKeepBroken(isStateHeld) ? WatchState.Failed : WatchState.Running;
    }

    /// <summary>
    /// 보고된 action으로 판정을 진행하고 _requireIndex를 갱신한다.
    /// Forbid에 있거나 Keep 상태가 깨지면 Failed, Require를 순서대로 모두 채우면 Succeeded, 그 외에는 Running을 반환한다.
    /// </summary>
    public WatchState Apply(DeviceAction action, Func<DeviceAction, bool> isStateHeld)
    {
        if (Data.Forbid.Contains(action) || IsKeepBroken(isStateHeld))
            return WatchState.Failed;

        if (_requireIndex < Data.Require.Count && Data.Require[_requireIndex] == action)
            _requireIndex++;

        // Require가 없는 구간은 끝날 때까지 위반이 없어야 성공이므로 여기서 성공 처리하지 않는다.
        if (Data.Require.Count > 0 && _requireIndex == Data.Require.Count)
            return WatchState.Succeeded;

        return WatchState.Running;
    }

    /// <summary>
    /// 구간 종료 시각이 되었을 때 최종 결과를 정한다.
    /// Require를 다 채우지 못했으면 Failed, 그 외에는 Succeeded를 반환한다.
    /// </summary>
    public WatchState Expire()
    {
        return _requireIndex < Data.Require.Count ? WatchState.Failed : WatchState.Succeeded;
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
