package com.skai.sts2advisor.route.debug;

import com.skai.sts2advisor.route.model.MapNodeSnapshot;
import com.skai.sts2advisor.route.model.MapStateSnapshot;
import com.skai.sts2advisor.route.model.NodeType;
import com.skai.sts2advisor.route.model.RunStateSnapshot;
import java.util.Arrays;

public final class DebugScenarioFactory {
    private DebugScenarioFactory() {
    }

    public static MapStateSnapshot sampleMap() {
        return new MapStateSnapshot(Arrays.asList(
            node("f0_start", 0, NodeType.START, 0, 0, new String[]{"f1_m1", "f1_shop"}, new String[]{}, true),
            node("f1_m1", 1, NodeType.MONSTER, 0, 1, new String[]{"f2_elite", "f2_event"}, new String[]{"f0_start"}, true),
            node("f1_shop", 1, NodeType.SHOP, 1, 1, new String[]{"f2_event", "f2_rest"}, new String[]{"f0_start"}, true),
            node("f2_elite", 2, NodeType.ELITE, 0, 2, new String[]{"f3_rest"}, new String[]{"f1_m1"}, true),
            node("f2_event", 2, NodeType.EVENT, 1, 2, new String[]{"f3_treasure", "f3_monster"}, new String[]{"f1_m1", "f1_shop"}, true),
            node("f2_rest", 2, NodeType.REST, 2, 2, new String[]{"f3_elite"}, new String[]{"f1_shop"}, true),
            node("f3_rest", 3, NodeType.REST, 0, 3, new String[]{"f4_monster"}, new String[]{"f2_elite"}, true),
            node("f3_treasure", 3, NodeType.TREASURE, 1, 3, new String[]{"f4_unknown"}, new String[]{"f2_event"}, true),
            node("f3_monster", 3, NodeType.MONSTER, 2, 3, new String[]{"f4_unknown"}, new String[]{"f2_event"}, true),
            node("f3_elite", 3, NodeType.ELITE, 3, 3, new String[]{"f4_rest"}, new String[]{"f2_rest"}, true),
            node("f4_monster", 4, NodeType.MONSTER, 0, 4, new String[]{}, new String[]{"f3_rest"}, true),
            node("f4_unknown", 4, NodeType.UNKNOWN, 1, 4, new String[]{}, new String[]{"f3_treasure", "f3_monster"}, true),
            node("f4_rest", 4, NodeType.REST, 2, 4, new String[]{}, new String[]{"f3_elite"}, true)
        ));
    }

    public static RunStateSnapshot sampleRunState() {
        return new RunStateSnapshot(
            "debug-run-001",
            1,
            0,
            28,
            70,
            154,
            1,
            3,
            "IRONCLAD",
            10,
            "f0_start"
        );
    }

    private static MapNodeSnapshot node(
        String nodeId,
        int floorIndex,
        NodeType nodeType,
        int x,
        int y,
        String[] outgoingEdges,
        String[] incomingEdges,
        boolean reachableFromCurrent
    ) {
        return new MapNodeSnapshot(
            nodeId,
            floorIndex,
            nodeType,
            x,
            y,
            Arrays.asList(outgoingEdges),
            Arrays.asList(incomingEdges),
            reachableFromCurrent
        );
    }
}
