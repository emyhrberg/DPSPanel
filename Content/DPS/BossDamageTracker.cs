using System.Collections.Generic;
using BetterDPS.UI.DPS;
using Terraria;
using Terraria.ModLoader;

namespace BetterDPS.Content.DPS
{
    public class BossDamageTracker : ModPlayer
    {
        // Keep track of damage done to bosses with a dictionary
        public Dictionary<string, int> bossesAndDamage = new Dictionary<string, int>();

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TrackBossDamage(target, damageDone);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TrackBossDamage(target, damageDone);
        }

        private void TrackBossDamage(NPC target, int damage)
        {
            if (target.boss && !target.friendly && target.life > 0)
            {
                string bossName = target.TypeName;

                // Add the boss to the dictionary if not present
                if (!bossesAndDamage.ContainsKey(bossName))
                {
                    bossesAndDamage[bossName] = 0;
                }

                // Increment the damage for the boss
                bossesAndDamage[bossName] += damage;

                // Update the DPS panel
                var panelState = ModContent.GetInstance<DPSPanelSystem>().state;
                panelState.UpdateDPSPanel(bossesAndDamage);
            }
        }
    }
}
