package com.skai.sts2advisor.route.serialization;

import com.skai.sts2advisor.route.RouteEvaluationContext;
import com.skai.sts2advisor.route.model.MapNodeSnapshot;
import com.skai.sts2advisor.route.model.RecommendationReason;
import com.skai.sts2advisor.route.model.RouteRecommendationResult;
import com.skai.sts2advisor.route.model.RunStateSnapshot;
import com.skai.sts2advisor.route.model.ScoredPath;
import java.util.List;
import java.util.Locale;

/**
 * 为后续日志系统和外部评分器替换预留统一 JSON 出口。
 * 当前实现只做最小手写序列化，避免提前引入 JSON 依赖。
 */
public final class RouteRecommendationJsonSerializer {
    public String serialize(RouteEvaluationContext context, RouteRecommendationResult result) {
        StringBuilder builder = new StringBuilder();
        builder.append("{");
        builder.append("\"schema_version\":\"route-advisor-v1\",");
        builder.append("\"run_state\":").append(runStateJson(context.getRunState())).append(",");
        builder.append("\"map_state\":").append(mapStateJson(context)).append(",");
        builder.append("\"recommendation_result\":").append(resultJson(result));
        builder.append("}");
        return builder.toString();
    }

    private String runStateJson(RunStateSnapshot runState) {
        StringBuilder builder = new StringBuilder();
        builder.append("{");
        appendField(builder, "run_id", runState.getRunId()); builder.append(",");
        appendNumber(builder, "act_index", runState.getActIndex()); builder.append(",");
        appendNumber(builder, "current_floor", runState.getCurrentFloor()); builder.append(",");
        appendNumber(builder, "current_hp", runState.getCurrentHp()); builder.append(",");
        appendNumber(builder, "max_hp", runState.getMaxHp()); builder.append(",");
        appendNumber(builder, "gold", runState.getGold()); builder.append(",");
        appendNumber(builder, "potion_count", runState.getPotionCount()); builder.append(",");
        appendNumber(builder, "potion_slots", runState.getPotionSlots()); builder.append(",");
        appendField(builder, "character_id", runState.getCharacterId()); builder.append(",");
        appendNumber(builder, "ascension_or_difficulty", runState.getAscensionOrDifficulty()); builder.append(",");
        appendField(builder, "current_node_id", runState.getCurrentNodeId());
        builder.append("}");
        return builder.toString();
    }

    private String mapStateJson(RouteEvaluationContext context) {
        StringBuilder builder = new StringBuilder();
        builder.append("{\"nodes\":[");
        List<MapNodeSnapshot> nodes = context.getMapState().getNodes();
        for (int i = 0; i < nodes.size(); i++) {
            if (i > 0) {
                builder.append(",");
            }
            builder.append(nodeJson(nodes.get(i)));
        }
        builder.append("]}");
        return builder.toString();
    }

    private String nodeJson(MapNodeSnapshot node) {
        StringBuilder builder = new StringBuilder();
        builder.append("{");
        appendField(builder, "node_id", node.getNodeId()); builder.append(",");
        appendNumber(builder, "floor_index", node.getFloorIndex()); builder.append(",");
        appendField(builder, "node_type", node.getNodeType().name()); builder.append(",");
        appendNumber(builder, "x", node.getX()); builder.append(",");
        appendNumber(builder, "y", node.getY()); builder.append(",");
        builder.append("\"outgoing_edges\":").append(stringArray(node.getOutgoingEdges())).append(",");
        builder.append("\"incoming_edges\":").append(stringArray(node.getIncomingEdges())).append(",");
        builder.append("\"reachable_from_current\":").append(node.isReachableFromCurrent());
        builder.append("}");
        return builder.toString();
    }

    private String resultJson(RouteRecommendationResult result) {
        StringBuilder builder = new StringBuilder();
        builder.append("{");
        builder.append("\"top_path\":").append(scoredPathJson(result.getTopPath())).append(",");
        builder.append("\"second_path\":").append(scoredPathJson(result.getSecondPath())).append(",");
        builder.append("\"all_path_scores\":[");
        List<ScoredPath> paths = result.getAllPathScores();
        for (int i = 0; i < paths.size(); i++) {
            if (i > 0) {
                builder.append(",");
            }
            builder.append(scoredPathJson(paths.get(i)));
        }
        builder.append("],");
        appendField(builder, "summary_reason", result.getSummaryReason()); builder.append(",");
        appendField(builder, "scoring_version", result.getScoringVersion());
        builder.append("}");
        return builder.toString();
    }

    private String scoredPathJson(ScoredPath path) {
        if (path == null) {
            return "null";
        }
        StringBuilder builder = new StringBuilder();
        builder.append("{");
        appendField(builder, "path_id", path.getPathId()); builder.append(",");
        builder.append("\"node_sequence\":").append(stringArray(path.getNodeSequence())).append(",");
        appendFloat(builder, "total_score", path.getTotalScore()); builder.append(",");
        appendFloat(builder, "risk_score", path.getRiskScore()); builder.append(",");
        appendFloat(builder, "reward_score", path.getRewardScore()); builder.append(",");
        appendFloat(builder, "recovery_score", path.getRecoveryScore()); builder.append(",");
        appendFloat(builder, "structure_score", path.getStructureScore()); builder.append(",");
        appendField(builder, "style_tag", path.getStyleTag().name()); builder.append(",");
        builder.append("\"reasons\":[");
        List<RecommendationReason> reasons = path.getReasons();
        for (int i = 0; i < reasons.size(); i++) {
            if (i > 0) {
                builder.append(",");
            }
            builder.append(reasonJson(reasons.get(i)));
        }
        builder.append("]");
        builder.append("}");
        return builder.toString();
    }

    private String reasonJson(RecommendationReason reason) {
        StringBuilder builder = new StringBuilder();
        builder.append("{");
        appendField(builder, "category", reason.getCategory()); builder.append(",");
        appendField(builder, "code", reason.getCode()); builder.append(",");
        appendField(builder, "message", reason.getMessage()); builder.append(",");
        appendFloat(builder, "impact", reason.getImpact());
        builder.append("}");
        return builder.toString();
    }

    private String stringArray(List<String> values) {
        StringBuilder builder = new StringBuilder();
        builder.append("[");
        for (int i = 0; i < values.size(); i++) {
            if (i > 0) {
                builder.append(",");
            }
            builder.append("\"").append(escape(values.get(i))).append("\"");
        }
        builder.append("]");
        return builder.toString();
    }

    private void appendField(StringBuilder builder, String name, String value) {
        builder.append("\"").append(name).append("\":");
        if (value == null) {
            builder.append("null");
            return;
        }
        builder.append("\"").append(escape(value)).append("\"");
    }

    private void appendNumber(StringBuilder builder, String name, int value) {
        builder.append("\"").append(name).append("\":").append(value);
    }

    private void appendFloat(StringBuilder builder, String name, double value) {
        builder.append("\"")
            .append(name)
            .append("\":")
            .append(String.format(Locale.US, "%.2f", value));
    }

    private String escape(String value) {
        return value.replace("\\", "\\\\").replace("\"", "\\\"");
    }
}
