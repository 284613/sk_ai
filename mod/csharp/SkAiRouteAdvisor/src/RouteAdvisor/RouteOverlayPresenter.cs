using Godot;
using System.Collections;
using System.Reflection;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Map;

namespace SkAiRouteAdvisor.RouteAdvisor;

internal sealed class RouteOverlayPresenter
{
    private const string OverlayNodeName = "__SkAiRouteAdvisorOverlay";
    private const string ScoreLabelPrefix = "__SkAiRouteAdvisorScore";
    private static readonly Dictionary<TextureRect, (Color color, Vector2 scale)> OriginalTickProperties = new();
    private static readonly FieldInfo? PathsField = typeof(NMapScreen).GetField("_paths", BindingFlags.NonPublic | BindingFlags.Instance);

    public void Present(RouteRecommendationSummary summary)
    {
        var mapScreen = NMapScreen.Instance;
        if (mapScreen == null || !mapScreen.IsOpen)
        {
            return;
        }

        var panel = EnsureOverlay(mapScreen);
        UpdateLabels(panel, summary);
        UpdateRouteScoreLabels(mapScreen, summary);
        UpdateHighlightedSegments(mapScreen, summary);
    }

    private static PanelContainer EnsureOverlay(NMapScreen mapScreen)
    {
        var existing = mapScreen.GetNodeOrNull<PanelContainer>(OverlayNodeName);
        if (existing != null)
        {
            return existing;
        }

        var panel = new PanelContainer
        {
            Name = OverlayNodeName,
            Position = new Vector2(24, 96),
            Size = new Vector2(390, 86),
            ZIndex = 500,
        };

        var box = new VBoxContainer
        {
            Name = "VBox",
        };

        panel.AddChild(box);
        box.AddChild(new Label { Name = "Title" });
        box.AddChild(new Label { Name = "Best" });
        box.AddChild(new Label { Name = "Reason" });

        mapScreen.AddChild(panel);
        return panel;
    }

    private static void UpdateLabels(PanelContainer panel, RouteRecommendationSummary summary)
    {
        var top1 = summary.RankedRoutes.ElementAtOrDefault(0);

        panel.GetNode<Label>("VBox/Title").Text =
            $"Route Advisor | {summary.Mode.ToString().ToLowerInvariant()} | F6 | HP {summary.CurrentHp}/{summary.MaxHp} | Gold {summary.Gold}";
        panel.GetNode<Label>("VBox/Best").Text = top1 == null
            ? "Best: unavailable"
            : $"Best {top1.TotalScore:F1} | {ShortPath(top1.DisplayPath)}";
        panel.GetNode<Label>("VBox/Reason").Text = top1?.Reasons.FirstOrDefault()?.Message ?? "暂无推荐理由";
    }

    private static void UpdateRouteScoreLabels(NMapScreen mapScreen, RouteRecommendationSummary summary)
    {
        var mapPointNodes = GetMapPointNodes(mapScreen).ToList();

        foreach (var mapPointNode in mapPointNodes)
        {
            foreach (var child in mapPointNode.GetChildren())
            {
                if (child is Node node && node.Name.ToString().StartsWith(ScoreLabelPrefix, StringComparison.Ordinal))
                {
                    mapPointNode.RemoveChild(node);
                    node.QueueFree();
                }
            }
        }

        var top1 = summary.RankedRoutes.ElementAtOrDefault(0);

        if (top1 != null)
        {
            AttachRouteLabels(mapPointNodes, top1, new Color(1f, 0.88f, 0.28f, 1f));
        }
    }

