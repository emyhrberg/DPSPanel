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
    /// 1) Normal single‐NPC bosses
    /// 2) Eater of Worlds (custom logic)
    /// 3) Other worm‐like bosses, by checking realLife
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class BossDamageTrackerMP : ModPlayer
    {
        #region Fight Classes

        /// <summary>
        /// For single‐NPC bosses (boss && realLife == -1).
        /// </summary>
        public class NormalBossFightMP(int whoAmI, string bossName, int bossHeadId, int initialLife, int currentLife)
        {
            public int whoAmI = whoAmI;
            public string bossName = bossName;
            public int damageTaken = 0;
            public int bossHeadId = bossHeadId;
            public int initialLife = initialLife; // Initial life of the boss
            public int currentLife = currentLife; // Current life of the boss
            public List<PlayerFightData> players = [];
            public List<Weapon> weapons = [];

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
            public string bossName;  // "Eater of Worlds"
            public int damageTaken;
            public int totalLife;
            public int totalLifeMax;
            public int bossHeadId;

            // Track which players have dealt damage and total weapon damage
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
            public string bossName;    // e.g., "The Destroyer"
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

            // Same approach for EoW
            if (eaterFight != null)
            {
                // Recompute total HP
                RecalculateEoWLife();
                // If it’s fully gone, clear
                if (eaterFight.totalLife <= 0)
                {
                    eaterFight.isAlive = false;
                    PacketSender.SendPlayerDamagePacket(eaterFight);
                    eaterFight = null;
                }
            }

            // Same approach for wormFight
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

            // 2) Now, see if we need to create a NEW fight (only if we don’t already have one)
            //    For example, check normalFight first:
            if (normalFight == null)
            {
                // Are there any active single-NPC bosses we can target?
                NPC normalBoss = Main.npc
                    .FirstOrDefault(n => n.active && n.boss && n.realLife == -1);

                if (normalBoss != null)
                {
                    // Found one! Create a new fight object and send it to the server
                    normalFight = new NormalBossFightMP(
                        normalBoss.whoAmI,
                        normalBoss.FullName,
                        normalBoss.life,
                        normalBoss.lifeMax,
                        normalBoss.GetBossHeadTextureIndex()
                    );
                    Log.Info("[MP] Created new normal boss fight2: " + normalFight.bossName);
                    PacketSender.SendPlayerDamagePacket(normalFight);
                }
            }

            // 3) If no normalFight, then check for EoW if we don’t already have one:
            if (eaterFight == null)
            {
                bool foundEoWSegment = false;
                for (int i = 0; i < Main.npc.Length; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && IsEaterOfWorlds(npc.type))
                    {
                        foundEoWSegment = true;
                        eaterFight = new EoWFightMP("Eater of Worlds", npc.GetBossHeadTextureIndex());
                        PacketSender.SendPlayerDamagePacket(eaterFight);
                        break; // only create once
                    }
                }
            }

            // 4) If still no EoW, see if there’s an active worm boss (other than EoW)
            if (wormFight == null)
            {
                // Find the head of a worm-like boss (boss && realLife != -1 && not EoW)
                NPC wormBossSegment = Main.npc.FirstOrDefault(n =>
                    n.active && n.boss && n.realLife != -1 && !IsEaterOfWorlds(n.type));

                if (wormBossSegment != null)
                {
                    NPC headNpc = Main.npc[wormBossSegment.realLife];
                    if (headNpc.boss)
                    {
                        wormFight = new WormBossFightMP(
                            wormBossSegment.realLife,
                            headNpc.FullName,
                            headNpc.GetBossHeadTextureIndex()
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
                Log.Info("[MP] Ignoring unknown projectile damage from proj: " + proj.Name);
                return;
            }

            TrackBossDamage(weaponID, weaponName, damageDone, target);
        }

        #endregion

        #region Internal Methods

        private void TrackBossDamage(int weaponID, string weaponName, int damageDone, NPC npc)
        {
            // Only proceed if there's an active fight or if tracking all entities is enabled.
            bool anyFightActive = normalFight != null || eaterFight != null || wormFight != null;
            if (!anyFightActive)
                return;

            // 1) Normal single‐NPC boss damage tracking.
            if (normalFight != null)
            {
                // Track damage if TrackAllEntities is on, or if this NPC is the current boss.
                if (Conf.C.TrackAllEntities || npc.whoAmI == normalFight.whoAmI)
                {
                    Player player = Main.LocalPlayer;
                    normalFight.UpdateWeapon(weaponID, weaponName, damageDone);
                    normalFight.UpdatePlayerDamage(player.name, player.whoAmI, damageDone);
                    normalFight.damageTaken += damageDone;
                }

                // If this is the main boss, update its health or end the fight.
                if (npc.whoAmI == normalFight.whoAmI)
                {
                    if (npc.life <= 0)
                    {
                        normalFight = null;
                        return;
                    }
                    else
                    {
                        normalFight.currentLife = npc.life;
                    }
                }
            }
            // 2) EoW segment tracking.
            else if (eaterFight != null && IsEaterOfWorlds(npc.type))
            {
                eaterFight.UpdateWeapon(weaponID, weaponName, damageDone);
                eaterFight.damageTaken += damageDone;

                RecalculateEoWLife();
                if (eaterFight.totalLife <= 0)
                {
                    eaterFight.UpdateWeapon(weaponID, weaponName, damageDone);
                    eaterFight = null;
                }
                else
                {
                    eaterFight.UpdateWeapon(weaponID, weaponName, damageDone);
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
                    wormFight.UpdateWeapon(weaponID, weaponName, damageDone);
                    wormFight = null;
                }
                else
                {
                    wormFight.UpdateWeapon(weaponID, weaponName, damageDone);
                }
            }
        }

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

                if (npc.realLife == headIndex)
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
