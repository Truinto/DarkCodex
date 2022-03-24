using HarmonyLib;
using Kingmaker;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony | Severity.Event | Severity.WIP, "Patch: Fix Area Effects", "enforces a 6 second cooldown on area effects")]
    [HarmonyPatch(typeof(AbilityAreaEffectRunAction))]
    public class Unused_Patch_FixAreaEffectDamage : IPartyCombatHandler, IGlobalSubscriber, ISubscriber
    {
        //base.Data.LastUseTime + 1.Rounds().Seconds > Game.Instance.TimeController.GameTime
        public static Dictionary<AbilityAreaEffectRunAction, Dictionary<UnitEntityData, TimeSpan>> times = new();

        [HarmonyPatch(nameof(AbilityAreaEffectRunAction.OnUnitEnter))]
        [HarmonyPostfix]
        public static void OnUnitEnter(MechanicsContext context, AreaEffectEntityData areaEffect, UnitEntityData unit, AbilityAreaEffectRunAction __instance)
        {
            Helper.PrintDebug($"AreaRunAction OnEnter unit={unit.CharacterName} area={areaEffect.Blueprint.name}");

            if (!__instance.UnitEnter.HasActions || !__instance.Round.HasActions)
                return;

            if (context.SourceAbility?.EffectOnAlly == AbilityEffectOnUnit.Helpful)
                return;

            if (!times.TryGetValue(__instance, out var units))
                times.Add(__instance, units = new());
            units[unit] = Game.Instance.TimeController.GameTime;
        }

        [HarmonyPatch(nameof(AbilityAreaEffectRunAction.OnRound))]
        [HarmonyPrefix]
        public static bool OnRound(MechanicsContext context, AreaEffectEntityData areaEffect, AbilityAreaEffectRunAction __instance)
        {
            if (!__instance.Round.HasActions)
                return false;

            using (ContextData<AreaEffectContextData>.Request().Setup(areaEffect))
            {
                var nowminus = Game.Instance.TimeController.GameTime - 1.Rounds().Seconds;

                foreach (UnitEntityData unit in areaEffect.InGameUnitsInside)
                {
                    if (times.TryGetValue(__instance, out var units) && units.TryGetValue(unit, out var lastuse) && lastuse > nowminus)
                    {
                        Helper.PrintDebug($"skipped OnRound unit={unit.CharacterName} lastuse={lastuse} nowminus={nowminus} area={areaEffect.Blueprint.name}");
                        continue;
                    }

                    using (context.GetDataScope(unit))
                    {
                        Helper.PrintDebug($"AreaRunAction OnRound unit={unit.CharacterName} nowminus={nowminus} area={areaEffect.Blueprint.name}");
                        __instance.Round.Run();
                    }
                }
            }
            return false;
        }

        public void HandlePartyCombatStateChanged(bool inCombat)
        {
            if (!inCombat)
                times.Clear();
        }
    }
}
