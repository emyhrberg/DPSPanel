using System.Collections.Generic;
using System.Linq;
using DPSPanel.Common.Configs;
using DPSPanel.DamageCalculation.Classes;
using DPSPanel.Helpers;
using DPSPanel.Networking;
using DPSPanel.UI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static DPSPanel.Common.Configs.Config;

namespace DPSPanel.Common.DamageCalculation
{
    /// <summary>
    /// Multiplayer version of boss damage tracking that handles:
    /// 1) Normal single-NPC bosses
    /// 2) Eater of Worlds (custom logic)
    /// 3) Other worm-like bosses, by checking realLife
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class BossDamageTrackerMP : ModPlayer
    {
        #region Fight Classes
        /// <summary>
        /// For single‐NPC bosses (boss && realLife == -1).
        /// </summary>
        public class NormalBossFightMP
        {
            public int whoAmI;
            public string bossName;
            public int damageTaken;
            public int bossHeadId;
            public int initialLife;
            public int currentLife;

            public List<PlayerFightData> players;
            public List<Weapon> weapons;

            public NormalBossFightMP(int whoAmI, string bossName, int bossHeadId, int initialLife, int currentLife)
            {
                this.whoAmI = whoAmI;
                this.bossName = bossName;
                this.bossHeadId = bossHeadId;
                this.initialLife = initialLife;
                this.currentLife = currentLife;

                damageTaken = 0;
                players = new List<PlayerFightData>();
                weapons = new List<Weapon>();
            }

            public void UpdatePlayerDamage(string playerName, int playerWhoAmI, int damageDone)
            {
                var existingPlayer = players.FirstOrDefault(p => p.playerName == playerName);
                if (existingPlayer == null)
                {
                    existingPlayer = new PlayerFightData(playerWhoAmI, playerName, damageDone);
                    players.Add(existingPlayer);
                }
                else
                {
                    existingPlayer.playerDamage += damageDone;
                }
            }

            public void UpdateWeapon(int weaponID, string weaponName, int damageDone)
            {
                var weapon = weapons.FirstOrDefault(w => w.weaponName == weaponName);
                if (weapon == null)
                {
                    weapon = new Weapon(weaponID, weaponName, damageDone);
                    weapons.Add(weapon);
                }
                else
                {
                    weapon.damage += damageDone;
                }

                // Re‐sort weapons by damage
                weapons = weapons.OrderByDescending(w => w.damage).ToList();

                // Broadcast changes to all clients
                PacketSender.SendPlayerDamagePacket(this);
            }
        }

        /// <summary>
        /// For the Eater of Worlds only (multi‐segment by NPC type).
        /// We look for segments with types: EaterofWorldsHead, Body, Tail.
        /// </summary>
        public class EoWFightMP
        {
            public bool isAlive;
            public string bossName;
            public int damageTaken;
            public int totalLife;
            public int totalLifeMax;
            public int bossHeadId;

            public List<PlayerFightData> players;
            public List<Weapon> weapons;

            public EoWFightMP(string bossName, int bossHeadId)
            {
                this.isAlive = true;
                this.bossName = bossName;
                this.bossHeadId = bossHeadId;
                this.damageTaken = 0;
                this.totalLife = 0;
                this.totalLifeMax = 0;
                this.players = new List<PlayerFightData>();
                this.weapons = new List<Weapon>();
            }

            public void UpdatePlayerDamage(string playerName, int playerWhoAmI, int damageDone)
            {
                var existingPlayer = players.FirstOrDefault(p => p.playerName == playerName);
                if (existingPlayer == null)
                {
                    existingPlayer = new PlayerFightData(playerWhoAmI, playerName, damageDone);
                    players.Add(existingPlayer);
                }
                else
                {
                    existingPlayer.playerDamage += damageDone;
                }
            }

            public void UpdateWeapon(int weaponID, string weaponName, int damageDone)
            {
                var weapon = weapons.FirstOrDefault(w => w.weaponName == weaponName);
                if (weapon == null)
                {
                    weapon = new Weapon(weaponID, weaponName, damageDone);
                    weapons.Add(weapon);
                }
                else
                {
                    weapon.damage += damageDone;
                }

                weapons = weapons.OrderByDescending(w => w.damage).ToList();
                PacketSender.SendPlayerDamagePacket(this);
            }
        }

        /// <summary>
        /// For other worm‐like bosses (e.g., The Destroyer).
        /// We rely on npc.realLife to link segments to the "head."
        /// </summary>
        public class WormBossFightMP
        {
            public bool isAlive;
            public int headIndex;      // NPC.whoAmI of the worm head
            public string bossName;
            public int damageTaken;
            public int totalLife;
            public int totalLifeMax;
            public int bossHeadId;

            public List<PlayerFightData> players;
            public List<Weapon> weapons;

            public WormBossFightMP(int headIndex, string bossName, int bossHeadId)
            {
                this.isAlive = true;
                this.headIndex = headIndex;
                this.bossName = bossName;
                this.bossHeadId = bossHeadId;
                this.damageTaken = 0;
                this.totalLife = 0;
                this.totalLifeMax = 0;
                this.players = new List<PlayerFightData>();
                this.weapons = new List<Weapon>();
            }

            public void UpdatePlayerDamage(string playerName, int playerWhoAmI, int damageDone)
            {
                var existingPlayer = players.FirstOrDefault(p => p.playerName == playerName);
                if (existingPlayer == null)
                {
                    existingPlayer = new PlayerFightData(playerWhoAmI, playerName, damageDone);
                    players.Add(existingPlayer);
                }
                else
                {
                    existingPlayer.playerDamage += damageDone;
                }
            }

            public void UpdateWeapon(int weaponID, string weaponName, int damageDone)
            {
                var weapon = weapons.FirstOrDefault(w => w.weaponName == weaponName);
                if (weapon == null)
                {
                    weapon = new Weapon(weaponID, weaponName, damageDone);
                    weapons.Add(weapon);
                }
                else
                {
                    weapon.damage += damageDone;
                }

                weapons = weapons.OrderByDescending(w => w.damage).ToList();
                PacketSender.SendPlayerDamagePacket(this);
            }
        }

        #endregion

        #region Fields

        private NormalBossFightMP normalFight;
        private EoWFightMP eaterFight;
        private WormBossFightMP wormFight;

        #endregion

        #region Hooks

        public override void PreUpdate()
        {
            // Only do client logic in MP
            if (Main.netMode != NetmodeID.MultiplayerClient)
                return;

            // --------------------------------------------------------------------------------
            // 1) Check if existing fights are still alive and end them if they're fully dead.
            // --------------------------------------------------------------------------------

            // Eater of Worlds
            if (eaterFight != null)
            {
                RecalculateEoWLife();
                if (eaterFight.totalLife <= 0)
                {
                    eaterFight.isAlive = false;
                    PacketSender.SendPlayerDamagePacket(eaterFight);
                    eaterFight = null;
                }
            }

            // Worm-like boss
            if (wormFight != null)
            {
                RecalculateWormLife();
                if (wormFight.totalLife <= 0)
                {
                    wormFight.isAlive = false;
                    PacketSender.SendPlayerDamagePacket(wormFight);
                    wormFight = null;
                }
            }

            // Normal single‐NPC boss
            if (normalFight != null)
            {
                NPC bossNpc = Main.npc[normalFight.whoAmI];
                if (!bossNpc.active || bossNpc.life <= 0)
                {
                    // Boss is dead or inactive
                    normalFight = null;
                }
                else
                {
                    // Update current life
                    normalFight.currentLife = bossNpc.life;
                }
            }

            // --------------------------------------------------------------------------------
            // 2) If we have no active fights, look for a NEW one to start.
            // --------------------------------------------------------------------------------

            // Normal single-NPC boss detection
            if (normalFight == null)
            {
                NPC normalBoss = Main.npc.FirstOrDefault(n =>
                    n.active && n.boss && n.realLife == -1);
                if (normalBoss != null)
                {
                    normalFight = new NormalBossFightMP(
                        whoAmI: normalBoss.whoAmI,
                        bossName: normalBoss.FullName,
                        bossHeadId: normalBoss.GetBossHeadTextureIndex(),
                        initialLife: normalBoss.lifeMax,
                        currentLife: normalBoss.life
                    );

                    Log.Info("[MP] Created new normal boss fight: " + normalFight.bossName);
                    PacketSender.SendPlayerDamagePacket(normalFight);
                }
            }

            // Eater of Worlds detection
            if (eaterFight == null)
            {
                bool foundEoW = false;
                for (int i = 0; i < Main.npc.Length; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && IsEaterOfWorlds(npc.type))
                    {
                        eaterFight = new EoWFightMP("Eater of Worlds", npc.GetBossHeadTextureIndex());
                        foundEoW = true;
                        PacketSender.SendPlayerDamagePacket(eaterFight);
                        break;
                    }
                }
            }

            // Other worm-like boss detection
            if (wormFight == null)
            {
                // Find the head of a worm-like boss (boss && realLife != -1 && not EoW)
                NPC wormBossSegment = Main.npc.FirstOrDefault(n =>
                    n.active && n.boss && n.realLife != -1 && !IsEaterOfWorlds(n.type));

                if (wormBossSegment != null)
                {
                    NPC headNpc = Main.npc[wormBossSegment.realLife];
                    if (headNpc.active && headNpc.boss)
                    {
                        wormFight = new WormBossFightMP(
                            headIndex: wormBossSegment.realLife,
                            bossName: headNpc.FullName,
                            bossHeadId: headNpc.GetBossHeadTextureIndex()
                        );
                        PacketSender.SendPlayerDamagePacket(wormFight);
                    }
                }
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                return;

            int weaponID = item?.type ?? -1;
            string weaponName = item?.Name ?? "UnknownItem";
            TrackBossDamage(weaponID, weaponName, damageDone, target);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                return;

            // Check GlobalProjectile data to identify the source weapon
            GlobalProj gProj = proj.GetGlobalProjectile<GlobalProj>();
            int weaponID = -1;
            string weaponName = "Unknown";

            if (gProj != null && gProj.sourceWeapon != null)
            {
                weaponID = gProj.sourceWeapon.type;
                weaponName = gProj.sourceWeapon.Name;
            }

            // Config option to ignore truly unknown projectiles
            if (weaponName == "Unknown" && !Conf.C.TrackUnknownDamage)
            {
                // Log.Info("[MP] Ignoring unknown projectile damage from proj: " + proj.Name);
                return;
            }

            TrackBossDamage(weaponID, weaponName, damageDone, target);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Main entry point for tracking boss damage. 
        /// Respects Conf.C.TrackAllEntities: 
        /// - If true, track damage on ANY NPC while a boss fight is active.
        /// - If false, only track damage if the NPC matches the current boss or its segments.
        /// </summary>
        private void TrackBossDamage(int weaponID, string weaponName, int damageDone, NPC npc)
        {
            bool anyFightActive = (normalFight != null || eaterFight != null || wormFight != null);
            if (!anyFightActive)
                return;

            Player localPlayer = Main.LocalPlayer;

            // ---------------------------------------------------------------------
            // Normal single‐NPC boss damage
            // ---------------------------------------------------------------------
            if (normalFight != null)
            {
                // If trackAllEntities is ON, record any hit.
                // Otherwise, only record if npc.whoAmI == normalFight.whoAmI.
                if (Conf.C.TrackAllEntities || npc.whoAmI == normalFight.whoAmI)
                {
                    normalFight.UpdateWeapon(weaponID, weaponName, damageDone);
                    normalFight.UpdatePlayerDamage(localPlayer.name, localPlayer.whoAmI, damageDone);
                    normalFight.damageTaken += damageDone;

                    // Check if that was the main boss
                    if (npc.whoAmI == normalFight.whoAmI && npc.life <= 0)
                    {
                        normalFight = null;
                        return;
                    }
                }
            }

            // ---------------------------------------------------------------------
            // Eater of Worlds damage
            // ---------------------------------------------------------------------
            if (eaterFight != null && eaterFight.isAlive)
            {
                // If trackAllEntities is ON, record any hit.
                // Otherwise, only record if this NPC is an EoW segment.
                if (Conf.C.TrackAllEntities || IsEaterOfWorlds(npc.type))
                {
                    eaterFight.UpdateWeapon(weaponID, weaponName, damageDone);
                    eaterFight.UpdatePlayerDamage(localPlayer.name, localPlayer.whoAmI, damageDone);
                    eaterFight.damageTaken += damageDone;

                    // Recalculate total life to see if fight ends
                    RecalculateEoWLife();
                    if (eaterFight.totalLife <= 0)
                    {
                        eaterFight.isAlive = false;
                        eaterFight = null;
                        return;
                    }
                }
            }

            // ---------------------------------------------------------------------
            // Worm‐like boss damage
            // ---------------------------------------------------------------------
            if (wormFight != null && wormFight.isAlive)
            {
                // If trackAllEntities is ON, record any hit.
                // Otherwise, only record if npc.realLife == wormFight.headIndex.
                if (Conf.C.TrackAllEntities || npc.realLife == wormFight.headIndex)
                {
                    wormFight.UpdateWeapon(weaponID, weaponName, damageDone);
                    wormFight.UpdatePlayerDamage(localPlayer.name, localPlayer.whoAmI, damageDone);
                    wormFight.damageTaken += damageDone;

                    // Recalculate total life to see if fight ends
                    RecalculateWormLife();
                    if (wormFight.totalLife <= 0)
                    {
                        wormFight.isAlive = false;
                        wormFight = null;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the given type belongs to Eater of Worlds (head, body, tail).
        /// </summary>
        private bool IsEaterOfWorlds(int npcType)
        {
            return npcType == NPCID.EaterofWorldsHead ||
                   npcType == NPCID.EaterofWorldsBody ||
                   npcType == NPCID.EaterofWorldsTail;
        }

        private void RecalculateEoWLife()
        {
            if (eaterFight == null)
                return;

            int total = 0;
            int totalMax = 0;

            for (int i = 0; i < Main.npc.Length; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && IsEaterOfWorlds(npc.type))
                {
                    total += npc.life;
                    totalMax += npc.lifeMax;
                }
            }

            eaterFight.totalLife = total;
            eaterFight.totalLifeMax = totalMax;
        }

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
                if (npc.active && npc.realLife == headIndex)
                {
                    total += npc.life;
                    totalMax += npc.lifeMax;
                }
            }

            wormFight.totalLife = total;
            wormFight.totalLifeMax = totalMax;
        }

        #endregion
    }
}
