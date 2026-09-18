using Assets.Scripts.Effects.EffectHandlers.Skills.Crusader;
using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.Effects.PrimitiveHandlers
{
    [RoPrimitive("DevotionLink")]
    public class DevotionLinkPrimitive : IPrimitiveHandler
    {
        public RagnarokEffectData.PrimitiveUpdateDelegate GetDefaultUpdateHandler() => Update;
        public RagnarokEffectData.PrimitiveRenderDelegate GetDefaultRenderHandler() => Render;

        private static void Update(RagnarokPrimitive primitive)
        {
            primitive.IsActive = primitive.Effect.FollowTarget != null && primitive.Effect.AimTarget != null;
            primitive.IsDirty = true;
        }

        private static void Render(RagnarokPrimitive primitive, MeshBuilder mb)
        {
            var effect = primitive.Effect;
            if (effect.FollowTarget == null || effect.AimTarget == null)
                return;

            var start = effect.FollowTarget.transform.position + Vector3.up * DevotionLinkEffect.Height;
            var end = effect.AimTarget.transform.position + Vector3.up * DevotionLinkEffect.Height;
            var direction = start - end;
            if (direction.sqrMagnitude < .000001f)
                return;

            // Native cross-sections stay vertical, even when the endpoints have different heights.
            var horizontal = new Vector3(direction.x, 0, direction.z);
            var side = horizontal.sqrMagnitude > .000001f
                ? new Vector3(-horizontal.z, 0, horizontal.x).normalized : Vector3.right;
            var frame = Mathf.Floor(primitive.CurrentPos * 60f);
            var phase = (int)(frame % 10f);
            var alpha = 45 + 2 * (phase < 5 ? phase : 8 - phase);
            var extension = frame < 15f ? Mathf.Sin(frame * 6f * Mathf.Deg2Rad) : 1f;
            end = Vector3.LerpUnclamped(start, end, extension);

            // Native primitive 38: a yellow inner tube and a blue outer tube, 10 sides each.
            for (var layer = 0; layer < 2; layer++)
            {
                var radius = .4f / 5f * (layer + 1);
                var color = layer == 0 ? new Color32(255, 255, 0, (byte)alpha)
                    : new Color32(0, 0, 255, (byte)(alpha * 2));
                var previous = side * radius;
                for (var segment = 1; segment <= 10; segment++)
                {
                    var angle = segment * 36f * Mathf.Deg2Rad;
                    var offset = radius * (side * Mathf.Cos(angle) - Vector3.up * Mathf.Sin(angle));
                    var transform = primitive.transform;
                    primitive.AddTexturedQuad(transform.InverseTransformPoint(start + previous),
                        transform.InverseTransformPoint(start + offset),
                        transform.InverseTransformPoint(end + previous),
                        transform.InverseTransformPoint(end + offset), color);
                    previous = offset;
                }
            }
        }
    }
}
