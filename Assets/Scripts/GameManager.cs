using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private MainPanelDisplay mainPanelDisplay;

    [SerializeField]
    private float deltaProgress;
    [SerializeField]
    private float maxProgressValue = 100f;

    private SystemTimer _systemTimer;
    private FacilityManager _facilityManager;
    private FaultScheduler _faultScheduler;
    private float _gaugeValue;

    public int _hp;

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
        _gaugeValue = 0;
        _hp = 5;
    }

    private void Initialize()
    {

        _facilityManager = GetComponent<FacilityManager>();
        _faultScheduler = GetComponent<FaultScheduler>();


        _facilityManager.Initialize(this);




        _facilityManager.OnFacilityStatusChanged += OnFacilityInteracted;
    }

    void Update()
    {
        if (IsNormal)
        {
            _gaugeValue += deltaProgress * Time.deltaTime;
            mainPanelDisplay.SetProgress(_gaugeValue / maxProgressValue);
            _faultScheduler.TryMakeFault(_gaugeValue / maxProgressValue);

        }
        if (_gaugeValue / maxProgressValue > 1)
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

