using System.Collections;
using UnityEngine;

public class EmergencyLightController : MonoBehaviour
{
    [SerializeField] private LegacyGameManager _gameManager;
    [SerializeField] private SystemTimer _systemTimer;

    public Light emergencyLight;

    public float lightDuration = 2f;

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
        if (emergencyLight == null) return;

        if (_emergencyLightCoroutine != null) return;

        _emergencyLightCoroutine = StartCoroutine(EmergencyLightCoroutine());
    }

    public void StopEmergencyLight()
    {
        if (_emergencyLightCoroutine == null) return;

        StopCoroutine(_emergencyLightCoroutine);
        emergencyLight.intensity = 0f;
        _emergencyLightCoroutine = null;
    }

    IEnumerator EmergencyLightCoroutine()
    {
        emergencyLight.intensity = 10f;
        yield return new WaitForSeconds(lightDuration);

        emergencyLight.intensity = 0f;
        _emergencyLightCoroutine = null;
    }
}
