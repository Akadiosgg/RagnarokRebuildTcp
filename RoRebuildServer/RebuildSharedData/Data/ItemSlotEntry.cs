using RebuildSharedData.Enum.EntityStats;
using RebuildSharedData.Util;

namespace RebuildSharedData.Data;

public interface ISerializableItem
{
    public void Serialize(IBinaryMessageWriter bw);
}

public unsafe struct RegularItem : ISerializableItem
{
    public int Id;
    public short Count;

    public static readonly int Size = 6;

    public readonly void Serialize(IBinaryMessageWriter bw)
    {
        bw.Write(Id);
        bw.Write(Count);
    }

    public static RegularItem Deserialize(IBinaryMessageReader br)
    {
        var entry = new RegularItem() { Id = br.ReadInt32(), Count = br.ReadInt16() };

        return entry;
    }

    public static RegularItem ZeroResult() => new() { Id = -1, Count = 0 };
}

[Flags]
public enum UniqueItemFlags : byte
{
    None = 0,
    CraftedItem = 1 << 0,
}

public unsafe struct UniqueItem : ISerializableItem
{
    public int Id;
    public byte ItemLevel;
    public short Count;
    public byte Flags;
    public byte Refine;
    public Guid UniqueId;
    public fixed int Data[4];
    public fixed int Modifiers[8];

    public static int Size => 73; //Id(4) + ItemLevel(1) Count(2) + Flags(1) + Refine (1) + UniqueId(16) + Data(4 * 4) + Modifiers(8 * (2 + 2))

    public void SetItemLevel(byte itemLevel) => ItemLevel = itemLevel;
    public int SlotData(int slot) => Data[slot];
    public int SetSlotData(int slot, int val) => Data[slot] = val;
    public short GetModifierIdAt(int index) => (short)(Modifiers[index] >> 16 & 0xFFFF);
    public short GetModifierValueAt(int index) => (short)(Modifiers[index] & 0xFFFF);
    public void SetModifierAt(int index, short id, short value) => Modifiers[index] = (id << 16) | (value & 0xFFFF);


    public void Serialize(IBinaryMessageWriter msg)
    {
        msg.Write(Id);
        msg.Write(ItemLevel);
        msg.Write(Count);
        msg.Write(Flags);
        msg.Write(Refine);
        msg.Write(UniqueId.ToByteArray());
        
        for (var i = 0; i < 4; i++)
            msg.Write(Data[i]);
        
        for (var i = 0; i < 8; i++)
        {
            msg.Write(Modifiers[i]);
        }
    }

    public readonly void SerializeAsRegularItem(IBinaryMessageWriter msg)
    {
        msg.Write(Id);
        msg.Write(Count);
    }

    public static UniqueItem Deserialize(IBinaryMessageReader br)
    {
        var entry = new UniqueItem()
        {
            Id = br.ReadInt32(),
            ItemLevel = br.ReadByte(),
            Count = br.ReadInt16(),
            Flags = br.ReadByte(),
            Refine = br.ReadByte(),
            UniqueId = new Guid(br.ReadBytes(16)),
        };

        for (var i = 0; i < 4; i++)
            entry.Data[i] = br.ReadInt32();

        for (var i = 0; i < 8; i++)
        {
            entry.Modifiers[i] = br.ReadInt32();
        }


        return entry;
    }

    public unsafe bool ContainsItemIdInSlot(int slotItemId)
    {
        fixed (int* data = Data)
        {
            for (int i = 0; i < 4; i++)
            {
                if (data[i] == slotItemId)
                    return true;
            }
        }

        return false;
    }

    public unsafe (int, int) FetchWeaponModifications()
    {
        //int[] weaponDamage = new int[(int)AttackElement.Special];
        int weaponDamage = 0;
        int weaponAttackPercent = 0;
        {
            for (int i = 0; i < 8; i++)
            {
                if (GetModifierIdAt(i) == 274)
                {
                    weaponAttackPercent = GetModifierValueAt(i);
                }
                if (GetModifierIdAt(i) == 275)
                {
                    weaponDamage = GetModifierValueAt(i);
                }
            }
        }
        return (weaponDamage, weaponAttackPercent);
    }
}