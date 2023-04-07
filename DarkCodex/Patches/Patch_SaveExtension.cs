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
                if (!Settings.State.saveMetadata || Main.appliedPatches == null || Main.appliedPatches.Count == 0)
                    return;

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
                    Helper.ShowMessageBox(Resource.LocalizedStrings[(int)Localized.MessagePatchChanged], yesLabel: Resource.LocalizedStrings[(int)Localized.MessageIgnoreThisTime], noLabel: Resource.LocalizedStrings[(int)Localized.MessageIUnderstand],
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

                for (int i = saveData.Count - 1; i >= 0; i--)
                {
                    if (!Main.skippedPatches.Contains(saveData[i]))
                        saveData.Remove(saveData[i]);
                }

                if (saveData.Count > 0)
                {
                    Helper.ShowMessageBox(Resource.LocalizedStrings[(int)Localized.MessageReenablePatch] + saveData.Join(), yesLabel: Resource.LocalizedStrings[(int)Localized.MessageEnablePatch], noLabel: Resource.LocalizedStrings[(int)Localized.MessageIgnoreThisTime],
                        onYes: () =>
                        {
                            foreach (var info in saveData)
                                Main.patchInfos.SetEnable(true, info, force: true);
                            Main.OnSaveGUI(null);
                            Helper.ShowMessageBox(Resource.LocalizedStrings[(int)Localized.MessagePatchesEnabled]);
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
