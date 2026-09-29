using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public enum lightState
{
    green,
    red,
    blink
}

public class LightController : MonoBehaviour
{
    public lightState _lightState;
    public float blinkInterval = 1f;

    public Renderer _renderer;
    public Light _light;
    public Color red = new Color(239f / 255f, 62f / 255f, 62f / 255f);
    public Color green = new Color(52f / 255f, 191f / 255f, 25f / 255f);

    public Color gray = new Color(194f / 255f, 194f / 255f, 194f / 255f);

    private Coroutine blinkCoroutine;

    void OnEnable()
    {
        _renderer = gameObject.GetComponent<Renderer>();
        _light = gameObject.GetComponent<Light>();

        SetLightState(_lightState);
    }

    void OnDisable()
    {
        StopBlink();
    }

    public void SetLightState(lightState state)
    {
        _lightState = state;
        switch (state)
        {
            case lightState.green:
                SetGreen();
                break;
            case lightState.red:
                SetRed();
                break;
            case lightState.blink:
                StartBlink();
                break;
        }
    }

    private void SetGreen()
    {
        if (_lightState != lightState.blink) StopBlink();
        _renderer.material.color = green;
        _light.enabled = true;
        _light.color = green;
    }

    private void SetRed()
    {
        if (_lightState != lightState.blink) StopBlink();
        _renderer.material.color = red;
        _light.enabled = true;
        _light.color = red;
    }

    private void SetGray()
    {
        if (_lightState != lightState.blink) StopBlink();
        _renderer.material.color = gray;
        _light.enabled = false;
    }

    private void StartBlink()
    {
        if (blinkCoroutine != null) return;
        blinkCoroutine = StartCoroutine(Blink());
    }

    private void StopBlink()
    {
        if (blinkCoroutine == null) return;

        StopCoroutine(blinkCoroutine);
        blinkCoroutine = null;
    }

    IEnumerator Blink()
    {
        var wait = new WaitForSeconds(blinkInterval);

        while (true)
        {
            SetRed();
            yield return wait;

            SetGray();
            yield return wait;
        }
    }
}
