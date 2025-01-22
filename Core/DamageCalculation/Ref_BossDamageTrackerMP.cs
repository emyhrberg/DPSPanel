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
//         //====================================================
//         // Nested Classes
//         //====================================================

//         /// <summary>
//         /// Represents an ongoing boss fight.
//         /// </summary>
//         public class Fight
//         {
//             public int initialLife;
//             public string bossName;
//             public int damageTaken;
//             public HashSet<CustomPlayer> players = new();

//             /// <summary>
//             /// Initialize Fight fields based on the given boss NPC.
//             /// </summary>
//             /// <param name="npc">The boss NPC to track.</param>
//             /// <param name="mod">Reference to the calling mod for logging.</param>
//             public static Fight CreateNewFight(NPC npc, Mod mod)
//             {
//                 if (npc == null) {
//                     mod.Logger.Warn("CreateNewFight called with null NPC. Returning null.");
//                     return null;
//                 }

//                 var fight = new Fight {
//                     initialLife = npc.lifeMax,
//                     bossName = npc.FullName,
//                     damageTaken = 0,
//                     players = new HashSet<CustomPlayer>()
//                 };

//                 mod.Logger.Info($"[Fight] Created new fight for boss '{fight.bossName}' (Max HP: {fight.initialLife}).");
//                 return fight;
//             }

//             /// <summary>
//             /// Retrieves an existing player entry or creates a new one if none exist.
//             /// </summary>
//             public CustomPlayer GetOrAddPlayer(string playerName, Mod mod)
//             {
//                 if (string.IsNullOrEmpty(playerName)) {
//                     mod.Logger.Warn($"GetOrAddPlayer called with invalid playerName: '{playerName}'");
//                     return null;
//                 }

//                 // Attempt to find an existing player
//                 var existingPlayer = players.FirstOrDefault(p => p.playerName == playerName);
//                 if (existingPlayer == null)
//                 {
//                     existingPlayer = new CustomPlayer(playerName);
//                     players.Add(existingPlayer);

//                     mod.Logger.Info($"[Fight] Added new player '{playerName}' to fight against '{bossName}'.");
//                 }

//                 return existingPlayer;
//             }
//         }

//         /// <summary>
//         /// Represents a player participating in a fight.
//         /// </summary>
//         public class CustomPlayer
//         {
//             public string playerName;
//             public int playerDamage;
//             public HashSet<Weapon> weapons = new();

//             public CustomPlayer(string name)
//             {
//                 playerName = name;
//                 playerDamage = 0;
//             }

//             /// <summary>
//             /// Gets or creates a new weapon entry for this player.
//             /// </summary>
//             public Weapon GetOrAddWeapon(string weaponName, Mod mod)
//             {
//                 if (string.IsNullOrEmpty(weaponName)) {
//                     mod.Logger.Warn($"GetOrAddWeapon called with invalid weaponName: '{weaponName}'");
//                     return null;
//                 }

//                 var existingWeapon = weapons.FirstOrDefault(w => w.weaponName == weaponName);
//                 if (existingWeapon == null)
//                 {
//                     existingWeapon = new Weapon(weaponName);
//                     weapons.Add(existingWeapon);

//                     mod.Logger.Info($"[CustomPlayer] Player '{playerName}' added weapon '{weaponName}'.");
//                 }

//                 return existingWeapon;
//             }
//         }

//         /// <summary>
//         /// Represents a weapon used by a player.
//         /// </summary>
//         public class Weapon
//         {
//             public string weaponName;
//             public int weaponDamage;

//             public Weapon(string name)
//             {
//                 weaponName = name;
//                 weaponDamage = 0;
//             }
//         }

//         //====================================================
//         // Fields
//         //====================================================
//         private Fight fight = null;

//         //====================================================
//         // Hooks
//         //====================================================
//         public override void PreUpdate()
//         {
//             // Check once per second (60 ticks)
//             if (Main.time % 60 == 0)
//             {
//                 // 1) Check if the current fight is over
//                 if (fight != null)
//                 {
//                     bool bossStillAlive = Main.npc.Any(npc =>
//                         npc.active && npc.boss && npc.FullName == fight.bossName);

