using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private MainPanelDisplay mainPanelDisplay;

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


    public FacilityManager FacilityManager => _facilityManager;
    public SystemTimer SystemTimer => _systemTimer;
    [SerializeField] private VignetteController vignetteController;

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
        mainPanelDisplay.Initialize(this);


        _facilityManager.OnFacilityStatusChanged += OnFacilityInteracted;
        _systemTimer.OnTimerEnd += OnTimerEnd;

        _faultScheduler = _facilityManager.FaultScheduler;
    }

    void Update()
    {
        if (IsNormal)
        {

            _progressValue += deltaProgress * Time.deltaTime;
            _faultScheduler.TryMakeFault(_progressValue / maxProgressValue);

            mainPanelDisplay.SetProgress(_progressValue / maxProgressValue);
        }

        _systemTimer.Tick();
        mainPanelDisplay.SetRemainingTime();
        vignetteController.SetVignetteIntensity(Mathf.Clamp((_systemTimer.FatalTime - _systemTimer.CurrentTime) / _systemTimer.FatalTime, 0f, 0.5f));

        if (_progressValue / maxProgressValue > 1)
        {
            //게임 승리 결과 표시
        }

    }
    public void OnFacilityInteracted(bool isCompleted)
    {
        // 내부에서 상태를 보고 처리하기.
        // 불안 상태
        // 1. 타이머가 꺼져 있으면 켜기
        if (!isCompleted)
        {
            if (!_systemTimer.IsActive) _systemTimer.SetTimer();
        }
        // 안정 상태
        // 1. 모든 설비가 정상 상태이면 타이머를 끄고 등등작업 해야함.
        // 2. 
        if (isCompleted)
        {
            //완료 사운드 재생.
            if (IsNormal)
            {
                _systemTimer.SetTimerEnd();
            }
        }
    }

    private void OnTimerEnd()
    {
        Debug.Log("시스템 유지 실패");
    }

}

