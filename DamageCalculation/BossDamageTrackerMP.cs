// using System.Collections.Generic;
// using System.Linq;
// using DPSPanel.Common.Configs;
// using DPSPanel.DamageCalculation.Classes;
// using DPSPanel.Helpers;
// using DPSPanel.Networking;
// using DPSPanel.UI;
// using Terraria;
// using Terraria.ID;
// using Terraria.ModLoader;
// using static DPSPanel.Common.Configs.Config;

// namespace DPSPanel.Common.DamageCalculation
// {
//     /// <summary>
//     /// Multiplayer version of boss damage tracking that handles:
//     /// 1) Normal single‐NPC bosses
//     /// 2) Eater of Worlds (custom logic)
//     /// 3) Other worm‐like bosses, by checking realLife
//     /// </summary>
//     [Autoload(Side = ModSide.Client)]
//     public class BossDamageTrackerMP : ModPlayer
//     {
//         #region Fight Classes

//         /// <summary>
//         /// For single‐NPC bosses (boss && realLife == -1).
//         /// </summary>
//         public class NormalBossFightMP
//         {
//             public bool isAlive;
//             public int whoAmI;
//             public string bossName;
//             public int currentLife;
//             public int initialLife;
//             public int damageTaken;
//             public int bossHeadId;
//             public List<PlayerFightData> players;
//             public List<Weapon> weapons;

//             public NormalBossFightMP(int whoAmI, string bossName, int currentLife, int initialLife, int bossHeadId)
//             {
//                 this.whoAmI = whoAmI;
//                 this.bossName = bossName;
//                 this.currentLife = currentLife;
//                 this.initialLife = initialLife;
//                 this.bossHeadId = bossHeadId;
//                 this.isAlive = true;
//                 this.damageTaken = 0;
//                 this.players = new List<PlayerFightData>();
//                 this.weapons = new List<Weapon>();
//             }

//             public void UpdatePlayerDamage(string playerName, int playerWhoAmI, int damageDone)
//             {
//                 var existingPlayer = players.FirstOrDefault(p => p.playerName == playerName);
//                 if (existingPlayer == null)
//                 {
//                     existingPlayer = new PlayerFightData(playerWhoAmI, playerName, damageDone);
//                     players.Add(existingPlayer);
//                 }
//                 else
//                 {
//                     existingPlayer.playerDamage += damageDone;
//                 }
//             }

//             public void UpdateWeapon(int weaponID, string weaponName, int damageDone)
//             {
//                 var weapon = weapons.FirstOrDefault(w => w.weaponName == weaponName);
//                 if (weapon == null)
//                 {
//                     weapon = new Weapon(weaponID, weaponName, damageDone);
//                     weapons.Add(weapon);
//                 }
//                 else
//                 {
//                     weapon.damage += damageDone;
//                 }

//                 // Re‐sort weapons by damage
//                 weapons = weapons.OrderByDescending(w => w.damage).ToList();

//                 // Broadcast changes to all clients
//                 PacketSender.SendPlayerDamagePacket(this);
//             }
//         }

//         /// <summary>
//         /// For the Eater of Worlds only (multi‐segment by NPC type).
//         /// We look for segments with types: EaterofWorldsHead, Body, Tail.
//         /// </summary>
//         public class EoWFightMP
//         {
//             public bool isAlive;
//             public string bossName;  // "Eater of Worlds"
//             public int damageTaken;
//             public int totalLife;
//             public int totalLifeMax;
//             public int bossHeadId;

//             // Track which players have dealt damage and total weapon damage
//             public List<PlayerFightData> players;
//             public List<Weapon> weapons;

//             public EoWFightMP(string bossName, int bossHeadId)
//             {
//                 this.isAlive = true;
//                 this.bossName = bossName;
//                 this.bossHeadId = bossHeadId;
//                 this.damageTaken = 0;
//                 this.totalLife = 0;
//                 this.totalLifeMax = 0;
//                 this.players = new List<PlayerFightData>();
//                 this.weapons = new List<Weapon>();
//             }

//             public void UpdatePlayerDamage(string playerName, int playerWhoAmI, int damageDone)
//             {
//                 var existingPlayer = players.FirstOrDefault(p => p.playerName == playerName);
//                 if (existingPlayer == null)
//                 {
//                     existingPlayer = new PlayerFightData(playerWhoAmI, playerName, damageDone);
//                     players.Add(existingPlayer);
//                 }
//                 else
//                 {
//                     existingPlayer.playerDamage += damageDone;
//                 }
//             }

