using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;

namespace BetterDPS.Content.Commands
{
    public class PrintDamage : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "p"; // command written in chat e.g. /summon
        public override string Description => "Prints the damage per second";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            /* Action taken upon executing the command */

            // Use the player's entity as the source
            IEntitySource source = caller.Player.GetSource_FromThis();
            int dps = caller.Player.dpsDamage; // Get the damage per second from the game

            // red color
            Main.NewText($"DPS: {dps}", Color.Red);
        }
    }
}