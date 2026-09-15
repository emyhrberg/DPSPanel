namespace DPSPanel.Common.DamageCalculation.Classes;

// Item IDs keep weapons distinct even when their display names are identical.
public sealed record Weapon(int weaponItemID, string weaponName, long damage);