//             public void UpdateWeapon(int weaponID, string weaponName, int damageDone)
//             {
//                 var weapon = weapons.FirstOrDefault(w => w.weaponName == weaponName);
//                 if (weapon == null)
//                 {
//                     weapon = new Weapon(weaponID, weaponName, damageDone);
//                     weapons.Add(weapon);
//                 }
//                 else
//                 {
//                     weapon.damage += damageDone;
//                 }

//                 weapons = weapons.OrderByDescending(w => w.damage).ToList();
//                 PacketSender.SendPlayerDamagePacket(this);
//             }
//         }

//         /// <summary>
//         /// For other worm‐like bosses (e.g., The Destroyer).
//         /// We rely on npc.realLife to link segments to the "head."
//         /// </summary>
//         public class WormBossFightMP
//         {
//             public bool isAlive;
//             public int headIndex;      // NPC.whoAmI of the worm head
//             public string bossName;    // e.g., "The Destroyer"
//             public int damageTaken;
//             public int totalLife;
//             public int totalLifeMax;
//             public int bossHeadId;

//             public List<PlayerFightData> players;
//             public List<Weapon> weapons;

//             public WormBossFightMP(int headIndex, string bossName, int bossHeadId)
//             {
//                 this.isAlive = true;
//                 this.headIndex = headIndex;
//                 this.bossName = bossName;
//                 this.bossHeadId = bossHeadId;
//                 this.damageTaken = 0;
//                 this.totalLife = 0;
//                 this.totalLifeMax = 0;
//                 this.players = new List<PlayerFightData>();
//                 this.weapons = new List<Weapon>();
//             }

//             public void UpdatePlayerDamage(string playerName, int playerWhoAmI, int damageDone)
//             {
//                 var existingPlayer = players.FirstOrDefault(p => p.playerName == playerName);
//                 if (existingPlayer == null)
//                 {
//                     existingPlayer = new PlayerFightData(playerWhoAmI, playerName, damageDone);
//                     players.Add(existingPlayer);
//                 }
//                 else
//                 {
//                     existingPlayer.playerDamage += damageDone;
//                 }
//             }

//             public void UpdateWeapon(int weaponID, string weaponName, int damageDone)
//             {
//                 var weapon = weapons.FirstOrDefault(w => w.weaponName == weaponName);
//                 if (weapon == null)
//                 {
//                     weapon = new Weapon(weaponID, weaponName, damageDone);
//                     weapons.Add(weapon);
//                 }
//                 else
//                 {
//                     weapon.damage += damageDone;
//                 }

//                 weapons = weapons.OrderByDescending(w => w.damage).ToList();
//                 PacketSender.SendPlayerDamagePacket(this);
//             }
//         }

//         #endregion

//         #region Fields

//         private NormalBossFightMP normalFight;
//         private EoWFightMP eaterFight;
//         private WormBossFightMP wormFight;

//         #endregion

//         #region Hooks

//         public override void PreUpdate()
//         {
//             // Only care about MP client logic (remove if you also want server‐side logic)
//             if (Main.netMode != NetmodeID.MultiplayerClient)
//                 return;

//             // Check once per second
//             if (Main.time % 60 == 0)
//             {
//                 bool normalBossFound = false;
//                 bool eaterFound = false;
//                 bool otherWormFound = false;

//                 for (int i = 0; i < Main.npc.Length; i++)
//                 {
//                     NPC npc = Main.npc[i];
//                     if (!npc.active)
//                         continue;

//                     // 1) Normal single‐NPC boss (boss && realLife == -1)
//                     if (npc.boss && npc.realLife == -1)
//                     {
//                         normalBossFound = true;
//                         // If we don't have a normalFight yet, create one
//                         if (normalFight == null)
//                         {
//                             normalFight = new NormalBossFightMP(
//                                 npc.whoAmI,
//                                 npc.FullName,
//                                 npc.life,
//                                 npc.lifeMax,
//                                 npc.GetBossHeadTextureIndex()
//                             );
//                             // Minimal log
//                             // Mod.Logger.Info($"[MP] Detected normal boss: {npc.FullName} (whoAmI={npc.whoAmI}).");
//                             PacketSender.SendPlayerDamagePacket(normalFight);
//                         }
//                     }

