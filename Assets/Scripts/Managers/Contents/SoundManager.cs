using System;

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SoundManager
{
    private const string CATALOG_PATH = "Datas/SoundCatalog";

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

        _buttonSource = CreateSource("Button");
        _sliderSource = CreateSource("Slider");
        _sfxSoruce = CreateSource("Sfx", pitch: 0.5f);
        _ambientSource = CreateSource("Ambient", volume: 0.5f, pitch: 0.6f);
        _subAmbientSource = CreateSource("SubAmbient", volume: 0.5f, pitch: 0.6f);
        _clockSource = CreateSource("Clock");
        _sirenSource = CreateSource("Siren", volume: 0.9f);
        _countDownSource = CreateSource("CountDown");
        _warningSource = CreateSource("Warning");
        _gameClearSource = CreateSource("GameClear", outputGroup: _catalog.GameClearOutput);
        _facilityButtonSource = CreateSource("FacilityButton");
        _facilityDragSource = CreateSource("FacilityDrag", volume: 0.3f, pitch: 0.3f);
        _facilitySnapSource = CreateSource("FacilitySnap", pitch: 0.5f);
        _sources = new[]
        {
            _buttonSource, _sliderSource, _sfxSoruce, _ambientSource, _clockSource,
            _sirenSource, _countDownSource, _warningSource, _gameClearSource,
            _facilityButtonSource, _facilityDragSource, _facilitySnapSource
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
        _buttonSource = _sliderSource = _sfxSoruce = _ambientSource = _clockSource = null;
        _sirenSource = _countDownSource = _warningSource = _gameClearSource = null;
        _facilityButtonSource = _facilityDragSource = _facilitySnapSource = null;
    }

    private void OnSceneChanged(Scene previousScene, Scene nextScene)
    {
        foreach (AudioSource source in _sources)
            source.Stop();
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

    /// <summary>Plays the catalog slider clip once on the initialized slider channel.</summary>
    public void SliderSound()
    {
        SoundPlay(_sliderSource, _catalog.Slider);
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
    public void EngineOffSound()
    {
        SoundPlay(_sfxSoruce, _catalog.EngineOff);
    }

    /// <summary>Overlays the catalog damage clip on the initialized Sfx channel without stopping it.</summary>
    public void TakingDamageSound()
    {
        _sfxSoruce.PlayOneShot(_catalog.TakingDamage);
    }

    /// <summary>Overlays the catalog repair-complete clip on the initialized Sfx channel.</summary>
    public void FixCompletedSound()
    {
        _sfxSoruce.PlayOneShot(_catalog.FixCompleted);
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

    /// <summary>Plays the catalog medium-repair clip once, replacing the initialized Sfx channel.</summary>
    public void MediumFixSound()
    {
        SoundPlay(_sfxSoruce, _catalog.MediumFix);
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
    /// 설비 버튼의 클릭음을 카탈로그에서 읽어 버튼 채널에 겹쳐 재생한다.
    /// 재생 중인 클릭음은 끊지 않고 유지한다.
    /// </summary>
    public void FacilityButtonClickSound()
    {
        _facilityButtonSource.PlayOneShot(_catalog.ButtonClick);
    }

    /// <summary>
    /// 설비 슬라이더가 움직이면 전용 채널에서 드래그음을 반복 재생한다.
    /// 이미 재생 중이면 시작하지 않아 루프 상태를 유지한다.
    /// </summary>
    public void StartFacilitySliderDragSound()
    {
        if (!_facilityDragSource.isPlaying)
            SoundPlay(_facilityDragSource, _catalog.FacilitySliderDrag, true);
    }

    /// <summary>
    /// 설비 슬라이더가 멈추면 전용 드래그 채널을 중지한다.
    /// 다른 효과음 채널에는 영향을 주지 않는다.
    /// </summary>
    public void StopFacilitySliderDragSound()
    {
        _facilityDragSource.Stop();
    }

    /// <summary>
    /// 설비 슬라이더가 목표 범위에 진입할 때 철컥 소리를 겹쳐 재생한다.
    /// 드래그 루프와 독립된 채널을 사용한다.
    /// </summary>
    public void FacilitySliderSnapSound()
    {
        _facilitySnapSource.PlayOneShot(_catalog.FacilitySliderSnap);
    }
}
