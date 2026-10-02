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

    private IEnumerator BlinkCoroutine()
    {
        float duration = _blinkCurve.keys[_blinkCurve.length - 1].time;
        float elapsed = 0f;

        while (true)
        {
            elapsed = Mathf.Repeat(elapsed + Time.deltaTime, duration);
            _light.enabled = _blinkCurve.Evaluate(elapsed) >= BLINK_ON_THRESHOLD;
            yield return null;
        }
    }
}
