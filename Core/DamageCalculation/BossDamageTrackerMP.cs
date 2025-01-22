// using System;
// using System.Collections.Generic;
// using System.Linq;
// using DPSPanel.Core.Helpers;
// using DPSPanel.Core.Panel;
// using Terraria;
// using Terraria.DataStructures;
// using Terraria.ModLoader;

// namespace DPSPanel.Core.DamageCalculation
// {
//     public class BossDamageTrackerMP : ModPlayer
//     {
//         #region Classes
//         internal class FightData
//         {
//             internal int initialLife;
//             internal string bossName;
//             internal int damageTaken;
//             internal bool isAlive;
//             internal HashSet<PlayerData> players = [];
//         }

//         internal class PlayerData
//         {
//             internal string playerName;
//             internal int playerDamage;
//             internal HashSet<WeaponData> weapons = [];
//         }

//         internal class WeaponData
//         {
//             internal string weaponName;
//             internal int weaponDamage;
//         }
//         #endregion

//         private FightData fight = null;

//         #region Hooks
//         public override void PreUpdate()
//         {
//             // // Iterate all NPCs every 1 second (idk how computationally heavy this is)
//             // if (Main.time % 60 == 0)
//             // {
//             //     // If there's an active fight and the boss is no longer alive or present, stop tracking
//             //     if (fight != null && !Main.npc.Any(npc => npc.active && npc.boss && npc.FullName == fight.bossName))
//             //     {
//             //         Mod.Logger.Info($"Boss {fight.bossName} despawned!");
//             //         fight = null; // Stop tracking the fight
//             //     }

//             //     // Start a fight if a boss spawns
//             //     foreach (NPC npc in Main.npc)
//             //         if (fight == null && npc.active && npc.boss && npc.life > 0)
//             //             fight = CreateNewBossFight(npc);
//             // }
//         }

//         public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
//         {
//             string sourceWeapon = GlobalProj.sourceWeapon?.Name;
//             UpdateFight(sourceWeapon, target, damageDone);
//         }
//         public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
//         {
//             // get source weapon
//             // ranged damage
//             string sourceWeapon = GlobalProj.sourceWeapon?.Name;
//             if (string.IsNullOrEmpty(sourceWeapon) && proj != null) {
//                 sourceWeapon = proj.Name;
//             }
//             UpdateFight(sourceWeapon, target, damageDone);
//         }
//         #endregion

//         #region Damage Calculation
//         private void UpdateFight(string sourceWeapon, NPC npc, int damageDone)
//         {
//             // Ensure NPC is a boss and alive
//             if (npc == null || !npc.active || !npc.boss || npc.life <= 0)
//                 return;

//             // Start a new fight if there isn't one
//             fight ??= new FightData
//             {
//                 initialLife = npc.lifeMax,
//                 bossName = npc.FullName,
//                 damageTaken = 0,
//                 isAlive = true
//             };

//             // Add player to fight if they're not already in it
//             PlayerData player = fight.players.FirstOrDefault(p => p.playerName == Main.LocalPlayer.name);
//             if (player == null)
//             {
//                 player = new PlayerData
//                 {
//                     playerName = Main.LocalPlayer.name,
//                     playerDamage = damageDone
//                 };
//                 fight.players.Add(player);
//             }

//             // Update damage
//             player.playerDamage += damageDone;

//             // Print fight data
//             PrintFightData();
//         }
//         #endregion

//         #region UI
//         private void UpdateUI(NPC npc)
//         {
//             PanelSystem sys = ModContent.GetInstance<PanelSystem>();
//             sys.state.container.panel.ClearPanelAndAllItems();
//             sys.state.container.panel.SetBossTitle(fight.bossName, null);
//             sys.state.container.bossIcon.UpdateBossIcon(npc);
//         }
//         #endregion 

//         #region Print
//         private void PrintFightData()
//         {
//             Mod.Logger.Info($"---------------{fight.bossName}-------------");
//             foreach (PlayerData player in fight.players)
//             {
//                 Mod.Logger.Info($"Player: {player.playerName} ({player.playerDamage})");
//             }
//         }
//         #endregion 
//     }
// }
