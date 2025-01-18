using Terraria.ModLoader;

namespace DPSPanel.MainCode.Panel
{
    public class BossDamageTrackerMP : ModPlayer
    {
        //        // --------------------------------------------------------------------------------
        //        // Classes
        //        // --------------------------------------------------------------------------------

        //        public class BossFight
        //        {
        //            public int bossId;            // Incremental ID (0, 1, 2, ...)
        //            public int initialLife;
        //            public string bossName;
        //            public int damageTaken;
        //            public HashSet<CustomPlayer> players = new();
        //        }

        //        public class CustomPlayer
        //        {
        //            public string playerName;
        //            public int totalDamage;
        //            public HashSet<Weapon> weapons = new();
        //        }

        //        public class Weapon
        //        {
        //            public string weaponName;
        //            public int damage;
        //        }

        //        // --------------------------------------------------------------------------------
        //        // Fields
        //        // --------------------------------------------------------------------------------
        //        private Dictionary<int, BossFight> fights = new();
        //        private int fightId = 0;
        //        private Weapon lastHitWeapon;

        //        // --------------------------------------------------------------------------------
        //        // Hooks
        //        // --------------------------------------------------------------------------------
        //        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        //        {
        //            TrackBossDamage(item.Name, damageDone, target);
        //        }

        //        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        //        {
        //            TrackBossDamage(proj.Name, damageDone, target);
        //        }

        //        // --------------------------------------------------------------------------------
        //        // Main Logic
        //        // --------------------------------------------------------------------------------
        //        private void TrackBossDamage(string weaponName, int damageDone, NPC npc)
        //        {
        //            if (!IsValidBoss(npc) || (IsNPCDead(npc)))
        //                return;

        //            if (!fights.TryGetValue(fightId, out var fight))
        //            {
        //                fight = CreateNewBossFight(npc, damageDone, weaponName);
        //                fights[fightId] = fight;
        //            }
        //            else
        //            {
        //                UpdateBossFight(fight, weaponName, damageDone);
        //            }

        //            LogBossFights();
        //        }

        //        private BossFight CreateNewBossFight(NPC npc, int damageDone, string weaponName)
        //        {
        //            return new BossFight
        //            {
        //                bossId = fightId,
        //                bossName = npc.FullName,
        //                initialLife = npc.lifeMax,
        //                damageTaken = damageDone,
        //                players =
        //                [
        //                    new CustomPlayer
        //                    {
        //                        playerName = Main.LocalPlayer.name,
        //                        totalDamage = damageDone,
        //                        weapons =
        //                        [
        //                            new Weapon
        //                            {
        //                                weaponName = weaponName,
        //                                damage = damageDone
        //                            }
        //                        ]
        //                    }
        //                ]
        //            };
        //        }

        //        private void UpdateBossFight(BossFight fight, string weaponName, int damageDone)
        //        {
        //            fight.damageTaken += damageDone;

        //            // Check if player is already in the fight
        //            var player = fight.players.FirstOrDefault(p => p.playerName == Main.LocalPlayer.name);
        //            if (player == null)
        //            {
        //                player = new CustomPlayer
        //                {
        //                    playerName = Main.LocalPlayer.name,
        //                    totalDamage = damageDone,
        //                    weapons =
        //                    [
        //                        new Weapon
        //                        {
        //                            weaponName = weaponName,
        //                            damage = damageDone
        //                        }
        //                    ]
        //                };
        //                fight.players.Add(player);
        //            }
        //            else
        //            {
        //                player.totalDamage += damageDone;

        //                // Check if weapon is already in the fight
        //                var weapon = player.weapons.FirstOrDefault(w => w.weaponName == weaponName);
        //                if (weapon == null)
        //                {
        //                    player.weapons.Add(new Weapon
        //                    {
        //                        weaponName = weaponName,
        //                        damage = damageDone
        //                    });
        //                }
        //                else
        //                {
        //                    weapon.damage += damageDone;
        //                }
        //            }
        //            // For final blow fix
        //            lastHitWeapon = new Weapon { weaponName = weaponName, damage = damageDone };
        //        }

