package com.skai.sts2advisor.route.ui;

import com.skai.sts2advisor.route.model.PathStyle;
import com.skai.sts2advisor.route.model.RouteRecommendationResult;
import com.skai.sts2advisor.route.model.ScoredPath;

public final class RouteRecommendationPanelModel {
    private final String modeLabel;
    private final ScoredPath topPath;
    private final ScoredPath secondPath;
    private final String summaryReason;

    public RouteRecommendationPanelModel(String modeLabel, ScoredPath topPath, ScoredPath secondPath, String summaryReason) {
        this.modeLabel = modeLabel;
        this.topPath = topPath;
        this.secondPath = secondPath;
        this.summaryReason = summaryReason;
    }

    public static RouteRecommendationPanelModel from(PathStyle style, RouteRecommendationResult result) {
        return new RouteRecommendationPanelModel(
            style.getDisplayName(),
            result.getTopPath(),
            result.getSecondPath(),
            result.getSummaryReason()
        );
    }

    public String getModeLabel() {
        return modeLabel;
    }

    public ScoredPath getTopPath() {
        return topPath;
    }

    public ScoredPath getSecondPath() {
        return secondPath;
    }

    public String getSummaryReason() {
        return summaryReason;
    }
}