    private static void UpdateHighlightedSegments(NMapScreen mapScreen, RouteRecommendationSummary summary)
    {
        ClearHighlightedSegments();

        var top1 = summary.RankedRoutes.ElementAtOrDefault(0);
        if (top1 == null || PathsField == null || top1.Points.Count < 2)
        {
            return;
        }

        if (PathsField.GetValue(mapScreen) is not IDictionary paths)
        {
            return;
        }

        for (var index = 1; index < top1.Points.Count; index++)
        {
            var from = top1.Points[index - 1].coord;
            var to = top1.Points[index].coord;
            var keyForward = (from, to);
            var keyBackward = (to, from);
            var pathTicks = paths.Contains(keyForward) ? paths[keyForward] : paths.Contains(keyBackward) ? paths[keyBackward] : null;
            if (pathTicks is not IReadOnlyList<TextureRect> ticks)
            {
                continue;
            }

            foreach (var tick in ticks)
            {
                if (tick == null || !GodotObject.IsInstanceValid(tick))
                {
                    continue;
                }

                if (!OriginalTickProperties.ContainsKey(tick))
                {
                    OriginalTickProperties[tick] = (tick.Modulate, tick.Scale);
                }

                tick.Modulate = new Color(1f, 0.83f, 0.25f, 1f);
                tick.Scale = new Vector2(1.28f, 1.28f);
            }
        }
    }

    private static void ClearHighlightedSegments()
    {
        var stale = new List<TextureRect>();
        foreach (var kvp in OriginalTickProperties)
        {
            if (!GodotObject.IsInstanceValid(kvp.Key))
            {
                stale.Add(kvp.Key);
                continue;
            }

            kvp.Key.Modulate = kvp.Value.color;
            kvp.Key.Scale = kvp.Value.scale;
        }

        foreach (var tick in stale)
        {
            OriginalTickProperties.Remove(tick);
        }
    }

    private static void AttachRouteLabels(
        IReadOnlyList<NMapPoint> mapPointNodes,
        ScoredRoute route,
        Color color
    )
    {
        for (var index = 1; index < route.NodeScores.Count; index++)
        {
            var fromPoint = route.Points[index - 1];
            var toPoint = route.Points[index];
            var nodeScore = route.NodeScores[index];

            var fromNode = mapPointNodes.FirstOrDefault(node => ReferenceEquals(node.Point, fromPoint));
            var toNode = mapPointNodes.FirstOrDefault(node => ReferenceEquals(node.Point, toPoint));
            if (fromNode == null || toNode == null)
            {
                continue;
            }

            var midpointOffset = ((fromNode.GlobalPosition + toNode.GlobalPosition) * 0.5f) - toNode.GlobalPosition;
            var segmentVector = toNode.GlobalPosition - fromNode.GlobalPosition;
            var normal = segmentVector.Length() > 0.01f
                ? new Vector2(-segmentVector.Y, segmentVector.X).Normalized() * 12f
                : new Vector2(0, -12);
            var finalOffset = midpointOffset + normal;

            var label = new Label
            {
                Name = $"{ScoreLabelPrefix}_Label_{index}",
                Text = FormatDelta(nodeScore.DeltaScore),
                Modulate = color,
            };
            label.AddThemeFontSizeOverride("font_size", 12);

            var container = new PanelContainer
            {
                Name = $"{ScoreLabelPrefix}_{index}",
                Position = finalOffset + new Vector2(-14, -10),
                MouseFilter = Control.MouseFilterEnum.Ignore,
            };
            container.AddThemeStyleboxOverride("panel", BuildScoreStyleBox());
            container.AddChild(label);

            toNode.AddChild(container);
        }
    }

    private static IEnumerable<NMapPoint> GetMapPointNodes(Node root)
    {
        foreach (var child in root.GetChildren())
        {
            if (child is NMapPoint mapPoint)
            {
                yield return mapPoint;
            }

            foreach (var nested in GetMapPointNodes(child))
            {
                yield return nested;
            }
        }
    }

    private static string FormatDelta(double value)
    {
        return value >= 0 ? $"+{value:F1}" : $"{value:F1}";
    }

    private static StyleBoxFlat BuildScoreStyleBox()
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(0f, 0f, 0f, 0.45f),
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            CornerRadiusBottomLeft = 4,
            CornerRadiusBottomRight = 4,
            ContentMarginLeft = 4,
            ContentMarginRight = 4,
            ContentMarginTop = 1,
            ContentMarginBottom = 1,
        };
    }

    private static string ShortPath(string pathText)
    {
        const int maxLength = 48;
        return pathText.Length <= maxLength ? pathText : $"{pathText[..maxLength]}...";
    }
}
