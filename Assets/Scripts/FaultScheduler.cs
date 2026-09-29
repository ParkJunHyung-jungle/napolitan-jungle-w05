using System;
using UnityEngine;

[System.Serializable]
public struct FaultSchedule
{
    public float FaultTiming;
    public int FaultFacilityCount;

}
public class FaultScheduler : MonoBehaviour
{
    public Action<int> OnFaultMade;

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
            OnFaultMade?.Invoke(currentFault.FaultFacilityCount);
            _faultIndex++;
        }


    }

}
