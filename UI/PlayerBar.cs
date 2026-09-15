using DPSPanel.Common.Configs;
using DPSPanel.Common.DamageCalculation.Classes;
using DPSPanel.Common.Systems;
using Terraria.UI;

namespace DPSPanel.UI;

public sealed class PlayerBar : DamageBar
{
    public int PlayerId { get; }
    public PlayerFightData Data { get; private set; }
    protected override bool HasIcon => Config.Conf.C.ShowPlayerIcons;

    public PlayerBar(int playerId) => PlayerId = playerId;

    public void SetPlayer(PlayerFightData player, long highest)
    {
        Data = player;
        SetData(player.Name, player.Damage, highest > 0 ? (int)(player.Damage * 100d / highest) : 0,
            ColorHelper.standardColors[PlayerId % ColorHelper.standardColors.Length]);
    }

    protected override void DrawIcon(SpriteBatch sb, CalculatedStyle dims)
    {
        if (!HasIcon || PlayerId < 0 || PlayerId >= Main.maxPlayers)
            return;
        Player player = Main.player[PlayerId];
        if (player.active)
            PlayerHeadFlipSystem.DrawHead(player, new Vector2(dims.X + 19, dims.Y + 20), 0.8f, Color.White);
    }
}
