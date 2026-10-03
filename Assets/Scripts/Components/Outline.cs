using UnityEngine;

public class Outline : MonoBehaviour
{
    [SerializeField]
    private uint OutlineMask = 1u << 1;
    [SerializeField]
    private Renderer[] renderers;

    private void Awake()
    {
        SetOutline(false);
    }

    /// <summary>
    /// 현재 오브젝트와 하위 오브젝트의 모든 Renderer를 수집한다.
    /// 비활성 오브젝트를 포함해 renderers 배열을 갱신한다.
    /// </summary>
    [ContextMenu("Collect Child Renderers")]
    private void CollectChildRenderers()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    /// <summary>
    /// 아웃라인 렌더링 레이어를 활성화하거나 비활성화한다.
    /// enabled 값을 사용해 renderers의 OutlineMask 상태를 변경한다.
    /// </summary>
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
