using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Linq;
using DPSPanel.Core.Configs;
using DPSPanel.Core.Helpers;
using System;
using Terraria.GameContent;
using Terraria.ID;
using log4net.Repository.Hierarchy;
using log4net;

namespace DPSPanel.Core.Panel
{
    public class BossDamageTrackerSP : ModPlayer
    {
        #region Classes
        public class BossFight
        {
            public int currentLife;
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

            public void FixFinalBlowDiscrepancy(Weapon weapon)
            {
                if (damageTaken < initialLife)
                {
                    int totalTrackedDamage = weapons.Sum(w => w.damage);
                    int unknownDamage = initialLife - totalTrackedDamage;
                    // add to unknown


                    ILog logger = ModContent.GetInstance<DPSPanel>().Logger;
                    logger.Info($"Final BLOW on Boss: {bossName} | Unknown Damage: {unknownDamage} weapon: {weapon.weaponName} | damage: {weapon.damage}");
                    // add to damage if weapon exists
                    
                    Weapon finalWeapon = weapons.FirstOrDefault(w => w.weaponName == weapon.weaponName);
                    if (finalWeapon != null)
                        finalWeapon.damage += unknownDamage;


                    // TODO if the final blow is an unknown weapon... doesnt work.
                    // Weapon unknownWeapon = weapons.FirstOrDefault(w => w.weaponName == "Unknown");
                    // if (unknownWeapon != null)
                    // {
                    //     unknownWeapon.damage += unknownDamage;
                    // }
                    // else
                    // {
                    //     unknownWeapon = new Weapon
                    //     {
                    //         weaponName = "Unknown",
                    //         weaponItemID = -1, // Invalid ID so no icon is drawn
                    //         damage = unknownDamage
                    //     };
                    //     weapons.Add(unknownWeapon);
                    //     weapons = weapons.OrderByDescending(w => w.damage).ToList();
                    //     PanelSystem sys = ModContent.GetInstance<PanelSystem>();
                    //     sys.state.container.panel.CreateDamageBar("Unknown");
                    // }

                    // if (weapon != null)
                    // {
                    //     // Add discrepancy to the weapon that landed the final blow
                    //     int discrepancy = initialLife - damageTaken;
                    //     weapon.damage += discrepancy;
                    //     damageTaken = initialLife;
                    // }
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
                if (weapons.Count == 0)
                {
                    mod.Logger.Info($"Boss: {bossName} | ID: {bossId} | No weapons tracked | DamageTaken: {damageTaken} | InitialLife: {initialLife}");
                    return;
                }

                string highestWeapon = weapons[0].weaponName;
                int highestDamage = weapons[0].damage;
                string s = $"{highestWeapon} ({highestDamage})";
                int weaponID = weapons[0].weaponItemID;
                // mod.Logger.Info($"Boss: {bossName} | ID: {bossId} | Highest weapon: {s} ID: {weaponID} | DamageTaken: {damageTaken} | InitialLife: {initialLife}");
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
                    Mod.Logger.Info($"Boss {fight.bossName} was killed or despawned!");
                    fight = null; // stop tracking
                }

                // 2) If no fight exists, check for an active boss to start tracking
                if (fight == null)
                {
                    NPC detectedBoss = null;

                    for (int i = 0; i < Main.npc.Length; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (IsValidBoss(npc) && npc.life > 0)
                        {
                            detectedBoss = npc;
                            break; // Found a valid boss, no need to check further
                        }
                    }

                    if (detectedBoss != null)
                    {
                        CreateNewBossFight(detectedBoss);
                        Mod.Logger.Info($"Boss {fight.bossName} created!");
                    }
                }

                // 3) If a fight is active, handle "Unknown" damage
                if (fight != null && fight.isAlive)
                {
                    // Find the current boss NPC based on the fight's boss name
                    NPC bossNPC = Main.npc.FirstOrDefault(npc => npc.active && npc.boss && npc.FullName == fight.bossName);

                    if (bossNPC != null)
                    {
                        fight.currentLife = bossNPC.life;
                        int totalTrackedDamage = fight.weapons.Where(w => w.weaponName != "Unknown").Sum(w => w.damage);
                        int unknownDamage = fight.initialLife - fight.currentLife - totalTrackedDamage;

                        Mod.Logger.Info($"Boss: {fight.bossName} | initial life: {fight.initialLife} | Total Tracked Damage: {totalTrackedDamage} | Current Life: {fight.currentLife} | Unknown Damage: {unknownDamage}");

                        // Ensure discrepancy is not negative
                        unknownDamage = Math.Max(unknownDamage, 0);

                        // Update the "Unknown" damage
                        Weapon unknownWeapon = fight.weapons.FirstOrDefault(w => w.weaponName == "Unknown");
                        if (unknownWeapon != null)
                        {
                            unknownWeapon.damage = unknownDamage;
                        }
                        else
                        {
                            if (unknownDamage == 0)
                                return;

                            // If "Unknown" doesn't exist, create it
                            bool unknownWeaponExists = fight.weapons.Any(w => w.weaponName == "Unknown");
                            if (!unknownWeaponExists)
                            {
                                unknownWeapon = new Weapon
                                {
                                    weaponName = "Unknown",
                                    weaponItemID = -1, // Invalid ID so no icon is drawn
                                    damage = unknownDamage
                                };
                                fight.damageTaken += unknownDamage;
                                fight.weapons.Add(unknownWeapon);
                                fight.weapons = fight.weapons.OrderByDescending(w => w.damage).ToList();

                                // Create damage bar for "Unknown" in the UI
                                PanelSystem sys = ModContent.GetInstance<PanelSystem>();
                                sys.state.container.panel.CreateDamageBar("Unknown");
                            } else {
                                unknownWeapon.damage = unknownDamage;
                            }
                        }

                        // Update the UI with the latest damage data
                        fight.SendBossFightToPanel();
                        fight.PrintFightData(Mod);
                    }
                    else
                    {
                        // Boss NPC is no longer active; ensure fight is stopped
                        Mod.Logger.Info($"Boss {fight.bossName} is no longer active!");
                        fight = null;
                    }
                }
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Pass the actual item type (ID) explicitly for melee
            int actualItemID = item?.type ?? -1;
            string actualItemName = item?.Name ?? "unknownitem";
            TrackBossDamage(actualItemID, actualItemName, damageDone, target);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            string weaponName = GetSourceWeaponName();
            int weaponID = GetSourceWeaponItemID();
            TrackBossDamage(weaponID, weaponName, damageDone, target);
        }
        #endregion

        #region Methods
        private void TrackBossDamage(int weaponID, string weaponName, int damageDone, NPC npc)
        {
            if (IsValidBoss(npc))
            {
                // If the boss died, handle final blow then end fight
                Weapon currentWeapon = fight.weapons.FirstOrDefault(w => w.weaponName == weaponName);

                if (HandleBossDeath(npc, currentWeapon)) 
                    return; 

                // Otherwise update the fight info
                fight.UpdateWeapon(weaponID, weaponName, damageDone);
                fight.damageTaken += damageDone;
                fight.currentLife = npc.life;

                // Update UI and log
                fight.SendBossFightToPanel();
                fight.PrintFightData(Mod);
            }
        }

        private void CreateNewBossFight(NPC npc)
        {
            if (fight == null)
            {
                fight = new BossFight
                {
                    // bossId = fightId,
                    currentLife = npc.life,
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

        private bool HandleBossDeath(NPC npc, Weapon weapon)
        {
            if (npc.life <= 0)
            {
                fight.isAlive = false;
                fight.FixFinalBlowDiscrepancy(weapon); // ensure total damage == boss max HP
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
