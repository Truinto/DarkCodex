using HarmonyLib;
using Kingmaker;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.UI.SaveLoadWindow;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex.Patches
{
    [HarmonyPatch]
    public class Patch_SaveExtension
    {
        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.PrepareSave))]
        [HarmonyPostfix]
        public static void OnSave(SaveInfo saveInfo)
        {
            if ((int)saveInfo.Type > 2)
                return;

            string path = saveInfo.FolderName.TrySubstring('.', -1, true);
            if (path == null)
                return;
            path += ".json";

            var data = new EntityPartKeyValueStorage(); // TODO: get data

            foreach (var info in Main.patchInfos)
            {
                if (info.)
            }

            Helper.Serialize(data, path: path);
        }

        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.DeleteSave))]
        [HarmonyPostfix]
        public static void OnDelete(SaveInfo saveInfo)
        {
            string path = saveInfo.FolderName.TrySubstring('.', -1, true);
            if (path == null)
                return;
            path += ".json";

            Helper.TryDelete(path);
        }

        [HarmonyPatch(typeof(SaveSlot), nameof(SaveSlot.OnButtonSaveLoad))]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyPrefix]
        public static bool BeforeLoad(SaveSlot __instance)
        {
            var saveInfo = __instance.SaveInfo;
            string path = saveInfo.FolderName.TrySubstring('.', -1, true);
            if (path == null)
                return true;
            path += ".json";

            var data = Helper.TryDeserialize<EntityPartKeyValueStorage>(path: path);
            if (data == null)
                return true;

            var dic = data.GetStorage("DarkCodex-Patches");
            if (dic == null)
                return true;

            foreach (string p in dic.Keys)
            {
            }

            // TODO: check data
            if (false)
            {
                string patches = null;
                Helper.ShowMessageBox("Critical patch missing! Either turn off 'Save Metadata' or press 'Enable' to close the game and enable: " + patches, yesLabel: "Enable", 
                    onYes: () => 
                    {
                        SystemUtil.ApplicationQuit();
                    });
                return false;
            }

            return true;
        }
    }
}
