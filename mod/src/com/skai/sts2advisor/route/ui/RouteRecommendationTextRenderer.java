package com.skai.sts2advisor.route.ui;

import com.skai.sts2advisor.route.model.RecommendationReason;
import com.skai.sts2advisor.route.model.ScoredPath;
import java.util.List;
import java.util.Locale;

/**
 * 当前只输出文本，便于先挂到 debug panel。
 * 真正的游戏内 overlay/面板渲染，待确认 StS2 mod UI 接口后再接入。
 */
public final class RouteRecommendationTextRenderer {
    public String render(RouteRecommendationPanelModel model) {
        StringBuilder builder = new StringBuilder();
        builder.append("Route Advisor").append('\n');
        builder.append("Mode: ").append(model.getModeLabel()).append('\n');
        builder.append(renderPath("Top-1", model.getTopPath()));
        builder.append(renderPath("Top-2", model.getSecondPath()));
        builder.append("Summary: ").append(model.getSummaryReason()).append('\n');
        builder.append("Display limitation: current renderer outputs text for a debug panel; game overlay wiring is pending SDK confirmation.").append('\n');
        return builder.toString();
    }

    private String renderPath(String label, ScoredPath path) {
        if (path == null) {
            return label + ": unavailable\n";
        }

        StringBuilder builder = new StringBuilder();
        builder.append(label)
            .append(": score=")
            .append(format(path.getTotalScore()))
            .append(" path=")
            .append(path.getNodeSequence())
            .append('\n');
        builder.append("  reason: ").append(primaryReason(path.getReasons())).append('\n');
        return builder.toString();
    }

    private String primaryReason(List<RecommendationReason> reasons) {
        if (reasons == null || reasons.isEmpty()) {
            return "无";
        }
        return reasons.get(0).getMessage();
    }

    private String format(double value) {
        return String.format(Locale.US, "%.2f", value);
    }
}
