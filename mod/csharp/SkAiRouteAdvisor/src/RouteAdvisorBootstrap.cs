using MegaCrit.Sts2.Core.Logging;
using SkAiRouteAdvisor.RouteAdvisor;

namespace SkAiRouteAdvisor;

internal static class RouteAdvisorBootstrap
{
    private static RouteAdvisorRuntime? _runtime;

    public static void Initialize()
    {
        _runtime ??= new RouteAdvisorRuntime();
        _runtime.Initialize();
        Log.Info("[SkAiRouteAdvisor] bootstrap complete; route runtime initialized");
    }

    public static void Shutdown()
    {
        _runtime?.Shutdown();
    }
}
