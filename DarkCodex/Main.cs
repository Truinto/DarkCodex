global using CodexLib;
global using HarmonyLib;
global using JetBrains.Annotations;
global using Kingmaker;
global using Kingmaker.Blueprints;
global using Kingmaker.Blueprints.Classes;
global using Kingmaker.Blueprints.Classes.Selection;
global using Kingmaker.Blueprints.Classes.Spells;
global using Kingmaker.Blueprints.Facts;
global using Kingmaker.Blueprints.Items.Armors;
global using Kingmaker.Blueprints.Root;
global using Kingmaker.Designers.Mechanics.Facts;
global using Kingmaker.EntitySystem;
global using Kingmaker.EntitySystem.Entities;
global using Kingmaker.EntitySystem.Stats;
global using Kingmaker.Enums;
global using Kingmaker.Enums.Damage;
global using Kingmaker.Items;
global using Kingmaker.Items.Slots;
global using Kingmaker.Localization;
global using Kingmaker.PubSubSystem;
global using Kingmaker.RuleSystem;
global using Kingmaker.RuleSystem.Rules;
global using Kingmaker.RuleSystem.Rules.Abilities;
global using Kingmaker.RuleSystem.Rules.Damage;
global using Kingmaker.UnitLogic;
global using Kingmaker.UnitLogic.Abilities;
global using Kingmaker.UnitLogic.Abilities.Blueprints;
global using Kingmaker.UnitLogic.Abilities.Components;
global using Kingmaker.UnitLogic.Abilities.Components.Base;
global using Kingmaker.UnitLogic.ActivatableAbilities;
global using Kingmaker.UnitLogic.Buffs.Blueprints;
global using Kingmaker.UnitLogic.Commands.Base;
global using Kingmaker.UnitLogic.FactLogic;
global using Kingmaker.UnitLogic.Mechanics;
global using Kingmaker.UnitLogic.Mechanics.Actions;
global using Kingmaker.UnitLogic.Mechanics.Components;
global using Kingmaker.UnitLogic.Mechanics.Conditions;
global using Kingmaker.UnitLogic.Mechanics.Properties;
global using Kingmaker.Utility;
global using Newtonsoft.Json;
global using System;
global using System.Collections.Generic;
global using System.Diagnostics;
global using System.Linq;
global using System.Reflection;
global using System.Reflection.Emit;
global using UnityEngine;
using DarkCodex;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using System.IO;
using System.Runtime.CompilerServices;
using UnityModManagerNet;

namespace Shared
{
    //#if DEBUG [EnableReloading] #endif
    public static partial class Main
    {
#if DEBUG
        public const bool AllowGuidGeneration = true;
#else
        public const bool AllowGuidGeneration = false;
#endif

        public static Harmony harmony;
        public static bool Enabled;
        public static string ModPath;
        internal static PatchInfoCollection patchInfos;
        internal static readonly List<string> appliedPatches = new();
        internal static readonly List<string> skippedPatches = new();
        internal static UnityModManager.ModEntry.ModLogger logger;
        public static bool applyNullFinalizer;

        public static bool IsInGame => Game.Instance.Player?.Party?.Any() ?? false; // RootUIContext.Instance?.IsInGame ?? false; //

        //[SaveOnReload] internal static int IsLoad;

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

        internal static bool restart;
        private static GUIStyle StyleBox;
        private static GUIStyle StyleLine;
        private static List<string> CategoryFolded = new();
        /// <summary>Draws the GUI</summary>
        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            using var scope = new Scope(Main.ModPath, Main.logger, Main.harmony, Main.AllowGuidGeneration);
            Settings state = Settings.State;

            if (StyleBox == null)
            {
                StyleBox = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter };
                StyleLine = new GUIStyle() { fixedHeight = 1, margin = new RectOffset(0, 0, 4, 4), };
                StyleLine.normal.background = new Texture2D(1, 1);
            }

            //GUILayout.Label($"toggleKineticistGatherPower {Resource.TestField()}");

            GUILayout.Label(Resource.LocalizedStrings[(int)Localized.MenuDisclaimer]);
            GUILayout.Label(Resource.LocalizedStrings[(int)Localized.MenuLegend]);

            if (Patch_AllowAchievements.Patched)
                Checkbox(ref state.allowAchievements, Resource.LocalizedStrings[(int)Localized.MenuAllowAchievements]);
            else
                GUILayout.Label(Resource.LocalizedStrings[(int)Localized.MenuManagedAchievements]);

            Checkbox(ref Settings.State.saveMetadata, Resource.LocalizedStrings[(int)Localized.MenuSaveMetadata]);

            Checkbox(ref state.PsychokineticistStat, Resource.LocalizedStrings[(int)Localized.MenuPsychokineticist]);
            Checkbox(ref state.reallyFreeCost, Resource.LocalizedStrings[(int)Localized.MenuLimitlessFeats]);

            //NumberField(nameof(Settings.magicItemBaseCost), "Cost of magic items (default: 1000)");
            //NumberFieldFast(ref _debug1, "Target Frame Rate");

#if DEBUG
            if (Main.IsInGame)
            {
                GUILayout.Space(10);
                GUILayout.Label("Advanced: Prebuild");

                if (GUILayout.Button("<color=yellow>Refresh list</color>", GUILayout.ExpandWidth(false)))
                    Patch_Prebuilds.Clear();

                foreach (var unit in Game.Instance.Player.PartyAndPets)
                {
                    if (!Patch_Prebuilds.HasPlan(unit))
                        GUILayout.Label("No plan for: " + unit.CharacterName);
                    else if (GUILayout.Button("Load plan for: " + unit.CharacterName, GUILayout.ExpandWidth(false)))
                        Patch_Prebuilds.SetPlan(unit);
                }
            }
#endif

