using HarmonyLib;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.UI.SaveLoadWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    [HarmonyPatch]
    public class Patch_SaveRescue
    {
        [HarmonyPatch(typeof(BlueprintConverter), nameof(BlueprintConverter.ReadJson))]
        public static void X()
        {

        }

        [HarmonyPatch(typeof(SaveSlot), nameof(SaveSlot.OnButtonSaveLoad))]
        [HarmonyPrefix]
        public static void BeforeLoad2(SaveSlot __instance)
        {
            try
            {
                var saveInfo = __instance.SaveInfo;
                var saver = saveInfo.Saver as ZipSaver;

                foreach (var file in saver.GetAllFiles())
                {
                    if (file == null || !file.EndsWith(".json"))
                        continue;

                    using var sw = saver.ReadJsonStream(file);
                }
            }
            catch (Exception e)
            {
                Helper.PrintException(e);
            }

        }
    }
}
