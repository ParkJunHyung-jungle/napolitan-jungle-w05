using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "SoundCatalog", menuName = "Audio/Sound Catalog")]
public class SoundCatalog : ScriptableObject
{
    [Header("UI Clips")]
    [SerializeField] private AudioClip _buttonClick;
    [SerializeField] private AudioClip _buttonHovering;
    [SerializeField] private AudioClip _slider;
    [SerializeField] private AudioClip _facilitySliderDrag;
    [SerializeField] private AudioClip _facilitySliderSnap;
    public AudioClip ButtonClick => _buttonClick;
    public AudioClip ButtonHovering => _buttonHovering;
    public AudioClip Slider => _slider;
    public AudioClip FacilitySliderDrag => _facilitySliderDrag;
    public AudioClip FacilitySliderSnap => _facilitySliderSnap;

    [Header("Sfx Clips")]
    [SerializeField] private AudioClip _engineOff;
    [SerializeField] private AudioClip _takingDamage;
    [SerializeField] private AudioClip _fixCompleted;
    [SerializeField] private AudioClip _mediumFix;
    [SerializeField] private AudioClip _gameOver;
    [SerializeField] private AudioClip _doorOpen;
    [SerializeField] private AudioClip _doorClose;
    [SerializeField] private AudioClip _doorLocked;
    [SerializeField] private AudioClip[] _knock;

    public AudioClip EngineOff => _engineOff;
    public AudioClip TakingDamage => _takingDamage;
    public AudioClip FixCompleted => _fixCompleted;
    public AudioClip MediumFix => _mediumFix;
    public AudioClip GameOver => _gameOver;
    public AudioClip DoorOpen => _doorOpen;
    public AudioClip DoorClose => _doorClose;
    public AudioClip DoorLocked => _doorLocked;
    public AudioClip[] Knock => _knock;

    [Header("Phone Clips")]
    [SerializeField] private AudioClip _phoneBell;
    [SerializeField] private AudioClip _phonePickUp;
    [SerializeField] private AudioClip _phoneOff;
    [SerializeField] private AudioClip _phoneHangUp;
    [SerializeField] private AudioClip[] _phoneManVoice;
    [SerializeField] private AudioClip[] _phoneWomenVoice;

    public AudioClip PhoneBell => _phoneBell;
    public AudioClip PhonePickUp => _phonePickUp;
    public AudioClip PhoneOff => _phoneOff;
    public AudioClip PhoneHangUp => _phoneHangUp;
    public AudioClip[] PhoneManVoice => _phoneManVoice;
    public AudioClip[] PhoneWomenVoice => _phoneWomenVoice;


    [Header("Ambient And Timer Clips")]
    [SerializeField] private AudioClip _ambient;
    [SerializeField] private AudioClip _subAmbient;
    [SerializeField] private AudioClip _clock;
    [SerializeField] private AudioClip _countDown;
    [SerializeField] public AudioClip _crying;
    public AudioClip Ambient => _ambient;
    public AudioClip SubAmbient => _subAmbient;
    public AudioClip Clock => _clock;
    public AudioClip CountDown => _countDown;
    public AudioClip Crying => _crying;

    [Header("Siren And Warning Clips")]
    [SerializeField] private AudioClip _simpleSiren;
    [SerializeField] private AudioClip _complaxSiren;
    [SerializeField] private AudioClip _halfHp;
    [SerializeField] private AudioClip _lowHp;
    public AudioClip SimpleSiren => _simpleSiren;
    public AudioClip ComplaxSiren => _complaxSiren;
    public AudioClip HalfHp => _halfHp;
    public AudioClip LowHp => _lowHp;

    [Header("Game Clear")]
    [SerializeField] private AudioClip _gameClear;
    [SerializeField] private AudioMixerGroup _gameClearOutput;
    public AudioClip GameClear => _gameClear;
    public AudioMixerGroup GameClearOutput => _gameClearOutput;
}
