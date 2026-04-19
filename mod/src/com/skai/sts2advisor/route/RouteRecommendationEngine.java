package com.skai.sts2advisor.route;

import com.skai.sts2advisor.route.model.MapStateSnapshot;
import com.skai.sts2advisor.route.model.PathStyle;
import com.skai.sts2advisor.route.model.RouteRecommendationResult;
import com.skai.sts2advisor.route.model.RunStateSnapshot;
import com.skai.sts2advisor.route.model.ScoredPath;
import java.util.Collections;
import java.util.List;

/**
 * 最小推荐编排层：路径枚举 -> 路径评分 -> Top-2 结果组装。
 */
public final class RouteRecommendationEngine {
    private final RoutePathEnumerator pathEnumerator;
    private final RouteScorer routeScorer;
    private final int lookaheadDepth;

    public RouteRecommendationEngine() {
        this(new RoutePathEnumerator(), new BaselineRouteScorer(), RoutePathEnumerator.DEFAULT_LOOKAHEAD_DEPTH);
    }

    public RouteRecommendationEngine(RoutePathEnumerator pathEnumerator, RouteScorer routeScorer, int lookaheadDepth) {
        this.pathEnumerator = pathEnumerator;
        this.routeScorer = routeScorer;
        this.lookaheadDepth = lookaheadDepth;
    }

    public RouteRecommendationResult recommend(
        MapStateSnapshot mapState,
        RunStateSnapshot runState,
        PathStyle style
    ) {
        List<ScoredPath> scoredPaths = routeScorer.score(
            mapState,
            runState,
            pathEnumerator.enumerate(mapState, runState, lookaheadDepth),
            style
        );
        if (scoredPaths.isEmpty()) {
            return new RouteRecommendationResult(
                null,
                null,
                Collections.<ScoredPath>emptyList(),
                "当前节点后没有可枚举路径",
                routeScorer.getScoringVersion()
            );
        }

        ScoredPath topPath = scoredPaths.get(0);
        ScoredPath secondPath = scoredPaths.size() > 1 ? scoredPaths.get(1) : null;
        String summaryReason = topPath.getReasons().isEmpty()
            ? "已按当前模式完成路径排序"
            : topPath.getReasons().get(0).getMessage();

        return new RouteRecommendationResult(
            topPath,
            secondPath,
            scoredPaths,
            summaryReason,
            routeScorer.getScoringVersion()
        );
    }
}
