using UnityEngine;

namespace Assets.Scripts.Effects.EffectHandlers.Skills.Crusader
{
    internal static class ShieldRingEffect
    {
        public const int DurationFrames = 200;

        public static Ragnarok3dEffect Create(GameObject target, EffectType effectType, EffectMaterialType materialType)
        {
            var effect = RagnarokEffectPool.Get3dEffect(effectType);
            effect.FollowTarget = target;
            effect.DestroyOnTargetLost = true;
            effect.transform.position = target.transform.position;
            effect.SetDurationByTime(DurationFrames / 60f);

            var primitive = effect.LaunchPrimitive(PrimitiveType.Casting3D,
                EffectSharedMaterialManager.GetMaterial(materialType), effect.Duration);
            primitive.UpdateHandler = UpdateRings;
            // Casting3D renders at 1/10 scale; native world units here convert at 1/5.
            primitive.transform.localScale = Vector3.one * 2f;
            primitive.CreateParts(4);
            for (var i = 0; i < primitive.Parts.Length; i++)
            {
                var part = primitive.Parts[i];
                part.Active = true;
                part.CoverAngle = 360f;
                part.RiseAngle = 90f;
                part.MaxHeight = 40f - i;
                part.Distance = 8f - i * .1f;
                part.RotStart = Mathf.Repeat(180f + i * 90f, 360f);
            }
            UpdateRings(primitive);
            return effect;
        }

        private static void UpdateRings(RagnarokPrimitive primitive)
        {
            // Use elapsed time so skipped render frames do not slow down the native animation.
            var frame = Mathf.Clamp(Mathf.FloorToInt(primitive.CurrentPos * 60f), 0, DurationFrames);
            var alpha = Mathf.Max(0, Mathf.Min(80, frame) - Mathf.Max(0, frame - (DurationFrames - 21)) * 5);
            for (var i = 0; i < primitive.Parts.Length; i++)
            {
                var part = primitive.Parts[i];
                part.Alpha = alpha;
                part.Angle = Mathf.Repeat(part.RotStart + frame * (i % 2 == 0 ? 1f : 2f), 360f);
                var phase = i < 2 ? frame + i * 90f : (frame + i * 45f) * 2f;
                var height = part.MaxHeight * (1f + .3f * Mathf.Sin(phase * Mathf.Deg2Rad));
                for (var j = 0; j < EffectPart.SegmentCount; j++)
                    part.Heights[j] = height * Mathf.Sin(j * 9f * Mathf.Deg2Rad);
            }
            primitive.IsDirty = true;
        }
    }
}
