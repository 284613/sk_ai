package com.skai.sts2advisor.route;

import com.skai.sts2advisor.route.model.MapNodeSnapshot;
import com.skai.sts2advisor.route.model.MapStateSnapshot;
import com.skai.sts2advisor.route.model.NodeType;
import com.skai.sts2advisor.route.model.PathCandidate;
import com.skai.sts2advisor.route.model.PathStyle;
import com.skai.sts2advisor.route.model.RecommendationReason;
import com.skai.sts2advisor.route.model.RunStateSnapshot;
import com.skai.sts2advisor.route.model.ScoredPath;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.List;

public final class BaselineRouteScorer implements RouteScorer {
    public static final String SCORING_VERSION = "route-baseline-v1";

    public List<ScoredPath> score(
        MapStateSnapshot mapState,
        RunStateSnapshot runState,
        List<PathCandidate> candidates,
        PathStyle style
    ) {
        BaselineScoringProfile profile = BaselineScoringProfile.forStyle(style);
        ArrayList<ScoredPath> scored = new ArrayList<ScoredPath>();
        for (PathCandidate candidate : candidates) {
            scored.add(scoreCandidate(mapState, runState, candidate, style, profile));
        }
        Collections.sort(scored, new Comparator<ScoredPath>() {
            public int compare(ScoredPath left, ScoredPath right) {
                return Double.compare(right.getTotalScore(), left.getTotalScore());
            }
        });
        return scored;
    }

    public String getScoringVersion() {
        return SCORING_VERSION;
    }

    private ScoredPath scoreCandidate(
        MapStateSnapshot mapState,
        RunStateSnapshot runState,
        PathCandidate candidate,
        PathStyle style,
        BaselineScoringProfile profile
    ) {
        double hpRatio = runState.getHpRatio();
        double riskScore = 0.0;
        double rewardScore = 0.0;
        double recoveryScore = 0.0;
        double structureScore = 0.0;
        int combatChain = 0;
        int highRiskChain = 0;
        boolean containsRest = false;
        boolean containsShop = false;
        boolean containsElite = false;
        ArrayList<RecommendationReason> reasons = new ArrayList<RecommendationReason>();
        List<MapNodeSnapshot> nodes = resolveNodes(mapState, candidate);

        for (int i = 0; i < nodes.size(); i++) {
            MapNodeSnapshot node = nodes.get(i);
            NodeType nodeType = node.getNodeType();

            if (nodeType == NodeType.MONSTER) {
                rewardScore += profile.getMonsterRewardValue();
                combatChain++;
                highRiskChain++;
            } else if (nodeType == NodeType.ELITE) {
                containsElite = true;
                rewardScore += profile.getEliteRewardValue();
                riskScore -= profile.getEliteRiskPenalty() * lowHpRiskMultiplier(hpRatio);
                combatChain++;
                highRiskChain++;
            } else if (nodeType == NodeType.UNKNOWN) {
                rewardScore += profile.getEventRewardValue() * 0.55;
                riskScore -= profile.getUnknownRiskPenalty() * lowHpRiskMultiplier(hpRatio);
                highRiskChain++;
                combatChain = 0;
            } else if (nodeType == NodeType.EVENT) {
                rewardScore += profile.getEventRewardValue();
                combatChain = 0;
                highRiskChain = 0;
            } else if (nodeType == NodeType.TREASURE) {
                rewardScore += profile.getTreasureRewardValue();
                combatChain = 0;
                highRiskChain = 0;
            } else if (nodeType == NodeType.REST) {
                containsRest = true;
                recoveryScore += profile.getRestBaseValue() + ((1.0 - hpRatio) * profile.getRestLowHpBonus());
                combatChain = 0;
                highRiskChain = 0;
            } else if (nodeType == NodeType.SHOP) {
                containsShop = true;
                recoveryScore += profile.getShopBaseValue() + (runState.getGold() * profile.getShopGoldScale());
                combatChain = 0;
                highRiskChain = 0;
            } else {
                combatChain = 0;
                highRiskChain = 0;
            }

            if (combatChain >= 3) {
                riskScore -= profile.getCombatRiskPenalty() * (combatChain - 1) * lowHpRiskMultiplier(hpRatio);
            }
            if (highRiskChain >= 2) {
                structureScore -= profile.getHighRiskChainPenalty() * (highRiskChain - 1);
            }
            if (i > 0) {
                NodeType previousType = nodes.get(i - 1).getNodeType();
                if (previousType == NodeType.REST && nodeType == NodeType.ELITE) {
                    structureScore += profile.getStructureComboBonus();
                }
                if (previousType == NodeType.ELITE && nodeType == NodeType.REST) {
                    structureScore += profile.getStructureComboBonus();
                }
            }
        }

        if (candidate.getNodeSequence().size() < 2) {
            structureScore -= profile.getShortPathPenalty();
        }

        if (hpRatio < 0.45 && containsRest) {
            reasons.add(new RecommendationReason("recovery", "low_hp_rest", "当前血量偏低，优先靠近篝火", 1.0));
        }
        if (containsElite && hasAdjacentCombo(nodes, NodeType.ELITE, NodeType.REST)) {
            reasons.add(new RecommendationReason("structure", "elite_rest", "此路径包含精英后接休息点，风险可控", 0.9));
        } else if (containsElite && hasAdjacentCombo(nodes, NodeType.REST, NodeType.ELITE)) {
            reasons.add(new RecommendationReason("structure", "rest_elite", "此路径在篝火后挑战精英，准备更充分", 0.8));
        }
        if (containsShop && runState.getGold() >= 120) {
            reasons.add(new RecommendationReason("recovery", "rich_shop", "金币充足，商店价值较高", 0.8));
        }
        if (hasLongCombatChain(nodes)) {
            reasons.add(new RecommendationReason("risk", "combat_chain", "连续战斗过多，短期掉血风险偏高", -0.9));
        }
        if (containsElite && style == PathStyle.AGGRESSIVE) {
            reasons.add(new RecommendationReason("reward", "elite_growth", "路径包含精英节点，成长收益更高", 0.9));
        }
        if (countNodeType(nodes, NodeType.UNKNOWN) >= 2) {
            reasons.add(new RecommendationReason("risk", "unknown_variance", "未知房较多，结果波动较大", -0.5));
        }
        if (reasons.isEmpty()) {
            reasons.add(new RecommendationReason("reward", "steady_value", "这条路径收益与风险较均衡", 0.4));
        }

        double totalScore =
            (riskScore * profile.getRiskWeight()) +
            (rewardScore * profile.getRewardWeight()) +
            (recoveryScore * profile.getRecoveryWeight()) +
            (structureScore * profile.getStructureWeight());

        return new ScoredPath(
            candidate.getPathId(),
            candidate.getNodeSequence(),
            totalScore,
            riskScore,
            rewardScore,
            recoveryScore,
            structureScore,
            reasons,
            style
        );
    }

