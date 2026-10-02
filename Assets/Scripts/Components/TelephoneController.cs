using System;
using UnityEngine;

public class TelephoneController : MonoBehaviour, IInteractable
{
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

        Managers.Sound.PhoneRinging();
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

        Managers.Sound.PhoneHangUp();
        OnCallStateChanged?.Invoke();
    }

    /// <summary>
    /// 외부 상호작용으로 전화를 끊는다.
    /// </summary>
    public void Interact()
    {
        // 전화기를 누르면 전화가 걸리고, 걸려있으면 끊는다.
        if (_isCalling)
            HangUp();
        else
            ReceiveCall();

        // 아래가 해당 함수의 의도. 위에는 디버깅용
        //HangUp();
    }
}
