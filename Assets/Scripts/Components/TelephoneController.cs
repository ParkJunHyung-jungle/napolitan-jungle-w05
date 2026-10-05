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

    [Header("Child")]
    [SerializeField]
    private GameObject _receiver;
    [SerializeField]
    private GameObject _playerReceiver;

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
        Managers.Timeline.OnPhoneHangUp += ForceHangUp;
    }

    /// <summary>
    /// PhoneRing 이벤트를 받아 전화를 벨이 울리는 상태로 변경한다.
    /// 통화 중 재생을 정리하고 상태를 Ringing으로 설정한 뒤 벨소리와 OnCallStateChanged를 실행한다.
    /// </summary>
    public void ReceiveCall()
    {
        if (_waitAndPlayCoroutine != null)
        {
            StopCoroutine(_waitAndPlayCoroutine);
            _waitAndPlayCoroutine = null;
        }
        if (_callState == CallState.InCall)
            Managers.Sound.StopCallEndSound();

        if (_playerReceiver != null)
            _playerReceiver.SetActive(false);
        _receiver.SetActive(true);
        _callState = CallState.Ringing;
        Managers.Timeline.Report(DeviceAction.PhoneRinging);

        Managers.Sound.PhoneRinging();
        OnCallStateChanged?.Invoke();
    }

    /// <summary>
    /// 벨이 울리거나 통화 중인 전화를 종료한다.
    /// 벨소리와 통화 종료음을 멈추고 대기 상태로 변경해 타임라인에 보고한 뒤 끊는 소리와 OnCallStateChanged를 호출한다.
    /// </summary>
    public void HangUp()
    {
        _callState = CallState.Idle;
        if (_playerReceiver != null)
            _playerReceiver.SetActive(false);
        Managers.Timeline.Report(DeviceAction.PhoneIdle);
        if (_waitAndPlayCoroutine != null)
        {
            StopCoroutine(_waitAndPlayCoroutine);
            _waitAndPlayCoroutine = null;
        }
        Managers.Sound.StopCallEndSound();
        Managers.Sound.PhoneHangUp();
        OnCallStateChanged?.Invoke();
    }

    /// <summary>
    /// 타임라인의 벨 중지 요청을 받아 아무도 받지 않은 전화를 대기 상태로 되돌린다.
    /// 벨이 울리는 중일 때만 벨소리를 멈추고 타임라인에 대기 상태를 보고한 뒤 OnCallStateChanged를 호출한다.
    /// </summary>
    private void StopRinging()
    {
        if (_callState != CallState.Ringing)
            return;

        _callState = CallState.Idle;
        if (_playerReceiver != null)
            _playerReceiver.SetActive(false);
        Managers.Timeline.Report(DeviceAction.PhoneIdle);

        Managers.Sound.StopPhoneRinging();
        OnCallStateChanged?.Invoke();
    }

    /// <summary>
    /// 타임라인의 전화 종료 요청을 받아 현재 상태와 관계없이 전화를 대기 상태로 만든다.
    /// 울리는 중이면 벨을 멈추고, 통화 중이면 끊은 뒤 수화기를 제자리에 표시한다.
    /// </summary>
    private void ForceHangUp()
    {
        switch (_callState)
        {
            case CallState.Ringing:
                StopRinging();
                break;
            case CallState.InCall:
                HangUp();
                _receiver.SetActive(true);
                break;
        }
    }

    /// <summary>
    /// 플레이어 상호작용으로 울리는 전화를 받거나 통화 중인 전화를 끊는다.
    /// Ringing을 InCall로, InCall을 Idle로 변경하며 Idle 상호작용은 무시한다.
    /// </summary>
    public void Interact()
    {
        switch (_callState)
        {
            case CallState.Idle:
                Managers.Sound.PhonePickUp();
                break;
            case CallState.Ringing:
                _callState = CallState.InCall;
                if (_playerReceiver != null)
                    _playerReceiver.SetActive(true);
                _receiver.SetActive(false);
                Managers.Timeline.Report(DeviceAction.PhoneAnswered);
                Managers.Sound.StopPhoneRinging();
                Managers.Sound.PhonePickUp();
                _waitAndPlayCoroutine = StartCoroutine(WaitAndPlay());
                OnCallStateChanged?.Invoke();
                break;
            case CallState.InCall:
                HangUp();
                _receiver.SetActive(true);
                break;

        }
    }
    /// <summary>
    /// 수화기 픽업 사운드가 끝난 뒤 통화 음성을 재생하고 종료음을 이어서 재생한다.
    /// 두 AudioSource의 재생 상태와 현재 통화 상태를 확인하며 완료 시 대기 코루틴 참조를 해제한다.
    /// </summary>
    private IEnumerator WaitAndPlay()
    {
        yield return new WaitUntil(() => !GetComponent<AudioSource>().isPlaying);
        if (_callState != CallState.InCall)
        {
            _waitAndPlayCoroutine = null;
            yield break;
        }

        int voiceIndex = UnityEngine.Random.Range(0, 2);
        if (voiceIndex == 0)
            Managers.Sound.TalkingManVoice();
        else
            Managers.Sound.TalkingWomenVoice();

        yield return new WaitUntil(() => !Managers.Sound.OnThePhoneSource.isPlaying);

        if (_callState == CallState.InCall)
            Managers.Sound.CallEndSound();

        _waitAndPlayCoroutine = null;
    }
}
