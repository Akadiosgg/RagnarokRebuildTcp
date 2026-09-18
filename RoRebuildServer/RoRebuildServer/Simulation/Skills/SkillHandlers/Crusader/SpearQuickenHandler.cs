using RebuildSharedData.Data;
using RebuildSharedData.Enum;
using RoRebuildServer.EntityComponents;
using RoRebuildServer.Networking;
using RoRebuildServer.Simulation.StatusEffects.Setup;
using RoRebuildServer.Simulation.Util;

namespace RoRebuildServer.Simulation.Skills.SkillHandlers.Crusader;

[SkillHandler(CharacterSkill.SpearQuicken, SkillClass.Physical, SkillTarget.Self)]
public class SpearQuickenHandler : SkillHandlerBase
{
    public override SkillValidationResult ValidateTarget(CombatEntity source, CombatEntity? target, Position position,
        int lvl, bool isIndirect, bool isItemSource)
    {
        if (source.Character.Type == CharacterType.Player && source.Player.MainWeaponClass != (int)WeaponClass.TwoHandSpear)
            return SkillValidationResult.IncorrectWeapon;

        return base.ValidateTarget(source, target, position, lvl, isIndirect, isItemSource);
    }

    public override void Process(CombatEntity source, CombatEntity? target, Position position, int lvl,
        bool isIndirect, bool isItemSource)
    {
        lvl = lvl.Clamp(1, 10);
        source.ApplyCooldownForSupportSkillAction();
        source.AddStatusEffect(StatusEffectState.NewStatusEffect(CharacterStatusEffect.SpearQuicken, lvl * 30f, 20 + lvl));
        CommandBuilder.SkillExecuteSelfTargetedSkillAutoVis(source.Character, CharacterSkill.SpearQuicken, lvl, isIndirect);
    }
}
