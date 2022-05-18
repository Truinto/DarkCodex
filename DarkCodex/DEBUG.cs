using HarmonyLib;
using Kingmaker;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Armies;
using Kingmaker.Armies.TacticalCombat;
using Kingmaker.Armies.TacticalCombat.Controllers;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.Kingdom;
using Kingmaker.Kingdom.Settlements;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI.Common;
using Kingmaker.UI.Loot;
using Kingmaker.UI.MVVM._VM.Loot;
using Kingmaker.UI.MVVM._VM.Tooltip.Templates;
using Kingmaker.UI.Tooltip;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.Utility;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TurnBased.Controllers;
using UnityEngine;
using UnityModManagerNet;
using Shared;
using CodexLib;
using Kingmaker.EntitySystem.Persistence;
using System.Text.RegularExpressions;

namespace DarkCodex
{
    public class DEBUG
    {
        [HarmonyPatch]
        public class Patch_UpdateSave130
        {
            private static Regex rx = new("\\\"DarkCodex\\.Patch_AbilityGroups\\+(.*?), DarkCodex\\\"");

            [HarmonyPatch(typeof(ThreadedGameLoader), nameof(ThreadedGameLoader.LoadJson))]
            [HarmonyPostfix]
            public static void Postfix(ISaver saver, string path, ref string __result)
            {
                if (path == "party")
                {
                    __result = rx.Replace(__result, "\"CodexLib.${1}, CodexLib\"");
                    Main.Print(__result);
                }
            }
        }

        public static void ExportAllIconTextures()
        {
            foreach (var bp in ResourcesLibrary.BlueprintsCache.m_LoadedBlueprints.Values)
            {
                if (bp.Blueprint is BlueprintUnitFact fact && fact.m_Icon != null)
                {
                    try
                    {
                        Texture.allowThreadedTextureCreation = true;
                        File.WriteAllBytes(Path.Combine(Main.ModPath, "IconsExport", fact.m_Icon.name), ImageConversion.EncodeToPNG(fact.m_Icon.texture));
                        Main.PrintDebug($"Export sprite {fact.m_Icon.name} from {fact.name} ");
                    }
                    catch (Exception)
                    {
                        Main.PrintDebug($"Didn't like sprite {fact.m_Icon.name} from {fact.name} ");
                    }
                }
            }
        }

        public class Date //#278
        {
            public static void SetDate()
            {
                //KingdomState.Instance.CurrentDay -= 1;
                //Helper.Print(Game.Instance.BlueprintRoot.Calendar.GetDateText(KingdomState.Instance.Date - Game.Instance.BlueprintRoot.Calendar.GetStartDate(), GameDateFormat.Full, true));
                Game.Instance.AdvanceGameTime(new TimeSpan(days: -1, 0, 0, 0, 0));
                Game.Instance.MatchTimeOfDay(null);
                Game.Instance.KingdomController.Tick();
            }
        }

        public class Loot //#365
        {
            public static void Open()
            {
                Main.PrintDebug("opening shared stash with items " + Game.Instance.Player.SharedStash.Count());

                Main.PrintDebug("Stash: " + Game.Instance.Player.SharedStash.Select(s => s.Blueprint.name).Join());
                Main.PrintDebug("Inventory: " + Game.Instance.Player.Inventory.Select(s => s.Blueprint.name).Join());

                try
                {
                    UnityModManager.UI.Instance.ToggleWindow();

                    if (Game.Instance.UI.LootWindowController == null)
                    {
                        Main.PrintDebug("MainCharacter is null");
                        Game.Instance.UI.LootWindowController.Initialize();
                    }

                    if (Game.Instance == null)
                        Main.PrintDebug("Instance is null");
                    if (Game.Instance.UI == null)
                        Main.PrintDebug("UI is null");
                    if (Game.Instance.UI?.LootWindowController == null)
                        Main.PrintDebug("LootWindowController is null");
                    if (Game.Instance.UI?.LootWindowController?.m_Collector == null)
                        Main.PrintDebug("m_Collector is null");
                    if (Game.Instance.Player?.MainCharacter.Value == null)
                        Main.PrintDebug("MainCharacter is null");
                    if (Game.Instance.Player.Party.FirstOrDefault() == null)
                        Main.PrintDebug("Party is empty");

                    //Kingmaker.UI.Loot.LootWindowController window = Game.Instance.UI.LootWindowController;
                    //window.HandleLootInterraction(Game.Instance.Player.Party.First(), Game.Instance.Player.SharedStash, "Remote Stash");


                }
                catch (Exception e)
                {
                    Main.Print(e.ToString());
                }

                //window.WindowMode = LootWindowMode.PlayerChest;
                //window.m_Collector.SetData(Game.Instance.Player.SharedStash, "Remote Stash");
                //window.Show(true);

            }
        }

