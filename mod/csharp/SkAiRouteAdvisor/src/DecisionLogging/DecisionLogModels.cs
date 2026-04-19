using System.Text.Json.Serialization;

namespace SkAiRouteAdvisor.DecisionLogging;

internal sealed class DecisionLogEntry
{
    public required DecisionLogMetadata Metadata { get; init; }
    public required DecisionRunStateSnapshot RunState { get; init; }
    public required DecisionEventDescriptor DecisionEvent { get; init; }
    public required IReadOnlyList<DecisionCandidateOption> CandidateOptions { get; init; }
    public required DecisionRecommendationSnapshot Recommendation { get; init; }
    public DecisionActualChoiceSnapshot? ActualChoice { get; init; }
    public required DecisionOutcomeSnapshot OutcomeSnapshot { get; init; }
}

internal sealed class DecisionLogMetadata
{
    public required string SchemaVersion { get; init; }
    public required string? GameVersion { get; init; }
    public required string? ModVersion { get; init; }
    public required string TimestampUtc { get; init; }
    public required string SessionId { get; init; }
    public required string RunId { get; init; }
    public required string? Seed { get; init; }
    public required string? CharacterId { get; init; }
    public required int? DifficultyOrAscension { get; init; }
    public required int? ActIndex { get; init; }
    public required int? FloorIndex { get; init; }
}

internal sealed class DecisionRunStateSnapshot
{
    public required int? CurrentHp { get; init; }
    public required int? MaxHp { get; init; }
    public required double? CurrentHpRatio { get; init; }
    public required int? Gold { get; init; }
    public required int? PotionCount { get; init; }
    public required int? PotionSlots { get; init; }
    public required string? CurrentNodeId { get; init; }
    public required DecisionMapPositionSnapshot? CurrentMapPosition { get; init; }
    public required DecisionDeckSummarySnapshot DeckSummary { get; init; }
    public required IReadOnlyList<DecisionRelicSummaryEntry> RelicSummary { get; init; }
}

internal sealed class DecisionMapPositionSnapshot
{
    public required int Row { get; init; }
    public required int Col { get; init; }
}

internal sealed class DecisionDeckSummarySnapshot
{
    public required int TotalCards { get; init; }
    public required int UpgradedCards { get; init; }
    public required IReadOnlyList<string> CardIds { get; init; }
    public required IReadOnlyDictionary<string, int> CardCountsById { get; init; }
}

internal sealed class DecisionRelicSummaryEntry
{
    public required string RelicId { get; init; }
    public required string? RelicName { get; init; }
    public required int StackCount { get; init; }
}

internal sealed class DecisionEventDescriptor
{
    public required string DecisionId { get; init; }
    public required string DecisionType { get; init; }
    public required string DecisionPhase { get; init; }
    public required string SourceScreen { get; init; }
    public required string ModeTag { get; init; }
    public required string? ModelVersionOrScorerVersion { get; init; }
}

internal sealed class DecisionCandidateOption
{
    public required string OptionId { get; init; }
    public required DecisionPayloadBase Payload { get; init; }
}

internal sealed class DecisionRecommendationSnapshot
{
    public required string? RecommendedOptionId { get; init; }
    public required string? SecondaryOptionId { get; init; }
    public required IReadOnlyDictionary<string, double> RecommendationScores { get; init; }
    public required IReadOnlyList<string> Reasons { get; init; }
    public double? Confidence { get; init; }
}

internal sealed class DecisionActualChoiceSnapshot
{
    public string? ChosenOptionId { get; init; }
    public DecisionChoicePayloadBase? ChosenOptionPayload { get; init; }
    public required string ChoiceSource { get; init; }
    public required string ChoiceTimestampUtc { get; init; }
}

internal sealed class DecisionOutcomeSnapshot
{
    public int? NextCombatDamageTaken { get; init; }
    public bool? SurvivedNextElite { get; init; }
    public int? FloorsSurvivedAfterDecision { get; init; }
    public int? HpAfterNFloors { get; init; }
    public int? GoldAfterNFloors { get; init; }
    public string? Notes { get; init; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "payload_type")]
[JsonDerivedType(typeof(RouteDecisionPayload), "route_payload")]
[JsonDerivedType(typeof(CardRewardDecisionPayload), "card_reward_payload")]
[JsonDerivedType(typeof(RelicChoiceDecisionPayload), "relic_choice_payload")]
internal abstract class DecisionPayloadBase
{
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "choice_payload_type")]
[JsonDerivedType(typeof(RouteActualChoicePayload), "route_choice_payload")]
[JsonDerivedType(typeof(CardRewardActualChoicePayload), "card_reward_choice_payload")]
[JsonDerivedType(typeof(RelicChoiceActualChoicePayload), "relic_choice_payload")]
internal abstract class DecisionChoicePayloadBase
{
}