    private List<MapNodeSnapshot> resolveNodes(MapStateSnapshot mapState, PathCandidate candidate) {
        ArrayList<MapNodeSnapshot> nodes = new ArrayList<MapNodeSnapshot>();
        for (String nodeId : candidate.getNodeSequence()) {
            nodes.add(mapState.requireNode(nodeId));
        }
        return nodes;
    }

    private boolean hasAdjacentCombo(List<MapNodeSnapshot> nodes, NodeType left, NodeType right) {
        for (int i = 1; i < nodes.size(); i++) {
            if (nodes.get(i - 1).getNodeType() == left && nodes.get(i).getNodeType() == right) {
                return true;
            }
        }
        return false;
    }

    private boolean hasLongCombatChain(List<MapNodeSnapshot> nodes) {
        int chain = 0;
        for (MapNodeSnapshot node : nodes) {
            if (node.getNodeType() == NodeType.MONSTER || node.getNodeType() == NodeType.ELITE) {
                chain++;
                if (chain >= 3) {
                    return true;
                }
            } else {
                chain = 0;
            }
        }
        return false;
    }

    private int countNodeType(List<MapNodeSnapshot> nodes, NodeType nodeType) {
        int count = 0;
        for (MapNodeSnapshot node : nodes) {
            if (node.getNodeType() == nodeType) {
                count++;
            }
        }
        return count;
    }

    private double lowHpRiskMultiplier(double hpRatio) {
        if (hpRatio <= 0.30) {
            return 1.8;
        }
        if (hpRatio <= 0.50) {
            return 1.3;
        }
        if (hpRatio <= 0.70) {
            return 1.05;
        }
        return 0.85;
    }
}
