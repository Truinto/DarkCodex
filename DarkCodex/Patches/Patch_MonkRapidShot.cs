using Kingmaker.Designers.Mechanics.Buffs;
using Shared;
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
            var data = new TranspilerTool(instructions, generator, original);

            data.Last().Rewind(typeof(HandSlot), nameof(HandSlot.HasShield));
            data.InsertAfter(Patch);

            return data.Code;
        }

        public static bool Patch(bool hasShield, MonkNoArmorAndMonkWeaponFeatureUnlock __instance)
        {
            return hasShield || __instance.Owner.Descriptor.HasFact(RapidShot);
        }

        public static BlueprintBuff RapidShot = Helper.ToRef<BlueprintBuffReference>("0f310c1e709e15e4fa693db15a4baeb4"); //RapidShotBuff
    }
}
