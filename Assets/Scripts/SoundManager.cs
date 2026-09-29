using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioMixer _audioMixer;

    [Header("사운드 소스")]
    [SerializeField] private AudioSource _buttonSource;
    [SerializeField] private AudioSource _sliderSource;
    [SerializeField] private AudioSource _sfxSoruce;
    [SerializeField] private AudioSource _ambientSource;
    [SerializeField] private AudioSource _clockSource;
    [SerializeField] private AudioSource _sirenSource;
    [SerializeField] private AudioSource _countDownSource;
    [SerializeField] private AudioSource _warningSource;

    [Header("오디오 클립")]
    [Header("버튼, 슬라이더 오디오 클립")]
    [SerializeField] private AudioClip _buttonClick;
    [SerializeField] private AudioClip _buttonHovering;
    [SerializeField] private AudioClip _slider;

    [Header("Sfx 오디오 클립")]
    [SerializeField] private AudioClip _engineOff;
    [SerializeField] private AudioClip _takingDamage;
    [SerializeField] private AudioClip _fixCompleted;
    [SerializeField] private AudioClip _mediumFix;

    [Header("엠비언트 오디오 클립")]
    [SerializeField] private AudioClip _ambient;

    [Header("시계 오디오 클립")]
    [SerializeField] private AudioClip _clock;

    [Header("사이렌 오디오 클립")]
    [SerializeField] private AudioClip _simpleSiren;
    [SerializeField] private AudioClip _complaxSiren;

    [Header("카운트 다운 오디오 클립")]
    [SerializeField] private AudioClip _countDown;

    [Header("위험상태 오디오 클립")]
    [SerializeField] private AudioClip _halfHp;
    [SerializeField] private AudioClip _lowHp;

    private void SoundPlay(AudioSource source, AudioClip clip, bool loop = false)
    {
        source.Stop();
        source.clip = clip;
        source.loop = loop;
        source.Play();
    }

    //버튼 및 슬라이더 소리
    public void ButtonClickSound()
    {
        SoundPlay(_buttonSource, _buttonClick, false);
    }

    public void ButtonHoveringSound()
    {
        SoundPlay(_buttonSource, _buttonHovering, false);
    }
    public void SliderSound()
    {
        SoundPlay(_sliderSource, _slider, false);
    }

    //엔진 루프 소리
    public void AmbientSound()
    {
        SoundPlay(_ambientSource, _ambient, true);
    }

    public void AmbientSoundOff()
    {
        _ambientSource.Stop();
    }
    //엔진 꺼지는 알림 소리
    public void EngineOffSound()
    {
        SoundPlay(_sfxSoruce, _engineOff, false);
    }

    //내구도 깎이는 알림 소리
    public void TakingDamageSound()
    {
        _sfxSoruce.PlayOneShot(_takingDamage);
    }

    //퍼즐 완료 소리
    public void FixCompletedSound()
    {
        //_sirenSource.Stop();
        //_clockSource.Stop();
        //_countDownSource.Stop();
        _sfxSoruce.PlayOneShot(_fixCompleted);
    }

    //30초 시계 루프 소리 / 시계 소리 정지
    public void ClockSound()
    {
        SoundPlay(_clockSource, _clock, true);

    }

    public void StopClockSound()
    {
        _clockSource.Stop();

    }

    //10초 카운트 다운 소리
    public void CountDownSound()
    {
        _clockSource.Stop();
        SoundPlay(_countDownSource, _countDown, false);
    }

    //내구도 절반 알림 소리
    public void HalfHpSound()
    {
        SoundPlay(_warningSource, _halfHp, false);
    }

    //내구도 낮음 알림 소리
    public void LowHpSound()
    {
        SoundPlay(_warningSource, _lowHp, false);
    }

    //단순 및 복잡한 사이렌 소리 / 사이렌 소리 정지 
    public void SimpleSirenSound()
    {
        SoundPlay(_sirenSource, _simpleSiren, true);
    }

    public void ComplaxSirenSound()
    {
        SoundPlay(_sirenSource, _complaxSiren, true);
    }

    public void StopSirenSound()
    {
        _sirenSource.Stop();
    }

    public void MediumFixSound()
    {
        SoundPlay(_sfxSoruce, _mediumFix, false);
    }

}
