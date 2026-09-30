using UnityEngine;

public class FacilityDoor : Facility
{
    [Header("Door")]
    [SerializeField] private DoorController _doorController;

    private bool _isOpen;
    private bool _targetOpen;


    protected override void GenerateGoal()
    {
        // 현재 상태의 반대를 목표로 설정
        // 열린 상태라면 닫기
        // 닫힌 상태라면 열기
        _targetOpen = !_isOpen;
    }

    protected override bool IsGoalReached()
    {
        return _isOpen == _targetOpen;
    }

    protected override void ResetDevices()
    {
        // 수리 완료 후 문을 닫힌 상태로 초기화
        _doorController.TrySetState(false);

        _isOpen = false;
        _targetOpen = false;
    }

    public override void Initialize()
    {
        _isOpen = false;
        _targetOpen = false;

        // 문을 닫힌 상태로 초기화
        _doorController.TrySetState(false);
    }

    /// <summary>
    /// 나중에 입력 시스템과 연결할 함수.
    /// true  : 문 열기
    /// false : 문 닫기
    /// </summary>
    public void SetDoorState(bool open)
    {
        _doorController.TrySetState(open);
    }
}
