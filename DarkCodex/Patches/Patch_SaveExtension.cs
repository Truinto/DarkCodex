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
using Shared;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Save Metadata", "will add extra metadata of enabled patches and warn if you try to load a game with missing patches", false)] // TODO: rework
    [HarmonyPatch]
    public class Patch_SaveExtension
    {
        public const string SaveKey = "DarkCodex-Patches";

        //[HarmonyPatch(typeof(SaveManager), nameof(SaveManager.SaveRoutine))]
        //[HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspilerSave(IEnumerable<CodeInstruction> instr)
        {
            var line = instr.ToList();
            var original = AccessTools.Method(typeof(ISaver), nameof(ISaver.SaveJson));

            for (int i = 0; i < line.Count; i++)
            {
                Helper.PrintDebug($"i={i} {line[i]}");

                if (line[i].Calls(original))
                {
                    //line.Insert(++i, new CodeInstruction(OpCodes.Ldarg_0));
                    //line.Insert(++i, CodeInstruction.Call(typeof(Patch_SaveExtension), nameof(Patch_SaveExtension.OnSave2)));
                    //Helper.PrintDebug("Patched SaveRoutine at " + i);
                    //return line;
                }
            }

            Helper.PrintError("Did not patch TranspilerSave");
            return instr;
        }


        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.SerializeAndSaveThread))]
        [HarmonyPrefix]
        public static void OnSave2(SaveInfo saveInfo)
        {
            try
            {
                if (!Settings.State.saveMetadata || Main.patchInfos == null)
                    return;

                Main.patchInfos.Update();
                if (!Main.restart)
                    Main.patchInfoSaved = Main.patchInfos.GetCriticalPatches();

                if (Main.patchInfoSaved == null)
                    return;

                string json = Helper.Serialize(Main.patchInfoSaved, indent: false, type: false);
                if (json == null || json == "")
                    return;

                saveInfo.Saver.SaveBytes(SaveKey, Encoding.Default.GetBytes(json));
                Helper.Print("Saved patch metadata");
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
                    Helper.PrintDebug("Patched LoadRoutine at " + i);
                    return line;
                }
            }

            Helper.PrintError("Did not patch TranspilerSave");
            return instr;
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
                if (!Settings.State.saveMetadata)
                {
                    Main.patchInfoSaved = null;
                    return true;
                }

                if (Main.restart)
                {
                    Helper.ShowMessageBox("Settings were changed recently. Restart game to apply new patches.", yesLabel: "Ignore this time", noLabel: "I understand",
                        onYes: () =>
                        {
                            Main.restart = false;
                        });
                    return false;
                }

                Main.patchInfoSaved = null;

                if (Main.patchInfos == null)
                    return true;

                //(__instance.SaveInfo?.Saver as ZipSaver)?.ZipFile?.UpdateEntry(SaveKey, "");
                //var json2 = (__instance.SaveInfo?.Saver as ZipSaver)?.ZipFile?[SaveKey];

                var bytes = __instance.SaveInfo?.Saver?.ReadBytes(SaveKey);
                if (bytes == null)
                    return true;

                var json = Encoding.Default.GetString(bytes);
                if (json == null || json == "")
                    return true;

                var saveData = Helper.Deserialize<IEnumerable<string>>(value: json);
                if (saveData == null)
                    return true;

                Main.patchInfoSaved = saveData;
                var mustEnable = Main.patchInfos.IsEnabledAll(saveData);
                if (mustEnable.Count() > 0)
                {
                    Helper.ShowMessageBox("Critical patch missing! Either turn off 'Save Metadata' or press 'Enable' to close the game and enable: " + mustEnable.Join(), yesLabel: "Quit Game & Enable",
                        onYes: () =>
                        {
                            foreach (var info in mustEnable)
                                Main.patchInfos.SetEnable(true, info, force: true);
                            Main.OnSaveGUI(null);
                            //SystemUtil.ApplicationQuit();
                            SystemUtil.ApplicationRestart();
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
    }
}
