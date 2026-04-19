namespace SkAiRouteAdvisor.DecisionLogging;

internal sealed class RouteDecisionPayload : DecisionPayloadBase
{
    public required string PathId { get; init; }
    public required IReadOnlyList<string> NodeSequence { get; init; }
    public required IReadOnlyList<string> NodeTypes { get; init; }
    public required double TotalScore { get; init; }
    public required double RiskScore { get; init; }
    public required double RewardScore { get; init; }
    public required double RecoveryScore { get; init; }
    public required double StructureScore { get; init; }
    public required IReadOnlyList<string> Reasons { get; init; }
    public required string StyleTag { get; init; }
}

internal sealed class CardRewardDecisionPayload : DecisionPayloadBase
{
    public required string? CardId { get; init; }
    public required string? CardName { get; init; }
    public required string? Rarity { get; init; }
    public required int? Cost { get; init; }
    public required bool Upgraded { get; init; }
    public double? Score { get; init; }
    public required IReadOnlyList<string> Reasons { get; init; }
    public required bool SkipOption { get; init; }
}

internal sealed class RelicChoiceDecisionPayload : DecisionPayloadBase
{
    public required string? RelicId { get; init; }
    public required string? RelicName { get; init; }
    public required string? RelicCategory { get; init; }
    public double? Score { get; init; }
    public required IReadOnlyList<string> Reasons { get; init; }
}

internal sealed class RouteActualChoicePayload : DecisionChoicePayloadBase
{
    public required string SelectedNextNodeId { get; init; }
    public required string SelectedNextNodeType { get; init; }
    public required IReadOnlyList<string> MatchedCandidateOptionIds { get; init; }
    public required bool ResolvedToSingleCandidate { get; init; }
}

internal sealed class CardRewardActualChoicePayload : DecisionChoicePayloadBase
{
    public required string? CardId { get; init; }
    public required string? CardName { get; init; }
    public required bool SkipOption { get; init; }
}

internal sealed class RelicChoiceActualChoicePayload : DecisionChoicePayloadBase
{
    public required string? RelicId { get; init; }
    public required string? RelicName { get; init; }
    public required string? RelicCategory { get; init; }
}
