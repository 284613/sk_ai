package com.skai.sts2advisor.route;

import com.skai.sts2advisor.route.model.MapStateSnapshot;
import com.skai.sts2advisor.route.model.RunStateSnapshot;

public final class RouteEvaluationContext {
    private final MapStateSnapshot mapState;
    private final RunStateSnapshot runState;

    public RouteEvaluationContext(MapStateSnapshot mapState, RunStateSnapshot runState) {
        this.mapState = mapState;
        this.runState = runState;
    }

    public MapStateSnapshot getMapState() {
        return mapState;
    }

    public RunStateSnapshot getRunState() {
        return runState;
    }
}
