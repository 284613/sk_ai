package com.skai.sts2advisor.route;

import com.skai.sts2advisor.route.model.MapStateSnapshot;
import com.skai.sts2advisor.route.model.PathCandidate;
import com.skai.sts2advisor.route.model.PathStyle;
import com.skai.sts2advisor.route.model.RunStateSnapshot;
import com.skai.sts2advisor.route.model.ScoredPath;
import java.util.List;

/**
 * 评分器接口刻意保持很薄，后续可以用日志回放评分器或模型评分器替换。
 */
public interface RouteScorer {
    List<ScoredPath> score(
        MapStateSnapshot mapState,
        RunStateSnapshot runState,
        List<PathCandidate> candidates,
        PathStyle style
    );

    String getScoringVersion();
}
