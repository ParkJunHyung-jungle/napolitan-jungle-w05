using System;
using UnityEngine;

public class TelephoneController : MonoBehaviour, IInteractable
{
    [Header("Telephone")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _ringingClip;

    private bool _isCalling;

    public bool IsCalling => _isCalling;

    public event Action OnCallStateChanged;

    /// <summary>
    /// 전화가 걸려온 상태로 변경한다.
    /// </summary>
    public void ReceiveCall()
    {
        if (_isCalling)
            return;

        _isCalling = true;

        if (_audioSource != null && _ringingClip != null)
        {
            _audioSource.clip = _ringingClip;
            _audioSource.loop = true;
            _audioSource.Play();
        }

        OnCallStateChanged?.Invoke();
    }

    /// <summary>
    /// 전화를 끊는다.
    /// </summary>
    public void HangUp()
    {
        if (!_isCalling)
            return;

        _isCalling = false;

        if (_audioSource != null)
        {
            _audioSource.Stop();
        }

        OnCallStateChanged?.Invoke();
    }

    /// <summary>
    /// 외부 상호작용으로 전화를 끊는다.
    /// </summary>
    public void Interact()
    {
        if (_isCalling)
            HangUp();
        else
            ReceiveCall();

        //HangUp(); 위에는 디버깅 용으로 만들더 둔 상태. 이후 이 코드만 냅두고 지우기
    }
}
