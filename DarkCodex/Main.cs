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
            Settings state = Settings.State;

            if (StyleBox == null)
            {
                StyleBox = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter };
                StyleLine = new GUIStyle() { fixedHeight = 1, margin = new RectOffset(0, 0, 4, 4), };
                StyleLine.normal.background = new Texture2D(1, 1);
            }

            GUILayout.Label("Disclaimer: Remember that playing with mods often makes them mandatory for your save game!");
            GUILayout.Label("Legend: [F] This adds a feat. You still need to pick feats/talents for these effects. If you already picked these features, then they stay in effect regardless of the option above."
                + "\n[*] Option is enabled/disabled immediately, without restart.");

            if (Patch_AllowAchievements.Patched)
                Checkbox(ref state.allowAchievements, "[*] Allow achievements - enables achievements while mods are active and also set corresponding flag to future save files");
            else
                GUILayout.Label("Allow achievements - managed by other mod");

            Checkbox(ref Settings.State.saveMetadata, "Save Metadata (warns when loading incompatible saves) WIP");

            Checkbox(ref state.PsychokineticistStat, "Psychokineticist Main Stat");
            Checkbox(ref state.reallyFreeCost, "Limitless feats always set cost to 0, instead of reducing by 1");

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
            GUILayout.Label("Advanced: Patch Control");
            GUILayout.Label("Options in red font may not be disabled during a playthrough. Options marked with <color=red><b>✖</b></color> will not be loaded. You can use this to disable certain patches you don't like or that cause you issues ingame."
                    + " Options marked with <color=yellow><b>!</b></color> are missing patches to work properly. Check the \"Patch\" section."
                    + "\n<color=yellow>Warning: All option require a restart. Disabling options may cause your current saves to be stuck at loading, until re-enabled.</color>");
            if (GUILayout.Button("Disable all homebrew", GUILayout.ExpandWidth(false)))
                patchInfos.Where(w => w.Homebrew).ForEach(attr => patchInfos.SetEnable(false, attr));
            GUILayout.Space(10);
            Checkbox(ref state.newFeatureDefaultOn, "New features default on", b =>
            {
                restart = true;
                patchInfos.Update();
            });

            //patchInfos.Update(); // TODO: check if update can be skipped here; check if settings are applied correctly; put favorite settings on top

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
                    GUILayout.Label(info.Class);

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
                GUILayout.Label(info.DisplayName.Grey(info.IsHidden).Red(info.IsDangerous), GUILayout.Width(300));
                GUILayout.Label(info.Description, GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
            GUILayout.Label("Debug");

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
                        sw.WriteLine($"|{info.Class}.{info.Method}|{info.Description.Replace('\n', ' ')}|{info.HomebrewStr}|{info.StatusStr}|");
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

            if (GUILayout.Button("Save settings!"))
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
                restart = false;
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
            if (str == null) str = "couldn't read";

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

        static partial void OnLoad(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnHideGUI = OnHideGUI;

            MasterPatch.Run(typeof(CodexLib.BpCache)); // this must run very early
            //Helper.Patch(typeof(Patch_SaveExtension)); // TODO: save extension

            //harmony.PatchAll(typeof(Main).Assembly);
            //harmony.Patch(HarmonyLib.AccessTools.Method(typeof(EnumUtils), nameof(EnumUtils.GetMaxValue), null, new Type[] { typeof(ActivatableAbilityGroup) }),
            //    postfix: new HarmonyMethod(typeof(Patch_ActivatableAbilityGroup).GetMethod("Postfix")));
        }

        static partial void OnMainMenu()
        {
            if (Settings.State.showBootupWarning)
            {
                UIUtility.ShowMessageBox("Installed Dark Codex.\nIf you want to disable certain features, do it now. Disabling 'red' features during a playthrough is not possible. Enabling features can be done at any time.", MessageModalBase.ModalType.Message, a => { }, null, 0, null, null, null);
                Settings.State.showBootupWarning = false;
                Settings.State.TrySave();
            }
        }

        static partial void OnBlueprintsLoaded()
        {
#if DEBUG
            using var scope = new Scope(modPath: Main.ModPath, logger: Main.logger, harmony: Main.harmony, allowGuidGeneration: true);
#else
            using var scope = new Scope(Main.ModPath, Main.logger, harmony, false);
#endif
            MasterPatch.Run();
            Print("Loading Dark Codex");
            patchInfos = new(Settings.State);

            // for Debug
#if DEBUG
            PatchSafe(typeof(DEBUG.WatchCalculateParams));
            PatchSafe(typeof(DEBUG.WatchSelectiveMetamagic));
            PatchSafe(typeof(DEBUG.WatchWeaponEnchantment));
            //PatchSafe(typeof(DEBUG.WatchSharedValue));
            PatchSafe(typeof(DEBUG.Settlement1));
            PatchSafe(typeof(DEBUG.Settlement2));
            PatchSafe(typeof(DEBUG.ArmyLeader1));
            PatchSafe(typeof(DEBUG.SpellReach));
            PatchSafe(typeof(Patch_Prebuilds));
            //PatchSafe(typeof(Patch_SaveExtension));
            LoadSafe(General.CreatePoison);
            LoadSafe(Kineticist.CreateElementalAscetic);
            LoadSafe(MartialArt.CreatePaladinVirtuousBravo);
            LoadSafe(Kineticist.FixBladeWhirlwind);
            LoadSafe(Spellcasters.CreateChannelForm);
            LoadSafe(Spells.CreateChillTouch);
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
            LoadSafe(Spells.PatchVarious);

            // General
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

            // MartialArt
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
            LoadSafe(General.CreateSpellPerfection); // keep last
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

        static partial void OnBlueprintsLoadedLast()
        {
#if DEBUG
            using var scope = new Scope(Main.ModPath, Main.logger, harmony, true);
#else
            using var scope = new Scope(Main.ModPath, Main.logger, harmony, false);
#endif

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
            Helper.ExportStrings();
            KineticistTree.Instance.Validate();
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
                path = File.ReadAllText(Path.Combine(Main.ModPath, "readme.link"));
                path = path.Trim();
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
                            if (!info.IsEvent && !info.IsHidden)
                                res.Add($"|{info.Class}.{info.Method}|{info.Description.Replace('\n', ' ')}|{info.HomebrewStr}|{info.StatusStr}|");

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
