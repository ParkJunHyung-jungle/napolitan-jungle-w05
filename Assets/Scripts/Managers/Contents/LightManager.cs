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
    private Material _morningSky;

    [Header("Lights")]
    private LightController _roomLight;
    private LightController _boardLight;
    private LightController _stairLight;
    private LightController _ambientLight;
    private LightController _emergencyLight;

    private Coroutine _emergencyPunchCoroutine;

    public Material LoadNightSky => Resources.Load<Material>("Materials/Night_Sky");
    public Material LoadMorningSky => Resources.Load<Material>("Materials/Morning_Sky");
    public GameObject LoadAmbientLight => Resources.Load<GameObject>("Prefabs/Lights/AmbientLight");
    public GameObject LoadRoomLight => Resources.Load<GameObject>("Prefabs/Lights/RoomLight");
    public GameObject LoadBoardLight => Resources.Load<GameObject>("Prefabs/Lights/BoardLight");
    public GameObject LoadStairLight => Resources.Load<GameObject>("Prefabs/Lights/StairLight");
    public GameObject LoadEmergencyLight => Resources.Load<GameObject>("Prefabs/Lights/EmergencyLight");

    public void Init()
    {
        _nightSky = LoadNightSky;
        _morningSky = LoadMorningSky;
        ApplyEnvironment();
        ApplyNightSky();

        _ambientLight = InstantiateLight(LoadAmbientLight).GetComponent<LightController>();
        _roomLight = InstantiateLight(LoadRoomLight).GetComponent<LightController>();
        _boardLight = InstantiateLight(LoadBoardLight).GetComponent<LightController>();
        _stairLight = InstantiateLight(LoadStairLight).GetComponent<LightController>();
        _emergencyLight = InstantiateLight(LoadEmergencyLight).GetComponent<LightController>();

        RoomLightTintDefault();
        RoomLightOn();

        Managers.Date.OnDayEnd += ApplyMorningSky;
    }

    public void Clear()
    {
        StopEffectCoroutine(ref _emergencyPunchCoroutine);
        _emergencyLight.SetIntensity(0f);

        RoomLightTintDefault();
        RoomLightOn();
    }
    /// <summary>
    /// 방 조명과 게시판 조명을 함께 켠다.
    /// _roomLight와 _boardLight를 켜진 상태로 바꾼다.
    /// </summary>
    public void RoomLightOn()
    {
        _roomLight.TurnOn();
        _boardLight.TurnOn();
    }

    /// <summary>
    /// 방 조명과 게시판 조명을 함께 끈다.
    /// _roomLight와 _boardLight를 꺼진 상태로 바꾼다.
    /// </summary>
    public void RoomLightOff()
    {
        _roomLight.TurnOff();
        _boardLight.TurnOff();
    }

    /// <summary>
    /// 방 조명과 게시판 조명을 같은 시점에 깜빡이게 한다.
    /// 두 조명은 같은 블링크 커브를 쓰므로 함께 깜빡인다.
    /// </summary>
    public void RoomLightBlink()
    {
        _roomLight.Blink();
        _boardLight.Blink();
    }

    public void RoomLightTintDefault()
    {
        _roomLight.SetColor(DEFAULT_COLOR);
    }

    public void RoomLightTintRed()
    {
        _roomLight.SetColor(RED_COLOR);
    }

    /// <summary>
    /// 하늘과 무관한 공통 환경 설정을 적용한다.
    /// RenderSettings.reflectionIntensity를 REFLECTION_INTENSITY로 변경한다.
    /// </summary>
    private void ApplyEnvironment()
    {
        RenderSettings.reflectionIntensity = REFLECTION_INTENSITY;
    }

    /// <summary>
    /// 스카이박스를 밤 하늘로 바꾸고 환경광을 다시 계산한다.
    /// _nightSky를 RenderSettings.skybox에 지정한다.
    /// </summary>
    private void ApplyNightSky()
    {
        RenderSettings.skybox = _nightSky;
        DynamicGI.UpdateEnvironment();
    }

    /// <summary>
    /// 하루 종료를 받아 스카이박스를 아침 하늘로 바꾸고 환경광을 다시 계산한다.
    /// _morningSky를 RenderSettings.skybox에 지정한다.
    /// </summary>
    private void ApplyMorningSky()
    {
        RenderSettings.skybox = _morningSky;
        DynamicGI.UpdateEnvironment();
    }

    private GameObject InstantiateLight(GameObject prefab)
    {
        GameObject instance = Object.Instantiate(prefab);
        instance.transform.SetParent(Managers.Instance.transform, true);

        return instance;
    }

    public void PunchEmergencyLight()
    {
        StopEffectCoroutine(ref _emergencyPunchCoroutine);
        _emergencyPunchCoroutine = Managers.Instance.StartCoroutine(EmergencyPunchCoroutine(EMERGENCY_INTENSITY, EMERGENCY_PUNCH_DURATION));
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
