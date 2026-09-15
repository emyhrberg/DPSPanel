using Terraria.DataStructures;

namespace DPSPanel.Common.DamageCalculation.Classes;

public sealed class GlobalProj : GlobalProjectile
{
    public int WeaponId { get; private set; } = -1;
    public string WeaponName { get; private set; } = "Unknown";
    public override bool InstancePerEntity => true;

    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        WeaponId = -1;
        WeaponName = "Unknown";
        if (source is IEntitySource_WithStatsFromItem withItem)
        {
            WeaponId = withItem.Item.type;
            WeaponName = withItem.Item.Name;
        }
        else if (source is EntitySource_Parent { Entity: Projectile parent })
        {
            var original = parent.GetGlobalProjectile<GlobalProj>();
            WeaponId = original.WeaponId;
            WeaponName = original.WeaponName;
        }
    }
}
