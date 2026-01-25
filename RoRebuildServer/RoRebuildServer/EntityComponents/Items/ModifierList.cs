
using RebuildSharedData.Data;
using RoRebuildServer.Data;
using RoRebuildServer.Logging;
using System.Collections.ObjectModel;


namespace RoRebuildServer.EntityComponents.Items;

public enum AffixType
{
    Prefix,
    Suffix,
    Count
}

public record struct TierInfo
{
    public byte Tier;
    public byte MinLevel;
    public short MinValue;
    public short MaxValue;
    public int Weight;
    public int CumulativeWeight;
    public TierInfo(byte tier, byte minLevel, short minValue, short maxValue, int weight, int cumulativeWeight = 0)
    {
        Tier = tier;
        MinValue = minValue;
        MaxValue = maxValue;
        Weight = weight;
        MinLevel = minLevel;
        CumulativeWeight = cumulativeWeight;
    }
}

public record struct ModifierWeightInfo
{
    public short ModifierId;
    public int Weight;
    public ModifierWeightInfo(short modifierId, int weight)
    {
        ModifierId = modifierId;
        Weight = weight;
    }
}

// With this class it can be determined what modifiers spawn on items, their tiers, and their weights. Modifiers are grouped by affix type (prefix/suffix), then by modifier id, then by tier (Modifiers field).
// It also keeps track of the total weights for each affix type (TotalAffixWeights field) and the weights of each modifier within each affix type (ModifierWeights field).

public class ModifierList
{
    public readonly int[] AffixWeights;

    public readonly List<ModifierWeightInfo>[] ModifierWeights;

    public readonly Dictionary<short, List<TierInfo>>[] Modifiers;

    public ModifierList()
    {
        int count = (int)AffixType.Count;

        AffixWeights = new int[count];
        ModifierWeights = new List<ModifierWeightInfo>[count];
        Modifiers = new Dictionary<short, List<TierInfo>>[count];

        for (int i = 0; i < count; i++)
        {
            ModifierWeights[i] = new List<ModifierWeightInfo>();
            Modifiers[i] = new Dictionary<short, List<TierInfo>>();
        }
    }

    public void GenerateModifiersForNewItem(ref UniqueItem item)
    {
        var modifierCount = GameRandom.WeightedRandomRoll(DataManager.modifierCountWeights, DataManager.totalModifierCountWeight);
        if (modifierCount == 0)
            return;

        var affixCount = (int)AffixType.Count;

        Span<bool> affixList = stackalloc bool[modifierCount]; // Determines the AffixTypes of rolled mods, starts with either 0 (Prefix) or 1 (Suffix) and then alternates
        var flip = GameRandom.Next(2) == 0;
        for (int i = 0; i < modifierCount; i++)
        {
            affixList[i] = flip;
            flip = !flip;
        }

        Span<int> affixWeights = stackalloc int[affixCount]; // Copy of AffixWeights, weights of already taken mods are subtracted from this
        AffixWeights.AsSpan().CopyTo(affixWeights);

        Span<TakenModifierSet> takenModifiers = stackalloc TakenModifierSet[affixCount]; // Saves already taken mods by index

        for (int i = 0; i < modifierCount; i++)
        {
            var affixIndex = affixList[i] ? 1 : 0; // 0 = Prefix, 1 = Suffix
            var modifierWeights = ModifierWeights[affixIndex]; // List of modifier weights for the current affixType and item
            var modIndex = GameRandom.WeightedRandomRoll(modifierWeights, x => x.Weight, affixWeights[affixIndex], takenModifiers[affixIndex]); // Choose modifier from modifierlist based on weights, excluding already taken mods
            var modifierId = modifierWeights[modIndex].ModifierId;
            var tierInfoList = Modifiers[affixIndex][modifierId]; // List of tier infos for the chosen modifier
            var tierIndex = GameRandom.WeightedRandomRoll(tierInfoList, x => x.Weight, ModifierWeights[affixIndex][modIndex].Weight); // Choose tier from tierinfo list based on weights
            var tierInfo = Modifiers[affixIndex][modifierId][tierIndex];
            var modifierValue = (short)GameRandom.NextInclusive(tierInfo.MinValue, tierInfo.MaxValue); // Choose modifier value within the chosen tier's min and max values
            affixWeights[affixIndex] -= ModifierWeights[affixIndex][modIndex].Weight; // Update affixWeights and takenModifiers to exclude the chosen modifier
            takenModifiers[affixIndex].Add((byte)modIndex);
            item.SetModifierIdAt(i, modifierId);
            item.SetModifierValueAt(i, modifierValue);
        }

    }

    public void AddModifierTier(string uniqueItemType, AffixType affixType, short modifierId, TierInfo tierInfo, string modifierName)
    {
        var affixIndex = (int)affixType;
        if (!Modifiers[affixIndex].ContainsKey(modifierId))
            Modifiers[affixIndex].Add(modifierId, new List<TierInfo>());
        if (ContainsModifierTier(affixType, modifierId, tierInfo.Tier))
            ServerLogger.LogWarning($"Could not add modifier tier to {uniqueItemType} - {affixType} - {modifierName} - Tier {tierInfo.Tier} because it already exists");
        else
        {
            Modifiers[affixIndex][modifierId].Add(tierInfo);
            AffixWeights[affixIndex] += tierInfo.Weight;

            var list = ModifierWeights[affixIndex];
            var modifierFound = false;
            var newWeight = 0;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].ModifierId == modifierId)
                {
                    newWeight = list[i].Weight + tierInfo.Weight;
                    list[i] = list[i] with { Weight = list[i].Weight + tierInfo.Weight };
                    modifierFound = true;
                    break;
                }
            }
            if (!modifierFound)
            {
                newWeight = tierInfo.Weight;
                list.Add(new ModifierWeightInfo(modifierId, tierInfo.Weight));
            }

            ServerLogger.LogVerbose($"Added modifier tier to {uniqueItemType} - {affixType} - {modifierName} - Tier {tierInfo.Tier} (Min: {tierInfo.MinValue}, " +
                $"Max: {tierInfo.MaxValue}, Weight: {tierInfo.Weight}) - TotalModifierWeight: {newWeight} - TotalAffixWeight: {AffixWeights[affixIndex]}");
        }
    }

    private bool ContainsModifierTier(AffixType affixType, short modifierId, byte tier)
    {
        var list = Modifiers[(int)affixType][modifierId];

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Tier == tier)
            {
                return true;
            }
        }
        return false;
    }

}

