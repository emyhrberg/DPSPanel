using System.Collections.Generic;
using DPSPanel.Common.Configs;
using DPSPanel.Common.DamageCalculation.Classes;
using DPSPanel.Networking;
using Terraria.ID;

namespace DPSPanel.Common.DamageCalculation;

/// <summary>Records only the local player's hits; shared by singleplayer and multiplayer.</summary>
public sealed class BossDamageTracker : ModPlayer
{
    private readonly Dictionary<int, Weapon> weapons = [];
    private long fightId;
    private int revision;
    private bool IsLocal => Main.netMode != NetmodeID.Server && Player.whoAmI == Main.myPlayer;

    public override void OnEnterWorld()
    {
        weapons.Clear();
        fightId = 0;
        revision = 0;
        if (IsLocal && Main.netMode == NetmodeID.MultiplayerClient)
            PacketSender.RequestSync();
    }

    public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) =>
        Track(item?.type ?? -1, item?.Name ?? "Unknown", target, damageDone);

    public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (!IsLocal || proj.owner != Player.whoAmI)
            return;
        var source = proj.GetGlobalProjectile<GlobalProj>();
        Track(source.WeaponId, source.WeaponName, target, damageDone);
    }

    private void Track(int itemId, string name, NPC target, int damage)
    {
        if (!IsLocal || damage <= 0 || (itemId < 0 && !Config.Conf.C.TrackUnknownDamage))
            return;
        var system = ModContent.GetInstance<EncounterSystem>();
        if (!system.CanTrack(target))
            return;
        if (fightId != system.State.Id)
        {
            weapons.Clear();
            fightId = system.State.Id;
            revision = 0;
        }
        weapons.TryGetValue(itemId, out var previous);
        if (previous == null && weapons.Count >= FightPacketCodec.MaxWeapons)
            return;
        weapons[itemId] = new Weapon(itemId, name, (previous?.damage ?? 0) + damage);
        var snapshot = new PlayerFightData(Player.whoAmI, Player.name, ++revision, weapons.Values);
        system.ReceiveSnapshot(fightId, snapshot);
        if (Main.netMode == NetmodeID.MultiplayerClient)
            PacketSender.Snapshot(fightId, snapshot);
    }
}
