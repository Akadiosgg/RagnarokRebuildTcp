using Assets.Scripts.Network;
using UnityEngine;

namespace Assets.Scripts.Effects.EffectHandlers.Skills.Crusader
{
    [RoEffect("DevotionLink")]
    public class DevotionLinkEffect : IEffectHandler
    {
        public const float Height = 8f / 5f;
        public const float PreviewDuration = 8f;

        public static Ragnarok3dEffect CreatePreview(GameObject source)
        {
            var entity = source.GetComponent<ServerControllable>();
            var facing = entity != null ? (entity.SpriteAnimator != null ? entity.SpriteAnimator.Angle : entity.Angle) : 0f;
            var target = new GameObject("DevotionPreviewTarget");
            target.transform.position = source.transform.position + Quaternion.Euler(0, facing, 0) * Vector3.forward * 6f;
            var effect = Create(source, target, PreviewDuration);
            effect.Flags[0] = 1; // Only the preview owns its temporary endpoint.
            return effect;
        }

        // durationSeconds == 0 keeps the link until EndEffect or either endpoint is lost.
        public static Ragnarok3dEffect Create(GameObject source, GameObject target, float durationSeconds = 0f)
        {
            var effect = RagnarokEffectPool.Get3dEffect(EffectType.DevotionLink);
            effect.FollowTarget = source;
            effect.AimTarget = target;
            effect.DestroyOnTargetLost = true;
            effect.transform.position = source.transform.position;
            effect.SetDurationByTime(durationSeconds > 0f ? durationSeconds : float.PositiveInfinity);
            effect.LaunchPrimitive(PrimitiveType.DevotionLink,
                EffectSharedMaterialManager.GetMaterial(EffectMaterialType.DevotionLink));
            return effect;
        }

        public bool Update(Ragnarok3dEffect effect, float pos, int step) =>
            effect.FollowTarget != null && effect.FollowTarget.activeInHierarchy &&
            effect.AimTarget != null && effect.AimTarget.activeInHierarchy && pos < effect.Duration;

        public void OnCleanup(Ragnarok3dEffect effect)
        {
            if (effect.Flags[0] == 1 && effect.AimTarget != null)
                Object.Destroy(effect.AimTarget);
        }
    }
}