//                     // 2) Check EoW segments specifically (by type)
//                     bool isEoW = (npc.type == NPCID.EaterofWorldsHead ||
//                                   npc.type == NPCID.EaterofWorldsBody ||
//                                   npc.type == NPCID.EaterofWorldsTail);
//                     if (isEoW)
//                     {
//                         eaterFound = true;
//                         // If we don't have an EoW fight yet, create it
//                         if (eaterFight == null)
//                         {
//                             eaterFight = new EoWFightMP("Eater of Worlds", npc.GetBossHeadTextureIndex());
//                             // Minimal log
//                             // Mod.Logger.Info($"[MP] Detected EoW segment: whoAmI={npc.whoAmI}, realLife={npc.realLife}.");
//                             PacketSender.SendPlayerDamagePacket(eaterFight);
//                         }
//                     }
//                     // 3) Check for other worm‐like boss (boss, realLife != -1, not EoW)
//                     else if (npc.realLife != -1)
//                     {
//                         NPC headNpc = Main.npc[npc.realLife];
//                         // If the head is a boss, and not EoW
//                         if (headNpc.boss && !IsEaterOfWorlds(headNpc.type))
//                         {
//                             otherWormFound = true;
//                             if (wormFight == null)
//                             {
//                                 wormFight = new WormBossFightMP(
//                                     npc.realLife,
//                                     headNpc.FullName,
//                                     headNpc.GetBossHeadTextureIndex()
//                                 );
//                                 // Minimal log
//                                 // Mod.Logger.Info($"[MP] Detected worm boss segment: whoAmI={npc.whoAmI}, realLife={npc.realLife}.");
//                                 PacketSender.SendPlayerDamagePacket(wormFight);
//                             }
//                         }
//                     }
//                 }

//                 // Check if normal boss is gone
//                 if (!normalBossFound && normalFight != null)
//                 {
//                     // normal boss ended
//                     normalFight.isAlive = false;
//                     PacketSender.SendPlayerDamagePacket(normalFight);
//                     normalFight = null;
//                 }
//                 else if (normalFight != null)
//                 {
//                     // If the normalFight is still active, ensure the boss hasn't died
//                     NPC bossNpc = Main.npc[normalFight.whoAmI];
//                     if (!bossNpc.active || bossNpc.life <= 0)
//                     {
//                         normalFight.isAlive = false;
//                         PacketSender.SendPlayerDamagePacket(normalFight);
//                         normalFight = null;
//                     }
//                     else
//                     {
//                         normalFight.currentLife = bossNpc.life;
//                     }
//                 }

//                 // Check if EoW is gone
//                 if (!eaterFound && eaterFight != null)
//                 {
//                     eaterFight.isAlive = false;
//                     PacketSender.SendPlayerDamagePacket(eaterFight);
//                     eaterFight = null;
//                 }
//                 else if (eaterFound && eaterFight != null)
//                 {
//                     RecalculateEoWLife();
//                     if (eaterFight.totalLife <= 0)
//                     {
//                         eaterFight.isAlive = false;
//                         PacketSender.SendPlayerDamagePacket(eaterFight);
//                         eaterFight = null;
//                     }
//                 }

//                 // Check if other worm is gone
//                 if (!otherWormFound && wormFight != null)
//                 {
//                     wormFight.isAlive = false;
//                     PacketSender.SendPlayerDamagePacket(wormFight);
//                     wormFight = null;
//                 }
//                 else if (otherWormFound && wormFight != null)
//                 {
//                     RecalculateWormLife();
//                     if (wormFight.totalLife <= 0)
//                     {
//                         wormFight.isAlive = false;
//                         PacketSender.SendPlayerDamagePacket(wormFight);
//                         wormFight = null;
//                     }
//                 }
//             }
//         }

//         public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
//         {
//             if (Main.netMode != NetmodeID.MultiplayerClient)
//                 return;

//             int weaponID = item?.type ?? -1;
//             string weaponName = item?.Name ?? "UnknownItem";
//             TrackBossDamage(weaponID, weaponName, damageDone, target);
//         }

//         public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
//         {
//             if (Main.netMode != NetmodeID.MultiplayerClient)
//                 return;

//             // If your GlobalProjectile stored the original weapon:
//             GlobalProj gProj = proj.GetGlobalProjectile<GlobalProj>();
//             int weaponID = -1;
//             string weaponName = "Unknown";
//             if (gProj != null && gProj.sourceWeapon != null)
//             {
//                 weaponID = gProj.sourceWeapon.type;
//                 weaponName = gProj.sourceWeapon.Name;
//             }

