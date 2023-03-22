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
    [PatchInfo(Severity.Harmony, "Save Metadata", "will add extra metadata of enabled patches and warn if you try to load a game with missing patches", false)]
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
                Main.Print($"DEBUG OnSave 1");

                if (!Settings.State.saveMetadata || Main.appliedPatches == null || Main.appliedPatches.Count == 0)
                    return;
                Main.Print($"DEBUG OnSave 2 {Main.appliedPatches.Join()}");

                string json = Helper.Serialize(Main.appliedPatches, indent: false, type: false);
                if (json == null || json == "")
                    return;

                saveInfo.Saver.SaveBytes(SaveKey, Encoding.Unicode.GetBytes(json));
                Main.Print("Saved patch metadata");
            }
            catch (Exception e)
            {
                Main.PrintException(e);
            }
        }

        //[HarmonyPatch(typeof(ThreadedGameLoader), nameof(ThreadedGameLoader.DoLoad))]
        [HarmonyPatch(typeof(Game), nameof(Game.LoadGame))]
        [HarmonyPrefix]
        public static void OnLoad2(SaveInfo saveInfo, Game __instance)
        {
            try
            {
                if (!Settings.State.saveMetadata)
                    return;

                if (Main.restart)
                {
                    Helper.ShowMessageBox("Patch settings were changed recently. You must restart game now!", yesLabel: "Ignore this time", noLabel: "I understand",
                        onYes: () =>
                        {
                            Main.restart = false;
                        });
                }

                var bytes = saveInfo?.Saver?.ReadBytes(SaveKey);
                if (bytes == null)
                    return;

                var json = Encoding.Unicode.GetString(bytes);
                if (json == null || json == "")
                    return;

                var saveData = Helper.Deserialize<List<string>>(value: json);
                if (saveData == null)
                    return;

                foreach (string patch in Main.appliedPatches)
                    saveData.Remove(patch);

                if (saveData.Count > 0)
                {
                    Helper.ShowMessageBox("Critical patch missing! Either turn off 'Save Metadata' or press 'Enable patches' to enable: " + saveData.Join(), yesLabel: "Enable patches", noLabel: "Ignore this time",
                        onYes: () =>
                        {
                            foreach (var info in saveData)
                                Main.patchInfos.SetEnable(true, info, force: true);
                            Main.OnSaveGUI(null);
                            Helper.ShowMessageBox("Patches enabled, you must restart game now!");
                            //SystemUtil.ApplicationQuit();
                            //SystemUtil.ApplicationRestart();
                        });
                    return;
                }
                return;
            }
            catch (Exception e)
            {
                Main.PrintException(e);
                return;
            }
        }
    }
}
