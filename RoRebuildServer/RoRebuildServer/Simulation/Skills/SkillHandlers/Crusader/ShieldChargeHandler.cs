using RebuildSharedData.Data;
using RebuildSharedData.Enum;
using RebuildSharedData.Enum.EntityStats;
using RoRebuildServer.EntityComponents;
using RoRebuildServer.EntityComponents.Util;
using RoRebuildServer.Networking;
using RoRebuildServer.Simulation.Util;

namespace RoRebuildServer.Simulation.Skills.SkillHandlers.Crusader;

[SkillHandler(CharacterSkill.ShieldCharge, SkillClass.Physical, SkillTarget.Enemy)]
public class ShieldChargeHandler : SkillHandlerBase
{
    public override int GetSkillRange(CombatEntity source, int lvl) => 3;

    public override SkillValidationResult ValidateTarget(CombatEntity source, CombatEntity? target, Position position,
        int lvl, bool isIndirect, bool isItemSource)
    {
        if (!source.HasShieldEquipped)
            return SkillValidationResult.IncorrectWeapon;

        return base.ValidateTarget(source, target, position, lvl, isIndirect, isItemSource);
    }

    public override void Process(CombatEntity source, CombatEntity? target, Position position, int lvl,
        bool isIndirect, bool isItemSource)
    {
        if (target == null || !target.IsValidTarget(source))
            return;

        lvl = lvl.Clamp(1, 5);
        var req = new AttackRequest(CharacterSkill.ShieldCharge, 1f + lvl * 0.2f, 1,
            AttackFlags.Physical | AttackFlags.Melee, AttackElement.None);
        var res = source.CalculateCombatResult(target, req);

        if (res.IsDamageResult && res.Damage > 0)
            res.KnockBack = (byte)(4 + lvl);

        source.ApplyCooldownForAttackAction(target);
        source.ExecuteCombatResult(res, false);
        CommandBuilder.SkillExecuteTargetedSkillAutoVis(source.Character, target.Character, CharacterSkill.ShieldCharge, lvl, res);

        if (res.IsDamageResult && res.Damage > 0)
            source.TryStunTarget(target, 150 + lvl * 50, res.TimeInSeconds);
    }
}
