using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

using UI;

public class GameManager
{
    private const float DEFAULT_MAX_MENTALITY = 100f;
    private const float MENTALITY_INSEIN_THRESHOLD = 0.25f;
    private const float MENTALITY_HEARTBEAT_THRESHOLD = 0.75f;
    private const float MENTALITY_STAGE_2_THRESHOLD = 0.75f;
    private const float MENTALITY_STAGE_3_THRESHOLD = 0.5f;
    private const float MENTALITY_STAGE_4_THRESHOLD = 0.25f;
    private const float STAGE_1_DISTORTION_INTENSITY = 10f;
    private const float STAGE_2_DISTORTION_INTENSITY = 25f;
    private const float STAGE_3_DISTORTION_INTENSITY = 40f;
    private const float STAGE_4_DISTORTION_INTENSITY = 60f;
    private const float MENTALITY_SOUND_FADE_SECONDS = 5f;

    public GameInfo GameInfo { get; private set; }

    public event Action<float, float> OnMentalityChanged;
    public event Action OnMissionCompleted;

    private bool _isDayEnd = false;
    public bool IsDayEnded => _isDayEnd;
    private bool _isGameOver;
    private Coroutine _mentalitySoundFadeRoutine;
    private AudioSource _mentalitySoundFadeHeartBeatSource;
    private AudioSource _mentalitySoundFadeInseinSource;
    private float _mentalitySoundFadeHeartBeatVolume;
    private float _mentalitySoundFadeInseinVolume;

    private bool _isLeverPulledAtTwo = false;
    public bool IsLeverPulledAtTwo => _isLeverPulledAtTwo;

    private float _maxMentality = DEFAULT_MAX_MENTALITY;
    public float MaxMentality => _maxMentality;
    private float _currentMentality;
    public float CurrentMentality => _currentMentality;

    private Canvas _startCanvas;
    private Canvas _dayEndCanvas;
    private Canvas _endingCanvas;
    private Canvas _gameOverCanvas;

    public GameObject StartCanvasPrefab => Resources.Load<GameObject>("Prefabs/UIs/StartCanvas");
    public GameObject DayEndCanvasPrefab => Resources.Load<GameObject>("Prefabs/UIs/DayEndCanvas");
    public GameObject EndingCanvasPrefab => Resources.Load<GameObject>("Prefabs/UIs/EndingCanvas");
    public GameObject GameOverCanvasPrefab => Resources.Load<GameObject>("Prefabs/UIs/GameOverCanvas");

    private FirstPersonController _player;
    public FirstPersonController Player { get; set; }

    /// <summary>
    /// 게임 정보와 날짜 이벤트를 초기화하고 정신력을 최대치로 설정한다.
    /// 정신력 최대값과 현재값을 관리하며 하루 종료 이벤트를 등록한다.
    /// </summary>
    public void Init()
    {
        _currentMentality = _maxMentality;
        GameInfo = Resources.Load<GameInfo>("Datas/GameInfo");
        Managers.Date.OnDayEnd += ShowDayEndCanvas;

        _startCanvas = UnityEngine.Object.Instantiate(StartCanvasPrefab).GetComponent<Canvas>();
        _startCanvas.transform.SetParent(Managers.Instance.transform);
        _startCanvas.gameObject.GetComponent<GameStateUI>().Button.onClick.AddListener(OnStartButtonClick);

        _dayEndCanvas = UnityEngine.Object.Instantiate(DayEndCanvasPrefab).GetComponent<Canvas>();
        _dayEndCanvas.transform.SetParent(Managers.Instance.transform);
        _dayEndCanvas.gameObject.SetActive(false);
        _dayEndCanvas.gameObject.GetComponent<GameStateUI>().Button.onClick.AddListener(OnDayEndButtonClick);

        _endingCanvas = UnityEngine.Object.Instantiate(EndingCanvasPrefab).GetComponent<Canvas>();
        _endingCanvas.transform.SetParent(Managers.Instance.transform);
        _endingCanvas.gameObject.SetActive(false);
        _endingCanvas.gameObject.GetComponent<GameStateUI>().Button.onClick.AddListener(OnEndingButtonClick);

        _gameOverCanvas = UnityEngine.Object.Instantiate(GameOverCanvasPrefab).GetComponent<Canvas>();
        _gameOverCanvas.transform.SetParent(Managers.Instance.transform);
        _gameOverCanvas.gameObject.SetActive(false);
        _gameOverCanvas.gameObject.GetComponent<GameStateUI>().Button.onClick.AddListener(OnGameOverButtonClick);
    }

    /// <summary>
    /// 게임 재시작 시 하루 종료 상태를 초기화한다.
    /// _isDayEnd와 IsMissionComplete를 초기화해 다음 플레이의 출구 판정을 준비한다.
    /// </summary>
    public void Clear()
    {
        StopMentalitySoundFade();
        _isDayEnd = false;
    }

