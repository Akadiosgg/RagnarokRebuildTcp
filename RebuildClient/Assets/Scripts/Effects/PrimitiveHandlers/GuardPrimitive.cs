using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.Effects.PrimitiveHandlers
{
    [RoPrimitive("Guard")]
    public class GuardPrimitive : IPrimitiveHandler
    {
        public RagnarokEffectData.PrimitiveUpdateDelegate GetDefaultUpdateHandler() => Update;
        public RagnarokEffectData.PrimitiveRenderDelegate GetDefaultRenderHandler() => Render;

        private static void Update(RagnarokPrimitive primitive)
        {
            primitive.IsDirty = true;
        }

        private static Vector3 ToClient(Vector3 position) => new(position.x / 5f, -position.y / 5f, position.z / 5f);

        private static void Render(RagnarokPrimitive primitive, MeshBuilder mb)
        {
            // Native EF_GUARD: primitive 61, three layers of a 3-by-5 curved shield.
            var frame = Mathf.FloorToInt(primitive.CurrentPos * 60f);
            var fadeFrames = Mathf.Max(0, frame - 26);
            var alpha = Mathf.Clamp(Mathf.Min(100, frame * 20) - fadeFrames * 6, 0, 100);
            var layer = primitive.Flags[0];
            var tint = layer == 0 ? 205 : layer == 1 ? 155 : 225;
            var color = new Color32((byte)tint, 255, (byte)tint, (byte)alpha);
            const float baseRadius = 7.8f;
            var radius = baseRadius + layer * 0.2f + (layer == 2 ? fadeFrames * 0.1f : 0f);
            var height = baseRadius + (layer == 0 ? 2f : 0f);
            var entity = primitive.Effect.SourceEntity;
            var facing = entity != null ? (entity.SpriteAnimator != null ? entity.SpriteAnimator.Angle : entity.Angle) : 0f;
            // Client facing is clockwise from north; the mesh angles are counterclockwise from east.
            var orientation = 90f - facing;

            for (var row = 0; row < 3; row++)
            {
                var elevation = (row - 1) * 45f * Mathf.Deg2Rad;
                var tilt = (row + 1) * 45f * Mathf.Deg2Rad;
                var halfSize = 3f - Mathf.Abs(row - 1);

                for (var column = 0; column < 5; column++)
                {
                    var angle = (orientation + column * 45f - 90f) * Mathf.Deg2Rad;
                    var yaw = angle - Mathf.PI / 2f;
                    var center = new Vector3(Mathf.Cos(angle) * Mathf.Cos(elevation) * radius,
                        Mathf.Sin(elevation) * radius - height, Mathf.Sin(angle) * Mathf.Cos(elevation) * radius);
                    var horizontal = new Vector3(Mathf.Cos(yaw), 0f, Mathf.Sin(yaw)) * halfSize;
                    var vertical = new Vector3(-Mathf.Cos(tilt) * Mathf.Sin(yaw), Mathf.Sin(tilt),
                        Mathf.Cos(tilt) * Mathf.Cos(yaw)) * halfSize;

                    primitive.AddTexturedQuad(ToClient(center + horizontal + vertical),
                        ToClient(center - horizontal + vertical), ToClient(center + horizontal - vertical),
                        ToClient(center - horizontal - vertical), color);
                }
            }
        }
    }
}
