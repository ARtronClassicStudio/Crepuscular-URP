using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable,VolumeComponentMenuForRenderPipeline("Crepuscular", typeof(UniversalRenderPipeline))]
public class Crepuscular : VolumeComponent, IPostProcessComponent
{

    public BoolParameter enabled = new(false, false);
    public VolumeParameter<Quality> quality = new() { value = Quality.High};
    public ClampedFloatParameter weight = new(1, 0, 1);
    public ClampedFloatParameter exposure = new(0.5f, 0, 1);
    public ClampedFloatParameter illuminationDecay = new(1, 0, 10);

    public enum Quality
    {
        Ultra,
        High,
        Medium,
        Low
    }

    public bool IsActive() => enabled.value;

    public bool IsTileCompatible() => true;
}