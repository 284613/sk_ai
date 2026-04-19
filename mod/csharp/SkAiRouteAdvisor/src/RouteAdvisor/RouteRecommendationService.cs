using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Runs;

namespace SkAiRouteAdvisor.RouteAdvisor;

internal sealed class RouteRecommendationService
{
    public RouteRecommendationSummary? Evaluate(RunState runState, RouteMode mode)
    {
        var startPoint = runState.CurrentMapPoint ?? runState.Map?.StartingMapPoint;
        if (startPoint == null)
        {
            return null;
        }

        var player = runState.Players.FirstOrDefault();
        if (player?.Creature == null)
        {
            return null;
        }

        var paths = new List<List<MapPoint>>();
        EnumeratePathsToBoss(startPoint, [], paths);
        if (paths.Count == 0)
        {
            return null;
        }

        var allRoutes = paths
            .Select(path => ScorePath(path, mode, player))
            .OrderByDescending(path => path.TotalScore)
            .ToList();
        var scoredRoutes = allRoutes.Take(2).ToList();

        return new RouteRecommendationSummary
        {
            Mode = mode,
            ActIndex = runState.CurrentActIndex,
            ActFloor = runState.ActFloor,
            TotalFloor = runState.TotalFloor,
            CurrentHp = player.Creature.CurrentHp,
            MaxHp = player.Creature.MaxHp,
            Gold = player.Gold,
            CharacterId = player.Character?.Id.ToString() ?? string.Empty,
            StartPoint = startPoint,
            AllRoutes = allRoutes,
            RankedRoutes = scoredRoutes,
        };
    }

    private void EnumeratePathsToBoss(MapPoint current, List<MapPoint> currentPath, List<List<MapPoint>> allPaths)
    {
        currentPath.Add(current);

        if (current.PointType == MapPointType.Boss)
        {
            allPaths.Add([.. currentPath]);
        }
        else if (current.Children != null && current.Children.Count > 0)
        {
            foreach (var child in current.Children)
            {
                EnumeratePathsToBoss(child, currentPath, allPaths);
            }
        }

        currentPath.RemoveAt(currentPath.Count - 1);
    }

    private ScoredRoute ScorePath(IReadOnlyList<MapPoint> path, RouteMode mode, Player player)
    {
        var currentHp = player.Creature.CurrentHp;
        var maxHp = player.Creature.MaxHp;
        var gold = player.Gold;
        var hpRatio = maxHp <= 0 ? 0.0 : (double)currentHp / maxHp;
        var weights = GetWeights(mode);
        var reasons = new List<RouteReason>();
        var nodeScores = new List<RouteNodeScore>();

        double riskScore = 0.0;
        double rewardScore = 0.0;
        double recoveryScore = 0.0;
        double structureScore = 0.0;
        double runningTotal = 0.0;
        var combatChain = 0;

        foreach (var indexedPoint in path.Select((point, index) => (point, index)))
        {
            var point = indexedPoint.point;
            double nodeRisk = 0.0;
            double nodeReward = 0.0;
            double nodeRecovery = 0.0;
            double nodeStructure = 0.0;

            switch (point.PointType)
            {
                case MapPointType.RestSite:
                    nodeRecovery += weights.RestBase + ((1.0 - hpRatio) * weights.RestLowHpBonus);
                    combatChain = 0;
                    break;
                case MapPointType.Shop:
                    nodeRecovery += weights.ShopBase + Math.Min(8.0, gold / 50.0);
                    combatChain = 0;
                    break;
                case MapPointType.Treasure:
                    nodeReward += weights.TreasureReward;
                    combatChain = 0;
                    break;
                case MapPointType.Elite:
                    nodeReward += weights.EliteReward;
                    nodeRisk -= weights.EliteRisk * LowHpRiskMultiplier(hpRatio);
                    combatChain++;
                    break;
                case MapPointType.Monster:
                    nodeReward += weights.MonsterReward;
                    nodeRisk -= weights.MonsterRisk * LowHpRiskMultiplier(hpRatio);
                    combatChain++;
                    break;
                case MapPointType.Unknown:
                    nodeReward += weights.UnknownReward;
                    nodeRisk -= weights.UnknownRisk * LowHpRiskMultiplier(hpRatio);
                    combatChain = 0;
                    break;
                case MapPointType.Boss:
                    combatChain = 0;
                    break;
            }

            if (combatChain >= 3)
            {
                nodeStructure -= weights.LongCombatPenalty * (combatChain - 2);
            }

            if (indexedPoint.index > 0)
            {
                var previousPoint = path[indexedPoint.index - 1].PointType;
                if (previousPoint == MapPointType.RestSite && point.PointType == MapPointType.Elite)
                {
                    nodeStructure += weights.RestBeforeEliteBonus;
                }

                if (previousPoint == MapPointType.Elite && point.PointType == MapPointType.RestSite)
                {
                    nodeStructure += weights.EliteBeforeRestBonus;
                }
            }

            riskScore += nodeRisk;
            rewardScore += nodeReward;
            recoveryScore += nodeRecovery;
            structureScore += nodeStructure;

            var weightedDelta =
                (nodeRisk * weights.RiskWeight) +
                (nodeReward * weights.RewardWeight) +
                (nodeRecovery * weights.RecoveryWeight) +
                (nodeStructure * weights.StructureWeight);
            runningTotal += weightedDelta;

            nodeScores.Add(new RouteNodeScore(
                point,
                ShortPointLabel(point),
                weightedDelta,
                runningTotal
            ));
        }

        if (hpRatio < 0.45 && path.Any(point => point.PointType == MapPointType.RestSite))
        {
            reasons.Add(new RouteReason("recovery", "当前血量偏低，优先靠近篝火", 1.0));
        }

        if (gold >= 120 && path.Any(point => point.PointType == MapPointType.Shop))
        {
            reasons.Add(new RouteReason("recovery", "金币充足，商店价值较高", 0.8));
        }

        if (HasPattern(path, MapPointType.RestSite, MapPointType.Elite))
        {
            reasons.Add(new RouteReason("structure", "此路径在篝火后挑战精英，准备更充分", 0.8));
        }
        else if (HasPattern(path, MapPointType.Elite, MapPointType.RestSite))
        {
            reasons.Add(new RouteReason("structure", "此路径包含精英后接休息点，风险可控", 0.8));
        }

        if (HasLongCombatChain(path))
        {
            reasons.Add(new RouteReason("risk", "连续战斗过多，短期掉血风险偏高", -0.8));
        }

        if (mode == RouteMode.Aggressive && path.Any(point => point.PointType == MapPointType.Elite))
        {
            reasons.Add(new RouteReason("reward", "路径包含精英节点，成长收益更高", 0.9));
        }

        if (reasons.Count == 0)
        {
            reasons.Add(new RouteReason("reward", "这条路径收益与风险较均衡", 0.4));
        }

        var totalScore =
            (riskScore * weights.RiskWeight) +
            (rewardScore * weights.RewardWeight) +
            (recoveryScore * weights.RecoveryWeight) +
            (structureScore * weights.StructureWeight);

        return new ScoredRoute
        {
            PathId = string.Join("->", path.Select(FormatPoint)),
            DisplayPath = BuildDisplayPath(path),
            Points = path,
            NodeScores = nodeScores,
            TotalScore = totalScore,
            RiskScore = riskScore,
            RewardScore = rewardScore,
            RecoveryScore = recoveryScore,
            StructureScore = structureScore,
            Reasons = reasons,
        };
    }

