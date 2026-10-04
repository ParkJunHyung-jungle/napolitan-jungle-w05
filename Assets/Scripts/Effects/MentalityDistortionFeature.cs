using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MentalityDistortionFeature : FullScreenPassRendererFeature
{
    private static readonly int MENTALITY_STRENGTH = Shader.PropertyToID("_MentalityStrength");
    private static readonly int RED_PROGRESS = Shader.PropertyToID("_RedProgress");
    private static readonly int WAVE_TIME = Shader.PropertyToID("_WaveTime");

    [Header("Mentality Distortion")]
    [SerializeField, Range(0.01f, 1f)] private float _distortionStartRatio = 0.75f;
    [SerializeField, Range(0f, 1f)] private float _distortionInitialStrength = 0.5f;

    [Header("Mentality Red Edge")]
    [SerializeField, Range(0.01f, 1f)] private float _redStartRatio = 0.25f;

    /// <summary>
    /// 현재 카메라에 전체 화면 왜곡 패스를 추가하면서 정신력에 따른 세기를 설정한다.
    /// Managers.Game의 현재/최대 정신력과 시작 비율을 사용하며 passMaterial의 왜곡과 붉은 효과 상태를 갱신한다.
    /// </summary>
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (passMaterial == null)
            return;

        float strength = 0f;
        float redProgress = -1f;
        if (Application.isPlaying && Managers.Timeline.HasActiveAnomalies)
        {
            GameManager game = Managers.Game;
            float mentalityRatio = game.CurrentMentality / game.MaxMentality;
            if (mentalityRatio <= _distortionStartRatio)
                strength = Mathf.Lerp(_distortionInitialStrength, 1f, 1f - mentalityRatio / _distortionStartRatio);
            if (mentalityRatio <= _redStartRatio)
                redProgress = 1f - mentalityRatio / _redStartRatio;
        }

        passMaterial.SetFloat(MENTALITY_STRENGTH, strength);
        passMaterial.SetFloat(RED_PROGRESS, redProgress);
        passMaterial.SetFloat(WAVE_TIME, Time.unscaledTime);
        base.AddRenderPasses(renderer, ref renderingData);
    }
}
