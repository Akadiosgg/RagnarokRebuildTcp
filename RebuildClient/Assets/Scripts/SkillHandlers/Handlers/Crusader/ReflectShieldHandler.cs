using Assets.Scripts.Effects.EffectHandlers.Skills.Crusader;
using Assets.Scripts.Network;
using RebuildSharedData.Enum;

namespace Assets.Scripts.SkillHandlers.Handlers.Crusader
{
    [SkillHandler(CharacterSkill.ReflectShield)]
    public class ReflectShieldHandler : SkillHandlerBase
    {
        public override bool DoesAttackTakeWeaponSound => false;

        public override void ExecuteSkillTargeted(ServerControllable src, ref AttackResultData attack)
        {
            src.PerformSkillMotion();
            ReflectShieldEffect.Create(src.gameObject);
        }
    }
}
