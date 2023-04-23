using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodexLib.Patches;
using Kingmaker.Enums;
using Kingmaker.EntitySystem.Persistence.JsonUtility;

namespace CodexLib
{
    /// <summary>
    /// If your mod uses components, run this code. Some components will not work properly without these patches.<br/>
    /// <br/>
    /// <code>MasterPatch.Run();</code>
    /// </summary>
    public static class MasterPatch
    {
        public static List<Type> PatchList = new()
        {
            //typeof(Event_AbilityEffectApplied),
            typeof(BpCache),
            typeof(Patch_AbilityAtWill),
            typeof(Patch_AbilityIsFullRound),
            typeof(Patch_ActionBarConvert),
            typeof(Patch_ActivatableActionBar),
            typeof(Patch_AOEAttackRolls),
            typeof(Patch_ConditionExemption),
            typeof(Patch_ContextStatValue),
            typeof(Patch_ContextValueParams),
            typeof(Patch_DebugReport),
            typeof(Patch_FixAbilityTargets),
            typeof(Patch_SummonWeapon),
            typeof(Patch_GetTargetProjectileFix),
            typeof(Patch_LevelUp),
            typeof(Patch_LocalizationChanged),
            typeof(Patch_MaterialComponent),
            typeof(Patch_MechanicsContextRecalculate),
            // TODO! fix metamagic ext; currently displayes everything twice
            //typeof(Patch_MetamagicExt),
            typeof(Patch_Prerequisite),
            typeof(Patch_ResourceOverride),
            typeof(Patch_RulebookEventBusPriority),
            typeof(Patch_RuleSpendCharge),
            typeof(Patch_SpellSelectionParametrized),
            typeof(Patch_SuppressError),
            typeof(Patch_TouchPersist),
            typeof(Patch_WeaponCategory),
        };

        /// <summary>
        /// Call at least once, if you plan to use components.
        /// </summary>
        public static void Run()
        {
            if (PatchList == null || PatchList.Count == 0)
                return;

            var harmony = Scope.Stack.First().harmony;
            foreach (var patch in PatchList)
                PatchSafe(harmony, patch);
            PatchList.Clear();
            PatchList = null;

            Helper.EnumCreateModifierDescriptor(Const.Intelligence, "Intelligence", "");
            Helper.EnumCreateModifierDescriptor(Const.Charisma, "Charisma", "");

            DefaultJsonSettings.DefaultSettings.Converters.Insert(0, new VariantSelectionDataConverter());
        }

        /// <summary>
        /// Use this if you want to enable only a specific patch. Ensures patch is only used once.
        /// </summary>
        public static bool Run(Type type)
        {
            if (PatchList == null)
                return false;

            bool contains = PatchList.Remove(type);
            if (contains)
                PatchSafe(Scope.Stack.First().harmony, type);
            return contains;
        }

        private static void PatchSafe(Harmony harmony, Type patch)
        {
            try
            {
                Scope.Stack.First().logger.Log("Patching " + patch.Name);

                if (patch.HasInterface(typeof(IGlobalSubscriber)))
                    EventBus.Subscribe(Activator.CreateInstance(patch));
                else
                    harmony.CreateClassProcessor(patch).Patch();
            }
            catch (Exception e)
            {
                Scope.Stack.First().logger.LogException(e);
            }
        }
    }
}
