using HarmonyLib;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Shared;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony | Severity.DefaultOff, "Dismiss Anything", "dismiss any spell regardless of who the caster is", true)]
    [HarmonyPatch(typeof(AbilityTargetIsAreaEffectFromCaster), nameof(AbilityTargetIsAreaEffectFromCaster.GetSuitableAreaEffect))]
    public class Patch_DismissAnything
    {
        public static bool Prefix(UnitEntityData caster, Vector3 target, ref AreaEffectEntityData __result)
        {
            if (caster == null)
                return true;

            foreach (var areaEffect in Game.Instance.State.AreaEffects)
            {
                if (areaEffect.Context.ParentContext is AbilityExecutionContext && areaEffect.View.Contains(target))
                {
                    __result = areaEffect;
                    return false;
                }
            }

            __result = null;
            return false;
        }
    }
}