//                     if (!bossStillAlive)
//                     {
//                         Mod.Logger.Info($"Boss '{fight.bossName}' despawned or died. Ending fight.");
//                         fight = null;
//                     }
//                 }

//                 // 2) Start a new fight if no fight is ongoing and a boss is present
//                 if (fight == null)
//                 {
//                     foreach (NPC npc in Main.npc)
//                     {
//                         if (npc.active && npc.boss && npc.life > 0)
//                         {
//                             fight = Fight.CreateNewFight(npc, Mod);
//                             break; // We found a boss; no need to keep checking
//                         }
//                     }
//                 }
//             }
//         }

//         public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
//         {
//             // Item-based (melee) damage
//             string weaponName = item?.Name ?? "Unknown Item";
//             UpdateFight(weaponName, target, damageDone);
//         }

//         public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
//         {
//             // Projectile-based (ranged/magic/other) damage
//             // If you want to retrieve the actual weapon used to fire it,
//             // you'd get it from a stored GlobalProjectile, etc.
//             string weaponName = proj?.Name ?? "Unknown Projectile";
//             UpdateFight(weaponName, target, damageDone);
//         }

//         //====================================================
//         // Main Logic
//         //====================================================
//         private void UpdateFight(string weaponName, NPC npc, int damageDone)
//         {
//             if (npc == null)
//             {
//                 Mod.Logger.Warn("UpdateFight called with null NPC; aborting.");
//                 return;
//             }

//             if (!npc.boss)
//             {
//                 Mod.Logger.Debug($"UpdateFight ignored: NPC '{npc.FullName}' is not a boss.");
//                 return;
//             }

//             // If there's no active fight, create one
//             if (fight == null)
//             {
//                 fight = Fight.CreateNewFight(npc, Mod);
//                 if (fight == null)
//                 {
//                     // If we failed to create a fight, nothing more to do
//                     return;
//                 }
//             }

//             // Get or add the local player
//             var playerName = Main.LocalPlayer.name;
//             var customPlayer = fight.GetOrAddPlayer(playerName, Mod);
//             if (customPlayer == null)
//             {
//                 Mod.Logger.Warn("UpdateFight could not retrieve or create CustomPlayer; aborting.");
//                 return;
//             }

//             // Get or add the weapon
//             var weapon = customPlayer.GetOrAddWeapon(weaponName, Mod);
//             if (weapon == null)
//             {
//                 Mod.Logger.Warn("UpdateFight could not retrieve or create weapon; aborting.");
//                 return;
//             }

//             // Increment damage counters
//             weapon.weaponDamage += damageDone;
//             customPlayer.playerDamage += damageDone;
//             fight.damageTaken += damageDone;

//             // Optionally log the new damage totals
//             Mod.Logger.Info($"[UpdateFight] '{playerName}' dealt {damageDone} damage with '{weaponName}' " +
//                             $"to boss '{fight.bossName}'. Total weapon dmg: {weapon.weaponDamage}, " +
//                             $"player dmg: {customPlayer.playerDamage}, fight dmg: {fight.damageTaken}.");

//             // Update UI
//             UpdateUI(npc);
//         }

//         //====================================================
//         // UI
//         //====================================================
//         private void UpdateUI(NPC npc)
//         {
//             if (fight == null || npc == null)
//                 return;

//             PanelSystem sys = ModContent.GetInstance<PanelSystem>();
//             if (sys?.state?.container?.panel == null || sys.state.container.bossIcon == null)
//             {
//                 Mod.Logger.Warn("[UpdateUI] PanelSystem or UI references are null, cannot update UI.");
//                 return;
//             }

//             sys.state.container.panel.ClearPanelAndAllItems();
//             sys.state.container.panel.SetBossTitle(fight.bossName, null);
//             sys.state.container.bossIcon.UpdateBossIcon(npc);
//         }
//     }
// }
