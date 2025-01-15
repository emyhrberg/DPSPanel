using System.Collections.Generic;
using BetterDPS.UI.DPS;
using Terraria;
using Terraria.ModLoader;

namespace BetterDPS.Content.DPS
{
    public class BossDamageTracker : ModPlayer
    {
        // Dictionary for active boss damage tracking
        private Dictionary<int, int> activeBossDamage = new();
        // Dictionary for finished boss damage tracking
        private Dictionary<string, int> deadBossDamage = new();

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
            if (!target.boss || target.friendly)
                return;

            // Identify boss by its unique whoAmI during the fight
            int bossID = target.whoAmI;

            if (target.life > 0)
            {
                // Update active boss damage
                if (!activeBossDamage.ContainsKey(bossID))
                    activeBossDamage[bossID] = 0;

                activeBossDamage[bossID] += damage;

                // Update the panel for active bosses
                UpdatePanel();
            }
            else
            {
                // Boss has died, transfer to deadBossDamage
                string bossKey = $"{target.TypeName} (#{bossID})";
                if (!deadBossDamage.ContainsKey(bossKey))
                {
                    deadBossDamage[bossKey] = activeBossDamage.ContainsKey(bossID) ? activeBossDamage[bossID] : 0;
                    activeBossDamage.Remove(bossID);

                    // Update the panel for dead bosses
                    UpdatePanel();
                }
            }
        }

        private void UpdatePanel()
        {
            var panelState = ModContent.GetInstance<DPSPanelSystem>().state;

            // Merge active and dead boss damages for display
            var allDamage = new Dictionary<string, int>();
            foreach (var kvp in activeBossDamage)
            {
                string name = $"{Main.npc[kvp.Key].TypeName} (#{kvp.Key})";
                allDamage[name] = kvp.Value;
            }
            foreach (var kvp in deadBossDamage)
            {
                if (!allDamage.ContainsKey(kvp.Key))
                    allDamage[kvp.Key] = kvp.Value;
            }

            // Update the panel
            panelState.UpdateDPSPanel(allDamage);
        }

        public override void OnEnterWorld()
        {
            activeBossDamage.Clear();
            deadBossDamage.Clear();
        }
    }
}
