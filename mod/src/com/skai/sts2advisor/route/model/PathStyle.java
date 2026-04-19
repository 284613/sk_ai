package com.skai.sts2advisor.route.model;

public enum PathStyle {
    SAFE,
    BALANCED,
    AGGRESSIVE;

    public String getDisplayName() {
        return name().toLowerCase();
    }
}
