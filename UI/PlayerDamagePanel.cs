using System.Collections.Generic;
using System.Linq;
using DPSPanel.Common.DamageCalculation.Classes;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace DPSPanel.UI;

/// <summary>A single popup owned by the root UI, outside the player-row height constraints.</summary>
public sealed class PlayerDamagePanel : UIPanel
{
    private readonly Dictionary<int, WeaponBar> bars = [];
    public bool IsVisible { get; set; }
    public int OwnerId { get; private set; } = -1;
    public int WeaponCount => bars.Count;

    public PlayerDamagePanel()
    {
        SetPadding(PanelLayout.Padding);
        MaxHeight.Set(float.MaxValue, 0);
        MaxWidth.Set(float.MaxValue, 0);
        BackgroundColor = new Color(27, 29, 85);
    }

    public void SetPlayer(PlayerFightData player)
    {
        if (OwnerId != player.PlayerId)
            Reset();
        OwnerId = player.PlayerId;
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
                Append(bar);
            }
            bar.SetWeapon(weapon, highest, ColorHelper.standardColors[i % ColorHelper.standardColors.Length]);
            bar.Top.Set(PanelLayout.RowTop(i), 0);
        }
        ApplyLayout();
    }

    public void ApplyLayout()
    {
        Width.Set(SizeHelper.GetWidthFromConfig(), 0);
        Height.Set(PanelLayout.Height(bars.Count), 0);
        Recalculate();
    }

    public void Reset()
    {
        RemoveAllChildren();
        bars.Clear();
        OwnerId = -1;
        IsVisible = false;
        Height.Set(PanelLayout.Height(0), 0);
    }

    public override bool ContainsPoint(Vector2 point) => IsVisible && base.ContainsPoint(point);
    public override void Draw(SpriteBatch sb)
    {
        if (IsVisible)
            base.Draw(sb);
    }
}
