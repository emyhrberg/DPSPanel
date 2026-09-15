using Terraria.DataStructures;

namespace DPSPanel.Common.DamageCalculation;

// Spawn identity is separate from whoAmI: Terraria reuses NPC array slots.
public sealed class EncounterNPC : GlobalNPC
{
    private static long nextSpawnId;
    public long SpawnId { get; private set; }
    public override bool InstancePerEntity => true;
    public override void OnSpawn(NPC npc, IEntitySource source) => SpawnId = ++nextSpawnId;
}
