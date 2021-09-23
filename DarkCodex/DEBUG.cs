using HarmonyLib;
using Kingmaker;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.Kingdom;
using Kingmaker.RuleSystem.Rules;
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

namespace DarkCodex
{
    public class DEBUG
    {
        public class Date //#278
        {
            public static void SetDate()
            {
                KingdomState.Instance.CurrentDay -= 1;
                Helper.Print(Game.Instance.BlueprintRoot.Calendar.GetDateText(KingdomState.Instance.Date - Game.Instance.BlueprintRoot.Calendar.GetStartDate(), GameDateFormat.Full, true));
            }
        }

        [HarmonyPatch(typeof(CombatController), nameof(CombatController.HandlePartyCombatStateChanged))]
        public class Rage //#262
        {
            private static BlueprintGuid rage = BlueprintGuid.Parse("df6a2cce8e3a9bd4592fb1968b83f730");
            private static bool setting = true;
            // Auto Enable Rage On Combat Start

            public static void Postfix(bool inCombat)
            {
                if (!setting) return;
                if (!inCombat) return;

                foreach (var unit in Game.Instance.Player.Party)
                {
                    foreach (var activatable in unit.ActivatableAbilities)
                    {
                        if (activatable.Blueprint.AssetGuid == rage)
                        {
                            activatable.TryStart();
                            break;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ItemEntity), nameof(ItemEntity.IsUsableFromInventory), MethodType.Getter)]
        public class ItemEntity_IsUsableFromInventory_Patch
        {
            // Allow Item Use From Inventory During Combat
            public static bool Prefix(ItemEntity __instance, ref bool __result)
            {
                //if (false) return true;

                BlueprintItemEquipment item = __instance.Blueprint as BlueprintItemEquipment;
                __result = item?.Ability != null;
                return false;
            }
        }

        [HarmonyPatch(typeof(MassLootHelper), nameof(MassLootHelper.GetMassLootFromCurrentArea))]
        public class PatchLootEverythingOnLeave
        {
            public static bool Prefix(ref IEnumerable<LootWrapper> __result)
            {
                //return true;

                var all_units = Game.Instance.State.Units.All.Where(w => w.IsInGame);
                var result_units = all_units.Where(unit => unit.HasLoot).Select(unit => new LootWrapper { Unit = unit }); //unit.IsRevealed && unit.IsDeadAndHasLoot

                var all_entities = Game.Instance.State.Entities.All.Where(w => w.IsInGame);
                var all_chests = all_entities.Select(s => s.Get<InteractionLootPart>()).Where(i => i?.Loot != Game.Instance.Player.SharedStash).NotNull();

                List<InteractionLootPart> tmp = TempList.Get<InteractionLootPart>();

                foreach (InteractionLootPart i in all_chests)
                {
                    //if (i.Owner.IsRevealed
                    //    && i.Loot.HasLoot
                    //    && (i.LootViewed
                    //        || (i.View is DroppedLoot && !i.Owner.Get<DroppedLoot.EntityPartBreathOfMoney>())
                    //        || i.View.GetComponent<SkinnedMeshRenderer>()))
                    if (i.Loot.HasLoot)
                    {
                        tmp.Add(i);
                    }
                }

                var result_chests = tmp.Distinct(new MassLootHelper.LootDuplicateCheck()).Select(i => new LootWrapper { InteractionLoot = i });

                __result = result_units.Concat(result_chests);

#if false   // TODO - check if this solves the invisible items in the loot all UI
                foreach (var loot in __result)
                {
                    if (loot.Unit != null)
                        loot.Unit.LootViewed = true;
                    if (loot.InteractionLoot != null)
                        loot.InteractionLoot.IsViewed = true;
                }
#endif

                return false;
            }
        }
    }
}
