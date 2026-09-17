using System;
using System.Collections.Generic;
using System.Linq;
using DPSPanel.Common.Configs;
using DPSPanel.Common.DamageCalculation.Classes;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.UI.Chat;

namespace DPSPanel.UI;

public sealed class MainPanel : UIPanel
{
    private readonly Dictionary<int, PlayerBar> playerBars = [];
    private readonly Dictionary<int, WeaponBar> weaponBars = [];
    private readonly ScrollableRows rows = new();
    private IReadOnlyList<PlayerFightData> displayedPlayers = Array.Empty<PlayerFightData>();
    private IReadOnlyDictionary<int, Player> portraits;
    private List<int> playerOrder = [];
    private int? hoveredPlayer;
    private bool singleplayer;
    private FightContext fight;
    public PlayerDamagePanel Popup { get; set; }
    public bool HideWhenInventoryOpen = true;
    public bool IsDisplayed => Parent is MainContainer { panelVisible: true } &&
        (HideWhenInventoryOpen || Main.playerInventory);

    public MainPanel()
    {
        MaxHeight.Set(float.MaxValue, 0);
        MaxWidth.Set(float.MaxValue, 0);
        Append(rows);
        ApplyLayout();
    }

    public void SetFight(FightContext context) => fight = context;

    private void ClearRows()
    {
        rows.Content.RemoveAllChildren();
        playerBars.Clear();
        weaponBars.Clear();
    }

    public void Reset()
    {
        ClearRows();
        displayedPlayers = Array.Empty<PlayerFightData>();
        portraits = null;
        playerOrder.Clear();
        rows.SetScroll(0);
        hoveredPlayer = null;
        fight = null;
        Popup?.Reset();
        ApplyLayout();
    }

    public void RebuildAppearance()
    {
        // Keep snapshots, identity, ranking, hover and scroll state; recreate the rendered rows.
        float offset = rows.ScrollOffset;
        ClearRows();
        SetPlayers(displayedPlayers, singleplayer, portraits);
        rows.SetScroll(offset);
        Popup?.RebuildAppearance();
    }

    public void SetPlayers(IReadOnlyList<PlayerFightData> players, bool isSingleplayer,
        IReadOnlyDictionary<int, Player> previewPortraits = null)
    {
        if (singleplayer != isSingleplayer)
        {
            ClearRows();
            playerOrder.Clear();
            rows.SetScroll(0);
            HidePopup();
            Popup?.Reset();
        }
        singleplayer = isSingleplayer;
        displayedPlayers = players.ToArray();
        portraits = previewPortraits;

        if (singleplayer)
            SetWeapons(players.FirstOrDefault()?.Weapons ?? Array.Empty<Weapon>());
        else
        {
            var ids = players.Select(p => p.PlayerId).ToHashSet();
            foreach (int id in playerBars.Keys.Where(id => !ids.Contains(id)).ToArray())
            {
                playerBars[id].Remove();
                playerBars.Remove(id);
                if (hoveredPlayer == id)
                    HidePopup();
            }
            long highest = players.Count > 0 ? players.Max(p => p.Damage) : 0;
            foreach (var player in players)
            {
                if (!playerBars.TryGetValue(player.PlayerId, out var bar))
                {
                    bar = new PlayerBar(player.PlayerId);
                    playerBars.Add(player.PlayerId, bar);
                    rows.Content.Append(bar);
                }
                Player portrait = null;
                portraits?.TryGetValue(player.PlayerId, out portrait);
                bar.SetPlayer(player, highest, portrait);
            }
            ArrangePlayers(hoveredPlayer.HasValue || PointerOverRow());
            if (hoveredPlayer is int owner && playerBars.TryGetValue(owner, out var selected))
                Popup?.SetPlayer(selected.Data);
        }
        ApplyLayout();
    }

    private void SetWeapons(IReadOnlyList<Weapon> weapons)
    {
        var sorted = weapons.OrderByDescending(w => w.damage).ThenBy(w => w.weaponItemID).ToArray();
        var ids = sorted.Select(w => w.weaponItemID).ToHashSet();
        foreach (int id in weaponBars.Keys.Where(id => !ids.Contains(id)).ToArray())
        {
            weaponBars[id].Remove();
            weaponBars.Remove(id);
        }
        long highest = sorted.FirstOrDefault()?.damage ?? 0;
        for (int i = 0; i < sorted.Length; i++)
        {
            var weapon = sorted[i];
            if (!weaponBars.TryGetValue(weapon.weaponItemID, out var bar))
            {
                bar = new WeaponBar();
                weaponBars.Add(weapon.weaponItemID, bar);
                rows.Content.Append(bar);
            }
            bar.SetWeapon(weapon, highest, ColorHelper.standardColors[i % ColorHelper.standardColors.Length]);
            bar.Top.Set(PanelLayout.RowTop(i, DPSPanelLayout.WeaponBarHeight, DPSPanelLayout.WeaponRowGap), 0);
        }
    }

