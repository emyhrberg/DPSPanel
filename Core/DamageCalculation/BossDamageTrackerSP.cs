using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Linq;
using DPSPanel.Core.Configs;
using DPSPanel.Core.Helpers;

namespace DPSPanel.Core.Panel
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
            public List<Weapon> weapons = [];
            public bool isAlive = false;
        }

        public class Weapon
        {
            public string weaponName;
            public int damage;
            public int itemID;
            public string itemType;
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
            Main.NewText("Hello, " + Main.LocalPlayer.name + "! To use the DPS panel, type /dps toggle in chat or toggle with K (set the keybind in controls).", Color.Yellow);
        }

        public override void PreUpdate()
        {
            // Iterate all NPCs every 1 second (idk how computationally heavy this is)
            if (Main.time % 60 == 0)
            {
                // If there's an active fight and the boss is no longer alive or present, stop tracking
                if (fight != null && !Main.npc.Any(npc => npc.active && npc.boss && npc.FullName == fight.bossName))
                {
                    Mod.Logger.Info($"Boss {fight.bossName} despawned!");
                    fight.isAlive = false;
                    fight = null; // Stop tracking the fight
                }

                for (int i = 0; i < Main.npc.Length; i++)
                {
                    NPC npc = Main.npc[i];
                    if (IsValidBoss(npc) && (fight == null || !fight.isAlive) && npc.life > 0)
                    {
                        CreateNewBossFight(npc);
                    }
                }
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // get source weapon
            string sourceWeapon = GlobalProj.sourceWeapon?.Name;
            if (string.IsNullOrEmpty(sourceWeapon))
            {
                sourceWeapon = "unknown";
            }

            Mod.Logger.Info("Item: " + item.Name + " SourceWeapon: " + sourceWeapon);

            TrackBossDamage(sourceWeapon, item.type, item.Name, damageDone, target);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            string sourceWeapon = GlobalProj.sourceWeapon?.Name;
            if (string.IsNullOrEmpty(sourceWeapon))
            {
                sourceWeapon = "unknown";
            }
            Mod.Logger.Info("Proj: " + proj.Name + " SourceWeapon: " + sourceWeapon);

            TrackBossDamage(sourceWeapon, proj.type, proj.Name, damageDone, target);
        }

        // --------------------------------------------------------------------------------
        // Main logic
        // --------------------------------------------------------------------------------

        private void CreateNewBossFight(NPC npc)
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
                    weapons = [],
                    isAlive = true
                };
                Mod.Logger.Info("New boss fight created: " + fight.bossName);
                var sys = ModContent.GetInstance<PanelSystem>();
                sys.state.container.panel.ClearPanelAndAllItems();
                sys.state.container.panel.SetBossTitle(npc.FullName, npc);
                sys.state.container.bossIcon.UpdateBossIcon(npc);
            }
        }

        private void UpdateWeapon(string _itemType, int itemID, string weaponName, int damageDone)
        {
            // Check if the weapon already exists
            var weapon = fight.weapons.FirstOrDefault(w => w.weaponName == weaponName);
            if (weapon == null)
            {
                // Add new weapon to the fight
                weapon = new Weapon { weaponName = weaponName, damage = damageDone, itemID = itemID, itemType = _itemType};
                fight.weapons.Add(weapon);
                fight.weapons = fight.weapons.OrderByDescending(w => w.damage).ToList();

                // Create a new bar for this weapon
                PanelSystem sys = ModContent.GetInstance<PanelSystem>();
                sys.state.container.panel.CreateDamageBar(weaponName);
            }
            else
            {
                // Update existing weapon's damage
                weapon.damage += damageDone;
            }
        }

        private void TrackBossDamage(string _itemType, int itemID, string weaponName, int damageDone, NPC npc)
        {
            // Check if NPC is a boss
            if (IsValidBoss(npc) && !HandleBossDeath(npc, weaponName))
            {
                UpdateWeapon(_itemType, itemID, weaponName, damageDone); // Add weapon to player's weapon list
                fight.damageTaken += damageDone; // Add damage to existing fight
                SendBossFightToPanel(); // Send boss fight data to show in UI
                PrintBossFight();
            }
        }

        private void SendBossFightToPanel()
        {
            // Sort weapons by descending damage
            fight.weapons = fight.weapons.OrderByDescending(w => w.damage).ToList();

            // Update the bars
            var panelSystem = ModContent.GetInstance<PanelSystem>();
            panelSystem.state.container.panel.UpdateDamageBars(fight.weapons);
        }

        // --------------------------------------------------------------------------------
        // Helper methods
        // --------------------------------------------------------------------------------
        private bool HandleBossDeath(NPC npc, string weaponName)
        {
            if (npc.life <= 0)
            {
                fight.isAlive = false;
                FixFinalBlowDiscrepancy(weaponName);
                SendBossFightToPanel();
                fightId++;
                fight = null;
                return true;
            }
            return false;
        }

        private void FixFinalBlowDiscrepancy(string weaponName)
        {
            // Check if the final blow was not accounted for
            if (fight.damageTaken < fight.initialLife)
            {
                foreach (var weapon in fight.weapons)
                {
                    if (weapon.weaponName == weaponName)
                    {
                        int discrepancy = fight.initialLife - fight.damageTaken;
                        weapon.damage += discrepancy;
                        fight.damageTaken = fight.initialLife;
                    }
                }
            }
        }

        private bool IsValidBoss(NPC npc)
        {
            return npc.boss && !npc.friendly;
        }

        private void PrintBossFight()
        {
            string weaponsDamages = string.Join(", ", fight.weapons.Select(w => w.weaponName + ": " + w.damage));
            Mod.Logger.Info($"Name: {fight.bossName} | ID: {fight.bossId} | WeaponsDamages: {weaponsDamages} | DamageTaken: {fight.damageTaken} | InitialLife: {fight.initialLife}");
        }
    }
}
