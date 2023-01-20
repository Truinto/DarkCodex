using Kingmaker.Blueprints.Items.Weapons;

namespace CodexLib.Patches
{
    [HarmonyPatch]
    public class Patch_FlameBlade
    {
        [HarmonyPatch(typeof(BlueprintItemWeapon), nameof(BlueprintItemWeapon.AttackType), MethodType.Getter)]
        [HarmonyPostfix]
        public static void Postfix1(BlueprintItemWeapon __instance, ref AttackType __result)
        {
            if (__instance.m_Enchantments.FirstOrDefault()?.Get()?.Components?.FirstOrDefault()?.GetType() == typeof(FlameBladeLogic))
                __result = AttackType.Touch;
        }

        [HarmonyPatch(typeof(ItemEntityWeapon), nameof(ItemEntityWeapon.OnDidEquipped))]
        [HarmonyPostfix]
        public static void Postfix2(ItemEntityWeapon __instance)
        {
            try
            {
                __instance.SpawnOverridenVisualFx();
            }
            catch (Exception ex) { Helper.PrintException(ex); }
        }
    }
}
