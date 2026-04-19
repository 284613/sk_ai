package com.skai.sts2advisor.route.model;

import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public final class MapStateSnapshot {
    private final List<MapNodeSnapshot> nodes;
    private final Map<String, MapNodeSnapshot> nodesById;

    public MapStateSnapshot(List<MapNodeSnapshot> nodes) {
        this.nodes = Collections.unmodifiableList(new ArrayList<MapNodeSnapshot>(nodes));
        this.nodesById = new HashMap<String, MapNodeSnapshot>();
        for (MapNodeSnapshot node : nodes) {
            this.nodesById.put(node.getNodeId(), node);
        }
    }

    public List<MapNodeSnapshot> getNodes() {
        return nodes;
    }

    public MapNodeSnapshot getNode(String nodeId) {
        return nodesById.get(nodeId);
    }

    public MapNodeSnapshot requireNode(String nodeId) {
        MapNodeSnapshot node = nodesById.get(nodeId);
        if (node == null) {
            throw new IllegalArgumentException("Unknown node id: " + nodeId);
        }
        return node;
    }
}