    private static bool HasPattern(IReadOnlyList<MapPoint> path, MapPointType first, MapPointType second)
    {
        for (var index = 1; index < path.Count; index++)
        {
            if (path[index - 1].PointType == first && path[index].PointType == second)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasLongCombatChain(IReadOnlyList<MapPoint> path)
    {
        var chain = 0;
        foreach (var point in path)
        {
            if (point.PointType == MapPointType.Monster || point.PointType == MapPointType.Elite)
            {
                chain++;
                if (chain >= 3)
                {
                    return true;
                }
            }
            else
            {
                chain = 0;
            }
        }

        return false;
    }

    private static double LowHpRiskMultiplier(double hpRatio)
    {
        if (hpRatio <= 0.30) return 1.8;
        if (hpRatio <= 0.50) return 1.3;
        if (hpRatio <= 0.70) return 1.05;
        return 0.85;
    }

    private static string FormatPoint(MapPoint point)
    {
        return $"{point.PointType}@{point.coord}";
    }

    private static string BuildDisplayPath(IReadOnlyList<MapPoint> path)
    {
        var futurePoints = path.Skip(1).ToList();
        if (futurePoints.Count == 0)
        {
            futurePoints.Add(path.Last());
        }

        return string.Join(" -> ", futurePoints.Select(ShortPointLabel));
    }

    private static string ShortPointLabel(MapPoint point)
    {
        return point.PointType switch
        {
            MapPointType.Ancient => "Anc",
            MapPointType.Monster => "Mon",
            MapPointType.Elite => "Elite",
            MapPointType.RestSite => "Rest",
            MapPointType.Shop => "Shop",
            MapPointType.Treasure => "Chest",
            MapPointType.Unknown => "?",
            MapPointType.Boss => "Boss",
            _ => point.PointType.ToString(),
        };
    }

    private static RouteWeights GetWeights(RouteMode mode)
    {
        return mode switch
        {
            RouteMode.Safe => new RouteWeights(1.6, 0.85, 1.3, 1.0, 4.5, 10.0, 6.0, 5.5, 2.0, 7.5, 4.0, 4.5, 4.0, 3.5),
            RouteMode.Aggressive => new RouteWeights(0.9, 1.45, 0.8, 1.0, 6.5, 15.0, 7.0, 2.5, 1.0, 5.0, 3.0, 3.0, 5.0, 2.0),
            _ => new RouteWeights(1.2, 1.15, 1.0, 1.0, 5.5, 13.0, 6.5, 4.0, 1.5, 6.0, 3.5, 4.0, 4.5, 3.0),
        };
    }

    private sealed record RouteWeights(
        double RiskWeight,
        double RewardWeight,
        double RecoveryWeight,
        double StructureWeight,
        double MonsterReward,
        double EliteReward,
        double TreasureReward,
        double UnknownReward,
        double MonsterRisk,
        double EliteRisk,
        double UnknownRisk,
        double RestBase,
        double RestLowHpBonus,
        double ShopBase,
        double RestBeforeEliteBonus = 4.0,
        double EliteBeforeRestBonus = 4.0,
        double LongCombatPenalty = 3.0
    );
}
