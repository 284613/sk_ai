package com.skai.sts2advisor.route.debug;

import com.skai.sts2advisor.route.RouteRecommendationPresenter;
import com.skai.sts2advisor.route.model.RouteRecommendationResult;
import com.skai.sts2advisor.route.ui.RouteRecommendationPanelModel;

public final class ConsoleRecommendationPresenter implements RouteRecommendationPresenter {
    public void present(RouteRecommendationPanelModel panelModel, RouteRecommendationResult result, String debugText) {
        System.out.println(debugText);
    }
}
