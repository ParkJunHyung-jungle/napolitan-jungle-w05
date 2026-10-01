using UnityEngine;

public class EmergencyLightController : MonoBehaviour
{
    [Header("Emergency Light")]
    public Light emergencyLight;

    void OnEnable()
    {
        Managers.Light.RegisterEmergencyLight(this, emergencyLight);
    }

    void OnDisable()
    {
        Managers.Light.UnregisterEmergencyLight(this);
    }
}
