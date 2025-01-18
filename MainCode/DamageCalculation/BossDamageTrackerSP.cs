using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Linq;

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
            Main.NewText("Hello, " + Main.LocalPlayer.name + "! To use the DPS panel, type /dps show in chat or toggle with K (set the keybind in controls).", Color.Yellow);
        }

        public override void PreUpdate()
        {
            // Iterate all NPCs every 1 second (idk how computationally heavy this is)
            if (Main.time % 60 == 0)
            {
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
            TrackBossDamage("Item", item.type, item.Name, damageDone, target);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TrackBossDamage("Projectile", proj.type, proj.Name, damageDone, target);
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
                PrintBossFight();
                var panelSystem = ModContent.GetInstance<PanelSystem>();
                panelSystem.state.panel.AddBossTitle(npc.FullName);

            }
        }

        private void UpdateWeapon(string _itemType, int itemID, string weaponName, int damageDone)
        {
            if (fight == null || fight.weapons == null)
            {
                Mod.Logger.Warn("UpdateWeapon called with uninitialized fight or weapons list.");
                return; // Exit early
            }

            var panelSystem = ModContent.GetInstance<PanelSystem>();
            if (panelSystem == null || panelSystem.state?.panel == null)
            {
                Mod.Logger.Warn("PanelSystem or panel is not initialized. Skipping slider update.");
                return; // Exit early
            }

            // Check if the weapon already exists
            var weapon = fight.weapons.FirstOrDefault(w => w.weaponName == weaponName);
            if (weapon == null)
            {
                // Add new weapon to the fight
                weapon = new Weapon { weaponName = weaponName, damage = damageDone, itemID = itemID, itemType = _itemType};
                fight.weapons.Add(weapon);
                fight.weapons = fight.weapons.OrderByDescending(w => w.damage).ToList();

                // Find this weapon's index
                int index = fight.weapons.FindIndex(w => w.weaponName == weaponName);

                // Create a new slider for this weapon
                panelSystem.state.panel.CreateSlider(weaponName);
            }
            else
            {
                // Update existing weapon's damage
                weapon.damage += damageDone;
            }

            // Sort weapons by damage and update sliders
            fight.weapons = fight.weapons.OrderByDescending(w => w.damage).ToList();
            panelSystem.state.panel.UpdateSliders(fight.weapons);
        }

        private void TrackBossDamage(string _itemType, int itemID, string weaponName, int damageDone, NPC npc)
        {
            // Check if NPC is a boss
            if (IsValidBoss(npc) && !HandleBossDeath(npc, weaponName))
            {
                // Add weapon to player's weapon list
                UpdateWeapon(_itemType, itemID, weaponName, damageDone);

                // Add damage to existing fight
                fight.damageTaken += damageDone;

                // Send boss fight data to show in UI
                SendBossFightToPanel();
                PrintBossFight();
            }
        }

        // --------------------------------------------------------------------------------
        // Send to panel
        // --------------------------------------------------------------------------------
        private void SendBossFightToPanel()
        {
            if (fight == null || fight.weapons.Count == 0) return;
            fight.weapons = fight.weapons.OrderByDescending(w => w.damage).ToList();
            int highestDamage = fight.weapons.First().damage;
            var panelSystem = ModContent.GetInstance<PanelSystem>();
            panelSystem.state.panel.UpdateSliders(fight.weapons);
        }

        // --------------------------------------------------------------------------------
        // Helper methods
        // --------------------------------------------------------------------------------
        private void PrintBossFight()
        {
            string weaponsDamages = string.Join(", ", fight.weapons.Select(w => w.weaponName + ": " + w.damage));
            Mod.Logger.Info($"Name: {fight.bossName} | ID: {fight.bossId} | WeaponsDamages: {weaponsDamages} | DamageTaken: {fight.damageTaken} | InitialLife: {fight.initialLife}");
        }

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
                        PrintBossFight();
                    }
                }
            }
        }

        private bool IsValidBoss(NPC npc)
        {
            return npc.boss && !npc.friendly;
        }
    }
}
