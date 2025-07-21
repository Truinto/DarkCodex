﻿using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.Modding;
using Kingmaker.UI.MVVM._VM.Tooltip.Templates;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Shared;
using CodexLib;
using Kingmaker.Utility;
using Kingmaker.Enums;
using System.Security.Policy;

namespace DarkCodex
{
    public class Resource
    {
        // clear before use
        public static StringBuilder sb = new();

        public static readonly int[] WeaponPrice = [
                100,
                2000,
                8000,
                18000,
                32000,
                50000,
                72000,
                98000,
                128000,
                162000,
                200000,
            ];

        public static ContextValue ValueRank = Helper.CreateContextValue();

        public class Cache
        {
            // Base
            public static readonly BlueprintItemWeaponReference WeaponUnarmed = Helper.ToRef<BlueprintItemWeaponReference>("f60c5a820b69fb243a4cce5d1d07d06e"); //Unarmed1d6
            public static readonly BlueprintItemWeaponReference WeaponTouch = Helper.ToRef<BlueprintItemWeaponReference>("bb337517547de1a4189518d404ec49d4"); //TouchItem
            public static readonly BlueprintItemWeaponReference WeaponRay = Helper.ToRef<BlueprintItemWeaponReference>("f6ef95b1f7bb52b408a5b345a330ffe8"); //RayItem
            public static readonly BlueprintItemWeaponReference WeaponBlastPhysical = Helper.ToRef<BlueprintItemWeaponReference>("65951e1195848844b8ab8f46d942f6e8"); //KineticBlastPhysicalWeapon
            public static readonly BlueprintItemWeaponReference WeaponBlastEnergy = Helper.ToRef<BlueprintItemWeaponReference>("4d3265a5b9302ee4cab9c07adddb253f"); //KineticBlastEnergyWeapon

            public static readonly BlueprintWeaponTypeReference WeaponTypeUnarmed = Helper.ToRef<BlueprintWeaponTypeReference>("fcca8e6b85d19b14786ba1ab553e23ad"); //Unarmed

            // Sprites
            public static readonly Texture IconPotBlack = Helper.CreateTexture(Path.Combine(Main.ModPath, "icons", "pot black.png"), 16, 16);
            public static readonly Texture IconBookBlack = Helper.CreateTexture(Path.Combine(Main.ModPath, "icons", "book black.png"), 16, 16);

            // Mods
            public static readonly BlueprintUnitPropertyReference PropertySneakAttackDice = new();
            public static readonly BlueprintUnitPropertyReference PropertyMaxMentalAttribute = new();
            public static readonly BlueprintUnitPropertyReference PropertyMythicDispel = new();
            public static readonly BlueprintFeatureReference FeatureFeralCombat = new();
            public static readonly BlueprintFeatureReference FeatureResourcefulCaster = new();
            public static readonly BlueprintFeatureReference FeatureMagicItemAdept = new();
            public static readonly BlueprintFeatureReference FeatureMindShield = new();
            public static readonly BlueprintWeaponTypeReference WeaponTypeButchering = new();

            // Cross Feature
            public static List<BlueprintAbility> AccursedStrike = [];

            public static void Save<T>(List<T> list, string path) where T : SimpleBlueprint
            {
                try
                {
                    if (File.Exists(path))
                        File.Delete(path);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    using var writer = new BinaryWriter(new FileStream(path, FileMode.CreateNew, FileAccess.Write));
                    foreach (var bp in list)
                        writer.Write(bp.AssetGuid.ToByteArray());
                }
                catch (Exception e)
                {
                    Main.PrintException(e);
                }
            }

            public static void Load<T>(List<T> list, string path) where T : SimpleBlueprint
            {
                var bpcache = ResourcesLibrary.BlueprintsCache;
                var buffer = new byte[16];

                try
                {
                    using var reader = new FileStream(path, FileMode.Open, FileAccess.Read);
                    while (reader.Read(buffer, 0, 16) == 16)
                    {
                        if (bpcache.Load(new BlueprintGuid(buffer)) is T bp)
                        {
                            if (!list.Contains(bp))
                                list.Add(bp);
                            else
                                Main.PrintDebug("duplicate blueprint: " + bp.AssetGuid);
                        }
                        else
                            Main.Print($"wrong guid in {path}");
                    }

                }
                catch (Exception e)
                {
                    Main.PrintException(e);
                }
                // Helper.Print($"loaded {list.Count} {typeof(T).Name}");
            }

            //public static void Dispose()
            //{
            //    foreach (var field in typeof(Cache).GetFields(BindingFlags.Public | BindingFlags.Static))
            //    {
            //        if (!field.FieldType.IsValueType)
            //            field.SetValue(null, null);
            //    }
            //}
        }

        public class Strings
        {
            public static LocalizedString Empty = new() { Key = "" };


            public static LocalizedString RoundPerLevel = new() { Key = "6250ccf0-1ed0-460f-8ce7-094c2da7e198" };
            public static LocalizedString RoundPerLevelRepeatSave = new() { Key = "d7cb2411-bd6e-4b59-a72d-43ea018de978" };
            public static LocalizedString RoundPerLevelPlus1 = new() { Key = "fb690795-9d4e-423b-a81f-44f6e43a17b5" };
            public static LocalizedString MinutesPerLevel = new() { Key = "00b2e4c2-aafe-487b-b890-d57473373da7" };
            public static LocalizedString TenMinutesPerLevel = new() { Key = "9178f928-579f-43d0-bd80-e67e478c7bfc" };
            public static LocalizedString HoursPerLevel = new() { Key = "cca23e25-a5b6-4138-b2bf-0b6387738a22" };

