using MegaCrit.Sts2.Core.Map;

namespace SkAiRouteAdvisor.RouteAdvisor;

internal enum RouteMode
{
    Safe,
    Balanced,
    Aggressive,
}

internal sealed record RouteReason(string Category, string Message, double Impact);

internal sealed record RouteNodeScore(
    MapPoint Point,
    string Label,
    double DeltaScore,
    double RunningScore
);

internal sealed class ScoredRoute
{
    public required string PathId { get; init; }
    public required string DisplayPath { get; init; }
    public required IReadOnlyList<MapPoint> Points { get; init; }
    public required IReadOnlyList<RouteNodeScore> NodeScores { get; init; }
    public required double TotalScore { get; init; }
    public required double RiskScore { get; init; }
    public required double RewardScore { get; init; }
    public required double RecoveryScore { get; init; }
    public required double StructureScore { get; init; }
    public required IReadOnlyList<RouteReason> Reasons { get; init; }
}

internal sealed class RouteRecommendationSummary
{
    public required RouteMode Mode { get; init; }
    public required int ActIndex { get; init; }
    public required int ActFloor { get; init; }
    public required int TotalFloor { get; init; }
    public required int CurrentHp { get; init; }
    public required int MaxHp { get; init; }
    public required int Gold { get; init; }
    public required string CharacterId { get; init; }
    public required MapPoint StartPoint { get; init; }
    public required IReadOnlyList<ScoredRoute> AllRoutes { get; init; }
    public required IReadOnlyList<ScoredRoute> RankedRoutes { get; init; }
    public string ScoringVersion => RouteAdvisorBuildInfo.ScoringVersion;
}
