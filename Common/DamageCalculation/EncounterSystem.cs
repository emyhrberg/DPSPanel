using System.Collections.Generic;
using System.Linq;
using DPSPanel.Common.Configs;
using DPSPanel.Common.DamageCalculation.Classes;
using DPSPanel.Networking;
using DPSPanel.UI;
using Terraria.ID;

namespace DPSPanel.Common.DamageCalculation;

/// <summary>The server (or singleplayer world) owns encounter boundaries.</summary>
public sealed class EncounterSystem : ModSystem
{
    public FightState State { get; } = new();
    public FightContext Current { get; private set; }
    private long nextId;
    private HashSet<long> bossInstances = [];
    private static MainSystem UI => Main.dedServ ? null : ModContent.GetInstance<MainSystem>();

    public override void OnWorldLoad() => Reset();
    public override void OnWorldUnload() => Reset();
    public override void PreUpdatePlayers()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            RefreshBosses();
    }

    private void Reset()
    {
        Current = null;
        nextId = 0;
        bossInstances.Clear();
        State.Reset();
        UI?.PresentEncounter(true);
    }

    public override void PostUpdateNPCs()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        RefreshBosses();
        if (Main.netMode == NetmodeID.Server)
        {
            foreach (int id in State.Players.Keys.Where(id => !Main.player[id].active).ToArray())
            {
                State.RemovePlayer(id);
                PacketSender.RemovePlayer(State.Id, id);
            }
        }
    }

    public void RefreshBosses()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        var bosses = Main.npc.Where(n => n.active && n.life > 0 && IsBossPart(n)).ToArray();
        if (bosses.Length == 0)
        {
            if (Current is { IsAlive: true, IsDummy: false })
                SetContext(Current with { IsAlive = false });
            bossInstances.Clear();
            return;
        }

        var instances = bosses.Select(n => n.GetGlobalNPC<EncounterNPC>().SpawnId).ToHashSet();
        if (Current == null || Current.IsDummy || !Current.IsAlive || !bossInstances.Overlaps(instances))
        {
            NPC boss = bosses.FirstOrDefault(n => n.boss) ?? bosses[0];
            SetContext(new FightContext(++nextId, IsEater(boss.type) ? Lang.GetNPCNameValue(NPCID.EaterofWorldsHead) : boss.FullName,
                boss.GetBossHeadTextureIndex(), false, true));
        }
        bossInstances = instances;
    }

    public static bool IsEater(int type) => type is NPCID.EaterofWorldsHead or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsTail;

    public static bool IsBossPart(NPC npc) => npc.boss || IsEater(npc.type) ||
        (npc.realLife >= 0 && npc.realLife < Main.maxNPCs && Main.npc[npc.realLife].boss);

    public bool CanTrack(NPC target)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            // Keep the current context for the killing hit, even after NPC deactivation.
            if (target.type == NPCID.TargetDummy)
            {
                if (Current is { IsDummy: false, IsAlive: true })
                    return false;
                if (Current == null || !Current.IsDummy)
                    SetContext(new FightContext(++nextId, target.FullName, -1, true, true));
                return true;
            }
            if (Current == null || Current.IsDummy || !Current.IsAlive)
            {
                RefreshBosses();
                // A boss can be killed by the first hit before the next NPC scan.
                if (Current is not { IsDummy: false, IsAlive: true } && IsBossPart(target))
                    SetContext(new FightContext(++nextId, target.FullName, target.GetBossHeadTextureIndex(), false, true));
            }
        }

        return Current is { IsDummy: false, IsAlive: true } &&
            target.type != NPCID.TargetDummy &&
            (Config.Conf.C.TrackAllEntities || IsBossPart(target));
    }

    public void SetContext(FightContext context)
    {
        if (context.Id < State.Id)
            return;
        bool changed = State.Begin(context.Id);
        if (!changed && Current is { IsAlive: false } && context.IsAlive)
            return;
        Current = context;
        UI?.PresentEncounter(changed);
        if (Main.netMode == NetmodeID.Server)
            PacketSender.Context(context);
    }

    public void ReceiveSnapshot(long id, PlayerFightData snapshot)
    {
        if (!State.Apply(id, snapshot))
            return;
        if (Main.netMode == NetmodeID.Server)
            PacketSender.Snapshot(id, snapshot);
        else
            RefreshPanel();
    }

    public void RemovePlayer(long id, int playerId)
    {
        if (id != State.Id)
            return;
        State.RemovePlayer(playerId);
        RefreshPanel();
    }

    public void ClearVisible()
    {
        State.ClearVisible();
        RefreshPanel();
    }

    public void RefreshPanel() => UI?.PresentEncounter();

    public void SendSync(int toClient)
    {
        if (Current == null)
            return;
        PacketSender.Context(Current, toClient);
        foreach (var snapshot in State.Players.Values)
            PacketSender.Snapshot(State.Id, snapshot, toClient);
    }
}
