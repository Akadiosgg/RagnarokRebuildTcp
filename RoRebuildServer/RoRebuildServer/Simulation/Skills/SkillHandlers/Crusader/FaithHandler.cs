using RebuildSharedData.Data;
using RebuildSharedData.Enum;
using RebuildSharedData.Enum.EntityStats;
using RoRebuildServer.EntityComponents;

namespace RoRebuildServer.Simulation.Skills.SkillHandlers.Crusader;

[SkillHandler(CharacterSkill.Faith, SkillClass.None, SkillTarget.Passive)]
public class FaithHandler : SkillHandlerBase
{
    public override void ApplyPassiveEffects(CombatEntity owner, int lvl)
    {
        owner.AddStat(CharacterStat.AddMaxHp, lvl * 200);
        owner.AddStat(CharacterStat.AddResistElementHoly, lvl * 5);
    }

    public override void RemovePassiveEffects(CombatEntity owner, int lvl)
    {
        owner.SubStat(CharacterStat.AddMaxHp, lvl * 200);
        owner.SubStat(CharacterStat.AddResistElementHoly, lvl * 5);
    }

    public override void Process(CombatEntity source, CombatEntity? target, Position position, int lvl,
        bool isIndirect, bool isItemSource)
    {
        throw new NotImplementedException();
    }
}
