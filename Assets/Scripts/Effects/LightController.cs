using System.Collections;

using UnityEngine;

public class LightController : MonoBehaviour
{
    private static readonly Color OFF_COLOR = new Color(194f / 255f, 194f / 255f, 194f / 255f);

    [Header("Hardware")]
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Light _light;
    private Material _material;

    [Header("Blink")]
    [SerializeField] private float _blinkInterval = 1f;
    private Coroutine _blinkCoroutine;

    [Header("Color")]
    private Color _color = Color.white;
    private bool _isOn;

    void Awake()
    {
        _material = _renderer.material;
    }

    /// <summary>
    /// 켜짐에 사용할 색을 color로 변경한다.
    /// _color를 저장하고, 현재 켜져 있으면 즉시 새 색을 적용한다.
    /// </summary>
    public void SetColor(Color color)
    {
        _color = color;

        if (_isOn) ApplyOn();
    }

    /// <summary>
    /// 깜빡임을 멈추고 조명을 켠다.
    /// _material, _light, _isOn을 켜짐 상태로 변경한다.
    /// </summary>
    public void TurnOn()
    {
        StopBlink();
        ApplyOn();
    }

    /// <summary>
    /// 깜빡임을 멈추고 조명을 끈다.
    /// _material, _light, _isOn을 꺼짐 상태로 변경한다.
    /// </summary>
    public void TurnOff()
    {
        StopBlink();
        ApplyOff();
    }

    /// <summary>
    /// _blinkInterval 간격으로 켜짐과 꺼짐을 반복하는 깜빡임을 시작한다.
    /// 기존 깜빡임을 멈추고 새 Coroutine을 _blinkCoroutine에 저장한다.
    /// </summary>
    public void Blink()
    {
        StopBlink();
        _blinkCoroutine = StartCoroutine(BlinkCoroutine());
    }

    /// <summary>
    /// 현재 _color로 머티리얼 기본색과 발광색, Light 색을 적용하고 Light를 켠다.
    /// _isOn을 true로 변경한다.
    /// </summary>
    private void ApplyOn()
    {
        _material.color = _color;
        _material.SetColor("_EmissionColor", _color);

        _light.color = _color;
        _light.enabled = true;
        _isOn = true;
    }

    /// <summary>
    /// 머티리얼을 꺼짐 색으로 바꾸고 발광과 Light를 끈다.
    /// _isOn을 false로 변경한다.
    /// </summary>
    private void ApplyOff()
    {
        _material.color = OFF_COLOR;
        _material.SetColor("_EmissionColor", Color.black);

        _light.enabled = false;
        _isOn = false;
    }

    /// <summary>
    /// 실행 중인 깜빡임 코루틴을 중지하고 핸들을 비운다.
    /// _blinkCoroutine을 null로 변경한다.
    /// </summary>
    private void StopBlink()
    {
        if (_blinkCoroutine == null) return;

        StopCoroutine(_blinkCoroutine);
        _blinkCoroutine = null;
    }

    /// <summary>
    /// _blinkInterval 간격으로 켜짐과 꺼짐을 반복한다.
    /// 오브젝트가 비활성화되거나 파괴되면 Unity가 자동으로 중지한다.
    /// </summary>
    private IEnumerator BlinkCoroutine()
    {
        WaitForSeconds wait = new WaitForSeconds(_blinkInterval);

        while (true)
        {
            ApplyOn();
            yield return wait;
            ApplyOff();
            yield return wait;
        }
    }
}
