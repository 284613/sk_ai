using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using SkAiRouteAdvisor.RouteAdvisor;

namespace SkAiRouteAdvisor.DecisionLogging;

internal sealed class DecisionLogService
{
    private readonly DecisionLogFactory _factory = new();
    private readonly DecisionLogJsonSerializer _serializer = new();
    private readonly DecisionLogFileWriter _fileWriter = new();
    private readonly DecisionLogPathProvider _pathProvider = new();

    private readonly string _sessionId = $"session_{DateTime.UtcNow:yyyyMMddTHHmmssZ}_{Guid.NewGuid():N}"[..40];
    private RunState? _trackedRunState;
    private string? _trackedRunId;

    public RouteDecisionLogToken LogRouteRecommendation(RouteRecommendationSummary summary, RunState runState, string sourceScreen)
    {
        var runId = EnsureRunId(runState);
        var decisionId = $"route_{DateTime.UtcNow:yyyyMMddTHHmmssfffZ}_{Guid.NewGuid():N}"[..46];
        var entry = _factory.CreateRouteRecommendationEntry(runState, summary, _sessionId, runId, decisionId, sourceScreen);
        WriteEntry(entry, runId);

        return new RouteDecisionLogToken
        {
            SessionId = _sessionId,
            RunId = runId,
            DecisionId = decisionId,
            SourceScreen = sourceScreen,
            ModeTag = summary.Mode.ToString().ToLowerInvariant(),
            ScorerVersion = summary.ScoringVersion,
            StartNodeId = DecisionLogValueFormatter.FormatMapPointId(summary.StartPoint),
            CandidateOptions = entry.CandidateOptions,
            Recommendation = entry.Recommendation,
        };
    }

    public void LogRouteChoice(RouteDecisionLogToken token, RouteRecommendationSummary summary, RunState runState, MapPoint chosenPoint, string choiceSource)
    {
        var entry = _factory.CreateRouteChoiceEntry(runState, summary, token, chosenPoint, choiceSource);
        WriteEntry(entry, token.RunId);
    }

    public CardDecisionLogToken LogCardRewardRecommendation(
        RunState runState,
        IReadOnlyList<CardModel> cardChoices,
        bool canSkip,
        string? recommendedOptionId = null,
        string? secondaryOptionId = null,
        IReadOnlyDictionary<string, double>? recommendationScores = null,
        IReadOnlyList<string>? reasons = null,
        string modeTag = "default",
        string? scorerVersion = null
    )
    {
        var runId = EnsureRunId(runState);
        var decisionId = $"card_{DateTime.UtcNow:yyyyMMddTHHmmssfffZ}_{Guid.NewGuid():N}"[..45];
        var entry = _factory.CreateCardRewardRecommendationEntry(
            runState,
            cardChoices,
            canSkip,
            _sessionId,
            runId,
            decisionId,
            recommendedOptionId,
            secondaryOptionId,
            recommendationScores,
            reasons,
            modeTag,
            scorerVersion
        );
        WriteEntry(entry, runId);
        return new CardDecisionLogToken
        {
            SessionId = _sessionId,
            RunId = runId,
            DecisionId = decisionId,
            SourceScreen = "card_reward_screen",
            ModeTag = modeTag,
            ScorerVersion = scorerVersion,
            CandidateOptions = entry.CandidateOptions,
            Recommendation = entry.Recommendation,
            CanSkip = canSkip,
        };
    }

    public void LogCardRewardChoice(RunState runState, CardDecisionLogToken token, CardModel? chosenCard, bool skipped, string choiceSource)
    {
        var entry = _factory.CreateCardRewardChoiceEntry(runState, token, chosenCard, skipped, choiceSource);
        WriteEntry(entry, token.RunId);
    }

