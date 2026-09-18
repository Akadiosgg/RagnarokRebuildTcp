using UnityEngine;

namespace Assets.Scripts.Effects.EffectHandlers.Skills.Crusader
{
    [RoEffect("Providence")]
    public class ProvidenceEffect : IEffectHandler
    {
        public const int DurationFrames = 200;

        // The STR overlay is attached separately through the same Effects.csv entry.
        public static Ragnarok3dEffect Create(GameObject target)
        {
            var effect = RagnarokEffectPool.Get3dEffect(EffectType.Providence);
            effect.FollowTarget = target;
            effect.DestroyOnTargetLost = true;
            effect.transform.position = target.transform.position;
            effect.SetDurationByTime(DurationFrames / 60f);

            var primitive = effect.LaunchPrimitive(PrimitiveType.Casting3D,
                EffectSharedMaterialManager.GetMaterial(EffectMaterialType.Providence), effect.Duration);
            primitive.UpdateHandler = UpdateRings;
            // Casting3D scales by 1/10; native world units convert at 1/5.
            primitive.transform.localScale = Vector3.one * 2f;
            primitive.CreateParts(4);
            for (var i = 0; i < primitive.Parts.Length; i++)
            {
                var part = primitive.Parts[i];
                part.Active = true;
                part.CoverAngle = 360f;
                part.RiseAngle = 83f - i;
                part.MaxHeight = 120f;
                part.Distance = 6f + i * .5f;
                part.RotStart = i * 90f;
            }
            UpdateRings(primitive);
            return effect;
        }

        private static void UpdateRings(RagnarokPrimitive primitive)
        {
            // Native primitive 41 starts its counter at 200: rise for 90 frames,
            // then hold. Alpha starts at 40 and falls by one after frame 50.
            var frame = Mathf.Clamp(Mathf.FloorToInt(primitive.CurrentPos * 60f), 0, DurationFrames);
            var alpha = Mathf.Max(0, 40 - Mathf.Max(0, frame - 50));
            foreach (var part in primitive.Parts)
            {
                part.Alpha = alpha;
                part.Angle = Mathf.Repeat(part.RotStart + frame, 360f);
                var height = part.MaxHeight * Mathf.Sin(Mathf.Min(frame, 90) * Mathf.Deg2Rad);
                for (var j = 0; j < EffectPart.SegmentCount; j++)
                    part.Heights[j] = height;
            }
            primitive.IsDirty = true;
        }

        public bool Update(Ragnarok3dEffect effect, float pos, int step) => pos < effect.Duration;
    }
}
