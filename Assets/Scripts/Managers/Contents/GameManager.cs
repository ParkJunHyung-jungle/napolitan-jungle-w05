using System;
using UnityEngine;

public class GameManager
{
    private const float DEFAULT_MAX_MENTALITY = 100f;

    public GameInfo GameInfo { get; private set; }

    public event Action<float, float> OnMentalityChanged;

    private bool _isDayEnd = false;

    private bool _isLeverPulledAtTwo = false;
    public bool IsLeverPulledAtTwo => _isLeverPulledAtTwo;

    private float _maxMentality = DEFAULT_MAX_MENTALITY;
    public float MaxMentality => _maxMentality;
    private float _currentMentality;
    public float CurrentMentality => _currentMentality;

    /// <summary>
    /// 게임 정보와 날짜 이벤트를 초기화하고 정신력을 최대치로 설정한다.
    /// 정신력 최대값과 현재값을 관리하며 게임 안내 이벤트를 등록한다.
    /// </summary>
    public void Init()
    {
        _currentMentality = _maxMentality;
        GameInfo = Resources.Load<GameInfo>("Datas/GameInfo");
        Managers.Date.OnMinuteChange += PrintFaxInstruction;
        Managers.Date.OnDayEnd += ShowEndCanvas;
        PrintFaxInstruction(0);
    }

    /// <summary>
    /// GameManager의 정리 요청을 처리한다.
    /// 현재 별도로 해제할 상태가 없어 저장된 정신력과 게임 정보를 유지한다.
    /// </summary>
    public void Clear()
    {

    }

    /// <summary>
    /// currentMinute에 맞는 팩스 안내를 요청한다.
    /// GameInfo의 안내 문구를 조회하고 해당 시간이 유효하면 팩스 메시지를 생성한다.
    /// </summary>
    public void PrintFaxInstruction(int currentMinute)
    {
        string instruction = GameInfo.GetInstruction(currentMinute);
        if (string.IsNullOrEmpty(instruction))
            return;

        Managers.Fax.InstantiateFaxMessage(currentMinute);
    }

    /// <summary>
    /// 하루 종료 캔버스를 한 번만 생성한다.
    /// _isDayEnd를 갱신하고 EndCanvas 리소스를 화면에 추가한다.
    /// </summary>
    public void ShowEndCanvas()
    {
        if (_isDayEnd)
            return;
        _isDayEnd = true;
        UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UIs/EndCanvas"));
    }

    /// <summary>
    /// 정신력 변화량을 현재값에 반영한다.
    /// amount를 사용해 현재 정신력을 0과 최대 정신력 사이로 제한하고 변경 이벤트를 알린다.
    /// </summary>
    public void ChangeMentality(float amount)
    {
        float currentMentality = Mathf.Clamp(_currentMentality + amount, 0f, _maxMentality);
        if (Mathf.Approximately(currentMentality, _currentMentality))
            return;

        _currentMentality = currentMentality;
        OnMentalityChanged?.Invoke(_currentMentality, _maxMentality);
    }

    /// <summary>
    /// 현재 정신력을 최대 정신력으로 초기화한다.
    /// 정신력이 변경되면 OnMentalityChanged 이벤트를 통해 새 상태를 알린다.
    /// </summary>
    public void ResetMentality()
    {
        ChangeMentality(_maxMentality - _currentMentality);
    }

    /// <summary>
    /// 레버가 두 시 정각에 당겨졌음을 기록한다.
    /// _isLeverPulledAtTwo를 true로 변경한다.
    /// </summary>
    public void MarkLeverPulledAtTwo()
    {
        _isLeverPulledAtTwo = true;
    }
}
