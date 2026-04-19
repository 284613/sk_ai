using Godot;

namespace SkAiRouteAdvisor.DecisionLogging;

internal sealed class DecisionLogPathProvider
{
    public string GetLogsRootDirectory()
    {
        var userDir = ProjectSettings.GlobalizePath("user://");
        return Path.Combine(userDir, "sk_ai_route_advisor", "decision_logs");
    }

    public string GetRunLogFilePath(string runId)
    {
        return Path.Combine(GetLogsRootDirectory(), $"{runId}.jsonl");
    }
}
