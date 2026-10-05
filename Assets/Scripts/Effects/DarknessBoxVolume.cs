using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider))]
public class DarknessBoxVolume : MonoBehaviour
{
    [Header("Darkness Box")]
    [SerializeField] private BoxCollider _boxCollider;
    [SerializeField, Min(0f)] private float _density = 1.5f;

    public static DarknessBoxVolume Active { get; private set; }
    public BoxCollider Box => _boxCollider;
    public float Density => _density;

    void OnEnable()
    {
        Active = this;
    }

    void OnDisable()
    {
        if (Active == this)
            Active = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(_boxCollider.center, _boxCollider.size);
        Gizmos.matrix = Matrix4x4.identity;
    }
}
