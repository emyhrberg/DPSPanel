using System;
using DPSPanel.Common.DamageCalculation.Classes;
using Terraria.GameContent;
using Terraria.UI;

namespace DPSPanel.UI;

public sealed class WeaponBar : DamageBar
{
    private int itemId = -1;
    protected override float RowHeight => DPSPanelLayout.WeaponBarHeight;

    public void SetWeapon(Weapon weapon, long highest, Color color)
    {
        itemId = weapon.weaponItemID;
        SetData(weapon.weaponName, weapon.damage, highest > 0 ? (int)(weapon.damage * 100d / highest) : 0, color);
    }

    protected override void DrawIcon(SpriteBatch sb, CalculatedStyle dims)
    {
        Texture2D texture;
        if (itemId <= 0 || itemId >= TextureAssets.Item.Length)
            texture = TextureAssets.NpcHead[0].Value;
        else
        {
            Main.instance.LoadItem(itemId);
            texture = TextureAssets.Item[itemId].Value;
        }
        Rectangle frame = itemId > 0 && itemId < Main.itemAnimations.Length && Main.itemAnimations[itemId] != null
            ? Main.itemAnimations[itemId].GetFrame(texture) : texture.Bounds;
        float size = Math.Max(1, DPSPanelLayout.WeaponIconSize);
        float scale = Math.Min(1f, size / Math.Max(frame.Width, frame.Height));
        sb.Draw(texture, new Vector2(dims.X + DPSPanelLayout.WeaponIconLeft + size / 2,
            dims.Y + dims.Height / 2 + DPSPanelLayout.WeaponIconOffsetY), frame, LayoutColors.From(DPSPanelLayout.IconColor),
            0f, frame.Size() / 2, scale, SpriteEffects.None, 0f);
    }
}
