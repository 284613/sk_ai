using MegaCrit.Sts2.Core.Logging;

namespace SkAiRouteAdvisor.DecisionLogging;

internal sealed class DecisionLogFileWriter
{
    public bool AppendJsonLine(string filePath, string jsonLine)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.AppendAllText(filePath, jsonLine + Environment.NewLine);
            Log.Info($"[SkAiRouteAdvisor] decision log written path={filePath}");
            return true;
        }
        catch (Exception exception)
        {
            Log.Error($"[SkAiRouteAdvisor] failed to write decision log path={filePath} error={exception.Message}");
            return false;
        }
    }
}
