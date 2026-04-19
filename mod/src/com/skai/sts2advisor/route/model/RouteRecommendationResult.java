package com.skai.sts2advisor.route.model;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

public final class RouteRecommendationResult {
    private final ScoredPath topPath;
    private final ScoredPath secondPath;
    private final List<ScoredPath> allPathScores;
    private final String summaryReason;
    private final String scoringVersion;

    public RouteRecommendationResult(
        ScoredPath topPath,
        ScoredPath secondPath,
        List<ScoredPath> allPathScores,
        String summaryReason,
        String scoringVersion
    ) {
        this.topPath = topPath;
        this.secondPath = secondPath;
        this.allPathScores = Collections.unmodifiableList(new ArrayList<ScoredPath>(allPathScores));
        this.summaryReason = summaryReason;
        this.scoringVersion = scoringVersion;
    }

    public ScoredPath getTopPath() {
        return topPath;
    }

    public ScoredPath getSecondPath() {
        return secondPath;
    }

    public List<ScoredPath> getAllPathScores() {
        return allPathScores;
    }

    public String getSummaryReason() {
        return summaryReason;
    }

    public String getScoringVersion() {
        return scoringVersion;
    }
}
