using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.Kineticist.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using CodexLib;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Dark Elementalist Burn", "for Wild Talents your current amount of burn includes the number of successful Soul Power uses", true)]
    [HarmonyPatch(typeof(KineticistBurnPropertyGetter), nameof(KineticistBurnPropertyGetter.GetBaseValue))]
    public class Patch_DarkElementalistBurn
    {
        public static BlueprintBuffReference BuffSuccessSoulPower = Helper.ToRef<BlueprintBuffReference>("a64951a66c70813448c3a80b9b32949c"); //DarkElementalistSuccessfullSoulPowerBuff

        public static void Prepare()
        {
            var bp = BuffSuccessSoulPower.Get();
            if (bp != null)
                bp.Stacking = StackingType.Rank; // last checked 1.2.0A_2
        }

        public static void Postfix(UnitEntityData unit, ref int __result)
        {
            int rank = unit.Buffs.GetBuff(BuffSuccessSoulPower)?.GetRank() ?? 0;
            __result += rank;
        }
    }
}