            GUILayout.Space(10);
            GUILayout.Label(Resource.LocalizedStrings[(int)Localized.MenuAdvancedPatch]);
            GUILayout.Label(Resource.LocalizedStrings[(int)Localized.MenuPatchExplanation]);
            if (GUILayout.Button(Resource.LocalizedStrings[(int)Localized.MenuDisableHomebrew], GUILayout.ExpandWidth(false)))
                patchInfos.Where(w => w.Homebrew).ForEach(attr => patchInfos.SetEnable(false, attr));
            GUILayout.Space(10);
            Checkbox(state.NewFeatureDefaultOn, Resource.LocalizedStrings[(int)Localized.MenuNewFeaturesDefault], b =>
            {
                state.NewFeatureDefaultOn = b;
                restart = true;
                patchInfos.Update();
            });

            string category = null;
            bool folded = false;
            foreach (var info in patchInfos)
            {
#if !DEBUG
                if (info.IsHidden) continue;
#endif
                if (info.Class != category)
                {
                    category = info.Class;
                    folded = CategoryFolded.Contains(category);

                    GUILayout.Box(GUIContent.none, StyleLine);
                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button(!info.DisabledAll ? "<color=green><b>✔</b></color>" : "<color=red><b>✖</b></color>", StyleBox, GUILayout.Width(20)))
                    {
                        restart = true;
                        patchInfos.SetEnable(info.DisabledAll, category + ".*");
                    }
                    GUILayout.Space(7);

                    if (GUILayout.Button(folded ? "<color=yellow><b>▶</b></color>" : "<color=lime><b>▼</b></color>", StyleBox, GUILayout.Width(20)))
                    {
                        if (folded)
                            CategoryFolded.Remove(category);
                        else
                            CategoryFolded.Add(category);
                    }

                    GUILayout.Space(3);
                    GUILayout.Label(info.DisplayClass);

                    GUILayout.EndHorizontal();
                }

                if (folded) continue;

                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                if (DrawInfoButton(info))
                {
                    restart = true;
                    patchInfos.SetEnable(info.Disabled, info);
                }
                GUILayout.Space(5);
#if DEBUG
                if (info.Homebrew) // TODO: improve menu
                    GUILayout.Label(Resource.Cache.IconPotBlack, GUILayout.ExpandWidth(false));
                else
                    GUILayout.Label(Resource.Cache.IconBookBlack, GUILayout.ExpandWidth(false));
                GUILayout.Space(5);
#endif
                GUILayout.Label(info.DisplayName.ToString().Grey(info.IsHidden).Red(info.IsDangerous), GUILayout.Width(300));
                GUILayout.Label(info.Description, GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
            GUILayout.Label(Resource.LocalizedStrings[(int)Localized.MenuDebug]);

            if (GUILayout.Button("Debug: Export Player Data", GUILayout.ExpandWidth(false)))
                ExportPlayerData();
            if (GUILayout.Button("Debug: Export LevelPlanData", GUILayout.ExpandWidth(false)))
                ExportLevelPlanData();
            //if (GUILayout.Button("Debug: Date minus 1", GUILayout.ExpandWidth(false)))
            //    DarkCodex.DEBUG.Date.SetDate();
            //if (GUILayout.Button("Debug: Open Shared Stash", GUILayout.ExpandWidth(false)))
            //    DarkCodex.DEBUG.Loot.Open();
            //if (GUILayout.Button("Debug: Export Icons", GUILayout.ExpandWidth(false)))
            //    DarkCodex.DEBUG.ExportAllIconTextures();
            if (GUILayout.Button("Debug: Pause Area Fxs", GUILayout.ExpandWidth(false)))
                Event_AreaEffects.Stop();
            if (GUILayout.Button("Debug: Continue Area Fxs", GUILayout.ExpandWidth(false)))
                Event_AreaEffects.Continue(force: true);
            if (GUILayout.Button("Debug: Print Content Table", GUILayout.ExpandWidth(false)))
            {
                using var sw = new StreamWriter(Path.Combine(Main.ModPath, "content.md"), false);
                sw.WriteLine("Content");
                sw.WriteLine("-----------");
                sw.WriteLine("| Option | Description | HB | Status |");
                sw.WriteLine("|--------|-------------|----|--------|");

                foreach (var info in patchInfos)
                    if (!info.IsEvent && !info.IsHidden)
                        sw.WriteLine($"|{info.Class}.{info.Method}|{info.Description.ToString().Replace('\n', ' ')}|{info.HomebrewStr}|{info.StatusStr}|");
            }
            if (GUILayout.Button("Debug: Generate CodexLib bin files (will lag)", GUILayout.ExpandWidth(false)))
                BpCache.ExportResources(Path.Combine(Helper.PathMods, "Blueprints.bin"));
            Checkbox(ref state.polymorphKeepInventory, "Debug: Enable polymorph equipment (restart to disable)");
            Checkbox(ref state.polymorphKeepModel, "Debug: Disable polymorph transformation [*]");
            Checkbox(ref state.verbose, "Debug: Verbose");
            Checkbox(ref state.debug_1, "Debug: Flag1");
            Checkbox(ref state.debug_2, "Debug: Flag2");
            Checkbox(ref state.debug_3, "Debug: Flag3");
            Checkbox(ref state.debug_4, "Debug: Flag4");

            GUILayout.Label("");

            if (GUILayout.Button(Resource.LocalizedStrings[(int)Localized.MenuSave]))
                OnSaveGUI(modEntry);

            //if (GUI.tooltip != null && GUI.tooltip != "")
            //{
            //    var mouse = Event.current.mousePosition;
            //    GUI.Label(new Rect(mouse.x, mouse.y, 0, 0), "Test tooltip");// GUI.tooltip);
            //}
        }

