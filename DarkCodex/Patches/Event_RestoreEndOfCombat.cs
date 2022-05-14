using CodexLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    [AllowedOn(typeof(BlueprintItemEquipmentUsable), false)]
    [PatchInfo(Severity.Event, "Event: Restore End Of Combat", "enables logic used by patchArrows")]
    public class Event_RestoreEndOfCombat : BlueprintComponent, IPartyCombatHandler, IGlobalSubscriber, ISubscriber
    {
        public void HandlePartyCombatStateChanged(bool inCombat)
        {
            if (inCombat)
                return;

            foreach (var item in Game.Instance.Player.Inventory)
            {
                if (item.Blueprint.GetComponent<Event_RestoreEndOfCombat>() != null)
                {
                    Main.PrintDebug($"RestoreEndOfCombat {item.Name}");
                    item.RestoreCharges();
                }
            }
        }
    }
}
