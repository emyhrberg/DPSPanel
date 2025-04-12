using System.Linq;
using DPSPanel.DamageCalculation.Classes;
using DPSPanel.Helpers;
using DPSPanel.UI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static DPSPanel.Common.Configs.Config;

namespace DPSPanel.Common.DamageCalculation
{
    public class BossDamageTrackerSP : ModPlayer
    {
        #region Fields

        private NormalBossFight normalFight;  // For single‐NPC bosses
        private EoWFight eaterFight;          // For the Eater of Worlds specifically
        private WormBossFight wormFight;      // For all other worm‐like bosses (e.g., The Destroyer)

        #endregion

        #region Hooks

        public override void PreUpdate()
        {
            // Only do local (single player) tracking;
            if (Main.netMode != NetmodeID.SinglePlayer)
                return;

            // Keep track of the current boss fight
            bool eaterFound = false;        // Did we find any EoW segment?
            bool otherWormFound = false;    // Did we find a worm that's NOT EoW?
            bool normalBossFound = false;   // Did we find a normal single‐NPC boss?

            // Check if the current fight whoAmI is still alive
            if (normalFight != null)
            {
                NPC bossNpc = Main.npc[normalFight.whoAmI];
                // Track the Name, NPC whoAmI, active status, and life
                // Log.SlowInfo($"[DPSPanel] Tracking boss: Name={bossNpc.FullName}, whoAmI={bossNpc.whoAmI}, Active={bossNpc.active}, Life={bossNpc.life}");

                // If any boss is alive, keep tracking something
                // This is a dumb way to fix the issue of multiple bosses being alive at the same time.
                if (Main.npc.Any(npc => npc.active && npc.boss))
                {
                    return;
                }

                if (!bossNpc.active || bossNpc.life <= 0)
                {
                    // Optionally, reset the panel.
                    // But we actually want to keep stats once boss is dead so we can view them, so we dont reset.
                    // MainSystem sys = ModContent.GetInstance<MainSystem>();
                    // sys.state.container.panel.ClearPanelAndAllItems();
                    // sys.state.container.panel.SetBossTitle("DPSPanel", -1, -1);

                    // Log.Info("[SP] Normal boss is dead or despawned.");
                    normalFight.SendBossFightToPanel();
                    normalFight = null;
                }
            }

            // Check for EoW
            foreach (NPC npc in Main.npc)
            {
                bool IsEaterOfWorldsAlive = npc.active && IsEaterOfWorlds(npc.type);
                if (eaterFight == null && IsEaterOfWorldsAlive)
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
                        Log.Info(
                                                $"[DPSPanel] Detected EoW segment: FullName={npc.FullName}, " +
                                                $"whoAmI={npc.whoAmI}, realLife={npc.realLife}"
                                            );
                    }
                    MainSystem sys = ModContent.GetInstance<MainSystem>();
                    sys.state.container.panel.ClearPanelAndAllItems();
                    sys.state.container.panel.SetBossTitle(
                        "Eater of Worlds",
                        -1,
                        npc.GetBossHeadTextureIndex()
                    );
                }
            }

            // Check for normal bosses
            bool isAnyBossAlive = Main.npc.Any(npc => npc.active && npc.boss);
            if (!isAnyBossAlive)
                return;

            // Get the boss
            // TODO fix so we can get multiple bosses
            NPC activeBossNpc = Main.npc.FirstOrDefault(npc => npc.active && npc.boss);

            // 1) Normal single‐NPC boss (boss && realLife == -1).
            if (activeBossNpc != null && activeBossNpc.realLife == -1)
            {
                // Log.Info("[SP] Found normal boss: " + activeBossNpc.FullName);
                normalBossFound = true;
                if (normalFight == null)
                {
                    normalFight = new NormalBossFight
                    {
                        whoAmI = activeBossNpc.whoAmI,
                        bossName = activeBossNpc.FullName,
                        currentLife = activeBossNpc.life,
                        // initialLife = activeBossNpc.lifeMax,
                        damageTaken = 0
                    };

                    var sys = ModContent.GetInstance<MainSystem>();
                    sys.state.container.panel.ClearPanelAndAllItems();
                    sys.state.container.panel.SetBossTitle(
                        activeBossNpc.FullName,
                        activeBossNpc.whoAmI,
                        activeBossNpc.GetBossHeadTextureIndex()
                    );
                }
            }