            public static LocalizedString OneRound = new() { Key = "62761fbe-42ce-4474-ae10-14f9024f4c57" };
            public static LocalizedString ThreeRounds = new() { Key = "c106ea15-f587-44ce-9472-fa7cd4c11dab" };
            public static LocalizedString OneMinute = new() { Key = "70e2c2f0-b2c6-423a-b6ec-c05084530366" };
            public static LocalizedString TenMinutes = new() { Key = "96bcc776-c41d-4467-99f7-d5848641ca11" };
            public static LocalizedString OneHour = new() { Key = "9e29a1ac-9b6c-42e1-aa32-08b51962127f" };
            public static LocalizedString OneDay = new() { Key = "b2581d37-9b43-4473-a755-f675929feaa2" };
            public static LocalizedString Permanent = new() { Key = "0b5bb39b-9e2e-4841-9f1c-5c20c306553b" };
            public static LocalizedString Instantaneous = new() { Key = "3d7fbfb9-10e8-4509-bab6-a8e4d3dbc3c8" };


            public static LocalizedString FortitudeHalf = new() { Key = "fc1ffd3d-d343-4dfe-8441-118b33c8026a" };
            public static LocalizedString FortitudePartial = new() { Key = "af1a01bb-3924-4663-94e8-79e080287aaa" };
            public static LocalizedString FortitudeNegates = new() { Key = "c8ec9dfb-37ba-485d-8c08-c45a6bfc88f3" };
            public static LocalizedString FortitudeNegatesRound = new() { Key = "23355cb5-0024-4da1-a542-0ef4a672ed8e" };

            public static LocalizedString WillHalf = new() { Key = "d47299a3-2f17-4e60-8199-65545e148a89" };
            public static LocalizedString WillPartial = new() { Key = "03e30000-0921-4296-a8b4-9566a9777a5d" };
            public static LocalizedString WillNegates = new() { Key = "7ac9f1bb-ab14-4d64-8543-4c97a64a71bd" };
            public static LocalizedString WillPartialRound = new() { Key = "1035efbd-2846-480a-ae8a-4593fe3c63d8" };
            public static LocalizedString WillNegatesRound = new() { Key = "50f1639f-a789-4939-bab6-557375828c4d" };

            public static LocalizedString ReflexHalf = new() { Key = "dccd7029-0a51-4e5b-9cb2-7a2969b61516" };
            public static LocalizedString ReflexPartial = new() { Key = "d58f2d26-2317-4023-921a-76d0c1590bcf" };
            public static LocalizedString ReflexNegates = new() { Key = "c649bb57-1a11-4d76-ae8c-8caa59feb39b" };
        }

