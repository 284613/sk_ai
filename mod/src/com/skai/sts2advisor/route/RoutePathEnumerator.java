package com.skai.sts2advisor.route;

import com.skai.sts2advisor.route.model.MapNodeSnapshot;
import com.skai.sts2advisor.route.model.MapStateSnapshot;
import com.skai.sts2advisor.route.model.PathCandidate;
import com.skai.sts2advisor.route.model.RunStateSnapshot;
import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

/**
 * 从当前节点出发做固定深度 DFS，只枚举“当前可走”的后续路径。
 *
 * 当前实现：
 * - 默认向前看 5 个节点，避免整图无界遍历。
 * - 只在达到深度上限或走到叶子节点时产出路径。
 * - 通过 node id 序列字符串去重。
 *
 * 当前限制：
 * - 还没有按游戏真实规则做更细的剪枝或支配关系合并。
 * - 还没有区分“事件后可能分支变化”等动态地图因素。
 *
 * 未来扩展：
 * - 接入真实地图状态后，可加入按层数、风险预算和收益上界的剪枝。
 * - 可补充更稳定的路径 canonicalization，减少等价路径重复评分。
 */
public final class RoutePathEnumerator {
    public static final int DEFAULT_LOOKAHEAD_DEPTH = 5;

    public List<PathCandidate> enumerate(MapStateSnapshot mapState, RunStateSnapshot runState) {
        return enumerate(mapState, runState, DEFAULT_LOOKAHEAD_DEPTH);
    }

    public List<PathCandidate> enumerate(MapStateSnapshot mapState, RunStateSnapshot runState, int maxDepth) {
        if (maxDepth <= 0) {
            throw new IllegalArgumentException("maxDepth must be > 0");
        }

        MapNodeSnapshot currentNode = mapState.requireNode(runState.getCurrentNodeId());
        Map<String, PathCandidate> dedupedPaths = new LinkedHashMap<String, PathCandidate>();
        for (String nextNodeId : currentNode.getOutgoingEdges()) {
            ArrayList<String> seed = new ArrayList<String>();
            seed.add(nextNodeId);
            dfs(mapState, nextNodeId, seed, maxDepth, dedupedPaths);
        }
        return new ArrayList<PathCandidate>(dedupedPaths.values());
    }

    private void dfs(
        MapStateSnapshot mapState,
        String currentNodeId,
        List<String> path,
        int maxDepth,
        Map<String, PathCandidate> dedupedPaths
    ) {
        MapNodeSnapshot node = mapState.requireNode(currentNodeId);
        boolean shouldStop = path.size() >= maxDepth || node.getOutgoingEdges().isEmpty();
        if (shouldStop) {
            String pathId = buildPathId(path);
            dedupedPaths.put(pathId, new PathCandidate(pathId, path));
            return;
        }

        for (String nextNodeId : node.getOutgoingEdges()) {
            if (path.contains(nextNodeId)) {
                continue;
            }
            ArrayList<String> nextPath = new ArrayList<String>(path);
            nextPath.add(nextNodeId);
            dfs(mapState, nextNodeId, nextPath, maxDepth, dedupedPaths);
        }
    }

    private String buildPathId(List<String> path) {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < path.size(); i++) {
            if (i > 0) {
                builder.append("->");
            }
            builder.append(path.get(i));
        }
        return builder.toString();
    }
}
