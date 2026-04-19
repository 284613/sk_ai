package com.skai.sts2advisor.route;

import com.skai.sts2advisor.route.model.RouteRecommendationResult;
import com.skai.sts2advisor.route.ui.RouteRecommendationPanelModel;

/**
 * 真实游戏接入时，只需要实现一个展示器，把面板模型挂到 debug panel 或 overlay。
 */
public interface RouteRecommendationPresenter {
    void present(RouteRecommendationPanelModel panelModel, RouteRecommendationResult result, String debugText);
}
