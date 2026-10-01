using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class LightManager
{
    private const float EMERGENCY_LIGHT_INTENSITY = 10f;

    [Header("Vignette")]
    private VignetteController _vignetteController;
    private PostProcessVolume _vignetteVolume;
    private Vignette _vignette;
    private LensDistortion _distortion;
    private float _maxVignetteIntensity;
    private float _distortionStrength;
    private float _distortionDuration;
    private int _distortionWaveCount;
    private float _distortionElapsed;
    private Coroutine _distortionCoroutine;

    [Header("Emergency Light")]
    private EmergencyLightController _emergencyLightController;
    private Light _emergencyLight;
    private float _emergencyLightDuration;
    private Coroutine _emergencyLightCoroutine;

    [Header("Facility Lights")]
    private readonly Dictionary<int, FacilityLightBinding> _facilityLights = new();

    /// <summary>
    /// 전역 조명 매니저를 초기화한다.
    /// 현재 추가 설정이 없어 상태를 변경하지 않는다.
    /// </summary>
    public void Init()
    {
    }

    /// <summary>
    /// 실행 중인 모든 조명 효과를 중지하고 씬 오브젝트 등록을 해제한다.
    /// 비네트와 비상등을 기본 강도로 되돌리고 시설 조명 매핑을 비운다.
    /// </summary>
    public void Clear()
    {
        ClearVignetteBinding();
        ClearEmergencyLightBinding();

        foreach (FacilityLightBinding binding in _facilityLights.Values)
        {
            StopFacilityBlink(binding);
        }

        _facilityLights.Clear();
    }

    /// <summary>
    /// 씬의 비네트 어댑터와 PostProcessVolume 및 왜곡 설정값을 등록한다.
    /// controller와 volume을 새 바인딩으로 저장하고 기존 왜곡 효과를 종료한다.
    /// </summary>
    internal void RegisterVignette(VignetteController controller, PostProcessVolume volume,
        float maxVignetteIntensity, float distortionStrength, float distortionDuration,
        int distortionWaveCount)
    {
        if (controller == null || volume == null) return;

        ClearVignetteBinding();

        _vignetteController = controller;
        _vignetteVolume = volume;
        _maxVignetteIntensity = maxVignetteIntensity;
        _distortionStrength = distortionStrength;
        _distortionDuration = distortionDuration;
        _distortionWaveCount = distortionWaveCount;

        PostProcessProfile profile = volume.profile;
        if (profile == null) return;

        if (profile.TryGetSettings(out Vignette vignette))
        {
            _vignette = vignette;
        }

        if (profile.TryGetSettings(out LensDistortion distortion))
        {
            _distortion = distortion;
        }
    }

    /// <summary>
    /// 비네트 어댑터가 비활성화될 때 자신과 일치하는 등록만 해제한다.
    /// controller가 현재 바인딩이면 비네트와 왜곡 강도를 0으로 초기화한다.
    /// </summary>
    internal void UnregisterVignette(VignetteController controller)
    {
        if (!object.ReferenceEquals(_vignetteController, controller)) return;

        ClearVignetteBinding();
    }

    /// <summary>
    /// 타이머 진행도와 활성 상태를 사용해 등록된 비네트 강도를 갱신한다.
    /// isActive가 거짓이면 0으로, 참이면 progress를 설정된 최대 강도로 제한해 적용한다.
    /// </summary>
    public void SetVignetteIntensity(float progress, bool isActive)
    {
        if (!IsVignetteBindingAlive())
        {
            ClearVignetteBinding();
            return;
        }

        if (_vignette == null) return;

        _vignette.intensity.value = isActive
            ? Mathf.Clamp(progress, 0f, _maxVignetteIntensity)
            : 0f;
    }

    /// <summary>
    /// 타이머 실패 한 번에 비네트 왜곡과 비상등 펄스를 함께 요청한다.
    /// 이미 실행 중인 각 효과는 중복 코루틴을 만들지 않고 기존 효과를 유지한다.
    /// </summary>
    public void TriggerTimerFailure()
    {
        StartVignetteDistortion();
        StartEmergencyLightPulse();
    }

    /// <summary>
    /// 씬의 비상등 어댑터와 Light 및 지속 시간을 등록한다.
    /// controller의 새 바인딩을 저장하고 기존 펄스와 광도를 초기화한다.
    /// </summary>
    internal void RegisterEmergencyLight(EmergencyLightController controller, Light emergencyLight,
        float lightDuration)
    {
        if (controller == null || emergencyLight == null) return;

        ClearEmergencyLightBinding();

        _emergencyLightController = controller;
        _emergencyLight = emergencyLight;
        _emergencyLightDuration = lightDuration;
    }

    /// <summary>
    /// 비상등 어댑터가 비활성화될 때 자신과 일치하는 등록만 해제한다.
    /// controller가 현재 바인딩이면 펄스를 중지하고 비상등 광도를 0으로 만든다.
    /// </summary>
    internal void UnregisterEmergencyLight(EmergencyLightController controller)
    {
        if (!object.ReferenceEquals(_emergencyLightController, controller)) return;

        ClearEmergencyLightBinding();
    }

    /// <summary>
    /// 시설 ID에 연결된 조명 어댑터와 렌더러, Light, 색상 설정을 등록한다.
    /// facilityID의 기존 깜빡임을 중지하고 controller의 현재 상태를 즉시 적용한다.
    /// </summary>
    internal void RegisterFacilityLight(int facilityID, LightController controller)
    {
        if (controller == null || controller._renderer == null || controller._light == null) return;

        if (_facilityLights.TryGetValue(facilityID, out FacilityLightBinding previousBinding))
        {
            StopFacilityBlink(previousBinding);
        }

        FacilityLightBinding binding = new FacilityLightBinding(controller);
        _facilityLights[facilityID] = binding;
        ApplyFacilityLightState(facilityID, binding, controller._lightState);
    }

    /// <summary>
    /// 시설 조명 어댑터가 비활성화될 때 ID와 controller가 모두 일치하는 등록만 해제한다.
    /// 일치하는 바인딩의 깜빡임 코루틴을 중지하고 시설 매핑에서 제거한다.
    /// </summary>
    internal void UnregisterFacilityLight(int facilityID, LightController controller)
    {
        if (!_facilityLights.TryGetValue(facilityID, out FacilityLightBinding binding)) return;
        if (!object.ReferenceEquals(binding.Controller, controller)) return;

        StopFacilityBlink(binding);
        _facilityLights.Remove(facilityID);
    }

    /// <summary>
    /// 시설 ID와 lightState를 사용해 등록된 표시등을 초록, 빨강 또는 깜빡임으로 전환한다.
    /// 적용한 상태를 어댑터에 저장하며 blink 상태의 코루틴은 전역 Managers가 소유한다.
    /// </summary>
    public void SetFacilityLight(int facilityID, lightState state)
    {
        if (!_facilityLights.TryGetValue(facilityID, out FacilityLightBinding binding)) return;

        if (!IsFacilityLightBindingAlive(binding))
        {
            StopFacilityBlink(binding);
            _facilityLights.Remove(facilityID);
            return;
        }

        binding.Controller._lightState = state;
        ApplyFacilityLightState(facilityID, binding, state);
    }

    /// <summary>
    /// 등록된 비네트 어댑터와 볼륨이 현재 씬에서 유효한지 확인한다.
    /// 두 Unity 오브젝트가 모두 살아 있으면 true를 반환한다.
    /// </summary>
    private bool IsVignetteBindingAlive()
    {
        return _vignetteController != null && _vignetteVolume != null;
    }

    /// <summary>
    /// 실행 중인 왜곡 코루틴과 Post Processing 강도를 초기화한다.
    /// 현재 비네트 씬 바인딩과 모든 왜곡 설정 참조를 해제한다.
    /// </summary>
    private void ClearVignetteBinding()
    {
        StopVignetteDistortion();

        if (_vignette != null)
        {
            _vignette.intensity.value = 0f;
        }

        _vignetteController = null;
        _vignetteVolume = null;
        _vignette = null;
        _distortion = null;
        _maxVignetteIntensity = 0f;
        _distortionStrength = 0f;
        _distortionDuration = 0f;
        _distortionWaveCount = 0;
    }

    /// <summary>
    /// 등록된 왜곡 설정이 유효하고 미실행 상태일 때 파형 코루틴을 시작한다.
    /// 생성된 Coroutine 핸들을 _distortionCoroutine에 저장한다.
    /// </summary>
    private void StartVignetteDistortion()
    {
        if (!IsVignetteBindingAlive())
        {
            ClearVignetteBinding();
            return;
        }

        if (_distortion == null || _distortionCoroutine != null) return;

        _distortionCoroutine = Managers.Instance.StartCoroutine(DistortionCoroutine());
    }

    /// <summary>
    /// 실행 중인 비네트 왜곡 코루틴을 중지하고 경과 시간과 왜곡 강도를 초기화한다.
    /// 저장된 Coroutine 핸들을 비워 다음 타이머 실패가 새 효과를 시작하게 한다.
    /// </summary>
    private void StopVignetteDistortion()
    {
        if (_distortionCoroutine != null)
        {
            Managers.Instance.StopCoroutine(_distortionCoroutine);
        }

        _distortionCoroutine = null;
        _distortionElapsed = 0f;

        if (_distortion != null)
        {
            _distortion.intensity.value = 0f;
        }
    }

    /// <summary>
    /// Time.deltaTime으로 감쇠 사인파를 계산해 등록된 LensDistortion에 적용한다.
    /// 설정된 지속 시간이 끝나면 왜곡 강도와 Coroutine 상태를 0과 null로 되돌린다.
    /// </summary>
    private IEnumerator DistortionCoroutine()
    {
        while (_distortionElapsed < _distortionDuration)
        {
            if (!IsVignetteBindingAlive() || _distortion == null)
            {
                _distortionCoroutine = null;
                _distortionElapsed = 0f;
                yield break;
            }

            _distortionElapsed += Time.deltaTime;
            float progress = _distortionElapsed / _distortionDuration;
            float fade = 1f - progress;
            float offset = Mathf.Sin(progress * Mathf.PI * 2f * _distortionWaveCount)
                * _distortionStrength * fade;

            _distortion.intensity.value = offset;
            yield return null;
        }

        _distortionCoroutine = null;
        _distortionElapsed = 0f;

        if (_distortion != null)
        {
            _distortion.intensity.value = 0f;
        }
    }

    /// <summary>
    /// 등록된 비상등 어댑터와 Light가 현재 씬에서 유효한지 확인한다.
    /// 두 Unity 오브젝트가 모두 살아 있으면 true를 반환한다.
    /// </summary>
    private bool IsEmergencyLightBindingAlive()
    {
        return _emergencyLightController != null && _emergencyLight != null;
    }

    /// <summary>
    /// 실행 중인 비상등 펄스를 중지하고 광도를 0으로 초기화한다.
    /// 현재 비상등 씬 바인딩과 지속 시간 설정을 해제한다.
    /// </summary>
    private void ClearEmergencyLightBinding()
    {
        StopEmergencyLightPulse();
        _emergencyLightController = null;
        _emergencyLight = null;
        _emergencyLightDuration = 0f;
    }

    /// <summary>
    /// 등록된 비상등이 유효하고 미실행 상태일 때 펄스 코루틴을 시작한다.
    /// 생성된 Coroutine 핸들을 _emergencyLightCoroutine에 저장한다.
    /// </summary>
    private void StartEmergencyLightPulse()
    {
        if (!IsEmergencyLightBindingAlive())
        {
            ClearEmergencyLightBinding();
            return;
        }

        if (_emergencyLightCoroutine != null) return;

        _emergencyLightCoroutine = Managers.Instance.StartCoroutine(EmergencyLightCoroutine());
    }

    /// <summary>
    /// 실행 중인 비상등 펄스 코루틴을 중지하고 등록된 Light 광도를 0으로 만든다.
    /// 저장된 Coroutine 핸들을 비워 다음 타이머 실패가 새 펄스를 시작하게 한다.
    /// </summary>
    private void StopEmergencyLightPulse()
    {
        if (_emergencyLightCoroutine != null)
        {
            Managers.Instance.StopCoroutine(_emergencyLightCoroutine);
        }

        _emergencyLightCoroutine = null;

        if (_emergencyLight != null)
        {
            _emergencyLight.intensity = 0f;
        }
    }

    /// <summary>
    /// 등록된 비상등 광도를 10으로 설정하고 scaled time 기준 지속 시간만큼 유지한다.
    /// 대기 후 살아 있는 Light를 0으로 되돌리고 Coroutine 상태를 비운다.
    /// </summary>
    private IEnumerator EmergencyLightCoroutine()
    {
        Light emergencyLight = _emergencyLight;
        emergencyLight.intensity = EMERGENCY_LIGHT_INTENSITY;

        yield return new WaitForSeconds(_emergencyLightDuration);

        if (emergencyLight != null)
        {
            emergencyLight.intensity = 0f;
        }

        if (object.ReferenceEquals(_emergencyLight, emergencyLight))
        {
            _emergencyLightCoroutine = null;
        }
    }

    /// <summary>
    /// 시설 조명 바인딩의 어댑터, Renderer, Light가 모두 유효한지 확인한다.
    /// 모든 Unity 오브젝트가 살아 있으면 true를 반환한다.
    /// </summary>
    private bool IsFacilityLightBindingAlive(FacilityLightBinding binding)
    {
        return binding.Controller != null && binding.Renderer != null && binding.Light != null;
    }

    /// <summary>
    /// 시설 조명 상태를 적용하기 전에 기존 깜빡임을 중지한다.
    /// state에 따라 초록, 빨강 또는 manager 소유 깜빡임으로 외형을 변경한다.
    /// </summary>
    private void ApplyFacilityLightState(int facilityID, FacilityLightBinding binding, lightState state)
    {
        StopFacilityBlink(binding);

        switch (state)
        {
            case lightState.green:
                ApplyFacilityLightColor(binding, binding.Green, binding.Green, true);
                break;
            case lightState.red:
                ApplyFacilityLightColor(binding, binding.Red, binding.Red, true);
                break;
            case lightState.blink:
                binding.BlinkCoroutine = Managers.Instance.StartCoroutine(
                    FacilityBlinkCoroutine(facilityID, binding));
                break;
        }
    }

    /// <summary>
    /// Renderer material의 기본색과 emissionColor를 바꾸고 Light 활성 및 색상을 설정한다.
    /// lightEnabled가 거짓이면 emission을 끄고 Light를 비활성화한다.
    /// </summary>
    private void ApplyFacilityLightColor(FacilityLightBinding binding, Color materialColor,
        Color emissionColor, bool lightEnabled)
    {
        Material material = binding.Renderer.material;
        material.color = materialColor;
        material.SetColor("_EmissionColor", emissionColor);

        binding.Light.enabled = lightEnabled;
        if (lightEnabled)
        {
            binding.Light.color = materialColor;
        }
    }

    /// <summary>
    /// 시설 조명의 manager 소유 깜빡임 코루틴을 중지하고 Coroutine 핸들을 비운다.
    /// 실행 중인 코루틴이 없으면 현재 표시 상태를 그대로 유지한다.
    /// </summary>
    private void StopFacilityBlink(FacilityLightBinding binding)
    {
        if (binding.BlinkCoroutine == null) return;

        Managers.Instance.StopCoroutine(binding.BlinkCoroutine);

        binding.BlinkCoroutine = null;
    }

    /// <summary>
    /// blinkInterval의 scaled time 간격으로 빨강과 회색을 반복 적용한다.
    /// facilityID가 다른 어댑터로 교체되거나 Unity 오브젝트가 파괴되면 등록을 정리한다.
    /// </summary>
    private IEnumerator FacilityBlinkCoroutine(int facilityID, FacilityLightBinding binding)
    {
        WaitForSeconds wait = new WaitForSeconds(binding.BlinkInterval);

        while (IsCurrentFacilityLightBinding(facilityID, binding)
            && IsFacilityLightBindingAlive(binding))
        {
            ApplyFacilityLightColor(binding, binding.Red, binding.Red, true);
            yield return wait;

            if (!IsCurrentFacilityLightBinding(facilityID, binding)
                || !IsFacilityLightBindingAlive(binding))
            {
                break;
            }

            ApplyFacilityLightColor(binding, binding.Gray, Color.black, false);
            yield return wait;
        }

        if (IsCurrentFacilityLightBinding(facilityID, binding))
        {
            binding.BlinkCoroutine = null;
            _facilityLights.Remove(facilityID);
        }
    }

    /// <summary>
    /// facilityID가 여전히 전달된 binding을 가리키는지 확인한다.
    /// 동일한 바인딩이면 true를 반환해 이전 씬 코루틴의 후속 변경을 차단한다.
    /// </summary>
    private bool IsCurrentFacilityLightBinding(int facilityID, FacilityLightBinding binding)
    {
        return _facilityLights.TryGetValue(facilityID, out FacilityLightBinding currentBinding)
            && object.ReferenceEquals(currentBinding, binding);
    }

    private sealed class FacilityLightBinding
    {
        public LightController Controller { get; }
        public Renderer Renderer { get; }
        public Light Light { get; }
        public Color Red { get; }
        public Color Green { get; }
        public Color Gray { get; }
        public float BlinkInterval { get; }
        public Coroutine BlinkCoroutine { get; set; }

        /// <summary>
        /// LightController의 하드웨어와 색상 및 깜빡임 설정을 불변 바인딩으로 복사한다.
        /// 생성된 바인딩은 manager의 시설 ID 매핑과 Coroutine 상태를 보관한다.
        /// </summary>
        public FacilityLightBinding(LightController controller)
        {
            Controller = controller;
            Renderer = controller._renderer;
            Light = controller._light;
            Red = controller.red;
            Green = controller.green;
            Gray = controller.gray;
            BlinkInterval = controller.blinkInterval;
        }
    }
}
