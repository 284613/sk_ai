using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;

namespace SkAiRouteAdvisor.DecisionLogging;

internal static class DecisionLogValueFormatter
{
    public static string FormatModelId(ModelId? modelId)
    {
        return modelId == null
            ? string.Empty
            : $"{modelId.Category.ToLowerInvariant()}:{modelId.Entry.ToLowerInvariant()}";
    }

    public static string FormatLocString(LocString? locString)
    {
        if (locString == null || locString.IsEmpty)
        {
            return string.Empty;
        }

        var formatted = locString.GetFormattedText();
        if (!string.IsNullOrWhiteSpace(formatted))
        {
            return formatted.Trim();
        }

        var raw = locString.GetRawText();
        if (!string.IsNullOrWhiteSpace(raw))
        {
            return raw.Trim();
        }

        return string.Empty;
    }

    public static string FormatMapPointId(MapPoint? mapPoint)
    {
        if (mapPoint == null)
        {
            return string.Empty;
        }

        return $"{mapPoint.PointType.ToString().ToLowerInvariant()}_{mapPoint.coord.row}_{mapPoint.coord.col}";
    }

    public static DecisionMapPositionSnapshot? FormatMapCoord(MapCoord? coord)
    {
        if (coord == null)
        {
            return null;
        }

        return new DecisionMapPositionSnapshot
        {
            Row = coord.Value.row,
            Col = coord.Value.col,
        };
    }

    public static string ToModeTag(string mode)
    {
        return string.IsNullOrWhiteSpace(mode) ? "default" : mode.Trim().ToLowerInvariant();
    }
}