        [HarmonyPatch(typeof(AbilityData), MethodType.Constructor, new Type[] { typeof(BlueprintAbility), typeof(UnitDescriptor), typeof(Ability), typeof(BlueprintSpellbook) })]
        public class SpellReach
        {
            public static void Prefix(AbilityData __instance, BlueprintAbility blueprint, UnitDescriptor caster)
            {
                if (!Settings.State.debug_1)
                    return;

                if (blueprint.Range == AbilityRange.Personal)
                    __instance.OverrideRange = AbilityRange.Touch;
            }
        }

        [PatchInfo(Severity.Extend | Severity.WIP, "Display All", "makes enchantments visible for items that don't usually display them", false)]
        [HarmonyPatch(typeof(UIUtilityItem), nameof(UIUtilityItem.FillEnchantmentDescription), new Type[] { typeof(ItemEntity), typeof(ItemTooltipData) })]
        public class Enchantments
        {
            public static void Postfix(ItemEntity item, ItemTooltipData itemTooltipData, ref string __result)
            {
                if (!item.IsIdentified)
                    return;

                // append enchantment qualities for items that don't usually display them
                itemTooltipData.Texts.TryGetValue(TooltipElement.Qualities, out string text);
                var sb = new StringBuilder(text);
                if (text == null)
                    sb.Append(UIUtilityItem.GetQualities(item));

                if (item.Ability != null)
                {
                    UIUtilityTexts.AddWord(sb, "Ability: ");
                    sb.Append(item.Ability.Name);
                }

                if (item.ActivatableAbility != null)
                {
                    UIUtilityTexts.AddWord(sb, "Activatable: ");
                    sb.Append(item.ActivatableAbility.Name);
                }

                if (sb.Length > 0)
                {
                    itemTooltipData.Texts[TooltipElement.Qualities] = sb.ToString();
                }

                if (!(item is ItemEntityWeapon || item is ItemEntityArmor || item is ItemEntityShield || item is ItemEntityUsable))
                {
                    foreach (ItemEnchantment itemEnchantment in item.VisibleEnchantments)
                    {
                        if (!string.IsNullOrEmpty(itemEnchantment.Blueprint.Description))
                        {
                            __result += string.Format("<b><align=\"center\">{0}</align></b>\n{1}\n\n", itemEnchantment.Blueprint.Name, itemEnchantment.Blueprint.Description);
                        }
                    }
                }

                if (item.Ability != null && !item.Ability.Blueprint.m_Description.IsEmpty())
                {
                    __result += string.Format("<b><align=\"center\">{0}</align></b>\n{1}\n\n", item.Ability.Name, item.Ability.Description);
                }

                if (item.ActivatableAbility != null && !item.ActivatableAbility.Blueprint.m_Description.IsEmpty())
                {
                    __result += string.Format("<b><align=\"center\">{0}</align></b>\n{1}\n\n", item.ActivatableAbility.Name, item.ActivatableAbility.Description);
                }
            }

