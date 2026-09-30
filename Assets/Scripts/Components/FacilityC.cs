using UnityEngine;

/// <summary>
/// 레버 설비. 고장 나면 레버를 당길 때마다 repairChance 확률로 회복된다.
/// 맞출 목표 상태가 없으므로 "목표 달성"은 당길 때마다 하는 확률 판정이다.
/// </summary>
public class FacilityC : Facility
{
    [SerializeField] private Lever lever;

    // [Header("레버 세팅")]
    // [Tooltip("한 번 당길 때 회복 확률 (0 ~ 1)")]
    // [SerializeField, Range(0f, 1f)] private float repairChance = 0.25f;

    private int requiredPullCount; // 수리에 필요한 횟수
    private int currentPullCount;  // 현재까지 내린 횟수

    public override void Initialize()
    {
        // lever.OnPulled += OnLeverPulled;
        // isFault = false;
        // 여러 번 초기화해도 이벤트가 중복 등록되지 않도록 처리
        lever.OnPulled -= OnLeverPulled;
        lever.OnPulled += OnLeverPulled;

        isFault = false;
        ResetDevices();
    }

    // 목표 상태가 없다
    protected override void GenerateGoal()
    {
        requiredPullCount = Random.Range(5, 8); // 5, 6, 7 중 하나
        currentPullCount = 0;
    }

    // 고장 중 당길 때만 불리므로, 부를 때마다 한 번 판정한다.
    // Random.value는 1.0도 나올 수 있으므로 확률 1은 따로 처리해 항상 성공하게 한다
    protected override bool IsGoalReached()
    {
        return requiredPullCount > 0 && currentPullCount >= requiredPullCount;
    }

    // 초기화할 장치 상태가 없다. 레버는 스프링이라 스스로 올라오고,
    // 강제로 되돌리면 내려가 있던 손잡이가 순간이동한다
    protected override void ResetDevices()
    {
        currentPullCount = 0;
        requiredPullCount = 0;
    }

    private void OnLeverPulled()
    {
        if (!isFault) return;

        currentPullCount++;
        OnDeviceChanged();
    }
}
