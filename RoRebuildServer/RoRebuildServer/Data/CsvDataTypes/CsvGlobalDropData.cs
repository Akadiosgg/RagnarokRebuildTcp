namespace RoRebuildServer.Data.CsvDataTypes;

public class CsvGlobalDropData
{
    public required string Code { get; set; }
    public required int InitialWeight { get; set; }
    public required int WeightScaling { get; set; }
    public required string DropType { get; set; }
    public required string MonsterRace { get; set; }
    public required string MonsterElement { get; set; }
    public required string MonsterSize { get; set; }
    public required string Region { get; set; }
    public required int MinLevel { get; set; }
}