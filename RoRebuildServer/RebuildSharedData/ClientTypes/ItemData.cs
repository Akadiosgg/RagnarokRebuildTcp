using RebuildSharedData.Enum;
using RebuildSharedData.Enum.EntityStats;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace RebuildSharedData.ClientTypes;

[Serializable]
public class ItemData
{
    public int Id;
    public string Code;
    public string Name;
    public int Weight;
    public int Price;
    public int SellPrice;
    public bool IsUnique;
    public ItemClass ItemClass;
    public ItemUseType UseType;
    public string Sprite;
    // For unique items
    public EquipPosition Position;
    public bool IsRefinable;
    public int ItemRank;
    public int Slots;
    public int MinLvl;
    public string EquipGroup;
    // For weapons
    public int Attack;
    public float AttackSpeed;
    public AttackElement AttackElement;
    public string WeaponClass;
    public int Range;
    public int SubType;
    // For equipment
    public int Defense;
    public int Flee;
    public int MagicDef;
}

[Serializable]
public class ItemDataList
{
    public List<ItemData> Items = null!;
}