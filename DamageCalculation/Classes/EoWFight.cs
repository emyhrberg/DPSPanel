using System.Collections.Generic;
using System.Linq;
using DPSPanel.UI;
using Terraria.ModLoader;

namespace DPSPanel.DamageCalculation.Classes
{
    /// <summary>
    /// Represents an Eater of Worlds fight, which is multi‐segment.
    /// We do not store whoAmI, because the worm has multiple NPCs.
    /// Instead, we sum them up each time from the known EoW segment types.
    /// </summary>
    public class EoWFight
    {
        public int damageTaken;
        public int totalLife;       // Current sum of all EoW segments
        public int totalLifeMax;    // Max sum of all EoW segments

        public List<Weapon> weapons = [];

        public void UpdateWeapon(int weaponID, string weaponName, int damageDone)
        {
            Weapon existing = weapons.FirstOrDefault(w => w.weaponName == weaponName);
            if (existing == null)
            {
                var newWeapon = new Weapon(weaponID, weaponName, damageDone);
                weapons.Add(newWeapon);
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