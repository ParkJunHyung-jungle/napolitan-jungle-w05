using UnityEngine;

public class Outline : MonoBehaviour
{
    private const uint OutlineMask = 1u << 1;

    [SerializeField] private Renderer[] renderers;

    public void SetOutline(bool enabled)
    {
        foreach (Renderer target in renderers)
        {
            if (target == null)
                continue;

            if (enabled)
                target.renderingLayerMask |= OutlineMask;
            else
                target.renderingLayerMask &= ~OutlineMask;
        }
    }
}
