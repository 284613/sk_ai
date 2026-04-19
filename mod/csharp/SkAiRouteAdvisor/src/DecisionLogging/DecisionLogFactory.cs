using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using SkAiRouteAdvisor.RouteAdvisor;

namespace SkAiRouteAdvisor.DecisionLogging;

internal sealed class DecisionLogFactory
{
    public DecisionLogEntry CreateRouteRecommendationEntry(
        RunState runState,
        RouteRecommendationSummary summary,
        string sessionId,
        string runId,
        string decisionId,
        string sourceScreen
    )
    {
        return new DecisionLogEntry
        {
            Metadata = CreateMetadata(runState, sessionId, runId),
            RunState = CreateRunStateSnapshot(runState),
            DecisionEvent = new DecisionEventDescriptor
            {
                DecisionId = decisionId,
                DecisionType = "route_decision",
                DecisionPhase = "recommendation_generated",
                SourceScreen = sourceScreen,
                ModeTag = summary.Mode.ToString().ToLowerInvariant(),
                ModelVersionOrScorerVersion = summary.ScoringVersion,
            },
            CandidateOptions = summary.AllRoutes.Select(route => CreateRouteCandidateOption(route, summary.Mode)).ToList(),
            Recommendation = new DecisionRecommendationSnapshot
            {
                RecommendedOptionId = summary.RankedRoutes.ElementAtOrDefault(0)?.PathId,
                SecondaryOptionId = summary.RankedRoutes.ElementAtOrDefault(1)?.PathId,
                RecommendationScores = summary.AllRoutes.ToDictionary(route => route.PathId, route => route.TotalScore),
                Reasons = summary.RankedRoutes.ElementAtOrDefault(0)?.Reasons.Select(reason => reason.Message).ToList() ?? [],
                Confidence = null,
            },
            ActualChoice = null,
            OutcomeSnapshot = CreateEmptyOutcomeSnapshot(),
        };
    }

    public DecisionLogEntry CreateRouteChoiceEntry(
        RunState runState,
        RouteRecommendationSummary summary,
        RouteDecisionLogToken token,
        MapPoint chosenPoint,
        string choiceSource
    )
    {
        var matchedOptionIds = token.CandidateOptions
            .Select(option => new
            {
                option.OptionId,
                Payload = option.Payload as RouteDecisionPayload,
            })
            .Where(item => item.Payload != null && item.Payload.NodeSequence.Skip(1).FirstOrDefault() == DecisionLogValueFormatter.FormatMapPointId(chosenPoint))
            .Select(item => item.OptionId)
            .ToList();

        var resolvedToSingleCandidate = matchedOptionIds.Count == 1;

        return new DecisionLogEntry
        {
            Metadata = CreateMetadata(runState, token.SessionId, token.RunId),
            RunState = CreateRunStateSnapshot(runState),
            DecisionEvent = new DecisionEventDescriptor
            {
                DecisionId = token.DecisionId,
                DecisionType = "route_decision",
                DecisionPhase = "actual_choice_recorded",
                SourceScreen = token.SourceScreen,
                ModeTag = token.ModeTag,
                ModelVersionOrScorerVersion = token.ScorerVersion,
            },
            CandidateOptions = token.CandidateOptions,
            Recommendation = token.Recommendation,
            ActualChoice = new DecisionActualChoiceSnapshot
            {
                ChosenOptionId = resolvedToSingleCandidate ? matchedOptionIds[0] : null,
                ChosenOptionPayload = new RouteActualChoicePayload
                {
                    SelectedNextNodeId = DecisionLogValueFormatter.FormatMapPointId(chosenPoint),
                    SelectedNextNodeType = chosenPoint.PointType.ToString().ToLowerInvariant(),
                    MatchedCandidateOptionIds = matchedOptionIds,
                    ResolvedToSingleCandidate = resolvedToSingleCandidate,
                },
                ChoiceSource = choiceSource,
                ChoiceTimestampUtc = DateTime.UtcNow.ToString("O"),
            },
            OutcomeSnapshot = CreateEmptyOutcomeSnapshot(),
        };
    }

