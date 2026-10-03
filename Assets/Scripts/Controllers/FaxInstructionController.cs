using UnityEngine;
using TMPro;

public class FaxInstructionController : MonoBehaviour, IInteractable
{
    [Header("Hold")]
    [SerializeField]
    private Vector3 _holdOffset = new Vector3(0f, 0f, 1f);
    [SerializeField]
    private Vector3 _holdRotationOffset = new Vector3(-10f, 0f, 0f);

    private Transform _playerCam;
    private Collider _collider;
    private TMP_Text _text;
    private Rigidbody _rb;
    private Quaternion _holdRotation;
    private bool _isReal;
    public bool IsHeld => _playerCam != null && transform.parent == _playerCam;
    public bool IsReal => _isReal;

    void Awake()
    {
        _playerCam = Camera.main.transform;
        _collider = GetComponent<Collider>();
        _text = GetComponentInChildren<TMP_Text>();
        _rb = GetComponent<Rigidbody>();
        _holdRotation = Quaternion.Euler(_holdRotationOffset);
    }

    /// <summary>
    /// 출력물에 표시할 문구와 진짜 명령서 여부를 설정한다.
    /// message를 _text에 표시하고 isReal을 _isReal에 저장한다. false이면 가짜 명령서이다.
    /// </summary>
    public void SetMessage(string message, bool isReal)
    {
        _text.text = message;
        _isReal = isReal;
    }

    public void Interact()
    {
        if (IsHeld) Drop();
        else PickUp();
    }

    public void PickUp()
    {
        transform.SetParent(_playerCam, false);
        transform.localPosition = _holdOffset;
        transform.localRotation = _holdRotation;

        _collider.enabled = false;
        _rb.useGravity = false;
        _rb.isKinematic = true;
    }

    public void Drop()
    {
        transform.SetParent(null, true);
        _collider.enabled = true;
        _rb.useGravity = true;
        _rb.isKinematic = false;
    }
}
