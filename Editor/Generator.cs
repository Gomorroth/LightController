using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace gomoru.su
{
    public static class Generator
    {
        public static void Generate(GameObject avatarObject, LightControllerGenerator generator)
        {
            var fx = generator.FX ?? Utils.CreateTemporaryAsset();

            var targets = avatarObject.GetComponentsInChildren<Renderer>(true)
                .Where(x => (x is MeshRenderer || x is SkinnedMeshRenderer) && x.tag != "EditorOnly")
                .Select(x => 
                    (Renderer: x, 
                     Material: x.sharedMaterials
                        .Where(y => y != null)
                        .FirstOrDefault(y => 
                            y.shader.name.IndexOf("lilToon", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            y.shader.EnumeratePropertyNames().Any(z => z == $"_{nameof(LilToonLightParameters.LightMinLimit)}"))
                        ))
                .Where(x => x.Material != null);

        }
    }
}
