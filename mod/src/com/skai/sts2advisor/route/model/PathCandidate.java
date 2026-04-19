package com.skai.sts2advisor.route.model;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

public final class PathCandidate {
    private final String pathId;
    private final List<String> nodeSequence;

    public PathCandidate(String pathId, List<String> nodeSequence) {
        this.pathId = pathId;
        this.nodeSequence = Collections.unmodifiableList(new ArrayList<String>(nodeSequence));
    }

    public String getPathId() {
        return pathId;
    }

    public List<String> getNodeSequence() {
        return nodeSequence;
    }
}
