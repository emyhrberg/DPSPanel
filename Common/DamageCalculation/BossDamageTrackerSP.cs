﻿using System;
using System.Collections.Generic;
using System.Linq;
using DPSPanel.Common.Configs;
using DPSPanel.Helpers;
using DPSPanel.UI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DPSPanel.Common.DamageCalculation
{
    public class BossDamageTrackerSP : ModPlayer
    {
        #region Classes
        public class BossFight
        {
            public int currentLife;
            public int whoAmI;
            public int initialLife;
            public string bossName;
            public int damageTaken;
            public bool isAlive = false;
            public List<Weapon> weapons = [];

            public void UpdateWeapon(int weaponID, string weaponName, int damageDone)
            {
                // Check if the weapon already exists
                Weapon weapon = weapons.FirstOrDefault(w => w.weaponName == weaponName);
                if (weapon == null)
                {
                    // Check if we already reach max cap of weapons
                    Config c = ModContent.GetInstance<Config>();

                    // Add a new weapon to the fight
                    weapon = new Weapon(weaponID, weaponName, damageDone);
                    weapons.Add(weapon);
                    weapons = weapons.OrderByDescending(w => w.damage).ToList();
                    MainSystem sys = ModContent.GetInstance<MainSystem>();
                    sys.state.container.panel.CreateWeaponBarSP(weaponName);
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

                    // If the final blow weapon reference is null, treat it as "Unknown"
                    if (weapon == null)
                    {
                        // Try to update the existing "Unknown" weapon damage
                        Weapon unknownWeapon = weapons.FirstOrDefault(w => w.weaponName == "Unknown");
                        if (unknownWeapon != null)
                        {
                            unknownWeapon.damage += unknownDamage;
                        }
                        else
                        {
                            // If "Unknown" doesn't exist, create it
                            unknownWeapon = new Weapon(-1, "Unknown", unknownDamage)
                            {
                                weaponName = "Unknown",
                                weaponItemID = -1, // Invalid ID so no icon is drawn
                                damage = unknownDamage
                            };
                            weapons.Add(unknownWeapon);

                            // Ensure the list is re-sorted and update the UI accordingly
                            weapons = weapons.OrderByDescending(w => w.damage).ToList();
                            MainSystem sys = ModContent.GetInstance<MainSystem>();
                            sys.state.container.panel.CreateWeaponBarSP("Unknown");
                        }
                    }
                    else
                    {
                        // Log.Info($"Final BLOW on Boss: {bossName} | Unknown Damage: {unknownDamage} weapon: {weapon.weaponName} | damage: {weapon.damage}");

                        // Add the discrepancy to the weapon that landed the final blow
                        Weapon finalWeapon = weapons.FirstOrDefault(w => w.weaponName == weapon.weaponName);
                        if (finalWeapon != null)
                        {
                            finalWeapon.damage += unknownDamage;
                        }
                    }
                }
            }

            public void SendBossFightToPanel()
            {
                weapons = weapons.OrderByDescending(w => w.damage).ToList();
                MainSystem sys = ModContent.GetInstance<MainSystem>();
                sys.state.container.panel.UpdateAllWeaponBarsSP(weapons);
            }

            public void PrintFightData(Mod mod)
            {
                if (weapons.Count == 0)
                {
                    // Log.info($"Boss: {bossName} | ID: {bossId} | No weapons tracked | DamageTaken: {damageTaken} | InitialLife: {initialLife}");
                    return;
                }

                string highestWeapon = weapons[0].weaponName;
                int highestDamage = weapons[0].damage;
                string s = $"{highestWeapon} ({highestDamage})";
                int weaponID = weapons[0].weaponItemID;
                // // Log.info($"Boss: {bossName} | ID: {bossId} | Highest weapon: {s} ID: {weaponID} | DamageTaken: {damageTaken} | InitialLife: {initialLife}");
            }
        }
        #endregion

        private BossFight fight;
        // private int fightId = 0;

        #region Hooks
        public override void PreUpdate()
        {
            if (Main.netMode != NetmodeID.SinglePlayer)
                return;

            // Check once per second
            if (Main.time % 60 == 0)
            {
                // 2) If no fight exists, check for an active boss to start tracking
                if (fight == null)
                {
                    NPC detectedBoss = null;

                    for (int i = 0; i < Main.npc.Length; i++)
                    {
                        NPC npc = Main.npc[i];

                        // Detect worm bosses (e.g., Eater of Worlds, Destroyer)
                        if (npc.active && npc.type == NPCID.EaterofWorldsHead)
                        {
                            detectedBoss = npc;
                            Log.Info("Eater of Worlds detected with real life and whoAmI: " + npc.whoAmI + ", " + npc.realLife);
                            break; // Found a valid boss, no need to check further
                        }

                        // Detect regular non-worm bosses.
                        if (IsValidBoss(npc) && npc.life > 0)
                        {
                            detectedBoss = npc;
                            break; // Found a valid boss, no need to check further
                        }
                    }

                    if (detectedBoss != null)
                    {
                        CreateNewBossFight(detectedBoss);
                        // Log.info($"(SP) Boss {fight.bossName} created!");
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

                        // // Log.info($"Boss: {fight.bossName} | initial life: {fight.initialLife} | Total Tracked Damage: {totalTrackedDamage} | Current Life: {fight.currentLife} | Unknown Damage: {unknownDamage}");

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
                                unknownWeapon = new Weapon(-1, "Unknown", unknownDamage)
                                {
                                    weaponName = "Unknown",
                                    weaponItemID = -1, // Invalid ID so no icon is drawn
                                    damage = unknownDamage
                                };
                                fight.damageTaken += unknownDamage;
                                fight.weapons.Add(unknownWeapon);
                                fight.weapons = fight.weapons.OrderByDescending(w => w.damage).ToList();

                                // Create damage bar for "Unknown" in the UI
                                MainSystem sys = ModContent.GetInstance<MainSystem>();
                                sys.state.container.panel.CreateWeaponBarSP("Unknown");
                            }
                            else
                            {
                                unknownWeapon.damage = unknownDamage;
                            }
                        }

                        // Update the UI with the latest damage data
                        fight.SendBossFightToPanel();
                        // fight.PrintFightData(Mod);
                    }
                    else
                    {
                        // Boss NPC is no longer active; ensure fight is stopped
                        // Log.info($"Boss {fight.bossName} is no longer active!");
                        fight = null;
                    }
                }
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode != NetmodeID.SinglePlayer)
                return;

            // Log.Info($"OnHitNPCWithItem: ({item.Name}) ({damageDone})");

            // Pass the actual item type (ID) explicitly for melee
            int actualItemID = item?.type ?? -1;
            string actualItemName = item?.Name ?? "unknownitem";
            TrackBossDamage(actualItemID, actualItemName, damageDone, target);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode != NetmodeID.SinglePlayer)
                return;

            // Grab the GlobalProj for *this* projectile
            GlobalProj gProj = proj.GetGlobalProjectile<GlobalProj>();

            // Fallback values in case there's no stored weapon
            int weaponID = -1;
            string weaponName = "Unknown";

            // If we actually stored an item in OnSpawn, use it
            if (gProj != null && gProj.sourceWeapon != null)
            {
                weaponID = gProj.sourceWeapon.type;
                weaponName = gProj.sourceWeapon.Name;
            }

            // Optional logging
            // Log.Info($"OnHitNPCWithProj: Projectile: {proj.Name}, Damage: {damageDone}, Weapon: {weaponName}, WeaponID: {weaponID}");

            // Now pass these IDs/names to your track method
            TrackBossDamage(weaponID, weaponName, damageDone, target);
        }

        #endregion

        #region Methods
        private void TrackBossDamage(int weaponID, string weaponName, int damageDone, NPC npc)
        {
            Config c = ModContent.GetInstance<Config>();

            if (fight != null && !npc.friendly)
            {
                // If not tracking all entities and the NPC is not the boss, return
                if (!c.TrackAllEntities && !(npc.boss && npc.whoAmI == fight.whoAmI))
                    return;

                // If the NPC is the actual boss.
                if (npc.whoAmI == fight.whoAmI)
                {
                    // If the boss died, handle final blow then end fight
                    Weapon currentWeapon = fight.weapons.FirstOrDefault(w => w.weaponName == weaponName);

                    if (HandleBossDeath(npc, currentWeapon))
                        return;

                    fight.currentLife = npc.life;
                }

                // Otherwise update the fight info
                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    fight.UpdateWeapon(weaponID, weaponName, damageDone);
                }

                fight.damageTaken += damageDone;

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
                    whoAmI = npc.whoAmI,
                    currentLife = npc.life,
                    initialLife = npc.lifeMax,
                    bossName = npc.FullName,
                    damageTaken = 0,
                    weapons = [],
                    isAlive = true
                };
                var sys = ModContent.GetInstance<MainSystem>();
                sys.state.container.panel.ClearPanelAndAllItems();
                sys.state.container.panel.SetBossTitle(npc.FullName, npc.whoAmI, npc.GetBossHeadTextureIndex());
            }
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

        private bool IgnoreGolem(NPC npc)
        {
            return npc.type == NPCID.Golem || npc.type == NPCID.GolemFistLeft || npc.type == NPCID.GolemFistRight;
        }
    }
}