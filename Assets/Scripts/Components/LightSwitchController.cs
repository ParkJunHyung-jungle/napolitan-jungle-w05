using System;

using UnityEngine;

public class LightSwitchController : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject _redLight;
    [SerializeField] private GameObject _greenLight;

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
        // 레버가 왼쪽을 향하므로 Z축 회전 부호를 뒤집어 위쪽으로 세운다.
        transform.localRotation = Quaternion.Euler(0f, 0f, -UP_ANGLE);

        Managers.Sound.RegisterAudioSource(AudioSourceTypes.LIGHTSWITCH, gameObject.GetComponent<AudioSource>());
    }

    /// <summary>
    /// 플레이어의 상호작용을 받아 레버의 위/아래 상태를 전환한다.
    /// 현재 IsOn 상태를 반전하고 로컬 회전을 변경한 뒤 변경된 상태를 StateChanged에 전달한다.
    /// </summary>
    public void Interact()
    {
        _isOn = !_isOn;
        ActiveLight();

        if (_isOn == false)
        {
            Managers.Sound.LightOnSound();
        }
        else
        {
            Managers.Sound.LightOffSound();
        }
        float angle = _isOn ? UP_ANGLE : DOWN_ANGLE;

        transform.localRotation = Quaternion.Euler(0f, 0f, -angle);
        StateChanged?.Invoke(_isOn);

    }

    private void ActiveLight()
    {
        _redLight.SetActive(_isOn);
        _greenLight.SetActive(!_isOn);
    }
}
