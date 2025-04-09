using System.Collections.Generic;
using System.Linq;
using DPSPanel.UI;
using Terraria.ModLoader;

namespace DPSPanel.DamageCalculation.Classes
{
    /// <summary>
    /// Represents a general multi‐segment worm‐like boss fight (e.g., The Destroyer),
    /// tracked by realLife. The "headIndex" is the whoAmI of the head segment.
    /// </summary>
    public class WormBossFight
    {
        public int headIndex;     // NPC.whoAmI of the worm's head
        public string bossName;   // The name of the worm boss (e.g., "The Destroyer")

        public int totalLife;
        public int totalLifeMax;
        public int damageTaken;

        public List<Weapon> weapons = new List<Weapon>();

        public WormBossFight(int headIndex, string bossName)
        {
            this.headIndex = headIndex;
            this.bossName = bossName;
        }

        public void UpdateWeapon(int weaponID, string weaponName, int damageDone)
        {
            Weapon existing = weapons.FirstOrDefault(w => w.weaponName == weaponName);
            if (existing == null)
            {
                var newWeapon = new Weapon(weaponID, weaponName, damageDone);
                weapons.Add(newWeapon);
                MainSystem sys = ModContent.GetInstance<MainSystem>();
                sys.state.container.panel.CreateWeaponBarSP(weaponName);
            }
            else
            {
                existing.damage += damageDone;
            }
        }

        public void SendBossFightToPanel()
        {
            weapons = weapons.OrderByDescending(w => w.damage).ToList();
            MainSystem sys = ModContent.GetInstance<MainSystem>();
            sys.state.container.panel.UpdateAllWeaponBarsSP(weapons);
        }
    }
}