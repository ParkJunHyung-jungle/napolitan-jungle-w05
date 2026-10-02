using System.Collections;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LegacyGameManager : MonoBehaviour
{
    [SerializeField]
    private StartPanelView startPanelView;
    [SerializeField]
    private EndPanel endPanel;
    [SerializeField]
    private EndPanel gameOverPanel;

    [Tooltip("시작 패널 동안 입력을 잠글 플레이어. 비어 있으면 씬에서 찾는다")]
    [SerializeField]
    private FirstPersonController playerController;

    [Header("진행 상태 값")]
    [SerializeField]
    private float deltaProgress;
    [SerializeField]
    private float maxProgressValue = 100f;


    [SerializeField]
    private int maxDurability;

    private SystemTimer _systemTimer;
    private FacilityManager _facilityManager;
    private FaultScheduler _faultScheduler;
    private float _progressValue;

    private int _durability;


    public FacilityManager FacilityManager => _facilityManager;
    public SystemTimer SystemTimer => _systemTimer;

    public bool IsNormal
    {
        get
        {
            if (_facilityManager.FaultCount == 0) return true;
            return false;
        }
    }
    void Start()
    {
        Initialize();
    }

    private void Initialize()
    {

        _facilityManager = GetComponent<FacilityManager>();

        _systemTimer = GetComponent<SystemTimer>();

        _facilityManager.Initialize(this);
        _systemTimer.Initialize(this);

        _facilityManager.OnFacilityStatusChanged += OnFacilityInteracted;
        _facilityManager.OnFaultOccurred += OnFaultOccurred;
        _systemTimer.OnTimerEnd += OnTimerEnd;

        _faultScheduler = _facilityManager.FaultScheduler;

        _durability = maxDurability;

        if (playerController == null) playerController = FindFirstObjectByType<FirstPersonController>();

        // 시작 패널 동안 플레이어 입력을 잠그고 커서를 푼다.
        // Cursor를 직접 바꾸지 않고 FirstPersonCamera를 거쳐야 포커스 복귀 때 다시 잠기지 않는다
        playerController.SetInputLocked(true);
        playerController.FirstPersonCamera.SetCursorLocked(false);

        Time.timeScale = 0f;

        startPanelView.Initialize(() =>
        {
            startPanelView.gameObject.SetActive(false);
            Time.timeScale = 1f;

            playerController.SetInputLocked(false);
            playerController.FirstPersonCamera.SetCursorLocked(true);
        });

        endPanel.Initialize(() =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
        gameOverPanel.Initialize(() =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });

        endPanel.gameObject.SetActive(false);
        gameOverPanel.gameObject.SetActive(false);

    }

    void Update()
    {
        if (IsNormal)
        {

            _progressValue += deltaProgress * Time.deltaTime;
            //_faultScheduler.TryMakeFault(_progressValue / maxProgressValue);
        }

        _systemTimer.Tick();

    }

    // 불안 상태 : 스케줄대로 고장이 났다. 타이머가 꺼져 있으면 스케줄의 제한 시간으로 켠다
    private void OnFaultOccurred(FaultSchedule schedule)
    {
        if (_systemTimer.IsActive) return;

        _systemTimer.SetTimer(schedule.FatalTime);
        Managers.Sound.ClockSound();
        Managers.Sound.EngineOffSound();
        // 첫 시작 때 개수에 따라 사이렌 종류 바꾸기
        if (_facilityManager.FaultCount > 1) Managers.Sound.ComplaxSirenSound();
        else Managers.Sound.SimpleSirenSound();
        Managers.Sound.AmbientSoundOff();
        Managers.Sound.SubAmbientSound();
    }

    // 고장 발생(false)은 OnFaultOccurred가 처리하므로 여기서는 수리 완료(true)만 본다
    public void OnFacilityInteracted(bool isCompleted)
    {
        // 안정 상태
        // 1. 모든 설비가 정상 상태이면 타이머를 끄고 등등작업 해야함.
        // 2. 
        if (isCompleted)
        {
            if (IsNormal)
            {

                //완료 사운드 재생.
                Managers.Sound.FixCompletedSound();

                //불안한 루프 끄고, 편안한 루프 키는

                _systemTimer.SetTimerEnd();
                Managers.Sound.AmbientSound();
                Managers.Sound.StopClockSound();
                Managers.Sound.StopSirenSound();
                Managers.Sound.SubAmbientSoundOff();
            }


            else
            {
                Managers.Sound.MediumFixSound();

                // 1개로 줄어들면 사이렌 종류만 바꾼다
                if ((_facilityManager.FaultCount == 1))
                {
                    Managers.Sound.StopSirenSound();
                    Managers.Sound.SimpleSirenSound();

                }

            }
        }
    }

    /// <summary>
    /// 타이머 종료 시 고장 난 시설을 정상 상태로 초기화하고 실패 조명 효과를 실행한다.
    /// 시설 목록과 타이머를 정리하고 현재 내구도를 감소시킨 뒤 효과음을 재생한다.
    /// </summary>
    private void OnTimerEnd()
    {
        foreach (Facility facility in _facilityManager.Facilities)
        {
            if (facility.IsFault()) facility.Clear();
        }

        _systemTimer.SetTimerEnd();
        Managers.Sound.AmbientSound();
        Managers.Sound.StopClockSound();
        Managers.Sound.StopSirenSound();
        Managers.Sound.SubAmbientSoundOff();
        //Managers.Light.TriggerTimerFailure();
        Debug.Log("시스템 유지 실패");
        _durability--;
        Managers.Sound.TakingDamageSound();
        if (_durability == 3) Managers.Sound.HalfHpSound();
        else if (_durability == 2) Managers.Sound.HalfHpSound();
        else if (_durability == 1) Managers.Sound.LowHpSound();
    }
}
