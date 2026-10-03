using System.Collections.Generic;

using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public sealed class RenderingLayerRenderObjectsFeature : ScriptableRendererFeature
{
    private const int OUTLINE_STENCIL_BIT = 1 << 3;

    [Header("Filter")]
    [SerializeField] private LayerMask gameObjectLayerMask = ~0;
    [SerializeField] private uint renderingLayerMask = 1u << 1;

    [Header("Outline")]
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    [SerializeField] private CompareFunction depthCompareFunction = CompareFunction.LessEqual;

    private OutlinePass _outlinePass;
    private Material _outlineMaskMaterial;

    public override void Create()
    {
        CoreUtils.Destroy(_outlineMaskMaterial);
        _outlineMaskMaterial = null;
        _outlinePass = null;

        if (outlineMaterial == null)
            return;

        _outlineMaskMaterial = new Material(outlineMaterial)
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        _outlineMaskMaterial.SetFloat("_Outline_Thickness", 0f);

        _outlinePass = new OutlinePass(
            gameObjectLayerMask,
            renderingLayerMask,
            outlineMaterial,
            _outlineMaskMaterial,
            depthCompareFunction)
        {
            renderPassEvent = renderPassEvent
        };
    }

    public override void AddRenderPasses(
        ScriptableRenderer renderer,
        ref RenderingData renderingData)
    {
        if (outlineMaterial == null || _outlinePass == null)
            return;

        renderer.EnqueuePass(_outlinePass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(_outlineMaskMaterial);
        _outlineMaskMaterial = null;
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
        private readonly Material _outlineMaskMaterial;
        private readonly RenderStateBlock _maskRenderStateBlock;
        private readonly RenderStateBlock _outlineRenderStateBlock;

        public OutlinePass(
            LayerMask gameObjectLayerMask,
            uint renderingLayerMask,
            Material outlineMaterial,
            Material outlineMaskMaterial,
            CompareFunction depthCompareFunction)
        {
            _gameObjectLayerMask = gameObjectLayerMask;
            _renderingLayerMask = renderingLayerMask;
            _outlineMaterial = outlineMaterial;
            _outlineMaskMaterial = outlineMaskMaterial;
            profilingSampler = new ProfilingSampler("Rendering Layer Outline");

            StencilState maskStencilState = new(
                enabled: true,
                readMask: (byte)OUTLINE_STENCIL_BIT,
                writeMask: (byte)OUTLINE_STENCIL_BIT,
                compareFunction: CompareFunction.Always,
                passOperation: StencilOp.Replace,
                failOperation: StencilOp.Keep,
                zFailOperation: StencilOp.Keep)
            {
                compareFunctionBack = CompareFunction.Always,
                passOperationBack = StencilOp.Replace,
                failOperationBack = StencilOp.Keep,
                zFailOperationBack = StencilOp.Keep
            };

            _maskRenderStateBlock = new RenderStateBlock(
                RenderStateMask.Depth |
                RenderStateMask.Stencil |
                RenderStateMask.Raster |
                RenderStateMask.Blend)
            {
                depthState = new DepthState(
                    writeEnabled: false,
                    compareFunction: CompareFunction.Always),
                stencilReference = OUTLINE_STENCIL_BIT,
                stencilState = maskStencilState,
                rasterState = new RasterState(CullMode.Off),
                blendState = CreateMaskBlendState()
            };

            StencilState outlineStencilState = new(
                enabled: true,
                readMask: (byte)OUTLINE_STENCIL_BIT,
                writeMask: 0,
                compareFunction: CompareFunction.NotEqual,
                passOperation: StencilOp.Keep,
                failOperation: StencilOp.Keep,
                zFailOperation: StencilOp.Keep)
            {
                compareFunctionBack = CompareFunction.NotEqual,
                passOperationBack = StencilOp.Keep,
                failOperationBack = StencilOp.Keep,
                zFailOperationBack = StencilOp.Keep
            };

            _outlineRenderStateBlock = new RenderStateBlock(
                RenderStateMask.Depth |
                RenderStateMask.Stencil |
                RenderStateMask.Raster)
            {
                depthState = new DepthState(
                    writeEnabled: false,
                    compareFunction: depthCompareFunction),
                stencilReference = OUTLINE_STENCIL_BIT,
                stencilState = outlineStencilState,
                rasterState = new RasterState(CullMode.Off)
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

            RendererListHandle maskRendererList = CreateRendererList(
                renderGraph,
                renderingData,
                cameraData,
                lightData,
                _outlineMaskMaterial,
                _maskRenderStateBlock);

            RendererListHandle outlineRendererList = CreateRendererList(
                renderGraph,
                renderingData,
                cameraData,
                lightData,
                _outlineMaterial,
                _outlineRenderStateBlock);

            using IRasterRenderGraphBuilder builder =
                renderGraph.AddRasterRenderPass<PassData>(
                    "Rendering Layer Outline Mask and Draw",
                    out PassData passData,
                    profilingSampler);

            passData.maskRendererList = maskRendererList;
            passData.outlineRendererList = outlineRendererList;

            if (!passData.maskRendererList.IsValid() ||
                !passData.outlineRendererList.IsValid())
                return;

            builder.UseRendererList(passData.maskRendererList);
            builder.UseRendererList(passData.outlineRendererList);

            builder.SetRenderAttachment(
                resourceData.activeColorTexture,
                0,
                AccessFlags.ReadWrite);

            builder.SetRenderAttachmentDepth(
                resourceData.activeDepthTexture,
                AccessFlags.ReadWrite);

            builder.SetRenderFunc(static (
                PassData data,
                RasterGraphContext context) =>
            {
                context.cmd.DrawRendererList(data.maskRendererList);
                context.cmd.DrawRendererList(data.outlineRendererList);
            });
        }

        /// <summary>
        /// Builds a renderer list using the outline filters and supplied override material and render state.
        /// renderingData, cameraData, lightData, material, and stateBlock determine the selected renderers and draw behavior.
        /// </summary>
        private RendererListHandle CreateRendererList(
            RenderGraph renderGraph,
            UniversalRenderingData renderingData,
            UniversalCameraData cameraData,
            UniversalLightData lightData,
            Material material,
            RenderStateBlock stateBlock)
        {
            DrawingSettings drawingSettings =
                RenderingUtils.CreateDrawingSettings(
                    ShaderTagIds,
                    renderingData,
                    cameraData,
                    lightData,
                    cameraData.defaultOpaqueSortFlags);

            drawingSettings.overrideMaterial = material;
            drawingSettings.overrideMaterialPassIndex = 0;

            FilteringSettings filteringSettings =
                new FilteringSettings(
                    RenderQueueRange.all,
                    _gameObjectLayerMask)
                {
                    renderingLayerMask = _renderingLayerMask
                };

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
                        new[] { stateBlock },
                        Allocator.Temp),
                    isPassTagName = false
                };

            return renderGraph.CreateRendererList(rendererListParams);
        }

        /// <summary>
        /// Creates a blend state that disables color writes for the original-geometry stencil mask.
        /// No input is required; the returned state preserves the camera color while stencil receives the mask.
        /// </summary>
        private static BlendState CreateMaskBlendState()
        {
            BlendState blendState = new();
            blendState.blendState0 = new RenderTargetBlendState
            {
                writeMask = (ColorWriteMask)0
            };

            return blendState;
        }

        private sealed class PassData
        {
            public RendererListHandle maskRendererList;
            public RendererListHandle outlineRendererList;
        }
    }
}
