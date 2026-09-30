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

    private FacilityManager _facilityManager;
    private int _faultIndex = 0;

    public float SoonFaultTiming => faultSchedules[_faultIndex].FaultTiming;

    public void Initialize()
    {

    }

    public void TryMakeFault(float progress)
    {
        if (_faultIndex >= faultSchedules.Length) return;

        if (progress > SoonFaultTiming)
        {
            var currentFault = faultSchedules[_faultIndex];
            //사고
            OnFaultMade?.Invoke(currentFault);
            _faultIndex++;
        }


    }

}
