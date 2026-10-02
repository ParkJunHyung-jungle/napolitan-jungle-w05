using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class LightManager
{
    private const float EMERGENCY_LIGHT_INTENSITY = 10f;
    private const float FAILURE_FADE_DURATION = 3f;

    public static readonly Color GREEN = new Color(52f / 255f, 191f / 255f, 25f / 255f);
    public static readonly Color RED = new Color(239f / 255f, 62f / 255f, 62f / 255f);

    [Header("Lights")]
    private LightController _frontLight;
    private LightController _sideWallLight;
    private readonly List<LightController> _lights = new();
    public LightController FrontLight => _frontLight;
    public LightController SideWallLight => _sideWallLight;

    [Header("Emergency Light")]
    private Light _emergencyLight;
    private Coroutine _emergencyLightCoroutine;

    public GameObject LoadLightFront => Resources.Load<GameObject>("Prefabs/Lights/Light_Front");
    public GameObject LoadLightSideWall => Resources.Load<GameObject>("Prefabs/Lights/Light_SideWall");
    public GameObject LoadLightEmergency => Resources.Load<GameObject>("Prefabs/Lights/Light_Emergency");

    /// <summary>
    /// 조명 프리팹을 로드해 생성하고 역할별 필드에 저장한다.
    /// 일반 조명은 기본 색으로 켜서 _lights에 등록하고, 비상등은 광도 0으로 꺼 둔다.
    /// </summary>
    public void Init()
    {
        _frontLight = InstantiateLight(LoadLightFront);
        _sideWallLight = InstantiateLight(LoadLightSideWall);

        _emergencyLight = InstantiateUnderManagers(LoadLightEmergency).GetComponent<Light>();
        _emergencyLight.intensity = 0f;
    }

    /// <summary>
    /// 모든 조명을 기본 색으로 켜고 비상등을 끈다.
    /// _lights의 색과 켜짐 상태를 변경하고 비상등 펄스를 중지한다.
    /// </summary>
    public void Clear()
    {
        SetAllColor(GREEN);
        TurnOnAll();
        StopEmergencyLightPulse();
    }

    /// <summary>
    /// 등록된 모든 조명을 켠다.
    /// _lights의 각 LightController를 켜짐으로 변경한다.
    /// </summary>
    public void TurnOnAll()
    {
        foreach (LightController light in _lights)
        {
            light.TurnOn();
        }
    }

    /// <summary>
    /// 등록된 모든 조명을 끈다.
    /// _lights의 각 LightController를 꺼짐으로 변경한다.
    /// </summary>
    public void TurnOffAll()
    {
        foreach (LightController light in _lights)
        {
            light.TurnOff();
        }
    }

    /// <summary>
    /// 등록된 모든 조명을 깜빡이게 한다.
    /// _lights의 각 LightController를 깜빡임으로 변경한다.
    /// </summary>
    public void BlinkAll()
    {
        foreach (LightController light in _lights)
        {
            light.Blink();
        }
    }

    /// <summary>
    /// 등록된 모든 조명의 색을 color로 변경한다.
    /// _lights의 각 LightController 색을 변경한다.
    /// </summary>
    public void SetAllColor(Color color)
    {
        foreach (LightController light in _lights)
        {
            light.SetColor(color);
        }
    }

    /// <summary>
    /// 타이머 실패 시 비상등을 켜고 FAILURE_FADE_DURATION 동안 서서히 끈다.
    /// 이미 실행 중이면 중복 코루틴을 만들지 않고 유지한다.
    /// </summary>
    public void TriggerTimerFailure()
    {
        if (_emergencyLightCoroutine != null) return;

        _emergencyLightCoroutine = Managers.Instance.StartCoroutine(EmergencyLightCoroutine());
    }

    /// <summary>
    /// prefab을 생성해 기본 색으로 켜고 _lights에 등록한다.
    /// 생성된 LightController를 반환한다.
    /// </summary>
    private LightController InstantiateLight(GameObject prefab)
    {
        LightController light = InstantiateUnderManagers(prefab).GetComponent<LightController>();
        light.SetColor(GREEN);
        light.TurnOn();
        _lights.Add(light);

        return light;
    }

    /// <summary>
    /// prefab을 프리팹에 저장된 위치에 생성하고 Managers 하위에 둔다.
    /// 생성된 GameObject를 반환한다.
    /// </summary>
    private GameObject InstantiateUnderManagers(GameObject prefab)
    {
        GameObject instance = Object.Instantiate(prefab);
        instance.transform.SetParent(Managers.Instance.transform, true);

        return instance;
    }

    /// <summary>
    /// 실행 중인 비상등 펄스를 중지하고 비상등 광도를 0으로 만든다.
    /// _emergencyLightCoroutine을 null로 변경한다.
    /// </summary>
    private void StopEmergencyLightPulse()
    {
        if (_emergencyLightCoroutine != null)
        {
            Managers.Instance.StopCoroutine(_emergencyLightCoroutine);
            _emergencyLightCoroutine = null;
        }

        _emergencyLight.intensity = 0f;
    }

    /// <summary>
    /// 비상등 광도를 EMERGENCY_LIGHT_INTENSITY에서 FAILURE_FADE_DURATION 동안 0까지 줄인다.
    /// 종료 후 광도를 0으로, _emergencyLightCoroutine을 null로 되돌린다.
    /// </summary>
    private IEnumerator EmergencyLightCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < FAILURE_FADE_DURATION)
        {
            elapsed += Time.deltaTime;
            _emergencyLight.intensity = EMERGENCY_LIGHT_INTENSITY
                * (1f - Mathf.Clamp01(elapsed / FAILURE_FADE_DURATION));
            yield return null;
        }

        _emergencyLight.intensity = 0f;
        _emergencyLightCoroutine = null;
    }
}
