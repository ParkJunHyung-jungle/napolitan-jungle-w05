using System;

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
    [SerializeField] private AudioSource _knockSource;
    [SerializeField] private AudioClip _knockClip;

    private bool _isOpen = false;
    private bool _isMoving = false;
    private bool _isKnocking;

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
            _closedRotation * Quaternion.Euler(0f, _openAngle, 0f);
    }

    private void OnDisable()
    {
        StopKnock();
    }

    private void Update()
    {
        if (!_isMoving)
            return;

        _elapsedTime += Time.deltaTime;

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
    /// 지정한 상태로 문을 연다/닫는다.
    /// 이미 이동 중이면 무시한다.
    /// </summary>
    public bool TrySetState(bool open)
    {
        if (_isMoving)
            return false;

        if (_isOpen == open)
            return false;

        _isOpen = open;
        _isMoving = true;
        _elapsedTime = 0f;

        return true;
    }

    /// <summary>
    /// 문 상호작용을 받아 노크를 멈추고 현재 열림 상태의 반대로 전환을 시도한다.
    /// 현재 _isOpen 상태를 사용하며 전환에 성공하면 OnDoorPressed를 호출한다.
    /// </summary>
    public void Interact()
    {
        StopKnock();
        bool changed = TrySetState(!_isOpen);

        // 실제로 문 상태 변경에 성공했을 때만 이벤트 발행
        if (changed)
        {
            OnDoorPressed?.Invoke();
        }
    }

    /// <summary>
    /// 외부 노크 이벤트를 받아 문을 두드리는 소리를 반복 재생한다.
    /// 설정된 _knockSource와 _knockClip을 사용하며 _isKnocking 상태를 켠다.
    /// </summary>
    public void StartKnockEvent()
    {
        if (_isKnocking || _knockSource == null || _knockClip == null)
            return;

        _isKnocking = true;
        _knockSource.clip = _knockClip;
        _knockSource.loop = true;
        _knockSource.Play();
    }

    /// <summary>
    /// 상호작용이나 비활성화를 받아 반복 노크 재생을 중지한다.
    /// _knockSource의 재생을 멈추고 _isKnocking 상태를 해제한다.
    /// </summary>
    private void StopKnock()
    {
        if (!_isKnocking)
            return;

        _isKnocking = false;
        if (_knockSource != null)
            _knockSource.Stop();
    }
}
