public class CsvModifier
{
    public required short Id { get; set; }
    public required string Name { get; set; }
    public required string TypeFlags { get; set; }
    public required string Description { get; set; }
    public required int DisplayScale { get; set; }
}


public class CsvUniqueItemTypeModifierList
{ 
    public required string UniqueItemType { get; set; }
    public required string AffixType { get; set; }
    public required string ModifierName { get; set; }
    public required byte Tier { get; set; }
    public required byte MinLevel { get; set; }
    public required short MinValue { get; set; }
    public required short MaxValue { get; set; }
    public required int Weight { get; set; }
}