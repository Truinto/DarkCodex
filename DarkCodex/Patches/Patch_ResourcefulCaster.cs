using HarmonyLib;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using System;
using System.Collections.Generic;
using Shared;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Resourceful Caster", "patches for Resourceful Caster", true)]
    [HarmonyPatch]
    public class Patch_ResourcefulCaster
    {
        [HarmonyPatch(typeof(RuleCastSpell), nameof(RuleCastSpell.ShouldSpendResource), MethodType.Getter)]
        [HarmonyPostfix]
        public static void Postfix1(RuleCastSpell __instance, ref bool __result) // arcane spell failure
        {
            try
            {
                if (__result == false)
                    return;

                if (__instance.Initiator == null)
                    return;

                if (!__instance.IsSpellFailed && !__instance.IsArcaneSpellFailed)
                    return;

                if (__instance.Initiator.Descriptor.HasFact(Resource.Cache.FeatureResourcefulCaster))
                    __result = false;
            }
            catch (Exception)
            {
            }
        }

        [HarmonyPatch(typeof(UnitUseAbility), nameof(UnitUseAbility.FailIfConcentrationCheckFailed))]
        [HarmonyPrefix]
        public static bool Prefix2(UnitUseAbility __instance, ref bool __result) // concentration failed
        {
            try
            {
                if (__instance.Executor == null
                    || !__instance.Executor.Descriptor.HasFact(Resource.Cache.FeatureResourcefulCaster))
                    return true;

                if (__instance.ConcentrationCheckFailed)
                {
                    __instance.SpawnInterruptFx();
                    __instance.ForceFinish(UnitCommand.ResultType.Fail);
                    if (__instance.AiAction != null)
                        __instance.Executor.CombatState.AIData.UseAction(__instance.AiAction, __instance);
                }
                __result = __instance.ConcentrationCheckFailed;
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        public static List<UnitEntityData> unitsSpellNotResisted = new();
        [HarmonyPatch(typeof(AbilityExecutionProcess), nameof(AbilityExecutionProcess.Tick))]
        [HarmonyPostfix]
        public static void Postfix3(AbilityExecutionProcess __instance) // all targets resisted
        {
            if (!__instance.IsEnded)
                return;

            try
            {
                if (__instance.Context?.MaybeCaster == null)
                    return;

                Helper.PrintDebug($"Cast complete {__instance.Context.AbilityBlueprint.name} from {__instance.Context.MaybeCaster.CharacterName}");
                if (!__instance.Context.MaybeCaster.Descriptor.HasFact(Resource.Cache.FeatureResourcefulCaster))
                    return;
                if (__instance.Context.IsDuplicateSpellApplied)
                    return;
                var spell = __instance.Context.Ability;
                if (spell == null)
                    return;
                var spellbook = spell.Spellbook;
                if (spellbook == null)
                    return;
                if (__instance.Context.RulebookContext == null)
                    return;

                bool hasSaves = false;
                bool allSavesPassed = true;
                foreach (var rule in __instance.Context.RulebookContext.AllEvents)
                {
                    if (rule is RuleSpellResistanceCheck resistance)
                    {
                        Helper.PrintDebug($" -SR {resistance.Target} {resistance.IsSpellResisted}");
                        hasSaves = true;
                        if (!resistance.IsSpellResisted)
                            unitsSpellNotResisted.Add(resistance.Target);
                    }

                    else if (rule is RuleSavingThrow save) // this works if Context.TriggerRule is used
                    {
                        Helper.PrintDebug($" -{save.Type} Save {save.Initiator} {save.IsPassed}");
                        hasSaves = true;
                        if (save.IsPassed)
                            unitsSpellNotResisted.Remove(save.Initiator);
                        else
                        {
                            allSavesPassed = false;
                            break;
                        }
                    }

                    else if (rule is RuleDispelMagic dispel)
                    {
                        Helper.PrintDebug($" -Dispel {dispel.Buff?.Blueprint?.name} {dispel.AreaEffect?.Blueprint?.name} {dispel.Success}");
                        hasSaves = true;
                        if (dispel.Success)
                        {
                            allSavesPassed = false;
                            break;
                        }    
                    }

                    //else Helper.PrintDebug(" -" + rule.GetType().FullName);
                }

                if (hasSaves && allSavesPassed && unitsSpellNotResisted.Count == 0)
                {
                    // refund spell if all targets resisted
                    spell = spell.ConvertedFrom ?? spell;

                    Helper.PrintDebug("Refunding spell");
                    Helper.PrintCombatLog($"{__instance.Context.Caster.CharacterName} regained {spell.Name}");

                    int level = spellbook.GetSpellLevel(spell);
                    if (spellbook.Blueprint.Spontaneous)
                    {
                        if (level > 0)
                            spellbook.m_SpontaneousSlots[level]++;
                    }
                    else
                    {
                        foreach (var slot in spellbook.m_MemorizedSpells[level])
                        {
                            if (!slot.Available && slot.Spell == spell)
                            {
                                slot.Available = true;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                unitsSpellNotResisted.Clear();
            }
        }

        [HarmonyPatch(typeof(RuleSpellResistanceCheck), nameof(RuleSpellResistanceCheck.OnTrigger))]
        [HarmonyPostfix]
        public static void Postfix4(RuleSpellResistanceCheck __instance) // push SR checks into history
        {
            try
            {
                __instance.Context?.SourceAbilityContext?.RulebookContext?.m_AllEvents?.Add(__instance);
                Helper.PrintDebug("added SR check to stack");
            }
            catch (Exception)
            {
            }
        }
    }
}
