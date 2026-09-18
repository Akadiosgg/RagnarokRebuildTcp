using Assets.Scripts.Network;
using RebuildSharedData.Enum;

namespace Assets.Scripts.SkillHandlers.Handlers.Crusader
{
    [SkillHandler(CharacterSkill.ShieldCharge, true)]
    public class ShieldChargeHandler : SkillHandlerBase
    {
        public override void ExecuteSkillTargeted(ServerControllable src, ref AttackResultData attack)
        {
            src?.PerformBasicAttackMotion();
            var target = attack.Target;
            if (target == null)
                return;

            CameraFollower.Instance.AttachEffectToEntity("ShieldCharge", target.gameObject, target.Id);
        }
    }
}
