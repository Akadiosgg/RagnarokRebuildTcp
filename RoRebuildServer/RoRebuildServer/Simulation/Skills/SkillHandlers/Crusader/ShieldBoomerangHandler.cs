using RebuildSharedData.Data;
using RebuildSharedData.Enum;
using RebuildSharedData.Enum.EntityStats;
using RoRebuildServer.Data;
using RoRebuildServer.EntityComponents;
using RoRebuildServer.EntityComponents.Util;
using RoRebuildServer.Networking;
using RoRebuildServer.Simulation.Util;

namespace RoRebuildServer.Simulation.Skills.SkillHandlers.Crusader;

[SkillHandler(CharacterSkill.ShieldBoomerang, SkillClass.Physical, SkillTarget.Enemy)]
public class ShieldBoomerangHandler : SkillHandlerBase
{
    public override int GetSkillRange(CombatEntity source, int lvl) => 1 + lvl.Clamp(1, 5) * 2;

    public override SkillValidationResult ValidateTarget(CombatEntity source, CombatEntity? target, Position position,
        int lvl, bool isIndirect, bool isItemSource)
    {
        if (!source.HasShieldEquipped)
            return SkillValidationResult.IncorrectWeapon;

        return base.ValidateTarget(source, target, position, lvl, isIndirect, isItemSource);
    }

    public override void Process(CombatEntity source, CombatEntity? target, Position position, int lvl,
        bool isIndirect, bool isItemSource)
    {
        if (target == null || !target.IsValidTarget(source))
            return;

        lvl = lvl.Clamp(1, 5);
        var req = new AttackRequest(CharacterSkill.ShieldBoomerang, 1f + lvl * 0.3f, 1,
            AttackFlags.Physical | AttackFlags.Ranged | AttackFlags.IgnoreWeaponBonuses, AttackElement.Neutral);

        if (source.Character.Type == CharacterType.Player)
        {
            var str = source.GetEffectiveStat(CharacterStat.Str);
            var dex = source.GetEffectiveStat(CharacterStat.Dex);
            var attack = source.GetStat(CharacterStat.AddAttackPower) + str + dex / 5 + (str / 10) * (str / 10);
            var shieldId = source.Player.GetItemIdForEquipSlot(EquipSlot.Shield);
            if (DataManager.ItemList.TryGetValue(shieldId, out var shield))
                attack += shield.Weight / 10;
            var bagId = source.Player.Equipment.ItemSlots[(int)EquipSlot.Shield];
            if (source.Player.Inventory != null && source.Player.Inventory.UniqueItems.TryGetValue(bagId, out var item))
                attack += item.Refine * 4;

            req.MinAtk = req.MaxAtk = Math.Max(1, attack * (100 + source.GetStat(CharacterStat.AddAttackPercent)) / 100);
        }

        var res = source.CalculateCombatResult(target, req);
        res.AttackMotionTime = 0.2f;
        // ShieldBoomerangEffect advances 0.4 world units per frame at 60 frames per second.
        res.Time = Time.ElapsedTimeFloat + source.Character.WorldPosition.DistanceTo(target.Character.WorldPosition) / 24f;
        source.ApplyCooldownForSupportSkillAction();
        source.ApplyAfterCastDelay(0.7f);
        source.ExecuteCombatResult(res, false);
        CommandBuilder.SkillExecuteTargetedSkillAutoVis(source.Character, target.Character, CharacterSkill.ShieldBoomerang, lvl, res);
    }
}
