using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class VignetteController : MonoBehaviour
{
    [Header("References")]
    public PostProcessVolume volume;

    [Header("Vignette Settings")]
    public float maxVignetteIntensity = 0.55f;
    public float distortionStrength = 15f;
    public float distortDuration = 0.3f;
    public int distortionWaveCount = 2;

    void OnEnable()
    {
        Managers.Light.RegisterVignette(this, volume, maxVignetteIntensity, distortionStrength,
            distortDuration, distortionWaveCount);
    }

    void OnDisable()
    {
        Managers.Light.UnregisterVignette(this);
    }
}
