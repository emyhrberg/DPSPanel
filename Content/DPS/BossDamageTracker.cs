using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using DPSPanel.UI.DPS;
using System.Linq;
using Terraria.GameContent.UI.Elements;

namespace DPSPanel.Content.DPS
{
    public class BossDamageTracker : ModPlayer
    {
        // Struct for tracking player damage data
        public struct PlayerDamageData
        {
            public string PlayerName; // Player's name
            public Dictionary<string, int> DamageSources; // Damage sources (e.g., items, projectiles)

            public PlayerDamageData(string playerName)
            {
                PlayerName = playerName;
                DamageSources = new Dictionary<string, int>();
            }

            public void AddDamage(string damageSource, int damage)
            {
                if (!DamageSources.ContainsKey(damageSource))
                {
                    DamageSources[damageSource] = 0;
                }
                DamageSources[damageSource] += damage;
            }

            public string GenerateLog()
            {
                return string.Join("\n", DamageSources.Select(ds => $"- {ds.Key}: {ds.Value} damage"));
            }
        }

        // Struct for tracking boss fight data
        public struct BossFightData
        {
            public string BossName; // Name of the boss
            public Dictionary<string, PlayerDamageData> Players; // Players involved in the fight

            public BossFightData(string bossName)
            {
                BossName = bossName;
                Players = new Dictionary<string, PlayerDamageData>();
            }

            public void AddPlayerDamage(string playerName, string damageSource, int damage)
            {
                if (!Players.ContainsKey(playerName))
                {
                    Players[playerName] = new PlayerDamageData(playerName);
                }
                Players[playerName].AddDamage(damageSource, damage);
            }

            public string GenerateLog()
            {
                var logs = new List<string> { $"[c/78ffa3:{BossName}]" };
                foreach (var player in Players.Values)
                {
                    logs.Add($"[c/ffffff:{player.PlayerName}]");
                    logs.Add(player.GenerateLog());
                }
                return string.Join("\n", logs);
            }
        }

        // Dictionary to track active bosses and their fight data
        private Dictionary<string, BossFightData> bosses = new();

        private void TrackBossDamage(NPC target, string damageSource, int damage)
        {
            // Only track damage for bosses
            if (!target.boss || target.friendly)
                return;

            // Use the boss's name and `whoAmI` to create a unique key for each fight
            string bossKey = $"{target.FullName} (ID: {target.whoAmI})";

            // Initialize a new boss fight if not already tracked
            if (!bosses.ContainsKey(bossKey))
            {
                bosses[bossKey] = new BossFightData(target.FullName);
                AddBossToPanel(bossKey); // Add a new boss row to the panel
            }

            // Add damage to the boss fight data
            bosses[bossKey].AddPlayerDamage(Main.LocalPlayer.name, damageSource, damage);

            // Update the panel with the latest damage data
            UpdatePanel(bossKey);
        }

        private void AddBossToPanel(string bossKey)
        {
            // Add a new boss header to the panel only once
            var panelSystem = ModContent.GetInstance<DPSPanelSystem>();
            if (!panelSystem.state.dpsPanel.bossEntries.ContainsKey(bossKey))
            {
                panelSystem.state.dpsPanel.AddItem($"[c/78ffa3:{bossKey}]");
            }
        }

        private void UpdatePanel(string bossKey)
        {
            var panelSystem = ModContent.GetInstance<DPSPanelSystem>();
            var panel = panelSystem.state.dpsPanel;

            // Clear existing entries for this boss in the panel
            panel.ClearItemsForBoss(bossKey);

            // Fetch the boss data
            var bossData = bosses[bossKey];

            // Add the updated data for this boss
            foreach (var playerEntry in bossData.Players)
            {
                string playerName = playerEntry.Key;
                var playerData = playerEntry.Value;

                // Add damage sources for each player
                foreach (var weapon in playerData.DamageSources)
                {
                    string log = $"{playerName} - {weapon.Key} - {weapon.Value} damage";
                    panel.AddItemForBoss(bossKey, log);
                }
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TrackBossDamage(target, item.Name, damageDone);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TrackBossDamage(target, proj.Name, damageDone);
        }
    }
}
