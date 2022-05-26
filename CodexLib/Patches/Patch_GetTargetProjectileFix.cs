using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CodexLib.Patches
{
    /// <summary>
    /// Fixes AbilityDeliverTeleportTrample not returning woldPosition.
    /// </summary>
    [HarmonyPatch]
    public class Patch_GetTargetProjectileFix
    {
        [HarmonyPatch(typeof(ClickWithSelectedAbilityHandler), nameof(ClickWithSelectedAbilityHandler.GetTarget))]
        [HarmonyPrefix]
        public static bool Prefix(GameObject gameObject, Vector3 worldPosition, AbilityData ability, ref TargetWrapper __result)
        {
            if (ability == null || ability.Blueprint.GetComponent<AbilityDeliverTeleportTrample>() == null)
                return true;

            UnitEntityData targetUnit = gameObject?.GetComponentNonAlloc<UnitEntityView>()?.EntityData;
            Vector3 target = targetUnit?.Position ?? worldPosition;
            Vector3 origin = target - ability.Caster.Unit.Position;
            Quaternion quaternion = (origin != Vector3.zero) ? Quaternion.LookRotation(origin) : Quaternion.identity;
            float orientation = quaternion.eulerAngles.y;
            __result = new TargetWrapper(target, orientation, targetUnit);
            return false;
        }
    }
}
