using UnityEngine;
using System;

public class DoorController : MonoBehaviour, IInteractable
{
    [Header("Door Movement")]
    [SerializeField] private float _duration = 1f;
    [SerializeField] private float _openAngle = 90f;
    [SerializeField]
    private AnimationCurve _moveCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private bool _isOpen = false;
    private bool _isMoving = false;

    private float _elapsedTime;

    private Quaternion _closedRotation;
    private Quaternion _openRotation;

    public bool IsOpen => _isOpen;
    public bool IsMoving => _isMoving;

    private IInteractable _interactableImplementation;
    public event Action OnDoorPressed;

    private void Awake()
    {
        _closedRotation = transform.localRotation;
        _openRotation =
            _closedRotation * Quaternion.Euler(0f, _openAngle, 0f);
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

    public void Interact()
    {
        bool changed = TrySetState(!_isOpen);

        // 실제로 문 상태 변경에 성공했을 때만 이벤트 발행
        if (changed)
        {
            OnDoorPressed?.Invoke();
        }
    }
}
