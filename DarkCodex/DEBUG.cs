using HarmonyLib;
using Kingmaker;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Armies;
using Kingmaker.Armies.TacticalCombat;
using Kingmaker.Armies.TacticalCombat.Controllers;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
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
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Common;
using Kingmaker.UI.Loot;
using Kingmaker.UI.MVVM._VM.Loot;
using Kingmaker.UI.MVVM._VM.Tooltip.Templates;
using Kingmaker.UI.Tooltip;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.Utility;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TurnBased.Controllers;
using UnityEngine;
using UnityModManagerNet;

namespace DarkCodex
{
    public class DEBUG
    {
        public static void ExportAllIconTextures()
        {
            foreach (var bp in ResourcesLibrary.BlueprintsCache.m_LoadedBlueprints.Values)
            {
                if (bp.Blueprint is BlueprintUnitFact fact && fact.m_Icon != null)
                {
                    try
                    {
                        Helper.SaveSprite(fact.m_Icon);
                        Helper.PrintDebug($"Export sprite {fact.m_Icon.name} from {fact.name} ");
                    }
                    catch (Exception)
                    {
                        Helper.PrintDebug($"Didn't like sprite {fact.m_Icon.name} from {fact.name} ");
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
                Helper.PrintDebug("opening shared stash with items " + Game.Instance.Player.SharedStash.Count());

                Helper.PrintDebug("Stash: " + Game.Instance.Player.SharedStash.Select(s => s.Blueprint.name).Join());
                Helper.PrintDebug("Inventory: " + Game.Instance.Player.Inventory.Select(s => s.Blueprint.name).Join());

                try
                {
                    UnityModManager.UI.Instance.ToggleWindow();

                    if (Game.Instance.UI.LootWindowController == null)
                    {
                        Helper.PrintDebug("MainCharacter is null");
                        Game.Instance.UI.LootWindowController.Initialize();
                    }

                    if (Game.Instance == null)
                        Helper.PrintDebug("Instance is null");
                    if (Game.Instance.UI == null)
                        Helper.PrintDebug("UI is null");
                    if (Game.Instance.UI?.LootWindowController == null)
                        Helper.PrintDebug("LootWindowController is null");
                    if (Game.Instance.UI?.LootWindowController?.m_Collector == null)
                        Helper.PrintDebug("m_Collector is null");
                    if (Game.Instance.Player?.MainCharacter.Value == null)
                        Helper.PrintDebug("MainCharacter is null");
                    if (Game.Instance.Player.Party.FirstOrDefault() == null)
                        Helper.PrintDebug("Party is empty");

                    //Kingmaker.UI.Loot.LootWindowController window = Game.Instance.UI.LootWindowController;
                    //window.HandleLootInterraction(Game.Instance.Player.Party.First(), Game.Instance.Player.SharedStash, "Remote Stash");


                }
                catch (Exception e)
                {
                    Helper.Print(e.ToString());
                }

                //window.WindowMode = LootWindowMode.PlayerChest;
                //window.m_Collector.SetData(Game.Instance.Player.SharedStash, "Remote Stash");
                //window.Show(true);

            }
        }

        [HarmonyPatch(typeof(Polymorph), nameof(Polymorph.OnActivate))]
        public class PolymorphTest1
        {
            public static void Prefix(Polymorph __instance)
            {
                __instance.m_KeepSlots = __instance.m_KeepSlots || Settings.StateManager.State.polymorphKeepInventory;
            }
        }

        [HarmonyPatch(typeof(Polymorph), nameof(Polymorph.TryReplaceView))]
        public class PolymorphTest2
        {
            public static bool Prefix(Polymorph __instance)
            {
                return !Settings.StateManager.State.polymorphKeepModel;
            }
        }

        [HarmonyPatch(typeof(UIUtilityItem), nameof(UIUtilityItem.FillEnchantmentDescription), new Type[] { typeof(ItemEntity), typeof(ItemTooltipData) })]
        public class Enchantments
        {
            public static void Postfix(ItemEntity item, ItemTooltipData itemTooltipData, ref string __result)
            {
                if (!item.IsIdentified)
                    return;

                if (item is ItemEntityWeapon || item is ItemEntityArmor || item is ItemEntityShield || item is ItemEntityUsable)
                    return;

                // append enchantment qualities for items that don't usually display them
                itemTooltipData.Texts.TryGetValue(TooltipElement.Qualities, out string text);
                StringBuilder sb = new StringBuilder(text);
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

                foreach (ItemEnchantment itemEnchantment in item.VisibleEnchantments)
                {
                    if (!string.IsNullOrEmpty(itemEnchantment.Blueprint.Description))
                    {
                        __result += string.Format("<b><align=\"center\">{0}</align></b>\n{1}\n\n", itemEnchantment.Blueprint.Name, itemEnchantment.Blueprint.Description);
                    }
                }
            }

            public static void NameAll()
            {
                StringBuilder sb = new StringBuilder();
                foreach (var bp in ResourcesLibrary.BlueprintsCache.m_LoadedBlueprints.Values)
                {
                    var enchantment = bp.Blueprint as BlueprintItemEnchantment;
                    if (enchantment?.m_EnchantName == null || enchantment.m_EnchantName.m_Key != "")
                        continue;

                    string name = enchantment.name;
                    if (name == null) continue;
                    name = name.Replace("Enchantment", "");
                    name = name.Replace("Enchant", "");
                    if (name.Length < 1) continue;
                    sb.Clear();
                    sb.Append(name[0]);
                    for (int i = 1; i < name.Length; i++)
                    {
                        if (name[i] <= 90)
                            sb.Append(' ');
                        if (name[i] <= 57)
                            sb.Append('+');
                        sb.Append(name[i]);
                    }

                    enchantment.m_EnchantName = sb.ToString().CreateString();
                }
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
    }
}
