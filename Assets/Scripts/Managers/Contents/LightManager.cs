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
    private LightController _faxLight;
    private LightController _stairLight;
    private LightController _ambientLight;
    private LightController _emergencyLight;

    private LightController _stairLightFakeUp;
    private LightController _stairLightFakeDown;
    private LightController _roomLightFakeUp;
    private LightController _roomLightFakeDown;
    private LightController _ambientLightFakeUp;
    private LightController _ambientLightFakeDown;

    private Coroutine _emergencyPunchCoroutine;

    public Material LoadNightSky => Resources.Load<Material>("Materials/Night_Sky");
    public Material LoadMorningSky => Resources.Load<Material>("Materials/Morning_Sky");
    public GameObject LoadAmbientLight => Resources.Load<GameObject>("Prefabs/Lights/AmbientLight");
    public GameObject LoadRoomLight => Resources.Load<GameObject>("Prefabs/Lights/RoomLight");
    public GameObject LoadBoardLight => Resources.Load<GameObject>("Prefabs/Lights/BoardLight");
    public GameObject LoadFaxLight => Resources.Load<GameObject>("Prefabs/Lights/FaxLight");
    public GameObject LoadStairLight => Resources.Load<GameObject>("Prefabs/Lights/StairLight");
    public GameObject LoadEmergencyLight => Resources.Load<GameObject>("Prefabs/Lights/EmergencyLight");
    public LightController EmergencyLight => _emergencyLight;
    public GameObject LoadStairLightFakeUp => Resources.Load<GameObject>("Prefabs/Lights/StairLightFakeUp");
    public GameObject LoadStairLightFakeDown => Resources.Load<GameObject>("Prefabs/Lights/StairLightFakeDown");
    public GameObject LoadRoomLightFakeUp => Resources.Load<GameObject>("Prefabs/Lights/RoomLightFakeUp");
    public GameObject LoadRoomLightFakeDown => Resources.Load<GameObject>("Prefabs/Lights/RoomLightFakeDown");
    public GameObject LoadAmbientLightFakeUp => Resources.Load<GameObject>("Prefabs/Lights/AmbientLightFakeUp");
    public GameObject LoadAmbientLightFakeDown => Resources.Load<GameObject>("Prefabs/Lights/AmbientLightFakeDown");


    /// <summary>
    /// 하늘과 조명 프리팹을 초기화하고 방 조명을 켠다.
    /// _roomLight의 깜빡임 상태 변경과 하루 종료 이벤트를 구독한다.
    /// </summary>
    public void Init()
    {
        _nightSky = LoadNightSky;
        _morningSky = LoadMorningSky;
        ApplyEnvironment();
        ApplyNightSky();

        _ambientLight = InstantiateLight(LoadAmbientLight).GetComponent<LightController>();
        _roomLight = InstantiateLight(LoadRoomLight).GetComponent<LightController>();
        _boardLight = InstantiateLight(LoadBoardLight).GetComponent<LightController>();
        _faxLight = InstantiateLight(LoadFaxLight).GetComponent<LightController>();
        _stairLight = InstantiateLight(LoadStairLight).GetComponent<LightController>();
        _emergencyLight = InstantiateLight(LoadEmergencyLight).GetComponent<LightController>();

        _stairLightFakeUp = InstantiateLight(LoadStairLightFakeUp).GetComponent<LightController>();
        _stairLightFakeDown = InstantiateLight(LoadStairLightFakeDown).GetComponent<LightController>();
        _roomLightFakeUp = InstantiateLight(LoadRoomLightFakeUp).GetComponent<LightController>();
        _roomLightFakeDown = InstantiateLight(LoadRoomLightFakeDown).GetComponent<LightController>();
        _ambientLightFakeUp = InstantiateLight(LoadAmbientLightFakeUp).GetComponent<LightController>();
        _ambientLightFakeDown = InstantiateLight(LoadAmbientLightFakeDown).GetComponent<LightController>();

        _roomLight.OnBlinkToggled += HandleRoomLightBlinkToggled;

        RoomLightTintDefault();
        RoomLightOn();

        Managers.Date.OnDayEnd += ApplyMorningSky;
    }

    /// <summary>
    /// 재시작을 위해 비상 조명 연출을 멈추고 방 조명, 게시판 조명, 팩스 조명을 기본 색으로 켠다.
    /// _emergencyLight 세기를 0으로, _roomLight 색을 DEFAULT_COLOR로 바꾼다.
    /// </summary>
    public void Clear()
    {
        StopEffectCoroutine(ref _emergencyPunchCoroutine);
        _emergencyLight.SetIntensity(0f);

        RoomLightTintDefault();
        // Managers.Clear에서 Sound가 먼저 정리되어 램프 소스가 없으므로 사운드 없이 조명만 켠다.
        _roomLight.TurnOn();
        _boardLight.TurnOn();
        _faxLight.TurnOn();
    }
    /// <summary>
    /// 방 조명, 게시판 조명, 팩스 조명을 함께 켜고 램프 루프 사운드를 재생한다.
    /// _roomLight, _boardLight, _faxLight를 켜진 상태로 바꾼다.
    /// </summary>
    public void RoomLightOn()
    {
        _roomLight.TurnOn();
        _boardLight.TurnOn();
        _faxLight.TurnOn();
        Managers.Sound.LampAmbientSound();
    }

    /// <summary>
    /// 방 조명, 게시판 조명, 팩스 조명을 함께 끄고 램프 루프 사운드를 정지한다.
    /// _roomLight, _boardLight, _faxLight를 꺼진 상태로 바꾼다.
    /// </summary>
    public void RoomLightOff()
    {
        _roomLight.TurnOff();
        _boardLight.TurnOff();
        _faxLight.TurnOff();
        Managers.Sound.StopLampAmbientSound();
    }

    /// <summary>
    /// 방 조명, 게시판 조명, 팩스 조명을 같은 시점에 깜빡이게 한다.
    /// 세 조명은 같은 블링크 커브를 쓰므로 함께 깜빡인다.
    /// </summary>
    public void RoomLightBlink()
    {
        _roomLight.Blink();
        _boardLight.Blink();
        _faxLight.Blink();
    }

    /// <summary>
    /// 방 조명이 깜빡이며 켜짐 상태가 바뀔 때 램프 루프 사운드를 맞춘다.
    /// isOn이 true면 램프 루프를 재생하고, false면 정지한다.
    /// </summary>
    private void HandleRoomLightBlinkToggled(bool isOn)
    {
        if (isOn)
            Managers.Sound.LampAmbientSound();
        else
            Managers.Sound.StopLampAmbientSound();
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
