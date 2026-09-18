using Assets.Scripts.Effects.EffectHandlers.General;
using Assets.Scripts.Network;
using RebuildSharedData.Enum;

namespace Assets.Scripts.SkillHandlers.Handlers.Crusader
{
    [SkillHandler(CharacterSkill.AutoGuard)]
    public class AutoGuardHandler : SkillHandlerBase
    {
        public override void ExecuteSkillTargeted(ServerControllable src, ref AttackResultData attack)
        {
            src?.PerformSkillMotion();
            if (src != null && src.StatusEffectState?.HasStatusEffect(CharacterStatusEffect.AutoGuard) == true)
                GuardEffect.Create(src.gameObject);
        }
    }
}
