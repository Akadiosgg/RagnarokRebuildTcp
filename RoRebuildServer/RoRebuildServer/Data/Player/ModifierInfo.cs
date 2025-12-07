using RebuildSharedData.Enum;
using RebuildSharedData.Enum.EntityStats;
using RoRebuildServer.EntityComponents.Items;

namespace RoRebuildServer.Data.Player;

[Flags]
public enum ModTypeFlags : int
{
    None = 0,
    HP = 1,
    SP = 2,
    Attack = 4,
    Defense = 8,
    Speed = 16,
    Crit = 32,
    Attribute = 64,
    Resistance = 128,
    Status = 256,
    Racial = 512,
    Elemental = 1024,
    Caster = 2048,
    Size = 4096,
    Trigger = 8192
}
public class ModifierInfo
{
    public string Name = null!;
    public ModTypeFlags TypeFlags;
    public ItemInteractionBase? Interaction = null;
}