using System.Collections.Generic;
using System.Linq;
using DPSPanel.Common.Configs;
using DPSPanel.UI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DPSPanel.Common.DamageCalculation
{
    public class BossDamageTrackerSP : ModPlayer
    {
        #region Classes

        /// <summary>
        /// Represents a normal single‐NPC boss fight (e.g., Eye of Cthulhu, Skeletron, etc.).
        /// </summary>
        public class NormalBossFight
        {
            public int whoAmI;        // NPC index of the boss
            public string bossName;   // e.g., "Eye of Cthulhu"
            public int currentLife;
            public int initialLife;
            public int damageTaken;

            public List<Weapon> weapons = new List<Weapon>();

            public void UpdateWeapon(int weaponID, string weaponName, int damageDone)
            {
                Weapon existing = weapons.FirstOrDefault(w => w.weaponName == weaponName);
                if (existing == null)
                {
                    var newWeapon = new Weapon(weaponID, weaponName, damageDone);
                    weapons.Add(newWeapon);
                    // We create the weapon bar once
                    ModContent.GetInstance<MainSystem>().state.container.panel.CreateWeaponBarSP(weaponName);
                }
                else
                {
                    existing.damage += damageDone;
                }
            }

            public void SendBossFightToPanel()
            {
                weapons = weapons.OrderByDescending(w => w.damage).ToList();
                ModContent.GetInstance<MainSystem>()
                          .state
                          .container
                          .panel
                          .UpdateAllWeaponBarsSP(weapons);
            }
        }

        /// <summary>
        /// Represents an Eater of Worlds fight, which is multi‐segment.
        /// We do not store whoAmI, because the worm has multiple NPCs.
        /// Instead, we sum them up each time.
        /// </summary>
        public class EoWFight
        {
            public int damageTaken;
            public int totalLife;       // Current sum of all EoW segments
            public int totalLifeMax;    // Max sum of all EoW segments

            public List<Weapon> weapons = [];

            public void UpdateWeapon(int weaponID, string weaponName, int damageDone)
            {
                Weapon existing = weapons.FirstOrDefault(w => w.weaponName == weaponName);
                if (existing == null)
                {
                    var newWeapon = new Weapon(weaponID, weaponName, damageDone);
                    weapons.Add(newWeapon);
                    // Create UI element for this new weapon
                    ModContent.GetInstance<MainSystem>().state.container.panel.CreateWeaponBarSP(weaponName);
                }
                else
                {
                    existing.damage += damageDone;
                }
            }

            public void SendBossFightToPanel()
            {
                weapons = weapons.OrderByDescending(w => w.damage).ToList();
                ModContent.GetInstance<MainSystem>()
                          .state
                          .container
                          .panel
                          .UpdateAllWeaponBarsSP(weapons);
            }
        }

        #endregion

        #region Fields

        private NormalBossFight normalFight;  // For single‐NPC bosses
        private EoWFight eaterFight;          // For Eater of Worlds

        #endregion

        #region Hooks

        public override void PreUpdate()
        {
            // Only do local (single player) tracking; change if you want serverside logic
            if (Main.netMode != NetmodeID.SinglePlayer)
                return;

            // Check once per second
            if (Main.time % 60 == 0)
            {
                bool eaterFound = false; // Did we find at least one EoW segment?
                bool normalBossFound = false; // Did we find a normal boss that we want to track?

                for (int i = 0; i < Main.npc.Length; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active)
                        continue;

                    // 1) Check if this is a normal (single‐NPC) boss 
                    //    (not the EoW segments)
                    if (npc.boss
                        && npc.type != NPCID.EaterofWorldsHead
                        && npc.type != NPCID.EaterofWorldsBody
                        && npc.type != NPCID.EaterofWorldsTail)
                    {
                        normalBossFound = true;

                        // If we don't already have a normalFight,
                        // create one now for this boss.
                        if (normalFight == null)
                        {
                            normalFight = new NormalBossFight
                            {
                                whoAmI = npc.whoAmI,
                                bossName = npc.FullName,
                                currentLife = npc.life,
                                initialLife = npc.lifeMax,
                                damageTaken = 0
                            };

                            var sys = ModContent.GetInstance<MainSystem>();
                            sys.state.container.panel.ClearPanelAndAllItems();
                            sys.state.container.panel.SetBossTitle(
                                npc.FullName,
                                npc.whoAmI,
                                npc.GetBossHeadTextureIndex()
                            );
                        }
                    }

                    // 2) Check if this is an Eater of Worlds segment
                    bool isEaterSegment =
                        npc.type == NPCID.EaterofWorldsHead ||
                        npc.type == NPCID.EaterofWorldsBody ||
                        npc.type == NPCID.EaterofWorldsTail;

                    if (isEaterSegment)
                    {
                        eaterFound = true;

                        // If we don't already have an eaterFight, create one
                        if (eaterFight == null)
                        {
                            eaterFight = new EoWFight
                            {
                                damageTaken = 0,
                                totalLife = 0,
                                totalLifeMax = 0
                            };

                            var sys = ModContent.GetInstance<MainSystem>();
                            sys.state.container.panel.ClearPanelAndAllItems();
                            sys.state.container.panel.SetBossTitle(
                                "Eater of Worlds",
                                -1, // no single whoAmI for the worm
                                npc.GetBossHeadTextureIndex()
                            );
                        }
                    }
                }

                // If we had a normalFight, but the boss is actually gone now, check that
                if (!normalBossFound && normalFight != null)
                {
                    // That means the normal boss is dead or despawned
                    normalFight.SendBossFightToPanel();
                    normalFight = null;
                }
                else if (normalFight != null)
                {
                    // If the normalFight is active, update currentLife from the NPC
                    NPC bossNpc = Main.npc[normalFight.whoAmI];
                    if (!bossNpc.active || bossNpc.life <= 0)
                    {
                        // Boss died
                        normalFight.SendBossFightToPanel();
                        normalFight = null;
                    }
                    else
                    {
                        // Just refresh currentLife in case it’s changed
                        normalFight.currentLife = bossNpc.life;
                    }
                }

                // If we had an eaterFight, but no segments are found, that means EoW is gone
                if (!eaterFound && eaterFight != null)
                {
                    eaterFight.SendBossFightToPanel();
                    eaterFight = null;
                }
                // If we still have an eaterFight and found segments, recalc total life
                else if (eaterFound && eaterFight != null)
                {
                    RecalculateWormLife();
                    // If totalLife is 0, the worm is effectively dead
                    if (eaterFight.totalLife <= 0)
                    {
                        eaterFight.SendBossFightToPanel();
                        eaterFight = null;
                    }
                }
            }
        }

        /// <summary>
        /// Recalculate total EoW life by summing active segments.
        /// </summary>
        private void RecalculateWormLife()
        {
            if (eaterFight == null)
                return;

            int totalLife = 0;
            int totalLifeMax = 0;

            for (int i = 0; i < Main.npc.Length; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active)
                    continue;

                bool isEaterSegment =
                    npc.type == NPCID.EaterofWorldsHead ||
                    npc.type == NPCID.EaterofWorldsBody ||
                    npc.type == NPCID.EaterofWorldsTail;

                if (isEaterSegment)
                {
                    totalLife += npc.life;
                    totalLifeMax += npc.lifeMax;
                }
            }

            eaterFight.totalLife = totalLife;
            eaterFight.totalLifeMax = totalLifeMax;
        }

        /// <summary>
        /// Called when you hit an NPC with an Item (melee, etc.).
        /// </summary>
        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode != NetmodeID.SinglePlayer)
                return;

            int weaponID = item?.type ?? -1;
            string weaponName = item?.Name ?? "Unknown";
            TrackBossDamage(weaponID, weaponName, damageDone, target);
        }

        /// <summary>
        /// Called when you hit an NPC with a Projectile (ranged, magic, summon, etc.).
        /// </summary>
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode != NetmodeID.SinglePlayer)
                return;

            // If your GlobalProjectile stored the original weapon:
            GlobalProj gProj = proj.GetGlobalProjectile<GlobalProj>();

            int weaponID = -1;
            string weaponName = "Unknown";
            if (gProj != null && gProj.sourceWeapon != null)
            {
                weaponID = gProj.sourceWeapon.type;
                weaponName = gProj.sourceWeapon.Name;
            }

            TrackBossDamage(weaponID, weaponName, damageDone, target);
        }

        #endregion

        #region Methods

        private void TrackBossDamage(int weaponID, string weaponName, int damageDone, NPC npc)
        {
            Config c = ModContent.GetInstance<Config>();
            // If we are not supposed to track all entities, and there's no active boss fight, just return
            bool anyFightActive = (normalFight != null || eaterFight != null);
            if (!anyFightActive)
                return;

            // 1) If we have a normalFight, is this NPC the boss for that fight?
            if (normalFight != null && npc.whoAmI == normalFight.whoAmI)
            {
                // Update the weapon damage
                normalFight.UpdateWeapon(weaponID, weaponName, damageDone);
                normalFight.damageTaken += damageDone;

                // If the boss died from this hit
                if (npc.life <= 0)
                {
                    normalFight.SendBossFightToPanel();
                    normalFight = null;
                }
                else
                {
                    normalFight.currentLife = npc.life;
                }

                // Update UI
                normalFight?.SendBossFightToPanel();
            }
            // 2) If we have an eaterFight, is this NPC an EoW segment?
            else if (eaterFight != null)
            {
                bool isEoWSegment =
                    npc.type == NPCID.EaterofWorldsHead ||
                    npc.type == NPCID.EaterofWorldsBody ||
                    npc.type == NPCID.EaterofWorldsTail;

                if (isEoWSegment)
                {
                    eaterFight.UpdateWeapon(weaponID, weaponName, damageDone);
                    eaterFight.damageTaken += damageDone;

                    // After hitting a segment, recalc total HP
                    RecalculateWormLife();
                    if (eaterFight.totalLife <= 0)
                    {
                        // The worm is fully dead
                        eaterFight.SendBossFightToPanel();
                        eaterFight = null;
                    }
                    else
                    {
                        // Keep tracking
                        eaterFight.SendBossFightToPanel();
                    }
                }
            }
        }

        #endregion
    }
}
