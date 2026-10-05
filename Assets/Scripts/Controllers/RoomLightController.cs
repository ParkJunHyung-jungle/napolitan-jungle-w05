using UnityEngine;

public class RoomLightController : MonoBehaviour
{
    private void Awake()
    {
        AudioSource audioSource = gameObject.GetComponent<AudioSource>();
        Managers.Sound.RegisterAudioSource(AudioSourceTypes.LAMP, audioSource);
    }
}
