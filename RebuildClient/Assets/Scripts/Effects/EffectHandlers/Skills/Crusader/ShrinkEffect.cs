using Assets.Scripts.Network;
using Assets.Scripts.Objects;
using Assets.Scripts.Sprites;
using UnityEngine;

namespace Assets.Scripts.Effects.EffectHandlers.Skills.Crusader
{
    [RoEffect("Shrink")]
    public class ShrinkEffect : IEffectHandler
    {
        public const int DurationFrames = 26;
        private static readonly Color FlashColor = new Color32(250, 250, 100, 255);

        public static Ragnarok3dEffect Create(GameObject target)
        {
            var effect = RagnarokEffectPool.Get3dEffect(EffectType.Shrink);
            effect.SetDurationByTime(DurationFrames / 60f);
            effect.FollowTarget = target;
            effect.DestroyOnTargetLost = true;
            effect.SourceEntity = target.GetComponent<ServerControllable>();
            effect.transform.position = target.transform.position;
            var sprite = effect.SourceEntity != null ? effect.SourceEntity.SpriteAnimator
                : target.GetComponentInChildren<RoSpriteAnimator>();
            effect.EffectData = sprite;
            if (sprite != null)
                sprite.BeginEffectTint(effect);
            return effect;
        }

        public bool Update(Ragnarok3dEffect effect, float pos, int step)
        {
            if (effect.FollowTarget == null || !effect.FollowTarget.activeInHierarchy || pos >= effect.Duration)
                return false;

            var frame = Mathf.FloorToInt(pos * 60f);
            if (frame >= 5 && effect.Flags[0] == 0)
            {
                effect.Flags[0] = 1;
                AudioManager.Instance.OneShotSoundEffect(effect.SourceEntityId, "ef_beginspell.ogg",
                    effect.FollowTarget.transform.position);
            }

            if (effect.EffectData is RoSpriteAnimator sprite && sprite != null)
            {
                // Native effect 599 alternates yellow/white on frames 5 through 25.
                // Sampling elapsed time preserves the timing when render frames are skipped.
                var tint = frame >= 5 && frame <= 25 && frame % 2 == 0 ? FlashColor : Color.white;
                sprite.SetEffectTint(effect, tint);
            }
            return true;
        }

        public void OnCleanup(Ragnarok3dEffect effect)
        {
            if (effect.EffectData is RoSpriteAnimator sprite && sprite != null)
                sprite.ClearEffectTint(effect);
            effect.SourceEntity = null;
        }
    }
}
