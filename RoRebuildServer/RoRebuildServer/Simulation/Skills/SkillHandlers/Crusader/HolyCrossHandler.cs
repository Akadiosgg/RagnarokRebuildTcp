using RebuildSharedData.Data;
using RebuildSharedData.Enum;
using RebuildSharedData.Enum.EntityStats;
using RoRebuildServer.EntityComponents;
using RoRebuildServer.EntityComponents.Util;
using RoRebuildServer.Networking;
using RoRebuildServer.Simulation.Util;

namespace RoRebuildServer.Simulation.Skills.SkillHandlers.Crusader;

[SkillHandler(CharacterSkill.HolyCross, SkillClass.Physical, SkillTarget.Enemy)]
public class HolyCrossHandler : SkillHandlerBase
{
    public override void Process(CombatEntity source, CombatEntity? target, Position position, int lvl,
        bool isIndirect, bool isItemSource)
    {
        if (target == null || !target.IsValidTarget(source))
            return;

        lvl = lvl.Clamp(1, 10);
        var req = new AttackRequest(CharacterSkill.HolyCross, 0.5f + lvl * 0.175f, 2,
            AttackFlags.Physical | AttackFlags.Melee, AttackElement.Holy);
        var res = source.CalculateCombatResult(target, req);

        source.ApplyCooldownForAttackAction(target);
        source.ExecuteCombatResult(res, false);
        CommandBuilder.SkillExecuteTargetedSkillAutoVis(source.Character, target.Character, CharacterSkill.HolyCross, lvl, res);

        if (res.IsDamageResult && res.Damage > 0)
            source.TryBlindTarget(target, lvl * 30, res.TimeInSeconds);
    }
}
