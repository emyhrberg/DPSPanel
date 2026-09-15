using System;
using System.Collections.Generic;
using System.Linq;
using DPSPanel.Common.DamageCalculation.Classes;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace DPSPanel.UI;

public sealed class PlayerDamagePanel : UIPanel
{
    private readonly Dictionary<int, WeaponBar> bars = [];
    private readonly ScrollableRows rows = new();
    private PlayerFightData data;
    public bool IsVisible { get; set; }
    public int OwnerId => data?.PlayerId ?? -1;
    public int WeaponCount => bars.Count;

    public PlayerDamagePanel()
    {
        MaxHeight.Set(float.MaxValue, 0);
        MaxWidth.Set(float.MaxValue, 0);
        Append(rows);
        ApplyLayout();
    }

    public void SetPlayer(PlayerFightData player)
    {
        if (OwnerId != player.PlayerId)
            Reset();
        data = player;
        var weapons = player.Weapons.OrderByDescending(w => w.damage).ThenBy(w => w.weaponItemID).ToArray();
        var ids = weapons.Select(w => w.weaponItemID).ToHashSet();
        foreach (int id in bars.Keys.Where(id => !ids.Contains(id)).ToArray())
        {
            bars[id].Remove();
            bars.Remove(id);
        }
        long highest = weapons.FirstOrDefault()?.damage ?? 0;
        for (int i = 0; i < weapons.Length; i++)
        {
            var weapon = weapons[i];
            if (!bars.TryGetValue(weapon.weaponItemID, out var bar))
            {
                bar = new WeaponBar();
                bars.Add(weapon.weaponItemID, bar);
                rows.Content.Append(bar);
            }
            bar.SetWeapon(weapon, highest, ColorHelper.standardColors[i % ColorHelper.standardColors.Length]);
            bar.Top.Set(PanelLayout.RowTop(i, DPSPanelLayout.WeaponBarHeight, DPSPanelLayout.WeaponRowGap), 0);
        }
        ApplyLayout();
    }

    public void RebuildAppearance()
    {
        var player = data;
        bool visible = IsVisible;
        float offset = rows.ScrollOffset;
        rows.Content.RemoveAllChildren();
        bars.Clear();
        if (player != null)
            SetPlayer(player);
        else
            ApplyLayout();
        rows.SetScroll(offset);
        IsVisible = visible && bars.Count > 0;
    }

    public void ApplyLayout()
    {
        PaddingLeft = Math.Max(0, DPSPanelLayout.PopupPaddingLeft);
        PaddingRight = Math.Max(0, DPSPanelLayout.PopupPaddingRight);
        PaddingTop = Math.Max(0, DPSPanelLayout.PopupPaddingTop);
        PaddingBottom = Math.Max(0, DPSPanelLayout.PopupPaddingBottom);
        BackgroundColor = LayoutColors.From(DPSPanelLayout.PopupBackground);
        BorderColor = LayoutColors.From(DPSPanelLayout.PanelBorder);
        float screenWidth = Parent?.GetInnerDimensions().Width ?? Main.screenWidth / Math.Max(0.1f, Main.UIScale);
        float screenHeight = Parent?.GetInnerDimensions().Height ?? Main.screenHeight / Math.Max(0.1f, Main.UIScale);
        Width.Set(Math.Min(Math.Max(50, SizeHelper.GetWidthFromConfig() * DPSPanelLayout.PopupWidthMultiplier),
            Math.Max(50, screenWidth - DPSPanelLayout.ScreenMargin * 2)), 0);
        float contentHeight = PanelLayout.RowsHeight(bars.Count, DPSPanelLayout.WeaponBarHeight, DPSPanelLayout.WeaponRowGap);
        var metrics = PanelLayout.Measure(contentHeight, PaddingTop + PaddingBottom,
            DPSPanelLayout.PopupMinHeight, DPSPanelLayout.PopupMaxHeight, screenHeight - DPSPanelLayout.ScreenMargin * 2);
        Height.Set(metrics.Height, 0);
        rows.SetExtent(metrics.ContentHeight, metrics.ViewportHeight);
        Recalculate();
    }

    public void Reset()
    {
        rows.Content.RemoveAllChildren();
        rows.SetScroll(0);
        bars.Clear();
        data = null;
        IsVisible = false;
        ApplyLayout();
    }

    public override bool ContainsPoint(Vector2 point) => IsVisible && base.ContainsPoint(point);
    public override void Draw(SpriteBatch sb)
    {
        if (IsVisible)
            base.Draw(sb);
    }
}