    public DecisionLogEntry CreateCardRewardRecommendationEntry(
        RunState runState,
        IReadOnlyList<CardModel> cardChoices,
        bool canSkip,
        string sessionId,
        string runId,
        string decisionId,
        string? recommendedOptionId = null,
        string? secondaryOptionId = null,
        IReadOnlyDictionary<string, double>? recommendationScores = null,
        IReadOnlyList<string>? reasons = null,
        string modeTag = "default",
        string? scorerVersion = null
    )
    {
        var candidateOptions = cardChoices.Select(CreateCardCandidateOption).ToList();
        if (canSkip)
        {
            candidateOptions.Add(new DecisionCandidateOption
            {
                OptionId = "skip",
                Payload = new CardRewardDecisionPayload
                {
                    CardId = null,
                    CardName = null,
                    Rarity = null,
                    Cost = null,
                    Upgraded = false,
                    Score = recommendationScores != null && recommendationScores.TryGetValue("skip", out var skipScore) ? skipScore : null,
                    Reasons = [],
                    SkipOption = true,
                },
            });
        }

        return new DecisionLogEntry
        {
            Metadata = CreateMetadata(runState, sessionId, runId),
            RunState = CreateRunStateSnapshot(runState),
            DecisionEvent = new DecisionEventDescriptor
            {
                DecisionId = decisionId,
                DecisionType = "card_reward_decision",
                DecisionPhase = "recommendation_generated",
                SourceScreen = "card_reward_screen",
                ModeTag = modeTag,
                ModelVersionOrScorerVersion = scorerVersion,
            },
            CandidateOptions = candidateOptions,
            Recommendation = new DecisionRecommendationSnapshot
            {
                RecommendedOptionId = recommendedOptionId,
                SecondaryOptionId = secondaryOptionId,
                RecommendationScores = recommendationScores ?? new Dictionary<string, double>(),
                Reasons = reasons ?? [],
                Confidence = null,
            },
            ActualChoice = null,
            OutcomeSnapshot = CreateEmptyOutcomeSnapshot(),
        };
    }

    public DecisionLogEntry CreateCardRewardChoiceEntry(
        RunState runState,
        CardDecisionLogToken token,
        CardModel? chosenCard,
        bool skipped,
        string choiceSource
    )
    {
        var choicePayload = new CardRewardActualChoicePayload
        {
            CardId = skipped ? null : DecisionLogValueFormatter.FormatModelId(chosenCard?.Id),
            CardName = skipped ? null : chosenCard?.Title,
            SkipOption = skipped,
        };

        return new DecisionLogEntry
        {
            Metadata = CreateMetadata(runState, token.SessionId, token.RunId),
            RunState = CreateRunStateSnapshot(runState),
            DecisionEvent = new DecisionEventDescriptor
            {
                DecisionId = token.DecisionId,
                DecisionType = "card_reward_decision",
                DecisionPhase = "actual_choice_recorded",
                SourceScreen = token.SourceScreen,
                ModeTag = token.ModeTag,
                ModelVersionOrScorerVersion = token.ScorerVersion,
            },
            CandidateOptions = token.CandidateOptions,
            Recommendation = token.Recommendation,
            ActualChoice = new DecisionActualChoiceSnapshot
            {
                ChosenOptionId = skipped ? "skip" : DecisionLogValueFormatter.FormatModelId(chosenCard?.Id),
                ChosenOptionPayload = choicePayload,
                ChoiceSource = choiceSource,
                ChoiceTimestampUtc = DateTime.UtcNow.ToString("O"),
            },
            OutcomeSnapshot = CreateEmptyOutcomeSnapshot(),
        };
    }