        public class Projectile
        {
            public const string AcidArrow00 = "89cd363b66b1df440b5281f7d3ef188d";
            public const string AcidArrow00_Up = "46e345b7b0ff0354b8a421f3c94f91d8";
            public const string AcidCommonProjectile00 = "d8abd128c02331a45a4f250a62722e8b";
            public const string AcidCommonProjectile00_Up = "cea4731c45545fa46b94a3880f3ac8f2";
            public const string AcidCone15Feet00 = "f6544caac8fe528489327cd86a84b025";
            public const string AcidCone30Feet00 = "155104dfdc285f3449610e625fa85729";
            public const string AcidCone30Feet00Breath = "cf6b3f4577782be43bd7f22f288388b2";
            public const string AcidCone50Feet00 = "214036a0c1b35464780ad140324c249c";
            public const string AcidLine00 = "33af0c7694f8d734397bd03e6d4b72f1";
            public const string AcidLine00Breath = "216f05939a74d634d8ec7d88f836c5c5";
            public const string AirCone15Feet00 = "524ed346b4014c349914810643e5035f";
            public const string AirCone30Feet00 = "5d395121ef0195c4580fef5363f095ff";
            public const string AlchemistAcidBomb00 = "b33865d0fbc186946a485fbd549f74ec";
            public const string AlchemistAcidBomb00_Up = "6eb8d09e0515bf6429142fdcd90966ba";
            public const string AlchemistColdBomb00 = "1da136f2985c8e645ac83deaebb1eb63";
            public const string AlchemistColdBomb00_Up = "dac0d98be6aba3c44a740ac9fe8d57d2";
            public const string AlchemistDispellingBomb00 = "e67b65932ae3e3847b7f1fd082dd214f";
            public const string AlchemistDispellingBomb00_Up = "b49d82ee1f11bcc49858e305ab15fbd4";
            public const string AlchemistElectroBomb00 = "6a0217210bcac6f408f48660915a4586";
            public const string AlchemistElectroBomb00_Up = "de7f98d998a181e4fa33bb468892dcbe";
            public const string AlchemistExplosiveBomb00 = "78fa47590c24f0b47acdec286e171e81";
            public const string AlchemistExplosiveBomb00_Up = "893d19e7997849c4ca2618dba1de9f87";
            public const string AlchemistFireBomb00 = "57dc3bc9fc07dd242a7283e379ae4284";
            public const string AlchemistFireBomb00_Up = "7be20a77d91b26c409e71c9903653bf0";
            public const string AlchemistForceBomb00 = "0bd5b3eba4ee9784f92276e93cc3cd35";
            public const string AlchemistForceBomb00_Up = "9287ae715f47b0c4089f4e37540ccca2";
            public const string AlchemistHolyBomb00 = "bb5faa86a8f3d7841ad7c94fd7d26da7";
            public const string AlchemistHolyBomb00_Up = "3aa4ed82c160a6e4488f3e6527e03c04";
            public const string AlchemistPoisonBomb00 = "b24342766c28d614a92def47a6b49fbe";
            public const string AlchemistPoisonBomb00_Up = "175dd3690d075994fa3f354f7591a6a2";
            public const string Arrow = "67fb7c23bb7d6d346b8515f2927516e0";
            public const string Arrow_2 = "d0252c90549d29340b22da64bd748273";
            public const string ArrowCone30Feet00 = "c07ceec4ab4220a4b99ecd94b6c13d2c";
            public const string ArrowCrossBow00 = "0f083f2598b3e6441992ebadbc0325aa";
            public const string ArrowCrossBow00_2 = "ea94fba085a387b48a7dba113ec6747b";
            public const string ArrowFire00 = "cd6fbf24b5f625245960c4b8e6f58292";
            public const string ArrowFire00_Up = "619febd1c5ab4484f8dd561c3cda571e";
            public const string AshGiantCatapult01 = "f153af21163157a4189af3b8856b79e0";
            public const string AshGiantCatapult02 = "e12e035bfc97bd048b71ead0ed5d9842";
            public const string BatteringBlast00 = "7b744bce71ea65f4ab43df2090fe59f2";
            public const string BatteringBlast00_Up = "6810bc67dbc33a34e9c7188fc16866a5";
            public const string BloomPortal00_CasterAppear = "46d246105f3ef0e468948951e10ffacf";
            public const string BloomPortal00_CasterDisappear = "dda994696cae79e458e75f3825ddb4fb";
            public const string BloomPortal00_SideAppear = "7f116d774ea8db74a8c2759976c71371";
            public const string BloomPortal00_SideDisappear = "41ef6304b00020049b583d3dcb05c9a7";
            public const string BuffRay00_Abjuration = "376315252774fc340bb586be85d3fe62";
            public const string BuffRay00_Abjuration_Up = "f2176ad4bc9459b43acd55f16fcc30ad";
            public const string BuffRay00_Conjuration = "47b49bc2cb18c0b4981ea6de72449001";
            public const string BuffRay00_Conjuration_Up = "1d6900b6bfda526448fa53ccb6e8e205";
            public const string BuffRay00_Divination = "536b7c74ba542a24a8ae82ac7cfc9e3b";
            public const string BuffRay00_Divination_Up = "9a0bada82127996499af2bc4091f45f6";
            public const string BuffRay00_Enchantment = "4714d0d0921111c4eabd0fe0c74f811f";
            public const string BuffRay00_Enchantment_Up = "6c3211173b35a7b449b7ce43cc29fb9b";
            public const string BuffRay00_Evocation = "6ca42684aff5cb24596b05d26e643fc7";
            public const string BuffRay00_Evocation_Up = "491dfedf7cd711045b8f2ce479043a36";
            public const string BuffRay00_Illusion = "8ce52acc16e4d5949b5345241935d34f";
            public const string BuffRay00_Illusion_Up = "11c7de2b40b269149a36035f3b87967f";
            public const string BuffRay00_Necromancy = "990fc1276d6ae6745b88785544b7b55e";
            public const string BuffRay00_Necromancy_Up = "939845aaa1d849945a530816b470a843";
            public const string BuffRay00_Transmutation = "dad37bb3fb7035d44922c15e4fed58ae";
            public const string BuffRay00_Transmutation_Up = "c539c9cd98b5d7c48ba660276dfa824e";
            public const string BurningArc00 = "0549a3702fff3994b8d14295ee3f4e3b";
            public const string BurningArc01 = "9ee4b4229966cc14ba58c189c387e2c6";
            public const string ChannelEnergyCone30Feet00 = "7363081f6144d604da3645a6ea94fcb1";
            public const string ChannelNegativeEnergyCone30Feet00 = "f8f8f0402c24a584bb076a2f222d4270";
            public const string ClashingRocks00 = "34285fa3830030c4ab9c969151de4374";
            public const string ClawDevil_projectile = "127be665d87f8634a85af05d394ee14f";
            public const string ColdCommonProjectile00 = "e82d266e0c068ab418a163fc41c40731";
            public const string ColdCommonProjectile00_Up = "067151df1fa3936498f25dc9ab5642b6";
            public const string ColdCone15Feet00 = "5af8b717a209fd444a1e4d077ed776f0";
            public const string ColdCone30Feet00 = "c202b61bf074a7442bf335b27721853f";
            public const string ColdCone30Feet00Breath = "72b45860bdfb81f4284aa005c04594dd";
            public const string ColdCone50Feet00 = "79a66a3766ae87146beb6000a73e8213";
            public const string ColdLine00 = "df0464dbf5b83804d9980eb42ed37462";
            public const string ColdLine00Breath = "fe327abf15980eb458ced542260794e2";
            public const string ColorSpray15Feet00 = "10d887991ea50ce428140411284f875e";
            public const string ColorSpray30Feet00 = "a03e8c31fe8b5534ebb8368933e6f4b7";
            public const string CreatureRovagug00_Projectile = "e1c6fab6d26df194aa08eb0123826d49";
            public const string CreatureRovagug00_Projectile_Up = "2dcf23dfa354a454bb26c236fd3a2304";
            public const string Dart_projectile = "533995c13cde02143b8c2f6b5b98598e";
            public const string DeathCurse00_Projectile = "e4072ffb43b821848a5daedfba6c6f9e";
            public const string DevilClawLeft00 = "11a1193626d322b49b3cf5578384142b";
            public const string DevilClawRight00 = "f76e194520d6f9946bb48d8852ce9e8c";
            public const string DimensionDoor00_CasterAppear = "4125f30c999bddc4492bf91d73c4cf64";
            public const string DimensionDoor00_CasterDisappear = "b9a3f1855ab08bf42a8b119818bcc6dd";
            public const string DimensionDoor00_SideAppear = "6c72207ef86803543b4b13352bcc5cf6";
            public const string DimensionDoor00_SideDisappear = "cdaff4fd8665656409ddffe42fbc07c1";
            public const string DimensionDoorHell00_CasterAppear = "19fac364f3b464045988d4c6579aa920";
            public const string DimensionDoorHell00_CasterDisappear = "22c4a82ce133a1c4da181fd74d883714";
            public const string DimensionDoorHell00_SideAppear = "e48d70215fe557e46a29cddde57720de";
            public const string DimensionDoorHell00_SideDisappear = "a32eed54acdcb3643bfc50784c8381c2";
            public const string Disintegrate00 = "467caea41e0c2144b9e78c31f49bb288";
            public const string Disintegrate00_Up = "532e1b1ecfb599642a7d6f27de962fb4";
            public const string DisruptUndead00 = "d543d55f7fdb60340af40ea8fc5e686d";
            public const string DisruptUndead00_Up = "fc7468fcca9122544b14eb976746982d";
            public const string Dummy00_Projectile = "b8e4b2d648683fb43b8a60d2bf36d2b2";
            public const string ElectroBallProjectile00 = "2117f0eb12152c7408b1a26d4965d72a";
            public const string ElectroBallProjectile00_Up = "b804391dbc792ef4ab42dd7c1723e6c9";
            public const string ElectroCommonProjectile00 = "1af8385214ca8774b98922b56caa0e92";
            public const string ElectroCommonProjectile00_Up = "eb69fcd56b1c4324db572070838639d0";
            public const string EnchantmentCone30Feet00 = "8c3c664b2bd74654e82fc70d3457e10d";
            public const string EnchantmentCone50Feet00 = "dfaee4096d707fb4eb74512142899d7f";
            public const string Enervation00 = "72aa6191e153a31468d76668cbc72fc7";
            public const string Enervation00_Old = "fcf4165ae22724e45859d108b1c81bba";
            public const string Enervation00_Old_Up = "21fd1d30b119d1d46a9e4e0c5ead600d";
            public const string Enervation00_Up = "6b56163ba714a824b9a9232c5130214b";
            public const string Fear00 = "448cbf940edfe5e4ab940e8de87329eb";
            public const string Fireball00 = "8927afa172e0fc54484a29fa0c4c40c4";
            public const string Fireball00_Up = "55eff201f42d29645b8652e9e703c950";
            public const string FireCommonProjectile00 = "30a5f408ea9d163418c86a7107fc4326";
            public const string FireCommonProjectile00_Up = "8d2f77cca8777fc4f8912fded75fcc03";
            public const string FireCone15Feet00 = "6dfc5e4c7d9ae3048984744222dbd0fa";
            public const string FireCone30Feet00 = "acf144d4da2638e4eadde1bb9dac29b4";
            public const string FireCone30Feet00Breath = "52c3a84f628ddde4dbfb38e4a581b01a";
            public const string FireCone50Feet00 = "4c272644b29989a40bcf1e6003cfe708";
            public const string FireElementalGem_FireBombStone00_Projectile = "fc8e3bf297aa9a145a7c21baf81a1ba5";
            public const string FireElementalGem_FireBombStone00_Projectile_Up = "7bd23fea023a44c49ba7b4669fb0e880";
            public const string FireLine00 = "ecf79fc871f15074e95698a3fef47aee";
            public const string FireLine00Breath = "fb88746261028a1468e60b9bbfe00a35";
            public const string FireLine00_Head = "7172842b720c3534897ebda2e0624c2d";
            public const string FlashStep00_CasterAppear = "ba5f002f77d6bb34d9a7171ce037ae2a";
            public const string FlashStep00_CasterDisappear = "1d09622a26483334c82c7887e7691dac";
            public const string FlashStep00_SideAppear = "b7d750a8975320246940ce92083ef566";
            public const string FlashStep00_SideDisappear = "6fa16670751701c4ca4b024341ab65e2";
            public const string ForceMissile00 = "c097ae0ab0864af44bdc228633612243";
            public const string ForceMissile00_Up = "5c89d1737bdc74a419ca50cbee13da5b";
            public const string GenericRay00 = "872f3269ebffe334fbc977b5a34b2de3";
            public const string GenericRay00_Up = "b00974f8738a8c948a8be5032e8fd1a4";
            public const string GiantSlugAcid00 = "086fb81945498de4f92731f51b4b245f";
            public const string GiantSlugAcid00_Up = "1f3ad3a1d8ea404438e185faaa0a8e72";
            public const string HandOfTheApprenticeProjectile = "c8559cabbf082234e80ad8e046bfa1a1";
            public const string HellfireRay00 = "64abeb7fa600aa94ab2ecd63daf687d9";
            public const string HellfireRay00_Up = "0397b120246d6124eb773412d519e6b8";
            public const string HitSnapBugProjectile = "bded82753bfc1a54ca59e5f61a6bd4d0";
            public const string Javelin_projectile = "ba6d2fdae677e664ca76c8837aa1afff";
            public const string Kinetic_AirBlastLine00 = "03689858955c6bf409be06f35f09946a";
            public const string Kinetic_Blizzard00_Projectile = "76678c1d607067b4f9e035d54ec08f67";
            public const string Kinetic_Blizzard00_Projectile_Up = "1fdf5df3c2462414e9ce865541a0cdea";
            public const string Kinetic_BlueFlame00_Projectile = "e72b90d2ddaae204297120233a74b236";
            public const string Kinetic_BlueFlame00_Projectile_Up = "dd5836aae3e3a594397608ec5e349460";
            public const string Kinetic_BlueFlameCone00_15Feet = "4e7b940a44b323444a84098748a61333";
            public const string Kinetic_BlueFlameLine00 = "798cd7f47c65e794bba6d7f696b6ef87";
            public const string Kinetic_ChargedWater00_Projectile = "bc49ca6e75929ff469711761c36229b1";
            public const string Kinetic_ChargedWater00_Projectile_Up = "dbbc477853b8f8e428b59778875e69ad";
            public const string Kinetic_ChargedWaterCone00_30Feet_Aoe = "7c469bab1991217479dcfd9d110e48b2";
            public const string Kinetic_ChargedWaterLine00 = "9f395f5ccfc3b8a44b62e2ec3a36fb52";
            public const string Kinetic_EarthBlast00_Projectile = "c28e153e8c212c1458ec2ee4092a794f";
            public const string Kinetic_EarthBlast00_Projectile_Up = "37d89aa911af5404bad6f5a9d2024e9f";
            public const string Kinetic_EarthBlastLine00 = "5d66a6c3cac5124469b2d0474e53ecab";
            public const string Kinetic_EarthSphere00_Projectile = "3751a263d0386ef45807e0111de1a5de";
            public const string Kinetic_EarthSphere00_Projectile_Up = "6959c90a315ec9148a5545ef4bdc1bdd";
            public const string Kinetic_ElectricBlastLine00 = "358b6f193b4b0d94aa8b884b4002cee6";
            public const string Kinetic_Ice00_Projectile = "6064be4a016527443a96d1d02e16d8fb";
            public const string Kinetic_IceBlastLine00 = "ab5cc0b286c3e8d4db125954b874f34a";
            public const string Kinetic_IceSphere00_Projectile = "99be5f02870297b48b0342ba44156dc2";
            public const string Kinetic_Magma00_Projectile = "e3954fb66cad910408998a29a38fd926";
            public const string Kinetic_Magma00_Projectile_Up = "a9cc443cac2ee704490612c97e2d4389";
            public const string Kinetic_MagmaLine00 = "568fede5d2d440146b58c76645588ecd";
            public const string Kinetic_Metal00_Projectile = "85e879aeb4b82994eb989874726790e8";
            public const string Kinetic_Metal00_Projectile_Up = "14acdc1cf19e7a0419ac8561a71bd6f6";
            public const string Kinetic_MetalBlastLine00 = "8dea10e90be82c14d98ac74b5f96805f";
            public const string Kinetic_MetalSphere00_Projectile = "f842a2ecea64c6444a2e161b32d49ef6";
            public const string Kinetic_MetalSphere00_Projectile_Up = "0f36b4b58f84f2c479982a7378fc1d5d";
            public const string Kinetic_Mud00_Projectile = "8193876bdd95bea4d98ab27a12acf374";
            public const string Kinetic_Mud00_Projectile_Up = "03a3a7976e26f3d4782f7cfb2a183df6";
            public const string Kinetic_MudLine00 = "7e2adb987ad724f43833e8eac83843c9";
            public const string Kinetic_Plasma00_Projectile = "fcfbceb1cc0e9764a9c11aca509bf2d4";
            public const string Kinetic_Plasma00_Projectile_Up = "64287d58ed8852944a04b11677b18d9d";
            public const string Kinetic_PlasmaLine00 = "1288a9729b18d3e4682d0f784e5fbd55";
            public const string Kinetic_Sandstorm00_Projectile = "b9e055b9f33aafe49807c44855c4f349";
            public const string Kinetic_Sandstorm00_Projectile_Up = "37749bdf02deb39469d08b1db30cb298";
            public const string Kinetic_SandstormLine00_Projectile = "6210012cfb1a268418bf7023abbe2e88";
            public const string Kinetic_Steam00_Projectile = "36e5df234b905d34f8f5ff542b1f21b8";
            public const string Kinetic_Steam00_Projectile_Up = "3fea4d17623557f4caa9a8d155a33d71";
            public const string Kinetic_SteamLine00 = "00deaa23c2468d04c8491e6468dd5000";
            public const string Kinetic_Thunderstorm00_Projectile = "0a47cc1408ebda749880ff96afb90137";
            public const string Kinetic_Thunderstorm00_Projectile_Up = "940ea79a9b102d44ea13e438da8d9f35";
            public const string Kinetic_ThunderstormLine00 = "9d359fd71f79aa348a85e1f4a843f4ed";
            public const string Kinetic_WaterBlast00_Projectile = "06e268d6a2b5a3a438c2dd52d68bfef6";
            public const string Kinetic_WaterBlast00_Projectile_Up = "a51570088027efa49a1cf8ebba1c921d";
            public const string Kinetic_WaterBlastCone00_30Feet_Aoe = "0ebec8e9eddc29e4496e163822f68ba5";
            public const string Kinetic_WaterLine00 = "f3566859ed1664543a18f1e235bc652c";
            public const string LanternKing00_Autoattack00 = "bd81b726270f63643a68fab4ea3d9257";
            public const string LanternKing_FireCone60Feet00 = "aad90e0f27064be48b1f59cf16609752";
            public const string LanternKing_FireLine00 = "06292045c1990e64e9fc0252fc3175ab";
            public const string LanternKing_Ray00 = "b48a6287dceefc24ea2d22867933e1ff";
            public const string LanternKing_Ray00_Up = "a69feb84fff7e0c468ff272b075ffbca";
            public const string LightningBolt00 = "c7734162c01abdc478418bfb286ed7a5";
            public const string LightningBolt00Breath = "4990cdb96ea77b5439afbc804f12d922";
            public const string LightningBolt00_Direct = "59baf3ad9b8660544902e4d4e9188173";
            public const string LightningBolt00_Miss = "23cffcf4535a9654895fc7815aa0442d";
            public const string LightStoneBlue00 = "b4cfac48e3a51fb4d9fd2d7e404f1690";
            public const string LightStoneRed00 = "91bbcc62755b5e540908887abfce1419";
            public const string MagicMissile00 = "2e3992d1695960347a7f9bdf8122966f";
            public const string MagicMissile00_Up = "26037ad1f9bac1f4bbb294f3cc0dc1c7";
            public const string MagicMissile01 = "741743ccd287a854fbb68ce70f75fa05";
            public const string MagicMissile02 = "674e6d958be63ff4a85a7e5fdc1e818a";
            public const string MagicMissile03 = "caadaf27d789793459a3e32cb0615d14";
            public const string MagicMissile04 = "43295b5988021f741a28b8bf0424a412";
            public const string Meat00_Projectile = "e83075672dfcf7a45ab009d338e5bc33";
            public const string MidnightArrow00 = "68d699fa9d0ebe84c8d35ab7c4e5c811";
            public const string MoltenOrb00 = "49c812020338e90479b54cfc5b1f6305";
            public const string MoltenOrb00_Up = "a5d9e855b3b69024ba0530a689612735";
            public const string Mythic1lvlAngel_RayOfAbsolution00 = "4899e8d4ec9237a4d835ed1d28a66d89";
            public const string Mythic1lvlAngel_RayOfAbsolution00_Up = "4662b22c6255631418c00f2ed9029bc7";
            public const string Mythic1lvlAzata_ElysiumBolt00 = "f00eb27234fbc39448b142f1257c8886";
            public const string Mythic1lvlAzata_ElysiumBolt00_Up = "b43d0e57427ae8741a4bf56f2e10b4a5";
            public const string Mythic4lvlAngel_BladeOfTheSun00 = "991c67fefa21e5e4e8cb5be39e279841";
            public const string Mythic4lvlTrickster_BreathOfMoney00 = "21b365ea64625394db778ca7dc779db1";
            public const string Mythic6lvlAngel_AvengerBlessing00 = "577c13f490b12eb47ac74ef03618e45a";
            public const string Mythic6lvlAngel_BoltOfJustice00 = "9104e4cb49397844e81b3d280d1ff249";
            public const string Mythic6lvlAngel_BoltOfJustice00_Projectile_Up = "c704a6304458c264fb5edf0d09d07fc1";
            public const string Mythic6lvlLich_BloodHeal00_Up = "3466f40abf3202248b633d48be98b135";
            public const string Mythic7lvlAzata_ConeOfLeaves15Feet00 = "d13e1e8eb274bd843b5626b79b0bcbd3";
            public const string Mythic7lvlAzata_ConeOfLeaves30Feet00 = "f0838899c5505364b989993afa640119";
            public const string Mythic7lvlAzata_RainbowArrows00 = "a8baac3173b5d77458e582ee41cc5d9f";
            public const string Mythic7lvlAzata_RainbowArrows01 = "54a9377a7b01f7d44a56e7713f939db6";
            public const string Mythic7lvlLich_BoneLance00 = "df32b6f69bac0294db51d2547cc9b9a4";
            public const string Mythic7lvlLich_BoneLance00_Chain_Projectile = "53149f7913f1a8649b8ee538ce139436";
            public const string Mythic7lvlLich_BoneLance00_Tentacle1_Projectile = "ba3833e8ac0a32144ae3bb1d1182f463";
            public const string Mythic7lvlLich_BoneLance00_Tentacle2_Projectile = "0ddbdb9af9ed1234b898978a318efa28";
            public const string Mythic7lvlLich_BoneLance00_Tentacle2_Projectile_Up = "23a6eed3023db3f4fb9b30d171d8308e";
            public const string NecromancyCone30Feet00 = "4d601ab51e167c04a8c6883260e872ee";
            public const string NecromancyCone50Feet00 = "d07c1c64e5f087e4a919688db7059f0e";
            public const string NegativeCommonProjectile00 = "3f068599c7549e84497154248278e5b0";
            public const string NegativeCommonProjectile00_Up = "3579e29a92f03f9448ca1bd39a2536eb";
            public const string NyrissaRay00 = "e812fdb24ee8959408025045773b1c71";
            public const string NyrissaRay00_Up = "bd81ad34a57028e4a987a0c25fb5f2b1";
            public const string PoisonArrow00 = "91c93711884621f4c995d215084d3982";
            public const string PoisonArrow00_Up = "2244588abcb7a274aa9d7de2ba1f2fdd";
            public const string PoisonCone15Feet00 = "2758f6a35e0e3544f8d7367e57f70d61";
            public const string PoisonCone30Feet00 = "8a85b8590decf4b48bc9b346afcb30ce";
            public const string PoisonCone50Feet00 = "06f486c151474044e908abdc44aed74c";
            public const string PolarRay00 = "68ce28c9ac213e7458670a72da007dd8";
            public const string PolarRay00_Up = "3241ed3e8edd2e747bc804b5e758ff57";
            public const string PrismaticSpray60Feet00 = "5769363a427374f428490092c57820a7";
            public const string ProjectileSpawner_AlchemistFireBomb00 = "00958e7d90dc13f43afc7ebe04f9194b";
            public const string ProjectileSpawner_ArrowAcid00 = "a47972e2a7bd4a249be2a376599a5c76";
            public const string ProjectileSpawner_ArrowAcid01 = "799a05a509f3ae64b8e9d6c142d793d1";
            public const string ProjectileSpawner_ArrowFire00 = "08857ea020f33a44595b5afcfb46e5e7";
            public const string ProjectileSpawner_ArrowFire01 = "58b4976a2a4755c448d7b06f4755bd5b";
            public const string ProjectileSpawner_Fireball00 = "761c7958ad17ce345889207b7b919422";
            public const string ProjectileSpawner_Kinetic_Mud00_Projectile = "6e35b9fa55834fb4786fddea94bdc5ce";
            public const string ProjectileSpawner_Meat00 = "96db56eb2ee077f409e2419ba687e7dd";
            public const string ProjectileSpawner_StoneThrow00 = "b048495198c5f4a4f8c1a1a1fecf9f4a";
            public const string RayOfEnfeeblement00 = "fe47a7660448bc54289823b07547bbe8";
            public const string RayOfEnfeeblement00_Up = "d5f8f5c095e42754bb1dc494822c4be7";
            public const string RayOfExhaustion00 = "8e38d2cfc358e124e93c792dea56ff9a";
            public const string RayOfExhaustion00_Up = "55713599bc3675c42860e49373979762";
            public const string RayOfFrost00 = "d6c9daec1256561408a7a72a6979359e";
            public const string RayOfFrost00_Up = "093881e34d781a248b4572b8eb0e24ce";
            public const string RockThrow00 = "0b84cce27ec7b2346935f1359882d74d";
            public const string RockThrow00_Up = "c2d7b322ede73d64ebfe018205b5b59b";
            public const string ScorchingRay00 = "8cc159ce94d29fe46a94b80ce549161f";
            public const string ScorchingRay00_Up = "e6fc2b335c4676e468e444318a22c445";
            public const string ScreamingFlames00 = "d24189c235ed4074a9d6d84e77e80900";
            public const string SearingLight00 = "2511627d593387d4d89004bec111ba31";
            public const string SearingLight00_Up = "2f23fcf5308a699499f5694215b47a05";
            public const string SnowBall00 = "81a8bff536bae184bacb3a58f0bc381a";
            public const string SnowBall00_Up = "f372eab8f8afec54da987dee187bb180";
            public const string SonicCommonRay00_Projectile = "4be4353c12c18c340823fc7f00727bd1";
            public const string SonicCone15Feet00 = "868f9126707bdc5428528dd492524d52";
            public const string SonicCone30Feet00 = "c7fd792125b79904881530dbc2ff83de";
            public const string SonicCone40Feet00 = "f899d93a411796b4685afc000c3466b0";
            public const string StarknifeCone30Feet00 = "1ee4fd0aed7c6c541a3b941b40f1899e";
            public const string StoneThrow00 = "c8e6e6e315030b443b5ab9bc07843bb2";
            public const string StoneThrow00_Up = "eed07526c4ae61c4b99e85fa8397cb4e";
            public const string SunBeam00 = "c4b0d8b4786a1244d9fbc4b424931b83";
            public const string TelekineticFist = "382f17ed6ed049f419089c5752b05eae";
            public const string TelekineticFist00_Up = "ab984ddba563d2d4faaf90f323511dcc";
            public const string TeleportationShift00 = "84d36a0bfd63cb24f8cb3f530c8676cb";
            public const string TeleportationShift00_Up = "ae42ed14f37e4194e8add0c73b44108e";
            public const string Test_projectile = "52c2455540e211841874ac85f88e89ba";
            public const string ThrowingAxe_projectile = "dbcc51cfd11fc1441a495daf9df9b340";
            public const string Tsunami00 = "f66ad4cd6848c6f43a8fd79cfda5c192";
            public const string UmbralStrike00 = "3d13c52359bcd6645988dd88a120d7c0";
            public const string UmbralStrike00_Old = "c5d06cdd4d9378246b9f6e6f777246e7";
            public const string UmbralStrike00_Old_Up = "1cc16e1c530caa5439e8259cffd3a827";
            public const string UmbralStrike00_Up = "83e58b45c7efab6488a5337e521c72cf";
            public const string VampiricTouch00 = "3b12e2485e793c9489536b71797cf99d";
            public const string VampiricTouch00_Up = "b6f4f2eb3c6b90142adae176e73cc56d";
            public const string Vinetrap00_Projectile_1 = "f00b4b57e29edc24b923ea5b9cefd25b";
            public const string VinetrapCurse00_Projectile = "6391d49ca3a0dfd4f9b2a72ac2123ea9";
            public const string WarpriestCleanserOfEvil_Up = "869a6fcb9f304d34e97fe97b222d3d36";
            public const string WindProjectile00 = "e093b08cd4cafe946962b339faf2310a";
            public const string WindProjectile00_Up = "6648c0c15eda1f14eb80e260f87c55ea";
        }