    private bool PointerOverRow()
    {
        Vector2 mouse = MainSystem.PointerPosition;
        return IsDisplayed && rows.ContainsPoint(mouse) && playerBars.Values.Any(bar => bar.ContainsPoint(mouse));
    }

    private void ArrangePlayers(bool freeze)
    {
        playerOrder = PanelLayout.OrderPlayers(playerOrder,
            playerBars.Values.Select(p => (p.PlayerId, p.Data.Damage)), freeze);
        for (int i = 0; i < playerOrder.Count; i++)
            playerBars[playerOrder[i]].Top.Set(PanelLayout.RowTop(i, DPSPanelLayout.PlayerBarHeight, DPSPanelLayout.PlayerRowGap), 0);
    }

    public void ApplyLayout()
    {
        PaddingLeft = Math.Max(0, DPSPanelLayout.PanelPaddingLeft);
        PaddingRight = Math.Max(0, DPSPanelLayout.PanelPaddingRight);
        PaddingTop = Math.Max(0, DPSPanelLayout.PanelPaddingTop);
        PaddingBottom = Math.Max(0, DPSPanelLayout.PanelPaddingBottom);
        BackgroundColor = LayoutColors.From(DPSPanelLayout.PanelBackground);
        BorderColor = LayoutColors.From(DPSPanelLayout.PanelBorder);
        float header = Math.Max(0, DPSPanelLayout.HeaderHeight) + Math.Max(0, DPSPanelLayout.HeaderGap);
        float contentHeight = singleplayer
            ? PanelLayout.RowsHeight(weaponBars.Count, DPSPanelLayout.WeaponBarHeight, DPSPanelLayout.WeaponRowGap)
            : PanelLayout.RowsHeight(playerBars.Count, DPSPanelLayout.PlayerBarHeight, DPSPanelLayout.PlayerRowGap);
        float screenHeight = Parent?.Parent?.GetInnerDimensions().Height ?? MainSystem.ViewportSize.Y;
        float screenWidth = Parent?.Parent?.GetInnerDimensions().Width ?? MainSystem.ViewportSize.X;
        float top = Math.Max(DPSPanelLayout.ScreenMargin, Parent?.GetDimensions().Y ?? 0);
        var metrics = PanelLayout.Measure(contentHeight, header + PaddingTop + PaddingBottom,
            DPSPanelLayout.MainMinHeight, DPSPanelLayout.MainMaxHeight, screenHeight - top - DPSPanelLayout.ScreenMargin);
        float width = Math.Min(SizeHelper.GetWidthFromConfig(), Math.Max(50, screenWidth - DPSPanelLayout.ScreenMargin * 2));
        Width.Set(width, 0);
        Height.Set(metrics.Height, 0);
        rows.Top.Set(header, 0);
        rows.SetExtent(metrics.ContentHeight, metrics.ViewportHeight);
        if (Parent is MainContainer container)
        {
            container.Width.Set(width, 0);
            container.Height.Set(metrics.Height, 0);
            container.Recalculate();
        }
        else
            Recalculate();
        Popup?.ApplyLayout();
    }