    public RelicDecisionLogToken LogRelicChoiceRecommendation(
        RunState runState,
        IReadOnlyList<RelicModel> relicChoices,
        string? recommendedOptionId = null,
        string? secondaryOptionId = null,
        IReadOnlyDictionary<string, double>? recommendationScores = null,
        IReadOnlyList<string>? reasons = null,
        string modeTag = "default",
        string? scorerVersion = null
    )
    {
        var runId = EnsureRunId(runState);
        var decisionId = $"relic_{DateTime.UtcNow:yyyyMMddTHHmmssfffZ}_{Guid.NewGuid():N}"[..46];
        var entry = _factory.CreateRelicChoiceRecommendationEntry(
            runState,
            relicChoices,
            _sessionId,
            runId,
            decisionId,
            recommendedOptionId,
            secondaryOptionId,
            recommendationScores,
            reasons,
            modeTag,
            scorerVersion
        );
        WriteEntry(entry, runId);
        return new RelicDecisionLogToken
        {
            SessionId = _sessionId,
            RunId = runId,
            DecisionId = decisionId,
            SourceScreen = "relic_choice_screen",
            ModeTag = modeTag,
            ScorerVersion = scorerVersion,
            CandidateOptions = entry.CandidateOptions,
            Recommendation = entry.Recommendation,
        };
    }

    public void LogRelicChoice(RunState runState, RelicDecisionLogToken token, RelicModel? chosenRelic, string choiceSource)
    {
        var entry = _factory.CreateRelicChoiceEntry(runState, token, chosenRelic, choiceSource);
        WriteEntry(entry, token.RunId);
    }

    public string? GetCurrentRunLogPath()
    {
        return _trackedRunId == null ? null : _pathProvider.GetRunLogFilePath(_trackedRunId);
    }

    public string CurrentSessionId => _sessionId;

    private void WriteEntry(DecisionLogEntry entry, string runId)
    {
        var filePath = _pathProvider.GetRunLogFilePath(runId);
        var jsonLine = _serializer.Serialize(entry);
        var written = _fileWriter.AppendJsonLine(filePath, jsonLine);
        if (written)
        {
            Log.Info($"[SkAiRouteAdvisor] decision event created type={entry.DecisionEvent.DecisionType} phase={entry.DecisionEvent.DecisionPhase} path={filePath}");
        }
    }

    private string EnsureRunId(RunState runState)
    {
        if (ReferenceEquals(_trackedRunState, runState) && !string.IsNullOrWhiteSpace(_trackedRunId))
        {
            return _trackedRunId!;
        }

        _trackedRunState = runState;
        _trackedRunId = $"run_{DateTime.UtcNow:yyyyMMddTHHmmssZ}_{Guid.NewGuid():N}"[..36];
        Log.Info($"[SkAiRouteAdvisor] new run log stream run_id={_trackedRunId}");
        return _trackedRunId;
    }
}

internal sealed class RouteDecisionLogToken
{
    public required string SessionId { get; init; }
    public required string RunId { get; init; }
    public required string DecisionId { get; init; }
    public required string SourceScreen { get; init; }
    public required string ModeTag { get; init; }
    public required string ScorerVersion { get; init; }
    public required string StartNodeId { get; init; }
    public required IReadOnlyList<DecisionCandidateOption> CandidateOptions { get; init; }
    public required DecisionRecommendationSnapshot Recommendation { get; init; }
}

internal sealed class CardDecisionLogToken
{
    public required string SessionId { get; init; }
    public required string RunId { get; init; }
    public required string DecisionId { get; init; }
    public required string SourceScreen { get; init; }
    public required string ModeTag { get; init; }
    public required string? ScorerVersion { get; init; }
    public required IReadOnlyList<DecisionCandidateOption> CandidateOptions { get; init; }
    public required DecisionRecommendationSnapshot Recommendation { get; init; }
    public required bool CanSkip { get; init; }
}

internal sealed class RelicDecisionLogToken
{
    public required string SessionId { get; init; }
    public required string RunId { get; init; }
    public required string DecisionId { get; init; }
    public required string SourceScreen { get; init; }
    public required string ModeTag { get; init; }
    public required string? ScorerVersion { get; init; }
    public required IReadOnlyList<DecisionCandidateOption> CandidateOptions { get; init; }
    public required DecisionRecommendationSnapshot Recommendation { get; init; }
}
