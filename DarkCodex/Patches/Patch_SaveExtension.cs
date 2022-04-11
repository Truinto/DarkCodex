using HarmonyLib;
using Kingmaker;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.UI.SaveLoadWindow;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    [HarmonyPatch]
    public class Patch_SaveExtension
    {
        public const string SaveKey = "DarkCodex-Patches";

        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.SaveRoutine))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspilerSave(IEnumerable<CodeInstruction> instr)
        {
            var line = instr.ToList();
            var original = AccessTools.Method(typeof(ISaver), nameof(ISaver.SaveJson));

            for (int i = 0; i < line.Count; i++)
            {
                if (line[i].Calls(original))
                {
                    line.Insert(++i, new CodeInstruction(OpCodes.Ldarg_0));
                    line.Insert(++i, CodeInstruction.Call(typeof(Patch_SaveExtension), nameof(Patch_SaveExtension.OnSave2)));
                    Helper.PrintDebug("Patched at " + i);
                    return line;
                }
            }

            Helper.PrintError("Did not patch TranspilerSave");
            return instr;
        }
        public static void OnSave2(SaveInfo saveInfo)
        {
            try
            {
                if (Main.patchInfos == null)
                    return;

                Main.patchInfos.Update();
                saveInfo.Saver.SaveJson(SaveKey, Helper.Serialize(Main.patchInfos.GetCriticalPatches()));
            }
            catch (Exception e)
            {
                Helper.PrintException(e);
            }
        }

        //[HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LoadRoutine))]
        //[HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspilerLoad(IEnumerable<CodeInstruction> instr)
        {
            var line = instr.ToList();
            var original = AccessTools.Method(typeof(ISaver), nameof(ISaver.ReadJson));

            for (int i = 0; i < line.Count; i++)
            {
                if (line[i].Calls(original))
                {
                    line.Insert(++i, new CodeInstruction(OpCodes.Ldarg_0));
                    line.Insert(++i, CodeInstruction.Call(typeof(Patch_SaveExtension), nameof(Patch_SaveExtension.OnLoad2)));
                    Helper.PrintDebug("Patched at " + i);
                    break;
                }
            }

            return line;
        }
        public static void OnLoad2(SaveInfo saveInfo)
        {
            // no use
        }

        [HarmonyPatch(typeof(SaveSlot), nameof(SaveSlot.OnButtonSaveLoad))]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyPrefix]
        public static bool BeforeLoad2(SaveSlot __instance)
        {
            try
            {
                if (Main.patchInfos == null)
                    return true;

                if (Main.restart)
                {
                    Helper.ShowMessageBox("Settings were changed. Restart game to apply new patches.", yesLabel: "Ignore this time",
                        onYes: () =>
                        {
                            Main.restart = false;
                        });
                    return false;
                }

                var saveInfo = __instance.SaveInfo;
                if (saveInfo?.Saver == null)
                    return true;

                string json = saveInfo.Saver.ReadJson(SaveKey);
                if (json == null || json == "")
                    return true;

                var saveData = Helper.Deserialize<IEnumerable<string>>(json);
                if (saveData == null)
                    return true;

                var mustEnable = Main.patchInfos.IsEnabledAll(saveData);// TODO: check data
                if (mustEnable.Count() > 0)
                {
                    Helper.ShowMessageBox("Critical patch missing! Either turn off 'Save Metadata' or press 'Enable' to close the game and enable: " + mustEnable.Join(), yesLabel: "Enable",
                        onYes: () =>
                        {
                            foreach (var info in mustEnable)
                                Main.patchInfos.SetEnable(true, info);
                            Main.OnSaveGUI(null);
                            SystemUtil.ApplicationQuit();
                            //SystemUtil.ApplicationRestart();
                        });
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                Helper.PrintException(e);
                return true;
            }
        }

        // --------- variant which saves outside the zks

        //[HarmonyPatch(typeof(SaveManager), nameof(SaveManager.PrepareSave))]
        //[HarmonyPostfix]
        public static void OnSave(SaveInfo saveInfo)
        {
            if ((int)saveInfo.Type > 2)
                return;

            string path = saveInfo.FolderName.TrySubstring('.', -1, true);
            if (path == null)
                return;
            path += ".json";

            var data = new EntityPartKeyValueStorage().GetStorage("DarkCodex-Patches"); // TODO: get data

            foreach (var p in Main.patchInfos)
            {
                if (p.IsDangerous && !p.Disabled && !p.DisabledAll)
                    data[p.FullName] = "true";
            }

            Helper.Serialize(data, path: path);
        }

        //[HarmonyPatch(typeof(SaveManager), nameof(SaveManager.DeleteSave))]
        //[HarmonyPostfix]
        public static void OnDelete(SaveInfo saveInfo)
        {
            string path = saveInfo.FolderName.TrySubstring('.', -1, true);
            if (path == null)
                return;
            path += ".json";

            Helper.TryDelete(path);
        }

        //[HarmonyPatch(typeof(SaveSlot), nameof(SaveSlot.OnButtonSaveLoad))]
        //[HarmonyPriority(Priority.HigherThanNormal)]
        //[HarmonyPrefix]
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