    /// <summary>
    /// 임무 시스템에서 완료를 알릴 때 완료 상태를 저장하고 구독자에게 한 번 통지한다.
    /// IsMissionComplete를 true로 변경하며 OnMissionCompleted를 호출한다.
    /// </summary>
    public void CompleteMission()
    {
        OnMissionCompleted?.Invoke();
    }

    /// <summary>
    /// 하루 종료 후 모든 명령서가 제출되면 임무 완료를 알리고 엔딩 화면을 표시한다.
    /// IsDayEnded와 IsMissionComplete를 확인하며 _endingCanvas와 입력 모드를 UI 상태로 변경한다.
    /// </summary>
    public void ShowEndingCanvas()
    {
        Managers.Sound.StopAllSoundsExceptMentalitySounds();
        StartMentalitySoundFade();
        _endingCanvas.gameObject.SetActive(true);
        Managers.Input.SetInputMode(InputMode.UI);
    }

    /// <summary>
    /// 하루 종료 캔버스를 한 번만 생성한다.
    /// _isDayEnd를 갱신하고 EndCanvas 리소스를 화면에 추가한다.
    /// </summary>
    public void ShowDayEndCanvas()
    {
        if (_isDayEnd)
            return;
        _isDayEnd = true;
        Managers.Sound.StopAllSoundsExceptMentalitySounds();
        StartMentalitySoundFade();
        _dayEndCanvas.gameObject.SetActive(true);
        Managers.Input.SetInputMode(InputMode.UI, false);
    }

    /// <summary>
    /// 하루 종료 캔버스를 한 번만 생성한다.
    /// _isDayEnd를 갱신하고 EndCanvas 리소스를 화면에 추가한다.
    /// </summary>
    public void ShowGameOverCanvas()
    {
        if (_isGameOver)
            return;

        _isGameOver = true;
        Managers.Sound.StopAllSoundsExceptMentalitySounds();
        Managers.Sound.GameOverSound();
        StartMentalitySoundFade();
        Time.timeScale = 0f;

        _gameOverCanvas.gameObject.SetActive(true);
        Managers.Input.SetInputMode(InputMode.UI);
    }

    /// <summary>
    /// 진행 중인 페이드가 없으면 HeartBeat와 Insein의 5초 페이드를 시작한다.
    /// 두 채널의 AudioSource를 코루틴에 전달하고 진행 상태를 저장한다.
    /// </summary>
    private void StartMentalitySoundFade()
    {
        if (_mentalitySoundFadeRoutine != null)
            return;

        _mentalitySoundFadeHeartBeatSource = Managers.Sound.HeartBeatSource;
        _mentalitySoundFadeInseinSource = Managers.Sound.InseinSource;
        _mentalitySoundFadeHeartBeatVolume = _mentalitySoundFadeHeartBeatSource.volume;
        _mentalitySoundFadeInseinVolume = _mentalitySoundFadeInseinSource.volume;
        _mentalitySoundFadeRoutine = Managers.Instance.StartCoroutine(FadeOutMentalitySounds());
    }

    /// <summary>
    /// 실행 중인 정신력 사운드 페이드를 중지하고 두 AudioSource의 원래 볼륨을 복구한다.
    /// </summary>
    private void StopMentalitySoundFade()
    {
        if (_mentalitySoundFadeRoutine == null)
            return;

        Managers.Instance.StopCoroutine(_mentalitySoundFadeRoutine);
        _mentalitySoundFadeRoutine = null;
        RestoreMentalitySoundVolumes();
    }

    /// <summary>
    /// 정신력 효과음 두 채널의 볼륨을 5초 동안 실시간으로 낮춘 뒤 재생을 중지한다.
    /// 입력 채널들의 초기 볼륨을 복구해 이후 효과음 재생에 영향을 주지 않는다.
    /// </summary>
    private IEnumerator FadeOutMentalitySounds()
    {
        float elapsed = 0f;

        while (elapsed < MENTALITY_SOUND_FADE_SECONDS)
        {
            elapsed += Time.unscaledDeltaTime;
            float volumeRatio = 1f - Mathf.Clamp01(elapsed / MENTALITY_SOUND_FADE_SECONDS);
            _mentalitySoundFadeHeartBeatSource.volume = _mentalitySoundFadeHeartBeatVolume * volumeRatio;
            _mentalitySoundFadeInseinSource.volume = _mentalitySoundFadeInseinVolume * volumeRatio;
            yield return null;
        }

        _mentalitySoundFadeHeartBeatSource.Stop();
        _mentalitySoundFadeInseinSource.Stop();
        RestoreMentalitySoundVolumes();
        _mentalitySoundFadeRoutine = null;
    }

