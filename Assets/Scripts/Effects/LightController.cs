using System;
using System.Collections;

using UnityEngine;

public class LightController : MonoBehaviour
{
    private const float BLINK_ON_THRESHOLD = 0f;

    [Header("Light")]
    [SerializeField] private Light _light;

    [Header("Blink")]
    [SerializeField] private AnimationCurve _blinkCurve;
    private Coroutine _blinkCoroutine;
    public event Action<bool> OnBlinkToggled;

    public void SetColor(Color color)
    {
        _light.color = color;
    }

    public void TurnOn()
    {
        StopBlink();
        _light.enabled = true;
    }

    public void TurnOff()
    {
        StopBlink();
        _light.enabled = false;
    }

    public void Blink()
    {
        StopBlink();
        _blinkCoroutine = StartCoroutine(BlinkCoroutine());
    }

    private void StopBlink()
    {
        if (_blinkCoroutine == null) return;

        StopCoroutine(_blinkCoroutine);
        _blinkCoroutine = null;
    }
    public void SetIntensity(float intensity)
    {
        _light.intensity = intensity;
    }

    /// <summary>
    /// _blinkCurve를 반복 평가해 조명의 켜짐 상태를 바꾼다.
    /// 켜짐 상태가 바뀐 프레임에만 _light.enabled를 변경하고 OnBlinkToggled로 바뀐 상태를 알린다.
    /// </summary>
    private IEnumerator BlinkCoroutine()
    {
        float duration = _blinkCurve.keys[_blinkCurve.length - 1].time;
        float elapsed = 0f;

        while (true)
        {
            elapsed = Mathf.Repeat(elapsed + Time.deltaTime, duration);
            bool isOn = _blinkCurve.Evaluate(elapsed) >= BLINK_ON_THRESHOLD;

            // 매 프레임 알리면 구독한 루프 사운드가 계속 다시 시작되므로 상태가 바뀐 순간에만 알린다.
            if (_light.enabled != isOn)
            {
                _light.enabled = isOn;
                OnBlinkToggled?.Invoke(isOn);
            }

            yield return null;
        }
    }
}
