using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace SkAiRouteAdvisor;

[ModInitializer(nameof(Initialize))]
public static class Plugin
{
    internal const string ModId = "SkAiRouteAdvisor";

    public static void Initialize()
    {
        Log.Info($"[{ModId}] loaded");
        RouteAdvisorBootstrap.Initialize();
    }

    public static void Unload()
    {
        RouteAdvisorBootstrap.Shutdown();
        Log.Info($"[{ModId}] unloaded");
    }
}
