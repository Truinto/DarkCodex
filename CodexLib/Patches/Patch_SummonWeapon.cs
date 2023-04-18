using Kingmaker.Blueprints.Items.Weapons;

namespace CodexLib.Patches
{
    /// <inheritdoc cref="SummonWeaponLogic"/>
    [HarmonyPatch]
    public class Patch_SummonWeapon
    {
        // Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon.AttackType
        //  -> patch: check for component to return different value
        // Kingmaker.UI.Common.UIUtility.GetDamageBonus (low prio)
        //  -> patch return value

        /// <summary>
        /// Force touch attack, regardless of original weapon type.
        /// </summary>
        [HarmonyPatch(typeof(BlueprintItemWeapon), nameof(BlueprintItemWeapon.AttackType), MethodType.Getter)]
        [HarmonyPostfix]
        public static void ForceTouchAttack(BlueprintItemWeapon __instance, ref AttackType __result)
        {
            if (__instance.m_Enchantments.FirstOrDefault()?.Get()?.Components?.FirstOrDefault()?.GetType() == typeof(SummonWeaponLogic))
                __result = AttackType.Touch;
        }

        /// <summary>
        /// Ensure visual effect are spawned.
        /// </summary>
        [HarmonyPatch(typeof(ItemEntityWeapon), nameof(ItemEntityWeapon.OnDidEquipped))]
        [HarmonyPostfix]
        public static void SpawnSfx(ItemEntityWeapon __instance)
        {
            try
            {
                __instance.SpawnOverridenVisualFx();
            }
            catch (Exception ex) { Helper.PrintException(ex); }
        }
    }
}