//             // Config option to ignore unknown projectiles
//             if (weaponName == "Unknown" && !Conf.C.TrackUnknownDamage)
//             {
//                 Log.Info("[MP] Ignoring unknown projectile damage from proj: " + proj.Name);
//                 return;
//             }

//             TrackBossDamage(weaponID, weaponName, damageDone, target);
//         }

//         #endregion

//         #region Internal Methods

//         private void TrackBossDamage(int weaponID, string weaponName, int damageDone, NPC npc)
//         {
//             // If no fights are active, do nothing
//             bool anyFightActive = (normalFight != null || eaterFight != null || wormFight != null);
//             if (!anyFightActive)
//                 return;

//             Config c = ModContent.GetInstance<Config>();

//             // Normal fight
//             if (normalFight != null && npc.whoAmI == normalFight.whoAmI)
//             {
//                 // If user wants to track all entities or specifically the designated boss
//                 if (!c.TrackAllEntities && !npc.boss)
//                     return;

//                 normalFight.damageTaken += damageDone;
//                 normalFight.currentLife = npc.life;
//                 normalFight.UpdatePlayerDamage(Main.LocalPlayer.name, Main.LocalPlayer.whoAmI, damageDone);
//                 normalFight.UpdateWeapon(weaponID, weaponName, damageDone);

//                 // If boss died
//                 if (npc.life <= 0)
//                 {
//                     normalFight.isAlive = false;
//                     PacketSender.SendPlayerDamagePacket(normalFight);
//                     normalFight = null;
//                 }
//                 return;
//             }

//             // Eater of Worlds fight (EoW)
//             if (eaterFight != null && IsEaterOfWorlds(npc.type))
//             {
//                 eaterFight.damageTaken += damageDone;
//                 eaterFight.UpdatePlayerDamage(Main.LocalPlayer.name, Main.LocalPlayer.whoAmI, damageDone);
//                 eaterFight.UpdateWeapon(weaponID, weaponName, damageDone);

//                 RecalculateEoWLife();
//                 if (eaterFight.totalLife <= 0)
//                 {
//                     eaterFight.isAlive = false;
//                     PacketSender.SendPlayerDamagePacket(eaterFight);
//                     eaterFight = null;
//                 }
//                 return;
//             }

//             // Other worm
//             if (wormFight != null && npc.realLife == wormFight.headIndex)
//             {
//                 wormFight.damageTaken += damageDone;
//                 wormFight.UpdatePlayerDamage(Main.LocalPlayer.name, Main.LocalPlayer.whoAmI, damageDone);
//                 wormFight.UpdateWeapon(weaponID, weaponName, damageDone);

//                 RecalculateWormLife();
//                 if (wormFight.totalLife <= 0)
//                 {
//                     wormFight.isAlive = false;
//                     PacketSender.SendPlayerDamagePacket(wormFight);
//                     wormFight = null;
//                 }
//             }
//         }

//         private bool IsEaterOfWorlds(int npcType)
//         {
//             return npcType == NPCID.EaterofWorldsHead ||
//                    npcType == NPCID.EaterofWorldsBody ||
//                    npcType == NPCID.EaterofWorldsTail;
//         }

//         private void RecalculateEoWLife()
//         {
//             if (eaterFight == null)
//                 return;

//             int total = 0;
//             int totalMax = 0;

//             for (int i = 0; i < Main.npc.Length; i++)
//             {
//                 NPC npc = Main.npc[i];
//                 if (!npc.active)
//                     continue;

//                 if (IsEaterOfWorlds(npc.type))
//                 {
//                     total += npc.life;
//                     totalMax += npc.lifeMax;
//                 }
//             }

//             eaterFight.totalLife = total;
//             eaterFight.totalLifeMax = totalMax;
//         }

//         private void RecalculateWormLife()
//         {
//             if (wormFight == null)
//                 return;

//             int total = 0;
//             int totalMax = 0;
//             int headIndex = wormFight.headIndex;

//             for (int i = 0; i < Main.npc.Length; i++)
//             {
//                 NPC npc = Main.npc[i];
//                 if (!npc.active)
//                     continue;

//                 if (npc.realLife == headIndex)
//                 {
//                     total += npc.life;
//                     totalMax += npc.lifeMax;
//                 }
//             }

//             wormFight.totalLife = total;
//             wormFight.totalLifeMax = totalMax;
//         }

//         #endregion
//     }
// }
