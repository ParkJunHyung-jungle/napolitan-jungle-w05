using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioMixer _audioMixer;

    [Header("사운드 소스")]
    [SerializeField] private AudioSource _buttonSource;
    [SerializeField] private AudioSource _sliderSource;
    [SerializeField] private AudioSource _sfxSoruce;
    [SerializeField] private AudioSource _engineSource;
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
    [SerializeField] private AudioClip _engine;
    [SerializeField] private AudioClip _engineOff;
    [SerializeField] private AudioClip _takingDamage;

    [Header("엔진 오디오 클립")]
    [SerializeField] private AudioClip _fixCompleted;

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


    public void ButtonClickSound()
    {

    }

    public void ButtonHoveringSound()
    {

    }
    public void SliderSound()
    {

    }

    public void EngineSound()
    {

    }

    public void EngineOffSound()
    {

    }

    public void TakingDamageSound()
    {

    }
    
    public void FixCompletedSound()
    {

    }
    public void ClockSound()
    {

    }

    public void CountDownSound()
    {

    }

    public void HalfHpSound()
    {

    }

    public void LowHpSound()
    {

    }

    public void SimpleSirenSound()
    {
        Play(_sirenSource, _simpleSiren, true);
    }

    public void ComplaxSirenSound()
    {

    }

}
