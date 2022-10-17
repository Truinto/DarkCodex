using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    [HarmonyPatch]
    public class Patch_BladedBrush
    {
        [HarmonyPatch(typeof(UnitPartMagus), nameof(UnitPartMagus.IsOneHandedWeapon))]
        [HarmonyPostfix]
        public static void Postfix1(ItemEntityWeapon weapon, ref bool __result)
        {
            var owner = weapon.Owner;
            if (owner == null)
                return;

            if (!__result)
                __result = owner.Ensure<UnitPartDamageGrace>().HasEntry(weapon.Blueprint.Category);
        }

        [HarmonyPatch(typeof(DuelistParry), nameof(DuelistParry.GetSuitableWeapon))]
        [HarmonyPrefix]
        public static bool Prefix2(DuelistParry __instance, ref HandSlot __result)
        {
            if (IsSuitable(__instance.Owner, __instance.Owner.Body.PrimaryHand.MaybeWeapon))
                __result = __instance.Owner.Body.PrimaryHand;
            return false;
        }

        [HarmonyPatch(typeof(DeflectArrows), nameof(DeflectArrows.IsOneHandedMeleePiercing))]
        [HarmonyPrefix]
        public static bool Prefix3(DeflectArrows __instance, HandSlot hand, ref bool __result)
        {
            __result = IsSuitable(__instance.Owner, hand.MaybeWeapon);
            return false;
        }

        public static bool IsSuitable(UnitEntityData owner, ItemEntityWeapon weapon)
        {
            if (owner == null || weapon == null)
                return false;

            if (weapon.Blueprint.DamageType.Type != DamageType.Physical)
                return false;

            bool isGrace = owner.Descriptor.Ensure<UnitPartDamageGrace>().HasEntry(weapon.Blueprint.Category);
            if (!isGrace && weapon.Blueprint.IsTwoHanded)
                return false;

            if (!weapon.Blueprint.DamageType.Physical.Form.HasFlag(PhysicalDamageForm.Piercing)
                && (weapon.Blueprint.Category != WeaponCategory.DuelingSword || !owner.State.Features.DuelingMastery)
                && !isGrace)
                return false;

            return true;
        }
    }
}
