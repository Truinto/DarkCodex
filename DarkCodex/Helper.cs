using Config;
using DarkCodex.Components;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.EquipmentEnchants;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Designers.Mechanics.Prerequisites;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Log;
using Kingmaker.UI.Log.CombatLog_ThreadSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using static Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell;

namespace DarkCodex
{
    public static class Helper
    {
        #region Other

        public static int MinMax(this int number, int min, int max)
        {
            return Math.Max(min, Math.Min(number, max));
        }

        public static void AttackRollPrint(RuleAttackRoll attackRoll)
        {
            if (attackRoll != null)
            {
                attackRoll.SuspendCombatLog = false;
                if (!attackRoll.Initiator.IsInGame || !attackRoll.Target.IsInGame || attackRoll.AutoHit)
                {
                    return;
                }
                using (GameLogContext.Scope)
                {
                    CombatLogMessage combatLogMessage = attackRoll.AttackLogEntry ?? ReportLogMessageBuilderAbstract.Strings.Attack.GetData(attackRoll);
                    CombatLogMessage combatLogMessage2 = attackRoll.ParryLogEntry ?? ReportLogMessageBuilderAbstract.Strings.Attack.GetParryData(attackRoll);
                    if (combatLogMessage2 != null)
                    {
                        //this.ReportCombatLogManager.ManageCombatMessageData(combatLogMessage2, attackRoll.Initiator, attackRoll.Target);
                    }
                    if (combatLogMessage != null && attackRoll.Result != AttackResult.Parried)
                    {
                        //this.ReportCombatLogManager.ManageCombatMessageData(combatLogMessage, attackRoll.Initiator, attackRoll.Target);
                    }
                }
            }
            throw new NotImplementedException();
        }

        #endregion

        #region Patching

        /// <summary>Needs ManualPatch attribute.</summary>
        public static void Patch(Type patch, bool _)
        {
            Print("ManualPatch " + patch.Name);
            var manual = patch.GetCustomAttributes(false).FirstOrDefault(f => f is ManualPatchAttribute) as ManualPatchAttribute;
            if (manual == null)
                throw new ArgumentException("Type must have ManualPatchAttribute");

            var prefix = patch.GetMethod("Prefix");
            var postfix = patch.GetMethod("Postfix");

            var attr = new HarmonyPatch(manual.declaringType, manual.methodName, manual.methodType);
            Main.harmony.Patch(
                        original: GetOriginalMethod(attr.info),
                        prefix: prefix != null ? new HarmonyMethod(prefix) : null,
                        postfix: postfix != null ? new HarmonyMethod(postfix) : null);

            patch.GetField("Patched", BindingFlags.Static | BindingFlags.Public)?.SetValue(null, true);
        }

        /// <summary>Needs HarmonyPatch attribute.</summary>
        public static void Patch(Type patch)
        {
            Print("Patching " + patch.Name);
            Main.harmony.CreateClassProcessor(patch).Patch();
        }

        public static void Unpatch(Type patch, HarmonyPatchType patchType)
        {
            Print("Unpatch " + patch);
            var attr = patch.GetCustomAttributes(false).FirstOrDefault(f => f is HarmonyPatch) as HarmonyPatch;
            if (attr == null)
                return;

            MethodBase orignal = attr.info.GetOriginalMethod();
            Main.harmony.Unpatch(orignal, patchType, Main.harmony.Id);
        }

        /// <summary>Only works with HarmonyPatch.</summary>
        public static bool IsPatched(Type patch)
        {
            var attr = patch.GetCustomAttributes(false).FirstOrDefault(f => f is HarmonyPatch) as HarmonyPatch;
            if (attr == null)
                throw new ArgumentException("Type must have HarmonyPatch attribute");

            MethodBase orignal = attr.info.GetOriginalMethod();
            var info = Harmony.GetPatchInfo(orignal);
            return info != null && (info.Prefixes?.Any() == true || info.Postfixes?.Any() == true || info.Transpilers?.Any() == true);
        }

        public static MethodBase GetOriginalMethod(this HarmonyMethod attr)
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
                throw new Exception("GetOriginalMethod " + ex.ToString());
            }

