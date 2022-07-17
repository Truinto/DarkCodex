using HarmonyLib;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony | Severity.DefaultOff, "Zippy Spell-Like", "allows zippy to work on spell-like abilities", true)]
    [HarmonyPatch(typeof(DublicateSpellComponent), "Kingmaker.PubSubSystem.IRulebookHandler<Kingmaker.RuleSystem.Rules.Abilities.RuleCastSpell>.OnEventDidTrigger")]
    public class Patch_ZippySpellLike
    {
        public static bool Prefix(RuleCastSpell evt, DublicateSpellComponent __instance)
        {
            if (evt.IsDuplicateSpellApplied 
                || !evt.Success 
                || (evt.Spell.Blueprint.Type != AbilityType.Spell && evt.Spell.Blueprint.Type != AbilityType.SpellLike)
                || !__instance.CheckAOE(evt.Spell))
                return false;

            AbilityData spell = evt.Spell;
            UnitEntityData newTarget = __instance.GetNewTarget(spell, evt.SpellTarget.Unit);
            if (newTarget == null)
                return false;

            Rulebook.Trigger(new RuleCastSpell(spell, newTarget) { IsDuplicateSpellApplied = true });
            return false;
        }
    }
}
