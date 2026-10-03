using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public sealed class RenderingLayerRenderObjectsFeature : ScriptableRendererFeature
{
    [Header("Filter")]
    [SerializeField] private LayerMask gameObjectLayerMask = ~0;
    [SerializeField] private uint renderingLayerMask = 1u << 1;

    [Header("Outline")]
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    [SerializeField] private CompareFunction depthCompareFunction = CompareFunction.LessEqual;

    private OutlinePass _outlinePass;

    public override void Create()
    {
        _outlinePass = new OutlinePass(
            gameObjectLayerMask,
            renderingLayerMask,
            outlineMaterial,
            depthCompareFunction)
        {
            renderPassEvent = renderPassEvent
        };
    }

    public override void AddRenderPasses(
        ScriptableRenderer renderer,
        ref RenderingData renderingData)
    {
        if (outlineMaterial == null)
            return;

        renderer.EnqueuePass(_outlinePass);
    }

    private sealed class OutlinePass : ScriptableRenderPass
    {
        private static readonly List<ShaderTagId> ShaderTagIds = new()
        {
            new ShaderTagId("SRPDefaultUnlit"),
            new ShaderTagId("UniversalForward"),
            new ShaderTagId("UniversalForwardOnly")
        };

        private readonly LayerMask _gameObjectLayerMask;
        private readonly uint _renderingLayerMask;
        private readonly Material _outlineMaterial;
        private readonly RenderStateBlock _renderStateBlock;

        public OutlinePass(
            LayerMask gameObjectLayerMask,
            uint renderingLayerMask,
            Material outlineMaterial,
            CompareFunction depthCompareFunction)
        {
            _gameObjectLayerMask = gameObjectLayerMask;
            _renderingLayerMask = renderingLayerMask;
            _outlineMaterial = outlineMaterial;

            profilingSampler = new ProfilingSampler("Rendering Layer Outline");

            // Outline은 depth를 검사하지만 depth 자체는 쓰지 않는다.
            _renderStateBlock = new RenderStateBlock(RenderStateMask.Depth)
            {
                depthState = new DepthState(
                    writeEnabled: false,
                    compareFunction: depthCompareFunction)
            };
        }

        public override void RecordRenderGraph(
            RenderGraph renderGraph,
            ContextContainer frameData)
        {
            UniversalResourceData resourceData =
                frameData.Get<UniversalResourceData>();

            UniversalRenderingData renderingData =
                frameData.Get<UniversalRenderingData>();

            UniversalCameraData cameraData =
                frameData.Get<UniversalCameraData>();

            UniversalLightData lightData =
                frameData.Get<UniversalLightData>();

            using IRasterRenderGraphBuilder builder =
                renderGraph.AddRasterRenderPass<PassData>(
                    "Rendering Layer Outline",
                    out PassData passData,
                    profilingSampler);

            DrawingSettings drawingSettings =
                RenderingUtils.CreateDrawingSettings(
                    ShaderTagIds,
                    renderingData,
                    cameraData,
                    lightData,
                    cameraData.defaultOpaqueSortFlags);

            // 원래 Material 대신 Outline Material로 이 추가 패스를 렌더링한다.
            drawingSettings.overrideMaterial = _outlineMaterial;
            drawingSettings.overrideMaterialPassIndex = 0;

            FilteringSettings filteringSettings =
                new FilteringSettings(
                    RenderQueueRange.opaque,
                    _gameObjectLayerMask);

            // 핵심:
            // Outline Rendering Layer bit가 켜진 Renderer만 선택한다.
            filteringSettings.renderingLayerMask = _renderingLayerMask;

            RendererListParams rendererListParams =
                new RendererListParams(
                    renderingData.cullResults,
                    drawingSettings,
                    filteringSettings)
                {
                    tagValues = new NativeArray<ShaderTagId>(
                        new[] { ShaderTagId.none },
                        Allocator.Temp),

                    stateBlocks = new NativeArray<RenderStateBlock>(
                        new[] { _renderStateBlock },
                        Allocator.Temp),

                    isPassTagName = false
                };

            passData.rendererList =
                renderGraph.CreateRendererList(rendererListParams);

            if (!passData.rendererList.IsValid())
                return;

            builder.UseRendererList(passData.rendererList);

            builder.SetRenderAttachment(
                resourceData.activeColorTexture,
                0,
                AccessFlags.Write);

            builder.SetRenderAttachmentDepth(
                resourceData.activeDepthTexture,
                AccessFlags.ReadWrite);

            builder.SetRenderFunc(static (
                PassData data,
                RasterGraphContext context) =>
            {
                context.cmd.DrawRendererList(data.rendererList);
            });
        }

        private sealed class PassData
        {
            public RendererListHandle rendererList;
        }
    }
}