    public DecisionLogEntry CreateRelicChoiceRecommendationEntry(
        RunState runState,
        IReadOnlyList<RelicModel> relicChoices,
        string sessionId,
        string runId,
        string decisionId,
        string? recommendedOptionId = null,
        string? secondaryOptionId = null,
        IReadOnlyDictionary<string, double>? recommendationScores = null,
        IReadOnlyList<string>? reasons = null,
        string modeTag = "default",
        string? scorerVersion = null
    )
    {
        return new DecisionLogEntry
        {
            Metadata = CreateMetadata(runState, sessionId, runId),
            RunState = CreateRunStateSnapshot(runState),
            DecisionEvent = new DecisionEventDescriptor
            {
                DecisionId = decisionId,
                DecisionType = "relic_choice_decision",
                DecisionPhase = "recommendation_generated",
                SourceScreen = "relic_choice_screen",
                ModeTag = modeTag,
                ModelVersionOrScorerVersion = scorerVersion,
            },
            CandidateOptions = relicChoices.Select(CreateRelicCandidateOption).ToList(),
            Recommendation = new DecisionRecommendationSnapshot
            {
                RecommendedOptionId = recommendedOptionId,
                SecondaryOptionId = secondaryOptionId,
                RecommendationScores = recommendationScores ?? new Dictionary<string, double>(),
                Reasons = reasons ?? [],
                Confidence = null,
            },
            ActualChoice = null,
            OutcomeSnapshot = CreateEmptyOutcomeSnapshot(),
        };
    }

    public DecisionLogEntry CreateRelicChoiceEntry(
        RunState runState,
        RelicDecisionLogToken token,
        RelicModel? chosenRelic,
        string choiceSource
    )
    {
        return new DecisionLogEntry
        {
            Metadata = CreateMetadata(runState, token.SessionId, token.RunId),
            RunState = CreateRunStateSnapshot(runState),
            DecisionEvent = new DecisionEventDescriptor
            {
                DecisionId = token.DecisionId,
                DecisionType = "relic_choice_decision",
                DecisionPhase = "actual_choice_recorded",
                SourceScreen = token.SourceScreen,
                ModeTag = token.ModeTag,
                ModelVersionOrScorerVersion = token.ScorerVersion,
            },
            CandidateOptions = token.CandidateOptions,
            Recommendation = token.Recommendation,
            ActualChoice = new DecisionActualChoiceSnapshot
            {
                ChosenOptionId = chosenRelic == null ? null : DecisionLogValueFormatter.FormatModelId(chosenRelic.Id),
                ChosenOptionPayload = chosenRelic == null
                    ? null
                    : new RelicChoiceActualChoicePayload
                    {
                        RelicId = DecisionLogValueFormatter.FormatModelId(chosenRelic.Id),
                        RelicName = DecisionLogValueFormatter.FormatLocString(chosenRelic.Title),
                        RelicCategory = chosenRelic.Rarity.ToString().ToLowerInvariant(),
                    },
                ChoiceSource = choiceSource,
                ChoiceTimestampUtc = DateTime.UtcNow.ToString("O"),
            },
            OutcomeSnapshot = CreateEmptyOutcomeSnapshot(),
        };
    }

    private DecisionLogMetadata CreateMetadata(RunState runState, string sessionId, string runId)
    {
        var player = runState.Players.FirstOrDefault();
        return new DecisionLogMetadata
        {
            SchemaVersion = RouteAdvisorBuildInfo.SchemaVersion,
            GameVersion = typeof(RunState).Assembly.GetName().Version?.ToString(),
            ModVersion = typeof(Plugin).Assembly.GetName().Version?.ToString(),
            TimestampUtc = DateTime.UtcNow.ToString("O"),
            SessionId = sessionId,
            RunId = runId,
            Seed = runState.Rng?.StringSeed,
            CharacterId = player?.Character?.Id == null ? null : DecisionLogValueFormatter.FormatModelId(player.Character.Id),
            DifficultyOrAscension = runState.AscensionLevel,
            ActIndex = runState.CurrentActIndex,
            FloorIndex = runState.ActFloor,
        };
    }

