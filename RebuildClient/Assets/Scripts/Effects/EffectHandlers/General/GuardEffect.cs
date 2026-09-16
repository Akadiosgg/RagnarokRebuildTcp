using Assets.Scripts.Network;
using Assets.Scripts.Objects;
using UnityEngine;

namespace Assets.Scripts.Effects.EffectHandlers.General
{
    [RoEffect("Guard")]
    public class GuardEffect : IEffectHandler
    {
        public const int DurationFrames = 43;

        public static Ragnarok3dEffect Create(GameObject target)
        {
            var effect = RagnarokEffectPool.Get3dEffect(EffectType.Guard);
            effect.SetDurationByFrames(DurationFrames);
            effect.FollowTarget = target;
            effect.SourceEntity = target.GetComponent<ServerControllable>();
            effect.DestroyOnTargetLost = true;
            effect.transform.position = target.transform.position;

            for (var layer = 0; layer < 3; layer++)
            {
                var material = EffectSharedMaterialManager.GetMaterial(layer == 2
                    ? EffectMaterialType.GuardGlow : EffectMaterialType.Guard);
                var primitive = effect.LaunchPrimitive(PrimitiveType.Guard, material, DurationFrames / 60f);
                primitive.Flags[0] = layer;
            }

            AudioManager.Instance.OneShotSoundEffect(effect.SourceEntityId, "kyrie_guard.ogg", target.transform.position, 0.8f);
            return effect;
        }

        public bool Update(Ragnarok3dEffect effect, float pos, int step)
        {
            // Use elapsed time so the animation keeps its duration at low frame rates.
            return pos < DurationFrames / 60f;
        }

        public void OnCleanup(Ragnarok3dEffect effect)
        {
            effect.SourceEntity = null;
        }
    }
}
