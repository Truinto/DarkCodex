using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityModManagerNet;
using HarmonyLib;
using Guid = DarkCodex.GuidManager;
using DarkCodex.Components;
using Kingmaker.Modding;
using Kingmaker.Achievements;
using Kingmaker;
using Config;
using System.IO;
using Newtonsoft.Json;
using Kingmaker.PubSubSystem;

namespace DarkCodex
{
    public class Main
    {
        public static Harmony harmony;

        /// <summary>True if mod is enabled. Doesn't do anything right now.</summary>
        public static bool Enabled { get; set; } = true;
        /// <summary>Path of current mod.</summary>
        public static string ModPath;

        internal static UnityModManager.ModEntry.ModLogger logger;

        #region GUI

        /// <summary>Called when the mod is turned to on/off.
        /// With this function you control an operation of the mod and inform users whether it is enabled or not.</summary>
        /// <param name="value">true = mod to be turned on; false = mod to be turned off</param>
        /// <returns>Returns true, if state can be changed.</returns>
        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Main.Enabled = value;
            return true;
        }

        private static bool bExpand = false;
        private static string[] loadSaveOptions = new string[] {
            "General.*",
            "Hexcrafter.*",
            "Items.*",
            "Kineticist.*",
            "Rogue.*",
            "Witch.*",
            "General.createAbilityFocus",
            "General.patchAngelsLight",
            "Hexcrafter.fixProgression",
            "Items.patchArrows",
            "Items.patchTerendelevScale",
            "Kineticist.createExtraWildTalentFeat",
            "Kineticist.createImpaleInfusion",
            "Kineticist.createKineticistBackground",
            "Kineticist.createMobileGatheringFeat",
            "Kineticist.patchDarkElementalist",
            "Kineticist.patchGatherPower",
            "Mythic.createLimitlessBardicPerformance",
            "Mythic.createLimitlessWitchHexes",
            "Rogue.createBleedingAttack",
            "Rogue.createExtraRogueTalent",
            "Witch.createCackleActivatable",
            "Witch.createIceTomb",
        };

        /// <summary>Draws the GUI</summary>
        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("Disclaimer: Remember that playing with mods makes them mandatory for your save game! If you want to uninstall anyway, then you need to remove all references to said mod. In my case respec all your characters, that should do the trick.");
            GUILayout.Label("Legend: [F] This adds a feat. You still need to pick feats/talents for these effects. If you already picked these features, then they stay in effect regardless of the option above."
                + "\n[*] Option is enabled/disabled immediately, without restart.");

            Checkbox(ref Settings.StateManager.State.allowAchievements, "[*] Allow achievements - enables achievements while mods are active and also set corresponding flag to future save files");

            GUILayout.Label("");

            //NumberField(nameof(Settings.magicItemBaseCost), "Cost of magic items (default: 1000)");

            GUILayout.Label("");

            if (!bExpand)
            {
                Checkbox(ref bExpand, "Expand 'Advanced feature select'");
            }
            else
            {
                Checkbox(ref bExpand, "Collapse 'Advanced feature select'");
                GUILayout.Label("Options marked with <color=red><b>✖</b></color> will not be loaded. You can use this to disable certain patches you don't like or that cause you issues ingame."
                    + "\nWarning: All option require a restart. Disabling options may cause your current saves to be stuck at loading, until re-enabled.");
                foreach (string str in loadSaveOptions)
                {
                    bool enabled = !Settings.StateManager.State.doNotLoad.Contains(str);
                    if (GUILayout.Button($"{(enabled ? "<color=green><b>✔</b></color>" : "<color=red><b>✖</b></color>")} {str}", GUILayout.ExpandWidth(false)))
                    {
                        if (enabled)
                            Settings.StateManager.State.doNotLoad.Add(str);
                        else
                            Settings.StateManager.State.doNotLoad.Remove(str);
                    }
                }

                if (GUILayout.Button("Export Player Data", GUILayout.ExpandWidth(false)))
                    ExportPlayerData();
            }

            GUILayout.Label("");

