using RebuildSharedData.Data;
using RebuildSharedData.Enum;
using RoRebuildServer.EntityComponents;
using RoRebuildServer.Networking;
using RoRebuildServer.Simulation.StatusEffects.Setup;
using RoRebuildServer.Simulation.Util;

namespace RoRebuildServer.Simulation.Skills.SkillHandlers.Crusader;

[SkillHandler(CharacterSkill.ReflectShield, SkillClass.Physical, SkillTarget.Self)]
public class ReflectShieldHandler : SkillHandlerBase
{
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
        lvl = lvl.Clamp(1, 10);
        source.ApplyCooldownForSupportSkillAction();
        source.AddStatusEffect(StatusEffectState.NewStatusEffect(CharacterStatusEffect.ReflectShield, 300f, lvl));
        CommandBuilder.SkillExecuteSelfTargetedSkillAutoVis(source.Character, CharacterSkill.ReflectShield, lvl, isIndirect);
    }
}
