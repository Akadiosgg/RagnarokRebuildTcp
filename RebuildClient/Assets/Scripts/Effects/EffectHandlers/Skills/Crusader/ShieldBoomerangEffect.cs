using Assets.Scripts.Network;
using Assets.Scripts.Objects;
using UnityEngine;

namespace Assets.Scripts.Effects.EffectHandlers.Skills.Crusader
{
    [RoEffect("ShieldBoomerang")]
    public class ShieldBoomerangEffect : IEffectHandler
    {
        public const string SoundName = "cru_shield boomerang.ogg";
        public const float TravelPerFrame = 2f / 5f;
        public const float Height = 8f / 5f;

        public struct ShieldImage
        {
            public Vector3 Position;
            public float Rotation;
            public int Alpha;
        }

        public class Animation
        {
            public readonly ShieldImage[] Images = new ShieldImage[4];
            public Vector3 Origin;
            public float Distance;
            public float Heading;
            public float HeightPerFrame;
            public int Frame;
            public int Phase;

            public Animation(Vector3 source, Vector3 destination)
            {
                Origin = source;
                var direction = destination - source;
                Distance = new Vector2(direction.x, direction.z).magnitude;
                Heading = Mathf.Repeat(Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg, 360f);
                HeightPerFrame = Distance > .0001f ? direction.y * TravelPerFrame / Distance : 0f;
                Images[0] = new ShieldImage { Position = source + Vector3.up * Height, Alpha = 255 };
            }

            public void Advance(Vector3 source)
            {
                Frame++;
                // The native primitive samples its three afterimages before moving, every third frame.
                if (Frame % 3 == 0)
                {
                    for (var i = 3; i > 0; i--)
                    {
                        Images[i] = Images[i - 1];
                        Images[i].Alpha = Mathf.Max(0, Images[i].Alpha - (i == 1 ? 150 : 25));
                    }
                }

                ref var shield = ref Images[0];
                shield.Rotation = Mathf.Repeat(shield.Rotation - 15f, 360f);
                if (Phase == 0)
                {
                    if (Frame * TravelPerFrame > Distance)
                        Phase = 1;
                    shield.Position.y += HeightPerFrame;
                }
                else if (Phase == 1)
                {
                    var direction = source - shield.Position;
                    var headingBack = Mathf.Repeat(Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg, 360f);
                    if (Mathf.Abs(Mathf.DeltaAngle(Heading, headingBack)) <= 4f)
                        Phase = 2;
                    else
                        Heading = Mathf.Repeat(Heading + 5f, 360f);
                }
                else
                {
                    shield.Alpha = Mathf.Max(0, shield.Alpha - 12);
                    shield.Position.y -= HeightPerFrame;
                }

                var radians = Heading * Mathf.Deg2Rad;
                shield.Position += new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians)) * TravelPerFrame;
            }
        }

        public static Ragnarok3dEffect CreatePreview(GameObject source)
        {
            var entity = source.GetComponent<ServerControllable>();
            var facing = entity != null ? (entity.SpriteAnimator != null ? entity.SpriteAnimator.Angle : entity.Angle) : 0f;
            return Create(source, source.transform.position + Quaternion.Euler(0, facing, 0) * Vector3.forward * 6f);
        }

        public static Ragnarok3dEffect Create(GameObject source, Vector3 destination)
        {
            var effect = RagnarokEffectPool.Get3dEffect(EffectType.ShieldBoomerang);
            var animation = new Animation(source.transform.position, destination);
            effect.SourceEntity = source.GetComponent<ServerControllable>();
            effect.AimTarget = source;
            effect.EffectData = animation;
            effect.transform.position = source.transform.position;
            // Allow the turn and fading trail to finish; bound the lifetime if the caster keeps moving.
            effect.SetDurationByTime((animation.Distance / TravelPerFrame + 180f) / 60f);
            effect.LaunchPrimitive(PrimitiveType.ShieldBoomerang,
                EffectSharedMaterialManager.GetMaterial(EffectMaterialType.ShieldBoomerang), effect.Duration);
            AudioManager.Instance.OneShotSoundEffect(effect.SourceEntityId, SoundName, source.transform.position);
            return effect;
        }

        public bool Update(Ragnarok3dEffect effect, float pos, int step)
        {
            var animation = (Animation)effect.EffectData;
            var source = effect.AimTarget != null ? effect.AimTarget.transform.position : animation.Origin;
            var frame = Mathf.FloorToInt(Mathf.Min(pos, effect.Duration) * 60f);
            while (animation.Frame < frame)
                animation.Advance(source);

            if (pos >= effect.Duration)
                return false;
            foreach (var image in animation.Images)
                if (image.Alpha > 0)
                    return true;
            return false;
        }

        public void OnCleanup(Ragnarok3dEffect effect)
        {
            effect.SourceEntity = null;
        }
    }
}
