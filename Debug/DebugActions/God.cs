using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace DPSPanel.Debug.DebugActions
{
    public class God : ModPlayer
    {
        public static bool GodEnabled = false;

        public override void OnEnterWorld()
        {
            base.OnEnterWorld();

            GodEnabled = true;
            Main.NewText("God mode enabled on enter world!", 255, 255, 0); // Yellow text
        }

        public override bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable)
        {
            if (GodEnabled)
            {
                // Prevents the player from dying
                return true;
            }
            return base.ImmuneTo(damageSource, cooldownCounter, dodgeable);
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (GodEnabled)
            {
                // Don't kill the player
                Player.statLife = Player.statLifeMax2;
                return false;
            }
            return true;
        }
    }
}
