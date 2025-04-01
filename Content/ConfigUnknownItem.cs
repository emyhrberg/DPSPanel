using Terraria.ModLoader;

namespace DPSPanel.Content
{
    public class ConfigUnknownItem : ModItem
    {
        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.width = 16;
            Item.height = 24;
        }
    }

}