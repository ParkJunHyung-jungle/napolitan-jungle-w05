using System;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;

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
        Managers.Date.OnMinuteChange += TryMakeFault;
        TryMakeFault(0);
    }

    public void Initialize()
    {

    }

    public void TryMakeFault(int minute)
    {
        if (minute % 60 == 20)
        {
            var currentFault = faultSchedules[_faultIndex];
            //사고
            OnFaultMade?.Invoke(currentFault);
        }
    }

}
