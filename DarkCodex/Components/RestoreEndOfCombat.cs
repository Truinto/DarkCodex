using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex.Components
{
    [AllowedOn(typeof(BlueprintItemEquipmentUsable), false)]
    [TypeId("f6b36505daa4414f93d2e3c94b1254ac")]
    public class RestoreEndOfCombat : BlueprintComponent, IPartyCombatHandler, IGlobalSubscriber, ISubscriber
    {
        public static void HandlePartyCombatEnd()
        {
            foreach (var unit in Game.Instance.Player.AllCharacters)
            {
                foreach (var item in unit.Inventory)
                {
                    if (item.Blueprint.GetComponent<RestoreEndOfCombat>() != null)
                    {
                        Helper.PrintDebug($"RestoreEndOfCombat {item.Name}");
                        item.RestoreCharges();
                    }
                }
            }
        }

        public void HandlePartyCombatStateChanged(bool inCombat)
        {
            if (inCombat)
                return;

            foreach (var unit in Game.Instance.Player.AllCharacters)
            {
                foreach (var item in unit.Inventory)
                {
                    if (item.Blueprint.GetComponent<RestoreEndOfCombat>() != null)
                    {
                        Helper.PrintDebug($"RestoreEndOfCombat {item.Name}");
                        item.RestoreCharges();
                    }
                }
            }
        }
    }
}
