using UnityEngine;

namespace Assets.Scripts.Effects.EffectHandlers.Skills.Crusader
{
    [RoEffect("ReflectShield")]
    public class ReflectShieldEffect : IEffectHandler
    {
        public const int DurationFrames = ShieldRingEffect.DurationFrames;

        public static Ragnarok3dEffect Create(GameObject target) =>
            ShieldRingEffect.Create(target, EffectType.ReflectShield, EffectMaterialType.ReflectShield);

        public bool Update(Ragnarok3dEffect effect, float pos, int step) => pos < effect.Duration;
    }
}
