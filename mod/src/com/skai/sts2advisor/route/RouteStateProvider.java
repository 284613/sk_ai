package com.skai.sts2advisor.route;

/**
 * 真实游戏接入时，只需要把地图界面和当前运行状态映射成 RouteEvaluationContext。
 */
public interface RouteStateProvider {
    RouteEvaluationContext capture();
}
