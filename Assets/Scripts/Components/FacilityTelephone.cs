using UnityEngine;

public class FacilityTelephone : Facility
{
    [SerializeField] private TelephoneController _telephoneController;

    private bool _isCalling;
    private bool _targetCalling;

    protected override void GenerateGoal()
    {
        // 전화가 오고 있는 상태라면 전화를 끊는 것이 목표
        _targetCalling = false;
    }

    protected override bool IsGoalReached()
    {
        return _isCalling == _targetCalling;
    }

    protected override void ResetDevices()
    {
        _telephoneController.HangUp();

        _isCalling = false;
        _targetCalling = false;
    }

    public override void Initialize()
    {
        _isCalling = _telephoneController.IsCalling;
        _targetCalling = false;
    }
}
