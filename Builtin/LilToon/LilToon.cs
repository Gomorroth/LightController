using gomoru.su.LightController.API;
using UnityEngine;

[AddComponentMenu("Light Controller/Light Controller LilToon Settings")]
public sealed partial class LilToon : ShaderSettings
{
    [Range(1f, 10f)]
    public float LightMaxLimitMax = 1;

    [Range(0, 1)]
    public float DistanceFadeEndMax = 1f;

    public LightingGroup Lighting = new LightingGroup();

    public BacklightGroup Backlight = new BacklightGroup();

    public DistanceFadeGroup DistanceFade = new DistanceFadeGroup();



    public override bool IsTargetMaterial(Material material)
    {
        return material?.shader?.name.IndexOf("lilToon", System.StringComparison.OrdinalIgnoreCase) != 0;
    }

    public override void OnParameterPostProcess(Parameter parameter)
    {
        if (parameter.Name == nameof(LightingGroup.LightMaxLimit))
        {
            parameter.MaxValue = LightMaxLimitMax;
        }
        else if (parameter.Name == $"{DistanceFade.Name}{nameof(DistanceFade.End)}")
        {
            parameter.MaxValue = DistanceFadeEndMax;
        }
    }
}