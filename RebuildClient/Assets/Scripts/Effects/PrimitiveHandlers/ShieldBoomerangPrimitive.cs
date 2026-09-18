using Assets.Scripts.Effects.EffectHandlers.Skills.Crusader;
using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.Effects.PrimitiveHandlers
{
    [RoPrimitive("ShieldBoomerang")]
    public class ShieldBoomerangPrimitive : IPrimitiveHandler
    {
        public RagnarokEffectData.PrimitiveUpdateDelegate GetDefaultUpdateHandler() => Update;
        public RagnarokEffectData.PrimitiveRenderDelegate GetDefaultRenderHandler() => Render;

        private static void Update(RagnarokPrimitive primitive) => primitive.IsDirty = true;

        private static void Render(RagnarokPrimitive primitive, MeshBuilder mb)
        {
            var animation = (ShieldBoomerangEffect.Animation)primitive.Effect.EffectData;
            var cameraRotation = Camera.main != null ? Camera.main.transform.rotation : Quaternion.identity;
            // Native primitive 42 uses four corners on a radius of seven world units.
            const float radius = 7f / 5f;
            for (var i = animation.Images.Length - 1; i >= 0; i--)
            {
                var image = animation.Images[i];
                if (image.Alpha <= 0)
                    continue;
                var rotation = cameraRotation * Quaternion.Euler(0, 0, image.Rotation);
                var right = rotation * Vector3.right * radius;
                var up = rotation * Vector3.up * radius;
                var center = image.Position;
                var transform = primitive.transform;
                primitive.AddTexturedQuad(transform.InverseTransformPoint(center - right),
                    transform.InverseTransformPoint(center + up), transform.InverseTransformPoint(center - up),
                    transform.InverseTransformPoint(center + right), new Color32(255, 255, 255, (byte)image.Alpha));
            }
        }
    }
}