    public void UpdateHover(Vector2 mouse)
    {
        if (Popup == null)
            return;
        if (!IsDisplayed || singleplayer || Parent is MainContainer { dragging: true })
        {
            HidePopup();
            return;
        }

        int? selected = null;
        if (Popup.IsVisible && hoveredPlayer.HasValue && (Popup.ContainsPoint(mouse) || InPopupBridge(mouse)))
            selected = hoveredPlayer;
        else if (rows.ContainsPoint(mouse))
            foreach (int id in playerOrder)
                if (playerBars[id].ContainsPoint(mouse))
                {
                    selected = id;
                    break;
                }

        if (selected is not int owner || !playerBars.TryGetValue(owner, out var bar))
        {
            bool wasHovered = hoveredPlayer.HasValue;
            HidePopup();
            if (wasHovered)
            {
                ArrangePlayers(false);
                Recalculate();
            }
            return;
        }

        if (hoveredPlayer != owner)
            Popup.SetPlayer(bar.Data);
        hoveredPlayer = owner;
        Popup.IsVisible = Popup.WeaponCount > 0;
        Main.LocalPlayer.mouseInterface = true;

        var row = bar.GetDimensions();
        var panel = GetDimensions();
        var screen = Parent.Parent.GetInnerDimensions();
        float margin = Math.Max(0, DPSPanelLayout.ScreenMargin);
        float gap = Math.Max(0, DPSPanelLayout.PopupGap);
        float x = panel.X + panel.Width + gap;
        if (x + Popup.Width.Pixels > screen.X + screen.Width - margin)
            x = panel.X - Popup.Width.Pixels - gap;
        x = Math.Clamp(x, screen.X + margin, Math.Max(screen.X + margin, screen.X + screen.Width - Popup.Width.Pixels - margin));
        float y = Math.Clamp(row.Y + DPSPanelLayout.PopupOffsetY, screen.Y + margin,
            Math.Max(screen.Y + margin, screen.Y + screen.Height - Popup.Height.Pixels - margin));
        Popup.Left.Set(x - screen.X, 0);
        Popup.Top.Set(y - screen.Y, 0);
        Popup.Recalculate();
    }

    private bool InPopupBridge(Vector2 mouse)
    {
        if (hoveredPlayer is not int owner || !playerBars.TryGetValue(owner, out var bar))
            return false;
        var row = bar.GetDimensions();
        var panel = GetDimensions();
        var popup = Popup.GetDimensions();
        var viewport = rows.GetDimensions();
        // Include the panel's padding and scrollbar gutter, so crossing them does not close the popup.
        float left = popup.X >= panel.X + panel.Width ? row.X + row.Width : popup.X + popup.Width;
        float right = popup.X >= panel.X + panel.Width ? popup.X : row.X;
        return mouse.X >= left && mouse.X <= right &&
            mouse.Y >= Math.Max(row.Y, viewport.Y) && mouse.Y <= Math.Min(row.Y + row.Height, viewport.Y + viewport.Height);
    }

    public void HidePopup()
    {
        hoveredPlayer = null;
        if (Popup != null)
            Popup.IsVisible = false;
    }

    public override bool ContainsPoint(Vector2 point) => IsDisplayed && base.ContainsPoint(point);

    public override void Draw(SpriteBatch sb)
    {
        if (IsDisplayed)
            base.Draw(sb);
    }

    protected override void DrawSelf(SpriteBatch sb)
    {
        base.DrawSelf(sb);
        var dims = GetInnerDimensions();
        bool showIcon = Config.Conf.C.ShowBossIcon && fight != null &&
            fight.HeadIcon >= 0 && fight.HeadIcon < TextureAssets.NpcHeadBoss.Length;
        float left = Math.Max(0, DPSPanelLayout.ToggleLeft + DPSPanelLayout.ToggleWidth - PaddingLeft + DPSPanelLayout.HeaderToggleGap);
        float iconSize = Math.Max(1, DPSPanelLayout.BossIconSize);
        float available = Math.Max(1f, dims.Width - left - DPSPanelLayout.HeaderTextPaddingRight -
            (showIcon ? iconSize + DPSPanelLayout.BossIconTextGap : 0));
        string name = fight?.Name ?? "DPSPanel";
        Vector2 size = FontAssets.MouseText.Value.MeasureString(name);
        float scale = Math.Min(Math.Max(0.01f, DPSPanelLayout.HeaderTextScale), available / Math.Max(1f, size.X));
        ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, name,
            new Vector2(dims.X + left + (available - size.X * scale) / 2 + DPSPanelLayout.HeaderTextOffsetX,
                dims.Y + (DPSPanelLayout.HeaderContentHeight - size.Y * scale) / 2 + DPSPanelLayout.HeaderTextOffsetY),
            LayoutColors.From(DPSPanelLayout.TextColor), 0f, Vector2.Zero, new Vector2(scale));
        if (showIcon)
            sb.Draw(TextureAssets.NpcHeadBoss[fight.HeadIcon].Value,
                new Rectangle((int)(dims.X + dims.Width - iconSize + DPSPanelLayout.BossIconOffsetX),
                    (int)(dims.Y + DPSPanelLayout.BossIconOffsetY), (int)iconSize, (int)iconSize),
                LayoutColors.From(DPSPanelLayout.IconColor));
    }
}
