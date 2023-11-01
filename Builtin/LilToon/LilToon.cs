using gomoru.su.LightController.API;
using gomoru.su.LightController.API.Attributes;
using UnityEngine;

public sealed partial class LilToon : ShaderSettings
{
    [Range(1f, 10f)]
    public float LightMaxLimitMax = 1;

    [Range(0, 1)]
    public float DistanceFadeEndMax = 1f;


    public LightingGroup Lighting = new LightingGroup();
}