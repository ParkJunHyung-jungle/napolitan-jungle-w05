using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class StairPortal : MonoBehaviour
{
    [Header("Destination")]
    [SerializeField] private Transform _extraRoomRoot;
    [SerializeField] private float _destinationLocalY;

    void OnTriggerEnter(Collider other)
    {
        FirstPersonController player = other.GetComponentInParent<FirstPersonController>();
        if (player == null)
            return;

        float worldY = _extraRoomRoot.TransformPoint(0f, _destinationLocalY, 0f).y;
        player.TeleportToY(worldY);
    }
}
