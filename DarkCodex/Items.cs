using DarkCodex.Components;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    public class Items
    {
        public static void patchArrows()
        {
            var ColdIronArrowsQuiverItem = ResourcesLibrary.TryGetBlueprint<BlueprintItemEquipmentUsable>("a5a537ad28053ad48a7be1c53d7fd7ed");
            var ColdIronArrowsQuiverItem_20Charges = ResourcesLibrary.TryGetBlueprint<BlueprintItemEquipmentUsable>("464ecede228b0f745a578f69a968226d");

            ColdIronArrowsQuiverItem.RestoreChargesOnRest = true;
            ColdIronArrowsQuiverItem_20Charges.RestoreChargesOnRest = true;

            ColdIronArrowsQuiverItem.AddComponents(new RestoreEndOfCombat());
            ColdIronArrowsQuiverItem_20Charges.ComponentsArray = ColdIronArrowsQuiverItem.ComponentsArray;
        }

        public static void patchTerendelevScale()
        {
            var TerendelevScaleItem = ResourcesLibrary.TryGetBlueprint<BlueprintItemEquipmentUsable>("816f244523b5455a85ae06db452d4330");
            TerendelevScaleItem.RestoreChargesOnRest = true;
        }
    }
}
