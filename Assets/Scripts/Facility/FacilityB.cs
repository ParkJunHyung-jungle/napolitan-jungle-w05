using UnityEngine;

public class FacilityB : Facility
{
    [SerializeField] FacilitySlider[] sliders;

    [Header("슬라이더 밸류 세팅")]
    [Tooltip("허용 범위 전체 폭. 0.1이면 목표 ±5%")]
    [SerializeField]
    private float tolerance = 0.05f;

    [Tooltip("목표를 만들 수 있는 값 범위. 레일 양 끝에 목표가 붙지 않게 한다")]
    [SerializeField, Range(0f, 1f)] private float targetMin = 0.05f;
    [SerializeField, Range(0f, 1f)] private float targetMax = 0.95f;

    [Header("Observation")]
    [SerializeField]private bool isFault;
    public override void Initialize()
    {
        foreach (var slider in sliders)
        {
            slider.Initialize();
            slider.OnValueChanged += OnSliderValueChanged;
        }
    }

    // 목표가 지정된 슬라이더 중 하나라도 범위 밖이면 고장. 목표가 없으면(정상 상태) 고장이 아니다
    public override bool IsFault()
    {
        foreach (var slider in sliders)
        {
            if (slider.HasTarget && !slider.IsInRange) return true;
        }
        return false;
    }

    public override void MakeFault()
    {
        foreach (var slider in sliders)
        {
            slider.SetTarget(MakeTarget(slider.Value), tolerance);
        }
    }

    public override void Clear()
    {
        foreach (var slider in sliders)
        {
            slider.ResetState();
        }
    }

    private void OnSliderValueChanged()
    {
        OnFacilityInteracted?.Invoke(IsFault(), facilityID);
        isFault = IsFault();
    }

    /// <summary>
    /// 현재 값에서 허용 범위 밖에 목표를 만든다 (「게임 진행 규칙」 7장).
    /// 경계에 걸려 고장 나자마자 수리되지 않도록 현재 값 ± tolerance(허용 범위 전체 폭) 구간을 비우고,
    /// [targetMin, targetMax]의 남은 구간에서 고르게 뽑는다.
    /// </summary>
    private float MakeTarget(float current)
    {
        float leftEnd = Mathf.Min(current - tolerance, targetMax);
        float rightStart = Mathf.Max(current + tolerance, targetMin);

        float leftLength = Mathf.Max(0f, leftEnd - targetMin);
        float rightLength = Mathf.Max(0f, targetMax - rightStart);
        float total = leftLength + rightLength;

        // 남은 구간이 없으면(허용 범위가 지나치게 넓은 경우) 현재 값에서 더 먼 끝을 쓴다
        if (total <= 0f)
            return current - targetMin > targetMax - current ? targetMin : targetMax;

        float pick = Random.Range(0f, total);
        return pick < leftLength ? targetMin + pick : rightStart + (pick - leftLength);
    }
}
