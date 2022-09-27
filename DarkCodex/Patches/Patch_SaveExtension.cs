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
using CodexLib;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Save Metadata", "will add extra metadata of enabled patches and warn if you try to load a game with missing patches", false)] // TODO: rework
    [HarmonyPatch]
    public class Patch_SaveExtension
    {
        public const string SaveKey = "DarkCodex-Patches"; // do not use .json extension! CreateStateData will cause an exception

        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.SerializeAndSaveThread))]
        [HarmonyPrefix]
        public static void OnSave(SaveInfo saveInfo)
        {
            try
            {
                if (!Settings.State.saveMetadata || Main.appliedPatches == null || Main.appliedPatches.Count == 0)
                    return;

                string json = Helper.Serialize(Main.appliedPatches, indent: false, type: false);
                if (json == null || json == "")
                    return;

                saveInfo.Saver.SaveBytes(SaveKey, Encoding.Default.GetBytes(json));
                Main.Print("Saved patch metadata");
            }
            catch (Exception e)
            {
                Main.PrintException(e);
            }
        }

        [HarmonyPatch(typeof(ThreadedGameLoader), nameof(ThreadedGameLoader.DoLoad))]
        [HarmonyPrefix]
        public static void OnLoad(ThreadedGameLoader __instance)
        {
            // no use
        }

        [HarmonyPatch(typeof(SaveSlot), nameof(SaveSlot.OnButtonSaveLoad))]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyPrefix]
        public static bool BeforeLoad(SaveSlot __instance)
        {
            try
            {
                if (!Settings.State.saveMetadata)
                    return true;

                if (__instance.SlotType != SaveSlotType.Load)
                    return true;

                if (Main.restart)
                {
                    Helper.ShowMessageBox("Patch settings were changed recently. Restart game to apply new patches.", yesLabel: "Ignore this time", noLabel: "I understand",
                        onYes: () =>
                        {
                            Main.restart = false;
                        });
                    return false;
                }

                var bytes = __instance.SaveInfo?.Saver?.ReadBytes(SaveKey);
                if (bytes == null)
                    return true;

                var json = Encoding.Default.GetString(bytes);
                if (json == null || json == "")
                    return true;

                var saveData = Helper.Deserialize<List<string>>(value: json);
                if (saveData == null)
                    return true;

                foreach (string patch in Main.appliedPatches)
                    saveData.Remove(patch);

                if (saveData.Count > 0)
                {
                    Helper.ShowMessageBox("Critical patch missing! Either turn off 'Save Metadata' or press 'Enable' to close the game and enable: " + saveData.Join(), yesLabel: "Quit Game & Enable",
                        onYes: () =>
                        {
                            foreach (var info in saveData)
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
                Main.PrintException(e);
                return true;
            }
        }
    }
}
