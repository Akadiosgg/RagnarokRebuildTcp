using RebuildSharedData.Enum;
using RebuildSharedData.Enum.EntityStats;
using RoRebuildServer.EntityComponents;
using RoRebuildServer.Simulation.StatusEffects.Setup;

namespace RoRebuildServer.Simulation.StatusEffects._2ndJobs;

[StatusEffectHandler(CharacterStatusEffect.SpearQuicken, StatusClientVisibility.Everyone, StatusEffectFlags.None, "AspdBoost")]
public class StatusSpearQuicken : StatusEffectBase
{
    public override float Duration => 300f;
    public override StatusUpdateMode UpdateMode => StatusUpdateMode.OnChangeEquipment;

    public override StatusUpdateResult OnChangeEquipment(CombatEntity ch, ref StatusEffectState state)
    {
        if (ch.Character.Type == CharacterType.Player && ch.Player.MainWeaponClass != (int)WeaponClass.TwoHandSpear)
            return StatusUpdateResult.EndStatus;

        return StatusUpdateResult.Continue;
    }

    public override void OnApply(CombatEntity ch, ref StatusEffectState state)
    {
        ch.AddStat(CharacterStat.AspdBonus, state.Value1);
    }

    public override void OnExpiration(CombatEntity ch, ref StatusEffectState state)
    {
        ch.SubStat(CharacterStat.AspdBonus, state.Value1);
    }
}
