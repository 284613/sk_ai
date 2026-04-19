package com.skai.sts2advisor.route;

import com.skai.sts2advisor.route.model.PathStyle;

public final class RouteAdvisorConfig {
    private final boolean enabled;
    private final boolean debugLoggingEnabled;
    private final PathStyle style;
    private final int lookaheadDepth;

    public RouteAdvisorConfig(boolean enabled, boolean debugLoggingEnabled, PathStyle style, int lookaheadDepth) {
        this.enabled = enabled;
        this.debugLoggingEnabled = debugLoggingEnabled;
        this.style = style;
        this.lookaheadDepth = lookaheadDepth;
    }

    public static RouteAdvisorConfig defaultConfig() {
        return new RouteAdvisorConfig(true, true, PathStyle.BALANCED, RoutePathEnumerator.DEFAULT_LOOKAHEAD_DEPTH);
    }

    public boolean isEnabled() {
        return enabled;
    }

    public boolean isDebugLoggingEnabled() {
        return debugLoggingEnabled;
    }

    public PathStyle getStyle() {
        return style;
    }

    public int getLookaheadDepth() {
        return lookaheadDepth;
    }
}
