using System;
using System.Collections;

using UnityEngine;

public class TelephoneController : MonoBehaviour, IInteractable
{
    private enum CallState
    {
        Idle,
        Ringing,
        InCall
    }

    [Header("통화 상태")]
    private CallState _callState;
    private Coroutine _waitAndPlayCoroutine;

    public bool IsCalling => _callState != CallState.Idle;
    public bool IsInCall => _callState == CallState.InCall;

    public event Action OnCallStateChanged;


    private void Awake()
    {
        Managers.Sound.RegisterAudioSource(AudioSourceTypes.TELEPHONE, gameObject.GetComponent<AudioSource>());
        Managers.Timeline.OnPhoneRing += ReceiveCall;
        Managers.Timeline.OnPhoneRingStop += StopRinging;
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
        if (_waitAndPlayCoroutine != null)
        {
            StopCoroutine(_waitAndPlayCoroutine);
            _waitAndPlayCoroutine = null;
        }
        Managers.Sound.StopCallEndSound();
        //Managers.Sound.CallEndSound();
        Managers.Sound.PhoneHangUp();
        OnCallStateChanged?.Invoke();
    }

    /// <summary>
    /// 타임라인의 벨 중지 요청을 받아 아무도 받지 않은 전화를 대기 상태로 되돌린다.
    /// 벨이 울리는 중일 때만 벨소리를 멈추고 OnCallStateChanged를 호출한다.
    /// </summary>
    private void StopRinging()
    {
        if (_callState != CallState.Ringing)
            return;

        _callState = CallState.Idle;

        // SoundManager에 벨 정지 기능이 없어 전화기에 등록된 오디오 소스를 직접 멈춘다.
        GetComponent<AudioSource>().Stop();
        OnCallStateChanged?.Invoke();
    }

    /// <summary>
    /// 플레이어 상호작용으로 울리는 전화를 받거나 통화 중인 전화를 끊는다.
    /// 현재 통화 상태에 따라 상태를 변경하며 대기 중에는 아무 동작도 하지 않는다.
    /// </summary>
    public void Interact()
    {
        // 디버깅용 상호작용으로 전화 걸기
        if (_callState == CallState.Idle)
        {
            ReceiveCall();
            return;
        }

        if (_callState == CallState.Ringing)
        {
            _callState = CallState.InCall;
            Managers.Timeline.Report(DeviceAction.PhoneAnswered);
            Managers.Sound.PhonePickUp();


            int i = UnityEngine.Random.Range(0, 2);
            if (i == 0)
                Managers.Sound.TalkingManVoice();
            else
                Managers.Sound.TalkingWomenVoice();
            _waitAndPlayCoroutine = StartCoroutine(WaitAndPlay());
            OnCallStateChanged?.Invoke();
        }
        else if (_callState == CallState.InCall)
            HangUp();
    }
    /// <summary>
    /// 전화 음성 소스의 재생이 끝날 때까지 기다린 후 통화 종료음을 재생한다.
    /// OnThePhoneSource의 재생 상태를 사용하며 완료 시 대기 코루틴 참조를 해제한다.
    /// </summary>
    private IEnumerator WaitAndPlay()
    {
        // 현재 클립의 재생이 끝날 때까지 대기
        yield return new WaitUntil(() => !Managers.Sound.OnThePhoneSource.isPlaying);

        // 같은 AudioSource에 다음 클립을 넣고 재생
        if (_callState == CallState.InCall)
            Managers.Sound.CallEndSound();

        _waitAndPlayCoroutine = null;
    }
}
