package com.skai.sts2advisor.route;

import com.skai.sts2advisor.route.model.PathStyle;

public final class BaselineScoringProfile {
    private final double riskWeight;
    private final double rewardWeight;
    private final double recoveryWeight;
    private final double structureWeight;
    private final double monsterRewardValue;
    private final double eliteRewardValue;
    private final double eventRewardValue;
    private final double treasureRewardValue;
    private final double eliteRiskPenalty;
    private final double unknownRiskPenalty;
    private final double combatRiskPenalty;
    private final double restBaseValue;
    private final double restLowHpBonus;
    private final double shopBaseValue;
    private final double shopGoldScale;
    private final double structureComboBonus;
    private final double highRiskChainPenalty;
    private final double shortPathPenalty;

    private BaselineScoringProfile(
        double riskWeight,
        double rewardWeight,
        double recoveryWeight,
        double structureWeight,
        double monsterRewardValue,
        double eliteRewardValue,
        double eventRewardValue,
        double treasureRewardValue,
        double eliteRiskPenalty,
        double unknownRiskPenalty,
        double combatRiskPenalty,
        double restBaseValue,
        double restLowHpBonus,
        double shopBaseValue,
        double shopGoldScale,
        double structureComboBonus,
        double highRiskChainPenalty,
        double shortPathPenalty
    ) {
        this.riskWeight = riskWeight;
        this.rewardWeight = rewardWeight;
        this.recoveryWeight = recoveryWeight;
        this.structureWeight = structureWeight;
        this.monsterRewardValue = monsterRewardValue;
        this.eliteRewardValue = eliteRewardValue;
        this.eventRewardValue = eventRewardValue;
        this.treasureRewardValue = treasureRewardValue;
        this.eliteRiskPenalty = eliteRiskPenalty;
        this.unknownRiskPenalty = unknownRiskPenalty;
        this.combatRiskPenalty = combatRiskPenalty;
        this.restBaseValue = restBaseValue;
        this.restLowHpBonus = restLowHpBonus;
        this.shopBaseValue = shopBaseValue;
        this.shopGoldScale = shopGoldScale;
        this.structureComboBonus = structureComboBonus;
        this.highRiskChainPenalty = highRiskChainPenalty;
        this.shortPathPenalty = shortPathPenalty;
    }

    public static BaselineScoringProfile forStyle(PathStyle style) {
        if (style == PathStyle.SAFE) {
            return new BaselineScoringProfile(
                1.6, 0.85, 1.3, 1.1,
                5.0, 11.0, 6.0, 7.0,
                8.0, 5.0, 3.0,
                5.0, 10.0,
                4.0, 0.06,
                4.0, 4.5, 3.0
            );
        }
        if (style == PathStyle.AGGRESSIVE) {
            return new BaselineScoringProfile(
                0.9, 1.45, 0.8, 1.0,
                6.5, 16.0, 7.0, 8.0,
                5.5, 3.0, 2.0,
                3.0, 7.0,
                3.0, 0.05,
                5.0, 2.5, 2.0
            );
        }
        return new BaselineScoringProfile(
            1.2, 1.15, 1.0, 1.0,
            5.5, 13.0, 6.5, 7.5,
            6.5, 4.0, 2.5,
            4.0, 8.5,
            3.5, 0.055,
            4.5, 3.5, 2.5
        );
    }

    public double getRiskWeight() {
        return riskWeight;
    }

    public double getRewardWeight() {
        return rewardWeight;
    }

    public double getRecoveryWeight() {
        return recoveryWeight;
    }

    public double getStructureWeight() {
        return structureWeight;
    }

    public double getMonsterRewardValue() {
        return monsterRewardValue;
    }

    public double getEliteRewardValue() {
        return eliteRewardValue;
    }

    public double getEventRewardValue() {
        return eventRewardValue;
    }

    public double getTreasureRewardValue() {
        return treasureRewardValue;
    }

    public double getEliteRiskPenalty() {
        return eliteRiskPenalty;
    }

    public double getUnknownRiskPenalty() {
        return unknownRiskPenalty;
    }

    public double getCombatRiskPenalty() {
        return combatRiskPenalty;
    }

    public double getRestBaseValue() {
        return restBaseValue;
    }

    public double getRestLowHpBonus() {
        return restLowHpBonus;
    }

    public double getShopBaseValue() {
        return shopBaseValue;
    }

    public double getShopGoldScale() {
        return shopGoldScale;
    }

    public double getStructureComboBonus() {
        return structureComboBonus;
    }

    public double getHighRiskChainPenalty() {
        return highRiskChainPenalty;
    }

    public double getShortPathPenalty() {
        return shortPathPenalty;
    }
}
