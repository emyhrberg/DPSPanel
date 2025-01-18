using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace DPSPanel
{
    public class DPSPanel : Mod
    {
        public override void Load()
        {
            // Log NPC head boss indices and names
            for (int i = 0; i < TextureAssets.NpcHeadBoss.Length; i++)
            {
                if (TextureAssets.NpcHeadBoss[i]?.IsLoaded ?? false)
                {
                    Logger.Info($"Boss Head Index: {i}");
                }
            }

            // Log all NPC indices and their names
            foreach (NPC npc in Main.npc)
            {
                if (npc != null && npc.boss)
                {
                    Logger.Info($"Boss Name: {npc.FullName}, Boss Index: {npc.type}");
                }
            }
        }

        public override void Unload()
        {
            // Cleanup if necessary
        }
    }
}
