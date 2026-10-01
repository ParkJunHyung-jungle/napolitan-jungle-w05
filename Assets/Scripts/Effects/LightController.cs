using UnityEngine;

public enum lightState
{
    green,
    red,
    blink
}

public class LightController : MonoBehaviour
{
    [Header("Runtime Binding")]
    private int _facilityID;
    private bool _isBound;

    [Header("State")]
    public lightState _lightState;
    public float blinkInterval = 1f;

    [Header("Hardware")]
    public Renderer _renderer;
    public Light _light;

    [Header("Colors")]
    public Color red = new Color(239f / 255f, 62f / 255f, 62f / 255f);
    public Color green = new Color(52f / 255f, 191f / 255f, 25f / 255f);
    public Color gray = new Color(194f / 255f, 194f / 255f, 194f / 255f);

    void OnEnable()
    {
        _renderer = GetComponent<Renderer>();
        _light = GetComponent<Light>();

        if (_isBound)
        {
            RegisterLight();
        }
    }

    void OnDisable()
    {
        if (_isBound) Managers.Light.UnregisterFacilityLight(_facilityID, this);
    }

    /// <summary>
    /// FacilityManager가 전달한 facilityID를 이 씬 조명 어댑터에 연결한다.
    /// 활성 상태이면 기존 등록을 교체하고 Managers.Light에 현재 하드웨어를 등록한다.
    /// </summary>
    public void BindFacility(int facilityID)
    {
        if (_isBound)
        {
            Managers.Light.UnregisterFacilityLight(_facilityID, this);
        }

        _facilityID = facilityID;
        _isBound = true;

        if (isActiveAndEnabled)
        {
            RegisterLight();
        }
    }

    /// <summary>
    /// 기존 호출용 lightState를 저장하고 연결된 시설 ID의 전역 조명 API로 전달한다.
    /// 바인딩 전 호출이면 상태만 보관해 다음 등록 시 적용한다.
    /// </summary>
    public void SetLightState(lightState state)
    {
        _lightState = state;
        if (!_isBound) return;

        Managers.Light.SetFacilityLight(_facilityID, state);
    }

    /// <summary>
    /// 현재 시설 ID와 직렬화된 렌더러, Light, 색상 및 간격을 전역 manager에 등록한다.
    /// 시설 ID의 현재 하드웨어와 상태를 전역 매니저에 등록한다.
    /// </summary>
    private void RegisterLight()
    {
        Managers.Light.RegisterFacilityLight(_facilityID, this);
    }
}
