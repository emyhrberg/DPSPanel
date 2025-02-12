namespace DPSPanel.Core.DamageCalculation
{
    public class PlayerFightData(int playerWhoAmI, string playerName, int playerDamage)
    {
        public int playerWhoAmI = playerWhoAmI;
        public string playerName = playerName;
        public int playerDamage = playerDamage;
    }
}
