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
        Managers.Timeline.OnPhoneCryingStop += StopCrying;
    }

    /// <summary>
    /// 타임라인의 울음 중지 요청을 받아 반복 재생 중인 우는 소리를 멈춘다.
    /// SoundManager에 정지 기능이 없어 _crying 오디오 소스를 직접 멈춘다.
    /// </summary>
    private void StopCrying()
    {
        _crying.Stop();
    }
}
