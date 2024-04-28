﻿using RebuildSharedData.Data;
using RebuildSharedData.Enum;
using RoRebuildServer.EntityComponents;

namespace RoRebuildServer.Simulation.Skills.SkillHandlers
{
    [SkillHandler(CharacterSkill.None, SkillClass.None, SkillTarget.SelfCast)]
    public class SkillHandlerNone : SkillHandlerBase
    {
        public override void Process(CombatEntity source, CombatEntity? target, Position position, int lvl)
        {
            //do nothing!
        }
    }
}
