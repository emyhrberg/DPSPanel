using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

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
        public override void OnEnterWorld()
        {
            Main.NewText("Hello, " + Main.LocalPlayer.name + "! To use the DPS panel, type /dps show in chat or toggle with K (set the keybind in controls).", Color.Yellow);
        }

        public override void OnHitAnything(float x, float y, Entity victim)
        {
            // check if golem was hit
            if (victim is NPC npc && IsValidBoss(npc))
            {
                Mod.Logger.Info("Unknown NPC. Hopefully Golem? was hit!");
            }
        }

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
                    var panelSystem = ModContent.GetInstance<PanelSystem>();
                    panelSystem.state.panel.AddBossTitle(npc.FullName);
                    panelSystem.state.panel.CreateSlider();
                }
                else
                {
                    // Add to existing fight
                    fight.damageTaken += damageDone;
                    fight.player.totalDamage += damageDone;
                }
                PrintBossFight();
                SendBossFightToPanel();
            }
        }

        // --------------------------------------------------------------------------------
        // Send to panel
        // --------------------------------------------------------------------------------
        private void SendBossFightToPanel()
        {
            var panelSystem = ModContent.GetInstance<PanelSystem>();

            // Use float to do division with percentages
            int damageProgress = (int)(((float)fight.damageTaken / (float)fight.initialLife) * 100f);
            int damageDone = fight.player.totalDamage;

            // Update slider with the new progress value.
            panelSystem.state.panel.UpdateSlider(damageDone, damageProgress);
        }

        // --------------------------------------------------------------------------------
        // Helper methods
        // --------------------------------------------------------------------------------
        private void PrintBossFight()
        {
            Mod.Logger.Info($"Name: {fight.bossName} | ID: {fight.bossId} | Player: {fight.player.playerName} | Damage: {fight.player.totalDamage}");
        }

        private bool IsNPCDead(NPC npc)
        {
            if (npc.life <= 0)
            {
                FixFinalBlowDiscrepancy();
                SendBossFightToPanel();
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