            return null;
        }

        #endregion

        #region Arrays
        public static T Create<T>(Action<T> action = null) where T : ScriptableObject
        {
            var result = ScriptableObject.CreateInstance<T>();
            if (action != null)
            {
                action(result);
            }
            return result;
        }

        public static T Instantiate<T>(T obj, Action<T> action = null) where T : ScriptableObject
        {
            var result = ScriptableObject.Instantiate<T>(obj);
            if (action != null)
            {
                action(result);
            }
            return result;
        }

        public static T CreateCopy<T>(T original, Action<T> action = null) where T : UnityEngine.Object
        {
            var clone = UnityEngine.Object.Instantiate(original);
            if (action != null)
            {
                action(clone);
            }
            return clone;
        }

        public static T[] ObjToArray<T>(this T obj)
        {
            if (obj == null) return null;
            return new T[] { obj };
        }

        public static T[] ToArray<T>(params T[] objs)
        {
            return objs;
        }

        /// <summary>Appends objects on array.</summary>
        public static T[] Append<T>(T[] orig, params T[] objs)
        {
            if (orig == null) orig = new T[0];

            int i, j;
            T[] result = new T[orig.Length + objs.Length];
            for (i = 0; i < orig.Length; i++)
                result[i] = orig[i];
            for (j = 0; i < result.Length; i++)
                result[i] = objs[j++];
            return result;
        }

        /// <summary>Appends objects on array and overwrites the original.</summary>
        public static T[] AppendAndReplace<T>(ref T[] orig, params T[] objs)
        {
            if (orig == null) orig = new T[0];

            int i, j;
            T[] result = new T[orig.Length + objs.Length];
            for (i = 0; i < orig.Length; i++)
                result[i] = orig[i];
            for (j = 0; i < result.Length; i++)
                result[i] = objs[j++];
            orig = result;
            return result;
        }

        public static T[] AppendAndReplace<T>(ref T[] orig, List<T> objs)
        {
            if (orig == null) orig = new T[0];

            T[] result = new T[orig.Length + objs.Count];
            int i;
            for (i = 0; i < orig.Length; i++)
                result[i] = orig[i];
            foreach (var obj in objs)
                result[i++] = obj;
            orig = result;
            return result;
        }

        #endregion

        #region Log

        private static bool _sb2_flag;
        private static StringBuilder _sb2 = new StringBuilder();

        /// <summary>Only prints message, if compiled on DEBUG.</summary>
        [System.Diagnostics.Conditional("DEBUG")]
        internal static void PrintDebug(string msg)
        {
            Main.logger?.Log(msg);
        }

        internal static void Print(string msg)
        {
            Main.logger?.Log(msg);
        }

        internal static void PrintException(Exception ex)
        {
            Main.logger?.LogException(ex);
        }

        private static readonly FieldInfo _fieldLabel = AccessTools.Field(typeof(Label), "label");
        [System.Diagnostics.Conditional("DEBUG")]
        internal static void PrintInstruction(CodeInstruction code, string str = "")
        {
            var labels = code.labels.Select(s => (int)_fieldLabel.GetValue(s)).Join();
            if (code.operand is Label label)
                Print($"{str} code:{code.opcode} goto:{_fieldLabel.GetValue(label)} labels:{labels}");
            else
                Print($"{str} code:{code.opcode} operand:{code.operand} type:{code.operand?.GetType().FullName} labels:{labels}");
        }

        [System.Diagnostics.Conditional("DEBUG")]
        internal static void PrintJoinDebug(string msg = "", string delimiter = ", ", bool flush = false)
        {
            PrintJoin(msg, delimiter, flush);
        }

        /// <summary>
        /// Joins text before printing.
        /// </summary>
        /// <param name="msg">Part of text to print.</param>
        /// <param name="delimiter">Delimiter to use. Set null for prefix.</param>
        /// <param name="flush">Flushes BEFORE appending msg.</param>
        internal static void PrintJoin(string msg = "", string delimiter = ", ", bool flush = false)
        {
            if (flush)
            {
                if (_sb2_flag)
                {
                    Print(_sb2.ToString());
                }

                _sb2.Clear();
                _sb2_flag = false;

                _sb2.Append(msg);
                return;
            }

            if (_sb2_flag && delimiter != null)
                _sb2.Append(delimiter);
            else if (delimiter != null)
                _sb2_flag = true;

            _sb2.Append(msg);
        }

        #endregion

        #region Strings

        public static LocalizedString GetString(string guid)
        {
            return new LocalizedString { Key = "" };
        }

        private static SHA1 _SHA = SHA1Managed.Create();
        private static StringBuilder _sb1 = new StringBuilder();
        private static Locale _lastLocale = Locale.enGB;
        private static Dictionary<string, string> _mappedStrings;
        public static LocalizedString CreateString(this string value, string key = null)
        {
            if (value == null || value == "")
                return new LocalizedString { Key = "" };

            if (key == null)
            {
                var sha = _SHA.ComputeHash(Encoding.UTF8.GetBytes(value));
                for (int i = 0; i < sha.Length; i++)
                    _sb1.Append(sha[i].ToString("x2"));
                key = _sb1.ToString();
                _sb1.Clear();
            }

            var pack = LocalizationManager.CurrentPack.Strings;
            if (LocalizationManager.CurrentPack.Locale != _lastLocale)
            {
                _lastLocale = LocalizationManager.CurrentPack.Locale;
                try
                {
                    _mappedStrings = new JsonManager().Deserialize<Dictionary<string, string>>(Path.Combine(Main.ModPath, LocalizationManager.CurrentPack.Locale.ToString() + ".json"));
                    foreach (var entry in _mappedStrings)
                        pack[entry.Key] = entry.Value;
                    _mappedStrings = null;
                }
                catch (Exception e)
                {
                    Print($"Could not read lanaguage file for {LocalizationManager.CurrentPack.Locale}: {e.Message}");
                }
            }

            if (!pack.ContainsKey(key))
            {
                pack.Add(key, value);
                _saveString(key, value);
            }

            return new LocalizedString { Key = key };
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void _saveString(string key, string value)
        {
            if (_mappedStrings == null)
                _mappedStrings = new Dictionary<string, string>();
            _mappedStrings[key] = value;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void ExportStrings()
        {
            if (_mappedStrings == null)
                return;

            Dictionary<string, string> oldmap = null;

            try
            {
                oldmap = new JsonManager().Deserialize<Dictionary<string, string>>(Path.Combine(Main.ModPath, "enGB.json"));

                foreach (var entry in _mappedStrings)
                    if (!oldmap.ContainsKey(entry.Key))
                        oldmap.Add(entry.Key, entry.Value);
            }
            catch (Exception) { }

            try
            {
                new JsonManager().Serialize(oldmap ?? _mappedStrings, Path.Combine(Main.ModPath, "enGB.json"));
                _mappedStrings = null;
            }
            catch (Exception e)
            {
                Print($"Failed export lanaguage file: {e.Message}");
            }
        }

        #endregion

        #region Components

        public static T AddComponents<T>(this T obj, params BlueprintComponent[] components) where T : BlueprintScriptableObject
        {
            foreach (var comp in components)
                comp.name = $"${comp.GetType().Name}${obj.AssetGuid}";

            obj.ComponentsArray = Append(obj.ComponentsArray, components);
            return obj;
        }

        public static T SetComponents<T>(this T obj, params BlueprintComponent[] components) where T : BlueprintScriptableObject
        {
            foreach (var comp in components)
                comp.name = $"${comp.GetType().Name}${obj.AssetGuid}";

            obj.ComponentsArray = components;
            return obj;
        }

        public static T ReplaceComponent<T, TOrig, TRep>(this T obj, TOrig original, TRep replacement) where T : BlueprintScriptableObject where TOrig : BlueprintComponent where TRep : BlueprintComponent
        {
            replacement.name = $"${replacement.GetType().Name}${obj.AssetGuid}";

            for (int i = 0; i < obj.ComponentsArray.Length; i++)
            {
                if (obj.ComponentsArray[i] is TOrig)
                {
                    obj.ComponentsArray[i] = replacement;
                    break;
                }
            }
            return obj;
        }

        public static T RemoveComponents<T, TRemove>(this T obj, TRemove _) where T : BlueprintScriptableObject where TRemove : BlueprintComponent
        {
            var list = obj.ComponentsArray.ToList();
            list.RemoveAll(r => r is TRemove);
            obj.ComponentsArray = list.ToArray();
            return obj;
        }

        public static List<BlueprintAbilityReference> GetBaseAndVariants(this List<BlueprintAbilityReference> source, Func<BlueprintAbility, bool> predicate = null)
        {
            var result = new List<BlueprintAbilityReference>();

            for (int i = 0; i < source.Count; i++)
            {
                result.Add(source[i]);

                var variants = source[i].Get().GetComponent<AbilityVariants>()?.m_Variants ?? Array.Empty<BlueprintAbilityReference>();
                foreach (var variant in variants)
                {
                    if (predicate == null || predicate(variant.Get()))
                        result.Add(variant);
                }
            }

            return result;
        }

        public static List<BlueprintAbilityReference> GetVariants(this List<BlueprintAbilityReference> source, Func<BlueprintAbility, bool> predicate = null)
        {
            var result = new List<BlueprintAbilityReference>();

            for (int i = 0; i < source.Count; i++)
            {
                var variants = source[i].Get().GetComponent<AbilityVariants>()?.m_Variants ?? Array.Empty<BlueprintAbilityReference>();
                foreach (var variant in variants)
                {
                    if (predicate == null || predicate(variant.Get()))
                        result.Add(variant);
                }
            }

            return result;
        }

        public static void AddFeature(this BlueprintArchetype obj, int level, BlueprintFeatureBase feature)
        {
            var levelentry = obj.AddFeatures.FirstOrDefault(f => f.Level == level);
            if (levelentry != null)
                levelentry.m_Features.Add(feature.ToRef());
            else
                AppendAndReplace(ref obj.AddFeatures, CreateLevelEntry(level, feature));
        }

        public static void RemoveFeature(this BlueprintArchetype obj, int level, BlueprintFeatureBase feature)
        {
            var levelentry = obj.RemoveFeatures.FirstOrDefault(f => f.Level == level);
            if (levelentry != null)
                levelentry.m_Features.Add(feature.ToRef());
            else
                AppendAndReplace(ref obj.RemoveFeatures, CreateLevelEntry(level, feature));
        }

        public static void AddFeature(this BlueprintProgression obj, int level, BlueprintFeatureBase feature, string pairWithGuid = null)
        {
            var levelentry = obj.LevelEntries.FirstOrDefault(f => f.Level == level);
            if (levelentry != null)
                levelentry.m_Features.Add(feature.ToRef());
            else
                AppendAndReplace(ref obj.LevelEntries, CreateLevelEntry(level, feature));

            if (pairWithGuid != null)
            {
                var pairGuid = BlueprintGuid.Parse(pairWithGuid);
                foreach (var ui in obj.UIGroups)
                {
                    if (ui.m_Features.Any(a => a.deserializedGuid == pairGuid))
                    {
                        ui.m_Features.Add(feature.ToRef());
                        break;
                    }
                }
            }

        }

        #endregion

        #region Context Values

        public static ContextValue CreateContextValue(int value)
        {
            return (ContextValue)value;
        }

        public static ContextValue CreateContextValue(AbilityRankType value = AbilityRankType.Default)
        {
            return new ContextValue() { ValueType = ContextValueType.Rank, ValueRank = value };
        }

        public static ContextValue CreateContextValue(AbilitySharedValue value)
        {
            return new ContextValue() { ValueType = ContextValueType.Shared, ValueShared = value };
        }

        public static ContextDurationValue CreateContextDurationValue(ContextValue diceCount = null, DiceType dice = DiceType.Zero, ContextValue bonus = null, DurationRate rate = DurationRate.Rounds)
        {
            return new ContextDurationValue()
            {
                DiceCountValue = diceCount ?? 0,
                DiceType = dice,
                BonusValue = bonus ?? 0,
                Rate = rate
            };
        }

        public static ContextDurationValue CreateContextDurationValue(AbilityRankType diceRank, DiceType dice = DiceType.One, int bonus = 0, DurationRate rate = DurationRate.Rounds)
        {
            return new ContextDurationValue()
            {
                DiceCountValue = CreateContextValue(diceRank),
                DiceType = dice,
                BonusValue = bonus,
                Rate = rate
            };
        }

        #endregion

        #region Create Advanced

        private static MethodInfo _memberwiseClone = AccessTools.Method(typeof(object), "MemberwiseClone");
        public static T Clone<T>(this T obj, string name, string guid = null) where T : SimpleBlueprint
        {
            if (guid == null)
                guid = GuidManager.i.Get(name);

            var result = (T)_memberwiseClone.Invoke(obj, null);
            AddAsset(result, guid);
            return result;

            //var result = Activator.CreateInstance<T>();
            //var fields = typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            //foreach (var field in fields)
            //    field.SetValue(result, field.GetValue(obj));
            //return result;
        }

        public static BlueprintFeatureSelection _basicfeats;
        public static void AddFeats(params BlueprintFeature[] feats)
        {
            if (_basicfeats == null)
                _basicfeats = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("247a4068296e8be42890143f451b4b45");
            Helper.AppendAndReplace(ref _basicfeats.m_AllFeatures, feats.ToRef());
        }

        private static BlueprintFeatureSelection _mythicfeats;
        private static BlueprintFeatureSelection _mythictalents;
        private static BlueprintFeatureSelection _mythicextratalents;
        public static void AddMythicTalent(BlueprintFeature feat)
        {
            if (_mythictalents == null)
                _mythictalents = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("ba0e5a900b775be4a99702f1ed08914d");
            if (_mythicextratalents == null)
                _mythicextratalents = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("8a6a511c55e67d04db328cc49aaad2b8");

            Helper.AppendAndReplace(ref _mythictalents.m_AllFeatures, feat.ToRef());
            _mythicextratalents.m_AllFeatures = _mythictalents.m_AllFeatures;
        }

        public static void AddMythicFeat(BlueprintFeature feat)
        {
            if (_mythicfeats == null)
                _mythicfeats = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("9ee0f6745f555484299b0a1563b99d81");

            Helper.AppendAndReplace(ref _mythicfeats.m_AllFeatures, feat.ToRef());
        }

        private static BlueprintFeatureSelection _roguefeats;
        private static BlueprintFeatureSelection _slayerfeats1;
        private static BlueprintFeatureSelection _slayerfeats2;
        private static BlueprintFeatureSelection _slayerfeats3;
        private static BlueprintFeatureSelection _vivsectionistfeats3;
        public static void AddRogueFeat(BlueprintFeature feat)
        {
            if (_roguefeats == null)
                _roguefeats = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("c074a5d615200494b8f2a9c845799d93");
            if (_slayerfeats1 == null)
                _slayerfeats1 = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("04430ad24988baa4daa0bcd4f1c7d118");
            if (_slayerfeats2 == null)
                _slayerfeats2 = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("43d1b15873e926848be2abf0ea3ad9a8");
            if (_slayerfeats3 == null)
                _slayerfeats3 = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("913b9cf25c9536949b43a2651b7ffb66");
            if (_vivsectionistfeats3 == null)
                _vivsectionistfeats3 = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("67f499218a0e22944abab6fe1c9eaeee");

            var reference = feat.ToRef();

            Helper.AppendAndReplace(ref _roguefeats.m_AllFeatures, reference);
            Helper.AppendAndReplace(ref _slayerfeats1.m_AllFeatures, reference);
            Helper.AppendAndReplace(ref _slayerfeats2.m_AllFeatures, reference);
            Helper.AppendAndReplace(ref _slayerfeats3.m_AllFeatures, reference);
            Helper.AppendAndReplace(ref _vivsectionistfeats3.m_AllFeatures, reference);
        }

        public static bool AddToAbilityVariants(this BlueprintAbility parent, params BlueprintAbility[] variants)
        {
            var comp = parent.GetComponent<AbilityVariants>();

            Helper.AppendAndReplace(ref comp.m_Variants, variants.ToRef());

            foreach (var v in variants)
            {
                v.Parent = parent;
            }
            return true;
        }

        public static BlueprintBuff Flags(this BlueprintBuff buff, bool? hidden = null, bool? stayOnDeath = null)
        {
            if (hidden != null)
            {
                if (hidden.Value)
                    buff.m_Flags |= BlueprintBuff.Flags.HiddenInUi;
                else
                    buff.m_Flags &= BlueprintBuff.Flags.HiddenInUi;
            }

            if (stayOnDeath != null)
            {
                if (stayOnDeath.Value)
                    buff.m_Flags |= BlueprintBuff.Flags.StayOnDeath;
                else
                    buff.m_Flags &= BlueprintBuff.Flags.StayOnDeath;
            }
            return buff;
        }

        public static void AddArcaneVendorItem(BlueprintItemReference item, int amount = 1)
        {
            var vendor_table = ResourcesLibrary.TryGetBlueprint<BlueprintSharedVendorTable>("5450d563aab78134196ee9a932e88671"); //ArcaneScrollsVendorTableI
            vendor_table.AddComponents(new LootItemsPackFixed() { m_Item = new LootItem() { m_Item = item }, m_Count = amount });
        }

        public static ContextCondition[] MakeConditionHasNoBuff(params BlueprintBuff[] buffs)
        {
            if (buffs == null || buffs[0] == null) throw new ArgumentNullException();
            var result = new ContextCondition[buffs.Length];

            for (int i = 0; i < result.Length; i++)
            {
                var buff = new ContextConditionHasBuff();
                buff.m_Buff = buffs[i].ToRef();
                buff.Not = true;
                result[i] = buff;
            }

            return result;
        }

        public static AddAreaEffect MakeAddAreaEffect(this BlueprintAbilityAreaEffect area)
        {
            var result = new AddAreaEffect();
            result.m_AreaEffect = area.ToRef();
            return result;
        }

        /// <summary>Adds a fact, but only fact not already granted through other means.</summary>
        public static AddFeatureIfHasFact MakeAddFactSafe(BlueprintUnitFactReference feature)
        {
            var result = new AddFeatureIfHasFact();
            result.m_CheckedFact = feature;
            result.m_Feature = feature;
            result.Not = true;
            return result;
        }

        public static BlueprintAbility TargetPoint(this BlueprintAbility ability, CastAnimationStyle animation = CastAnimationStyle.Directional, bool self = false)
        {
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.CanTargetEnemies = true;
            ability.CanTargetPoint = true;
            ability.CanTargetFriends = true;
            ability.CanTargetSelf = self;
            ability.Animation = animation;
            return ability;
        }
        public static BlueprintAbility TargetEnemy(this BlueprintAbility ability, CastAnimationStyle animation = CastAnimationStyle.Directional)
        {
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.CanTargetEnemies = true;
            ability.CanTargetPoint = false;
            ability.CanTargetFriends = false;
            ability.CanTargetSelf = false;
            ability.Animation = animation;
            return ability;
        }
        public static BlueprintAbility TargetSelf(this BlueprintAbility ability, CastAnimationStyle animation = CastAnimationStyle.Omni)
        {
            ability.CanTargetEnemies = false;
            ability.CanTargetPoint = false;
            ability.CanTargetFriends = false;
            ability.CanTargetSelf = true;
            ability.Animation = animation;
            return ability;
        }

        #endregion

        #region Create

        public static void AddAsset(this SimpleBlueprint bp, string guid) => AddAsset(bp, BlueprintGuid.Parse(guid));
        public static void AddAsset(this SimpleBlueprint bp, Guid guid) => AddAsset(bp, new BlueprintGuid(guid));
        public static void AddAsset(this SimpleBlueprint bp, BlueprintGuid guid)
        {
            if (guid == BlueprintGuid.Empty)
                throw new ArgumentException("GUID must not be empty!");
            bp.AssetGuid = guid;
            ResourcesLibrary.BlueprintsCache.AddCachedBlueprint(guid, bp);
        }

        public static ContextActionCastSpell CreateContextActionCastSpell(BlueprintAbilityReference spell, bool castByTarget = false)
        {
            var result = new ContextActionCastSpell();
            result.m_Spell = spell;
            result.CastByTarget = castByTarget;
            return result;
        }

        public static ContextActionMeleeAttack CreateContextActionMeleeAttack(bool isPoint = false)
        {
            var result = new ContextActionMeleeAttack();
            result.SelectNewTarget = isPoint;
            result.AutoHit = false;
            result.IgnoreStatBonus = false;
            result.AutoCritThreat = false;
            result.AutoCritConfirmation = false;
            return result;
        }

        public static AddStatBonus CreateAddStatBonus(int value, StatType stat, ModifierDescriptor descriptor = ModifierDescriptor.UntypedStackable)
        {
            var result = new AddStatBonus();
            result.Value = value;
            result.Stat = stat;
            result.Descriptor = descriptor;
            return result;
        }

        public static AddInitiatorAttackRollTrigger CreateAddInitiatorAttackRollTrigger(ActionList Action, bool OnOwner = false, bool SneakAttack = false, bool OnlyHit = true, bool CriticalHit = false, bool CheckWeapon = false, WeaponCategory WeaponCategory = 0)
        {
            var result = new AddInitiatorAttackRollTrigger();
            result.Action = Action;
            result.OnOwner = OnOwner;
            result.SneakAttack = SneakAttack;
            result.OnlyHit = OnlyHit;
            result.CriticalHit = CriticalHit;
            result.CheckWeapon = CheckWeapon;
            result.WeaponCategory = WeaponCategory;
            return result;
        }

        public static PrerequisiteFullStatValue CreatePrerequisiteFullStatValue(StatType stat, int value = 0)
        {
            var result = new PrerequisiteFullStatValue();
            result.Stat = stat;
            result.Value = value;
            return result;
        }

        public static AddCondition CreateAddCondition(UnitCondition condition)
        {
            return new AddCondition
            {
                Condition = condition
            };
        }

        public static SpellDescriptorComponent CreateSpellDescriptorComponent(SpellDescriptor descriptor)
        {
            return new SpellDescriptorComponent { Descriptor = descriptor };
        }

        public static AbilityTargetHasFact CreateAbilityTargetHasFact(bool inverted, params BlueprintUnitFactReference[] facts)
        {
            var result = new AbilityTargetHasFact();
            result.Inverted = inverted;
            result.m_CheckedFacts = facts;
            return result;
        }

        public static ContextRankConfig CreateContextRankConfig(ContextRankBaseValueType baseValueType, ContextRankProgression progression = ContextRankProgression.AsIs, AbilityRankType type = AbilityRankType.Default, int? min = null, int? max = null, int startLevel = 0, int stepLevel = 0, bool exceptClasses = false, StatType stat = StatType.Unknown, BlueprintUnitPropertyReference customProperty = null, BlueprintCharacterClassReference[] classes = null, BlueprintArchetypeReference[] archetypes = null, BlueprintFeatureReference feature = null, BlueprintFeatureReference[] featureList = null/*, (int, int)[] customProgression = null*/)
        {
            var result = new ContextRankConfig();
            result.m_Type = type;
            result.m_BaseValueType = baseValueType;
            result.m_Progression = progression;
            result.m_UseMin = min.HasValue;
            result.m_Min = min ?? 0;
            result.m_UseMax = max.HasValue;
            result.m_Max = max ?? 20;
            result.m_StartLevel = startLevel;
            result.m_StepLevel = stepLevel;
            result.m_Feature = feature;
            result.m_CustomProperty = customProperty;
            result.m_Stat = stat;
            result.m_Class = classes ?? Array.Empty<BlueprintCharacterClassReference>();
            result.Archetype = ToRef<BlueprintArchetypeReference>(null);
            result.m_AdditionalArchetypes = archetypes ?? Array.Empty<BlueprintArchetypeReference>();
            result.m_FeatureList = featureList ?? Array.Empty<BlueprintFeatureReference>();

            return result;
        }

        public static ContextCalculateSharedValue CreateContextCalculateSharedValue(AbilitySharedValue ValueType = AbilitySharedValue.Damage, ContextDiceValue Value = null, double Modifier = 1.0)
        {
            if (Value == null)
                Value = CreateContextDiceValue(DiceType.One, AbilityRankType.DamageDice, AbilityRankType.DamageBonus);

            var result = new ContextCalculateSharedValue();
            result.ValueType = ValueType;
            result.Value = Value;
            result.Modifier = Modifier;
            return result;
        }

        public static ContextDiceValue CreateContextDiceValue(DiceType dice, ContextValue diceCount = null, ContextValue bonus = null)
        {
            return new ContextDiceValue()
            {
                DiceType = dice,
                DiceCountValue = diceCount ?? CreateContextValue(),
                BonusValue = bonus ?? 0
            };
        }

        public static ContextDiceValue CreateContextDiceValue(DiceType dice, AbilityRankType dicecount, AbilityRankType bonus)
        {
            return new ContextDiceValue()
            {
                DiceType = dice,
                DiceCountValue = CreateContextValue(dicecount),
                BonusValue = CreateContextValue(bonus)
            };
        }

        public static ContextActionDealDamage CreateContextActionDealDamage(PhysicalDamageForm physical, ContextDiceValue damage, bool isAoE = false, bool halfIfSaved = false, bool IgnoreCritical = false, bool half = false, bool alreadyHalved = false, AbilitySharedValue sharedValue = 0, bool readShare = false, bool writeShare = false)
        {
            // physical damage
            var c = new ContextActionDealDamage();
            c.DamageType = new DamageTypeDescription()
            {
                Type = DamageType.Physical,
                Common = new DamageTypeDescription.CommomData(),
                Physical = new DamageTypeDescription.PhysicalData() { Form = physical }
            };
            c.Duration = CreateContextDurationValue();
            c.Value = damage;
            c.IsAoE = isAoE;
            c.HalfIfSaved = halfIfSaved;
            c.IgnoreCritical = IgnoreCritical;
            c.Half = half;
            c.AlreadyHalved = alreadyHalved;
            c.ReadPreRolledFromSharedValue = readShare;
            c.PreRolledSharedValue = readShare ? sharedValue : 0;
            c.WriteResultToSharedValue = writeShare;
            c.ResultSharedValue = writeShare ? sharedValue : 0;
            return c;
        }

        public static ContextActionDealDamage CreateContextActionDealDamage(DamageEnergyType energy, ContextDiceValue damage, bool isAoE = false, bool halfIfSaved = false, bool IgnoreCritical = false, bool half = false, bool alreadyHalved = false, AbilitySharedValue sharedValue = 0, bool readShare = false, bool writeShare = false)
        {
            // energy damage
            var c = new ContextActionDealDamage();
            c.DamageType = new DamageTypeDescription()
            {
                Type = DamageType.Energy,
                Energy = energy,
                Common = new DamageTypeDescription.CommomData(),
                Physical = new DamageTypeDescription.PhysicalData()
            };
            c.Duration = CreateContextDurationValue();
            c.Value = damage;
            c.IsAoE = isAoE;
            c.HalfIfSaved = halfIfSaved;
            c.IgnoreCritical = IgnoreCritical;
            c.Half = half;
            c.AlreadyHalved = alreadyHalved;
            c.ReadPreRolledFromSharedValue = readShare;
            c.PreRolledSharedValue = readShare ? sharedValue : 0;
            c.WriteResultToSharedValue = writeShare;
            c.ResultSharedValue = writeShare ? sharedValue : 0;
            return c;
        }

        public static AbilityCasterHasFacts CreateAbilityCasterHasFacts(bool NeedsAll = false, params BlueprintUnitFactReference[] Facts)
        {
            var result = new AbilityCasterHasFacts();
            result.m_Facts = Facts;
            result.NeedsAll = NeedsAll;
            return result;
        }

        public static AbilityDeliverProjectile CreateAbilityDeliverProjectile(BlueprintProjectileReference projectile, AbilityProjectileType type = AbilityProjectileType.Simple, BlueprintItemWeaponReference weapon = null, Feet length = default(Feet), Feet width = default(Feet))
        {
            var result = new AbilityDeliverProjectile();
            result.m_Projectiles = projectile.ObjToArray();
            result.Type = type;
            result.m_Length = length;
            result.m_LineWidth = width;
            result.m_Weapon = weapon;
            result.Type = AbilityProjectileType.Line;
            result.NeedAttackRoll = true;
            return result;
        }

        public static AbilityShowIfCasterHasFact CreateAbilityShowIfCasterHasFact(BlueprintUnitFactReference UnitFact)
        {
            var result = new AbilityShowIfCasterHasFact();
            result.m_UnitFact = UnitFact;
            return result;
        }

        public static AbilityRequirementActionAvailable CreateAbilityRequirementActionAvailable(bool Not, ActionType Action, float Amount = 3f)
        {
            var result = new AbilityRequirementActionAvailable();
            result.Not = Not;
            result.Action = Action;
            result.Amount = Amount;
            return result;
        }

        public static AbilityEffectRunAction CreateAbilityEffectRunAction(SavingThrowType save = SavingThrowType.Unknown, params GameAction[] actions)
        {
            if (actions == null || actions[0] == null) throw new ArgumentNullException();
            var result = new AbilityEffectRunAction();
            result.SavingThrowType = save;
            result.Actions = new ActionList() { Actions = actions };
            return result;
        }

        public static ContextActionRemoveBuff CreateContextActionRemoveBuff(BlueprintBuff buff, bool toCaster = false)
        {
            var result = new ContextActionRemoveBuff();
            result.m_Buff = buff.ToRef();
            result.ToCaster = toCaster;
            return result;
        }

        public static ContextConditionHasBuff CreateContextConditionHasBuff(this BlueprintBuff buff)
        {
            var hasBuff = new ContextConditionHasBuff();
            hasBuff.m_Buff = buff.ToRef();
            return hasBuff;
        }

        public static ActionList CreateActionList(params GameAction[] actions)
        {
            if (actions == null || actions.Length == 1 && actions[0] == null) actions = Array.Empty<GameAction>();
            return new ActionList() { Actions = actions };
        }

        public static Conditional CreateConditional(Condition condition, GameAction ifTrue = null, GameAction ifFalse = null, bool OperationAnd = true)
        {
            var c = new Conditional();
            c.ConditionsChecker = new ConditionsChecker() { Conditions = condition.ObjToArray(), Operation = OperationAnd ? Operation.And : Operation.Or };
            c.IfTrue = CreateActionList(ifTrue);
            c.IfFalse = CreateActionList(ifFalse);
            return c;
        }

        public static Conditional CreateConditional(Condition[] condition, GameAction[] ifTrue = null, GameAction[] ifFalse = null, bool OperationAnd = true)
        {
            var c = new Conditional();
            c.ConditionsChecker = new ConditionsChecker() { Conditions = condition, Operation = OperationAnd ? Operation.And : Operation.Or };
            c.IfTrue = CreateActionList(ifTrue);
            c.IfFalse = CreateActionList(ifFalse);
            return c;
        }

        public static ContextActionConditionalSaved CreateContextActionConditionalSaved(GameAction succeed = null, GameAction failed = null)
        {
            var result = new ContextActionConditionalSaved();
            result.Succeed = CreateActionList(succeed);
            result.Failed = CreateActionList(failed);
            return result;
        }

        public static AbilityRequirementHasBuffs CreateAbilityRequirementHasBuffs(bool Not, params BlueprintBuff[] Buffs)
        {
            var result = new AbilityRequirementHasBuffs();
            result.Not = Not;
            result.Buffs = Buffs;
            return result;
        }

        public static AbilityRequirementHasBuffTimed CreateAbilityRequirementHasBuffTimed(CompareType Compare, TimeSpan TimeLeft, params BlueprintBuff[] Buffs)
        {
            var result = new AbilityRequirementHasBuffTimed();
            result.Compare = Compare;
            result.Buffs = Buffs;
            result.TimeLeft = TimeLeft;
            return result;
        }

        public static ContextActionApplyBuff CreateContextActionApplyBuff(this BlueprintBuff buff, int duration = 0, DurationRate rate = DurationRate.Rounds, bool dispellable = false, bool toCaster = false, bool asChild = false, bool permanent = false)
        {
            return CreateContextActionApplyBuff(buff, CreateContextDurationValue(bonus: duration, rate: rate), fromSpell: false, toCaster: toCaster, asChild: asChild, dispellable: dispellable, permanent: permanent);
        }

        public static ContextActionApplyBuff CreateContextActionApplyBuff(this BlueprintBuff buff, ContextDurationValue duration, bool fromSpell = false, bool dispellable = true, bool toCaster = false, bool asChild = false, bool permanent = false)
        {
            var result = new ContextActionApplyBuff();
            result.m_Buff = buff.ToRef();
            result.DurationValue = duration;
            result.IsFromSpell = fromSpell;
            result.IsNotDispelable = !dispellable;
            result.ToCaster = toCaster;
            result.AsChild = asChild;
            result.Permanent = permanent;
            return result;
        }

        public static LevelEntry CreateLevelEntry(int level, params BlueprintFeatureBase[] features)
        {
            var result = new LevelEntry();
            result.Level = level;
            result.m_Features = features.ToRef().ToList();
            return result;
        }

        public static ClassLevelsForPrerequisites CreateClassLevelsForPrerequisites(BlueprintCharacterClassReference target_class, int bonus = 0, BlueprintCharacterClassReference source_class = null, double multiplier = 0d)
        {
            var result = new ClassLevelsForPrerequisites();
            result.m_FakeClass = target_class;
            result.m_ActualClass = source_class ?? target_class;
            result.Summand = bonus;
            result.Modifier = multiplier;
            return result;
        }

        public static PrerequisiteFeaturesFromList CreatePrerequisiteFeaturesFromList(bool any = false, params BlueprintFeatureReference[] features)
        {
            var result = new PrerequisiteFeaturesFromList();
            result.m_Features = features;
            result.Group = any ? Prerequisite.GroupType.Any : Prerequisite.GroupType.All;
            result.Amount = 1;
            return result;
        }

        public static PrerequisiteFeature CreatePrerequisiteFeature(this BlueprintFeatureReference feat, bool any = false)
        {
            var result = new PrerequisiteFeature();
            result.m_Feature = feat;
            result.Group = any ? Prerequisite.GroupType.Any : Prerequisite.GroupType.All;
            return result;
        }

        public static PrerequisiteClassLevel CreatePrerequisiteClassLevel(BlueprintCharacterClassReference @class, int level, bool any = false)
        {
            var result = new PrerequisiteClassLevel();
            result.m_CharacterClass = @class;
            result.Level = level;
            result.Group = any ? Prerequisite.GroupType.Any : Prerequisite.GroupType.All;
            return result;
        }

        public static PrerequisiteArchetypeLevel CreatePrerequisiteArchetypeLevel(BlueprintArchetypeReference m_Archetype, int level = 1, bool any = false, BlueprintCharacterClassReference m_CharacterClass = null)
        {
            var result = new PrerequisiteArchetypeLevel();
            result.m_Archetype = m_Archetype;
            result.m_CharacterClass = m_CharacterClass ?? m_Archetype.Get().GetParentClass().ToRef();
            result.Level = level;
            result.Group = any ? Prerequisite.GroupType.Any : Prerequisite.GroupType.All;
            return result;
        }

        public static PrerequisiteNoArchetype CreatePrerequisiteNoArchetype(BlueprintArchetypeReference m_Archetype, BlueprintCharacterClassReference m_CharacterClass = null)
        {
            var result = new PrerequisiteNoArchetype();
            result.m_Archetype = m_Archetype;
            result.m_CharacterClass = m_CharacterClass ?? m_Archetype.Get().GetParentClass().ToRef();
            return result;
        }

        public static AddFacts CreateAddFacts(params BlueprintUnitFactReference[] facts)
        {
            var result = new AddFacts();
            result.m_Facts = facts;
            return result;
        }

        public static BlueprintBuff CreateBlueprintBuff(string name, string displayName, string description, string guid = null, Sprite icon = null, PrefabLink fxOnStart = null)
        {
            if (guid == null)
                guid = GuidManager.i.Get(name);

            var result = new BlueprintBuff();
            result.name = name;
            result.m_DisplayName = displayName.CreateString();
            result.m_Description = description.CreateString();
            result.m_Icon = icon;
            result.FxOnStart = fxOnStart ?? new PrefabLink();
            result.FxOnRemove = new PrefabLink();
            result.IsClassFeature = true;

            AddAsset(result, guid);
            return result;
        }

        public static BlueprintFeature CreateBlueprintFeature(string name, string displayName, string description, string guid = null, Sprite icon = null, FeatureGroup group = 0)
        {
            if (guid == null)
                guid = GuidManager.i.Get(name);

            var result = new BlueprintFeature();
            result.IsClassFeature = true;
            result.name = name;
            result.m_DisplayName = displayName.CreateString();
            result.m_Description = description.CreateString();
            result.m_Icon = icon;
            result.Groups = group == 0 ? Array.Empty<FeatureGroup>() : ToArray(group);

            AddAsset(result, guid);
            return result;
        }

        public static BlueprintAbility CreateBlueprintAbility(string name, string displayName, string description, string guid, Sprite icon, AbilityType type, CommandType actionType, AbilityRange range, LocalizedString duration = null, LocalizedString savingThrow = null)
        {
            if (guid == null)
                guid = GuidManager.i.Get(name);

            var result = new BlueprintAbility();
            result.name = name;
            result.m_DisplayName = displayName.CreateString();
            result.m_Description = description.CreateString();
            result.m_Icon = icon;
            result.ResourceAssetIds = Array.Empty<string>();
            result.Type = type;
            result.ActionType = actionType;
            result.Range = range;
            result.LocalizedDuration = duration ?? Resource.Strings.Empty;
            result.LocalizedSavingThrow = savingThrow ?? Resource.Strings.Empty;

            AddAsset(result, guid);
            return result;
        }

        public static BlueprintFeatureSelection CreateBlueprintFeatureSelection(string name, string displayName, string description, string guid = null, Sprite icon = null, FeatureGroup group = 0)
        {
            if (guid == null)
                guid = GuidManager.i.Get(name);

            var result = new BlueprintFeatureSelection();
            result.IsClassFeature = true;
            result.name = name;
            result.m_DisplayName = displayName.CreateString();
            result.m_Description = description.CreateString();
            result.Groups = group == 0 ? Array.Empty<FeatureGroup>() : ToArray(group);
            result.m_Icon = icon;

            AddAsset(result, guid);
            return result;
        }

        public static BlueprintParametrizedFeature CreateBlueprintParametrizedFeature(string name, string displayName, string description, string guid = null, Sprite icon = null, FeatureGroup group = 0, FeatureParameterType parameterType = FeatureParameterType.Custom, AnyBlueprintReference[] blueprints = null)
        {
            if (guid == null)
                guid = GuidManager.i.Get(name);

            var result = new BlueprintParametrizedFeature();
            result.IsClassFeature = true;
            result.name = name;
            result.m_DisplayName = displayName.CreateString();
            result.m_Description = description.CreateString();
            result.m_Icon = icon;
            result.Groups = group == 0 ? Array.Empty<FeatureGroup>() : ToArray(group);
            result.ParameterType = parameterType; //FeatureParameterType.FeatureSelection
            result.BlueprintParameterVariants = blueprints;

            AddAsset(result, guid);
            return result;
        }

        public static BlueprintActivatableAbility CreateBlueprintActivatableAbility(string name, string displayName, string description, out BlueprintBuff buff, string guid = null, Sprite icon = null, CommandType commandType = CommandType.Free, AbilityActivationType activationType = AbilityActivationType.Immediately, ActivatableAbilityGroup group = ActivatableAbilityGroup.None, bool deactivateImmediately = true, bool onByDefault = false, bool onlyInCombat = false, bool deactivateEndOfCombat = false, bool deactivateAfterRound = false, bool deactivateWhenStunned = false, bool deactivateWhenDead = false, bool deactivateOnRest = false, bool useWithSpell = false, int groupWeight = 1)
        {
            if (guid == null)
                guid = GuidManager.i.Get(name);

            var result = new BlueprintActivatableAbility();
            result.name = name;
            result.m_DisplayName = displayName.CreateString();
            result.m_Description = description.CreateString();
            result.m_Icon = icon;
            result.ResourceAssetIds = Array.Empty<string>();

            //result.m_Buff = buff;
            result.m_ActivateWithUnitCommand = commandType;
            result.ActivationType = activationType;
            result.DeactivateImmediately = deactivateImmediately;
            result.IsOnByDefault = onByDefault;
            result.OnlyInCombat = onlyInCombat;

            result.DeactivateIfCombatEnded = deactivateEndOfCombat;
            result.DeactivateAfterFirstRound = deactivateAfterRound;
            result.DeactivateIfOwnerDisabled = deactivateWhenStunned;
            result.DeactivateIfOwnerUnconscious = deactivateWhenDead;
            result.DoNotTurnOffOnRest = deactivateOnRest;

            result.Group = group;
            result.WeightInGroup = groupWeight; // how many resources one activation costs
            result.m_ActivateOnUnitAction = useWithSpell ? AbilityActivateOnUnitActionType.CastSpell : 0; // when spell casts costs resources

            //result.IsTargeted = ;
            //result.m_SelectTargetAbility = ;

            AddAsset(result, guid);

            // make activatable buff
            buff = new BlueprintBuff();
            buff.name = name + "_Buff";
            buff.m_DisplayName = result.m_DisplayName;
            buff.m_Description = result.m_Description;
            buff.AssetGuid = BlueprintGuid.Parse(GuidManager.i.Get(buff.name));
            buff.m_Icon = icon;
            buff.FxOnStart = new PrefabLink();
            buff.FxOnRemove = new PrefabLink();
            buff.IsClassFeature = true;
            AddAsset(buff, buff.AssetGuid);

            result.m_Buff = buff.ToRef();
            return result;
        }

        public static BlueprintAbilityAreaEffect CreateBlueprintAbilityAreaEffect(string name, string guid = null, bool applyEnemy = false, bool applyAlly = false, AreaEffectShape shape = AreaEffectShape.Cylinder, Feet size = default(Feet), PrefabLink sfx = null, BlueprintBuffReference buffWhileInside = null, ActionList unitEnter = null, ActionList unitExit = null, ActionList unitMove = null, ActionList unitRound = null)
        {
            if (!applyAlly && !applyEnemy)
                throw new ArgumentException("area must effect either allies or enemies");

            if (guid == null)
                guid = GuidManager.i.Get(name);

            var result = new BlueprintAbilityAreaEffect();
            result.name = name;
            result.Shape = shape;
            result.Size = size;
            result.Fx = sfx ?? new PrefabLink();

            if (applyEnemy && applyAlly)
                result.m_TargetType = BlueprintAbilityAreaEffect.TargetType.Any;
            else if (applyEnemy)
                result.m_TargetType = BlueprintAbilityAreaEffect.TargetType.Enemy;
            else if (applyAlly)
                result.m_TargetType = BlueprintAbilityAreaEffect.TargetType.Ally;

            if (buffWhileInside != null)
            {
                AbilityAreaEffectBuff areabuff = new AbilityAreaEffectBuff(); // applies buff while inside
                areabuff.Condition = new ConditionsChecker();
                areabuff.CheckConditionEveryRound = false;
                areabuff.m_Buff = buffWhileInside;

                result.SetComponents(areabuff);
            }

            if (unitEnter != null || unitExit != null || unitMove != null || unitRound != null)
            {
                AbilityAreaEffectRunAction runaction = new AbilityAreaEffectRunAction(); // runs actions that persist even when leaving
                runaction.UnitEnter = unitEnter ?? new ActionList();
                runaction.UnitExit = unitExit ?? new ActionList();
                runaction.UnitMove = unitMove ?? new ActionList();
                runaction.Round = unitRound ?? new ActionList();

                result.AddComponents(runaction);
            }

            AddAsset(result, guid);
            return result;
        }

        public static BlueprintWeaponEnchantment CreateBlueprintWeaponEnchantment(string name, string enchantName = null, string description = null, string prefix = null, string suffix = null, string guid = null, int enchantValue = 0)
        {
            if (guid == null)
                guid = GuidManager.i.Get(name);

            var result = new BlueprintWeaponEnchantment();
            result.name = name;
            result.m_EnchantName = enchantName.CreateString();
            result.m_Description = description.CreateString();
            result.m_Prefix = prefix.CreateString();
            result.m_Suffix = suffix.CreateString();
            result.m_EnchantmentCost = enchantValue;

            AddAsset(result, guid);
            return result;
        }

        public static BlueprintUnitProperty CreateBlueprintUnitProperty(string name)
        {
            var guid = GuidManager.i.Get(name);
            var result = new BlueprintUnitProperty();
            result.name = name;

            AddAsset(result, guid);
            return result;
        }

        #endregion

        #region ToReference

        public static void SetReference(this BlueprintReferenceBase reference, SimpleBlueprint bp)
        {
            reference.Cached = bp;
            reference.deserializedGuid = bp.AssetGuid;
            reference.guid = bp.AssetGuid.ToString();
        }

        public static T ToRef<T>(this string guid) where T : BlueprintReferenceBase, new()
        {
            T tref = Activator.CreateInstance<T>();
            tref.ReadGuidFromJson(guid);
            return tref;
        }


        public static BlueprintUnitFactReference ToRef2(this BlueprintAbility feature)
        {
            if (feature == null) return null;
            var result = new BlueprintUnitFactReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }
        public static BlueprintUnitFactReference ToRef2(this BlueprintFeature feature)
        {
            if (feature == null) return null;
            var result = new BlueprintUnitFactReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }
        public static BlueprintUnitFactReference ToRef2(this BlueprintBuff feature)
        {
            if (feature == null) return null;
            var result = new BlueprintUnitFactReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }


        public static AnyBlueprintReference ToRef3(this BlueprintAbility feature)
        {
            if (feature == null) return null;
            var result = new AnyBlueprintReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }
        public static AnyBlueprintReference[] ToRef3(this BlueprintAbility[] feature)
        {
            if (feature == null) return null;
            var result = new AnyBlueprintReference[feature.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new AnyBlueprintReference();
                result[i].deserializedGuid = feature[i].AssetGuid;
            }
            return result;
        }


        public static AnyBlueprintReference ToRef(this BlueprintScriptableObject feature)
        {
            if (feature == null) return null;
            var result = new AnyBlueprintReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }
        public static AnyBlueprintReference[] ToRef(this BlueprintScriptableObject[] feature)
        {
            if (feature == null) return null;
            var result = new AnyBlueprintReference[feature.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new AnyBlueprintReference();
                result[i].deserializedGuid = feature[i].AssetGuid;
            }
            return result;
        }

        public static BlueprintFeatureReference ToRef(this BlueprintFeature feature)
        {
            if (feature == null) return null;
            //feature.ToReference<BlueprintFeatureReference>();
            var result = new BlueprintFeatureReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }
        public static BlueprintFeatureReference[] ToRef(this BlueprintFeature[] feature)
        {
            if (feature == null) return null;
            var result = new BlueprintFeatureReference[feature.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new BlueprintFeatureReference();
                result[i].deserializedGuid = feature[i].AssetGuid;
            }
            return result;
        }

        public static BlueprintAbilityReference ToRef(this BlueprintAbility feature)
        {
            if (feature == null) return null;
            var result = new BlueprintAbilityReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }
        public static BlueprintAbilityReference[] ToRef(this BlueprintAbility[] feature)
        {
            if (feature == null) return null;
            var result = new BlueprintAbilityReference[feature.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new BlueprintAbilityReference();
                result[i].deserializedGuid = feature[i].AssetGuid;
            }
            return result;
        }

        public static BlueprintCharacterClassReference ToRef(this BlueprintCharacterClass feature)
        {
            if (feature == null) return null;
            var result = new BlueprintCharacterClassReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }
        public static BlueprintCharacterClassReference[] ToRef(this BlueprintCharacterClass[] feature)
        {
            if (feature == null) return null;
            var result = new BlueprintCharacterClassReference[feature.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new BlueprintCharacterClassReference();
                result[i].deserializedGuid = feature[i].AssetGuid;
            }
            return result;
        }

        public static BlueprintFeatureBaseReference ToRef(this BlueprintFeatureBase feature)
        {
            if (feature == null) return null;
            var result = new BlueprintFeatureBaseReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }
        public static BlueprintFeatureBaseReference[] ToRef(this BlueprintFeatureBase[] feature)
        {
            if (feature == null) return null;
            var result = new BlueprintFeatureBaseReference[feature.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new BlueprintFeatureBaseReference();
                result[i].deserializedGuid = feature[i].AssetGuid;
            }
            return result;
        }

        public static BlueprintBuffReference ToRef(this BlueprintBuff feature)
        {
            if (feature == null) return null;
            var result = new BlueprintBuffReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }
        public static BlueprintBuffReference[] ToRef(this BlueprintBuff[] feature)
        {
            if (feature == null) return null;
            var result = new BlueprintBuffReference[feature.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new BlueprintBuffReference();
                result[i].deserializedGuid = feature[i].AssetGuid;
            }
            return result;
        }

        public static BlueprintUnitFactReference ToRef(this BlueprintUnitFact feature)
        {
            if (feature == null) return null;
            var result = new BlueprintUnitFactReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }
        public static BlueprintUnitFactReference[] ToRef(this BlueprintUnitFact[] feature)
        {
            if (feature == null) return null;
            var result = new BlueprintUnitFactReference[feature.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new BlueprintUnitFactReference();
                result[i].deserializedGuid = feature[i].AssetGuid;
            }
            return result;
        }

        public static BlueprintUnitPropertyReference ToRef(this BlueprintUnitProperty feature)
        {
            if (feature == null) return null;
            var result = new BlueprintUnitPropertyReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }
        public static BlueprintUnitPropertyReference[] ToRef(this BlueprintUnitProperty[] feature)
        {
            if (feature == null) return null;
            var result = new BlueprintUnitPropertyReference[feature.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new BlueprintUnitPropertyReference();
                result[i].deserializedGuid = feature[i].AssetGuid;
            }
            return result;
        }

        public static BlueprintArchetypeReference ToRef(this BlueprintArchetype feature)
        {
            if (feature == null) return null;
            var result = new BlueprintArchetypeReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }
        public static BlueprintArchetypeReference[] ToRef(this BlueprintArchetype[] feature)
        {
            if (feature == null) return null;
            var result = new BlueprintArchetypeReference[feature.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new BlueprintArchetypeReference();
                result[i].deserializedGuid = feature[i].AssetGuid;
            }
            return result;
        }

        public static BlueprintProjectileReference ToRef(this BlueprintProjectile feature)
        {
            if (feature == null) return null;
            var result = new BlueprintProjectileReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }
        public static BlueprintProjectileReference[] ToRef(this BlueprintProjectile[] feature)
        {
            if (feature == null) return null;
            var result = new BlueprintProjectileReference[feature.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new BlueprintProjectileReference();
                result[i].deserializedGuid = feature[i].AssetGuid;
            }
            return result;
        }

        public static BlueprintItemWeaponReference ToRef(this BlueprintItemWeapon feature)
        {
            if (feature == null) return null;
            var result = new BlueprintItemWeaponReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }
        public static BlueprintItemWeaponReference[] ToRef(this BlueprintItemWeapon[] feature)
        {
            if (feature == null) return null;
            var result = new BlueprintItemWeaponReference[feature.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new BlueprintItemWeaponReference();
                result[i].deserializedGuid = feature[i].AssetGuid;
            }
            return result;
        }

        public static BlueprintWeaponTypeReference ToRef(this BlueprintWeaponType feature)
        {
            if (feature == null) return null;
            var result = new BlueprintWeaponTypeReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }
        public static BlueprintWeaponTypeReference[] ToRef(this BlueprintWeaponType[] feature)
        {
            if (feature == null) return null;
            var result = new BlueprintWeaponTypeReference[feature.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new BlueprintWeaponTypeReference();
                result[i].deserializedGuid = feature[i].AssetGuid;
            }
            return result;
        }

        public static BlueprintWeaponEnchantmentReference ToRef(this BlueprintWeaponEnchantment feature)
        {
            if (feature == null) return null;
            var result = new BlueprintWeaponEnchantmentReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }
        public static BlueprintWeaponEnchantmentReference[] ToRef(this BlueprintWeaponEnchantment[] feature)
        {
            if (feature == null) return null;
            var result = new BlueprintWeaponEnchantmentReference[feature.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new BlueprintWeaponEnchantmentReference();
                result[i].deserializedGuid = feature[i].AssetGuid;
            }
            return result;
        }

        public static BlueprintAbilityAreaEffectReference ToRef(this BlueprintAbilityAreaEffect feature)
        {
            if (feature == null) return null;
            var result = new BlueprintAbilityAreaEffectReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }
        public static BlueprintAbilityAreaEffectReference[] ToRef(this BlueprintAbilityAreaEffect[] feature)
        {
            if (feature == null) return null;
            var result = new BlueprintAbilityAreaEffectReference[feature.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new BlueprintAbilityAreaEffectReference();
                result[i].deserializedGuid = feature[i].AssetGuid;
            }
            return result;
        }

        #endregion

        #region Image

        public static Sprite StealIcon(string guid)
        {
            try
            {
                return ResourcesLibrary.TryGetBlueprint<BlueprintUnitFact>(guid)?.Icon;
            }
            catch (Exception)
            {
                Helper.Print("Could not import icon from " + guid);
                return null;
            }
        }

        public static Sprite CreateSprite(string filename, int width = 64, int height = 64)
        {
            try
            {
                var bytes = File.ReadAllBytes(Path.Combine(Main.ModPath, "Icons", filename));
                var texture = new Texture2D(width, height);
                texture.LoadImage(bytes);
                return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0, 0));
            }
            catch (Exception e)
            {
                PrintException(e);
                return null;
            }
        }

        public static void SaveSprite(Sprite icon) // not working
        {
            Texture.allowThreadedTextureCreation = true;
            File.WriteAllBytes(Path.Combine(Main.ModPath, "IconsExport", icon.name), ImageConversion.EncodeToPNG(icon.texture));
        }

        #endregion
    }
}
