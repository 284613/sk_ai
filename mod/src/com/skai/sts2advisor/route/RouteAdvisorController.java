package com.skai.sts2advisor.route;

import com.skai.sts2advisor.route.debug.RouteDebugFormatter;
import com.skai.sts2advisor.route.model.RouteRecommendationResult;
import com.skai.sts2advisor.route.ui.RouteRecommendationPanelModel;
import com.skai.sts2advisor.route.ui.RouteRecommendationTextRenderer;

/**
 * 薄控制器：状态提供 -> 推荐引擎 -> 展示器。
 */
public final class RouteAdvisorController {
    private final RouteAdvisorConfig config;
    private final RouteRecommendationEngine engine;
    private final RouteRecommendationTextRenderer textRenderer;
    private final RouteDebugFormatter debugFormatter;

    public RouteAdvisorController(RouteAdvisorConfig config) {
        this(
            config,
            new RouteRecommendationEngine(
                new RoutePathEnumerator(),
                new BaselineRouteScorer(),
                config.getLookaheadDepth()
            ),
            new RouteRecommendationTextRenderer(),
            new RouteDebugFormatter()
        );
    }

    public RouteAdvisorController(
        RouteAdvisorConfig config,
        RouteRecommendationEngine engine,
        RouteRecommendationTextRenderer textRenderer,
        RouteDebugFormatter debugFormatter
    ) {
        this.config = config;
        this.engine = engine;
        this.textRenderer = textRenderer;
        this.debugFormatter = debugFormatter;
    }

    public RouteRecommendationResult evaluate(RouteEvaluationContext context) {
        if (!config.isEnabled()) {
            return null;
        }
        return engine.recommend(context.getMapState(), context.getRunState(), config.getStyle());
    }

    public void publish(RouteEvaluationContext context, RouteRecommendationPresenter presenter) {
        RouteRecommendationResult result = evaluate(context);
        if (result == null) {
            return;
        }
        RouteRecommendationPanelModel panelModel = RouteRecommendationPanelModel.from(config.getStyle(), result);
        String debugText = config.isDebugLoggingEnabled()
            ? debugFormatter.format(context.getMapState(), context.getRunState(), result)
            : "";
        presenter.present(panelModel, result, textRenderer.render(panelModel) + debugText);
    }
}
