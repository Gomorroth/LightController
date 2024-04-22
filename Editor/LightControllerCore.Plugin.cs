using System.Collections.Generic;
using nadena.dev.ndmf;

[assembly: ExportsPlugin(typeof(gomoru.su.LightController.LightControllerCore))]

namespace gomoru.su.LightController;

partial class LightControllerCore : Plugin<LightControllerCore>
{
    public override string DisplayName => "Light Controller";

    public override string QualifiedName => "gomoru.su.light-controller";

    private const string ModularAvatarQualifiedName = "nadena.dev.modular-avatar";

    protected override void Configure()
    {
        InPhase(BuildPhase.Generating)
        .BeforePlugin(ModularAvatarQualifiedName)
        .Run(GeneratePass.Instance);
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
                Object.DestroyImmediate(controller);
            }
        }
    }
}

[ParameterProviderFor(typeof(LightController))]
internal sealed class LightControllerParameterProvider : IParameterProvider
{
    private LightController instance;

    public LightControllerParameterProvider(LightController instance) => this.instance = instance;

    public IEnumerable<ProvidedParameter> GetSuppliedParameters(BuildContext context = null)
    {
        if (instance == null)
            yield break;

        yield return Parameter<int>("SyncTargetIndex");
        yield return Parameter<float>("SyncedValue");
        yield return Parameter<bool>("UseBacklight");
        yield return Parameter<bool>("BacklightReceiveShadow");
        yield return Parameter<bool>("BacklightBackfaceMask");
        yield return Parameter<bool>("DistanceFadeBackfaceForceShadow");
    }

    private ProvidedParameter Parameter<T>(string name, bool sync = true, ParameterNamespace @namespace = ParameterNamespace.Animator)
            => Parameter(name, sync, @namespace, typeof(T) == typeof(int) ? AnimatorControllerParameterType.Int : typeof(T) == typeof(float) ? AnimatorControllerParameterType.Float : typeof(T) == typeof(bool) ? AnimatorControllerParameterType.Bool : default);

    private ProvidedParameter Parameter(string name, bool sync = true, ParameterNamespace @namespace = ParameterNamespace.Animator, AnimatorControllerParameterType? type = null)
        => new(name, @namespace, instance, LightControllerCore.Instance, type)
        {
            WantSynced = sync,
        };
}