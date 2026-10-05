using System;
using System.Collections;

using UnityEngine;

public class TelephoneController : MonoBehaviour, IInteractable
{
    private enum CallState
    {
        Idle,
        Ringing,
        InCall,
        CallEnd
    }

    [Header("Child")]
    [SerializeField]
    private GameObject _receiver;
    [SerializeField]
    private GameObject _playerReceiver;

    [Header("통화 상태")]
    private CallState _callState;
    private Coroutine _waitAndPlayCoroutine;
    private bool _isReceiverHeld;

    public bool IsCalling => _callState != CallState.Idle;
    public bool IsInCall => _callState == CallState.InCall;

    public event Action OnCallStateChanged;


    private void Awake()
    {
        Managers.Sound.RegisterAudioSource(AudioSourceTypes.TELEPHONE, gameObject.GetComponent<AudioSource>());
        Managers.Timeline.OnPhoneRing += ReceiveCall;
        Managers.Timeline.OnPhoneRingStop += StopRinging;
        Managers.Timeline.OnPhoneHangUp += ForceHangUp;
    }

    /// <summary>
    /// PhoneRing 이벤트를 받아 전화를 벨이 울리는 상태로 변경한다.
    /// 통화 중 재생을 정리하고 상태를 Ringing으로 설정한 뒤 벨소리와 OnCallStateChanged를 실행한다.
    /// 수화기를 들고 있으면 손에 든 채로 벨만 울린다.
    /// </summary>
    public void ReceiveCall()
    {
        if (_waitAndPlayCoroutine != null)
        {
            StopCoroutine(_waitAndPlayCoroutine);
            _waitAndPlayCoroutine = null;
        }
        if (_callState == CallState.InCall || _callState == CallState.CallEnd)
            Managers.Sound.StopCallEndSound();

        _callState = CallState.Ringing;
        Managers.Timeline.Report(DeviceAction.PhoneRinging);

        Managers.Sound.PhoneRinging();
        OnCallStateChanged?.Invoke();
    }

    /// <summary>
    /// 통화 중이거나 끊긴 수신음 상태인 전화를 종료하고 수화기를 전화기 위에 내려놓는다.
    /// 통화 음성과 끊긴 수신음을 멈추고 끊는 소리를 재생한 뒤 대기 상태를 타임라인에 보고하고 OnCallStateChanged를 호출한다.
    /// </summary>
    public void HangUp()
    {
        _callState = CallState.Idle;
        SetReceiverHeld(false);
        if (_waitAndPlayCoroutine != null)
        {
            StopCoroutine(_waitAndPlayCoroutine);
            _waitAndPlayCoroutine = null;
        }
        Managers.Sound.StopCallEndSound();
        Managers.Sound.PhoneHangUp();
        // 일찍 끊은 펀치로 게임오버가 되면 소리를 모두 멈추므로 끊는 소리보다 뒤에 보고한다.
        Managers.Timeline.Report(DeviceAction.PhoneIdle);
        OnCallStateChanged?.Invoke();
    }

    /// <summary>
    /// 타임라인의 벨 중지 요청을 받아 아무도 받지 않은 전화의 벨을 멈춘다.
    /// 수화기를 들고 있으면 CallEnd로 돌아가 끊긴 수신음을 재생하고, 아니면 Idle로 변경한다.
    /// 변경한 상태를 타임라인에 보고한 뒤 OnCallStateChanged를 호출한다.
    /// </summary>
    private void StopRinging()
    {
        if (_callState != CallState.Ringing)
            return;

        Managers.Sound.StopPhoneRinging();
        if (_isReceiverHeld)
        {
            _callState = CallState.CallEnd;
            Managers.Timeline.Report(DeviceAction.PhoneCallEnded);
            Managers.Sound.CallEndSound();
        }
        else
        {
            _callState = CallState.Idle;
            Managers.Timeline.Report(DeviceAction.PhoneIdle);
        }
        OnCallStateChanged?.Invoke();
    }

    /// <summary>
    /// 타임라인의 전화 종료 요청을 받아 현재 상태와 관계없이 전화를 대기 상태로 만든다.
    /// 울리는 중이면 벨을 멈추고, 수화기를 들고 있으면 끊어서 제자리에 내려놓는다.
    /// </summary>
    private void ForceHangUp()
    {
        if (_callState == CallState.Ringing)
            StopRinging();
        // 수화기를 든 채 울리던 전화는 벨이 멈춘 뒤 CallEnd가 되므로 이어서 끊는다.
        if (_callState == CallState.InCall || _callState == CallState.CallEnd)
            HangUp();
    }

    /// <summary>
    /// 플레이어 상호작용으로 수화기를 들거나, 울리는 전화를 받거나, 통화를 끊는다.
    /// Idle은 CallEnd로, Ringing은 InCall로, InCall과 CallEnd는 Idle로 변경한다.
    /// </summary>
    public void Interact()
    {
        switch (_callState)
        {
            case CallState.Idle:
                _callState = CallState.CallEnd;
                SetReceiverHeld(true);
                Managers.Timeline.Report(DeviceAction.PhoneCallEnded);
                Managers.Sound.PhonePickUp();
                _waitAndPlayCoroutine = StartCoroutine(WaitAndPlay());
                OnCallStateChanged?.Invoke();
                break;
            case CallState.Ringing:
                _callState = CallState.InCall;
                SetReceiverHeld(true);
                Managers.Timeline.Report(DeviceAction.PhoneAnswered);
                Managers.Sound.StopPhoneRinging();
                Managers.Sound.PhonePickUp();
                _waitAndPlayCoroutine = StartCoroutine(WaitAndPlay());
                OnCallStateChanged?.Invoke();
                break;
            case CallState.InCall:
            case CallState.CallEnd:
                HangUp();
                break;

        }
    }
    /// <summary>
    /// 수화기 픽업 사운드가 끝난 뒤 통화 중이면 통화 음성을 재생하고 CallEnd로 변경해 타임라인에 보고한다.
    /// 이어서 끊긴 수신음을 반복 재생하며, 전화 없이 수화기를 든 경우에는 음성 없이 끊긴 수신음만 재생한다.
    /// 완료 시 대기 코루틴 참조를 해제한다.
    /// </summary>
    private IEnumerator WaitAndPlay()
    {
        yield return new WaitUntil(() => !GetComponent<AudioSource>().isPlaying);

        // 상태가 바뀌는 경로는 모두 이 코루틴을 멈추므로 여기서는 InCall 또는 CallEnd만 남는다.
        if (_callState == CallState.InCall)
        {
            int voiceIndex = UnityEngine.Random.Range(0, 2);
            if (voiceIndex == 0)
                Managers.Sound.TalkingManVoice();
            else
                Managers.Sound.TalkingWomenVoice();

            yield return new WaitUntil(() => !Managers.Sound.OnThePhoneSource.isPlaying);

            _callState = CallState.CallEnd;
            Managers.Timeline.Report(DeviceAction.PhoneCallEnded);
            OnCallStateChanged?.Invoke();
        }

        Managers.Sound.CallEndSound();
        _waitAndPlayCoroutine = null;
    }

    /// <summary>
    /// 수화기를 플레이어 손 또는 전화기 위에 표시한다.
    /// isHeld를 _isReceiverHeld에 저장하고 _playerReceiver와 _receiver의 활성 상태를 서로 반대로 설정한다.
    /// </summary>
    private void SetReceiverHeld(bool isHeld)
    {
        _isReceiverHeld = isHeld;
        if (_playerReceiver != null)
            _playerReceiver.SetActive(isHeld);
        _receiver.SetActive(!isHeld);
    }
}
