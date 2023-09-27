using nadena.dev.ndmf;
using UnityEngine;

[assembly: ExportsPlugin(typeof(gomoru.su.LightController.LightControllerCore))]

namespace gomoru.su.LightController
{
    partial class LightControllerCore : Plugin<LightControllerCore>
    {
        public override string DisplayName => "Light Controller";
        public override string QualifiedName => "gomoru.su.light-controller";

        private const string ModularAvatarQualifiedName = "nadena.dev.modular-avatar";

        protected override void Configure()
        {
            InPhase(BuildPhase.Transforming)
            .BeforePlugin(ModularAvatarQualifiedName)
            .Run(new GeneratePass());
        }

        private sealed class GeneratePass : Pass<GeneratePass>
        {
            public override string DisplayName => "Light Controller";

            protected override void Execute(BuildContext context)
            {
                var controller = context.AvatarRootObject.GetComponentInChildren<LightController>();
                if (controller != null)
                {
                    Generate(context, controller);
                    GameObject.DestroyImmediate(controller);
                }
            }
        }
    }
}