            [PatchInfo(Severity.Extend | Severity.WIP, "Name All", "gives all enchantments a name and description", false)]
            public static void NameAll()
            {
                Resource.Cache.Ensure();

                StringBuilder sb = new();
#if DEBUG
                using (StreamWriter sw = new(Path.Combine(Main.ModPath, "enchantment-export.txt"), false)) // todo: remove debug log
                {
                    sw.WriteLine("names");
#endif
                    foreach (var enchantment in Resource.Cache.Enchantment)
                    {
                        if (enchantment?.m_EnchantName == null || enchantment.m_EnchantName.m_Key != "" && enchantment.m_EnchantName != "") // todo: check if string conversion is worth it
                            continue;

                        string name = enchantment.name;
                        if (name == null) continue;
                        name = name.Replace("Enchantment", "");
                        name = name.Replace("Enchant", "");
                        name = name.Replace("Plus", "");
                        if (name.Length < 1) continue;
                        sb.Clear();
                        sb.Append(name[0]);
                        for (int i = 1; i < name.Length; i++)
                        {
                            // space uppercase char, unless the previous char was already spaced, unless the next char is lowercase (if any)
                            if (name[i].IsUppercase() && (!name[i - 1].IsUppercase() || (name.Length > i + 1 && name[i + 1].IsLowercase())))
                                sb.Append(' ');

                            if (name[i].IsNumber() && !name[i - 1].IsNumber()) // prefix number blocks with '+'
                                sb.Append('+');

                            if (name[i] != '_')         // print char, except '_'
                                sb.Append(name[i]);
                            else if (sb.IsNotSpaced())  // replace '_' unless last char is already spacebar
                                sb.Append(' ');
                        }

                        enchantment.m_EnchantName = sb.ToString().CreateString();
#if DEBUG
                        sw.Write(enchantment.AssetGuid);
                        sw.Write("\t");
                        sw.WriteLine(sb);
#endif
                    }
#if DEBUG
                    sw.WriteLine("descriptions");
#endif
                    foreach (var item in Resource.Cache.Item)
                    {
                        try
                        {
                            if (item == null || item.m_DescriptionText == null || item.m_DescriptionText.IsEmpty() || item.m_DescriptionText == "")
                                continue;

                            foreach (var enchantent in item.CollectEnchantments().Where(w => w.m_Description == null || w.m_Description.IsEmpty() || w.m_Description == ""))
                            {
                                enchantent.m_Description = ((string)item.m_DescriptionText).CreateString("enchant#" + item.m_DescriptionText.m_Key);
#if DEBUG
                                sw.Write(enchantent.AssetGuid);
                                sw.Write("\t");
                                sw.WriteLine(enchantent.m_Description.ToString());
#endif
                            }
                        }
                        catch (Exception)
                        {
#if DEBUG
                            sw.WriteLine(item.AssetGuid + "\tcaused crash");
#endif
                        }
                        // regex to fix linebreaks "\n(?![a-f0-9]{32}\t)", "\\n"
                    }
#if DEBUG
                }
#endif
            }

            public static string ConvertName(string name)
            {
                if (name == null)
                    return null;

                name = name.Replace("Enchantment", "");
                name = name.Replace("Enchant", "");
                name = name.Replace("Plus", "");
                if (name.Length < 1)
                    return null;

                var sb = Resource.sb;
                sb.Clear();
                sb.Append(name[0]);
                for (int i = 1; i < name.Length; i++)
                {
                    // space uppercase char, unless the previous char was already spaced, unless the next char is lowercase (if any)
                    if (name[i].IsUppercase() && (!name[i - 1].IsUppercase() || (name.Length > i + 1 && name[i + 1].IsLowercase())))
                        if (sb.IsNotSpaced())
                            sb.Append(' ');

                    // prefix number blocks with '+'
                    if (name[i].IsNumber() && !name[i - 1].IsNumber())
                    {
                        if (sb.IsNotSpaced()) 
                            sb.Append(' ');
                        sb.Append('+');
                    }

                    if (name[i] != '_')         // print char, except '_'
                        sb.Append(name[i]);
                    else if (sb.IsNotSpaced())  // replace '_' unless last char is already spacebar
                        sb.Append(' ');
                }

                return sb.ToString();
            }

        }

