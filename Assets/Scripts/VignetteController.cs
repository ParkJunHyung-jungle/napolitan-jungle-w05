using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class VignetteController : MonoBehaviour
{
    public PostProcessVolume volume;
    private bool isVignetteExists = false;
    private bool isDistortionExists = false;
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private SystemTimer _systemTimer;

    public float maxVignetteIntensity = 0.55f;
    public float distortionStrength = 15f;
    public float distortDuration = 0.3f;
    public int distortionWaveCount = 2;
    private float elapsed = 0f;

    private Coroutine _distortCoroutine = null;

    Vignette _vignette;
    LensDistortion _distortion;

    public void Initialize(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    void OnEnable()
    {
        _systemTimer.OnTimerEnd += StartDistortion;
    }

    void OnDestroy()
    {
        _systemTimer.OnTimerEnd -= StartDistortion;
    }

    void Start()
    {
        if (volume.profile.TryGetSettings<Vignette>(out var vignette))
        {
            isVignetteExists = true;
            _vignette = vignette;
        }

        if (volume.profile.TryGetSettings<LensDistortion>(out var distortion))
        {
            isDistortionExists = true;
            _distortion = distortion;
        }
    }

    public void SetVignetteIntensity(float intensity)
    {
        if (!isVignetteExists) return;

        if (_gameManager.SystemTimer.IsActive)
        {
            // vinette 강도 설정. 정상일 때는 0f, 위험할 때는 0.5f까지 - Durability 따라서? or Timer 따라서?
            _vignette.intensity.value = Mathf.Clamp(intensity, 0f, maxVignetteIntensity);
        }
        else
        {
            _vignette.intensity.value = 0f;
        }
    }

    public void StartDistortion()
    {
        if (!isDistortionExists) return;

        if (_distortCoroutine != null) return;

        _distortCoroutine = StartCoroutine(DistortionCoroutine());
    }

    public void StopDistortion()
    {
        if (_distortCoroutine == null) return;

        StopCoroutine(_distortCoroutine);
        _distortCoroutine = null;
        elapsed = 0f;
        _distortion.intensity.value = 0f;

    }

    IEnumerator DistortionCoroutine()
    {
        while (elapsed < distortDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / distortDuration;
            float fade = 1f - progress;

            float offset = Mathf.Sin(progress * Mathf.PI * 2f * distortionWaveCount) * distortionStrength * fade;

            _distortion.intensity.value = offset;

            yield return null;
        }

        _distortCoroutine = null;
        elapsed = 0f;
        _distortion.intensity.value = 0f;
    }

}
