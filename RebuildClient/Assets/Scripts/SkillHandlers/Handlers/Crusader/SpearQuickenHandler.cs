using Assets.Scripts.Network;
using RebuildSharedData.Enum;

namespace Assets.Scripts.SkillHandlers.Handlers.Crusader
{
    [SkillHandler(CharacterSkill.SpearQuicken)]
    public class SpearQuickenHandler : SkillHandlerBase
    {
        public override void ExecuteSkillTargeted(ServerControllable src, ref AttackResultData attack)
        {
            if (!attack.IsIndirect && src.CharacterType != CharacterType.Monster)
                src.PerformSkillMotion();
            CameraFollower.Instance.AttachEffectToEntity("SpearQuicken", src.gameObject, src.Id);
        }
    }
}
