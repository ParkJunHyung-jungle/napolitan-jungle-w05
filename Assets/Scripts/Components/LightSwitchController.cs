using System;

using UnityEngine;

public class LightSwitchController : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject _redLight;
    [SerializeField] private GameObject _greenLight;
    [SerializeField] private Transform _leverPivot;
    private const float UP_ANGLE = 30f;
    private const float DOWN_ANGLE = -30f;

    private bool _isOn = true;
    // 조명의 상태를 나타내는 프로퍼티와 이벤트.
    public bool IsOn => _isOn;
    public event Action<bool> StateChanged;

    void Awake()
    {
        _isOn = true;
        ActiveLight();
        if (_leverPivot == null)
            _leverPivot = transform.Find("LeverPivot");
        // 레버가 왼쪽을 향하므로 Z축 회전 부호를 뒤집어 위쪽으로 세운다.
        _leverPivot.localRotation = Quaternion.Euler(0f, 0f, -UP_ANGLE);

        Managers.Sound.RegisterAudioSource(AudioSourceTypes.LIGHTSWITCH, gameObject.GetComponent<AudioSource>());
    }

    void OnEnable()
    {
        Managers.Date.OnDayEnd += HandleDayEnd;
        Managers.Timeline.OnLightToggle += HandleLightToggle;
    }

    void OnDisable()
    {
        Managers.Date.OnDayEnd -= HandleDayEnd;
        Managers.Timeline.OnLightToggle -= HandleLightToggle;
    }

    /// <summary>
    /// 플레이어의 상호작용을 받아 레버의 위/아래 상태를 전환한다.
    /// 현재 IsOn 상태를 반전하고 로컬 회전을 변경한 뒤 변경된 상태를 StateChanged와 타임라인에 전달한다.
    /// </summary>
    public void Interact()
    {
        _isOn = !_isOn;
        ActiveLight();

        if (_isOn == false)
        {
            Managers.Sound.LightOnSound();

            Managers.Light.RoomLightOff();

        }
        else
        {
            Managers.Sound.LightOffSound();
            Managers.Light.RoomLightOn();
            // 램프 루프 재생이 램프 소스를 먼저 정지하므로 플리커 소리는 조명을 켠 뒤 재생한다.
            Managers.Sound.LampFlickerSound();
        }
        float angle = _isOn ? UP_ANGLE : DOWN_ANGLE;

        _leverPivot.localRotation = Quaternion.Euler(0f, 0f, -angle);
        StateChanged?.Invoke(_isOn);
        Managers.Timeline.Report(_isOn ? DeviceAction.LightOn : DeviceAction.LightOff);

    }

    /// <summary>
    /// 하루 종료를 받아 켜져 있는 조명을 끈다.
    /// _isOn이 true일 때만 Interact로 레버, 실내 조명, 타임라인 상태를 꺼진 상태로 바꾼다.
    /// </summary>
    private void HandleDayEnd()
    {
        if (!_isOn)
            return;

        Interact();
    }

    /// <summary>
    /// 타임라인의 전등 토글 이벤트를 받아 전등 스위치를 현재 상태의 반대로 전환한다.
    /// Interact로 레버, 표시등, 실내 조명, 램프 루프, 타임라인 상태를 함께 바꾼다.
    /// </summary>
    private void HandleLightToggle()
    {
        Interact();
    }

    private void ActiveLight()
    {
        _redLight.SetActive(_isOn);
        _greenLight.SetActive(!_isOn);
    }
}
