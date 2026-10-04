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
    private Collider[] _playerColliders;
    private Transform[] _hierarchyTransforms;
    private int _playerLayer;
    private int _interactableLayer;
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
        _playerColliders = _playerCam.root.GetComponentsInChildren<Collider>();
        _hierarchyTransforms = GetComponentsInChildren<Transform>(true);
        _playerLayer = LayerMask.NameToLayer("Player");
        _interactableLayer = LayerMask.NameToLayer("Interactable");

        SetPlayerCollisionIgnored();
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

    /// <summary>
    /// 현재 들고 있는 상태에 따라 지시서를 들거나 놓는다.
    /// IsHeld를 확인하고 PickUp 또는 Drop을 호출해 부모와 물리 상태를 변경한다.
    /// </summary>
    public void Interact()
    {
        if (IsHeld) Drop();
        else PickUp();
    }

    /// <summary>
    /// 지시서를 Player Camera의 자식으로 이동하고 Rigidbody를 들고 있는 상태로 변경한다.
    /// _playerCam과 hold 위치 설정값을 사용하고 지시서 계층을 Player 레이어로 변경한다.
    /// </summary>
    public void PickUp()
    {
        transform.SetParent(_playerCam, false);
        transform.localPosition = _holdOffset;
        transform.localRotation = _holdRotation;
        SetHierarchyLayer(_playerLayer);

        _rb.detectCollisions = true;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.useGravity = false;
        _rb.isKinematic = true;
    }

    /// <summary>
    /// 지시서를 현재 위치에 둔 뒤 Rigidbody를 떨어지는 상태로 변경한다.
    /// 부모를 해제하고 계층을 Slider 레이어로 복원하며 중력과 동적 물리를 활성화한다.
    /// </summary>
    public void Drop()
    {
        transform.SetParent(null, true);
        SetHierarchyLayer(_interactableLayer);

        _rb.detectCollisions = true;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.useGravity = true;
        _rb.isKinematic = false;
    }

    /// <summary>
    /// 지시서 계층의 모든 GameObject에 layer를 적용한다.
    /// layer 값을 사용해 조준 Raycast가 들고 있는 지시서를 무시하도록 변경한다.
    /// </summary>
    private void SetHierarchyLayer(int layer)
    {
        foreach (Transform target in _hierarchyTransforms)
            target.gameObject.layer = layer;
    }

    /// <summary>
    /// 지시서와 Player Collider 사이의 충돌을 항상 무시하도록 설정한다.
    /// _playerColliders를 사용해 지시서가 들리거나 떨어질 때 모두 플레이어를 통과하게 한다.
    /// </summary>
    private void SetPlayerCollisionIgnored()
    {
        foreach (Collider playerCollider in _playerColliders)
            Physics.IgnoreCollision(_collider, playerCollider, true);
    }
}
