using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    //[HarmonyPatch]
    public class XXPatch
    {
        public IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(BlueprintExtenstions),
                nameof(BlueprintExtenstions.GetComponent),
                new Type[] { typeof(BlueprintScriptableObject) },
                new Type[] { typeof(SpellDescriptorComponent) }
                );

            yield return AccessTools.Method(typeof(BlueprintExtenstions),
                nameof(BlueprintExtenstions.GetComponents),
                new Type[] { typeof(BlueprintScriptableObject) },
                new Type[] { typeof(SpellDescriptorComponent) }
                );
        }

        public static bool Prefix(BlueprintScriptableObject blueprint, ref object __result, MethodBase __originalMethod)
        {
            var list = new List<SpellDescriptorComponent>();
            var change = default(ChangeSpellElementalDamage);
            var overwrite = default(SpellDescriptorComponent);
            if (blueprint != null)
            {
                for (int i = 0; i < blueprint.ComponentsArray.Length; i++)
                    if (blueprint.ComponentsArray[i] is SpellDescriptorComponent comp)
                        list.Add(comp);
            }

            if (__originalMethod.Name == "GetComponents")
            {
                __result = (IEnumerable<SpellDescriptorComponent>)list;
            }
            else if (list.Count > 0)
            {
                __result = (SpellDescriptorComponent)list[0];
                if (list.Count > 1)
                    Helper.PrintDebug($"{blueprint.name}:{blueprint.AssetGuid} has too many SpellDescriptorComponent");
            }

            return false;
        }
    }

    // remove, instead redo IncreaseSpellDescriptorDC, IncreaseSpellContextDescriptorDC, IncreaseSpellDescriptorCasterLevel, AddOutgoingDamageBonus
    public class ChangeSpellElementalDamageFix : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulePriority
    {
        public int Priority => 400;

        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }
    }
}
