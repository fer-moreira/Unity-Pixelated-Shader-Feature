using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelatedRenderPass : ScriptableRendererFeature
{
    class PixelEffectRenderPass : ScriptableRenderPass
    {
        RenderTargetIdentifier tempTexture, sourceTexture;
        private Material material;

        public PixelEffectRenderPass(Material material) : base()
        {
            this.material = material;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            sourceTexture = renderingData.cameraData.renderer.cameraColorTarget;
            tempTexture = RTHandles.Alloc(new RenderTargetIdentifier("_TempTexture"), name: "_TempTexture");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("PixelatedFX");

            RenderTextureDescriptor targetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            targetDescriptor.depthBufferBits = 0;
            cmd.GetTemporaryRT(Shader.PropertyToID("_TempTexture"), targetDescriptor, FilterMode.Bilinear);

            Blit(cmd, sourceTexture, tempTexture, material);
            Blit(cmd, tempTexture, sourceTexture);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    private PixelEffectRenderPass m_RenderPass;
    public Material m_Material;
    public RenderPassEvent m_RenderEvent = RenderPassEvent.BeforeRenderingPostProcessing;



    public override void Create()
    {
        m_RenderPass = new PixelEffectRenderPass(m_Material);
        m_RenderPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_RenderPass);
    }
}


