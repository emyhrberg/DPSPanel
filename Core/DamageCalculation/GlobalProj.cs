using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace DPSPanel.Core.DamageCalculation
{
    public class GlobalProj : GlobalProjectile
    {
        // We store the weapon that fired this projectile
        public static Item sourceWeapon;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (source is IEntitySource_WithStatsFromItem withStats)
                sourceWeapon = withStats.Item; // sourceWeapon is of type: Item
        }
    }
}
