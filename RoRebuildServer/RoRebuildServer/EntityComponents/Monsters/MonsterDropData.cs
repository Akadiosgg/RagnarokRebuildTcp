namespace RoRebuildServer.EntityComponents.Monsters;

public class MonsterDropData
{
    public record MonsterDropEntry(int Id, int Chance, int CountMin = 1, int CountMax = 1);
    public int RefíneMaterialTotalWeight;

    public List<MonsterDropEntry> NativeDrops = new();
    public List<MonsterDropEntry> RefineMaterialDrops = new();

}