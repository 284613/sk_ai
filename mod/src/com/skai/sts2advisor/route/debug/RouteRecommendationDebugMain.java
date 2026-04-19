package com.skai.sts2advisor.route.debug;

import com.skai.sts2advisor.route.RouteAdvisorConfig;
import com.skai.sts2advisor.route.RouteAdvisorController;
import com.skai.sts2advisor.route.RouteEvaluationContext;
import com.skai.sts2advisor.route.model.PathStyle;
import com.skai.sts2advisor.route.model.RouteRecommendationResult;
import com.skai.sts2advisor.route.serialization.RouteRecommendationJsonSerializer;

public final class RouteRecommendationDebugMain {
    public static void main(String[] args) {
        PathStyle style = parseStyle(args);
        RouteEvaluationContext context = new RouteEvaluationContext(
            DebugScenarioFactory.sampleMap(),
            DebugScenarioFactory.sampleRunState()
        );
        RouteAdvisorController controller = new RouteAdvisorController(
            new RouteAdvisorConfig(true, true, style, 5)
        );
        RouteRecommendationResult result = controller.evaluate(context);
        controller.publish(context, new ConsoleRecommendationPresenter());
        System.out.println("JSON Export:");
        System.out.println(new RouteRecommendationJsonSerializer().serialize(context, result));
    }

    private static PathStyle parseStyle(String[] args) {
        if (args == null || args.length == 0) {
            return PathStyle.BALANCED;
        }
        String raw = args[0].trim().toUpperCase();
        for (PathStyle style : PathStyle.values()) {
            if (style.name().equals(raw)) {
                return style;
            }
        }
        return PathStyle.BALANCED;
    }
}
