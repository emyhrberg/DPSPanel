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
        #region Classes
        public class BossFight
        {
            public int bossId;            
            public int initialLife;
            public string bossName;
            public int damageTaken;
            public bool isAlive = false;
            public List<Weapon> weapons = [];

            public void UpdateWeapon(int weaponID, string weaponName, int damageDone)
            {
                // Check if the weapon already exists
                var weapon = weapons.FirstOrDefault(w => w.weaponName == weaponName);
                if (weapon == null)
                {
                    // Add a new weapon to the fight
                    weapon = new Weapon 
                    {
                        weaponItemID = weaponID,
                        weaponName = weaponName,
                        damage = damageDone,
                    };
                    weapons.Add(weapon);
                    weapons = weapons.OrderByDescending(w => w.damage).ToList();
                    PanelSystem sys = ModContent.GetInstance<PanelSystem>();
                    sys.state.container.panel.CreateDamageBar(weaponName);
                }
                else
                {
                    // Update existing weapon's damage
                    weapon.damage += damageDone;
                }
            }

            public void FixFinalBlowDiscrepancy(string weaponName)
            {
                if (damageTaken < initialLife)
                {
                    var weapon = weapons.FirstOrDefault(w => w.weaponName == weaponName);
                    if (weapon != null)
                    {
                        int discrepancy = initialLife - damageTaken;
                        weapon.damage += discrepancy;
                        damageTaken = initialLife;
                    }
                }
            }

            public void SendBossFightToPanel()
            {
                weapons = weapons.OrderByDescending(w => w.damage).ToList();
                PanelSystem sys = ModContent.GetInstance<PanelSystem>();
                sys.state.container.panel.UpdateDamageBars(weapons);
            }

            public void PrintFightData(Mod mod)
            {
                string highestWeapon = weapons[0].weaponName;
                int highestDamage = weapons[0].damage;
                string s = $"{highestWeapon} ({highestDamage})";
                mod.Logger.Info($"Boss: {bossName} | ID: {bossId} | Highest weapon: {s} | DamageTaken: {damageTaken} | InitialLife: {initialLife}");
            }
        }

        public class Weapon
        {
            internal int weaponItemID; // the internal item ID, can be (0-5000+...) used for drawing.
            internal string weaponName;
            internal int damage;
        }
        #endregion

        private BossFight fight;
        // private int fightId = 0;

        #region Hooks
        public override void OnEnterWorld()
        {
            Main.NewText(
                "Hello, " + Main.LocalPlayer.name + 
                "! To use the DPS panel, type /dps toggle in chat or toggle with K (set the keybind in controls).", 
                Color.Yellow
            );
        }

        public override void PreUpdate()
        {
            // Check once per second
            if (Main.time % 60 == 0)
            {
                // 1) If there's an active fight and the boss is no longer alive or present, stop tracking
                if (fight != null && !Main.npc.Any(npc => npc.active && npc.boss && npc.FullName == fight.bossName))
                {
                    Mod.Logger.Info($"Boss {fight.bossName} despawned!");
                    fight = null; // stop tracking
                }

                // 2) If no fight, check if a boss is present; start a new fight if so
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
            TrackBossDamage(damageDone, target);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TrackBossDamage(damageDone, target);
        }
        #endregion

        #region Methods
        private void CreateNewBossFight(NPC npc)
        {
            if (fight == null)
            {
                fight = new BossFight
                {
                    // bossId = fightId,
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

        private void TrackBossDamage(int damageDone, NPC npc)
        {
            if (IsValidBoss(npc))
            {
                string weaponName = GetSourceWeaponName();
                int weaponID = GetSourceWeaponItemID();

                // If the boss died, handle final blow then end fight
                if (HandleBossDeath(npc, weaponName)) 
                    return; 

                // Otherwise update the fight info
                fight.UpdateWeapon(weaponID, weaponName, damageDone);
                fight.damageTaken += damageDone;

                // Update UI and log
                fight.SendBossFightToPanel();
                fight.PrintFightData(Mod);
            }
        }

        private string GetSourceWeaponName()
        {
            string sourceWeapon = GlobalProj.sourceWeapon?.Name;
            if (string.IsNullOrEmpty(sourceWeapon))
                sourceWeapon = "unknown";
            return sourceWeapon;
        }

        private int GetSourceWeaponItemID()
        {
            int sourceWeaponID = GlobalProj.sourceWeapon?.type ?? -1; // -1 is an invalid item ID
            return sourceWeaponID;
        }

        private bool HandleBossDeath(NPC npc, string weaponName)
        {
            if (npc.life <= 0)
            {
                fight.isAlive = false;
                fight.FixFinalBlowDiscrepancy(weaponName); // ensure total damage == boss max HP
                fight.SendBossFightToPanel();

                // fightId++;
                fight = null;
                return true;
            }
            return false;
        }

        private bool IsValidBoss(NPC npc)
        {
            return npc.boss && !npc.friendly;
        }
        #endregion
    
    }
}
