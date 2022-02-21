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
    public class RestoreEndOfCombat : BlueprintComponent, IPartyCombatHandler, IGlobalSubscriber, ISubscriber
    {
        public void HandlePartyCombatStateChanged(bool inCombat)
        {
            if (inCombat)
                return;

            foreach (var item in Game.Instance.Player.Inventory)
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