            // 2) Check EoW specifically by type:
            bool isEoWSegment =
                activeBossNpc.type == NPCID.EaterofWorldsHead ||
                activeBossNpc.type == NPCID.EaterofWorldsBody ||
                activeBossNpc.type == NPCID.EaterofWorldsTail;

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
                    Log.Info(
                        $"[DPSPanel] Detected EoW segment: FullName={activeBossNpc.FullName}, " +
                        $"whoAmI={activeBossNpc.whoAmI}, realLife={activeBossNpc.realLife}"
                    );

                    var sys = ModContent.GetInstance<MainSystem>();
                    sys.state.container.panel.ClearPanelAndAllItems();
                    // We can just name it "Eater of Worlds" or npc.FullName
                    sys.state.container.panel.SetBossTitle(
                        "Eater of Worlds",
                        -1,
                        activeBossNpc.GetBossHeadTextureIndex()
                    );
                }
            }
            // 3) Check if realLife != -1 for "other worm" logic (e.g., The Destroyer).
            //    But skip EoW since that's already handled by the check above.
            else if (activeBossNpc.realLife != -1)
            {
                NPC headNpc = Main.npc[activeBossNpc.realLife];
                // If the head is a boss and it's not EoW, let's treat it as a worm boss
                if (headNpc.boss && !IsEaterOfWorlds(headNpc.type))
                {
                    otherWormFound = true;

                    // Create a general wormFight if none exists
                    if (wormFight == null)
                    {
                        wormFight = new WormBossFight(activeBossNpc.realLife, headNpc.FullName)
                        {
                            damageTaken = 0,
                            totalLife = 0,
                            totalLifeMax = 0
                        };

                        // Minimal logging
                        Log.Info(
                            $"[DPSPanel] Detected worm-like boss segment: FullName={activeBossNpc.FullName}, " +
                            $"whoAmI={activeBossNpc.whoAmI}, realLife={activeBossNpc.realLife}. " +
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

        private static bool IsEaterOfWorlds(int npcType)
        {
            return npcType == NPCID.EaterofWorldsHead ||
                    npcType == NPCID.EaterofWorldsBody ||
                    npcType == NPCID.EaterofWorldsTail;
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

            // Config option to ignore unknown projectiles
            if (weaponName == "Unknown" && !Conf.C.TrackUnknownDamage)
            {
                // Log.Info("[SP] Ignoring unknown proj with name: " + proj.Name + ", damage: " + damageDone);
                return;
            }

            TrackBossDamage(weaponID, weaponName, damageDone, target);
        }

        #endregion

        #region Methods

        private void TrackBossDamage(int weaponID, string weaponName, int damageDone, NPC npc)
        {
            // Only proceed if there's an active fight or if tracking all entities is enabled.
            bool anyFightActive = normalFight != null || eaterFight != null || wormFight != null;
            if (!anyFightActive && !Conf.C.TrackAllEntities)
                return;

            // 1) Normal single‐NPC boss damage tracking.
            if (normalFight != null)
            {
                // Update weapon info and damage taken no matter what.
                normalFight.UpdateWeapon(weaponID, weaponName, damageDone);
                normalFight.damageTaken += damageDone;

                // Only update currentLife if the hit came from the primary boss NPC.
                if (npc.whoAmI == normalFight.whoAmI)
                {
                    if (npc.life <= 0)
                    {
                        normalFight.SendBossFightToPanel();
                        normalFight = null;
                        return;
                    }
                    else
                    {
                        normalFight.currentLife = npc.life;
                    }
                }
                else if (Conf.C.TrackAllEntities)
                {
                    // If tracking all entities, consider aggregating the life of all relevant NPCs instead.
                    // For now we simply leave currentLife unchanged, or you could call a recalculation method.
                    // normalFight.currentLife = RecalculateNormalBossLife();
                }
                normalFight.SendBossFightToPanel();
            }
            // 2) EoW segment tracking.
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
            // 3) Worm-like boss tracking.
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