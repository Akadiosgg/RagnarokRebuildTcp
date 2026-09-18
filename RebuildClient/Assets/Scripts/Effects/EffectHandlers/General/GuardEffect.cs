using Assets.Scripts.Network;
using Assets.Scripts.Objects;
using UnityEngine;

namespace Assets.Scripts.Effects.EffectHandlers.General
{
    [RoEffect("Guard")]
    public class GuardEffect : IEffectHandler
    {
        public const int DurationFrames = 43;

        public static Ragnarok3dEffect Create(GameObject target, Vector3? hitSourcePosition = null)
        {
            var effect = RagnarokEffectPool.Get3dEffect(EffectType.Guard);
            effect.SetDurationByFrames(DurationFrames);
            effect.FollowTarget = target;
            effect.SourceEntity = target.GetComponent<ServerControllable>();
            effect.DestroyOnTargetLost = true;
            effect.transform.position = target.transform.position;

            var entity = effect.SourceEntity;
            var facing = entity != null ? (entity.SpriteAnimator != null ? entity.SpriteAnimator.Angle : entity.Angle) : 0f;
            if (hitSourcePosition.HasValue)
            {
                var direction = hitSourcePosition.Value - target.transform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.0001f)
                    facing = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            }
            for (var layer = 0; layer < 3; layer++)
            {
                var material = EffectSharedMaterialManager.GetMaterial(layer == 2
                    ? EffectMaterialType.GuardGlow : EffectMaterialType.Guard);
                var primitive = effect.LaunchPrimitive(PrimitiveType.Guard, material, DurationFrames / 60f);
                primitive.Flags[0] = layer;
            }

            // Rotate after launching the primitives: the pool initializes their world rotation to identity.
            effect.transform.rotation = Quaternion.Euler(0f, facing, 0f);

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
