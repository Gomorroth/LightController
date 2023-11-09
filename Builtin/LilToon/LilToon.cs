using gomoru.su.LightController.API;
using UnityEngine;

[AddComponentMenu("Light Controller/LilToon")]
public sealed partial class LilToon : ShaderSettings
{
    [Range(1f, 10f)]
    public float LightMaxLimitMax = 1;

    [Range(0, 1)]
    public float DistanceFadeEndMax = 1f;

    public LightingGroup Lighting = new LightingGroup();

    public BacklightGroup Backlight = new BacklightGroup();
}