using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DateManager
{
    private float _elapsedTime;

    public Action<int> OnMinuteChange;
    public Action OnDayEnd;
    public Action OnDayStart;

    public float ElapsedTime
    {
        get => _elapsedTime;
        set
        {
            if (Managers.Game.IsDayEnded)
                return;

            int pastMinute = CurrentMinute;
            float dayEndTime = CurrentDay * Managers.Game.GameInfo.SecondsPerDay;
            _elapsedTime = Mathf.Min(value, dayEndTime);

            if (_elapsedTime >= dayEndTime)
            {
                OnDayEnd?.Invoke();
                return;
            }

            if (pastMinute != CurrentMinute)
            {
                OnMinuteChange?.Invoke(CurrentMinute);
            }
        }
    }

    public int CurrentDay =>
        Managers.Game.IsDayEnded
            ? Mathf.CeilToInt(_elapsedTime / Managers.Game.GameInfo.SecondsPerDay)
            : Mathf.FloorToInt(_elapsedTime / Managers.Game.GameInfo.SecondsPerDay) + 1;
    public float DayProgress =>
        Managers.Game.IsDayEnded
            ? 1f
            : _elapsedTime % Managers.Game.GameInfo.SecondsPerDay / Managers.Game.GameInfo.SecondsPerDay;
    public int CurrentMinute
    {
        get
        {
            float startTime = Managers.Game.GameInfo.StartTime;
            float endTime = Managers.Game.GameInfo.EndTime;

            return Mathf.FloorToInt((startTime + DayProgress * (endTime - startTime)) * 60f);
        }
    }

    public string CurrentTime
    {
        get
        {
            int totalMinutes = CurrentMinute;

            int hour = totalMinutes / 60;
            int minute = totalMinutes % 60;

            return $"{hour:00}:{minute:00} AM";
        }
    }

    public void Init()
    {
        _elapsedTime = 0f;
    }

    /// <summary>
    /// 다음 날 시작 이벤트를 구독 중인 오브젝트에 전달한다.
    /// OnDayStart를 호출해 하루 종료 연출을 초기 상태로 복구하도록 알린다.
    /// </summary>
    public void StartDay()
    {
        OnDayStart?.Invoke();
    }

    /// <summary>
    /// 게임 재시작을 위해 경과 시간을 초기 상태로 되돌린다.
    /// _elapsedTime을 0으로 변경해 다시 Day1부터 시간이 진행되게 한다.
    /// </summary>
    public void Clear()
    {
        _elapsedTime = 0f;
    }

    public void ReloadScene()
    {
        Managers.Clear();
        SceneManager.LoadScene(0);
    }
}
