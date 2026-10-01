using UnityEngine;

public class GameManager
{
    public GameInfo GameInfo { get; private set; }

    private bool _isDayEnd = false;

    private bool _isLeverPulledAtTwo = false;
    public bool IsLeverPulledAtTwo => _isLeverPulledAtTwo;

    public void Init()
    {
        GameInfo = Resources.Load<GameInfo>("Datas/GameInfo");
        Managers.Date.OnMinuteChange += PrintFaxInstruction;
        Managers.Date.OnDayEnd += ShowEndCanvas;
    }

    public void Clear()
    {

    }

    public void PrintFaxInstruction(int currentMinute)
    {
        string instruction = GameInfo.GetInstruction(currentMinute);
        if (string.IsNullOrEmpty(instruction))
            return;

        Managers.Fax.InstantiateFaxMessage(currentMinute);
    }

    public void ShowEndCanvas()
    {
        if (_isDayEnd)
            return;
        _isDayEnd = true;
        Object.Instantiate(Resources.Load<GameObject>("Prefabs/UIs/EndCanvas"));
    }

    public void MarkLeverPulledAtTwo()
    {
        _isLeverPulledAtTwo = true;
    }
}
