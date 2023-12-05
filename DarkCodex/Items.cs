using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.EquipmentEnchants;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic.Commands.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using CodexLib;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Prerequisites;

namespace DarkCodex
{
    public class Items
    {
        [PatchInfo(Severity.Extend, "Durable Cold Iron Arrows", "will pick up non-magical arrows after combat", false, Requirement: typeof(Event_RestoreEndOfCombat))]
        public static void PatchArrows()
        {
            var ColdIronArrowsQuiverItem = Helper.Get<BlueprintItemEquipmentUsable>("a5a537ad28053ad48a7be1c53d7fd7ed");
            var ColdIronArrowsQuiverItem_20Charges = Helper.Get<BlueprintItemEquipmentUsable>("464ecede228b0f745a578f69a968226d");

            ColdIronArrowsQuiverItem.RestoreChargesOnRest = true;
            ColdIronArrowsQuiverItem_20Charges.RestoreChargesOnRest = true;

            ColdIronArrowsQuiverItem.AddComponents(new Event_RestoreEndOfCombat());
            ColdIronArrowsQuiverItem_20Charges.Components = ColdIronArrowsQuiverItem.Components;
        }

        [PatchInfo(Severity.Extend, "Terendelev´s Scale", "make the revive scale usable once per day", true)]
        public static void PatchTerendelevScale()
        {
            var TerendelevScaleItem = Helper.Get<BlueprintItemEquipmentUsable>("816f244523b5455a85ae06db452d4330");
            TerendelevScaleItem.RestoreChargesOnRest = true;
        }

        [PatchInfo(Severity.Create | Severity.Faulty | Severity.Hidden, "Kinetic Artifact", "new weapon for Kineticists", true)]
        public static void CreateKineticArtifact()
        {
            // see  #105 #210
            var bladetype = Helper.ToRef<BlueprintWeaponTypeReference>("b05a206f6c1133a469b2f7e30dc970ef"); //KineticBlastPhysicalBlade
            var staff = Helper.Get<BlueprintItemWeapon>("e33fd75689190094f897a57a227fa272"); //BurnedAshwoodItem
            var enchant_air = Helper.ToRef<BlueprintWeaponEnchantmentReference>("1d64abd0002b98043b199c0e3109d3ee"); //AirKineticBladeEnchantment
            var enchant_3 = Helper.ToRef<BlueprintWeaponEnchantmentReference>("80bb8a737579e35498177e1e3c75899b"); //Enhancement3
            //var air_damage = Helper.Get<BlueprintAbility>("89cc522f2e1444b40ba1757320c58530"); //AirBlastKineticBladeDamage
            //var air_burn = Helper.Get<BlueprintAbility>("77cb8c607b263194894a929c8ac59708"); //KineticBladeAirBlastBurnAbility

            var fake_blade_damage = Helper.CreateBlueprintAbility(
                "KineticCatalystDamage",
                "",
                "",
                icon: null,
                type: AbilityType.Special,
                actionType: UnitCommand.CommandType.Standard,
                range: AbilityRange.Close
                ).TargetEnemy();
                //.SetComponents(air_damage.Components)
                //.RemoveComponents<AbilityShowIfCasterHasFact>();
            fake_blade_damage.Hidden = true;

            var fake_blade_burn = Helper.CreateBlueprintAbility(
                "KineticCatalystBurn",
                "",
                "",
                icon: null,
                type: AbilityType.Special,
                actionType: UnitCommand.CommandType.Free,
                range: AbilityRange.Close
                ).TargetSelf();
                //.SetComponents(air_burn.Components);
            fake_blade_burn.Hidden = true;

            var enchant_kinetic = Helper.CreateBlueprintWeaponEnchantment(
                "KineticCatalystEnchantment",
                "Catalyst",
                "Apply enchantment bonus to kinetic blasts."
                //).SetComponents(
                //new KineticBlastEnhancement(),
                //new AddUnitFactEquipment() { m_Blueprint = fake_blade_damage.ToRef2() },
                //new AddUnitFactEquipment() { m_Blueprint = fake_blade_burn.ToRef2() }
                );

            var weapon = new BlueprintItemWeapon();
            weapon.m_Type = bladetype;
            weapon.m_Size = Size.Medium;
            weapon.m_Enchantments = [enchant_3, enchant_kinetic.ToRef(), enchant_air];
            weapon.m_OverrideDamageDice = true;
            weapon.m_VisualParameters = staff.m_VisualParameters;
            weapon.m_Icon = staff.Type.Icon;
            weapon.m_Cost = 50000;
            weapon.m_DisplayNameText = Helper.CreateString("Elemental Catalyst");
            weapon.m_DescriptionText = Helper.CreateString("This +3 staff grants its wearer its enchantment bonus on attack and damage rolls with kinetic blasts. It can be used like a simple physical kinetic blade. Wielding this weapon doesn’t prevent a kineticst from gathering power.");
            weapon.name = "KineticCatalystStaff";
            weapon.AddAsset(Helper.GetGuid(weapon.name));
            //weapon.SetComponents(new WeaponKineticBlade() { m_ActivationAbility = fake_blade_burn.ToRef(), m_Blast = fake_blade_damage.ToRef() });

            //Helper.AddArcaneVendorItem(weapon.ToReference<BlueprintItemReference>(), 1);
        }

