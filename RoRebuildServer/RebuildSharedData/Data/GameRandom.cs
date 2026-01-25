using RebuildSharedData.Extensions;

namespace RebuildSharedData.Data;

public struct TakenModifierSet
{
    private ulong Mask; // values 0–63, there are much fewer entries in ModifierWeights

    public void Clear() => Mask = 0;

    public bool Contains(byte value)
        => (Mask & (1UL << value)) != 0;

    public void Add(byte value)
        => Mask |= (1UL << value);
}
public class GameRandom
{
    private static readonly Random _global = new Random();
    private static int _seed = 0;
    [ThreadStatic] private static Random? local;

    public static int Next() => NextInclusive(0, Int32.MaxValue);
    public static int Next(int max) => Next(0, max);
    public static int NextInclusive(int max) => NextInclusive(0, max);

    public static short NextShort() => (short)NextDouble(0, short.MaxValue);
    public static short NextShort(short max) => (short)NextDouble(0, max);
    public static short NextShort(short min, short max) => (short)NextDouble(min, max);

    public static float NextFloat() => (float)NextDouble();
    public static float NextFloat(float max) => (float)NextDouble(0, (double)max);
    public static float NextFloat(float min, float max) => (float)NextDouble((double)min, (double)max);

    public static double NextDouble(double max) => NextDouble(0f, max);

    private static void Initialize()
    {
        lock (_global)
        {
            if (local == null)
            {
                if (_seed == 0)
                    _seed = _global.Next();
                else
                    Interlocked.Increment(ref _seed);

                local = new Random(_seed);
            }
        }
    }


    public static int Next(int min, int max)
    {
        if (local == null)
            Initialize();

        return local!.Next(min, max);
    }

    public static int NextInclusive(int min, int max)
    {
        if (local == null)
            Initialize();

        return local!.Next(min, max + 1);
    }

    public static double NextDouble()
    {
        if (local == null)
            Initialize();

        return local!.NextDouble();
    }

    public static double NextDouble(double min, double max)
    {
        if (local == null)
            Initialize();

        return local!.NextDouble().Remap(0, 1, min, max);
    }
    //Randomly chooses index from a weighted list based on the lists total weight and ignores indexes in ignoredIndexes
    public static int WeightedRandomRoll<T>(IReadOnlyList<T> weightList, Func<T, int> weightSelector, int totalWeight, TakenModifierSet ignoredIndexes = default)
    {
        var randomNumber = Next(totalWeight);
        for (int i = 0; i < weightList.Count; i++)
        {
            if (ignoredIndexes.Contains((byte)i))
                continue;
            int w = weightSelector(weightList[i]);
            if (randomNumber < w)
                return i;

            randomNumber -= w;
        }

        return -1;
    }

    public static int WeightedRandomRoll(IReadOnlyList<int> weightList, int totalWeight)
    {
        var randomNumber = Next(totalWeight);
        for (int i = 0; i < weightList.Count; i++)
        {
            int w = weightList[i];
            if (randomNumber < w)
                return i;

            randomNumber -= w;
        }

        return -1;
    }
}