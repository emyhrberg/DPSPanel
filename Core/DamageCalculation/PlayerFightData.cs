namespace DPSPanel.Core.DamageCalculation
{
    public class PlayerFightData(int playerID, string playerName, int playerDamage)
    {
        public int playerID = playerID;
        public string playerName = playerName;
        public int playerDamage = playerDamage;
    }
}
