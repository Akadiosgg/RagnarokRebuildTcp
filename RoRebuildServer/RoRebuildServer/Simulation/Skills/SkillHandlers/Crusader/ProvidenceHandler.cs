using RebuildSharedData.Data;
using RebuildSharedData.Enum;
using RoRebuildServer.EntityComponents;
using RoRebuildServer.EntityComponents.Character;
using RoRebuildServer.Simulation.StatusEffects.Setup;
using RoRebuildServer.Simulation.Util;

namespace RoRebuildServer.Simulation.Skills.SkillHandlers.Crusader;

[SkillHandler(CharacterSkill.Providence, SkillClass.Magic, SkillTarget.Ally)]
public class ProvidenceHandler : SkillHandlerBase
{
    public override int GetSkillRange(CombatEntity source, int lvl) => 9;
    public override float GetCastTime(CombatEntity source, CombatEntity? target, Position position, int lvl) => 3f;

    public override SkillValidationResult ValidateTarget(CombatEntity source, CombatEntity? target, Position position,
        int lvl, bool isIndirect, bool isItemSource)
    {
        if (source == target)
            return SkillValidationResult.CannotTargetSelf;
        if (target == null || target.Character.Type == CharacterType.Player
            && (JobType)target.Player.JobId is JobType.JobCrusader or JobType.JobPecoCrusader)
            return SkillValidationResult.InvalidTarget;

        return StandardValidationForAllyTargetedAttack(source, target, position);
    }

    public override void Process(CombatEntity source, CombatEntity? target, Position position, int lvl,
        bool isIndirect, bool isItemSource)
    {
        if (target == null || source == target)
            return;

        lvl = lvl.Clamp(1, 5);
        target.AddStatusEffect(StatusEffectState.NewStatusEffect(CharacterStatusEffect.Providence, 180f, lvl * 5));

        var res = DamageInfo.SupportSkillResult(source.Entity, target.Entity, CharacterSkill.Providence);
        GenericCastAndInformSupportSkill(source, target, CharacterSkill.Providence, lvl, ref res, isIndirect, true);
    }
}
