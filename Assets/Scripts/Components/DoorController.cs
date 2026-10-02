using System;
using System.Collections;

using UnityEngine;

public class DoorController : MonoBehaviour, IInteractable
{
    [Header("Door Movement")]
    [SerializeField] private float _duration = 1f;
    [SerializeField] private float _openAngle = 90f;
    [SerializeField]
    private AnimationCurve _moveCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Knock")]
    [SerializeField, Min(0.01f)] private float _knockInterval = 1f;

    private bool _isOpen = false;
    private bool _isMoving = false;
    private bool _isKnocking;
    private Coroutine _knockRoutine;

    [Header("Lock")]
    [SerializeField] private bool _isLocked = false;

    private float _elapsedTime;

    private Quaternion _closedRotation;
    private Quaternion _openRotation;

    public bool IsOpen => _isOpen;
    public bool IsMoving => _isMoving;
    public bool IsKnocking => _isKnocking;

    private IInteractable _interactableImplementation;
    public event Action OnDoorPressed;

    private void Awake()
    {
        _closedRotation = transform.localRotation;
        _openRotation =
            _closedRotation * Quaternion.Euler(0f, -_openAngle, 0f);
        // Manaers. += StartKnockEvent;
    }

    private void Update()
    {
        if (!_isMoving)
            return;

        _elapsedTime += Time.deltaTime;

        // 문이 천천히 열리도록
        float t = Mathf.Clamp01(_elapsedTime / _duration);
        float curveValue = _moveCurve.Evaluate(t);

        if (_isOpen)
        {
            transform.localRotation = Quaternion.Lerp(
                _closedRotation,
                _openRotation,
                curveValue
            );
        }
        else
        {
            transform.localRotation = Quaternion.Lerp(
                _openRotation,
                _closedRotation,
                curveValue
            );
        }

        if (t >= 1f)
        {
            _isMoving = false;
            _elapsedTime = 0f;
        }
    }


    /// <summary>
    /// 문 상호작용을 받아 노크를 멈추고 현재 열림 상태의 반대로 전환을 시도한다.
    /// 현재 _isOpen 상태를 사용하며 전환에 성공하면 OnDoorPressed를 호출한다.
    /// </summary>
    public void Interact()
    {
        if (_isLocked)
        {
            // Managers.Sound.PlaySound(SoundType.DoorLocked);
            return;
        }


        bool changed = TrySetState(!_isOpen);

        // 실제로 문 상태 변경에 성공했을 때만 이벤트 발행
        if (changed)
        {
            OnDoorPressed?.Invoke();
        }
    }

    /// <summary>
    /// 지정한 상태로 문을 연다/닫는다.
    /// 이미 이동 중이면 무시한다.
    /// </summary>
    public bool TrySetState(bool open)
    {
        if (_isMoving)
            return false;

        if (open == false)
        {
            //Managers.Sound.PlaySound(SoundType.DoorOpen);
        }
        else
        {
            //Managers.Sound.PlaySound(SoundType.DoorClose); 
        }

        StopKnock();

        _isOpen = open;
        _isMoving = true;
        _elapsedTime = 0f;

        return true;
    }


    /// <summary>
    /// 외부 노크 이벤트를 받아 일정 간격으로 노크 재생을 예약한다.
    /// _knockInterval을 사용하며 _isKnocking 상태와 반복 코루틴을 시작한다.
    /// </summary>
    public void StartKnockEvent()
    {
        if (_isKnocking)
            return;

        _isKnocking = true;
        _knockRoutine = StartCoroutine(KnockRoutine());
    }

    /// <summary>
    /// _isKnocking 상태 동안 즉시 한 번, 이후 _knockInterval마다 노크를 예약한다.
    /// 간격 대기를 반환하며 노크 재생 위치는 Manager 연결을 위해 주석으로 남긴다.
    /// </summary>
    private IEnumerator KnockRoutine()
    {
        while (_isKnocking)
        {
            //Manager: 노크 소리 1회 재생.
            yield return new WaitForSeconds(_knockInterval);
        }
    }

    /// <summary>
    /// 상호작용이나 비활성화를 받아 반복 노크 예약을 중지한다.
    /// _knockRoutine을 멈추고 _isKnocking 상태를 해제한다.
    /// </summary>
    private void StopKnock()
    {
        _isKnocking = false;
        if (_knockRoutine == null)
            return;

        StopCoroutine(_knockRoutine);
        _knockRoutine = null;
    }

}
