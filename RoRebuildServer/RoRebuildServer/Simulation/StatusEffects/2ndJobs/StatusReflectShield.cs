using RebuildSharedData.Enum;
using RoRebuildServer.EntityComponents;
using RoRebuildServer.EntityComponents.Character;
using RoRebuildServer.Networking;
using RoRebuildServer.Simulation.StatusEffects.Setup;
using RoRebuildServer.Simulation.Util;

namespace RoRebuildServer.Simulation.StatusEffects._2ndJobs;

[StatusEffectHandler(CharacterStatusEffect.ReflectShield, StatusClientVisibility.Ally)]
public class StatusReflectShield : StatusEffectBase
{
    public override float Duration => 300f;
    public override StatusUpdateMode UpdateMode => StatusUpdateMode.OnTakeDamage | StatusUpdateMode.OnChangeEquipment;

    public override StatusUpdateResult OnChangeEquipment(CombatEntity ch, ref StatusEffectState state) =>
        ch.HasShieldEquipped ? StatusUpdateResult.Continue : StatusUpdateResult.EndStatus;

    public override StatusUpdateResult OnTakeDamage(CombatEntity ch, ref StatusEffectState state, ref DamageInfo info)
    {
        if (!ch.HasShieldEquipped)
            return StatusUpdateResult.EndStatus;

        if (!info.Flags.HasFlag(DamageApplicationFlags.MeleeDamage)
            || info.Flags.HasFlag(DamageApplicationFlags.SkipOnHitTriggers)
            || !info.IsDamageResult || info.TotalDamage <= 0 || info.Source == ch.Entity
            || !info.Source.TryGet<CombatEntity>(out var attacker)
            || attacker.Character.State == CharacterState.Dead || attacker.Character.Map != ch.Character.Map)
            return StatusUpdateResult.Continue;

        var reflected = new DamageInfo
        {
            Source = ch.Entity,
            Target = attacker.Entity,
            AttackPosition = ch.Character.Position,
            AttackSkill = CharacterSkill.ReflectShield,
            Result = AttackResult.NormalDamage,
            Damage = Math.Max(1, (int)((long)info.TotalDamage * (10 + 3 * int.Clamp(state.Value1, 1, 10)) / 100)),
            HitCount = 1,
            Time = Time.ElapsedTimeFloat,
            Flags = DamageApplicationFlags.PhysicalDamage | DamageApplicationFlags.NoHitLock | DamageApplicationFlags.SkipOnHitTriggers
        };
        ch.ExecuteCombatResult(reflected, false);
        CommandBuilder.AttackAutoVis(null, attacker.Character, reflected, false);
        return StatusUpdateResult.Continue;
    }
}
