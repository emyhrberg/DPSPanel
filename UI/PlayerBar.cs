using DPSPanel.Common.Configs;
using DPSPanel.Common.DamageCalculation.Classes;
using DPSPanel.Common.Systems;
using Terraria.UI;

namespace DPSPanel.UI;

public sealed class PlayerBar : DamageBar
{
    public int PlayerId { get; }
    public PlayerFightData Data { get; private set; }
    private Player previewPortrait;
    protected override bool HasIcon => Config.Conf.C.ShowPlayerIcons;
    protected override float RowHeight => DPSPanelLayout.PlayerBarHeight;
    protected override float TextStart => DPSPanelLayout.PlayerIconLeft + DPSPanelLayout.PlayerIconSize + DPSPanelLayout.IconTextGap;

    public PlayerBar(int playerId) => PlayerId = playerId;

    public void SetPlayer(PlayerFightData player, long highest, Player portrait = null)
    {
        Data = player;
        previewPortrait = portrait;
        SetData(player.Name, player.Damage, highest > 0 ? (int)(player.Damage * 100d / highest) : 0,
            ColorHelper.standardColors[PlayerId % ColorHelper.standardColors.Length]);
    }

    protected override void DrawIcon(SpriteBatch sb, CalculatedStyle dims)
    {
        if (!HasIcon)
            return;
        Player player = previewPortrait ?? (PlayerId >= 0 && PlayerId < Main.maxPlayers ? Main.player[PlayerId] : null);
        if (player?.active == true)
            PlayerHeadFlipSystem.DrawHead(player,
                new Vector2(dims.X + DPSPanelLayout.PlayerIconLeft + DPSPanelLayout.PlayerIconSize / 2,
                    dims.Y + dims.Height / 2 + DPSPanelLayout.PlayerIconOffsetY),
                DPSPanelLayout.PlayerHeadScale, LayoutColors.From(DPSPanelLayout.IconColor));
    }
}
