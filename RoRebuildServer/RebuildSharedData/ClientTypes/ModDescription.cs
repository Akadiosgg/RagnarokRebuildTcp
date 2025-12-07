#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
namespace RebuildSharedData.ClientTypes;

[Serializable]
public class ModDescription
{
    public short ModId;
    public string Description;
}