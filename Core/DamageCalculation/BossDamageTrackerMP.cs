using System;
using System.Collections.Generic;
using System.Linq;
using DPSPanel.Core.Panel;
using Terraria;
using Terraria.ModLoader;

namespace DPSPanel.Core.DamageCalculation
{
    public class BossDamageTrackerMP : ModPlayer
    {
        public class Fight
        {
            public int initialLife;
            public string bossName;
            public int damageTaken;
            public HashSet<CustomPlayer> players = [];
        }

        public class CustomPlayer
        {
            public string playerName;
            public int totalDamage;
        }

        private Fight fight = null;

        #region Hooks
        public override void PreUpdate()
        {
            // Iterate all NPCs every 1 second (idk how computationally heavy this is)
            if (Main.time % 60 == 0)
            {
                // If there's an active fight and the boss is no longer alive or present, stop tracking
                if (fight != null && !Main.npc.Any(npc => npc.active && npc.boss && npc.FullName == fight.bossName))
                {
                    Mod.Logger.Info($"Boss {fight.bossName} despawned!");
                    fight = null; // Stop tracking the fight
                }

                // Start a fight if a boss spawns
                foreach (NPC npc in Main.npc)
                    if (fight == null && npc.active && npc.boss && npc.life > 0)
                        fight = CreateNewBossFight(npc);
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // melee damage
            UpdateFight(item.Name, target, damageDone);
        }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // projectile damage.
            UpdateFight(proj.Name, target, damageDone);
        }
        #endregion

        #region Main Logic
        private void UpdateFight(string weaponName, NPC npc, int damageDone)
        {
            if (!npc.boss) return;

            // create a new fight if there isn't one
            if (fight == null)
                fight = CreateNewBossFight(npc);

            // create a new player if current player hasnt been added to the fight yet
            if (!fight.players.Any(p => p.playerName == Main.LocalPlayer.name))
                fight = AddNewPlayer(Main.LocalPlayer.name);

            // update damage done to the boss
            foreach (var player in fight.players)
            {
                if (player.playerName == Main.LocalPlayer.name)
                {
                    player.totalDamage += damageDone;
                    Mod.Logger.Info("Player: " + player.playerName + " Damage: " + player.totalDamage);
                }
            }
        }

        private Fight CreateNewBossFight(NPC npc)
        {
                fight = new Fight
                {
                    initialLife = npc.lifeMax,
                    bossName = npc.FullName,
                    damageTaken = 0,
                    players = []
                };
            return fight;
        }

        private Fight AddNewPlayer(string playerName)
        {
            fight.players.Add(new CustomPlayer
            {
                playerName = playerName,
                totalDamage = 0
            });
            return fight;
        }
        #endregion

        #region UI
        private void UpdateDamageBarWithFightInfo(NPC npc)
        {
            PanelSystem sys = ModContent.GetInstance<PanelSystem>();
            sys.state.container.panel.ClearPanelAndAllItems();
            sys.state.container.panel.SetBossTitle(fight.bossName, null);
            sys.state.container.bossIcon.UpdateBossIcon(npc);
        }
        #endregion 
    }
}
