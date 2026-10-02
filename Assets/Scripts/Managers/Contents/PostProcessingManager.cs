using System.Collections;

using UnityEngine;

using UnityEngine.Rendering.PostProcessing;

public class PostProcessingManager
{
    private const float MAX_DISTORTION_INTENSITY = 100f;
    private const float FAILURE_VIGNETTE_PEAK = 0.55f;
    private const float FAILURE_VIGNETTE_DURATION = 3f;
    private const float FAILURE_DISTORTION_STRENGTH = 30f;
    private const float FAILURE_DISTORTION_DURATION = 0.6f;
    private const int FAILURE_DISTORTION_WAVE_COUNT = 2;

    [Header("Vignette")]
    private Vignette _vignette;
    private float _vignetteBase;
    private float _vignettePunch;
    private Coroutine _vignetteBaseCoroutine;
    private Coroutine _vignettePunchCoroutine;

    [Header("Distortion")]
    private LensDistortion _lensDistortion;
    private float _distortionBase;
    private float _distortionPunch;
    private Coroutine _distortionBaseCoroutine;
    private Coroutine _distortionPunchCoroutine;

    public GameObject LoadPostProcessVolume => Resources.Load<GameObject>("Prefabs/UIs/Volume");

    /// <summary>
    /// 포스트 프로세싱 볼륨 프리팹을 생성해 Managers 하위에 둔다.
    /// 볼륨 프로필에서 Vignette와 LensDistortion 설정을 가져와 저장한다.
    /// </summary>
    public void Init()
    {
        GameObject volumeObject = Object.Instantiate(LoadPostProcessVolume);
        volumeObject.transform.SetParent(Managers.Instance.transform, true);

        PostProcessProfile profile = volumeObject.GetComponent<PostProcessVolume>().profile;
        profile.TryGetSettings(out _vignette);
        profile.TryGetSettings(out _lensDistortion);
    }

    /// <summary>
    /// 실행 중인 모든 효과를 중지하고 기본값과 순간값을 0으로 되돌린다.
    /// 비네트와 왜곡 세기를 0으로 적용한다.
    /// </summary>
    public void Clear()
    {
        StopEffectCoroutine(ref _vignetteBaseCoroutine);
        StopEffectCoroutine(ref _vignettePunchCoroutine);
        StopEffectCoroutine(ref _distortionBaseCoroutine);
        StopEffectCoroutine(ref _distortionPunchCoroutine);

        _vignetteBase = 0f;
        _vignettePunch = 0f;
        _distortionBase = 0f;
        _distortionPunch = 0f;

        ApplyVignette();
        ApplyDistortion();
    }

    /// <summary>
    /// 비네트 기본 세기를 duration 동안 intensity까지 변경한다.
    /// duration이 0 이하이면 즉시 적용하며, 변경된 값은 _vignetteBase에 유지된다.
    /// </summary>
    public void SetVignette(float intensity, float duration = 0f)
    {
        StopEffectCoroutine(ref _vignetteBaseCoroutine);

        if (duration <= 0f)
        {
            _vignetteBase = intensity;
            ApplyVignette();
            return;
        }

        _vignetteBaseCoroutine = Managers.Instance.StartCoroutine(VignetteBaseCoroutine(intensity, duration));
    }

    /// <summary>
    /// 비네트를 peak만큼 순간적으로 올린 뒤 duration 동안 0으로 줄인다.
    /// 실행 중인 순간 효과는 중지하고 새로 시작하며, _vignetteBase는 변경하지 않는다.
    /// </summary>
    public void PunchVignette(float peak, float duration)
    {
        StopEffectCoroutine(ref _vignettePunchCoroutine);
        _vignettePunchCoroutine = Managers.Instance.StartCoroutine(VignettePunchCoroutine(peak, duration));
    }

    /// <summary>
    /// 렌즈 왜곡 기본 세기를 duration 동안 intensity까지 변경한다.
    /// duration이 0 이하이면 즉시 적용하며, 변경된 값은 _distortionBase에 유지된다.
    /// </summary>
    public void SetDistortion(float intensity, float duration = 0f)
    {
        StopEffectCoroutine(ref _distortionBaseCoroutine);

        if (duration <= 0f)
        {
            _distortionBase = intensity;
            ApplyDistortion();
            return;
        }

        _distortionBaseCoroutine = Managers.Instance.StartCoroutine(DistortionBaseCoroutine(intensity, duration));
    }