        //        // --------------------------------------------------------------------------------
        //        // Utility Methods
        //        // --------------------------------------------------------------------------------
        //        private bool IsNPCDead(NPC npc)
        //        {
        //            if (npc.life <= 0)
        //            {
        //                FixFinalBlowDiscrepancy();
        //                fightId++;
        //                return true;
        //            }
        //            return false;
        //        }

        //        private bool IsValidBoss(NPC npc)
        //        {
        //            return npc.boss && !npc.friendly;
        //        }

        //        public void LogBossFights()
        //        {
        //            if (fights.Count == 0)
        //            {
        //                ModContent.GetInstance<DPSPanel>().Logger.Info("No boss fights have been tracked yet.");
        //                return;
        //            }

        //            // Print latest fight
        //            ILog logger = ModContent.GetInstance<DPSPanel>().Logger;

        //            // Check if fightid exists in the dictionary
        //            if (!fights.ContainsKey(fightId))
        //            {
        //                // Get the latest fight ID
        //                fightId = fights.Keys.Max();
        //                logger.Info($"Took the latest fight ID instead: {fightId} ");
        //            }

        //            var fight = fights[fightId];
        //            logger.Info($"{fight.bossName} ID {fight.bossId}");
        //            foreach (var player in fight.players)
        //            {
        //                logger.Info($"{player.playerName} ({player.totalDamage})");
        //            }

        //            //foreach (var fight in fights.Values)
        //            //{
        //            //    ModContent.GetInstance<DPSPanel>().Logger.Info($"{fight.bossName} ID {fight.bossId}");
        //            //    foreach (var player in fight.players)
        //            //    {
        //            //        ModContent.GetInstance<DPSPanel>().Logger.Info($"{player.playerName} ({player.totalDamage})");
        //            //    }
        //            //}

        //        }

        //        private void FixFinalBlowDiscrepancy()
        //        {
        //            if (!fights.TryGetValue(fightId, out var currFight))
        //                return; // No active fight to fix

        //            int discrepancy = currFight.damageTaken - currFight.initialLife;
        //            if (discrepancy == 0)
        //                return; // Damage matches boss life; no fix needed

        //            // Correct the total damage for the boss fight
        //            currFight.damageTaken -= discrepancy;

        //            // Adjust the last hitter's damage based on weaponName
        //            if (lastHitWeapon != null)
        //            {
        //                // Find the player who used a weapon with the same name as the lastHitWeapon
        //                var lastHitter = currFight.players.FirstOrDefault(p =>
        //                    p.weapons.Any(w => w.weaponName == lastHitWeapon.weaponName));

        //                if (lastHitter != null)
        //                {
        //                    // Now find that weapon object
        //                    var weapon = lastHitter.weapons.FirstOrDefault(w => w.weaponName == lastHitWeapon.weaponName);
        //                    if (weapon != null)
        //                    {
        //                        // Adjust weapon damage
        //                        weapon.damage -= discrepancy;
        //                        weapon.damage = Math.Max(0, weapon.damage); // Prevent negative values

        //                        // Adjust player's total damage
        //                        lastHitter.totalDamage -= discrepancy;
        //                        lastHitter.totalDamage = Math.Max(0, lastHitter.totalDamage); // Prevent negative values
        //                    }
        //                }
        //            }

        //            // Update the dictionary entry for the boss fight
        //            fights[fightId] = currFight;

        //            // Log the updated damage
        //            string playersDamageSummary = string.Join(", ", currFight.players
        //                .Select(p => $"{p.playerName}: {p.weapons.Sum(w => w.damage)}"));

        //            ModContent.GetInstance<DPSPanel>().Logger.Info(
        //                $"[Boss Fight {fightId} - {currFight.bossName}] " +
        //                $"Adjusted Damage: {currFight.damageTaken} | Players: {playersDamageSummary}"
        //            );
        //        }
    }
}
