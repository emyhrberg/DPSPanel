using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace DPSPanel.DamageCalculation.Classes
{
    public class GlobalProj : GlobalProjectile
    {
        // We store the weapon that fired this projectile
        public Item sourceWeapon;

        public override bool InstancePerEntity => true;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (source is IEntitySource_WithStatsFromItem withStats)
            {
                sourceWeapon = withStats.Item; // sourceWeapon is of type: Item
            }
        }
    }
}
