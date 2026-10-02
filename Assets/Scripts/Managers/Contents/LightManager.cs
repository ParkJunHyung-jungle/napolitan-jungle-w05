using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

public class LightManager
{
    private const float REFLECTION_INTENSITY = 0f;
    private const float EMERGENCY_INTENSITY = 50f;
    private const float EMERGENCY_PUNCH_DURATION = 3f;
    private static readonly Color DEFAULT_COLOR = new Color32(0xFF, 0xF7, 0xF7, 0xFF);
    private static readonly Color RED_COLOR = new Color32(0xEF, 0x3E, 0x3E, 0xFF);

    [Header("Environment")]
    private Material _nightSky;

    [Header("Lights")]
    private LightController _roomLight;
    private LightController _stairLight;
    private LightController _ambientLight;
    private LightController _emergencyLight;

    private Coroutine _emergencyPunchCoroutine;

    public Material LoadNightSky => Resources.Load<Material>("Materials/Night_Sky");
    public GameObject LoadAmbientLight => Resources.Load<GameObject>("Prefabs/Lights/AmbientLight");
    public GameObject LoadRoomLight => Resources.Load<GameObject>("Prefabs/Lights/RoomLight");
    public GameObject LoadStairLight => Resources.Load<GameObject>("Prefabs/Lights/StairLight");
    public GameObject LoadEmergencyLight => Resources.Load<GameObject>("Prefabs/Lights/EmergencyLight");

    public void Init()
    {
        _nightSky = LoadNightSky;
        ApplyEnvironment();

        _ambientLight = InstantiateLight(LoadAmbientLight).GetComponent<LightController>();
        _roomLight = InstantiateLight(LoadRoomLight).GetComponent<LightController>();
        _stairLight = InstantiateLight(LoadStairLight).GetComponent<LightController>();
        _emergencyLight = InstantiateLight(LoadEmergencyLight).GetComponent<LightController>();

        RoomLightTintDefault();
        RoomLightOn();
    }

    public void Clear()
    {
        StopEffectCoroutine(ref _emergencyPunchCoroutine);
        _emergencyLight.SetIntensity(0f);

        RoomLightTintDefault();
        RoomLightOn();
    }
    public void RoomLightOn()
    {
        _roomLight.TurnOn();
    }

    public void RoomLightOff()
    {
        _roomLight.TurnOff();
    }

    public void RoomLightBlink()
    {
        _roomLight.Blink();
    }

    public void RoomLightTintDefault()
    {
        _roomLight.SetColor(DEFAULT_COLOR);
    }

    public void RoomLightTintRed()
    {
        _roomLight.SetColor(RED_COLOR);
    }

    private void ApplyEnvironment()
    {
        RenderSettings.skybox = _nightSky;
        RenderSettings.reflectionIntensity = REFLECTION_INTENSITY;
        DynamicGI.UpdateEnvironment();
    }

    private GameObject InstantiateLight(GameObject prefab)
    {
        GameObject instance = Object.Instantiate(prefab);
        instance.transform.SetParent(Managers.Instance.transform, true);

        return instance;
    }

    public void TriggerTimerFailure()
    {
        if (_emergencyPunchCoroutine != null) return;

        PunchEmergencyLight(EMERGENCY_INTENSITY, EMERGENCY_PUNCH_DURATION);
    }

    public void PunchEmergencyLight(float peak, float duration)
    {
        StopEffectCoroutine(ref _emergencyPunchCoroutine);
        _emergencyPunchCoroutine = Managers.Instance.StartCoroutine(EmergencyPunchCoroutine(peak, duration));
    }

    private void StopEffectCoroutine(ref Coroutine coroutine)
    {
        if (coroutine == null) return;

        Managers.Instance.StopCoroutine(coroutine);
        coroutine = null;
    }

    private IEnumerator EmergencyPunchCoroutine(float peak, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _emergencyLight.SetIntensity(peak * (1f - Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }

        _emergencyLight.SetIntensity(0f);
        _emergencyPunchCoroutine = null;
    }
}
