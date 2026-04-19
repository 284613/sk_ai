package com.skai.sts2advisor.route.debug;

import com.skai.sts2advisor.route.model.MapStateSnapshot;
import com.skai.sts2advisor.route.model.RecommendationReason;
import com.skai.sts2advisor.route.model.RouteRecommendationResult;
import com.skai.sts2advisor.route.model.RunStateSnapshot;
import com.skai.sts2advisor.route.model.ScoredPath;
import java.util.List;
import java.util.Locale;

public final class RouteDebugFormatter {
    public String format(MapStateSnapshot mapState, RunStateSnapshot runState, RouteRecommendationResult result) {
        StringBuilder builder = new StringBuilder();
        builder.append("Route Debug Summary").append('\n');
        builder.append("Current node: ").append(runState.getCurrentNodeId()).append('\n');
        builder.append("HP: ").append(runState.getCurrentHp()).append("/").append(runState.getMaxHp()).append('\n');
        builder.append("Gold: ").append(runState.getGold()).append('\n');
        builder.append("Map nodes: ").append(mapState.getNodes().size()).append('\n');
        builder.append("Enumerated paths: ").append(result.getAllPathScores().size()).append('\n');
        builder.append("Scoring version: ").append(result.getScoringVersion()).append('\n');

        for (ScoredPath path : result.getAllPathScores()) {
            builder.append("- ")
                .append(path.getPathId())
                .append(" total=")
                .append(format(path.getTotalScore()))
                .append(" risk=")
                .append(format(path.getRiskScore()))
                .append(" reward=")
                .append(format(path.getRewardScore()))
                .append(" recovery=")
                .append(format(path.getRecoveryScore()))
                .append(" structure=")
                .append(format(path.getStructureScore()))
                .append('\n');
            builder.append("  reasons: ").append(reasons(path.getReasons())).append('\n');
        }
        return builder.toString();
    }

    private String reasons(List<RecommendationReason> reasons) {
        if (reasons == null || reasons.isEmpty()) {
            return "[]";
        }
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < reasons.size(); i++) {
            if (i > 0) {
                builder.append(" | ");
            }
            builder.append(reasons.get(i).getMessage());
        }
        return builder.toString();
    }

    private String format(double value) {
        return String.format(Locale.US, "%.2f", value);
    }
}
