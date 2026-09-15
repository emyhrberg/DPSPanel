using System.Collections.Generic;
using System.Linq;

namespace DPSPanel.Common.DamageCalculation.Classes;

public sealed class PlayerFightData
{
    public int PlayerId { get; }
    public string Name { get; }
    public int Revision { get; }
    public IReadOnlyList<Weapon> Weapons { get; }
    public long Damage => Weapons.Sum(w => w.damage);

    public PlayerFightData(int playerId, string name, int revision, IEnumerable<Weapon> weapons)
    {
        PlayerId = playerId;
        Name = name;
        Revision = revision;
        Weapons = weapons.ToArray();
    }
}
