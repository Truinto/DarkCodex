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
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.Kingdom;
using Kingmaker.Kingdom.Settlements;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Loot;
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
        [HarmonyPatch(typeof(RuleAttackRoll), nameof(RuleAttackRoll.OnTrigger))]
        public static class Patch_RuleRollD20 // todo remove code
        {
            // patch RuleAttackRoll.OnTrigger
            // transpile replace RulebookEvent.Dice.D20 with custom method

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
            {
                List<CodeInstruction> list = instr.ToList();
                MethodInfo original = AccessTools.PropertyGetter(typeof(RulebookEvent.Dice), nameof(RulebookEvent.Dice.D20));
                MethodInfo replacement = AccessTools.Method(typeof(Patch_RuleRollD20), nameof(D20));

                for (int i = 0; i < list.Count; i++)
                {
                    var mi = list[i].operand as MethodInfo;
                    if (mi != null && mi == original)
                    {
                        Helper.PrintDebug("Patch_RuleRollD20 at " + i);
                        list[i].operand = replacement;
                    }
                }

                return list;
            }

            public static RuleRollD20 D20()
            {
                RuleRollD20 ruleRollD = new RuleRollD20(Rulebook.CurrentContext.CurrentEvent?.Initiator);
                ruleRollD.PreRollDice();
                Rulebook.Trigger(ruleRollD);
                return ruleRollD;
            }

            public static List<int> FakeRolls = new List<int>();
        }

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

                UnityModManager.UI.Instance.ToggleWindow();

                Kingmaker.UI.Loot.LootWindowController window = Game.Instance.UI.LootWindowController;
                //window.HandleLootInterraction(Game.Instance.Player.MainCharacter.Value, Game.Instance.Player.SharedStash, "Remote Stash");
                window.WindowMode = LootWindowMode.PlayerChest;
                window.m_Collector.SetData(Game.Instance.Player.SharedStash, "Remote Stash");
                window.Show(true);

            }
        }

        // #364 -> toggle UI open/close, if open show all judgments and put in list in order they are enabled, print numbers to show order

        public class Enchantments
        {
            public static void X()
            {
                //UIUtilityItem.FillEnchantmentDescription
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
