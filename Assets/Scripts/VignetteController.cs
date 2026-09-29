using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class VignetteController : MonoBehaviour
{
    public PostProcessVolume volume;
    private bool isVignetteExists = false;
    Vignette _vignette;

    void Start()
    {
        if (volume.profile.TryGetSettings<Vignette>(out var vignette))
        {
            isVignetteExists = true;
            _vignette = vignette;
        }
    }

    public void SetVignetteIntensity(float intensity)
    {
        if (!isVignetteExists) return;
        // vinette 강도 설정. 정상일 때는 0f, 위험할 때는 0.5f까지 - Durability 따라서? or Timer 따라서?
        _vignette.intensity.value = Mathf.Clamp01(intensity);
    }

}
