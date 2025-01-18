using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using System.Linq;
using System;
using log4net.Core;
using log4net;

namespace DPSPanel.MainCode.Panel
{
    public class BossDamageTrackerSP : ModPlayer
    {
        // --------------------------------------------------------------------------------
        // Classes
        // --------------------------------------------------------------------------------
        public class BossFight
        {
            public int bossId;            // Incremental ID (0, 1, 2, ...)
            public int initialLife;
            public string bossName;
            public int damageTaken;
            public CustomPlayer player;
        }

        public class CustomPlayer
        {
            public string playerName;
            public int totalDamage;
            public HashSet<Weapon> weapons = [];
        }

        public class Weapon
        {
            public string weaponName;
            public int damage;
        }

        // --------------------------------------------------------------------------------
        // Fields
        // --------------------------------------------------------------------------------
        private BossFight fight;
        private int fightId = 0;

        // --------------------------------------------------------------------------------
        // Hooks
        // --------------------------------------------------------------------------------
        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TrackBossDamage(item.Name, damageDone, target);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TrackBossDamage(proj.Name, damageDone, target);
        }

        // --------------------------------------------------------------------------------
        // Main logic
        // --------------------------------------------------------------------------------

        private void TrackBossDamage(string weaponName, int damageDone, NPC npc)
        {
            // Check if NPC is a boss
            if (IsValidBoss(npc) && !IsNPCDead(npc))
            {
                if (fight == null)
                {
                    // Create a new fight
                    fight = new BossFight
                    {
                        bossId = fightId,
                        initialLife = npc.lifeMax,
                        bossName = npc.FullName,
                        damageTaken = 0,
                        player = new CustomPlayer
                        {
                            playerName = Main.LocalPlayer.name,
                            totalDamage = damageDone
                        }
                    };
                }
                else
                {
                    // Add to existing fight
                    fight.damageTaken += damageDone;
                    fight.player.totalDamage += damageDone;
                }
                PrintBossFight();
            }
        }

        // --------------------------------------------------------------------------------
        // Helper methods
        // --------------------------------------------------------------------------------
        private void PrintBossFight()
        {
            ILog log = ModContent.GetInstance<DPSPanel>().Logger;
            log.Info($"Name: {fight.bossName} | ID: {fight.bossId} | Player: {fight.player.playerName} | Damage: {fight.player.totalDamage}");
        }

        private bool IsNPCDead(NPC npc)
        {
            if (npc.life <= 0)
            {
                FixFinalBlowDiscrepancy();
                fightId++;
                fight = null;
                return true;
            }
            return false;
        }

        private void FixFinalBlowDiscrepancy()
        {
            // Check if the final blow was not accounted for
            if (fight.damageTaken < fight.initialLife)
            {
                fight.damageTaken = fight.initialLife;
                fight.player.totalDamage = fight.initialLife;
                PrintBossFight();
            }
        }

        private bool IsValidBoss(NPC npc)
        {
            return npc.boss && !npc.friendly;
        }
    }
}
