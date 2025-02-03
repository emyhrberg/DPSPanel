using DPSPanel.UI;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace DPSPanel.Configs
{
    public class Commands : ModCommand
    {
        public override CommandType Type => CommandType.Chat; // Makes the command available in chat
        public override string Command => "dps"; // The main command is "/dps"
        public override string Description => "Usage: /dps <add> <clear> <toggle>"; // Is shown when using "/help"
        public override string Usage => "/dps <add> <clear> <toggle>"; // idk what this does

        // create list of example players using real funny russian names. 10 players
        private string[] players = ["Vladimir", "Ivan", "Dmitri", "Sergei, Alexei", "Yuri", "Anatoli", "Boris", "Mikhail", "Nikolai", "Pavel"];
        int i = 0;

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var sys = ModContent.GetInstance<PanelSystem>();

            // validate args
            if (args.Length != 1)
            {
                throw new UsageException("Error: Incorrect command. Use /help for tips.");
            }

            string target = args[0];

            if (target == "add")
            {
                // damage = random number between 100 and 2000
                int damage = Main.rand.Next(100, 2000);
                int playerHeadIndex = 0;
                i++;
                if (i >= players.Length)
                    i = Main.rand.Next(0, players.Length);
                string randomName = players[i];
                sys.state.container.panel.UpdateDamageBars(randomName, damage, playerHeadIndex);
            }
            else if (target == "clear")
            {
                sys.state.container.panel.ClearPanelAndAllItems();
                sys.state.container.panel.SetBossTitle("DPSPanel", -1);
            }
            else if (target == "toggle")
            {
                ModContent.GetInstance<Config>().AlwaysShowButton = !ModContent.GetInstance<Config>().AlwaysShowButton;

                Rectangle pos = Main.LocalPlayer.getRect();
                Color color = ModContent.GetInstance<Config>().AlwaysShowButton ? Color.Green : Color.Red;
                string text = ModContent.GetInstance<Config>().AlwaysShowButton ? "Button will always show" : "Button will only show when inventory is open";
                CombatText.NewText(pos, color, text);
            }
            else
            {
                throw new UsageException("Error: Incorrect argument. Use /help for tips.");
            }
        }
    }
}