            if (GUILayout.Button("Save settings!"))
            {
                OnSaveGUI(modEntry);
            }

        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            foreach (var entry in NumberTable)
            {
                try
                {
                    var field = typeof(Settings).GetField(entry.Key);

                    if (field.FieldType == typeof(int))
                    {
                        if (int.TryParse(entry.Value, out int num))
                            field.SetValue(Settings.StateManager.State, num);
                    }
                    else
                    {
                        if (float.TryParse(entry.Value, out float num))
                            field.SetValue(Settings.StateManager.State, num);
                    }
                }
                catch (Exception)
                {
                    Helper.Print($"Error while parsing number '{entry.Value}' for '{entry.Key}'");
                }
            }

            Settings.StateManager.TrySaveConfigurationState();
        }

        private static void Checkbox(ref bool value, string label, Action<bool> action = null)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button($"{(value ? "<color=green><b>✔</b></color>" : "<color=red><b>✖</b></color>")} {label}", GUILayout.ExpandWidth(false)))
            {
                value = !value;
                action?.Invoke(value);
            }

            GUILayout.EndHorizontal();
        }

        private static void NumberFieldFast(ref float value, string label)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(label + ": ", GUILayout.ExpandWidth(false));
            if (float.TryParse(GUILayout.TextField(value.ToString(), GUILayout.Width(230f)), out float newvalue))
                value = newvalue;

            GUILayout.EndHorizontal();
        }

        private static Dictionary<string, string> NumberTable = new Dictionary<string, string>();
        private static void NumberField(string key, string label)
        {
            NumberTable.TryGetValue(key, out string str);
            if (str == null) try { str = typeof(Settings).GetField(key).GetValue(Settings.StateManager.State).ToString(); } catch (Exception) { }
            if (str == null) str = "couldn't read";

            GUILayout.BeginHorizontal();

            GUILayout.Label(label + ": ", GUILayout.ExpandWidth(false));
            NumberTable[key] = GUILayout.TextField(str, GUILayout.Width(230f));

            GUILayout.EndHorizontal();
        }

        #endregion

        #region Load

        /// <summary>Loads on game start.</summary>
        /// <param name="modEntry.Info">Contains all fields from the 'Info.json' file.</param>
        /// <param name="modEntry.Path">The path to the mod folder e.g. '\Steam\steamapps\common\YourGame\Mods\TestMod\'.</param>
        /// <param name="modEntry.Active">Active or inactive.</param>
        /// <param name="modEntry.Logger">Writes logs to the 'Log.txt' file.</param>
        /// <param name="modEntry.OnToggle">The presence of this function will let the mod manager know that the mod can be safely disabled during the game.</param>
        /// <param name="modEntry.OnGUI">Called to draw UI.</param>
        /// <param name="modEntry.OnSaveGUI">Called while saving.</param>
        /// <param name="modEntry.OnUpdate">Called by MonoBehaviour.Update.</param>
        /// <param name="modEntry.OnLateUpdate">Called by MonoBehaviour.LateUpdate.</param>
        /// <param name="modEntry.OnFixedUpdate">Called by MonoBehaviour.FixedUpdate.</param>
        /// <returns>Returns true, if no error occurred.</returns>
        internal static bool Load(UnityModManager.ModEntry modEntry)
        {
            ModPath = modEntry.Path;
            logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            try
            {
                harmony = new Harmony(modEntry.Info.Id);
                harmony.PatchAll(typeof(Main).Assembly);
                //harmony.Patch(HarmonyLib.AccessTools.Method(typeof(EnumUtils), nameof(EnumUtils.GetMaxValue), null, new Type[] { typeof(ActivatableAbilityGroup) }),
                //    postfix: new HarmonyMethod(typeof(Patch_ActivatableAbilityGroup).GetMethod("Postfix")));
            }
            catch (Exception ex)
            {
                Helper.PrintException(ex);
            }


            return true;
        }

        #endregion

        #region Load_Patch

        [HarmonyPatch(typeof(StartGameLoader), "LoadAllJson")]
        public static class StartGameLoader_LoadAllJson
        {
            private static bool Run = false;
            public static void Postfix()
            {
                if (Run) return; Run = true;

                try
                {
                    Helper.Print("Loading Dark Codex");

                    LoadSafe(General.patchAngelsLight);

                    LoadSafe(Items.patchArrows);
                    LoadSafe(Items.patchTerendelevScale);

                    LoadSafe(Mythic.createLimitlessBardicPerformance);
                    LoadSafe(Mythic.createLimitlessWitchHexes);

                    LoadSafe(Kineticist.patchGatherPower);
                    LoadSafe(Kineticist.patchDarkElementalist);
                    LoadSafe(Kineticist.createKineticistBackground);
                    LoadSafe(Kineticist.createMobileGatheringFeat);
                    LoadSafe(Kineticist.createImpaleInfusion);

                    LoadSafe(Witch.createIceTomb);

                    LoadSafe(Hexcrafter.fixProgression);

                    LoadSafe(Rogue.createBleedingAttack);

                    LoadSafe(General.createAbilityFocus); // keep last
                    LoadSafe(Kineticist.createExtraWildTalentFeat); // keep last
                    LoadSafe(Witch.createCackleActivatable); // keep last
                    LoadSafe(Rogue.createExtraRogueTalent); // keep last

                    EventBus.Subscribe(new RestoreEndOfCombat());
                    //Patch_CombatState.End += RestoreEndOfCombat.HandlePartyCombatEnd;


                    Helper.Patch(typeof(Patch_AllowAchievements), "Prefix", null);

                    Helper.Print("Finished loading Dark Codex");
#if DEBUG
                    Helper.PrintDebug("Running in debug.");
                    Helper.ExportStrings();
                    Guid.i.WriteAll();
#endif
                }
                catch (Exception ex)
                {
                    Helper.PrintException(ex);
                }
            }
        }

        #endregion

        #region Helper

        public static bool LoadSafe(Action action)
        {
#if DEBUG
            var watch = Stopwatch.StartNew();
#endif
            string name = action.Method.DeclaringType.Name + "." + action.Method.Name;

            if (CheckSetting(name))
            {
                Helper.Print($"Skipped loading {name}");
                return false;
            }

            try
            {
                Helper.Print($"Loading {name}");
                action();
#if DEBUG
                watch.Stop();
                Helper.PrintDebug("Loaded in milliseconds: " + watch.ElapsedMilliseconds);
#endif
                return true;
            }
            catch (Exception e)
            {
#if DEBUG
                watch.Stop();
#endif
                Helper.PrintException(e);
                return false;
            }
        }

        public static bool LoadSafe(Action<bool> action, bool flag)
        {
#if DEBUG
            var watch = Stopwatch.StartNew();
#endif
            string name = action.Method.DeclaringType.Name + "." + action.Method.Name;

            if (CheckSetting(name))
            {
                Helper.Print($"Skipped loading {name}");
                return false;
            }

            try
            {
                Helper.Print($"Loading {name}:{flag}");
                action(flag);
#if DEBUG
                watch.Stop();
                Helper.PrintDebug("Loaded in milliseconds: " + watch.ElapsedMilliseconds);
#endif
                return true;
            }
            catch (Exception e)
            {
#if DEBUG
                watch.Stop();
#endif
                Helper.PrintException(e);
                return false;
            }
        }

        private static bool CheckSetting(string name)
        {
            foreach (string str in Settings.StateManager.State.doNotLoad)
            {
                if (str == name)
                    return true;

                if (str.EndsWith(".*", StringComparison.Ordinal) && name.StartsWith(str.Substring(0, str.Length - 1), StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static void ExportPlayerData()
        {
            try
            {
                JsonSerializer serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.All,
                });

                using (StreamWriter sw = new StreamWriter(Path.Combine(ModPath, "player.json")))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, Game.Instance.Player.Party);
                }

                Helper.Print("Exported player data.");
            }
            catch (Exception e)
            {
                Helper.PrintException(e);
            }
        }

        #endregion

    }
}
