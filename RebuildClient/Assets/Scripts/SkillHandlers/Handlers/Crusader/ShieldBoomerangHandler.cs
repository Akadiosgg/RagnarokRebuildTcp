using Assets.Scripts.Effects.EffectHandlers.Skills.Crusader;
using Assets.Scripts.Network;
using RebuildSharedData.Enum;

namespace Assets.Scripts.SkillHandlers.Handlers.Crusader
{
    [SkillHandler(CharacterSkill.ShieldBoomerang)]
    public class ShieldBoomerangHandler : SkillHandlerBase
    {
        public override bool DoesAttackTakeWeaponSound => false;

        public override void ExecuteSkillTargeted(ServerControllable src, ref AttackResultData attack)
        {
            src.LookAtOrDefault(attack.Target);
            src.PerformThrowMotion();
            if (attack.Target != null)
                ShieldBoomerangEffect.Create(src.gameObject, attack.Target.transform.position);
        }
    }
}
