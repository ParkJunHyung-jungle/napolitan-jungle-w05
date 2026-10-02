using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

public class LightManager
{
    private const float REFLECTION_INTENSITY = 0f;
    private static readonly Color DEFAULT_COLOR = new Color32(0xFF, 0xF7, 0xF7, 0xFF);
    private static readonly Color RED_COLOR = new Color32(0xEF, 0x3E, 0x3E, 0xFF);

    [Header("Environment")]
    private Material _nightSky;

    [Header("Lights")]
    private LightController _roomLight;

    public Material LoadNightSky => Resources.Load<Material>("Materials/Night_Sky");
    public GameObject LoadRoomLight => Resources.Load<GameObject>("Prefabs/Lights/RoomLight");

    public void Init()
    {
        _nightSky = LoadNightSky;
        ApplyEnvironment();

        _roomLight = InstantiateRoomLight(LoadRoomLight).GetComponent<LightController>();
        RoomLightTintDefault();
        RoomLightOn();
    }

    public void Clear()
    {
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

    private GameObject InstantiateRoomLight(GameObject prefab)
    {
        GameObject instance = Object.Instantiate(prefab);
        instance.transform.SetParent(Managers.Instance.transform, true);

        return instance;
    }
}
