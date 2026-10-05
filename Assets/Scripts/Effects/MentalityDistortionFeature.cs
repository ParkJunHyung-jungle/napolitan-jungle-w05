using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MentalityDistortionFeature : FullScreenPassRendererFeature
{
    private const float EFFECT_DAMPING_TIME = 0.5f;

    private static readonly int MENTALITY_STRENGTH = Shader.PropertyToID("_MentalityStrength");
    private static readonly int RED_PROGRESS = Shader.PropertyToID("_RedProgress");
    private static readonly int WAVE_TIME = Shader.PropertyToID("_WaveTime");

    [Header("Mentality Distortion")]
    [SerializeField, Range(0.01f, 1f)] private float _distortionStartRatio = 0.75f;
    [SerializeField, Range(0f, 1f)] private float _distortionInitialStrength = 0.5f;

    [Header("Mentality Red Edge")]
    [SerializeField, Range(0.01f, 1f)] private float _redStartRatio = 0.25f;

    private float _currentStrength;
    private float _strengthVelocity;
    private float _currentRedProgress = -1f;
    private float _redProgressVelocity;

    /// <summary>
    /// 현재 카메라에 전체 화면 왜곡 패스를 추가하면서 정신력에 따른 세기를 설정한다.
    /// Managers.Game의 DayEnd 상태, 이상현상, 문 열림과 전등 꺼짐 상태, 정신력 비율을 사용하며 passMaterial의 왜곡과 붉은 효과 상태를 갱신한다.
    /// </summary>
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (passMaterial == null)
            return;

        float strength = 0f;
        float redProgress = -1f;
        if (Application.isPlaying && (Managers.Game.IsDayEnded || Managers.Timeline.HasActiveAnomalies
            || Managers.Timeline.IsDoorLeftOpen || Managers.Timeline.IsLightLeftOff))
        {
            GameManager game = Managers.Game;
            float mentalityRatio = game.CurrentMentality / game.MaxMentality;
            if (game.IsDayEnded || mentalityRatio <= _distortionStartRatio)
            {
                float distortionProgress = 1f - mentalityRatio / _distortionStartRatio;
                strength = Mathf.Lerp(_distortionInitialStrength, 1f, Mathf.Pow(distortionProgress, 0.7f));
            }
            if (mentalityRatio <= _redStartRatio)
                redProgress = Mathf.Pow(1f - mentalityRatio / _redStartRatio, 0.7f);
        }

        _currentStrength = Mathf.SmoothDamp(_currentStrength, strength, ref _strengthVelocity,
            EFFECT_DAMPING_TIME, Mathf.Infinity, Time.unscaledDeltaTime);
        _currentRedProgress = Mathf.SmoothDamp(_currentRedProgress, redProgress, ref _redProgressVelocity,
            EFFECT_DAMPING_TIME, Mathf.Infinity, Time.unscaledDeltaTime);
        passMaterial.SetFloat(MENTALITY_STRENGTH, _currentStrength);
        passMaterial.SetFloat(RED_PROGRESS, _currentRedProgress);
        passMaterial.SetFloat(WAVE_TIME, Time.unscaledTime);
        base.AddRenderPasses(renderer, ref renderingData);
    }
}
