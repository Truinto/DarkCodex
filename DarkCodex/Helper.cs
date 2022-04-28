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
using Kingmaker.PubSubSystem;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI;
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
using Kingmaker.View.Equipment;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using static Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell;

namespace DarkCodex
{
    /* 
     * Notes:
     * BlueprintComponent has a field OwnerBlueprint. When components are shared between blueprints, these may cause weird bugs.
     * 
     */

    public static class Helper
    {
        #region Other

        public static int MinMax(this int number, int min, int max)
        {
            return Math.Max(min, Math.Min(number, max));
        }

        public static void AttackRollPrint(this RuleAttackRoll attackRoll)
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

        #region Patching/Harmony

        /// <summary>Needs ManualPatch attribute.</summary>
        public static void Patch(Type patch, bool _)
        {
            Print("ManualPatch " + patch.Name);
            if (patch.GetCustomAttributes(false).FirstOrDefault(f => f is ManualPatchAttribute) is not ManualPatchAttribute manual)
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
            if (patch.GetCustomAttributes(false).FirstOrDefault(f => f is HarmonyPatch) is not HarmonyPatch attr)
                return;

            MethodBase orignal = attr.info.GetOriginalMethod();
            Main.harmony.Unpatch(orignal, patchType, Main.harmony.Id);
        }

        /// <summary>Only works with HarmonyPatch.</summary>
        public static bool IsPatched(Type patch)
        {
            if (patch.GetCustomAttributes(false).FirstOrDefault(f => f is HarmonyPatch) is not HarmonyPatch attr)
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

        public static void ReplaceCall(this CodeInstruction code, Type type, string name, Type[] parameters = null, Type[] generics = null)
        {
            var repl = CodeInstruction.Call(type, name, parameters, generics);
            code.opcode = repl.opcode;
            code.operand = repl.operand;
        }

        #endregion

        #region Arrays

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
        public static T[] Append<T>(this T[] orig, params T[] objs)
        {
            if (orig == null) orig = new T[0];
            if (objs == null) objs = new T[0];

            int i, j;
            T[] result = new T[orig.Length + objs.Length];
            for (i = 0; i < orig.Length; i++)
                result[i] = orig[i];
            for (j = 0; i < result.Length; i++)
                result[i] = objs[j++];
            return result;
        }

        public static List<T> Append<T>(this List<T> orig, List<T> objs)
        {
            var result = new List<T>(orig);
            result.AddRange(objs);
            return result;
        }

        /// <summary>Appends objects on array and overwrites the original.</summary>
        public static void AppendAndReplace<T>(ref T[] orig, params T[] objs)
        {
            if (orig == null) orig = new T[0];
            if (objs == null) objs = new T[0];

            int i, j;
            T[] result = new T[orig.Length + objs.Length];
            for (i = 0; i < orig.Length; i++)
                result[i] = orig[i];
            for (j = 0; i < result.Length; i++)
                result[i] = objs[j++];
            orig = result;
        }

        public static void AppendAndReplace<T>(ref T[] orig, List<T> objs)
        {
            if (orig == null) orig = new T[0];

            T[] result = new T[orig.Length + objs.Count];
            int i;
            for (i = 0; i < orig.Length; i++)
                result[i] = orig[i];
            foreach (var obj in objs)
                result[i++] = obj;
            orig = result;
        }

        public static void InsertAt<T>(ref T[] orig, T obj, int index = -1)
        {
            if (orig == null) orig = new T[0];
            if (index < 0 || index > orig.Length) index = orig.Length;

            T[] result = new T[orig.Length + 1];
            for (int i = 0, j = 0; i < result.Length; i++)
            {
                if (i == index)
                    result[i] = obj;
                else
                    result[i] = orig[j++];
            }
            orig = result;
        }

        public static void RemoveAt<T>(ref T[] orig, int index)
        {
            if (orig == null) orig = new T[0];
            if (index < 0 || index >= orig.Length) return;

            T[] result = new T[orig.Length - 1];
            for (int i = 0, j = 0; i < result.Length; i++)
            {
                if (i != index)
                    result[i] = orig[j++];
            }
            orig = result;
        }

        public static void RemoveGet<T>(this List<T> list, List<T> result, Func<T, bool> predicate)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (predicate(list[i]))
                {
                    result.Add(list[i]);
                    list.RemoveAt(i);
                    return;
                }
            }
        }

