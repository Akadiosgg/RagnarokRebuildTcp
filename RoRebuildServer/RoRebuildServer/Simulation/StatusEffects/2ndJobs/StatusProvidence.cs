using RebuildSharedData.Enum;
using RebuildSharedData.Enum.EntityStats;
using RoRebuildServer.EntityComponents;
using RoRebuildServer.Simulation.StatusEffects.Setup;

namespace RoRebuildServer.Simulation.StatusEffects._2ndJobs;

[StatusEffectHandler(CharacterStatusEffect.Providence, StatusClientVisibility.Owner)]
public class StatusProvidence : StatusEffectBase
{
    public override void OnApply(CombatEntity ch, ref StatusEffectState state)
    {
        ch.AddStat(CharacterStat.AddResistElementHoly, state.Value1);
        ch.AddStat(CharacterStat.AddResistRaceDemon, state.Value1);
    }

    public override void OnExpiration(CombatEntity ch, ref StatusEffectState state)
    {
        ch.SubStat(CharacterStat.AddResistElementHoly, state.Value1);
        ch.SubStat(CharacterStat.AddResistRaceDemon, state.Value1);
    }
}
