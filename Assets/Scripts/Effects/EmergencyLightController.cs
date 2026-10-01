using UnityEngine;

public class EmergencyLightController : MonoBehaviour
{
    [Header("Emergency Light")]
    public Light emergencyLight;
    public float lightDuration = 2f;

    void OnEnable()
    {
        Managers.Light.RegisterEmergencyLight(this, emergencyLight, lightDuration);
    }

    void OnDisable()
    {
        Managers.Light.UnregisterEmergencyLight(this);
    }
}
