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
            int pastMinute = CurrentMinute;
            int pastDay = CurrentDay;
            _elapsedTime = value;

            // 날짜가 바뀌면 분이 0으로 돌아가므로 분 변경 대신 하루 종료만 알린다.
            if (pastDay != CurrentDay)
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
        Mathf.FloorToInt(_elapsedTime / Managers.Game.GameInfo.SecondsPerDay) + 1;
    public float DayProgress =>
        _elapsedTime % Managers.Game.GameInfo.SecondsPerDay / Managers.Game.GameInfo.SecondsPerDay;
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

    public void Clear()
    {

    }

    public void ReloadScene()
    {
        Managers.Clear();
        SceneManager.LoadScene(0);
    }
}
