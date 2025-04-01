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
                    // Create the weapon bar once
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
        /// Instead, we sum them up each time from the known EoW segment types.
        /// </summary>
        public class EoWFight
        {
            public int damageTaken;
            public int totalLife;       // Current sum of all EoW segments
            public int totalLifeMax;    // Max sum of all EoW segments

            public List<Weapon> weapons = new List<Weapon>();

            public void UpdateWeapon(int weaponID, string weaponName, int damageDone)
            {
                Weapon existing = weapons.FirstOrDefault(w => w.weaponName == weaponName);
                if (existing == null)
                {
                    var newWeapon = new Weapon(weaponID, weaponName, damageDone);
                    weapons.Add(newWeapon);
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
        /// Represents a general multi‐segment worm‐like boss fight (e.g., The Destroyer),
        /// tracked by realLife. The "headIndex" is the whoAmI of the head segment.
        /// </summary>
        public class WormBossFight
        {
            public int headIndex;     // NPC.whoAmI of the worm's head
            public string bossName;   // The name of the worm boss (e.g., "The Destroyer")

            public int totalLife;
            public int totalLifeMax;
            public int damageTaken;

            public List<Weapon> weapons = new List<Weapon>();

            public WormBossFight(int headIndex, string bossName)
            {
                this.headIndex = headIndex;
                this.bossName = bossName;
            }

            public void UpdateWeapon(int weaponID, string weaponName, int damageDone)
            {
                Weapon existing = weapons.FirstOrDefault(w => w.weaponName == weaponName);
                if (existing == null)
                {
                    var newWeapon = new Weapon(weaponID, weaponName, damageDone);
                    weapons.Add(newWeapon);
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
        private EoWFight eaterFight;          // For the Eater of Worlds specifically
        private WormBossFight wormFight;      // For all other worm‐like bosses (e.g., The Destroyer)

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
                bool eaterFound = false;        // Did we find any EoW segment?
                bool otherWormFound = false;    // Did we find a worm that's NOT EoW?
                bool normalBossFound = false;   // Did we find a normal single‐NPC boss?

                for (int i = 0; i < Main.npc.Length; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active)
                        continue;

                    // 1) Normal single‐NPC boss (boss && realLife == -1).
                    //    EoW and other worms typically have realLife != -1
                    //    so this excludes them.
                    if (npc.boss && npc.realLife == -1)
                    {
                        normalBossFound = true;
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

                    // 2) Check EoW specifically by type:
                    bool isEoWSegment =
                        npc.type == NPCID.EaterofWorldsHead ||
                        npc.type == NPCID.EaterofWorldsBody ||
                        npc.type == NPCID.EaterofWorldsTail;

                    if (isEoWSegment)
                    {
                        eaterFound = true;
                        if (eaterFight == null)
                        {
                            eaterFight = new EoWFight
                            {
                                damageTaken = 0,
                                totalLife = 0,
                                totalLifeMax = 0
                            };

                            // Minimal logging
                            Mod.Logger.Info(
                                $"[DPSPanel] Detected EoW segment: FullName={npc.FullName}, " +
                                $"whoAmI={npc.whoAmI}, realLife={npc.realLife}"
                            );

                            var sys = ModContent.GetInstance<MainSystem>();
                            sys.state.container.panel.ClearPanelAndAllItems();
                            // We can just name it "Eater of Worlds" or npc.FullName
                            sys.state.container.panel.SetBossTitle(
                                "Eater of Worlds",
                                -1,
                                npc.GetBossHeadTextureIndex()
                            );
                        }
                    }
                    // 3) Check if realLife != -1 for "other worm" logic (e.g., The Destroyer).
                    //    But skip EoW since that's already handled by the check above.
                    else if (npc.realLife != -1)
                    {
                        NPC headNpc = Main.npc[npc.realLife];
                        // If the head is a boss and it's not EoW, let's treat it as a worm boss
                        if (headNpc.boss && !IsEaterOfWorlds(headNpc.type))
                        {
                            otherWormFound = true;

                            // Create a general wormFight if none exists
                            if (wormFight == null)
                            {
                                wormFight = new WormBossFight(npc.realLife, headNpc.FullName)
                                {
                                    damageTaken = 0,
                                    totalLife = 0,
                                    totalLifeMax = 0
                                };

                                // Minimal logging
                                Mod.Logger.Info(
                                    $"[DPSPanel] Detected worm-like boss segment: FullName={npc.FullName}, " +
                                    $"whoAmI={npc.whoAmI}, realLife={npc.realLife}. " +
                                    $"Head = {headNpc.FullName} (whoAmI={headNpc.whoAmI})"
                                );

                                var sys = ModContent.GetInstance<MainSystem>();
                                sys.state.container.panel.ClearPanelAndAllItems();
                                sys.state.container.panel.SetBossTitle(
                                    headNpc.FullName,
                                    -1, // no single whoAmI for the entire worm
                                    headNpc.GetBossHeadTextureIndex()
                                );
                            }
                        }
                    }
                }

                // NORMAL BOSS cleanup
                if (!normalBossFound && normalFight != null)
                {
                    // That means the normal boss is dead or despawned
                    normalFight.SendBossFightToPanel();
                    normalFight = null;
                }
                else if (normalFight != null)
                {
                    // If the boss is still active, update life
                    NPC bossNpc = Main.npc[normalFight.whoAmI];
                    if (!bossNpc.active || bossNpc.life <= 0)
                    {
                        normalFight.SendBossFightToPanel();
                        normalFight = null;
                    }
                    else
                    {
                        normalFight.currentLife = bossNpc.life;
                    }
                }

                // EATER OF WORLDS cleanup
                if (!eaterFound && eaterFight != null)
                {
                    eaterFight.SendBossFightToPanel();
                    eaterFight = null;
                }
                else if (eaterFound && eaterFight != null)
                {
                    RecalculateEoWLife();
                    if (eaterFight.totalLife <= 0)
                    {
                        eaterFight.SendBossFightToPanel();
                        eaterFight = null;
                    }
                }

                // OTHER WORM cleanup
                if (!otherWormFound && wormFight != null)
                {
                    wormFight.SendBossFightToPanel();
                    wormFight = null;
                }
                else if (otherWormFound && wormFight != null)
                {
                    RecalculateWormLife();
                    if (wormFight.totalLife <= 0)
                    {
                        wormFight.SendBossFightToPanel();
                        wormFight = null;
                    }
                }
            }
        }

        /// <summary>
        /// Helper: Check if an npcType corresponds to Eater of Worlds segments.
        /// </summary>
        private bool IsEaterOfWorlds(int npcType)
        {
            return (npcType == NPCID.EaterofWorldsHead ||
                    npcType == NPCID.EaterofWorldsBody ||
                    npcType == NPCID.EaterofWorldsTail);
        }

        /// <summary>
        /// Sums up the active life of all EoW segments.
        /// </summary>
        private void RecalculateEoWLife()
        {
            if (eaterFight == null)
                return;

            int total = 0;
            int totalMax = 0;

            for (int i = 0; i < Main.npc.Length; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active)
                    continue;

                if (IsEaterOfWorlds(npc.type))
                {
                    total += npc.life;
                    totalMax += npc.lifeMax;
                }
            }

            eaterFight.totalLife = total;
            eaterFight.totalLifeMax = totalMax;
        }

        /// <summary>
        /// Sums up the active life of all worm segments sharing the wormFight.headIndex.
        /// </summary>
        private void RecalculateWormLife()
        {
            if (wormFight == null)
                return;

            int total = 0;
            int totalMax = 0;
            int headIndex = wormFight.headIndex;

            for (int i = 0; i < Main.npc.Length; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active)
                    continue;

                // All segments that belong to the same worm have npc.realLife == headIndex
                if (npc.realLife == headIndex)
                {
                    total += npc.life;
                    totalMax += npc.lifeMax;
                }
            }

            wormFight.totalLife = total;
            wormFight.totalLifeMax = totalMax;
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
            // Only track if a fight is active
            bool anyFightActive = (normalFight != null || eaterFight != null || wormFight != null);
            if (!anyFightActive)
                return;

            // 1) Is this NPC the single‐NPC boss we’re tracking?
            if (normalFight != null && npc.whoAmI == normalFight.whoAmI)
            {
                normalFight.UpdateWeapon(weaponID, weaponName, damageDone);
                normalFight.damageTaken += damageDone;

                if (npc.life <= 0)
                {
                    normalFight.SendBossFightToPanel();
                    normalFight = null;
                }
                else
                {
                    normalFight.currentLife = npc.life;
                }
                normalFight?.SendBossFightToPanel();
            }
            // 2) Is this an EoW segment?
            else if (eaterFight != null && IsEaterOfWorlds(npc.type))
            {
                eaterFight.UpdateWeapon(weaponID, weaponName, damageDone);
                eaterFight.damageTaken += damageDone;

                RecalculateEoWLife();
                if (eaterFight.totalLife <= 0)
                {
                    eaterFight.SendBossFightToPanel();
                    eaterFight = null;
                }
                else
                {
                    eaterFight.SendBossFightToPanel();
                }
            }
            // 3) Is this part of another worm‐like boss?
            else if (wormFight != null && npc.realLife == wormFight.headIndex)
            {
                wormFight.UpdateWeapon(weaponID, weaponName, damageDone);
                wormFight.damageTaken += damageDone;

                RecalculateWormLife();
                if (wormFight.totalLife <= 0)
                {
                    wormFight.SendBossFightToPanel();
                    wormFight = null;
                }
                else
                {
                    wormFight.SendBossFightToPanel();
                }
            }
        }

        #endregion
    }
}
