package com.skai.sts2advisor.route.model;

public final class RunStateSnapshot {
    private final String runId;
    private final int actIndex;
    private final int currentFloor;
    private final int currentHp;
    private final int maxHp;
    private final int gold;
    private final int potionCount;
    private final int potionSlots;
    private final String characterId;
    private final int ascensionOrDifficulty;
    private final String currentNodeId;

    public RunStateSnapshot(
        String runId,
        int actIndex,
        int currentFloor,
        int currentHp,
        int maxHp,
        int gold,
        int potionCount,
        int potionSlots,
        String characterId,
        int ascensionOrDifficulty,
        String currentNodeId
    ) {
        this.runId = runId;
        this.actIndex = actIndex;
        this.currentFloor = currentFloor;
        this.currentHp = currentHp;
        this.maxHp = maxHp;
        this.gold = gold;
        this.potionCount = potionCount;
        this.potionSlots = potionSlots;
        this.characterId = characterId;
        this.ascensionOrDifficulty = ascensionOrDifficulty;
        this.currentNodeId = currentNodeId;
    }

    public String getRunId() {
        return runId;
    }

    public int getActIndex() {
        return actIndex;
    }

    public int getCurrentFloor() {
        return currentFloor;
    }

    public int getCurrentHp() {
        return currentHp;
    }

    public int getMaxHp() {
        return maxHp;
    }

    public int getGold() {
        return gold;
    }

    public int getPotionCount() {
        return potionCount;
    }

    public int getPotionSlots() {
        return potionSlots;
    }

    public String getCharacterId() {
        return characterId;
    }

    public int getAscensionOrDifficulty() {
        return ascensionOrDifficulty;
    }

    public String getCurrentNodeId() {
        return currentNodeId;
    }

    public double getHpRatio() {
        if (maxHp <= 0) {
            return 0.0;
        }
        return (double) currentHp / (double) maxHp;
    }
}
