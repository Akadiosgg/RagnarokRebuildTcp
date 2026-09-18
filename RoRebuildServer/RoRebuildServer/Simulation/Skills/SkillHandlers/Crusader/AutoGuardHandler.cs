using RebuildSharedData.Data;
using RebuildSharedData.Enum;
using RoRebuildServer.EntityComponents;
using RoRebuildServer.Networking;
using RoRebuildServer.Simulation.StatusEffects.Setup;
using RoRebuildServer.Simulation.Util;

namespace RoRebuildServer.Simulation.Skills.SkillHandlers.Crusader;

[SkillHandler(CharacterSkill.AutoGuard, SkillClass.Physical, SkillTarget.Self)]
public class AutoGuardHandler : SkillHandlerBase
{
    public override bool ShouldSkillCostSp(CombatEntity source) => !source.HasStatusEffectOfType(CharacterStatusEffect.AutoGuard);

    public override SkillValidationResult ValidateTarget(CombatEntity source, CombatEntity? target, Position position,
        int lvl, bool isIndirect, bool isItemSource)
    {
        if (!source.HasStatusEffectOfType(CharacterStatusEffect.AutoGuard) && !source.HasShieldEquipped)
            return SkillValidationResult.IncorrectWeapon;

        return base.ValidateTarget(source, target, position, lvl, isIndirect, isItemSource);
    }

    public override void Process(CombatEntity source, CombatEntity? target, Position position, int lvl,
        bool isIndirect, bool isItemSource)
    {
        lvl = lvl.Clamp(1, 10);
        source.ApplyCooldownForSupportSkillAction();

        if (!source.RemoveStatusOfTypeIfExists(CharacterStatusEffect.AutoGuard))
            source.AddStatusEffect(StatusEffectState.NewStatusEffect(CharacterStatusEffect.AutoGuard, 300f, lvl));

        CommandBuilder.SkillExecuteSelfTargetedSkillAutoVis(source.Character, CharacterSkill.AutoGuard, lvl, isIndirect);
    }
}