        [PatchInfo(Severity.Create | Severity.WIP, "Butchering Axe", "new weapon type Butchering Axe", false)]
        public static void CreateButcheringAxe()
        {
            // TODO: fix new weapon category
            //Helper.EnumCreateWeaponCategory(Const.ButcheringAxe, "Butchering Axe"); 

            //AccessTools.Field(typeof(WeaponCategoryExtension), nameof(WeaponCategoryExtension.Data)).SetValue(null,
            //    Helper.Append(WeaponCategoryExtension.Data, new WeaponCategoryExtension.DataItem(
            //        Const.ButcheringAxe,
            //        WeaponSubCategory.Exotic,
            //        WeaponSubCategory.Melee,
            //        WeaponSubCategory.Metal)));

            var butchering = Helper.CreateBlueprintWeaponEnchantment(
                "ButcheringAxeEnchantment",
                "Butchering",
                "If your Strength is less than 19, you take a –2 penalty on attacks with it, as you’re unable to maneuver its daunting size and weight."
                ).SetComponents(
                new ButcheringAxeLogic());

            var tmp = Helper.CreateBlueprintWeaponType(
                "ButcheringAxeType",
                "Butchering Axe",
                "e8059a8eac62cd74f9171d748a5ae428",
                category: WeaponCategory.Greataxe, //Const.ButcheringAxe,
                damage: new DiceFormula(3, DiceType.D6),
                form: PhysicalDamageForm.Slashing,
                critMod: DamageCriticalModifierType.X3);
            tmp.m_Enchantments = butchering.ToRef().ObjToArray();
            Resource.Cache.WeaponTypeButchering.SetReference(tmp);
            var butcherType = Resource.Cache.WeaponTypeButchering;

            var standard = Helper.CreateBlueprintItemWeapon(
                "ButcheringAxeStandard",
                 weaponType: butcherType,
                price: 65);
            var plus1 = Helper.CreateBlueprintItemWeapon(
                 "ButcheringAxePlus1",
                 weaponType: butcherType,
                 price: Resource.WeaponPrice[1]).SetEnchantment("d42fc23b92c640846ac137dc26e000d4"); //Enhancement1
            var plus2 = Helper.CreateBlueprintItemWeapon(
                 "ButcheringAxePlus2",
                 weaponType: butcherType,
                 price: Resource.WeaponPrice[2]).SetEnchantment("eb2faccc4c9487d43b3575d7e77ff3f5"); //Enhancement2
            var plus3 = Helper.CreateBlueprintItemWeapon(
                 "ButcheringAxePlus3",
                 weaponType: butcherType,
                 price: Resource.WeaponPrice[3]).SetEnchantment("80bb8a737579e35498177e1e3c75899b"); //Enhancement3
            var plus4 = Helper.CreateBlueprintItemWeapon(
                 "ButcheringAxePlus4",
                 weaponType: butcherType,
                 price: Resource.WeaponPrice[4]).SetEnchantment("783d7d496da6ac44f9511011fc5f1979"); //Enhancement4
            var plus5 = Helper.CreateBlueprintItemWeapon(
                 "ButcheringAxePlus5",
                 weaponType: butcherType,
                 price: Resource.WeaponPrice[5]).SetEnchantment("bdba267e951851449af552aa9f9e3992"); //Enhancement5

            Helper.AddExoticVendorItem(standard.ToReference<BlueprintItemReference>(), 3);
            Helper.AddExoticVendorItem(plus1.ToReference<BlueprintItemReference>(), 3);
            Helper.AddExoticVendorItem(plus2.ToReference<BlueprintItemReference>(), 3);
            Helper.AddExoticVendorItem(plus3.ToReference<BlueprintItemReference>(), 3);
            Helper.AddExoticVendorItem(plus4.ToReference<BlueprintItemReference>(), 3);
            Helper.AddExoticVendorItem(plus5.ToReference<BlueprintItemReference>(), 3);

            var prof = Helper.CreateBlueprintFeature(
                "ButcheringAxeProficiency",
                "Weapon Proficiency (Butchering Axe)",
                "You become proficient with butchering axes and can use them as a weapon.",
                group: FeatureGroup.ExoticWeaponProficiency
                ).SetComponents(
                Helper.CreateAddProficiencies(Const.ButcheringAxe),
                Helper.CreatePrerequisiteNotProficient(Const.ButcheringAxe),
                Helper.CreateAddStartingEquipment(standard.ToReference<BlueprintItemReference>())
                );

            //var profselect = Helper.Get<BlueprintFeatureSelection>("9a01b6815d6c3684cb25f30b8bf20932"); //ExoticWeaponProficiencySelection
            //Helper.AppendAndReplace(ref profselect.m_AllFeatures, prof.ToRef());
            //Helper.AppendAndReplace(ref profselect.GetComponent<PrerequisiteNotProficient>().WeaponProficiencies, Const.ButcheringAxe);
        }