        public class Sfx
        {
            public const string PreStart_Earth = "69a83b56c1265464f8626a2ab414364a";
            public const string Start_Earth = "852b687aad7863e438c61339dd35d85d";

            public const string PreStart_Electric = "a0b5b95a9a139944c965c593a0a77ff7";
            public const string Start_Electric = "4daa50efa21f9564fb3c5cd35d022cbf";

            public const string Burst10_Flamestrike = "d4d5d1e3ca05da444a969ecb217d8f15";
            public const string Burst10_Fire = "a4464254a634f9f41a91a37cb8ef48fd";
            public const string Burst10_Magma = "0e2c7eccf4456fb4285ceff4c2dae0fa";
            public const string Burst15_Holy = "94708cc80dd5eb14a9a5ec04365a1fdc";
            public const string Burst30_Holy = "d58c16961298c6f4490ed4c158c142e6";

            public const string Weapon_Fire = "91e5a56dd421a2941984a36a2af164b6";
            public const string Weapon_Shock = "1d1465ffa2699644ba8dfac48cb33195";
        }

        internal static string[]? _localizedStrings;
        public static string[] LocalizedStrings
        {
            get
            {
                if (_localizedStrings != null)
                    return _localizedStrings;

                using var scope = new Scope(Main.ModPath!, Main.logger!, Main.harmony!, Main.AllowGuidGeneration);
                return _localizedStrings =
                [
                    //None
                    "", 
                    //MenuDisclaimer
                    Helper.CreateString("Disclaimer: Remember that playing with mods often makes them mandatory for your save game!"),
                    //MenuLegend
                    Helper.CreateString("Legend: [F] This adds a feat. You still need to pick feats/talents for these effects. If you already picked these features, then they stay in effect regardless of the option above.\n[*] Option is enabled/disabled immediately, without restart."),
                    //MenuAllowAchievements
                    Helper.CreateString("[*] Allow achievements - enables achievements while mods are active and also set corresponding flag to future save files"),
                    //MenuManagedAchievements
                    Helper.CreateString("Allow achievements - managed by other mod"),
                    //MenuSaveMetadata
                    Helper.CreateString("Save Metadata (warns when loading incompatible saves)"),
                    //MenuPsychokineticist
                    Helper.CreateString("Psychokineticist Main Stat"),
                    //MenuLimitlessFeats
                    Helper.CreateString("Limitless feats always set cost to 0, instead of reducing by 1"),
                    //MenuAdvancedPatch
                    Helper.CreateString("Advanced: Patch Control"),
                    //MenuPatchExplanation
                    Helper.CreateString("Options in red font may not be disabled during a playthrough. Options marked with <color=red><b>✖</b></color> will not be loaded. You can use this to disable certain patches you don't like or that cause you issues ingame. Options marked with <color=yellow><b>!</b></color> are missing patches to work properly. Check the \"Patch\" section.\n<color=yellow>Warning: All option require a restart. Disabling options may cause your current saves to be stuck at loading, until re-enabled.</color>"),
                    //MenuDisableHomebrew
                    Helper.CreateString("Disable all homebrew"),
                    //MenuNewFeaturesDefault
                    Helper.CreateString("New features default on"),
                    //MenuDebug
                    Helper.CreateString("Debug"),
                    //MenuSave
                    Helper.CreateString("Save settings!"),
                    //MessagePatchChanged
                    Helper.CreateString("Patch settings were changed recently. You must restart game now!"),
                    //MessageIgnoreThisTime
                    Helper.CreateString("Ignore this time"),
                    //MessageIUnderstand
                    Helper.CreateString("I understand"),
                    //MessageReenablePatch
                    Helper.CreateString("[DarkCodex] Patches not found since last time. Do you want to re-enable them or ignore it? (You can turn this off in the menu 'Save Metadata'.) \n"),
                    //MessageEnablePatch
                    Helper.CreateString("Enable patches"),
                    //MessagePatchesEnabled
                    Helper.CreateString("Patches enabled, you must restart game now!"),
                ];
            }
        }
    }

    public enum Localized
    {
        None,
        MenuDisclaimer,
        MenuLegend,
        MenuAllowAchievements,
        MenuManagedAchievements,
        MenuSaveMetadata,
        MenuPsychokineticist,
        MenuLimitlessFeats,
        MenuAdvancedPatch,
        MenuPatchExplanation,
        MenuDisableHomebrew,
        MenuNewFeaturesDefault,
        MenuDebug,
        MenuSave,
        MessagePatchChanged,
        MessageIgnoreThisTime,
        MessageIUnderstand,
        MessageReenablePatch,
        MessageEnablePatch,
        MessagePatchesEnabled,
    }
}
