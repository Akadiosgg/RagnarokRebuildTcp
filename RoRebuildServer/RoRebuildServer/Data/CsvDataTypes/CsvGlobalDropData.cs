namespace RoRebuildServer.Data.CsvDataTypes;

public class CsvGlobalDropData
{
    public string Code { get; set; }
    public int InitialWeight { get; set; }
    public int WeightScaling { get; set; }
    public string DropType { get; set; }
    public string MonsterRace { get; set; }
    public string MonsterElement { get; set; }
    public string MonsterSize { get; set; }
    public string Region { get; set; }
    public int MinLevel { get; set; }
}