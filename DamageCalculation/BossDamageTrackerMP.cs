using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria.ID;
using static DPSPanel.DPSPanel;
using DPSPanel.Helpers;
using DPSPanel.UI;
using DPSPanel.Configs;

namespace DPSPanel.DamageCalculation
{
    [Autoload(Side = ModSide.Client)]
    public class BossDamageTrackerMP : ModPlayer
    {
        #region Classes
        public class BossFight
        {
            public int bossHeadId = -1;
            public int currentLife;
            public int whoAmI;
            public int initialLife;
            public string bossName;
            public int damageTaken;
            public bool isAlive = false;
            public List<PlayerFightData> players;
            public List<Weapon> weapons = [];

            public void UpdatePlayerDamage(string playerName, int damageDone)
            {
                PlayerFightData player = players.FirstOrDefault(p => p.playerName == playerName);
                if (player == null)
                {
                    player = new PlayerFightData
                    {
                        playerName = playerName,
                        playerDamage = damageDone
                    };
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
        public override void OnEnterWorld()
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;

            Config c = ModContent.GetInstance<Config>();
            if (!c.ShowPopupMessage)
                return;

            Main.NewText(
                "[DPSPanel] Hello, " + Main.LocalPlayer.name +
                "! To use DPSPanel, type /dps toggle in chat or toggle with K (set the keybind in Settings -> Controls).",
                Color.SkyBlue
            );
        }

        public override void PreUpdate()
        {
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
                            if (IgnoreGolem(npc))
                                continue;
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
            if (IgnoreGolem(target))
                return;

            // Pass the actual item type (ID) explicitly for melee
            int actualItemID = item?.type ?? -1;
            string actualItemName = item?.Name ?? "unknownitem";
            TrackBossDamage(actualItemID, actualItemName, damageDone, target);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (IgnoreGolem(target))
                return;

            string weaponName = GetSourceWeaponName();
            int weaponID = GetSourceWeaponItemID();
            TrackBossDamage(weaponID, weaponName, damageDone, target);
        }
        #endregion

        #region Damage Tracking
        private void TrackBossDamage(int weaponID, string weaponName, int damageDone, NPC npc)
        {
            if (IsValidBoss(npc))
            {
                if (fight != null && npc.life <= 0)
                {
                    fight.isAlive = false;
                    SendPlayerDamagePacket();
                    fight = null;
                    return;
                }

                // Mod.Logger.Info($"whoAmI: {npc.whoAmI} | fight BOSS ID: {fight.bossId}");

                if (fight != null && fight.whoAmI == npc.whoAmI)
                {
                    // Update the fight data
                    fight.damageTaken += damageDone;
                    fight.currentLife = npc.life;
                    fight.UpdatePlayerDamage(Main.LocalPlayer.name, damageDone);
                    SendPlayerDamagePacket();
                }
            }
        }

        private void CreateNewBossFight(NPC npc)
        {
            if (fight == null)
            {
                fight = new BossFight
                {
                    bossHeadId = npc.GetBossHeadTextureIndex(),
                    whoAmI = npc.whoAmI,
                    currentLife = npc.life,
                    initialLife = npc.lifeMax,
                    bossName = npc.FullName,
                    damageTaken = 0,
                    players = [],
                    isAlive = true
                };
                // Mod.Logger.Info("New boss fight created: " + fight.bossName);

                SendPlayerDamagePacket();
            }
        }
        #endregion

        #region Networking
        private void SendPlayerDamagePacket()
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;

            // Get variables to send
            string player = Main.LocalPlayer.name;
            int damageDone = fight.players.FirstOrDefault(p => p.playerName == Main.LocalPlayer.name)?.playerDamage ?? 0;

            // Create and send the packet
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)ModMessageType.FightPacket);
            packet.Write(player);
            packet.Write(damageDone);
            packet.Write(fight.whoAmI);
            packet.Write(fight.bossName);
            packet.Write(fight.bossHeadId);
            packet.Write(Main.LocalPlayer.whoAmI);

            ModContent.GetInstance<DPSPanel>().Logger.Info($"[Client] Sent player: {player} | dmg: {damageDone} | BossWhoAmI: {fight.whoAmI} | BossName: {fight.bossName} | BossHeadID: {fight.bossHeadId} | LocalPlayerWhoAmI: {Main.LocalPlayer.whoAmI}");
            packet.Send(); // send the packet to the server
        }

        #endregion

        #region Helpers
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