    /// <summary>
    /// 페이드에서 저장한 HeartBeat와 Insein의 원래 볼륨을 복구하고 소스 참조를 비운다.
    /// </summary>
    private void RestoreMentalitySoundVolumes()
    {
        if (_mentalitySoundFadeHeartBeatSource != null)
            _mentalitySoundFadeHeartBeatSource.volume = _mentalitySoundFadeHeartBeatVolume;
        if (_mentalitySoundFadeInseinSource != null)
            _mentalitySoundFadeInseinSource.volume = _mentalitySoundFadeInseinVolume;

        _mentalitySoundFadeHeartBeatSource = null;
        _mentalitySoundFadeInseinSource = null;
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

        bool wasInseinRange = _currentMentality / _maxMentality <= MENTALITY_INSEIN_THRESHOLD;
        bool wasHeartbeatRange = _currentMentality / _maxMentality <= MENTALITY_HEARTBEAT_THRESHOLD;
        _currentMentality = currentMentality;
        Debug.Log($"정신력: {_currentMentality:F1} / {_maxMentality:F1}, 요청 변화량: {amount:+0.0;-0.0;0}");
        ApplyMentalityDistortion();
        OnMentalityChanged?.Invoke(_currentMentality, _maxMentality);

        bool hasActiveAnomalies = Managers.Timeline.HasActiveAnomalies
            || Managers.Timeline.IsDoorLeftOpen || Managers.Timeline.IsLightLeftOff;
        if (!hasActiveAnomalies)
        {
            Managers.Sound.StopInseinSound();
            Managers.Sound.StopHeartBeatSound();
        }


        bool isInseinRange = _currentMentality / _maxMentality <= MENTALITY_INSEIN_THRESHOLD;
        if (hasActiveAnomalies && wasInseinRange != isInseinRange)
        {
            if (isInseinRange)
                Managers.Sound.InseinSound();
            else
                Managers.Sound.StopInseinSound();
        }

        bool isHeartbeatRange = _currentMentality / _maxMentality <= MENTALITY_HEARTBEAT_THRESHOLD;
        if (hasActiveAnomalies && wasHeartbeatRange != isHeartbeatRange)
        {
            if (isHeartbeatRange)
                Managers.Sound.HeartBeatSound();
            else
                Managers.Sound.StopHeartBeatSound();
        }

        if (Mathf.Approximately(_currentMentality, 0f))
            ShowGameOverCanvas();
    }

    /// <summary>
    /// 정신력 실패 연출과 감소를 한 번에 실행한다.
    /// penalty만큼 정신력을 줄이고 비상등, 왜곡, 비네트 punch를 재생한다.
    /// </summary>
    public void PunchMentality(float penalty)
    {
        Managers.Light.PunchEmergencyLight();
        Managers.PostProcessing.PunchDistortion();
        Managers.PostProcessing.PunchVignette();
        ChangeMentality(-penalty);
        Managers.Sound.TakingDamageSound();
        Managers.Sound.GetHitSound();

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
    /// 레버가 두 시 정각에 당겨졌음을 기록한다.
    /// _isLeverPulledAtTwo를 true로 변경한다.
    /// </summary>
    public void MarkLeverPulledAtTwo()
    {
        _isLeverPulledAtTwo = true;
    }

    /// <summary>
    /// 시작 버튼 입력을 받아 시작 화면을 닫고 플레이어 입력 모드로 전환한다.
    /// _startCanvas의 활성 상태와 입력 모드를 변경하고 00:00 타임라인 이벤트를 실행한다.
    /// </summary>
    private void OnStartButtonClick()
    {
        _startCanvas.gameObject.SetActive(false);
        Managers.Input.SetInputMode(InputMode.Player);
        Managers.Timeline.TriggerEvents(0);
    }

    /// <summary>
    /// 하루 종료 버튼 입력을 받아 하루 종료 화면을 닫고 플레이어 입력 모드로 전환한다.
    /// _dayEndCanvas의 활성 상태와 입력 모드를 변경하며 게임 시계는 종료 상태로 유지한다.
    /// </summary>
    private void OnDayEndButtonClick()
    {
        _dayEndCanvas.gameObject.SetActive(false);
        Managers.Input.SetInputMode(InputMode.Player);
    }

    /// <summary>
    /// 엔딩 화면의 퇴근 버튼 입력을 받아 현재 씬을 다시 연다.
    /// _endingCanvas를 비활성화하고 매니저 상태를 초기화한다.
    /// </summary>
    private void OnEndingButtonClick()
    {
        _endingCanvas.gameObject.SetActive(false);
        Managers.Clear();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// 게임오버 버튼 입력을 받아 게임오버 화면을 닫고 현재 씬을 다시 연다.
    /// _gameOverCanvas를 비활성화하고 시간과 매니저 상태를 정리한다.
    /// </summary>
    private void OnGameOverButtonClick()
    {
        _gameOverCanvas.gameObject.SetActive(false);
        Time.timeScale = 1f;
        Managers.Clear();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
