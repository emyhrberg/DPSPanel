using System.Collections.Generic;
using System.Linq;
using DPSPanel.Common.Configs;
using DPSPanel.Helpers;
using DPSPanel.Networking;
using DPSPanel.UI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DPSPanel.Common.DamageCalculation
{
    [Autoload(Side = ModSide.Client)]
    public class BossDamageTrackerMP : ModPlayer
    {
        #region Classes
        public class BossFight(int bossHeadId, int currentLife, int whoAmI, int initialLife, string bossName, int damageTaken, bool isAlive, List<PlayerFightData> players, List<Weapon> weapons)
        {
            public int bossHeadId = bossHeadId;
            public int currentLife = currentLife;
            public int whoAmI = whoAmI;
            public int initialLife = initialLife;
            public string bossName = bossName;
            public int damageTaken = damageTaken;
            public bool isAlive = isAlive;
            public List<PlayerFightData> players = players;
            public List<Weapon> weapons = weapons;

            public void UpdateWeapon(int weaponID, string weaponName, int damageDone)
            {
                // Check if the weapon already exists
                Weapon weapon = weapons.FirstOrDefault(w => w.weaponName == weaponName);
                if (weapon == null)
                {
                    // Add a new weapon to the fight
                    weapon = new Weapon(weaponID, weaponName, damageDone);
                    weapons.Add(weapon);
                    weapons = weapons.OrderByDescending(w => w.damage).ToList();
                }
                else
                {
                    // Update existing weapon's damage
                    weapon.damage += damageDone;
                }

                // send packet after updating weapon
                PacketSender.SendPlayerDamagePacket(this);
            }

            public void UpdatePlayerDamage(string playerName, int playerWhoAmI, int damageDone)
            {
                // Look for an existing record using playerName.
                // (You might also want to compare playerWhoAmI in a real scenario.)
                PlayerFightData player = players.FirstOrDefault(p => p.playerName == playerName);
                if (player == null)
                {
                    player = new PlayerFightData(playerWhoAmI, playerName, damageDone);
                    players.Add(player);
                    // // Log.Info($"[BossDamageTrackerMP.UpdatePlayerDamage] Added new player '{playerName}' (ID: {playerWhoAmI}) with initial damage: {damageDone}. Total players now: {players.Count}");
                }
                else
                {
                    player.playerDamage += damageDone;
                    // // Log.Info($"[BossDamageTrackerMP.UpdatePlayerDamage] Updated player '{playerName}' (ID: {playerWhoAmI}) damage to: {player.playerDamage}");
                }
            }

            public void PrintFightData(Mod mod)
            {
                if (players.Count == 0)
                    return;

                string playersDamages = string.Join(", ", players.Select(p => $"{p.playerName} ({p.playerDamage})"));
                // // Log.info($"Boss: {bossName} | Life: {currentLife}/{initialLife} | Damage Taken: {damageTaken} | Players: {playersDamages}");
            }
        }
        #endregion

        private BossFight fight;

        #region Hooks
        public override void PreUpdate()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                return;

            // Check once per second
            int frequencyCheck = 60;

            if (Main.time % frequencyCheck == 0)
            {
                // 1) If there's an active fight and the boss is no longer alive or present, stop tracking
                if (fight != null && !Main.npc.Any(npc => npc.active && npc.boss && npc.FullName == fight.bossName))
                {
                    // // Log.info($"Boss {fight.bossName} was killed or despawned!");
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
                        // Log.info($"Boss fight {fight.bossName} created!");
                    }
                }
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                return;

            // Pass the actual item type (ID) explicitly for melee
            int actualItemID = item?.type ?? -1;
            string actualItemName = item?.Name ?? "unknownitem";
            TrackBossDamage(actualItemID, actualItemName, damageDone, target);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
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

            TrackBossDamage(weaponID, weaponName, damageDone, target);
        }
        #endregion

        #region Damage Tracking
        private void TrackBossDamage(int weaponID, string weaponName, int damageDone, NPC npc)
        {
            Config c = ModContent.GetInstance<Config>();
            MainSystem sys = ModContent.GetInstance<MainSystem>();

            // No active fight? Then nothing to do.
            if (fight == null)
                return;

            bool isDesignatedBoss = npc.boss && npc.FullName == fight.bossName;

            if (c.TrackAllEntities)
            {
                if (isDesignatedBoss && npc.life <= 0)
                {
                    fight.isAlive = false;
                    PacketSender.SendPlayerDamagePacket(fight);
                    fight = null;

                    sys.state.container.panel.CurrentBossAlive = false;
                    // Log.Info("Boss fight ended.");
                    return;
                }

                sys.state.container.panel.CurrentBossAlive = true;

                fight.damageTaken += damageDone;
                if (isDesignatedBoss)
                {
                    fight.currentLife = npc.life;
                }

                // Use the actual playerWhoAmI (from Main.LocalPlayer here is your local client,
                // but on the server you must pass the appropriate value) rather than always Main.LocalPlayer.
                fight.UpdatePlayerDamage(Main.LocalPlayer.name, Main.LocalPlayer.whoAmI, damageDone);
                fight.UpdateWeapon(weaponID, weaponName, damageDone);
                PacketSender.SendPlayerDamagePacket(fight);
            }
            else
            {
                if (!isDesignatedBoss)
                    return;

                if (npc.life <= 0)
                {
                    fight.isAlive = false;
                    PacketSender.SendPlayerDamagePacket(fight);
                    fight = null;
                    sys.state.container.panel.CurrentBossAlive = false;
                    // Log.Info("Boss fight ended.");
                    return;
                }

                if (fight.whoAmI == npc.whoAmI)
                {
                    sys.state.container.panel.CurrentBossAlive = true;
                    fight.damageTaken += damageDone;
                    fight.currentLife = npc.life;
                    fight.UpdatePlayerDamage(Main.LocalPlayer.name, Main.LocalPlayer.whoAmI, damageDone);
                    fight.UpdateWeapon(weaponID, weaponName, damageDone);
                    PacketSender.SendPlayerDamagePacket(fight);
                }
            }
        }

        private void CreateNewBossFight(NPC npc)
        {
            if (fight == null)
            {
                fight = new BossFight(
                    npc.GetBossHeadTextureIndex(),
                    npc.life,
                    npc.whoAmI,
                    npc.lifeMax,
                    npc.FullName,
                    0,
                    true,
                    new List<PlayerFightData>(),
                    new List<Weapon>()
                );

                PacketSender.SendPlayerDamagePacket(fight);
            }
        }
        #endregion

        #region Helpers

        private bool IsValidBoss(NPC npc)
        {
            return npc.boss && !npc.friendly;
        }

        private bool IgnoreGolem(NPC npc)
        {
            return npc.type == NPCID.Golem || npc.type == NPCID.GolemFistLeft || npc.type == NPCID.GolemFistRight;
        }
        #endregion
    }
}
