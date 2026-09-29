using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class LightController : MonoBehaviour
{
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
    }

    void OnDisable()
    {
        StopBlink();
    }

    public void SetGreen()
    {
        _renderer.material.color = green;
        _light.enabled = true;
        _light.color = green;
    }

    public void SetRed()
    {
        _renderer.material.color = red;
        _light.enabled = true;
        _light.color = red;
    }

    public void SetGray()
    {
        _renderer.material.color = gray;
        _light.enabled = false;
    }

    public void StartBlink()
    {
        if (blinkCoroutine != null) return;
        blinkCoroutine = StartCoroutine(Blink());
    }

    public void StopBlink()
    {
        if (blinkCoroutine == null) return;

        StopCoroutine(Blink());
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
