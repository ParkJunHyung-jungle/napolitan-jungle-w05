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
    public bool IsHeld => _playerCam != null && transform.parent == _playerCam;

    void Awake()
    {
        _playerCam = Camera.main.transform;
        _collider = GetComponent<Collider>();
        _text = GetComponentInChildren<TMP_Text>();
        _rb = GetComponent<Rigidbody>();
        _holdRotation = Quaternion.Euler(_holdRotationOffset);
    }

    public void SetMessage(string message)
    {
        _text.text = message;
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
