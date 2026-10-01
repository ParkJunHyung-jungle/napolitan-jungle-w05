using System;
using UnityEngine;

[System.Serializable]
public struct FaultSchedule
{
    public float FaultTiming;
    public int[] FaultFacilityIDs;
    public float FatalTime;

}
public class FaultScheduler : MonoBehaviour
{
    public Action<FaultSchedule> OnFaultMade;

    [SerializeField]
    private FaultSchedule[] faultSchedules;

    private DateManager _dateManager;

    private FacilityManager _facilityManager;
    private int _faultIndex = 0;

    public float SoonFaultTiming => faultSchedules[_faultIndex].FaultTiming; // 스케쥴의 타이밍을 가져옴


    private void Start()
    {
        _facilityManager = GetComponent<FacilityManager>();
        Managers.Date.OnMinuteChange += TryMakeAFault;
        TryMakeAFault(0);
        Managers.Date.OnMinuteChange += TryMakeBFault;
        TryMakeBFault(0);
        Managers.Date.OnMinuteChange += TryMakeCFault;
        TryMakeCFault(0);
    }

    public void Initialize()
    {

    }

    public void TryMakeAFault(int minute)
    {
        if (minute % 60 != 35)
            return;

        var currentFault = faultSchedules[0];
        //사고
        OnFaultMade?.Invoke(currentFault);
    }

    public void TryMakeBFault(int minute)
    {
        if (minute % 60 != 5)
            return;

        var currentFault = faultSchedules[1];
        //사고
        OnFaultMade?.Invoke(currentFault);
    }

    public void TryMakeCFault(int minute)
    {
        if (minute % 60 != 20)
            return;

        var currentFault = faultSchedules[2];
        //사고
        OnFaultMade?.Invoke(currentFault);
    }

}
