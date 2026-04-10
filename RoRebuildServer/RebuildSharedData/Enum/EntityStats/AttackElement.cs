namespace RebuildSharedData.Enum.EntityStats
{
    public enum AttackElement : byte
    {
        Neutral,
        Earth,
        Water,
        Fire,
        Wind,
        Poison,
        Undead,
        Dark,
        Holy,
        Ghost,
        Special, // AttackElement.Special is equal to number of possible elements
        None
    }
}