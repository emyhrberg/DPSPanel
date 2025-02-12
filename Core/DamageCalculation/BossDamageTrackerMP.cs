using System.Collections.Generic;
using System.Linq;
using DPSPanel.Core.Configs;
using DPSPanel.Core.Networking;
using DPSPanel.Helpers;
using DPSPanel.UI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DPSPanel.Core.DamageCalculation
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
                    // Check if we already reach max cap of weapons
                    Config c = ModContent.GetInstance<Config>();
                    if (weapons.Count >= c.MaxWeaponsDisplayed)
                    {
                        // Add to unknown weapon
                        Weapon unknownWeapon = weapons.FirstOrDefault(w => w.weaponName == "Unknown");
                        if (unknownWeapon != null)
                        {
                            unknownWeapon.damage += damageDone;
                        }
                        else
                        {
                            unknownWeapon = new Weapon(-1, "Unknown", damageDone);
                            weapons.Add(unknownWeapon);
                            weapons = weapons.OrderByDescending(w => w.damage).ToList();
                        }
                        return;
                    }

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

            public void UpdatePlayerDamage(string playerName, int damageDone)
            {
                PlayerFightData player = players.FirstOrDefault(p => p.playerName == playerName);
                if (player == null)
                {
                    player = new PlayerFightData(Main.LocalPlayer.whoAmI, playerName, damageDone);
                    players.Add(player);
                }
                else
                {
                    player.playerDamage += damageDone;
                }
            }

            public void PrintFightData(Mod mod)
            {
                if (players.Count == 0)
                    return;

                string playersDamages = string.Join(", ", players.Select(p => $"{p.playerName} ({p.playerDamage})"));
                // mod.Logger.Info($"Boss: {bossName} | Life: {currentLife}/{initialLife} | Damage Taken: {damageTaken} | Players: {playersDamages}");
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
                    // Mod.Logger.Info($"Boss {fight.bossName} was killed or despawned!");
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

            string weaponName = GetSourceWeaponName();
            int weaponID = GetSourceWeaponItemID();
            TrackBossDamage(weaponID, weaponName, damageDone, target);
        }
        #endregion

        #region Damage Tracking
        private void TrackBossDamage(int weaponID, string weaponName, int damageDone, NPC npc)
        {
            Config c = ModContent.GetInstance<Config>();

            // Debug log: report the call details.
            // Log.Info($"[BossFight] TrackBossDamage called with weaponID: {weaponID}, weaponName: {weaponName}, damageDone: {damageDone}, NPC: {npc.FullName}, boss flag: {npc.boss}, current life: {npc.life}");

            // No active fight? Then nothing to do.
            if (fight == null)
                return;

            // Determine whether this NPC is the designated boss.
            bool isDesignatedBoss = npc.boss && npc.FullName == fight.bossName;

            // If config is set to track all entities during a boss fight...
            if (c.TrackAllEntities)
            {
                // If this is the designated boss and it has died, end the fight.
                if (isDesignatedBoss && npc.life <= 0)
                {
                    // Log.Info("[BossFight] Designated boss has died. Ending fight.");
                    fight.isAlive = false;
                    PacketSender.SendPlayerDamagePacket(fight);
                    fight = null;
                    return;
                }

                // Regardless of NPC type, add the damage.
                fight.damageTaken += damageDone;
                // Log.Info($"[BossFight] Added {damageDone} damage. Total damage is now {fight.damageTaken}.");

                // Only update the boss’s current life if this NPC is the designated boss.
                if (isDesignatedBoss)
                {
                    fight.currentLife = npc.life;
                    // Log.Info($"[BossFight] Updated designated boss's current life to {npc.life}.");
                }

                // Update the player damage record and send out an update packet.
                fight.UpdatePlayerDamage(Main.LocalPlayer.name, damageDone);
                fight.UpdateWeapon(weaponID, weaponName, damageDone);
                PacketSender.SendPlayerDamagePacket(fight);
            }
            // Else, only track damage for the designated boss.
            else
            {
                // Ignore damage if this is not the designated boss.
                if (!isDesignatedBoss)
                    return;

                // If the designated boss dies, end the fight.
                if (npc.life <= 0)
                {
                    fight.isAlive = false;
                    PacketSender.SendPlayerDamagePacket(fight);
                    fight = null;
                    return;
                }

                // Ensure that the NPC hit is the one we're tracking.
                if (fight.whoAmI == npc.whoAmI)
                {
                    fight.damageTaken += damageDone;
                    fight.currentLife = npc.life;
                    fight.UpdatePlayerDamage(Main.LocalPlayer.name, damageDone);
                    fight.UpdateWeapon(weaponID, weaponName, damageDone);
                    // Log.Info($"[BossFight] Added {damageDone} damage to designated boss. Total damage is now {fight.damageTaken}.");
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
                Mod.Logger.Info("(MP) New boss fight created: " + fight.bossName);

                PacketSender.SendPlayerDamagePacket(fight);
            }
        }
        #endregion

        #region Helpers
        private string GetSourceWeaponName()
        {
            string sourceWeapon = GlobalProj.sourceWeapon?.Name;
            if (string.IsNullOrEmpty(sourceWeapon))
                sourceWeapon = "Unknown";
            return sourceWeapon;
        }

        private int GetSourceWeaponItemID()
        {
            int sourceWeaponID = GlobalProj.sourceWeapon?.type ?? -1; // -1 is an invalid item ID
            return sourceWeaponID;
        }

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