        public static void RemoveGet<T1, T2>(this List<T1> list, List<T2> result, Func<T1, bool> predicate, Func<T1, T2> select)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (predicate(list[i]))
                {
                    result.Add(select(list[i]));
                    list.RemoveAt(i);
                    return;
                }
            }
        }

        public static void Deconstruct<K, V>(this KeyValuePair<K, V> pair, out K key, out V val)
        {
            key = pair.Key;
            val = pair.Value;
        }

        #endregion

        #region Log

        private static bool _sb2_flag;
        private static StringBuilder _sb2 = new();

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

        internal static void PrintError(string msg)
        {
            Main.logger?.Log("[Exception/Error] " + msg);
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

        public static void ShowMessageBox(string messageText, Action onYes = null, int waitTime = 0, string yesLabel = null, string noLabel = null)
        {
            EventBus.RaiseEvent<IMessageModalUIHandler>(w =>
            {
                w.HandleOpen(messageText: messageText,
                    modalType: onYes == null ? MessageModalBase.ModalType.Message : MessageModalBase.ModalType.Dialog,
                    onClose: onYes == null ? null : a => { if (a == MessageModalBase.ButtonType.Yes) try { onYes(); } catch { } },
                    onLinkInvoke: null,
                    yesLabel: yesLabel,
                    noLabel: noLabel,
                    onTextResult: null,
                    inputText: null,
                    inputPlaceholder: null,
                    waitTime: waitTime,
                    maxInputTextLength: uint.MaxValue,
                    items: null);
            }, true);
        }

        public static void ShowInputBox(string messageText, Action<string> onOK = null, Action onCancel = null, int waitTime = 0, string yesLabel = null, string noLabel = null)
        {
            EventBus.RaiseEvent<IMessageModalUIHandler>(w =>
            {
                w.HandleOpen(messageText: messageText,
                    modalType: MessageModalBase.ModalType.TextField,
                    onClose: onCancel == null ? null : a => { if (a == MessageModalBase.ButtonType.No) try { onCancel(); } catch { } },
                    onLinkInvoke: null,
                    yesLabel: yesLabel,
                    noLabel: noLabel,
                    onTextResult: onOK,
                    inputText: null,
                    inputPlaceholder: null,
                    waitTime: waitTime,
                    maxInputTextLength: uint.MaxValue,
                    items: null);
            }, true);
        }

        #endregion

        #region JsonSerializer

        private static JsonSerializerSettings _jsetting = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            DefaultValueHandling = DefaultValueHandling.Include,
            TypeNameHandling = TypeNameHandling.Auto,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.None
        };

        public static string Serialize(object value, bool indent = true, string path = null, bool append = false)
        {
            _jsetting.Formatting = indent ? Formatting.Indented : Formatting.None;
            string result = JsonConvert.SerializeObject(value, _jsetting);

            if (path != null)
            {
                path = Path.Combine(Main.ModPath, path);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using var sw = new StreamWriter(path, append);
                sw.WriteLine(result);
                sw.Close();
            }

            return result;
        }

        public static string Serialize<T>(T value, bool indent = true, string path = null, bool append = false)
        {
            _jsetting.Formatting = indent ? Formatting.Indented : Formatting.None;
            string result = JsonConvert.SerializeObject(value, typeof(T), _jsetting);

            if (path != null)
            {
                path = Path.Combine(Main.ModPath, path);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using var sw = new StreamWriter(path, append);
                sw.WriteLine(result);
                sw.Close();
            }

            return result;
        }

        public static object Deserialize(string path = null, string value = null)
        {
            if (path != null)
            {
                using var sr = new StreamReader(Path.Combine(Main.ModPath, path));
                value = sr.ReadToEnd();
                sr.Close();
            }

            if (value != null)
                return JsonConvert.DeserializeObject(value);
            return null;
        }

        public static T Deserialize<T>(string path = null, string value = null)
        {
            if (path != null)
            {
                using var sr = new StreamReader(Path.Combine(Main.ModPath, path));
                value = sr.ReadToEnd();
                sr.Close();
            }

            if (value != null)
                return JsonConvert.DeserializeObject<T>(value);
            return default;
        }

        public static T TryDeserialize<T>(string path = null, string value = null)
        {
            try
            {
                return Deserialize<T>(path, value);
            }
            catch (Exception e)
            {
                Helper.PrintException(e);
                return default;
            }
        }

        public static void TryPrintFile(string path, string content, bool append = true)
        {
            try
            {
                path = Path.Combine(Main.ModPath, path);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using var sw = new StreamWriter(path, append);
                sw.WriteLine(content);
            }
            catch (Exception e)
            {
                Helper.PrintException(e);
            }
        }

        public static void TryPrintBytes(string path, byte[] data)
        {
            try
            {
                path = Path.Combine(Main.ModPath, path);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, data);
            }
            catch (Exception e)
            {
                Helper.PrintException(e);
            }
        }

        public static byte[] TryReadBytes(string path)
        {
            try
            {
                return File.ReadAllBytes(Path.Combine(Main.ModPath, path));
            }
            catch (Exception e)
            {
                Helper.PrintException(e);
                return new byte[0];
            }
        }

        public static void TryDelete(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception e)
            {
                Helper.PrintException(e);
            }
        }

        #endregion

        #region Strings

        public static LocalizedString GetString(string key)
        {
            return new LocalizedString { Key = key };
        }

        public static bool IsEmptyKey(this LocalizedString key)
        {
            if (key == null)
                return true;
            if (key == "")
                return true;

            if (LocalizationManager.CurrentPack.GetText(key.Key, false) == "")
                return true;

            return false;
        }

        private static SHA1 _SHA = SHA1Managed.Create();
        private static StringBuilder _sb1 = new();
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

            var pack = LocalizationManager.CurrentPack;
            if (LocalizationManager.CurrentPack.Locale != _lastLocale)
            {
                _lastLocale = LocalizationManager.CurrentPack.Locale;
                try
                {
                    _mappedStrings = new JsonManager().Deserialize<Dictionary<string, string>>(Path.Combine(Main.ModPath, LocalizationManager.CurrentPack.Locale.ToString() + ".json"));
                    foreach (var entry in _mappedStrings)
                        pack.PutString(entry.Key, entry.Value);
                    _mappedStrings = null;
                }
                catch (Exception e)
                {
                    Print($"Could not read lanaguage file for {LocalizationManager.CurrentPack.Locale}: {e.Message}");
                }
            }

            if (!pack.m_Strings.ContainsKey(key))
            {
                pack.PutString(key, value);
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
                oldmap = Helper.Deserialize<Dictionary<string, string>>(path: Path.Combine(Main.ModPath, "enGB.json"));

                foreach (var entry in _mappedStrings)
                    if (!oldmap.ContainsKey(entry.Key))
                        oldmap.Add(entry.Key, entry.Value);
            }
            catch (Exception) { }

            try
            {
                Helper.Serialize(oldmap ?? _mappedStrings, path: Path.Combine(Main.ModPath, "enGB.json"));
                _mappedStrings = null;
            }
            catch (Exception e)
            {
                Print($"Failed export lanaguage file: {e.Message}");
            }
        }

        /// <summary>Returns substring. Always excludes char 'c'. Returns null, if index is out of range or char not found.</summary>
        /// <param name="str">source string</param>
        /// <param name="c">char to search for</param>
        /// <param name="start">start index; negative number search last index instead</param>
        /// <param name="tail">get tail instead of head</param>
        public static string TrySubstring(this string str, char c, int start = 0, bool tail = false)
        {
            try
            {
                if (tail)
                {
                    if (start < 0)
                        return str.Substring(str.LastIndexOf(c) + 1);
                    return str.Substring(str.IndexOf(c, start) + 1);
                }

                if (start < 0)
                    return str.Substring(0, str.LastIndexOf(c));
                return str.Substring(start, str.IndexOf(c, start));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool IsNumber(this char c)
        {
            return c >= 48 && c <= 57;
        }

        public static bool IsUppercase(this char c)
        {
            return c >= 65 && c <= 90;
        }

        public static bool IsLowercase(this char c)
        {
            return c >= 97 && c <= 122;
        }

        public static bool IsNotSpaced(this StringBuilder sb)
        {
            if (sb.Length == 0)
                return false;

            return sb[sb.Length - 1] != ' ';
        }

        public static string Red(this string text, bool condition = true)
        {
            if (condition)
                return $"<color=red>{text}</color>";
            return text;
        }

        public static string Grey(this string text, bool condition = true)
        {
            if (condition)
                return $"<color=grey>{text}</color>";
            return text;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWithO(this string source, string value) => source.StartsWith(value, StringComparison.Ordinal);

        #endregion

        #region Components

        public static T AddComponents<T>(this T obj, params BlueprintComponent[] components) where T : BlueprintScriptableObject
        {
            obj.Components = Append(obj.Components, components);

            for (int i = 0; i < obj.Components.Length; i++)
                if (obj.Components[i].name == null)
                    obj.Components[i].name = $"${obj.Components[i].GetType().Name}${obj.AssetGuid}${i}";

            return obj;
        }

        public static T SetComponents<T>(this T obj, params BlueprintComponent[] components) where T : BlueprintScriptableObject
        {
            for (int i = 0; i < components.Length; i++)
                components[i].name = $"${components[i].GetType().Name}${obj.AssetGuid}${i}";

            obj.Components = components;
            return obj;
        }

        public static T ReplaceComponent<T, TOrig, TRep>(this T obj, TOrig original, TRep replacement) where T : BlueprintScriptableObject where TOrig : BlueprintComponent where TRep : BlueprintComponent
        {
            for (int i = 0; i < obj.ComponentsArray.Length; i++)
            {
                if (obj.Components[i] is TOrig)
                {
                    obj.Components[i] = replacement;
                    replacement.name = $"${replacement.GetType().Name}${obj.AssetGuid}${i}";
                    break;
                }
            }

            return obj;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T RemoveComponents<T, TRemove>(T obj) where T : BlueprintScriptableObject where TRemove : BlueprintComponent
        {
            var list = obj.ComponentsArray.ToList();
            list.RemoveAll(r => r is TRemove);
            obj.Components = list.ToArray();
            return obj;
        }
        public static BlueprintAbility RemoveComponents<TRemove>(this BlueprintAbility obj) where TRemove : BlueprintComponent
            => RemoveComponents<BlueprintAbility, TRemove>(obj);
        public static BlueprintFeature RemoveComponents<TRemove>(this BlueprintFeature obj) where TRemove : BlueprintComponent
            => RemoveComponents<BlueprintFeature, TRemove>(obj);
        public static BlueprintBuff RemoveComponents<TRemove>(this BlueprintBuff obj) where TRemove : BlueprintComponent
            => RemoveComponents<BlueprintBuff, TRemove>(obj);

        public static T RemoveComponents<T>(this T obj, Predicate<BlueprintComponent> match) where T : BlueprintScriptableObject
        {
            var list = obj.ComponentsArray.ToList();
            list.RemoveAll(match);
            obj.Components = list.ToArray();
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

        public static bool IsRank(this AbilityData ability, int min = -1, int max = int.MaxValue, bool ifEmpty = true)
        {
            var rank = ability.Blueprint.GetComponent<FeatureRank>();
            if (rank == null)
                return ifEmpty;
            return rank.Rank >= min && rank.Rank <= max;
        }

        public static bool IsRank(this BlueprintAbility ability, int min = -1, int max = int.MaxValue, bool ifEmpty = true)
        {
            var rank = ability.GetComponent<FeatureRank>();
            if (rank == null)
                return ifEmpty;
            return rank.Rank >= min && rank.Rank <= max;
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

        public static ContextDurationValue CreateContextDurationValue(AbilityRankType diceRank, DiceType dice = DiceType.Zero, int bonus = 0, DurationRate rate = DurationRate.Rounds)
        {
            if ((int)dice <= 1 && bonus == 0)
                return new ContextDurationValue()
                {
                    DiceCountValue = 0,
                    DiceType = DiceType.Zero,
                    BonusValue = CreateContextValue(diceRank),
                    Rate = rate
                };

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
        /// <summary>
        /// This creates a shallow copy. This means BlueprintComponents are shared and morphing can happen.<br/><br/>
        /// Watch out for these fields:<br/>
        /// BlueprintFeature: IsPrerequisiteFor
        /// BlueprintAbility: m_Parent
        /// </summary>
        /// <param name="guid2">guid to merge with, use for dynamic blueprints (unknown list of blueprints), otherwise empty</param>
        /// <returns></returns>
        public static T Clone<T>(this T obj, string name, string guid2 = null) where T : SimpleBlueprint
        {
            string guid = null;
            if (guid2 != null)
                guid = MergeIds(name, obj.AssetGuid.ToString(), guid2);

            if (guid == null)
                guid = GuidManager.i.Get(name);
            else
                GuidManager.i.Reg(guid);

            var result = (T)_memberwiseClone.Invoke(obj, null);
            result.name = name;
            AddAsset(result, guid);

            // prevent some morphing
            if (result is BlueprintFeature feature)
            {
                feature.IsPrerequisiteFor = new();
            }
            else if (result is BlueprintAbility ability)
            {
                ability.m_Parent = null;
            }

            return result;

            //var result = Activator.CreateInstance<T>();
            //var fields = typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            //foreach (var field in fields)
            //    field.SetValue(result, field.GetValue(obj));
            //return result;
        }

        public static T Clone<T>(this T obj, BlueprintScriptableObject parent) where T : BlueprintComponent
        {
            var result = (T)_memberwiseClone.Invoke(obj, null);
            obj.OwnerBlueprint = parent;
            return result;
        }

        public static T Clone<T>(this T obj, Action<T> action = null) where T : class
        {
            var result = (T)_memberwiseClone.Invoke(obj, null);
            action?.Invoke(result);
            return result;
        }

        private static ulong ParseGuidLow(string id) => ulong.Parse(id.Substring(id.Length - 16), System.Globalization.NumberStyles.HexNumber);
        private static ulong ParseGuidHigh(string id) => ulong.Parse(id.Substring(0, id.Length - 16), System.Globalization.NumberStyles.HexNumber);
        public static string MergeIds(string name, string guid1, string guid2, string guid3 = null)
        {
            // Parse into low/high 64-bit numbers, and then xor the two halves.
            ulong low = ParseGuidLow(guid1);
            ulong high = ParseGuidHigh(guid1);

            low ^= ParseGuidLow(guid2);
            high ^= ParseGuidHigh(guid2);

            if (guid3 != null)
            {
                low ^= ParseGuidLow(guid3);
                high ^= ParseGuidHigh(guid3);
            }

            var result = high.ToString("x16") + low.ToString("x16");
            PrintDebug($"MergeIds {guid1} + {guid2} + {guid3} = {result}");
            GuidManager.i.AddDynamic(name, result);
            return result;
        }

        public static BlueprintFeatureSelection _basicfeats1;
        public static BlueprintFeatureSelection _basicfeats2;
        public static BlueprintFeatureSelection _combatfeats1;
        public static BlueprintFeatureSelection _combatfeats2;
        public static BlueprintFeatureSelection _combatfeats3;
        public static void AddFeats(params BlueprintFeature[] feats)
        {
            if (_basicfeats1 == null) //BasicFeatSelection
                _basicfeats1 = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("247a4068296e8be42890143f451b4b45");
            Helper.AppendAndReplace(ref _basicfeats1.m_AllFeatures, feats.ToRef());

            if (_basicfeats2 == null) //ExtraFeatMythicFeat
                _basicfeats2 = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("e10c4f18a6c8b4342afe6954bde0587b");
            Helper.AppendAndReplace(ref _basicfeats2.m_AllFeatures, feats.ToRef());
        }
        public static void AddCombatFeat(BlueprintFeature feats)
        {
            if (_combatfeats1 == null) //FighterFeatSelection
                _combatfeats1 = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("41c8486641f7d6d4283ca9dae4147a9f");
            Helper.AppendAndReplace(ref _combatfeats1.m_AllFeatures, feats.ToRef());

            if (_combatfeats2 == null) //CombatTrick
                _combatfeats2 = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("c5158a6622d0b694a99efb1d0025d2c1");
            Helper.AppendAndReplace(ref _combatfeats2.m_AllFeatures, feats.ToRef());

            if (_combatfeats3 == null) //ExtraFeatMythicFeat
                _combatfeats3 = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("e10c4f18a6c8b4342afe6954bde0587b");
            Helper.AppendAndReplace(ref _combatfeats3.m_AllFeatures, feats.ToRef());

            AddFeats(feats);
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
        private static BlueprintFeatureSelection _vivsectionistfeats;
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
            if (_vivsectionistfeats == null)
                _vivsectionistfeats = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("67f499218a0e22944abab6fe1c9eaeee");

            var reference = feat.ToRef();

            Helper.AppendAndReplace(ref _roguefeats.m_AllFeatures, reference);
            Helper.AppendAndReplace(ref _slayerfeats1.m_AllFeatures, reference);
            Helper.AppendAndReplace(ref _slayerfeats2.m_AllFeatures, reference);
            Helper.AppendAndReplace(ref _slayerfeats3.m_AllFeatures, reference);
            Helper.AppendAndReplace(ref _vivsectionistfeats.m_AllFeatures, reference);
        }

        private static BlueprintFeatureSelection _witchhexes;
        private static BlueprintFeatureSelection _shamanhexes;
        private static BlueprintFeatureSelection _hexcrafterhexes;
        public static void AddHex(BlueprintFeature hex, bool allowShaman = true)
        {
            if (_witchhexes == null)
                _witchhexes = Helper.Get<BlueprintFeatureSelection>("9846043cf51251a4897728ed6e24e76f");
            if (_shamanhexes == null)
                _shamanhexes = Helper.Get<BlueprintFeatureSelection>("4223fe18c75d4d14787af196a04e14e7");
            if (_hexcrafterhexes == null)
                _hexcrafterhexes = Helper.Get<BlueprintFeatureSelection>("ad6b9cecb5286d841a66e23cea3ef7bf");

            var reference = hex.ToRef();

            Helper.AppendAndReplace(ref _witchhexes.m_AllFeatures, reference);
            if (allowShaman)
                Helper.AppendAndReplace(ref _shamanhexes.m_AllFeatures, reference);
            Helper.AppendAndReplace(ref _hexcrafterhexes.m_AllFeatures, reference);
        }

        public static BlueprintAbility AddToAbilityVariants(this BlueprintAbility parent, params BlueprintAbility[] variants)
        {
            var comp = parent.GetComponent<AbilityVariants>();
            if (comp == null)
            {
                comp = new AbilityVariants();
                comp.m_Variants = variants.ToRef();
                parent.AddComponents(comp);
            }
            else
            {
                Helper.AppendAndReplace(ref comp.m_Variants, variants.ToRef());
            }

            foreach (var v in variants)
            {
                v.Parent = parent;
            }
            return parent;
        }

        public static BlueprintBuff Flags(this BlueprintBuff buff, bool? hidden = null, bool? stayOnDeath = null, bool? isFromSpell = null, bool? harmful = null)
        {
            if (hidden != null)
            {
                if (hidden.Value)
                    buff.m_Flags |= BlueprintBuff.Flags.HiddenInUi;
                else
                    buff.m_Flags &= ~BlueprintBuff.Flags.HiddenInUi;
            }

            if (stayOnDeath != null)
            {
                if (stayOnDeath.Value)
                    buff.m_Flags |= BlueprintBuff.Flags.StayOnDeath;
                else
                    buff.m_Flags &= ~BlueprintBuff.Flags.StayOnDeath;
            }

            if (isFromSpell != null)
            {
                if (isFromSpell.Value)
                    buff.m_Flags |= BlueprintBuff.Flags.IsFromSpell;
                else
                    buff.m_Flags &= ~BlueprintBuff.Flags.IsFromSpell;
            }

            if (harmful != null)
            {
                if (harmful.Value)
                    buff.m_Flags |= BlueprintBuff.Flags.Harmful;
                else
                    buff.m_Flags &= ~BlueprintBuff.Flags.Harmful;
            }

            return buff;
        }

        public static void AddArcaneVendorItem(BlueprintItemReference item, int amount = 1)
        {
            var vendor_table = ResourcesLibrary.TryGetBlueprint<BlueprintSharedVendorTable>("5450d563aab78134196ee9a932e88671"); //ArcaneScrollsVendorTableI
            vendor_table.AddComponents(new LootItemsPackFixed() { m_Item = new LootItem() { m_Item = item }, m_Count = amount });
        }

        public static void AddExoticVendorItem(BlueprintItemReference item, int amount = 1)
        {
            var vendor_table = ResourcesLibrary.TryGetBlueprint<BlueprintSharedVendorTable>("5f722616e64f41b4f960cd00c0b4896c"); //RE_Chapter3VendorTableExotic
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

        public static ContextActionSavingThrow MakeContextActionSavingThrow(SavingThrowType savingthrow, GameAction succeed, GameAction failed)
        {
            var result = new ContextActionSavingThrow();
            result.Type = savingthrow;
            result.Actions = CreateActionList(new ContextActionConditionalSaved()
            {
                Succeed = CreateActionList(succeed),
                Failed = CreateActionList(failed)
            });
            return result;
        }

        public static void MakeStickySpell(BlueprintAbility spell, out BlueprintAbility cast, out BlueprintAbility effect)
        {
            const string guid_cast = "5fc3a01f26584d84b9c2bef04ec6cd8b";
            const string guid_effect = "73749154177c4f8c83231da1c9dc9f6f";

            cast = spell.Clone(spell.name + "_Cast", guid2: guid_cast)
                    .RemoveComponents<AbilityDeliverProjectile>()
                    .RemoveComponents<AbilityEffectRunAction>();
            effect = spell.Clone(spell.name + "_Effect", guid2: guid_effect)
                    .RemoveComponents<AbilityDeliverProjectile>();

            cast.AddComponents(CreateAbilityEffectStickyTouch(effect.ToRef()));
            cast.Range = AbilityRange.Touch;
            cast.Animation = CastAnimationStyle.Self;

            effect.AddComponents(CreateAbilityDeliverTouch());
            effect.Range = AbilityRange.Touch;
            effect.Animation = CastAnimationStyle.Touch;
        }

        public static BlueprintAbility TargetPoint(this BlueprintAbility ability, CastAnimationStyle animation = CastAnimationStyle.Directional, bool self = false)
        {
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.EffectOnAlly = AbilityEffectOnUnit.Harmful;
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
            ability.EffectOnAlly = AbilityEffectOnUnit.None;
            ability.CanTargetEnemies = true;
            ability.CanTargetPoint = false;
            ability.CanTargetFriends = false;
            ability.CanTargetSelf = false;
            ability.Animation = animation;
            return ability;
        }
        public static BlueprintAbility TargetAlly(this BlueprintAbility ability, CastAnimationStyle animation = CastAnimationStyle.Directional, bool self = true)
        {
            ability.EffectOnEnemy = AbilityEffectOnUnit.None;
            ability.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            ability.CanTargetEnemies = false;
            ability.CanTargetPoint = false;
            ability.CanTargetFriends = true;
            ability.CanTargetSelf = self;
            ability.Animation = animation;
            return ability;
        }
        public static BlueprintAbility TargetAny(this BlueprintAbility ability, CastAnimationStyle animation = CastAnimationStyle.Directional, bool self = true)
        {
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.EffectOnAlly = AbilityEffectOnUnit.Harmful;
            ability.CanTargetEnemies = true;
            ability.CanTargetPoint = false;
            ability.CanTargetFriends = true;
            ability.CanTargetSelf = self;
            ability.Animation = animation;
            return ability;
        }
        public static BlueprintAbility TargetSelf(this BlueprintAbility ability, CastAnimationStyle animation = CastAnimationStyle.Omni)
        {
            ability.EffectOnEnemy = AbilityEffectOnUnit.None;
            ability.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            ability.CanTargetEnemies = false;
            ability.CanTargetPoint = false;
            ability.CanTargetFriends = false;
            ability.CanTargetSelf = true;
            ability.Animation = animation;
            return ability;
        }

        /// <summary>Puts a new Conditional infront of existing ActionList.</summary>
        public static ActionList InjectCondition(this ActionList actionList, Condition condition, bool ifTrue = true)
        {
            if (ifTrue)
                actionList.Actions = new GameAction[] { CreateConditional(condition.ObjToArray(), ifTrue: actionList.Actions) };
            else
                actionList.Actions = new GameAction[] { CreateConditional(condition.ObjToArray(), ifFalse: actionList.Actions) };

            return actionList;
        }

        public static Conditional AddCondition(this Conditional conditional, Condition condition)
        {
            InsertAt(ref conditional.ConditionsChecker.Conditions, condition);
            return conditional;
        }

        public static ActionList InsertAt(this ActionList actionList, GameAction gameAction, int index = -1)
        {
            InsertAt(ref actionList.Actions, gameAction, index);
            return actionList;
        }

        #endregion

        #region Blueprint Weapon

        /// <param name="BlueprintWeaponEnchantment">direct type, reference, or string</param>
        public static BlueprintItemWeapon SetEnchantment(this BlueprintItemWeapon bp, params object[] BlueprintWeaponEnchantment)
        {
            var list = new List<BlueprintWeaponEnchantmentReference>();

            foreach (var obj in BlueprintWeaponEnchantment)
            {
                if (obj is string guid)
                    list.Add(guid.ToRef<BlueprintWeaponEnchantmentReference>());
                else if (obj is BlueprintWeaponEnchantmentReference reference)
                    list.Add(reference);
                else if (obj is BlueprintWeaponEnchantment enchantment)
                    list.Add(enchantment.ToRef());
                else
                    throw new ArgumentException("invalid type: " + obj.GetType().AssemblyQualifiedName);
            }

            bp.m_Enchantments = list.ToArray();
            return bp;
        }

        public static void GenerateEnchantedWeapons(this BlueprintWeaponType weaponType, List<BlueprintItemWeapon> list, BlueprintWeaponEnchantment enchantment, bool full, params object[] BlueprintWeaponEnchantment)
        {
            var plus1 = Helper.ToRef<BlueprintWeaponEnchantmentReference>("d42fc23b92c640846ac137dc26e000d4");
            var plus2 = Helper.ToRef<BlueprintWeaponEnchantmentReference>("eb2faccc4c9487d43b3575d7e77ff3f5");
            var plus3 = Helper.ToRef<BlueprintWeaponEnchantmentReference>("80bb8a737579e35498177e1e3c75899b");
            var plus4 = Helper.ToRef<BlueprintWeaponEnchantmentReference>("783d7d496da6ac44f9511011fc5f1979");
            var plus5 = Helper.ToRef<BlueprintWeaponEnchantmentReference>("bdba267e951851449af552aa9f9e3992");

            throw new NotImplementedException();
        }

        #endregion

        #region Blueprint Progression

        /// <summary>Merges LevelEntries or appends new ones.</summary>
        public static void AddEntries(this BlueprintProgression progression, List<LevelEntry> entries)
        {
            for (int i = entries.Count - 1; i > 0; i--)
            {
                if (progression.LevelEntries.TryFind(f => f.Level == entries[i].Level, out var entry))
                {
                    entry.m_Features.AddRange(entries[i].m_Features);
                    entries.RemoveAt(i);
                }
            }

            if (entries.Count > 0)
            {
                Helper.AppendAndReplace(ref progression.LevelEntries, entries);
            }
        }
        public static void AddEntries(this BlueprintProgression progression, params LevelEntry[] entries) => AddEntries(progression, entries.ToList());

        public static void AddFeature(this BlueprintProgression progression, int level, BlueprintFeature feature, string pairWithGuid = null)
        {
            var levelentry = progression.LevelEntries.FirstOrDefault(f => f.Level == level);
            if (levelentry != null)
                levelentry.m_Features.Add(feature.ToReference<BlueprintFeatureBaseReference>());
            else
                AppendAndReplace(ref progression.LevelEntries, CreateLevelEntry(level, feature));

            if (pairWithGuid != null)
            {
                var pairGuid = BlueprintGuid.Parse(pairWithGuid);
                foreach (var ui in progression.UIGroups)
                {
                    if (ui.m_Features.Any(a => a.deserializedGuid == pairGuid))
                    {
                        ui.m_Features.Add(feature.ToReference<BlueprintFeatureBaseReference>());
                        break;
                    }
                }
            }

        }

        public static void ClearEntries(this BlueprintProgression progression, int level)
        {
            if (level < 0)
                progression.LevelEntries = Array.Empty<LevelEntry>();
            else
            {
                for (int i = progression.LevelEntries.Length - 1; i >= 0; i--)
                {
                    if (progression.LevelEntries[i].Level == level)
                    {
                        RemoveAt(ref progression.LevelEntries, i);
                        break;
                    }
                }
            }
        }

        #endregion

        #region Blueprint Selection

        public static BlueprintFeatureSelection Add(this BlueprintFeatureSelection selection, params BlueprintFeatureReference[] features)
        {
            Helper.AppendAndReplace(ref selection.m_AllFeatures, features);

            return selection;
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

        public static AbilityEffectStickyTouch CreateAbilityEffectStickyTouch(BlueprintAbilityReference ability)
        {
            var result = new AbilityEffectStickyTouch();
            result.m_TouchDeliveryAbility = ability;
            return result;
        }

        public static AbilityDeliverTouch CreateAbilityDeliverTouch()
        {
            var result = new AbilityDeliverTouch();
            result.m_TouchWeapon = Resource.Cache.WeaponTouch;
            return result;
        }

        public static AddAreaEffect CreateAddAreaEffect(this BlueprintAbilityAreaEffect area)
        {
            var result = new AddAreaEffect();
            result.m_AreaEffect = area.ToRef();
            return result;
        }

        public static CombatStateTrigger CreateCombatStateTrigger(GameAction start = null, GameAction end = null)
        {
            var result = new CombatStateTrigger();
            result.CombatStartActions = CreateActionList(start);
            result.CombatEndActions = CreateActionList(start);
            return result;
        }

        public static AddFactOnlyParty CreateAddFactOnlyParty(BlueprintUnitFactReference fact, FeatureParam param = null)
        {
            var result = new AddFactOnlyParty();
            result.Feature = fact;
            result.Parameter = param;
            return result;
        }

        public static AutoMetamagic CreateAutoMetamagic(Metamagic metamagic, List<BlueprintAbilityReference> abilities = null, AutoMetamagic.AllowedType type = AutoMetamagic.AllowedType.Any)
        {
            var result = new AutoMetamagic();
            result.m_AllowedAbilities = type;
            result.Metamagic = metamagic;
            result.Abilities = abilities ?? new List<BlueprintAbilityReference>();
            return result;
        }

        public static AddFactContextActions CreateAddFactContextActions(GameAction[] on = null, GameAction[] off = null, GameAction[] round = null)
        {
            var result = new AddFactContextActions();
            result.Activated = CreateActionList(on);
            result.Deactivated = CreateActionList(off);
            result.NewRound = CreateActionList(round);
            return result;
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

        public static ContextActionAddFeature CreateContextActionAddFeature(BlueprintFeatureReference permanent)
        {
            var result = new ContextActionAddFeature();
            result.m_PermanentFeature = permanent;
            return result;
        }

        /// <summary>Adds a fact, but only fact not already granted through other means.</summary>
        public static AddFeatureIfHasFact CreateAddFeatureIfHasFact(BlueprintUnitFactReference feature)
        {
            var result = new AddFeatureIfHasFact();
            result.m_CheckedFact = feature;
            result.m_Feature = feature;
            result.Not = true;
            return result;
        }

        public static AddFeatureIfHasFact CreateAddFeatureIfHasFact(BlueprintUnitFactReference checkedFact, BlueprintUnitFactReference feature, bool not = false)
        {
            var result = new AddFeatureIfHasFact();
            result.m_CheckedFact = checkedFact;
            result.m_Feature = feature;
            result.Not = not;
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

        public static AddInitiatorAttackWithWeaponTrigger CreateAddInitiatorAttackWithWeaponTrigger(ActionList Action, bool OnlyHit = true, bool OnlySneakAttack = false)
        {
            var result = new AddInitiatorAttackWithWeaponTrigger();
            result.Action = Action;
            result.OnlyHit = OnlyHit;
            result.OnlySneakAttack = OnlySneakAttack;
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

        public static AddConditionExceptions CreateAddConditionExceptions(UnitCondition condition, params BlueprintBuffReference[] buffs)
        {
            var result = new AddConditionExceptions();
            result.Condition = condition;
            result.Exception = new UnitConditionExceptionsFromBuff { Exceptions = buffs };
            return result;
        }

        public static SpellDescriptorComponent CreateSpellDescriptorComponent(SpellDescriptor descriptor)
        {
            return new SpellDescriptorComponent { Descriptor = descriptor };
        }

        public static SpellImmunityToSpellDescriptor CreateSpellImmunityToSpellDescriptor(SpellDescriptor descriptor, BlueprintUnitFactReference ignoreFact = null)
        {
            var result = new SpellImmunityToSpellDescriptor();
            result.Descriptor = descriptor;
            result.m_CasterIgnoreImmunityFact = ignoreFact;
            return result;
        }

        public static AddMechanicsFeature CreateAddMechanicsFeature(AddMechanicsFeature.MechanicsFeatureType feature)
        {
            var result = new AddMechanicsFeature();
            result.m_Feature = feature;
            return result;
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

        public static AbilityDeliverProjectile CreateAbilityDeliverProjectile(BlueprintProjectileReference projectile, AbilityProjectileType type = AbilityProjectileType.Simple, BlueprintItemWeaponReference weapon = null, Feet length = default, Feet width = default)
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
            if (save == SavingThrowType.Unknown)
                result.Actions = CreateActionList(actions);
            else
                result.Actions = CreateActionList(CreateContextActionConditionalSaved(failed: actions));
            return result;
        }

        public static AbilityExecuteActionOnCast CreateAbilityExecuteActionOnCast(params GameAction[] actions)
        {
            var result = new AbilityExecuteActionOnCast();
            result.Actions = CreateActionList(actions);
            result.Conditions = new ConditionsChecker() { Operation = Operation.And, Conditions = Array.Empty<Condition>() };
            return result;
        }

        public static AbilityExecuteActionOnCast CreateAbilityExecuteActionOnCast(GameAction[] actions, Condition[] conditions = null, Operation operation = Operation.And)
        {
            var result = new AbilityExecuteActionOnCast();
            result.Actions = CreateActionList(actions);
            result.Conditions = new ConditionsChecker() { Operation = operation, Conditions = conditions ?? Array.Empty<Condition>() };
            return result;
        }

        public static ConditionsChecker CreateConditionsChecker(Operation operation = Operation.And, params Condition[] conditions)
        {
            var result = new ConditionsChecker() { Operation = operation, Conditions = conditions ?? Array.Empty<Condition>() };
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

        public static ContextConditionHasFact CreateContextConditionHasFact(BlueprintUnitFactReference fact, bool not = false)
        {
            var result = new ContextConditionHasFact();
            result.m_Fact = fact;
            result.Not = not;
            return result;
        }

        public static ContextConditionHasFactRank CreateContextConditionHasFactRank(BlueprintUnitFactReference fact, ContextValue rank)
        {
            var result = new ContextConditionHasFactRank();
            result.Fact = fact;
            result.RankValue = rank;
            return result;
        }

        public static ContextConditionCasterHasFact CreateContextConditionCasterHasFact(BlueprintUnitFactReference fact)
        {
            var result = new ContextConditionCasterHasFact();
            result.m_Fact = fact;
            return result;
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

        public static ContextActionConditionalSaved CreateContextActionConditionalSaved(GameAction[] succeed = null, GameAction[] failed = null)
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

        public static ContextActionApplyBuff CreateContextActionApplyBuff(this BlueprintBuff buff, int duration = 0, DurationRate rate = DurationRate.Rounds, bool fromSpell = false, bool dispellable = false, bool toCaster = false, bool asChild = false, bool permanent = false)
        {
            return CreateContextActionApplyBuff(buff, CreateContextDurationValue(bonus: duration, rate: rate), fromSpell: fromSpell, toCaster: toCaster, asChild: asChild, dispellable: dispellable, permanent: permanent);
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

        public static ContextActionApplyBuff CreateContextActionApplyBuff(this BlueprintBuff buff, float durationInSeconds, bool fromSpell = false, bool dispellable = true, bool toCaster = false, bool asChild = false, bool permanent = false)
        {
            var result = new ContextActionApplyBuff();
            result.m_Buff = buff.ToRef();
            result.UseDurationSeconds = true;
            result.DurationSeconds = durationInSeconds;
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

        public static PrerequisiteMythicLevel CreatePrerequisiteMythicLevel(int level)
        {
            var result = new PrerequisiteMythicLevel();
            result.Level = level;
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

        public static PrerequisiteArchetypeLevel CreatePrerequisiteArchetypeLevel(BlueprintArchetypeReference archetype, int level = 1, bool any = false, BlueprintCharacterClassReference characterClass = null)
        {
            var result = new PrerequisiteArchetypeLevel();
            result.m_Archetype = archetype;
            result.m_CharacterClass = characterClass ?? archetype.Get().GetParentClass().ToRef();
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

        public static DuplicateSpell CreateDuplicateSpell(Func<AbilityData, bool> abilityCheck, int radius = 30)
        {
            return new DuplicateSpell
            {
                Radius = radius,
                AbilityCheck = abilityCheck
            };
        }

        public static LevelUpAddSelectionHasFact CreateLevelUpAddSelectionHasFact(BlueprintUnitFact[] facts, IFeatureSelection selection)
        {
            var result = new LevelUpAddSelectionHasFact();
            result.Facts = facts;
            result.Selection = selection; // takes BlueprintFeatureSelection or BlueprintParametrizedFeature
            return result;
        }

        public static LearnSpellList CreateLearnSpellList(BlueprintSpellListReference spellList, BlueprintCharacterClassReference characterClass = null, BlueprintArchetypeReference archetype = null)
        {
            var result = new LearnSpellList();
            result.m_SpellList = spellList;
            result.m_CharacterClass = characterClass ?? archetype.Get().GetParentClass().ToRef();
            result.m_Archetype = archetype;
            return result;
        }

        public static AddKnownSpell CreateAddKnownSpell(BlueprintAbilityReference spell, int SpellLevel, BlueprintCharacterClassReference characterClass = null, BlueprintArchetypeReference archetype = null)
        {
            var result = new AddKnownSpell(); //AddSpellKnownTemporary
            result.m_Spell = spell;
            result.SpellLevel = SpellLevel;
            result.m_CharacterClass = characterClass ?? archetype.Get().GetParentClass().ToRef();
            result.m_Archetype = archetype;
            return result;
        }

        public static BlueprintSpellList CreateBlueprintSpellList()
        {
            throw new NotImplementedException();
            //var result = new BlueprintSpellList();
            //return result;
        }

        public static BlueprintBuff CreateBlueprintBuff(string name, string displayName, string description, string guid = null, Sprite icon = null, PrefabLink fxOnStart = null)
        {
            if (guid == null)
                guid = GuidManager.i.Get(name);
            else
                GuidManager.i.Reg(guid);

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
            else
                GuidManager.i.Reg(guid);

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
            else
                GuidManager.i.Reg(guid);

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

        public static BlueprintFeatureSelection CreateBlueprintFeatureSelection(string name, string displayName, string description, string guid = null, Sprite icon = null, FeatureGroup group = 0, SelectionMode mode = SelectionMode.OnlyNew)
        {
            if (guid == null)
                guid = GuidManager.i.Get(name);
            else
                GuidManager.i.Reg(guid);

            var result = new BlueprintFeatureSelection();
            result.IsClassFeature = true;
            result.name = name;
            result.m_DisplayName = displayName.CreateString();
            result.m_Description = description.CreateString();
            result.Groups = group == 0 ? Array.Empty<FeatureGroup>() : ToArray(group);
            result.m_Icon = icon;
            result.Mode = mode;

            AddAsset(result, guid);
            return result;
        }

        public static BlueprintParametrizedFeature CreateBlueprintParametrizedFeature(string name, string displayName, string description, string guid = null, Sprite icon = null, FeatureGroup group = 0, FeatureParameterType parameterType = FeatureParameterType.Custom, AnyBlueprintReference[] blueprints = null)
        {
            if (guid == null)
                guid = GuidManager.i.Get(name);
            else
                GuidManager.i.Reg(guid);

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
            else
                GuidManager.i.Reg(guid);

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

        public static BlueprintAbilityAreaEffect CreateBlueprintAbilityAreaEffect(string name, string guid = null, bool applyEnemy = false, bool applyAlly = false, AreaEffectShape shape = AreaEffectShape.Cylinder, Feet size = default, PrefabLink sfx = null, BlueprintBuffReference buffWhileInside = null, ActionList unitEnter = null, ActionList unitExit = null, ActionList unitMove = null, ActionList unitRound = null)
        {
            if (!applyAlly && !applyEnemy)
                throw new ArgumentException("area must effect either allies or enemies");

            if (guid == null)
                guid = GuidManager.i.Get(name);
            else
                GuidManager.i.Reg(guid);

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
                AbilityAreaEffectBuff areabuff = new(); // applies buff while inside
                areabuff.Condition = new ConditionsChecker();
                areabuff.CheckConditionEveryRound = false;
                areabuff.m_Buff = buffWhileInside;

                result.SetComponents(areabuff);
            }

            if (unitEnter != null || unitExit != null || unitMove != null || unitRound != null)
            {
                AbilityAreaEffectRunAction runaction = new(); // runs actions that persist even when leaving
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
            else
                GuidManager.i.Reg(guid);

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

        public static BlueprintWeaponType CreateBlueprintWeaponType(string name, string displayName, string cloneFromWeaponType, Feet range = default, DiceFormula damage = default, PhysicalDamageForm form = PhysicalDamageForm.Bludgeoning, int critRange = 20, DamageCriticalModifierType critMod = DamageCriticalModifierType.X2, WeaponFighterGroupFlags? fighterGroup = null, WeaponCategory? category = null, float? weight = null, string guid = null)
        {
            // note: take care to not mutate m_VisualParameters!
            if (guid == null)
                guid = GuidManager.i.Get(name);
            else
                GuidManager.i.Reg(guid);
            var clone = ResourcesLibrary.TryGetBlueprint<BlueprintWeaponType>(cloneFromWeaponType);
            if (range == default)
                range = 5.Feet();
            if (damage == default)
                damage = new DiceFormula(1, DiceType.D6);

            var result = new BlueprintWeaponType();
            result.name = name;

            result.m_TypeNameText = displayName.CreateString();
            result.m_DefaultNameText = result.m_TypeNameText;
            result.m_DescriptionText = "".CreateString();
            result.m_MasterworkDescriptionText = "".CreateString();
            result.m_MagicDescriptionText = "".CreateString();

            result.m_AttackRange = range;
            result.m_BaseDamage = damage;
            result.m_DamageType = new DamageTypeDescription();
            result.m_DamageType.Physical.Form = form;
            result.m_CriticalRollEdge = critRange;
            result.m_CriticalModifier = critMod;
            result.m_FighterGroupFlags = fighterGroup ?? clone.m_FighterGroupFlags;
            result.Category = category ?? clone.Category;
            result.m_Weight = weight ?? clone.m_Weight;

            result.m_VisualParameters = clone.m_VisualParameters;
            result.m_Icon = clone.Icon;
            result.m_IsTwoHanded = clone.m_IsTwoHanded;
            result.m_IsLight = clone.m_IsLight;
            result.m_IsMonk = clone.m_IsMonk;
            result.m_IsNatural = clone.m_IsNatural;
            result.m_IsUnarmed = clone.m_IsUnarmed;
            result.m_OverrideAttackBonusStat = clone.m_OverrideAttackBonusStat;
            result.m_AttackBonusStatOverride = clone.m_AttackBonusStatOverride;
            result.m_ShardItem = clone.m_ShardItem;

            result.m_Enchantments = clone.m_Enchantments;

            AddAsset(result, guid);
            return result;
        }

        public static BlueprintItemWeapon CreateBlueprintItemWeapon(string name, string displayName, string description, BlueprintWeaponTypeReference weaponType, DiceFormula? damageOverride = null, DamageTypeDescription form = null, BlueprintItemWeaponReference secondWeapon = null, bool primaryNatural = false, string guid = null, int price = 1000)
        {
            if (guid == null)
                guid = GuidManager.i.Get(name);
            else
                GuidManager.i.Reg(guid);

            var result = new BlueprintItemWeapon();
            result.name = name;

            result.m_DisplayNameText = displayName.CreateString();
            result.m_DescriptionText = description.CreateString();
            result.m_Type = weaponType;
            result.m_Size = Size.Medium;
            result.m_OverrideDamageDice = damageOverride != null;
            result.m_DamageDice = damageOverride.GetValueOrDefault();
            result.m_OverrideDamageType = form != null;
            result.m_DamageType = form;
            result.Double = secondWeapon != null;
            result.m_SecondWeapon = secondWeapon;
            result.KeepInPolymorph = false;
            result.m_AlwaysPrimary = primaryNatural;
            result.m_Cost = price;

            // didn't needs these values so far
            result.CR = 0;
            result.m_Ability = null;
            result.m_ActivatableAbility = null;
            result.SpendCharges = false;
            result.Charges = 1;
            result.RestoreChargesOnRest = result.SpendCharges;
            result.CasterLevel = 1;
            result.SpellLevel = 1;
            result.DC = 11;
            result.m_Weight = 0;
            result.m_FlavorText = "".CreateString();
            result.m_NonIdentifiedNameText = "".CreateString();
            result.m_NonIdentifiedDescriptionText = "".CreateString();
            result.m_Icon = null; //can be null

            result.m_Enchantments = Array.Empty<BlueprintWeaponEnchantmentReference>();

            // not sure
            result.OnEnableWithLibrary();
            result.m_VisualParameters.m_Projectiles = Array.Empty<BlueprintProjectileReference>();
            result.m_VisualParameters.m_PossibleAttachSlots = Array.Empty<UnitEquipmentVisualSlotType>();

            AddAsset(result, guid);
            return result;
        }

        #endregion

        #region Library

        public static PrefabLink GetPrefabLink(string assetId)
        {
            return new PrefabLink() { AssetId = assetId };
        }

        public static T Get<T>(string guid) where T : SimpleBlueprint
        {
            var sb = ResourcesLibrary.TryGetBlueprint(BlueprintGuid.Parse(guid));

            if (sb is T bp)
                return bp;
            else if (sb == null)
                Print("ERROR: could not resolve " + guid);
            else
                Print($"ERROR: invalid conversion {sb.name} : {guid}");

            return null;
        }

        #endregion

        #region GameActions

        // recursively search for GameActions
        public static List<T> Get<T>(this BlueprintAbility ability, Action<T> func = null, Func<T, bool> match = null) where T : GameAction
        {
            var list = ability.GetComponent<AbilityEffectRunAction>()?.Actions?.Actions.Get<T>();

            if (match != null)
                for (int i = list.Count - 1; i > 0; i--)
                    if (match.Invoke(list[i]))
                        list.RemoveAt(i);

            if (func != null)
                list.ForEach(func);

            return list;
        }
        public static List<T> Get<T>(this GameAction[] thisActions) where T : GameAction
        {
            var list = new List<T>();
            thisActions.Get<T>(ref list);
            return list;
        }
        public static void Get<T>(this GameAction[] thisActions, ref List<T> list) where T : GameAction
        {
            if (thisActions == null)
                return;

            foreach (var action in thisActions)
            {
                if (action is T x)
                    list.Add(x);

                foreach (var field in action.GetType().GetFields())
                {
                    if (field.FieldType == typeof(ActionList))
                        ((ActionList)field.GetValue(thisActions))?.Actions.Get<T>(ref list);
                }
            }
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
            //var res = ResourcesLibrary.TryGetBlueprint(BlueprintGuid.Parse(guid));            
            //if (res is BlueprintUnitFact res1)
            try
            {
                var bp = ResourcesLibrary.TryGetBlueprint(BlueprintGuid.Parse(guid));
                if (bp is BlueprintUnitFact fact)
                    return fact.m_Icon;
                if (bp is BlueprintWeaponType weapontype)
                    return weapontype.m_Icon;
            }
            catch (Exception)
            {
            }
            Print("Could not import icon from " + guid);
            return null;
        }

        public static Sprite CreateSprite(string filename, int width = 64, int height = 64)
        {
            try
            {
                var bytes = File.ReadAllBytes(Path.Combine(Main.ModPath, "Icons", filename));
                var texture = new Texture2D(width, height);
                texture.LoadImage(bytes);
                return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0, 0));
            }
            catch (Exception e)
            {
                PrintException(e);
                return null;
            }
        }

        public static Texture CreateTexture(string filename, int width = 64, int height = 64)
        {
            try
            {
                var bytes = File.ReadAllBytes(Path.Combine(Main.ModPath, "Icons", filename));
                var texture = new Texture2D(width, height);
                texture.LoadImage(bytes);
                return texture;
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
