using System;
using System.Collections.Generic;
using System.Linq;

namespace DPSPanel.Core.Utilities;

/// <summary>Shared dimensions for panels and the rows they contain, in UI pixels.</summary>
public static class PanelLayout
{
    public const float BarHeight = 40f;
    public const float Spacing = 10f;
    public const float Padding = 5f;
    public const float HeaderHeight = 40f;

    public static float Height(int rows, float header = 0f) =>
        Padding * 2 + header + Math.Max(0, rows) * BarHeight + Math.Max(0, rows - 1) * Spacing;

    public static float RowTop(int row, float header = 0f) => header + row * (BarHeight + Spacing);

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
