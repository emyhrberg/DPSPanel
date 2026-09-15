using System;
using System.Collections.Generic;
using System.Linq;
using DPSPanel.UI;

namespace DPSPanel.Core.Utilities;

public static class PanelLayout
{
    // Compatibility helpers also used by the regression harness.
    public static float BarHeight => DPSPanelLayout.WeaponBarHeight;
    public static float Spacing => DPSPanelLayout.WeaponRowGap;
    public static float Padding => DPSPanelLayout.PanelPaddingTop;
    public static float HeaderHeight => DPSPanelLayout.HeaderHeight;

    public static float RowsHeight(int count, float rowHeight, float gap) =>
        Math.Max(0, count) * Math.Max(1, rowHeight) + Math.Max(0, count - 1) * Math.Max(0, gap);

    public static float Height(int rows, float header = 0f) =>
        Padding * 2 + header + RowsHeight(rows, BarHeight, Spacing);

    public static float RowTop(int row, float header = 0f) => header + RowTop(row, BarHeight, Spacing);

    public static float RowTop(int row, float rowHeight, float gap) =>
        Math.Max(0, row) * (Math.Max(1, rowHeight) + Math.Max(0, gap));

    public static float ClampScroll(float offset, float content, float viewport) =>
        Math.Clamp(offset, 0, Math.Max(0, content - viewport));

    public static PanelMetrics Measure(float contentHeight, float chromeHeight, float minimum, float maximum, float available)
    {
        contentHeight = Math.Max(0, contentHeight);
        chromeHeight = Math.Max(0, chromeHeight);
        float limit = Math.Max(chromeHeight + 1, available);
        if (maximum > 0)
            limit = Math.Min(limit, Math.Max(chromeHeight + 1, maximum));
        float total = Math.Clamp(Math.Max(chromeHeight + contentHeight, minimum), chromeHeight, limit);
        return new PanelMetrics(total, Math.Max(0, total - chromeHeight), contentHeight);
    }

    public static List<int> OrderPlayers(IEnumerable<int> previous, IEnumerable<(int Id, long Damage)> players, bool freeze)
    {
        var ranked = players.OrderByDescending(p => p.Damage).ThenBy(p => p.Id).Select(p => p.Id).ToList();
        if (!freeze)
            return ranked;
        var present = ranked.ToHashSet();
        var ordered = previous.Where(present.Contains).ToList();
        ordered.AddRange(ranked.Where(id => !ordered.Contains(id)));
        return ordered;
    }
}

public readonly record struct PanelMetrics(float Height, float ViewportHeight, float ContentHeight);
