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
    
    public bool IsNormal
    {
        get
        {
            if (_facilityManager.FaultCount == 0) return true;
            return false;
        }
    }
    void Awake()
    {
     
    }

    private void Initialize()
    {

        _facilityManager = GetComponent<FacilityManager>();

        _facilityManager.Initialize(this);




        _facilityManager.OnFacilityStatusChanged += OnFacilityInteracted;
    }

    void Update()
    {
        if (IsNormal)
        {
            _progressValue += deltaProgress * Time.deltaTime;
            mainPanelDisplay.SetProgress(_progressValue / maxProgressValue);
            _facilityManager.FaultScheduler.TryMakeFault(_progressValue / maxProgressValue);

        }
        if (_progressValue / maxProgressValue > 1)
        {
            //게임 승리 결과 표시
        }

    }
    public void OnFacilityInteracted(bool isCompleted)
    {

        mainPanelDisplay.RefreshUI();

        if(isCompleted)
        {
            //정상이 된다
            //타이머 끄고. 
        }

    }


}

