using UnityEngine;

[System.Serializable]
public struct FaultSchedule
{
    public float FaultTiming;
    public int FaultFacilityCount;

}
public class FaultScheduler : MonoBehaviour
{
    [SerializeField]
    private   FaultSchedule[] faultSchedules;

    private int _faultIndex = 0;

    public float SoonFaultTiming => faultSchedules[_faultIndex].FaultTiming;

    public void Initialize()
    {

    }

    public void MakeFault()
    {
        //갯수 지정된 만큼 설비 망가뜨리기.
        var currentFault = faultSchedules[_faultIndex];
        //사고

        _faultIndex++;
    }

}
