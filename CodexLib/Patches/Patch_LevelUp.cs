using Kingmaker.UnitLogic.Class.LevelUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib.Patches
{
    public class Patch_LevelUp
    {
        [HarmonyPatch(typeof(LevelUpController), nameof(LevelUpController.Start))]
        public static class LevelUpController_StartPatch
        {
            public static void Prefix(UnitDescriptor unit)
            {
                EventBus.RaiseEvent(unit.Unit, (IBeforeLevelUpHandler h) => h.BeforeLevelUp());
            }
        }
    }
}
