using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ResearchType
{
    SelfGenerate,
    SelfGenerate2,
    BatteryMax,
    TowerEfficiency,
    WheelPower,
    WheelPower2,
    TowerWire,
    MoveFast,

}

public class ResourcesData
{
    public float CurrentEnergy;
    public float MaxEnergy;
    public float Scale;
    public float Mass;
}

public class DateManager
{
    private float _elapsedTime;

    public float ElapsedTime
    {
        get => _elapsedTime;
        set => _elapsedTime = value;
    }

    public int CurrentDay =>
        Mathf.FloorToInt(_elapsedTime / Managers.Game.GameInfo.SecondsPerDay) + 1;
    public float DayProgress =>
        _elapsedTime % Managers.Game.GameInfo.SecondsPerDay / Managers.Game.GameInfo.SecondsPerDay;
    public int CurrentMinute
    {
        get
        {
            float dayProgress = DayProgress;

            int minutesPerDay = 24 * 60;

            return Mathf.FloorToInt(dayProgress * minutesPerDay)
                % minutesPerDay;
        }
    }
    public string CurrentTime
    {
        get
        {
            int totalMinutes = CurrentMinute;

            int hour = totalMinutes / 60;
            int minute = totalMinutes % 60;

            return $"{hour:00}:{minute:00}";
        }
    }

    public void Init()
    {
        _elapsedTime = 0f;
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
