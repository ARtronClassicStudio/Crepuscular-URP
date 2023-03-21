using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public class CrepuscularPass : ScriptableRenderPass
{
    RenderTargetIdentifier source;
    RenderTargetIdentifier destinationA;
    RenderTargetIdentifier destinationB;
    RenderTargetIdentifier latestDest;

    readonly int temporaryRTIdA = Shader.PropertyToID("_TempRT");
    readonly int temporaryRTIdB = Shader.PropertyToID("_TempRTB");
    private const string kShaderName = "Hidden/Crepuscular";
    private Material m_Material;

    public CrepuscularPass() => renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        if (Shader.Find(kShaderName) != null)
            m_Material = new Material(Shader.Find(kShaderName));      
        else
            Debug.LogError($"Unable to find shader '{kShaderName}'. Post Process Volume New Post Process Volume is unable to load.");

        RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = 0;
       
        var renderer = renderingData.cameraData.renderer;
        source = renderer.cameraColorTarget;
        cmd.GetTemporaryRT(temporaryRTIdA, descriptor, FilterMode.Bilinear);
        destinationA = new RenderTargetIdentifier(temporaryRTIdA);
        cmd.GetTemporaryRT(temporaryRTIdB, descriptor, FilterMode.Bilinear);
        destinationB = new RenderTargetIdentifier(temporaryRTIdB);
        
    }
    
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {

        CommandBuffer cmd = CommandBufferPool.Get("CrepuscularRenderer");
        cmd.Clear();

        var stack = VolumeManager.instance.stack;

        void BlitTo(Material mat, int pass = 0)
        {
            var first = latestDest;
            var last = first == destinationA ? destinationB : destinationA;
            Blit(cmd, first, last, mat, pass);

            latestDest = last;
        }

        latestDest = source;
        var fx = stack.GetComponent<Crepuscular>();
   

        if (fx.IsActive())
        {

            switch (fx.quality.value)
            {
                case Crepuscular.Quality.Ultra: m_Material.SetFloat(Shader.PropertyToID("_NumSamples"), 1024); break;
                case Crepuscular.Quality.High: m_Material.SetFloat(Shader.PropertyToID("_NumSamples"), 300); break;
                case Crepuscular.Quality.Medium: m_Material.SetFloat(Shader.PropertyToID("_NumSamples"), 150); break;
                case Crepuscular.Quality.Low: m_Material.SetFloat(Shader.PropertyToID("_NumSamples"), 50); break;
            }

            m_Material.SetFloat(Shader.PropertyToID("_Density"), 1);
            m_Material.SetFloat(Shader.PropertyToID("_Weight"), fx.weight.value);
            m_Material.SetFloat(Shader.PropertyToID("_Decay"), 1);
            m_Material.SetFloat(Shader.PropertyToID("_Exposure"), fx.exposure.value);
            m_Material.SetFloat(Shader.PropertyToID("_IlluminationDecay"), fx.illuminationDecay.value);
            if (!fx.useColorDirectional.value)
            {
                m_Material.SetColor(Shader.PropertyToID("_ColorRay"), fx.color.value);
            }

            foreach (var l in renderingData.lightData.visibleLights)
            {
                if (l.lightType == LightType.Directional)
                {
                    m_Material.SetVector(Shader.PropertyToID("_LightPos"), renderingData.cameraData.camera.WorldToViewportPoint(renderingData.cameraData.camera.transform.position - l.light.transform.forward));
                    if (fx.useColorDirectional.value)
                    {
                        m_Material.SetColor(Shader.PropertyToID("_ColorRay"), l.light.color);
                    }
                }
            }

            BlitTo(m_Material);
        }

        Blit(cmd, latestDest, source);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(temporaryRTIdA);
        cmd.ReleaseTemporaryRT(temporaryRTIdB);
    }


}