    /// <summary>
    /// strength 세기와 waveCount 주기의 감쇠 파동으로 렌즈 왜곡을 duration 동안 흔든다.
    /// 실행 중인 순간 효과는 중지하고 새로 시작하며, _distortionBase는 변경하지 않는다.
    /// </summary>
    public void PunchDistortion(float strength, float duration, int waveCount)
    {
        StopEffectCoroutine(ref _distortionPunchCoroutine);
        _distortionPunchCoroutine = Managers.Instance.StartCoroutine(
            DistortionPunchCoroutine(strength, duration, waveCount));
    }

    /// <summary>
    /// 타이머 실패 프리셋으로 렌즈 왜곡과 비네트 순간 효과를 재생한다.
    /// 이미 실행 중인 순간 효과는 중복 시작하지 않고 유지한다.
    /// </summary>
    public void TriggerTimerFailure()
    {
        if (_distortionPunchCoroutine == null)
        {
            PunchDistortion(FAILURE_DISTORTION_STRENGTH, FAILURE_DISTORTION_DURATION, FAILURE_DISTORTION_WAVE_COUNT);
        }

        if (_vignettePunchCoroutine == null)
        {
            PunchVignette(FAILURE_VIGNETTE_PEAK, FAILURE_VIGNETTE_DURATION);
        }
    }

    /// <summary>
    /// 비네트 기본값과 순간값을 더해 0~1로 제한한 뒤 프로필에 적용한다.
    /// _vignette 세기를 변경한다.
    /// </summary>
    private void ApplyVignette()
    {
        _vignette.intensity.value = Mathf.Clamp01(_vignetteBase + _vignettePunch);
    }

    /// <summary>
    /// 왜곡 기본값과 순간값을 더해 허용 범위로 제한한 뒤 프로필에 적용한다.
    /// _lensDistortion 세기를 변경한다.
    /// </summary>
    private void ApplyDistortion()
    {
        _lensDistortion.intensity.value = Mathf.Clamp(
            _distortionBase + _distortionPunch, -MAX_DISTORTION_INTENSITY, MAX_DISTORTION_INTENSITY);
    }

    /// <summary>
    /// coroutine이 실행 중이면 중지하고 핸들을 비운다.
    /// 전달된 coroutine 핸들을 null로 변경한다.
    /// </summary>
    private void StopEffectCoroutine(ref Coroutine coroutine)
    {
        if (coroutine == null) return;

        Managers.Instance.StopCoroutine(coroutine);
        coroutine = null;
    }

    /// <summary>
    /// _vignetteBase를 현재값에서 target까지 duration 동안 선형으로 변경한다.
    /// 종료 후 target을 유지하고 _vignetteBaseCoroutine을 null로 되돌린다.
    /// </summary>
    private IEnumerator VignetteBaseCoroutine(float target, float duration)
    {
        float start = _vignetteBase;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _vignetteBase = Mathf.Lerp(start, target, elapsed / duration);
            ApplyVignette();
            yield return null;
        }

        _vignetteBase = target;
        ApplyVignette();
        _vignetteBaseCoroutine = null;
    }

    /// <summary>
    /// _vignettePunch를 peak에서 duration 동안 0까지 줄인다.
    /// 종료 후 순간값을 0으로, _vignettePunchCoroutine을 null로 되돌린다.
    /// </summary>
    private IEnumerator VignettePunchCoroutine(float peak, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _vignettePunch = peak * (1f - Mathf.Clamp01(elapsed / duration));
            ApplyVignette();
            yield return null;
        }

        _vignettePunch = 0f;
        ApplyVignette();
        _vignettePunchCoroutine = null;
    }

    /// <summary>
    /// _distortionBase를 현재값에서 target까지 duration 동안 선형으로 변경한다.
    /// 종료 후 target을 유지하고 _distortionBaseCoroutine을 null로 되돌린다.
    /// </summary>
    private IEnumerator DistortionBaseCoroutine(float target, float duration)
    {
        float start = _distortionBase;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _distortionBase = Mathf.Lerp(start, target, elapsed / duration);
            ApplyDistortion();
            yield return null;
        }

        _distortionBase = target;
        ApplyDistortion();
        _distortionBaseCoroutine = null;
    }

    /// <summary>
    /// duration 동안 strength 세기와 waveCount 주기의 감쇠 사인파를 _distortionPunch에 적용한다.
    /// 종료 후 순간값을 0으로, _distortionPunchCoroutine을 null로 되돌린다.
    /// </summary>
    private IEnumerator DistortionPunchCoroutine(float strength, float duration, int waveCount)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);

            _distortionPunch = Mathf.Sin(progress * Mathf.PI * 2f * waveCount) * strength * (1f - progress);
            ApplyDistortion();
            yield return null;
        }

        _distortionPunch = 0f;
        ApplyDistortion();
        _distortionPunchCoroutine = null;
    }
}
