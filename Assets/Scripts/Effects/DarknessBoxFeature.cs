using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Volume 에서 범위와 밀도를 계산한 후, 실제 적용하는 클래스
/// </summary>
public class DarknessBoxFeature : FullScreenPassRendererFeature
{
    private static readonly int BOX_MIN = Shader.PropertyToID("_BoxMin");
    private static readonly int BOX_MAX = Shader.PropertyToID("_BoxMax");
    private static readonly int WORLD_TO_BOX = Shader.PropertyToID("_WorldToBox");
    private static readonly int DENSITY = Shader.PropertyToID("_Density");
    private static readonly int DARKNESS_COLOR = Shader.PropertyToID("_DarknessColor");

    /// <summary>
    /// 활성 DarknessBoxVolume의 BoxCollider 로컬 범위, 월드 변환 역행렬과 밀도를 전달해 현재 카메라에 패스를 추가한다.
    /// 활성 영역이 없으면 패스를 추가하지 않는다.
    /// </summary>
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        //DarknessBoxVolume volume = DarknessBoxVolume.Active;
        //// 해당 조건문이 없으면 우리는 에디터에서 아무 것도 볼 수 없게 됩니다.
        //if (passMaterial == null || volume == null)
        //    return;

        //BoxCollider box = volume.Box;
        //Vector3 halfSize = box.size * 0.5f;
        //passMaterial.SetVector(BOX_MIN, box.center - halfSize);
        //passMaterial.SetVector(BOX_MAX, box.center + halfSize);
        //passMaterial.SetMatrix(WORLD_TO_BOX, volume.transform.worldToLocalMatrix);
        //passMaterial.SetFloat(DENSITY, volume.Density);
        //passMaterial.SetColor(DARKNESS_COLOR, volume.DarknessColor);
        //base.AddRenderPasses(renderer, ref renderingData);
    }

}
