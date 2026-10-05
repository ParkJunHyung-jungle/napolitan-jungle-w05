using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SoundManager
{
    private const string CATALOG_PATH = "Datas/SoundCatalog";
    private const float PHONE_BELL_REPEAT_SECONDS = 3f;

    [Header("Runtime Audio")]
    private SoundCatalog _catalog;
    private GameObject _sourceRoot;
    private AudioSource[] _sources;
    private AudioSource _buttonSource;
    private AudioSource _sliderSource;
    private AudioSource _sfxSoruce;
    private AudioSource _ambientSource;
    private AudioSource _subAmbientSource;
    private AudioSource _clockSource;
    private AudioSource _sirenSource;
    private AudioSource _countDownSource;
    private AudioSource _warningSource;
    private AudioSource _gameClearSource;
    private AudioSource _facilityButtonSource;
    private AudioSource _facilityDragSource;
    private AudioSource _facilitySnapSource;
    private AudioSource _doorSource;
    private AudioSource _lockedDoorSource;
    private AudioSource _cryingSource;
    private AudioSource _phoneSource;
    private Coroutine _phoneRingingRoutine;
    private AudioSource _onThePhoneSource;
    private AudioSource _lightSwitchSource;
    private AudioSource _faxSource;
    private AudioSource _inseinSource;
    private AudioSource _paperSource;
    private AudioSource _heartBeatSource;
    private AudioSource _shredderSource;
    private AudioSource _lampSource;
    private AudioSource _footStepSource;

    public AudioSource OnThePhoneSource => _onThePhoneSource;
    public AudioSource InseinSource => _inseinSource;
    public AudioSource HeartBeatSource => _heartBeatSource;

    /// <summary>
    /// Loads the Resources catalog and creates twelve independent audio channels under
    /// Managers.Instance. Repeated calls preserve the existing channels and playback.
    /// </summary>
    public void Init()
    {
        if (_sourceRoot != null)
            return;

        _catalog = Resources.Load<SoundCatalog>(CATALOG_PATH);
        if (_catalog == null)
            throw new InvalidOperationException($"Missing SoundCatalog at Resources/{CATALOG_PATH}.");

        Transform managerRoot = Managers.Instance.transform;
        _sourceRoot = new GameObject("Sound");
        _sourceRoot.transform.SetParent(managerRoot, false);

        //2D 사운드 소스 초기화
        _buttonSource = CreateSource("Button");
        _sliderSource = CreateSource("Slider");
        _sfxSoruce = CreateSource("Sfx");
        _ambientSource = CreateSource("Ambient", volume: 0.3f);
        _subAmbientSource = CreateSource("SubAmbient");
        _clockSource = CreateSource("Clock");
        _sirenSource = CreateSource("Siren", volume: 0.9f);
        _countDownSource = CreateSource("CountDown");
        _warningSource = CreateSource("Warning");
        _gameClearSource = CreateSource("GameClear", outputGroup: _catalog.GameClearOutput);
        _onThePhoneSource = CreateSource("OnThePhone", volume: 0.7f);
        _inseinSource = CreateSource("Insein", volume: 0.7f);
        _paperSource = CreateSource("HandlingPaper");
        _heartBeatSource = CreateSource("HeartBeat", volume: 0.7f);
        _shredderSource = CreateSource("Shredder", volume: 0.7f);
        _footStepSource = CreateSource("Footstep");



        AudioHighPassFilter highPass = _onThePhoneSource.gameObject.AddComponent<AudioHighPassFilter>();
        highPass.cutoffFrequency = 400f;

        AudioDistortionFilter distortion = _onThePhoneSource.gameObject.AddComponent<AudioDistortionFilter>();
        distortion.distortionLevel = 0.15f;

        AudioLowPassFilter lowPass = _onThePhoneSource.gameObject.AddComponent<AudioLowPassFilter>();
        lowPass.cutoffFrequency = 3000f;

        _sources = new[]
        {
            _buttonSource, _sliderSource, _sfxSoruce, _ambientSource, _subAmbientSource, _clockSource,
            _sirenSource, _countDownSource, _warningSource, _gameClearSource,
            _facilityButtonSource, _facilityDragSource, _facilitySnapSource, _doorSource, _cryingSource, _lockedDoorSource,
            _phoneSource, _onThePhoneSource, _lightSwitchSource, _faxSource, _inseinSource, _paperSource, _heartBeatSource, _footStepSource
        };
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    /// <summary>
    /// Stops all owned channels, releases their clips, and destroys the audio child
    /// without destroying Managers or catalog assets. Resets state so Init can recreate it.
    /// </summary>
    public void Clear()
    {
        if (_sourceRoot != null)
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
            if (_phoneRingingRoutine != null)
                StopPhoneRinging();
            foreach (AudioSource source in _sources)
            {
                source.Stop();
                source.clip = null;
            }

            _sourceRoot.SetActive(false);
            UnityEngine.Object.Destroy(_sourceRoot);
        }

        _sourceRoot = null;
        _sources = null;
        _catalog = null;
        _buttonSource = _sliderSource = _sfxSoruce = _ambientSource = _subAmbientSource = _clockSource = null;
        _sirenSource = _countDownSource = _warningSource = _gameClearSource = null;
        _facilityButtonSource = _facilityDragSource = _facilitySnapSource = null;
        _doorSource = null;
        _cryingSource = null;
        _lockedDoorSource = null;
        _phoneSource = null;
        _onThePhoneSource = null;
        _lightSwitchSource = null;
        _faxSource = null;
        _inseinSource = null;
        _paperSource = null;

        _heartBeatSource = null;
        _lampSource = null;
        _footStepSource = null;

    }

    private void OnSceneChanged(Scene previousScene, Scene nextScene)
    {
        if (_phoneRingingRoutine != null)
            StopPhoneRinging();
        foreach (AudioSource source in _sources)
        {
            if (source != null)
                source.Stop();
        }
    }

    /// <summary>
    /// HeartBeat와 Insein을 제외한 모든 효과음 채널의 재생을 중지한다.
    /// 관리 중인 채널과 씬 오브젝트에 등록된 채널의 재생 상태를 변경한다.
    /// </summary>
    public void StopAllSoundsExceptMentalitySounds()
    {
        if (_phoneRingingRoutine != null)
            StopPhoneRinging();

        foreach (AudioSource source in _sources)
        {
            if (source != null && source != _heartBeatSource && source != _inseinSource)
                source.Stop();
        }

        if (_shredderSource != null)
            _shredderSource.Stop();
        if (_lampSource != null)
            _lampSource.Stop();
    }

    /// audioSourceTypes에 해당하는 사운드 채널에 전달받은 audioSource를 등록한다.
    /// 이후 해당 종류의 사운드를 재생할 때 사용할 AudioSource 참조를 변경한다.
    /// </summary>
    public void RegisterAudioSource(AudioSourceTypes audioSourceTypes, AudioSource audioSource)
    {
        switch (audioSourceTypes)
        {
            case AudioSourceTypes.DOOR:
                _doorSource = audioSource;
                break;
            case AudioSourceTypes.LOCKEDDOOR:
                _lockedDoorSource = audioSource;
                break;
            case AudioSourceTypes.LIGHTSWITCH:
                _lightSwitchSource = audioSource;
                break;
            case AudioSourceTypes.FAX:
                _faxSource = audioSource;
                break;
            case AudioSourceTypes.CRYING:
                _cryingSource = audioSource;
                break;
            case AudioSourceTypes.FOOTSTEP:
                _footStepSource = audioSource;
                break;
            case AudioSourceTypes.TELEPHONE:
                _phoneSource = audioSource;
                break;
            case AudioSourceTypes.LAMP:
                _lampSource = audioSource;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Creates a non-autoplaying 2D source under the owned root using name, volume,
    /// pitch, and optional outputGroup. Returns the newly configured channel.
    /// </summary>
    private AudioSource CreateSource(string name, float volume = 1f, float pitch = 1f,
        AudioMixerGroup outputGroup = null)
    {
        GameObject channel = new GameObject(name);
        channel.transform.SetParent(_sourceRoot.transform, false);
        AudioSource source = channel.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.volume = volume;
        source.pitch = pitch;
        source.outputAudioMixerGroup = outputGroup;
        return source;
    }
    /// <summary>
    /// 3D 오디오 소스를 추가하고 초기 설정을 적용
    /// 자동 재생을 해제한 새 소스를 반환
    /// </summary>
    private AudioSource Create3DSource(GameObject target)
    {
        AudioSource source = target.GetComponent<AudioSource>();
        return source;


    }

    /// <summary>
    /// Stops source and starts clip with the supplied loop flag, replacing playback
    /// only on that channel. Requires Init to have created the owned source.
    /// </summary>
    private void SoundPlay(AudioSource source, AudioClip clip, bool loop = false)
    {
        source.Stop();
        source.clip = clip;
        source.loop = loop;
        source.Play();
    }

    /// <summary>Plays the catalog click clip once on the initialized button channel.</summary>
    public void ButtonClickSound()
    {
        SoundPlay(_buttonSource, _catalog.ButtonClick);
    }

    /// <summary>Plays the catalog hover clip once, replacing the initialized button channel.</summary>
    public void ButtonHoveringSound()
    {
        SoundPlay(_buttonSource, _catalog.ButtonHovering);
    }


    /// <summary>Loops the catalog ambient clip on the initialized ambient channel.</summary>
    public void AmbientSound()
    {
        SoundPlay(_ambientSource, _catalog.Ambient, true);
    }

    /// <summary>Stops playback on the initialized ambient channel without affecting other channels.</summary>
    public void AmbientSoundOff()
    {
        _ambientSource.Stop();
    }


    /// <summary>Loops the catalog ambient clip on the initialized ambient channel.</summary>
    public void SubAmbientSound()
    {
        SoundPlay(_subAmbientSource, _catalog.SubAmbient, true);
    }

    /// <summary>Stops playback on the initialized ambient channel without affecting other channels.</summary>
    public void SubAmbientSoundOff()
    {
        _subAmbientSource.Stop();
    }

    /// <summary>Plays the catalog engine-off clip once, replacing the initialized Sfx channel.</summary>


    /// <summary>Overlays the catalog damage clip on the initialized Sfx channel without stopping it.</summary>
    public void TakingDamageSound()
    {
        _sfxSoruce.PlayOneShot(_catalog.TakingDamage);
    }
    public void GetHitSound()
    {
        _sfxSoruce.PlayOneShot(_catalog.HitPlayer);
    }

    /// <summary>Loops the catalog clock clip on the initialized clock channel.</summary>
    public void ClockSound()
    {
        SoundPlay(_clockSource, _catalog.Clock, true);
    }

    /// <summary>Stops the initialized clock channel without stopping the countdown channel.</summary>
    public void StopClockSound()
    {
        _clockSource.Stop();
    }

    /// <summary>Stops the initialized clock and plays the catalog countdown clip once on its own channel.</summary>
    public void CountDownSound()
    {
        _clockSource.Stop();
        SoundPlay(_countDownSource, _catalog.CountDown);
    }

    /// <summary>Plays the catalog half-health clip once, replacing the initialized warning channel.</summary>
    public void HalfHpSound()
    {
        SoundPlay(_warningSource, _catalog.HalfHp);
    }

    /// <summary>Plays the catalog low-health clip once, replacing the initialized warning channel.</summary>
    public void LowHpSound()
    {
        SoundPlay(_warningSource, _catalog.LowHp);
    }

    /// <summary>Loops the catalog simple siren clip, replacing the initialized siren channel.</summary>
    public void SimpleSirenSound()
    {
        SoundPlay(_sirenSource, _catalog.SimpleSiren, true);
    }

    /// <summary>Loops the catalog complex siren clip, replacing the initialized siren channel.</summary>
    public void ComplaxSirenSound()
    {
        SoundPlay(_sirenSource, _catalog.ComplaxSiren, true);
    }

    /// <summary>Stops playback on the initialized siren channel without affecting other channels.</summary>
    public void StopSirenSound()
    {
        _sirenSource.Stop();
    }

    /// <summary>Plays the catalog game-over clip once, replacing the initialized Sfx channel.</summary>
    public void GameOverSound()
    {
        SoundPlay(_sfxSoruce, _catalog.GameOver);
    }

    /// <summary>Plays the catalog game-clear clip once on its independent initialized UISound channel.</summary>
    public void GameClearSound()
    {
        SoundPlay(_gameClearSource, _catalog.GameClear);
    }



    /// <summary>
    /// 문 열림 소리를 한번 재생한다
    /// doorSource에 다른 재생과 겹칠 수 있다.
    /// </summary>
    public void DoorOpenSound()
    {
        SoundPlay(_doorSource, _catalog.DoorOpen);

    }

    /// <summary>
    /// 잠긴 문 채널에서 반복 재생 중인 노크 소리를 중지한다.
    /// _lockedDoorSource의 재생 상태만 변경하고 문 열림/닫힘 효과음은 유지한다.
    /// </summary>
    public void DoorKnockSoundOff()
    {
        _doorSource.Stop();
    }



    /// <summary>
    /// 문 닫힘 소리를 한번 재생한다.
    /// doorSource에 다른 재생과 겹칠 수 있다.
    /// </summary>
    public void DoorCloseSound()
    {
        SoundPlay(_doorSource, _catalog.DoorClose, false);
    }
    /// 문 잠김 소리를 한번 재생한다.
    /// </summary>
    public void DoorLockedSound()
    {
        SoundPlay(_lockedDoorSource, _catalog.DoorLocked, false);
    }
    /// <summary>
    /// 문 노크 소리를 루프로 재생한다.
    /// doorSource에 다른 재생과 겹칠 수 있다.
    /// </summary>
    public void DoorKnockSound()
    {
        AudioClip[] clips = _catalog.Knock;
        int index = UnityEngine.Random.Range(0, clips.Length);
        SoundPlay(_doorSource, clips[index], true);
    }

    /// <summary>
    /// PhoneBell을 전화기 소스에서 반복 재생하고 짧은 울림 구간을 다시 시작한다.
    /// 카탈로그의 벨소리를 사용하며 반복 코루틴을 저장한다.
    /// </summary>
    public void PhoneRinging()
    {
        SoundPlay(_phoneSource, _catalog.PhoneBell, true);
        _phoneRingingRoutine = Managers.Instance.StartCoroutine(RepeatPhoneBell());
    }

    /// <summary>
    /// 현재 전화기 소스의 벨소리 반복과 재생을 중지한다.
    /// 저장된 코루틴과 전화기 소스의 상태를 정리한다.
    /// </summary>
    public void StopPhoneRinging()
    {
        if (_phoneRingingRoutine != null)
        {
            Managers.Instance.StopCoroutine(_phoneRingingRoutine);
            _phoneRingingRoutine = null;
        }

        _phoneSource.Stop();
    }

    /// <summary>
    /// 전화기 벨소리의 첫 울림이 끝난 뒤 재생 위치를 처음으로 되돌린다.
    /// PHONE_BELL_REPEAT_SECONDS 간격으로 전화기 소스의 재생 위치를 갱신한다.
    /// </summary>
    private IEnumerator RepeatPhoneBell()
    {
        while (true)
        {
            yield return new WaitForSeconds(PHONE_BELL_REPEAT_SECONDS);
            _phoneSource.time = 0f;
        }
    }
    public void PhonePickUp()
    {
        _phoneSource.PlayOneShot(_catalog.PhonePickUp);

    }
    public void PhoneHangUp()
    {
        SoundPlay(_phoneSource, _catalog.PhoneHangUp);

    }
    public void TalkingManVoice()
    {
        AudioClip[] clips = _catalog.PhoneManVoice;
        int index = UnityEngine.Random.Range(0, clips.Length);
        SoundPlay(_onThePhoneSource, clips[index], false);

    }
    public void TalkingWomenVoice()
    {
        AudioClip[] clips = _catalog.PhoneWomenVoice;
        int index = UnityEngine.Random.Range(0, clips.Length);
        SoundPlay(_onThePhoneSource, clips[index], false);
    }


    public void CallEndSound()
    {
        SoundPlay(_onThePhoneSource, _catalog.PhoneOff, true);

    }

    public void StopCallEndSound()
    {
        _onThePhoneSource.Stop();
    }

    /// 우는 소리를 루프로 재생한다.
    /// </summary>
    public void CryingSound()
    {
        SoundPlay(_cryingSource, _catalog.Crying, true);
    }
    public void StopCryingSound()
    {
        _cryingSource.Stop();
    }
    public void LightOnSound()
    {
        SoundPlay(_lightSwitchSource, _catalog.SwitchOn);
    }
    public void LightOffSound()
    {
        SoundPlay(_lightSwitchSource, _catalog.SwitchOff);
    }
    public void FaxSound()
    {
        SoundPlay(_faxSource, _catalog.Fax);
    }
    public void FaxErrorSound()
    {
        SoundPlay(_faxSource, _catalog.FaxError);
    }
    public void InseinSound()
    {
        SoundPlay(_inseinSource, _catalog.Insein, true);
    }
    public void StopInseinSound()
    {
        _inseinSource.Stop();
    }
    public void PaperSound()
    {
        _paperSource.PlayOneShot(_catalog.HandlingPaper);
    }
    public void LampFlickerSound()
    {
        _lampSource.PlayOneShot(_catalog.LampFlicker);

    }
    public void HeartBeatSound()
    {
        SoundPlay(_heartBeatSource, _catalog.HeartBeat, true);
    }
    public void StopHeartBeatSound()
    {
        _heartBeatSource.Stop();
    }
    /// <summary>
    /// RoomLightController가 등록한 램프 소스에서 램프 루프 사운드를 반복 재생한다.
    /// _catalog.LampAmbient를 사용하며 _lampSource의 재생을 교체한다.
    /// </summary>
    public void LampAmbientSound()
    {
        SoundPlay(_lampSource, _catalog.LampAmbient, true);

    }
    public void StopLampAmbientSound()
    {
        _lampSource.Stop();

    }
    public void ShredderSound()
    {
        _shredderSource.PlayOneShot(_catalog.Shredder);

    }
    public void LaughSound()
    {
        _faxSource.PlayOneShot(_catalog.WomenLaugh);
    }

    /// <summary>
    /// SoundCatalog의 발소리 목록에서 무작위 클립을 선택한다.
    /// 선택한 클립을 발소리 전용 AudioSource에서 한 번 재생한다.
    /// </summary>
    public void FootStepSound()
    {
        AudioClip[] clips = _catalog.FootStep;
        int index = UnityEngine.Random.Range(0, clips.Length);
        _footStepSource.PlayOneShot(clips[index]);
    }
}
