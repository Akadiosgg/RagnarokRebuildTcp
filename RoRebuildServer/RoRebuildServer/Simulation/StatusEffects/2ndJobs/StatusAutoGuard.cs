using RebuildSharedData.Data;
using RebuildSharedData.Enum;
using RoRebuildServer.EntityComponents;
using RoRebuildServer.EntityComponents.Character;
using RoRebuildServer.EntityComponents.Util;
using RoRebuildServer.Simulation.StatusEffects.Setup;

namespace RoRebuildServer.Simulation.StatusEffects._2ndJobs;

[StatusEffectHandler(CharacterStatusEffect.AutoGuard, StatusClientVisibility.Ally)]
public class StatusAutoGuard : StatusEffectBase
{
    private static readonly int[] BlockChance = [5, 10, 14, 18, 21, 24, 26, 28, 29, 30];

    public override float Duration => 300f;
    public override StatusUpdateMode UpdateMode => StatusUpdateMode.OnCalculateDamageTaken | StatusUpdateMode.OnChangeEquipment;

    public override StatusUpdateResult OnChangeEquipment(CombatEntity ch, ref StatusEffectState state) =>
        ch.HasShieldEquipped ? StatusUpdateResult.Continue : StatusUpdateResult.EndStatus;

    public override StatusUpdateResult OnCalculateDamage(CombatEntity ch, ref StatusEffectState state,
        ref AttackRequest req, ref DamageInfo info)
    {
        if (!ch.HasShieldEquipped)
            return StatusUpdateResult.EndStatus;

        if (!req.Flags.HasFlag(AttackFlags.Physical) || req.Flags.HasFlag(AttackFlags.Magical)
            || !info.IsDamageResult || info.TotalDamage <= 0)
            return StatusUpdateResult.Continue;

        var level = int.Clamp(state.Value1, 1, 10);
        if (GameRandom.Next(0, 100) >= BlockChance[level - 1])
            return StatusUpdateResult.Continue;

        info.Result = AttackResult.Block;
        info.Damage = 0;
        info.DamageOffHand = 0;
        info.KnockBack = 0;
        info.BlockDelay = level <= 5 ? 0.3f : level < 10 ? 0.2f : 0.1f;
        return StatusUpdateResult.Continue;
    }
}
