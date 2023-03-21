using System;
using UnityEngine.Rendering.Universal;

[Serializable]
public class CrepuscularRenderer : ScriptableRendererFeature
{
    private CrepuscularPass pass;
    public override void Create() => pass = new CrepuscularPass();
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) => renderer.EnqueuePass(pass);
}