    private DecisionRunStateSnapshot CreateRunStateSnapshot(RunState runState)
    {
        var player = runState.Players.FirstOrDefault();
        var currentHp = player?.Creature?.CurrentHp;
        var maxHp = player?.Creature?.MaxHp;
        double? hpRatio = currentHp.HasValue && maxHp.HasValue && maxHp.Value > 0
            ? (double)currentHp.Value / maxHp.Value
            : null;

        var deckCards = player?.Deck?.Cards ?? [];
        var cardIds = deckCards
            .Select(card => DecisionLogValueFormatter.FormatModelId(card.Id))
            .OrderBy(cardId => cardId)
            .ToList();
        var cardCounts = cardIds
            .GroupBy(cardId => cardId)
            .ToDictionary(group => group.Key, group => group.Count());

        var relicSummary = (player?.Relics ?? [])
            .Select(relic => new DecisionRelicSummaryEntry
            {
                RelicId = DecisionLogValueFormatter.FormatModelId(relic.Id),
                RelicName = DecisionLogValueFormatter.FormatLocString(relic.Title),
                StackCount = relic.StackCount,
            })
            .ToList();

        return new DecisionRunStateSnapshot
        {
            CurrentHp = currentHp,
            MaxHp = maxHp,
            CurrentHpRatio = hpRatio,
            Gold = player?.Gold,
            PotionCount = player?.Potions.Count(),
            PotionSlots = player?.MaxPotionCount,
            CurrentNodeId = DecisionLogValueFormatter.FormatMapPointId(runState.CurrentMapPoint),
            CurrentMapPosition = DecisionLogValueFormatter.FormatMapCoord(runState.CurrentMapCoord),
            DeckSummary = new DecisionDeckSummarySnapshot
            {
                TotalCards = deckCards.Count,
                UpgradedCards = deckCards.Count(card => card.IsUpgraded),
                CardIds = cardIds,
                CardCountsById = cardCounts,
            },
            RelicSummary = relicSummary,
        };
    }

    private static DecisionCandidateOption CreateRouteCandidateOption(ScoredRoute route, RouteMode mode)
    {
        return new DecisionCandidateOption
        {
            OptionId = route.PathId,
            Payload = new RouteDecisionPayload
            {
                PathId = route.PathId,
                NodeSequence = route.Points.Select(DecisionLogValueFormatter.FormatMapPointId).ToList(),
                NodeTypes = route.Points.Select(point => point.PointType.ToString().ToLowerInvariant()).ToList(),
                TotalScore = route.TotalScore,
                RiskScore = route.RiskScore,
                RewardScore = route.RewardScore,
                RecoveryScore = route.RecoveryScore,
                StructureScore = route.StructureScore,
                Reasons = route.Reasons.Select(reason => reason.Message).ToList(),
                StyleTag = mode.ToString().ToLowerInvariant(),
            },
        };
    }

    private static DecisionCandidateOption CreateCardCandidateOption(CardModel card)
    {
        return new DecisionCandidateOption
        {
            OptionId = DecisionLogValueFormatter.FormatModelId(card.Id),
            Payload = new CardRewardDecisionPayload
            {
                CardId = DecisionLogValueFormatter.FormatModelId(card.Id),
                CardName = card.Title,
                Rarity = card.Rarity.ToString().ToLowerInvariant(),
                Cost = card.CurrentStarCost,
                Upgraded = card.IsUpgraded,
                Score = null,
                Reasons = [],
                SkipOption = false,
            },
        };
    }

    private static DecisionCandidateOption CreateRelicCandidateOption(RelicModel relic)
    {
        return new DecisionCandidateOption
        {
            OptionId = DecisionLogValueFormatter.FormatModelId(relic.Id),
            Payload = new RelicChoiceDecisionPayload
            {
                RelicId = DecisionLogValueFormatter.FormatModelId(relic.Id),
                RelicName = DecisionLogValueFormatter.FormatLocString(relic.Title),
                RelicCategory = relic.Rarity.ToString().ToLowerInvariant(),
                Score = null,
                Reasons = [],
            },
        };
    }

    private static DecisionOutcomeSnapshot CreateEmptyOutcomeSnapshot()
    {
        return new DecisionOutcomeSnapshot
        {
            NextCombatDamageTaken = null,
            SurvivedNextElite = null,
            FloorsSurvivedAfterDecision = null,
            HpAfterNFloors = null,
            GoldAfterNFloors = null,
            Notes = null,
        };
    }
}
