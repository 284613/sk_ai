package com.skai.sts2advisor.route.model;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

public final class MapNodeSnapshot {
    private final String nodeId;
    private final int floorIndex;
    private final NodeType nodeType;
    private final int x;
    private final int y;
    private final List<String> outgoingEdges;
    private final List<String> incomingEdges;
    private final boolean reachableFromCurrent;

    public MapNodeSnapshot(
        String nodeId,
        int floorIndex,
        NodeType nodeType,
        int x,
        int y,
        List<String> outgoingEdges,
        List<String> incomingEdges,
        boolean reachableFromCurrent
    ) {
        this.nodeId = nodeId;
        this.floorIndex = floorIndex;
        this.nodeType = nodeType;
        this.x = x;
        this.y = y;
        this.outgoingEdges = Collections.unmodifiableList(new ArrayList<String>(outgoingEdges));
        this.incomingEdges = Collections.unmodifiableList(new ArrayList<String>(incomingEdges));
        this.reachableFromCurrent = reachableFromCurrent;
    }

    public String getNodeId() {
        return nodeId;
    }

    public int getFloorIndex() {
        return floorIndex;
    }

    public NodeType getNodeType() {
        return nodeType;
    }

    public int getX() {
        return x;
    }

    public int getY() {
        return y;
    }

    public List<String> getOutgoingEdges() {
        return outgoingEdges;
    }

    public List<String> getIncomingEdges() {
        return incomingEdges;
    }

    public boolean isReachableFromCurrent() {
        return reachableFromCurrent;
    }
}
