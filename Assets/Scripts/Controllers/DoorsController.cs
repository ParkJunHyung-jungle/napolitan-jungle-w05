using UnityEngine;

public class DoorsController : MonoBehaviour
{
    [Header("AudioSource")]
    [SerializeField]
    AudioSource _door;
    [SerializeField]
    AudioSource _lockedDoor;
    [SerializeField]
    AudioSource _crying;

    private void Awake()
    {
        Managers.Sound.RegisterAudioSource(AudioSourceTypes.DOOR, _door);
        Managers.Sound.RegisterAudioSource(AudioSourceTypes.LOCKEDDOOR, _lockedDoor);
        Managers.Sound.RegisterAudioSource(AudioSourceTypes.CRYING, _crying);
    }
}
