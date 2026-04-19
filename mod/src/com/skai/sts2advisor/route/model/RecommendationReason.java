package com.skai.sts2advisor.route.model;

public final class RecommendationReason {
    private final String category;
    private final String code;
    private final String message;
    private final double impact;

    public RecommendationReason(String category, String code, String message, double impact) {
        this.category = category;
        this.code = code;
        this.message = message;
        this.impact = impact;
    }

    public String getCategory() {
        return category;
    }

    public String getCode() {
        return code;
    }

    public String getMessage() {
        return message;
    }

    public double getImpact() {
        return impact;
    }
}
