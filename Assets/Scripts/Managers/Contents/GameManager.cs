using System;
using UnityEngine;
using UnityEngine.SceneManagement;

using UI;

public class GameManager
{
    private const float DEFAULT_MAX_MENTALITY = 100f;
    private const float MENTALITY_STAGE_2_THRESHOLD = 0.75f;
    private const float MENTALITY_STAGE_3_THRESHOLD = 0.5f;
    private const float MENTALITY_STAGE_4_THRESHOLD = 0.25f;
    private const float STAGE_1_DISTORTION_INTENSITY = 10f;
    private const float STAGE_2_DISTORTION_INTENSITY = 25f;
    private const float STAGE_3_DISTORTION_INTENSITY = 40f;
    private const float STAGE_4_DISTORTION_INTENSITY = 60f;

    public GameInfo GameInfo { get; private set; }

    public event Action<float, float> OnMentalityChanged;

    private bool _isDayEnd = false;
    private bool _isGameOver;

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
        ApplyMentalityDistortion();
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
        if (_isGameOver)
            return;

        float currentMentality = Mathf.Clamp(_currentMentality + amount, 0f, _maxMentality);
        if (Mathf.Approximately(currentMentality, _currentMentality))
            return;

        _currentMentality = currentMentality;
        ApplyMentalityDistortion();
        OnMentalityChanged?.Invoke(_currentMentality, _maxMentality);

        if (Mathf.Approximately(_currentMentality, 0f))
            ShowGameOver();
    }

    /// <summary>
    /// 현재 정신력을 최대 정신력으로 초기화한다.
    /// 정신력이 변경되면 OnMentalityChanged 이벤트를 통해 새 상태를 알린다.
    /// </summary>
    public void ResetMentality()
    {
        _isGameOver = false;
        ChangeMentality(_maxMentality - _currentMentality);
        ApplyMentalityDistortion();
    }

    /// <summary>
    /// 현재 정신력 비율에 맞는 네 단계 왜곡 강도를 설정한다.
    /// _currentMentality와 _maxMentality를 사용하며 PostProcessingManager의 렌즈 왜곡을 갱신한다.
    /// </summary>
    private void ApplyMentalityDistortion()
    {
        float mentalityRatio = _currentMentality / _maxMentality;
        float intensity;

        if (mentalityRatio > MENTALITY_STAGE_2_THRESHOLD)
            intensity = STAGE_1_DISTORTION_INTENSITY;
        else if (mentalityRatio > MENTALITY_STAGE_3_THRESHOLD)
            intensity = STAGE_2_DISTORTION_INTENSITY;
        else if (mentalityRatio > MENTALITY_STAGE_4_THRESHOLD)
            intensity = STAGE_3_DISTORTION_INTENSITY;
        else
            intensity = STAGE_4_DISTORTION_INTENSITY;

        Managers.PostProcessing.SetDistortion(intensity);
    }

    /// <summary>
    /// 게임오버 화면과 사운드를 실행하고 재시작 버튼에 현재 씬 재로드를 연결한다.
    /// _isGameOver를 설정해 중복 실행을 막고 Time.timeScale을 정지 상태로 변경한다.
    /// </summary>
    private void ShowGameOver()
    {
        if (_isGameOver)
            return;

        _isGameOver = true;
        Time.timeScale = 0f;
        Managers.Sound.GameOverSound();

        EndPanel[] panels = UnityEngine.Object.FindObjectsByType<EndPanel>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (EndPanel panel in panels)
        {
            if (panel.gameObject.name != "GameOverPanel")
                continue;

            panel.Initialize(() =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            });
            panel.gameObject.SetActive(true);
            break;
        }
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