        [PatchInfo(Severity.Create, "Impact Enchantment", "new enchantment Impact", false)]
        public static void CreateImpactEnchantment()
        {
            var impact = Helper.CreateBlueprintWeaponEnchantment(
                "ImpactEnchantment",
                "Impact",
                "An impact weapon delivers a potent kinetic jolt when it strikes, dealing damage as if the weapon were one size category larger. In addition, any bull rush combat maneuver the wielder attempts while wielding the weapon gains a bonus equal to the weapon’s enhancement bonus.",
                prefix: "Impactful",
                enchantValue: 2
                ).SetComponents(
                new ModifyWeaponSize() { SizeCategoryChange = 1 },
                new ScalingCMBonus() { Type = CombatManeuver.BullRush }
                ).ToRef();

            var butcherType = Resource.Cache.WeaponTypeButchering;
            if (butcherType.Get() != null)
            {
                var w1 = Helper.CreateBlueprintItemWeapon(
                     "ButcheringAxePlus1Impact",
                     "",
                     "",
                     weaponType: butcherType,
                     price: Resource.WeaponPrice[3]).SetEnchantment("d42fc23b92c640846ac137dc26e000d4", impact); //Enhancement1

                var w2 = Helper.CreateBlueprintItemWeapon(
                     "ButcheringAxePlus3Impact",
                     "",
                     "",
                     weaponType: butcherType,
                     price: Resource.WeaponPrice[5]).SetEnchantment("80bb8a737579e35498177e1e3c75899b", impact); //Enhancement3

                var w3 = Helper.CreateBlueprintItemWeapon(
                     "ButcheringAxePlus5Impact",
                     "",
                     "",
                     weaponType: butcherType,
                     price: Resource.WeaponPrice[7]).SetEnchantment("bdba267e951851449af552aa9f9e3992", impact); //Enhancement5

                var w4 = Helper.CreateBlueprintItemWeapon(
                     "ButcheringAxePlus5ImpactBaneEvil",
                     "",
                     "",
                     weaponType: butcherType,
                     price: Resource.WeaponPrice[8]).SetEnchantment("bdba267e951851449af552aa9f9e3992", impact, "20ba9055c6ae1e44ca270c03feacc53b"); //Enhancement5, BaneOutsiderEvil

                Helper.AddExoticVendorItem(w1.ToReference<BlueprintItemReference>(), 3);
                Helper.AddExoticVendorItem(w2.ToReference<BlueprintItemReference>(), 2);
                Helper.AddExoticVendorItem(w3.ToReference<BlueprintItemReference>(), 2);
                Helper.AddExoticVendorItem(w4.ToReference<BlueprintItemReference>(), 2);
            }
        }
    }
}
