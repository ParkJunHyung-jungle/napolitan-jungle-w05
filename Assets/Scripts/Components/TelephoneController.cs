using System;

using UnityEngine;

public class TelephoneController : MonoBehaviour, IInteractable
{
    private enum CallState
    {
        Idle,
        Ringing,
        InCall
    }

    private CallState _callState;

    public bool IsCalling => _callState != CallState.Idle;
    public bool IsInCall => _callState == CallState.InCall;

    public event Action OnCallStateChanged;


    private void Awake()
    {
        Managers.Sound.RegisterAudioSource(AudioSourceTypes.TELEPHONE, gameObject.GetComponent<AudioSource>());
    }

    /// <summary>
    /// 외부 수신 요청을 받아 대기 중인 전화를 벨이 울리는 상태로 변경한다.
    /// 대기 상태에서만 벨소리를 시작하고 OnCallStateChanged를 호출한다.
    /// </summary>
    public void ReceiveCall()
    {
        if (_callState != CallState.Idle)
            return;

        _callState = CallState.Ringing;

        Managers.Sound.PhoneRinging();
        OnCallStateChanged?.Invoke();
    }

    /// <summary>
    /// 벨이 울리거나 통화 중인 전화를 종료한다.
    /// 대기 상태로 변경하고 끊는 소리와 OnCallStateChanged를 호출한다.
    /// </summary>
    public void HangUp()
    {
        if (_callState == CallState.Idle)
            return;

        _callState = CallState.Idle;

        Managers.Sound.PhoneOffSound();
        Managers.Sound.PhoneHangUp();
        OnCallStateChanged?.Invoke();
    }

    /// <summary>
    /// 플레이어 상호작용으로 울리는 전화를 받거나 통화 중인 전화를 끊는다.
    /// 현재 통화 상태에 따라 상태를 변경하며 대기 중에는 아무 동작도 하지 않는다.
    /// </summary>
    public void Interact()
    {
        // 디버깅용 상호작용으로 전화 걸기
        //if (_callState == CallState.Idle)
        //{
        //    ReceiveCall();
        //    return;
        //}

        if (_callState == CallState.Ringing)
        {
            _callState = CallState.InCall;
            Managers.Sound.PhonePickUp();


            int i = UnityEngine.Random.Range(0, 1);
            if (i == 0)
                Managers.Sound.TalkingManVoice();
            else
                Managers.Sound.TalkingWomenVoice();

            OnCallStateChanged?.Invoke();
        }
        else if (_callState == CallState.InCall)
            HangUp();
    }
}
