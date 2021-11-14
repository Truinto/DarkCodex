using DarkCodex.Components;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.EquipmentEnchants;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic.Commands.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    public class Items
    {
        [PatchInfo(Severity.Extend, "Durable Cold Iron Arrows", "will pick up non-magical arrows after combat", false)]
        public static void patchArrows()
        {
            var ColdIronArrowsQuiverItem = ResourcesLibrary.TryGetBlueprint<BlueprintItemEquipmentUsable>("a5a537ad28053ad48a7be1c53d7fd7ed");
            var ColdIronArrowsQuiverItem_20Charges = ResourcesLibrary.TryGetBlueprint<BlueprintItemEquipmentUsable>("464ecede228b0f745a578f69a968226d");

            ColdIronArrowsQuiverItem.RestoreChargesOnRest = true;
            ColdIronArrowsQuiverItem_20Charges.RestoreChargesOnRest = true;

            ColdIronArrowsQuiverItem.AddComponents(new RestoreEndOfCombat());
            ColdIronArrowsQuiverItem_20Charges.ComponentsArray = ColdIronArrowsQuiverItem.ComponentsArray;
        }

        [PatchInfo(Severity.Extend, "Terendelev´s Scale", "make the revive scale usable once per day", true)]
        public static void patchTerendelevScale()
        {
            var TerendelevScaleItem = ResourcesLibrary.TryGetBlueprint<BlueprintItemEquipmentUsable>("816f244523b5455a85ae06db452d4330");
            TerendelevScaleItem.RestoreChargesOnRest = true;
        }

        [PatchInfo(Severity.Create, "Kinetic Artifact", "new weapon for Kineticists", true)]
        public static void createKineticArtifact()
        {
            var bladetype = Helper.ToRef<BlueprintWeaponTypeReference>("b05a206f6c1133a469b2f7e30dc970ef"); //KineticBlastPhysicalBlade
            var staff = ResourcesLibrary.TryGetBlueprint<BlueprintItemWeapon>("e33fd75689190094f897a57a227fa272"); //BurnedAshwoodItem
            var enchant_air = Helper.ToRef<BlueprintWeaponEnchantmentReference>("1d64abd0002b98043b199c0e3109d3ee"); //AirKineticBladeEnchantment
            var enchant_3 = Helper.ToRef<BlueprintWeaponEnchantmentReference>("80bb8a737579e35498177e1e3c75899b"); //Enhancement3
            var air_damage = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("89cc522f2e1444b40ba1757320c58530"); //AirBlastKineticBladeDamage
            var air_burn = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("77cb8c607b263194894a929c8ac59708"); //KineticBladeAirBlastBurnAbility

            var fake_blade_damage = Helper.CreateBlueprintAbility(
                "KineticCatalystDamage",
                "",
                "",
                null, null,
                AbilityType.Special,
                UnitCommand.CommandType.Standard,
                AbilityRange.Close
                ).TargetEnemy()
                .SetComponents(air_damage.ComponentsArray)
                .RemoveComponents(default(AbilityShowIfCasterHasFact));
            fake_blade_damage.Hidden = true;

            var fake_blade_burn = Helper.CreateBlueprintAbility(
                "KineticCatalystBurn",
                "",
                "",
                null, null,
                AbilityType.Special,
                UnitCommand.CommandType.Free,
                AbilityRange.Close
                ).TargetSelf()
                .SetComponents(air_burn.ComponentsArray);
            fake_blade_burn.Hidden = true;

            var enchant_kinetic = Helper.CreateBlueprintWeaponEnchantment(
                "KineticCatalystEnchantment",
                "Catalyst",
                "Apply enchantment bonus to kinetic blasts."
                ).SetComponents(
                new KineticBlastEnhancement(),
                new AddUnitFactEquipment() { m_Blueprint = fake_blade_damage.ToRef2() },
                new AddUnitFactEquipment() { m_Blueprint = fake_blade_burn.ToRef2() }
                );

            var weapon = new BlueprintItemWeapon();
            weapon.m_Type = bladetype;
            weapon.m_Size = Size.Medium;
            weapon.m_Enchantments = new BlueprintWeaponEnchantmentReference[] { enchant_3, enchant_kinetic.ToRef(), enchant_air };
            weapon.m_OverrideDamageDice = true;
            weapon.m_VisualParameters = staff.m_VisualParameters;
            weapon.m_Icon = staff.Type.Icon;
            weapon.m_Cost = 50000;
            weapon.m_DisplayNameText = Helper.CreateString("Elemental Catalyst");
            weapon.m_DescriptionText = Helper.CreateString("This +3 staff grants its wearer its enchantment bonus on attack and damage rolls with kinetic blasts. It can be used like a simple physical kinetic blade. Wielding this weapon doesn’t prevent a kineticst from gathering power.");
            weapon.name = "KineticCatalystStaff";
            weapon.AddAsset(GuidManager.i.Get(weapon.name));
            weapon.SetComponents(new WeaponKineticBlade() { m_ActivationAbility = fake_blade_burn.ToRef(), m_Blast = fake_blade_damage.ToRef() });

            Helper.AddArcaneVendorItem(weapon.ToReference<BlueprintItemReference>(), 1);
        }
    }
}
