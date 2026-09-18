using Assets.Scripts.Network;
using RebuildSharedData.Enum;

namespace Assets.Scripts.SkillHandlers.Handlers.Crusader
{
    [SkillHandler(CharacterSkill.HolyCross, true)]
    public class HolyCrossHandler : SkillHandlerBase
    {
        public override bool DoesAttackTakeWeaponSound => false;

        public override void ExecuteSkillTargeted(ServerControllable src, ref AttackResultData attack)
        {
            src?.PerformBasicAttackMotion();
            var target = attack.Target;
            if (target == null)
                return;

            CameraFollower.Instance.AttachEffectToEntity("HolyCross", target.gameObject, target.Id);
        }
    }
}
