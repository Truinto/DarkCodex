using HarmonyLib;
using Kingmaker;
using Kingmaker.Achievements;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Kingmaker.UI.MVVM._PCView.ActionBar;
using Owlcat.Runtime.UI.Controls.Other;
using System.IO;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Stats;
using Newtonsoft.Json.Serialization;
using System.Text.RegularExpressions;
using System.Linq;
using Shared;
using CodexLib;

namespace DarkCodex
{
    [PatchInfo(Severity.Hidden | Severity.WIP, "Patch: Prebuild export/import", "export/import predefined build information", false)]
    [HarmonyPatch]
    public class Patch_Prebuilds
    {
        /* goal: export player build into file; import file to apply as auto-level-up
         * milestones:
         * - ~~find entry into LevelPlan~~
         * - ~~save persistance~~
         * - ~~multiclass support / auto choose class~~
         * - ~~pet companion support~~
         * - mythic class support
         * 
         * findings:
         * - prebuild is defined by AddClassLevels, for players mostly on BlueprintFeature
         * - player class's are named "Prebuild{class}FeatureList"
         * - story companion's are named "{name}PregenFeatureList"
         * - prebuilds can be used, if m_PlanChanged is false & m_LevelPlans holds valid data
         * - mythic builds m_MythicLevelPlanDisabled is false & unit has AddClassLevels component with valid data
         * 
         */

        public static Dictionary<string, string> Alias = new()
        {
            { "Class", "Kingmaker.UnitLogic.Class.LevelUp.Actions.SelectClass, Assembly-CSharp" },
            { "Archetype", "Kingmaker.UnitLogic.Class.LevelUp.Actions.AddArchetype, Assembly-CSharp" },
            { "ApplyMechanics", "Kingmaker.UnitLogic.Class.LevelUp.Actions.ApplyClassMechanics, Assembly-CSharp" },
            { "Spellbook", "Kingmaker.UnitLogic.Class.LevelUp.Actions.ApplySpellbook, Assembly-CSharp" },
            { "Attribute", "Kingmaker.UnitLogic.Class.LevelUp.Actions.SpendAttributePoint, Assembly-CSharp" },
            { "CalculateSkillPoints", "Kingmaker.UnitLogic.Class.LevelUp.Actions.ApplySkillPoints, Assembly-CSharp" },
            { "SkillPoint", "Kingmaker.UnitLogic.Class.LevelUp.Actions.SpendSkillPoint, Assembly-CSharp" },
            { "Feature", "Kingmaker.UnitLogic.Class.LevelUp.Actions.SelectFeature, Assembly-CSharp" },
            { "Spell", "Kingmaker.UnitLogic.Class.LevelUp.Actions.SelectSpell, Assembly-CSharp" },

            { "Wrapper", "DarkCodex.Patch_Prebuilds+Wrapper, DarkCodex" },
            { "SetStat", "DarkCodex.Patch_Prebuilds+SetStat, DarkCodex" },
        };

        private static Dictionary<UnitEntityData, bool> _hasPlan = new();

        public static void Clear()
        {
            _hasPlan.Clear();
        }

        public static bool HasPlan(UnitEntityData unit)
        {
            if (_hasPlan.TryGetValue(unit, out bool value))
                return value;
            value = File.Exists(Path.Combine(Main.ModPath, "builds", unit.CharacterName + ".json"));
            _hasPlan[unit] = value;
            return value;
        }

        public static void SetPlan(UnitEntityData unit)
        {
            List<LevelPlanData> list = null;
            try
            {
                var data = File.ReadAllText(Path.Combine(Main.ModPath, "builds", unit.CharacterName + ".json"));
                Regex rx = new("\"\\$type\":\\s?\"(.*?)\"");
                data = rx.Replace(data, m =>
                {
                    if (Alias.TryGetValue(m.Groups[1].Value, out string ret))
                        return $"\"$type\":\"{ret}\"";
                    return m.Value;
                });

                list = Helper.Deserialize<List<LevelPlanData>>(value: data);// path: Path.Combine("builds", unit.CharacterName + ".json"));
            }
            catch (Exception ex)
            {
                Main.PrintException(ex);
            }
            if (list == null || list.Count == 0)
                return;

            unit.Progression.m_LevelPlans.Clear();
            unit.Progression.m_LevelPlans.AddRange(list);
        }

        public static void GetTestPlan()
        {
            var list = new List<LevelPlanData>()
            {
                new LevelPlanData(2, new ILevelUpAction[] { new Wrapper(new ApplySkillPoints()), new SetStat(StatType.Strength, 20) }),
            };
            Helper.Serialize(list, path: Path.Combine(Main.ModPath, "builds", "testdata.json"));
        }

