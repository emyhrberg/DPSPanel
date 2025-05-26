using System.Collections.Generic;
using System.Linq;
using DPSPanel.UI;
using Terraria.ModLoader;

namespace DPSPanel.DamageCalculation.Classes
{/// <summary>
 /// Represents a fight with any dummy
 /// </summary>
    public class DummyFight
    {
        public int whoAmI;        // NPC index of the boss
        public string bossName;   // e.g., "Eye of Cthulhu"
        public int currentLife;
        public int damageTaken;

        public List<Weapon> weapons = [];

        public void UpdateWeapon(int weaponID, string weaponName, int damageDone)
        {
            Weapon existing = weapons.FirstOrDefault(w => w.weaponName == weaponName);
            if (existing == null)
            {
                var newWeapon = new Weapon(weaponID, weaponName, damageDone);
                weapons.Add(newWeapon);
                // Create the weapon bar once
                ModContent.GetInstance<MainSystem>().state.container.panel.CreateWeaponBarSP(weaponName);
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