        private static void OnHideGUI(UnityModManager.ModEntry modEntry)
        {
            if (restart)
            {
                UIUtility.ShowMessageBox("Warning! Patch selection changed. Restart game before saving. If you cannot load your save, re-enable patches.", MessageModalBase.ModalType.Message, a => { }, null, 0, null, null, null);
            }
        }

        internal static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            foreach (var entry in NumberTable)
            {
                try
                {
                    var field = typeof(Settings).GetField(entry.Key);
                    if (field.FieldType == typeof(int))
                    {
                        if (int.TryParse(entry.Value, out int num))
                            field.SetValue(Settings.State, num);
                    }
                    else if (field.FieldType == typeof(float))
                    {
                        if (float.TryParse(entry.Value, out float num))
                            field.SetValue(Settings.State, num);
                    }
                }
                catch (Exception)
                {
                    Print($"Error while parsing number '{entry.Value}' for '{entry.Key}'");
                }
            }

            Settings.State.TrySave();
        }

        private static string lastEnum;
        private static void Checkbox<T>(ref T value, string label, Action<T> action = null) where T : Enum
        {
            if (GUILayout.Button(label, GUILayout.ExpandWidth(false)))
            {
                if (lastEnum == label)
                    lastEnum = null;
                else
                    lastEnum = label;
            }

            if (lastEnum != label)
                return;

            string name = value.ToString();
            var names = Enum.GetNames(typeof(T));
            int index = names.IndexOf(name);
            int newindex = GUILayout.SelectionGrid(index, names, 10); //GUILayout.SelectionGrid(index, names, names.Length)

            if (index == newindex)
                return;

            value = (T)Enum.Parse(typeof(T), names[newindex]);
            action?.Invoke(value);
        }

        private static void Checkbox(ref bool value, string label, Action<bool> action = null)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(value ? "<color=green><b>✔</b></color>" : "<color=red><b>✖</b></color>", StyleBox, GUILayout.Width(20)))
            {
                value = !value;
                action?.Invoke(value);
            }
            GUILayout.Space(5);
            GUILayout.Label(label, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }

        private static void Checkbox(bool value, string label, Action<bool> action)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(value ? "<color=green><b>✔</b></color>" : "<color=red><b>✖</b></color>", StyleBox, GUILayout.Width(20)))
            {
                action.Invoke(!value);
            }
            GUILayout.Space(5);
            GUILayout.Label(label, GUILayout.ExpandWidth(false));
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

        private static Dictionary<string, string> NumberTable = new();
        private static void NumberField(string key, string label)
        {
            NumberTable.TryGetValue(key, out string str);
            if (str == null) try { str = typeof(Settings).GetField(key).GetValue(Settings.State).ToString(); } catch (Exception) { }
            str ??= "couldn't read";

            GUILayout.BeginHorizontal();

            GUILayout.Label(label + ": ", GUILayout.ExpandWidth(false));
            NumberTable[key] = GUILayout.TextField(str, GUILayout.Width(230f));

            GUILayout.EndHorizontal();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool DrawInfoButton(PatchInfoAttribute info)
        {
            GUIContent text;
            if (info.DisabledAll)
            {
                if (info.Disabled)
                    text = new("<color=grey><b>✖</b></color>", "Disabled by category.");
                else
                    text = new("<color=grey><b>✔</b></color>", "Disabled by category.");
            }
            else if (info.Disabled)
                text = new("<color=red><b>✖</b></color>", "");
            else if (info.Requirement?.Name is string req
                    && patchInfos.FirstOrDefault(f => f.Method == req) is PatchInfoAttribute other
                    && (other.Disabled || other.DisabledAll))
                text = new("<color=yellow><b>!</b></color>", "This patch won't work without " + req);
            else
                text = new("<color=green><b>✔</b></color>", "");

            return GUILayout.Button(text, StyleBox, GUILayout.Width(20));
        }

        private static void DrawToolip()
        {
            Vector3 x = Input.mousePosition;

            Rect r = new(x.x - 10, x.y, 150, 20);

            GUI.Box(r, "This patch won't work without {0}");
        }

        #endregion

        #region Load

        private static void OnLoad(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnHideGUI = OnHideGUI;

            // localization logic
            LocalizedStringCached.Resolver = f => Helper.CreateString(f.Default);
            Helper.OnLocaleChange += () =>
            {
                Resource._localizedStrings = null;
                foreach (var patch in patchInfos)
                {
                    patch.DisplayName.Clear();
                    patch.Description.Clear();
                }
            };

            MasterPatch.Run(typeof(CodexLib.BpCache)); // this must run very early
            Helper.Patch(typeof(Patch_SaveExtension));

            //harmony.PatchAll(typeof(Main).Assembly);
            //harmony.Patch(HarmonyLib.AccessTools.Method(typeof(EnumUtils), nameof(EnumUtils.GetMaxValue), null, new Type[] { typeof(ActivatableAbilityGroup) }),
            //    postfix: new HarmonyMethod(typeof(Patch_ActivatableAbilityGroup).GetMethod("Postfix")));
        }

        private static void OnMainMenu()
        {
            if (Settings.State.showBootupWarning)
            {
                UIUtility.ShowMessageBox("Installed Dark Codex.\nIf you want to disable certain features, do it now. Disabling 'red' features during a playthrough is not possible. Enabling features can be done at any time.", MessageModalBase.ModalType.Message, a => { }, null, 0, null, null, null);
                Settings.State.showBootupWarning = false;
                Settings.State.TrySave();
            }
        }

        private static object instanceSettings;
        private static FieldInfo toggleKineticistGatherPower;
        private static void OnToggleModMenu(bool open)
        {
            if (!open)
            {
                try
                {
                    if (instanceSettings == null)
                    {
                        instanceSettings = false;
                        var type = Type.GetType("ToyBox.Main, ToyBox");
                        if (type != null)
                        {
                            instanceSettings = type.GetField("settings", Helper.BindingAll).GetValue(null);
                            var typeSettings = instanceSettings.GetType();
                            toggleKineticistGatherPower = typeSettings.GetField("toggleKineticistGatherPower", Helper.BindingAll);
                        }
                    }

                    if (toggleKineticistGatherPower != null)
                        RestrictionCanGatherPowerAbility.Cheat = (bool)toggleKineticistGatherPower.GetValue(instanceSettings);
                }
                catch (Exception e) { PrintDebug(e.ToString()); }
            }
        }

        private static void OnBlueprintsLoaded()
        {
            using var scope = new Scope(Main.ModPath, Main.logger, harmony, AllowGuidGeneration);
            MasterPatch.Run();
            Print("Loading Dark Codex");
            patchInfos = new(Settings.State);

            // for Debug
#if DEBUG
            //PatchSafe(typeof(DEBUG.WatchCalculateParams));
            PatchSafe(typeof(DEBUG.WatchSelectiveMetamagic));
            //PatchSafe(typeof(DEBUG.WatchWeaponEnchantment));
            //PatchSafe(typeof(DEBUG.WatchSharedValue));
            PatchSafe(typeof(DEBUG.Settlement1));
            PatchSafe(typeof(DEBUG.Settlement2));
            PatchSafe(typeof(DEBUG.ArmyLeader1));
            PatchSafe(typeof(DEBUG.SpellReach));
            PatchSafe(typeof(Patch_Prebuilds));
            LoadSafe(General.CreatePoison);
            LoadSafe(Kineticist.CreateElementalAscetic);
            LoadSafe(Kineticist.FixBladeWhirlwind);
#endif
            LoadSafe(DEBUG.Enchantments.NameAll);
            PatchSafe(typeof(DEBUG.Enchantments));
            LoadSafe(General.CreateBardStopSong);

            // Cache
            LoadSafe(General.CreatePropertyMaxMentalAttribute);
            LoadSafe(General.CreatePropertyGetterSneakAttack);
            LoadSafe(General.CreateMythicDispelProperty);
            LoadSafe(BleedBuff.Create);

            // Harmony Patches
            PatchUnique(typeof(Patch_AllowAchievements));
            PatchSafe(typeof(Patch_FixPolymorphGather));
            PatchSafe(typeof(Patch_TrueGatherPowerLevel));
            PatchSafe(typeof(Patch_KineticistAllowOpportunityAttack));
            PatchSafe(typeof(Patch_EnvelopingWindsCap));
            PatchSafe(typeof(Patch_MagicItemAdept));
            PatchSafe(typeof(Patch_ActivatableOnNewRound));
            PatchSafe(typeof(Patch_ActivatableHandleUnitRunCommand));
            PatchSafe(typeof(Patch_ActivatableOnTurnOn));
            PatchSafe(typeof(Patch_ActivatableTryStart));
            PatchSafe(typeof(Patch_ResourcefulCaster));
            PatchSafe(typeof(Patch_PreferredSpellMetamagic));
            PatchSafe(typeof(Patch_FixAreaDoubleDamage));
            PatchSafe(typeof(Patch_FixAreaEndOfTurn));
            PatchSafe(typeof(Patch_Polymorph));
            PatchSafe(typeof(Patch_EnduringSpells));
            PatchSafe(typeof(Patch_UnlockClassLevels));
            PatchSafe(typeof(Patch_DarkElementalistBurn));
            PatchSafe(typeof(Patch_DismissAnything));
            PatchSafe(typeof(Patch_FixQuickenMetamagic));
            PatchSafe(typeof(Patch_HexcrafterSpellStrike));
            PatchSafe(typeof(Patch_BackgroundChecks));
            PatchSafe(typeof(Patch_ArcanistSpontaneous));
            PatchSafe(typeof(Patch_ZippySpellLike));
            PatchSafe(typeof(Patch_AbilityRange));
            PatchSafe(typeof(Patch_FixMisc));
            PatchSafe(typeof(Patch_ParryAlways));
            PatchSafe(typeof(Patch_AzataFavorableMagic));
            PatchSafe(typeof(Patch_RespecPartially));
            PatchSafe(typeof(Patch_LimitlessActivatables));
            PatchSafe(typeof(Patch_FixFeatureSelection));
            //PatchSafe(typeof(Patch_FixEldritchArcherSpellstrike)); // TODO: fix or remove

            // Spells - early
            LoadSafe(Spells.CreateDebugSpells);
            LoadSafe(Spells.CreateBladedDash);
            LoadSafe(Spells.CreateHealingFlames);
            LoadSafe(Spells.CreateFlameBlade);
            LoadSafe(Spells.CreateDivineTrident);
            LoadSafe(Spells.CreateProduceFlame);
            LoadSafe(Spells.CreateChillTouch);
            LoadSafe(Spells.PatchVarious);

            // General
            LoadSafe(General.CreateSpellPerfection);
            LoadSafe(General.CreatePreferredSpell);
            LoadSafe(General.CreateHeritage);
            LoadSafe(General.CreateMadMagic);
            LoadSafe(General.CreateSacredSummons);
            LoadSafe(General.FixMasterShapeshifter);
            LoadSafe(General.PatchAngelsLight);
            LoadSafe(General.PatchBasicFreebieFeats);
            LoadSafe(General.PatchHideBuffs);
            LoadSafe(General.PatchVarious);
            LoadSafe(General.CreateDirtyFighting);
            LoadSafe(General.CreateOpportuneParry);
            LoadSafe(General.CreateKitsuneFoxfire);

            // Spellcasters
            LoadSafe(Spellcasters.FixBloodlineArcane);
            LoadSafe(Spellcasters.PatchArcanistBrownFur);
            LoadSafe(Spellcasters.CreatePurifyingChannel);
            LoadSafe(Spellcasters.CreateBestowHope);
            LoadSafe(Spellcasters.CreateEnergyChannel);
            LoadSafe(Spellcasters.CreateChannelForm);

            // MartialArt
            LoadSafe(MartialArt.CreatePaladinVirtuousBravo);
            LoadSafe(MartialArt.CreateProdigiousTwoWeaponFighting);
            LoadSafe(MartialArt.CreateBladedBrush);

            // Items
            LoadSafe(Items.PatchArrows);
            LoadSafe(Items.PatchTerendelevScale);
            LoadSafe(Items.CreateKineticArtifact);
            LoadSafe(Items.CreateButcheringAxe);
            LoadSafe(Items.CreateImpactEnchantment);

            // Mythic
            LoadSafe(Mythic.CreateLimitlessBardicPerformance);
            LoadSafe(Mythic.CreateLimitlessSmite);
            LoadSafe(Mythic.CreateLimitlessBombs);
            LoadSafe(Mythic.CreateLimitlessArcanePool);
            LoadSafe(Mythic.CreateLimitlessArcaneReservoir);
            LoadSafe(Mythic.CreateLimitlessKi);
            LoadSafe(Mythic.CreateLimitlessDomain);
            LoadSafe(Mythic.CreateLimitlessShaman);
            LoadSafe(Mythic.CreateLimitlessWarpriest);
            LoadSafe(Mythic.CreateLimitlessWarpriestBlessing);
            LoadSafe(Mythic.CreateLimitlessInquisitorBane);
            LoadSafe(Mythic.CreateLimitlessBloodlineClaws);
            LoadSafe(Mythic.ExtendLimitlessAnimalFocus);
            LoadSafe(Mythic.CreateKineticMastery);
            LoadSafe(Mythic.CreateMagicItemAdept);
            LoadSafe(Mythic.CreateResourcefulCaster);
            LoadSafe(Mythic.CreateSwiftHuntersBond);
            LoadSafe(Mythic.CreateDemonMastery);
            LoadSafe(Mythic.CreateDemonLord);
            LoadSafe(Mythic.CreateMetamagicAdept);
            LoadSafe(Mythic.CreateMythicEschewMaterials);
            LoadSafe(Mythic.CreateMythicCompanion);
            LoadSafe(Mythic.CreateNotAChance);
            LoadSafe(Mythic.CreateHarmoniousMage);
            LoadSafe(Mythic.PatchKineticOvercharge);
            LoadSafe(Mythic.PatchLimitlessDemonRage);
            LoadSafe(Mythic.PatchUnstoppable);
            LoadSafe(Mythic.PatchBoundlessHealing);
            LoadSafe(Mythic.PatchBoundlessInjury);
            LoadSafe(Mythic.PatchRangingShots);
            LoadSafe(Mythic.PatchWanderingHex);
            LoadSafe(Mythic.PatchJudgementAura);
            LoadSafe(Mythic.PatchAscendantSummons);
            LoadSafe(Mythic.PatchAlwaysAChance);
            LoadSafe(Mythic.PatchElementalBarrage);
            LoadSafe(Mythic.PatchVarious);

            // Kineticist
            LoadSafe(Kineticist.CreateElementalScion);
            //LoadSafe(Kineticist.CreateElementalAscetic);
            LoadSafe(Kineticist.FixWallInfusion);
            LoadSafe(Kineticist.CreateKineticistBackground);
            LoadSafe(Kineticist.CreateMobileGatheringFeat);
            LoadSafe(Kineticist.CreateImpaleInfusion);
            LoadSafe(Kineticist.CreateChainInfusion);
            LoadSafe(Kineticist.CreateWhipInfusion);
            LoadSafe(Kineticist.CreateBladeRushInfusion);
            LoadSafe(Kineticist.CreateAutoMetakinesis);
            LoadSafe(Kineticist.CreateHurricaneQueen);
            LoadSafe(Kineticist.CreateMindShield);
            LoadSafe(Kineticist.PatchGatherPower);
            LoadSafe(Kineticist.PatchDarkElementalist);
            LoadSafe(Kineticist.PatchDemonCharge); // after createMobileGatheringFeat
            LoadSafe(Kineticist.PatchVarious);
            LoadSafe(Kineticist.FixBloodKineticist);
            LoadSafe(Kineticist.FixBlastsAreSpellLike);
            LoadSafe(Kineticist.FixExpandedElementFocus);
            LoadSafe(Kineticist.CreateKineticFist);
            LoadSafe(Kineticist.CreateKineticEnergizeWeapon);
            LoadSafe(Kineticist.CreateSelectiveMetakinesis);
            LoadSafe(Kineticist.CreateVenomInfusion); // keep late

            // Monk
            LoadSafe(Monk.CreateFeralCombatTraining);
            LoadSafe(Monk.PatchSoheiRapidShot);

            // Witch
            LoadSafe(Witch.CreateIceTomb);
            LoadSafe(Witch.CreateSplitHex);
            LoadSafe(Witch.FixBoundlessHealing);
            LoadSafe(Witch.FixFortuneHex);

            // Magus
            LoadSafe(Magus.CreateAccursedStrike);
            LoadSafe(Magus.FixHexcrafterProgression);
            LoadSafe(Magus.PatchSwordSaint);

            // Rogue
            LoadSafe(Rogue.CreateBleedingAttack);

            // Ranger
            LoadSafe(Ranger.CreateImprovedHuntersBond);

            // Extra Features - keep last
            LoadSafe(General.CreateBackgrounds); // keep last
            LoadSafe(General.FixSpellElementChange); // keep last
            LoadSafe(Mythic.CreateLimitlessWitchHexes); // keep last
            LoadSafe(General.CreateAbilityFocus); // keep last
            LoadSafe(Kineticist.CreateExtraWildTalentFeat); // keep last
            LoadSafe(Witch.CreateExtraHex); // keep last
            LoadSafe(Witch.CreateCackleActivatable); // keep last
            LoadSafe(Rogue.CreateExtraRogueTalent); // keep last
            LoadSafe(Mythic.CreateExtraMythicFeats); // keep last
            LoadSafe(Mythic.CreateSwiftHex); // keep last

            // Event subscriptions
            SubscribeSafe(typeof(Event_RestoreEndOfCombat));
            SubscribeSafe(typeof(Event_AreaEffects));
        }

        private static void OnBlueprintsLoadedLast()
        {
            using var scope = new Scope(Main.ModPath, Main.logger, harmony, AllowGuidGeneration);

            LoadSafe(Kineticist.CreateExpandedElement);

            // Unlocks
            LoadSafe(Unlock.UnlockSpells);
            LoadSafe(Unlock.UnlockAnimalCompanion);
            LoadSafe(Unlock.UnlockKineticist);

            patchInfos.Sort(); // sort info list for GUI
            patchInfos.Update();
#if DEBUG
            PrintDebug("Running in debug.");
            ExportContent();
            _ = Resource.LocalizedStrings;
            Helper.ExportStrings();
            Helper.ExportStrings("enchant");
            KineticistTree.Instance.Validate();
#endif
        }

        #endregion

        #region UnityModManager

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
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
            ModPath = modEntry.Path;
            logger = modEntry.Logger;
            modEntry.OnUnload = Unload;

            try
            {
                EnsureCodexLib(modEntry.Path);
                harmony = new Harmony(modEntry.Info.Id);
                Patch(typeof(Patches));
                OnLoad(modEntry);
                Enabled = true;
                return true;
            }
            catch (Exception ex)
            {
                logger?.LogException(ex);
                return false;
            }
        }

        public static bool Unload(UnityModManager.ModEntry modEntry)
        {
            harmony?.UnpatchAll(modEntry.Info.Id);
            Enabled = false;
            return true;
        }

        private static void EnsureCodexLib(string modPath)
        {
            if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName.StartsWith("CodexLib, ")))
            {
                PrintDebug("CodexLib already loaded.");
                return;
            }

            string path = null;
            Version version = null;
            modPath = new DirectoryInfo(modPath).Parent.FullName;
            PrintDebug("Looking for CodexLib in " + modPath);

            foreach (string cPath in Directory.GetFiles(modPath, "CodexLib.dll", SearchOption.AllDirectories))
            {
                try
                {
                    var cVersion = new Version(FileVersionInfo.GetVersionInfo(cPath).FileVersion);
                    PrintDebug($"Found: newer={version == null || cVersion > version} version={cVersion} @ {cPath}");
                    if (version == null || cVersion > version)
                    {
                        path = cPath;
                        version = cVersion;
                    }
                }
                catch (Exception) { }
            }

            if (path != null)
            {
                try
                {
                    Print("Loading CodexLib " + path);
                    AppDomain.CurrentDomain.Load(File.ReadAllBytes(path));
                }
                catch (Exception) { }
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                PrintDebug("Requested " + args.Name);

                if (ModPath != null && args.Name.StartsWith("CodexLib, "))
                {
                    string path = null;
                    Version version = null;

                    foreach (string cPath in Directory.GetFiles(Directory.GetParent(ModPath).FullName, "CodexLib.dll"))
                    {
                        var cVersion = new Version(FileVersionInfo.GetVersionInfo(cPath).FileVersion);
                        if (version == null || cVersion > version)
                        {
                            path = cPath;
                            version = cVersion;
                        }
                    }

                    if (path != null)
                    {
                        Print("AssemblyResolve " + path);
                        return Assembly.LoadFrom(path);
                    }
                }
            }
            catch (Exception ex) { logger?.LogException(ex); }
            return null;
        }

        [HarmonyPatch]
        private static class Patches
        {
            //[HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))] // used by some mods
            //[HarmonyPostfix]
            //[HarmonyPriority(Priority.First + 5)]
            private static void Postfix1()
            {
                try
                {
                }
                catch (Exception ex) { logger?.LogException(ex); }
            }

            [HarmonyPatch(typeof(StartGameLoader), nameof(StartGameLoader.LoadAllJson))]
            [HarmonyPriority(Priority.Normal)]
            [HarmonyPostfix]
            private static void Postfix2()
            {
                try
                {
                    OnBlueprintsLoaded();
                }
                catch (Exception ex) { logger?.LogException(ex); }
            }

            [HarmonyPatch(typeof(StartGameLoader), nameof(StartGameLoader.LoadAllJson))]
            [HarmonyPriority(Priority.Last - 5)]
            [HarmonyPostfix]
            private static void Postfix3()
            {
                try
                {
                    OnBlueprintsLoadedLast();
                    RunLastNow();

                    if (applyNullFinalizer)
                    {
                        var nullFinalizer = new HarmonyMethod(AccessTools.Method(typeof(Main), nameof(Main.NullFinalizer)));
                        foreach (var patch in harmony.GetPatchedMethods().ToArray())
                        {
                            if (Harmony.GetPatchInfo(patch).Finalizers.Count == 0)
                                harmony.Patch(patch, finalizer: nullFinalizer);
                        }
                    }
                }
                catch (Exception ex) { logger?.LogException(ex); }
            }

            [HarmonyPatch(typeof(MainMenu), nameof(MainMenu.Start))]
            [HarmonyPostfix]
            private static void Postfix4()
            {
                try
                {
                    OnMainMenu();
                    OnToggleModMenu(false);
                }
                catch (Exception ex) { logger?.LogException(ex); }
            }

            [HarmonyPatch(typeof(UnityModManager.UI), nameof(UnityModManager.UI.ToggleWindow), typeof(bool))]
            [HarmonyPostfix]
            private static void Postfix5(bool open, bool ___mOpened)
            {
                if (open != ___mOpened)
                    OnToggleModMenu(open);
            }
        }

        #endregion

        #region Helper

        private static List<(string, Action)> _patchLast = new();
        internal static void RunLast(string message, Action action)
        {
            if (_patchLast == null)
                action();
            else
                _patchLast.Add((message, action));
        }
        private static void RunLastNow()
        {
            if (_patchLast == null)
                return;

            if (_patchLast.Count > 0)
                PrintDebug("Running _patchLast");

            foreach (var (message, action) in _patchLast)
            {
                try
                {
                    PrintDebug(message);
                    action();
                }
                catch (Exception e) { PrintException(e); }
            }

            _patchLast = null;
        }

        internal static void Print(string msg) => logger?.Log(msg);

        [System.Diagnostics.Conditional("DEBUG")]
        internal static void PrintDebug(string msg) => logger?.Log(msg);

        private static int _exceptionCount;
        internal static void PrintException(Exception ex)
        {
            if (_exceptionCount > 1000)
                return;

            logger?.LogException(ex);

            _exceptionCount++;
            if (_exceptionCount > 1000)
                logger?.Log("-- too many exceptions, future exceptions are suppressed");
        }

        internal static void PrintError(string msg) => logger?.Log("[Error/Exception] " + msg);

        internal static void Patch(Type patch)
        {
            Print("Patching " + patch.Name);
            var processor = harmony.CreateClassProcessor(patch);
            processor.Patch();
#if DEBUG
            var patches = GetConflictPatches(processor);
            if (patches != null && patches.Count > 0)
                PrintDebug("warning: potential conflict\n\t" + patches.Join(s => $"{s.owner}, {s.PatchMethod.Name}", "\n\t"));
#endif
        }

        /// <summary>
        /// Only works if all harmony attributes are on the class. Does not support bulk patches.
        /// </summary>
        internal static bool IsPatched(Type patch)
        {
            try
            {
                if (patch.GetCustomAttributes(false).FirstOrDefault(f => f is HarmonyPatch) is not HarmonyPatch attr)
                    throw new ArgumentException("Type must have HarmonyPatch attribute");

                var orignal = attr.info.GetOriginalMethod() ?? throw new Exception($"GetOriginalMethod returned null {attr.info}");
                var info = Harmony.GetPatchInfo(orignal);
                return info != null && (info.Prefixes.Any() || info.Postfixes.Any() || info.Transpilers.Any());
            }
            catch (Exception e) { PrintException(e); }
            return true;
        }

        internal static List<Patch> GetConflictPatches(PatchClassProcessor processor)
        {
            var list = new List<Patch>();

            try
            {
                foreach (var patch in processor.patchMethods)
                {
                    var orignal = patch.info.GetOriginalMethod() ?? throw new Exception($"GetOriginalMethod returned null {patch.info}");

                    // if unpatched, no conflict
                    var info = Harmony.GetPatchInfo(orignal);
                    if (info == null)
                        continue;

                    // if foreign transpilers, warn conflict
                    list.AddRange(info.Transpilers.Where(a => a.owner != harmony.Id));

                    // if foreign prefixes with return type and identical priority as own prefix, warn conflict
                    var prio = info.Prefixes.Where(w => w.owner == harmony.Id).Select(s => s.priority);
                    list.AddRange(info.Prefixes.Where(w => w.owner != harmony.Id && w.PatchMethod.ReturnType != typeof(void) && prio.Contains(w.priority)));
                }
            }
            catch (Exception e) { PrintException(e); }

            return list;
        }

        internal static bool LoadSafe(Action action)
        {
            ProcessInfo(action.Method);
#if DEBUG
            var watch = Stopwatch.StartNew();
#endif
            string name = action.Method.DeclaringType.Name + "." + action.Method.Name;

            if (CheckSetting(name))
            {
                Print($"Skipped loading {name}");
                return false;
            }

            try
            {
                Print($"Loading {name}");
                action();
#if DEBUG
                watch.Stop();
                PrintDebug("Loaded in milliseconds: " + watch.ElapsedMilliseconds);
#endif
                return true;
            }
            catch (Exception e)
            {
#if DEBUG
                watch.Stop();
#endif
                PrintException(e);
                return false;
            }
        }

        internal static bool LoadSafe(Action<bool> action, bool flag)
        {
            ProcessInfo(action.Method);
#if DEBUG
            var watch = Stopwatch.StartNew();
#endif
            string name = action.Method.DeclaringType.Name + "." + action.Method.Name;

            if (CheckSetting(name))
            {
                Print($"Skipped loading {name}");
                return false;
            }

            try
            {
                Print($"Loading {name}:{flag}");
                action(flag);
#if DEBUG
                watch.Stop();
                PrintDebug("Loaded in milliseconds: " + watch.ElapsedMilliseconds);
#endif
                return true;
            }
            catch (Exception e)
            {
#if DEBUG
                watch.Stop();
#endif
                PrintException(e);
                return false;
            }
        }

        internal static void PatchUnique(Type patch)
        {
            /// needs to have field: static bool Patched
            ProcessInfo(patch);
            if (IsPatched(patch))
            {
                Print("Skipped patching because not unique " + patch.Name);
                return;
            }

            if (PatchSafe(patch))
                patch.GetField("Patched", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(null, true);
        }

        internal static bool PatchSafe(Type patch)
        {
            ProcessInfo(patch);
            if (CheckSetting("Patch." + patch.Name))
            {
                Print("Skipped patching " + patch.Name);
                return false;
            }

            try
            {
                Patch(patch);
                return true;
            }
            catch (Exception e)
            {
                PrintException(e);
                return false;
            }
        }

        internal static void SubscribeSafe(Type type)
        {
            ProcessInfo(type);
            if (CheckSetting("Patch." + type.Name))
            {
                Print("Skipped subscribing to " + type.Name);
                return;
            }

            try
            {
                Print("Subscribing to " + type.Name);
                EventBus.Subscribe(Activator.CreateInstance(type));
            }
            catch (Exception e)
            {
                PrintException(e);
            }
        }

        internal static MethodBase GetOriginalMethod(this HarmonyMethod attr)
        {
            try
            {
                switch (attr.methodType)
                {
                    case MethodType.Normal:
                        if (attr.methodName is null)
                            return null;
                        return AccessTools.DeclaredMethod(attr.declaringType, attr.methodName, attr.argumentTypes);

                    case MethodType.Getter:
                        if (attr.methodName is null)
                            return null;
                        return AccessTools.DeclaredProperty(attr.declaringType, attr.methodName).GetGetMethod(true);

                    case MethodType.Setter:
                        if (attr.methodName is null)
                            return null;
                        return AccessTools.DeclaredProperty(attr.declaringType, attr.methodName).GetSetMethod(true);

                    case MethodType.Constructor:
                        return AccessTools.DeclaredConstructor(attr.declaringType, attr.argumentTypes);

                    case MethodType.StaticConstructor:
                        return AccessTools.GetDeclaredConstructors(attr.declaringType)
                            .Where(c => c.IsStatic)
                            .FirstOrDefault();
                }
            }
            catch (AmbiguousMatchException ex)
            {
                throw new Exception("GetOriginalMethod", ex);
            }

            return null;
        }

        private static bool CheckSetting(string name)
        {
            bool skip = patchInfos?.IsDisenabled(name) ?? false;

            if (!skip && !appliedPatches.Contains(name))
                appliedPatches.Add(name);
            if (skip && !skippedPatches.Contains(name))
                skippedPatches.Add(name);

            return skip;
        }

        private static void ProcessInfo(MemberInfo info)
        {
            if (info == null)
                return;

            if (Attribute.GetCustomAttribute(info, typeof(PatchInfoAttribute)) is not PatchInfoAttribute attr)
            {
                PrintDebug(info.Name + " has no PatchInfo");
                return;
            }

            patchInfos?.Add(attr, info);
        }

        internal static Exception NullFinalizer(Exception __exception)
        {
#if !DEBUG
            return null;
#else
            if (__exception == null)
                return null;
            try
            {
                PrintException(__exception);
            }
            catch (Exception) { }
            return null;
#endif
        }

        #endregion

        private static void ExportContent()
        {
            try
            {
                string path = Path.Combine(Main.ModPath, "readme.link");
                if (!File.Exists(path))
                    return;
                path = File.ReadAllText(path).Trim();
                if (!File.Exists(path))
                    return;

                var org = File.ReadAllLines(path).ToList();
                var res = new List<string>(org.Count + 10);

                for (int i = 0; i < org.Count; i++)
                {
                    res.Add(org[i]);
                    if (org[i] == "Content")
                    {
                        res.Add("-----------");
                        res.Add("| Option | Description | HB | Status |");
                        res.Add("|--------|-------------|----|--------|");

                        foreach (var info in patchInfos)
                        {
                            if (!info.IsHidden) // don't remove, this is to force localize this string
                            {
                                info.DisplayClass.ToString();
                                info.DisplayName.ToString();
                                info.Description.ToString();
                            }

                            if (!info.IsEvent && !info.IsHidden)
                            {
                                res.Add($"|{info.Class}.{info.Method}|{info.Description.ToString().Replace('\n', ' ')}|{info.HomebrewStr}|{info.StatusStr}|");
                            }
                        }

                        res.Add("");

                        for (; i < org.Count; i++) // skip forward until empty line is found
                            if (org[i] == "")
                                break;
                    }
                }

                if (org.Count != res.Count || !org.SequenceEqual(res))
                {
                    File.WriteAllLines(path, res);
                }
            }
            catch (Exception e) { PrintException(e); }
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

                using (StreamWriter sw = new(Path.Combine(ModPath, "player.json")))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, Game.Instance.Player.Party);
                }

                Print("Exported player data.");
            }
            catch (Exception e)
            {
                PrintException(e);
            }
        }

        private static void ExportLevelPlanData()
        {
            try
            {
                Patch_Prebuilds.GetTestPlan();

                var list = new List<List<LevelPlanData>>();

                foreach (var unit in Game.Instance.Player.PartyAndPets)
                {
                    list.Add(unit.Progression.m_LevelPlans);
                }

                Helper.Serialize(list, path: Path.Combine(Main.ModPath, "partylevelplan.json"));
                Print("Exported player data.");
            }
            catch (Exception e)
            {
                PrintException(e);
            }
        }
    }
}