        [HarmonyPatch(typeof(LevelUpController), MethodType.Constructor, typeof(UnitEntityData), typeof(bool), typeof(LevelUpState.CharBuildMode))]
        [HarmonyPrefix]
        public static void Debug1(UnitEntityData unit, bool autoCommit, LevelUpState.CharBuildMode mode, LevelUpController __instance)
        {
            if (Main.IsInGame && Settings.State.verbose && unit.IsPlayerFaction)
                Main.PrintDebug($"LevelUpController Constructor: unit={unit.CharacterName} auto={autoCommit} mode={mode}");
        }

        [HarmonyPatch(typeof(LevelUpController), nameof(LevelUpController.ApplyLevelUpPlan))]
        [HarmonyPrefix]
        public static void IgnoreSettings(ref bool ignoreSettings)
        {
            ignoreSettings = true;
        }

        [HarmonyPatch(typeof(LevelUpController), nameof(LevelUpController.ApplyLevelUpActions))]
        [HarmonyPrefix]
        [HarmonyPriority(350)]
        public static bool OnLevelUp(UnitEntityData unit, LevelUpController __instance, ref List<ILevelUpAction> __result)
        {
            try
            {
                Regex rx = new("\"\\$type\":\\s?\"(.*?)\"");
                using var sw = new StreamWriter(Path.Combine(Main.ModPath, "Log-LevelUp.txt"), true);
                sw.WriteLine("START leveling");
                sw.WriteLine("\tname=" + __instance.Unit.CharacterName);
                sw.WriteLine("\tmode=" + __instance.State.Mode);
                sw.WriteLine("\tauto=" + __instance.AutoCommit);
                sw.WriteLine("\tlevel=" + __instance.Unit.Progression.CharacterLevel);
                sw.WriteLine("\tmythic=" + __instance.Unit.Progression.MythicLevel);
                sw.WriteLine("\ttotal=" + __instance.LevelUpActions.Count);
                foreach (var action in __instance.LevelUpActions)
                {
                    //var data = Helper.Serialize(action, indent: false);
                    var data = OwlcatJsonConvert.SerializeObject(action, Formatting.Indented);
                    data = rx.Replace(data, m =>
                    {
                        var ret = Alias.FirstOrDefault(f => f.Value == m.Groups[1].Value).Key;
                        if (ret != null)
                            return $"\"$type\":\"{ret}\"";
                        return m.Value;
                    });

                    sw.WriteLine(data);
                }
                sw.WriteLine("STOP\n");
            }
            catch (Exception e)
            {
                Main.PrintException(e);
            }

            var list = new List<ILevelUpAction>();
            foreach (var action in __instance.LevelUpActions)
            {
                if (!action.Check(__instance.State, unit.Descriptor))
                {
                    Helper.TryPrintFile(Path.Combine(Main.ModPath, "Log-LevelUp.txt"), $"ERROR: illegal {action.GetType().Name}\n");
                    PFLog.Default.Log("Invalid action: " + action, Array.Empty<object>());
                }
                else
                {
                    list.Add(action);
                    action.Apply(__instance.State, unit.Descriptor);
                    __instance.State.OnApplyAction();
                }
            }
            unit.Progression.ReapplyFeaturesOnLevelUp();
            __result = list;
            return false;
        }

        public class Wrapper : ILevelUpAction
        {
            [JsonProperty]
            public bool ForceApply;
            [JsonProperty]
            public ILevelUpAction Parent;

            public Wrapper(ILevelUpAction parent, bool forceApply = true)
            {
                this.Parent = parent;
                this.ForceApply = forceApply;
            }

            public LevelUpActionPriority Priority => Parent.Priority;

            public bool NeedUpdateUnitView => Parent.NeedUpdateUnitView;

            public void PostLoad() => Parent.PostLoad();

            public void Apply(LevelUpState state, UnitDescriptor unit)
            {
                try
                {
                    Parent.Apply(state, unit);
                }
                catch (Exception e)
                {
                    Main.PrintException(e);
                }
            }

            public bool Check(LevelUpState state, UnitDescriptor unit)
            {
                try
                {
                    if (Parent.Check(state, unit))
                        return true;
                    else if (this.ForceApply)
                    {
                        Main.Print($"Action {Parent.GetType().Name} did not fulfill requirements, but was applied anyway.");
                        return true;
                    }

                    Main.Print($"Action {Parent.GetType().Name} did not fulfill requirements and was discarded.");
                    return false;
                }
                catch (Exception e)
                {
                    Main.PrintException(e);
                    return false;
                }
            }
        }

        public class SetStat : ILevelUpAction
        {
            [JsonProperty]
            public StatType Attribute;
            [JsonProperty]
            public int Value;

            public SetStat(StatType attribute, int value)
            {
                this.Attribute = attribute;
                this.Value = value;
            }

            public LevelUpActionPriority Priority => LevelUpActionPriority.RemoveAttribute;

            public bool NeedUpdateUnitView => false;

            public void Apply(LevelUpState state, UnitDescriptor unit)
            {
                unit.Stats.GetStat(Attribute).BaseValue = Value;
            }
            public bool Check(LevelUpState state, UnitDescriptor unit) => true;
            public void PostLoad() { }
        }
    }
}
