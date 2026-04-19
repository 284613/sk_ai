package com.skai.sts2advisor.route.model;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

public final class ScoredPath {
    private final String pathId;
    private final List<String> nodeSequence;
    private final double totalScore;
    private final double riskScore;
    private final double rewardScore;
    private final double recoveryScore;
    private final double structureScore;
    private final List<RecommendationReason> reasons;
    private final PathStyle styleTag;

    public ScoredPath(
        String pathId,
        List<String> nodeSequence,
        double totalScore,
        double riskScore,
        double rewardScore,
        double recoveryScore,
        double structureScore,
        List<RecommendationReason> reasons,
        PathStyle styleTag
    ) {
        this.pathId = pathId;
        this.nodeSequence = Collections.unmodifiableList(new ArrayList<String>(nodeSequence));
        this.totalScore = totalScore;
        this.riskScore = riskScore;
        this.rewardScore = rewardScore;
        this.recoveryScore = recoveryScore;
        this.structureScore = structureScore;
        this.reasons = Collections.unmodifiableList(new ArrayList<RecommendationReason>(reasons));
        this.styleTag = styleTag;
    }

    public String getPathId() {
        return pathId;
    }

    public List<String> getNodeSequence() {
        return nodeSequence;
    }

    public double getTotalScore() {
        return totalScore;
    }

    public double getRiskScore() {
        return riskScore;
    }

    public double getRewardScore() {
        return rewardScore;
    }

    public double getRecoveryScore() {
        return recoveryScore;
    }

    public double getStructureScore() {
        return structureScore;
    }

    public List<RecommendationReason> getReasons() {
        return reasons;
    }

    public PathStyle getStyleTag() {
        return styleTag;
    }
}
