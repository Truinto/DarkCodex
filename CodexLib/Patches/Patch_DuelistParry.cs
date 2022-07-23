using CodexLib;
using HarmonyLib;
using Kingmaker;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.FactLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib.Patches
{
    [HarmonyPatch]
    public class Patch_DuelistParry
    {
        [HarmonyPatch(typeof(DuelistParry), nameof(DuelistParry.OnEventDidTrigger), typeof(RuleAttackRoll))]
        [HarmonyPrefix]
        public static bool Prefix(RuleAttackRoll evt, DuelistParry __instance)
        {
            int num;
            var parry = evt.Parry;
            var owner = __instance.Owner;

            if (parry == null || parry.Initiator != owner || !parry.IsTriggered)
                return false;

            if (evt.Result == AttackResult.Parried && owner.State.Features.DuelistRiposte)
                Game.Instance.CombatEngagementController.ForceAttackOfOpportunity(owner, evt.Initiator, false);

            if (owner.HasFlag(MechanicFeature.ParryUseAttackOfOpportunity) && (num = owner.CombatState.AttackOfOpportunityCount) > 0)
            {
                owner.CombatState.AttackOfOpportunityCount = num - 1;
                return false;
            }

            owner.RemoveFact(__instance.Fact);
            return false;
        }
    }
}
