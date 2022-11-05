using Kingmaker.Designers.Mechanics.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    [HarmonyPatch]
    public class Patch_MonkRapidShot
    {
        [HarmonyPatch(typeof(MonkNoArmorAndMonkWeaponFeatureUnlock), nameof(MonkNoArmorAndMonkWeaponFeatureUnlock.CheckEligibility))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler1(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var data = new TranspilerData(instructions, generator, original);

            data.Last().Rewind(typeof(HandSlot), nameof(HandSlot.HasShield));
            data.Seek(OpCodes.And);
            data.InsertAfter(Patch);
            data.InsertAfter(OpCodes.And);

            return data.Code;
        }

        public static bool Patch(MonkNoArmorAndMonkWeaponFeatureUnlock __instance)
        {
            return !__instance.Owner.Descriptor.HasFact(RapidShot);
        }

        public static BlueprintBuff RapidShot = Helper.ToRef<BlueprintBuffReference>("0f310c1e709e15e4fa693db15a4baeb4"); //RapidShotBuff
    }
}