        [HarmonyPatch(typeof(TacticalCombatTurnController), nameof(TacticalCombatTurnController.ReadyToStartNextTurn), MethodType.Getter)]
        public class ArmyLeader1
        {
            public static void Leader()
            {
                var attacker = Game.Instance.TacticalCombat.Data.Attacker.LeaderData;
                var defender = Game.Instance.TacticalCombat.Data.Defender.LeaderData;

                bool isAttacking = attacker.Faction == ArmyFaction.Crusaders;

                //Game.Instance.TacticalCombat.Data.Turn.
            }

            public static void Prefix(TacticalCombatTurnController __instance)
            {

                var turn = Game.Instance.TacticalCombat.Data.Turn;
                var unit = turn.Unit;

                if (unit == null || TacticalCombatHelper.IsDemon(unit))
                    return;

                unit.GetTacticalData().LeaderActionUsed = false;

                if (!__instance.TurnEnded)
                    return;

                return;

                __instance.m_PrevTurnEndTime = null;
                turn.UsedActionsCount = 0;
                turn.UpdateStandardAction(false);
            }
        }

        [HarmonyPatch(typeof(SettlementState), nameof(SettlementState.GetSellPrice))]
        public class Settlement1
        {
            public static float sell_multiplier = 1f;

            public static void Postfix(ref KingdomResourcesAmount __result)
            {
                __result *= sell_multiplier;
            }
        }

        [HarmonyPatch(typeof(SettlementState), nameof(SettlementState.GetActualCost), new Type[] { typeof(BlueprintSettlementBuilding) })]
        public class Settlement2
        {
            public static float buy_multiplier = 1f;
            public static float sell_multiplier = 1f;

            public static void Postfix(BlueprintSettlementBuilding bp, SettlementState __instance, ref KingdomResourcesAmount __result)
            {
                if (__instance.m_SellDiscountedBuilding == bp)
                    __result *= sell_multiplier; // re-buying a sold building should cost the same
                else
                    __result *= buy_multiplier;
            }
        }

        [HarmonyPatch(typeof(RuleCalculateAbilityParams), nameof(RuleCalculateAbilityParams.OnTrigger))]
        public class WatchCalculateParams
        {
            public static void Postfix(RulebookEventContext context, RuleCalculateAbilityParams __instance)
            {
                if (__instance.Initiator == null || !__instance.Initiator.IsPlayerFaction || !Settings.State.verbose)
                    return;

                AbilityParams para = __instance.Result;
                Main.PrintDebug($"RuleCalculateAbilityParams blueprint={__instance.Blueprint} CL={para.CasterLevel} SL={para.SpellLevel} DC={para.DC} Metamagic={para.Metamagic} Bonus={para.RankBonus}");
            }
        }

        public class TestEventBus
        {
            //RulebookEventBus.Subscribe([CanBeNull] IInitiatorRulebookSubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
            public static void Postfix(IInitiatorRulebookSubscriber subscriber, ISubscriptionProxy proxy, RulebookEventBus __instance)
            {
                if (subscriber == null)
                    return;
                var unitEntityData = (proxy?.GetSubscribingUnit()) ?? subscriber.GetSubscribingUnit();
                if (unitEntityData == null)
                    return;

                var types = InterfaceFinder<IInitiatorRulebookSubscriber>.GetSubscribedRulebookTypes(subscriber);
                var listeners = RulebookEventBus.InitiatorRulebookSubscribers.Sure(unitEntityData).m_Listeners;
                foreach (var (type, listener) in listeners)
                {
                    if (listener.GetType().GetField("List", Helper.BindingInstance)?.GetValue(listener) is List<object> list)
                    {
                        list.Sort(); //list[i] is IRulebookHandler<RuleCastSpell>                        
                    }
                }
            }
        }
    }
}
