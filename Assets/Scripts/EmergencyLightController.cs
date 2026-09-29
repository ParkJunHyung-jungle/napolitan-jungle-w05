using UnityEngine;
using System.Collections;

public class EmergencyLightController : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private SystemTimer _systemTimer;

    private bool isLightExists = false;
    public float lightDuration = 1.5f;

    private Coroutine _emergencyLightCoroutine = null;

    void OnEnable()
    {
        _systemTimer.OnTimerEnd += StartEmergencyLight;
    }

    void OnDestroy()
    {
        _systemTimer.OnTimerEnd -= StartEmergencyLight;
    }

    public void StartEmergencyLight()
    {
        if (!isLightExists) return;

        if (_emergencyLightCoroutine != null) return;

        _emergencyLightCoroutine = StartCoroutine(EmergencyLightCoroutine());
    }

    public void StopEmergencyLight()
    {
        if (_emergencyLightCoroutine == null) return;

        StopCoroutine(_emergencyLightCoroutine);
        _emergencyLightCoroutine = null;
    }

    IEnumerator EmergencyLightCoroutine()
    {
        gameObject.SetActive(true);
        isLightExists = true;
        yield return new WaitForSeconds(lightDuration);

        gameObject.SetActive(false);
        _emergencyLightCoroutine = null;
        isLightExists = false;
    }
}
