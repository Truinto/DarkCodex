using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1591 // ignore missing XML comment

namespace CodexLib
{
    /// <summary>
    /// Reference tree of all the kineticist blueprints. Includes base game and mods.
    /// </summary>
    public class KineticistTree
    {
        private static KineticistTree instance;
        public static KineticistTree Instance { get => instance ??= new(); }

        public KineticistTree()
        {
            // Expanded Defense: d741f298dfae8fc40b4615aaf83b6548
            EnabledDarkCodex = UnityModManagerNet.UnityModManager.FindMod("DarkCodex")?.Active == true;
            EnabledElementsExpanded = UnityModManagerNet.UnityModManager.FindMod("KineticistElementsExpanded")?.Active == true;

            @Class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9ec1ad924d889272429eb8391");
            KineticKnight = Helper.ToRef<BlueprintArchetypeReference>("7d61d9b2250260a45b18c5634524a8fb");
            ElementalScion = Helper.ToRef<BlueprintArchetypeReference>("180c6e3574aa4c938e73952cb02d1535");
            ElementalAscetic = Helper.ToRef<BlueprintArchetypeReference>("33780145192140a38a72665b9f877328");
            KineticBlast = Helper.ToRef<BlueprintFeatureReference>("93efbde2764b5504e98e6824cab3d27c");
            KineticistMainStatProperty = Helper.ToRef<BlueprintUnitPropertyReference>("f897845bbbc008d4f9c1c4a03e22357a");

            FocusFirst = Helper.ToRef<BlueprintFeatureSelectionReference>("1f3a15a3ae8a5524ab8b97f469bf4e3d");
            FocusSecond = Helper.ToRef<BlueprintFeatureSelectionReference>("4204bc10b3d5db440b1f52f0c375848b");
            FocusThird = Helper.ToRef<BlueprintFeatureSelectionReference>("e2c1718828fc843479f18ab4d75ded86");
            FocusKnight = Helper.ToRef<BlueprintFeatureSelectionReference>("b1f296f0bd16bc242ae35d0638df82eb");
            SelectionInfusion = Helper.ToRef<BlueprintFeatureSelectionReference>("58d6f8e9eea63f6418b107ce64f315ea");
            SelectionWildTalent = Helper.ToRef<BlueprintFeatureSelectionReference>("5c883ae0cd6d7d5448b7a420f51f8459");
            ExpandedElement = Helper.ToRef<BlueprintFeatureSelectionReference>("acdb730a59e64153964505587b809f93");
            ExtraWildTalent = Helper.ToRef<BlueprintFeatureSelectionReference>("bd287f6d1c5247da9b81761cab64021c");
            CompositeBuff = Helper.ToRef<BlueprintBuffReference>("cb30a291c75def84090430fbf2b5c05e");

            #region Metakinesis

            MetakinesisBuffs = new()
            {
                Helper.Get<BlueprintBuff>("f5f3aa17dd579ff49879923fb7bc2adb"), //MetakinesisEmpowerBuff
                Helper.Get<BlueprintBuff>("f8d0f7099e73c95499830ec0a93e2eeb"), //MetakinesisEmpowerCheaperBuff
                Helper.Get<BlueprintBuff>("870d7e67e97a68f439155bdf465ea191"), //MetakinesisMaximizedBuff
                Helper.Get<BlueprintBuff>("b8f43f0040155c74abd1bc794dbec320"), //MetakinesisMaximizedCheaperBuff
                Helper.Get<BlueprintBuff>("f690edc756b748e43bba232e0eabd004"), //MetakinesisQuickenBuff
                Helper.Get<BlueprintBuff>("c4b74e4448b81d04f9df89ed14c38a95"), //MetakinesisQuickenCheaperBuff
            };

            #endregion

            #region Elements Basic

            Air = new()
            {
                Selection = Helper.ToRef<BlueprintFeatureSelectionReference>("49e55e8f24e1ad24e910fefc0258adba"),
                Progession = Helper.ToRef<BlueprintProgressionReference>("6f1d86ae43adf1049834457ce5264003"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("cb09e292ad9acc3428fa0dfdcbb83883"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("0ab1552e2ebdacf44bb7b20f5393366d"),
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("ccc7583a7cb345a41a33a8e11ddd91b5"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("89acea313b9a9cb4d86bbbca01b90346"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("43ff67143efb86d4f894b10577329050"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("89cc522f2e1444b40ba1757320c58530"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("77cb8c607b263194894a929c8ac59708")
                },
                DamageType = GetDamageType(PhysicalDamageForm.Bludgeoning)
            };

            Electric = new()
            {
                Selection = Air.Selection,
                Progession = Helper.ToRef<BlueprintProgressionReference>("ba7767cb03f7f3949ad08bd3ff8a646f"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("c2c28b6f6f000314eb35fff49bb99920"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("45eb571be891c4c4581b6fcddda72bcd"),
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("55b13ffc1588e4840aaddda7800b85e8"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("b9e9011e24abcab4996e6bd3228bd60b"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("31862bcb47f539649ae59d7e18f8ed11"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("ca608f545b07ec045954aee5ff94640a"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("8d351d2c4af133a41b103aa25f0c38cc")
                },
                DamageType = GetDamageType(e1: DamageEnergyType.Electricity)
            };

            Earth = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("d945ac76fc6a06e44b890252824db30a"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("7f5f82c1108b961459c9884a0fa0f5c4"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("e53f34fb268a7964caf1566afb82dadd"),
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("cfb2ef3aacffde141915a7d5464d3bb3"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("77d9c04214a9bd84bbc1eefabcd98220"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("a72c3375b022c124986365d23596bd21"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("4fc5cf33da20b5444ad3a96c77af8d20"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("d386e82ad6ef52a4ab5251bc2dc6d93f")
                },
                DamageType = GetDamageType(PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing)
            };

            Fire = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("fbed3ca8c0d89124ebb3299ccf68c439"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("cbc88c4c166a0ce4a95375a0a721bd01"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("83d5873f306ac954cad95b6aeeeb2d8c"),
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("17fc78086533bfd4aa3818317c6210bc"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("41e9a0626aa54824db9293f5de71f23f"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("3ca6bbdb3c1dea541891f0568f52db05"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("6e24958866ac8a9498fa6a7396d87270"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("879b666ce3247ed4b8aa379d5946c38e")
                },
                DamageType = GetDamageType(e1: DamageEnergyType.Fire)
            };

            Water = new()
            {
                Selection = Helper.ToRef<BlueprintFeatureSelectionReference>("53a8c2f3543147b4d913c6de0c57c7e8"),
                Progession = Helper.ToRef<BlueprintProgressionReference>("e4027e0fec48e8048a172c6627d4eba9"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("560887b5187098b428364de03e628b53"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("d663a8d40be1e57478f34d6477a67270"),
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("b4f2dc3830fbe6147b701872fbdb87c4"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("70524e9d61b22e948aee1dfe11dc67c8"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("6a1bc011f6bbc7745876ce2692ecdfb5"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("92724a6d6a6225d4895b41e35e973599"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("cf09fb24e432a5c49a1bd9add89699ee")
                },
                DamageType = GetDamageType(PhysicalDamageForm.Bludgeoning)
            };

            Cold = new()
            {
                Selection = Water.Selection,
                Progession = Helper.ToRef<BlueprintProgressionReference>("dbb1159b0e8137c4ea20434a854ae6a8"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("ce625487d909b154c9305e60e4fc7d60"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("7980e876b0749fc47ac49b9552e259c1"),
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("52f6af214e8954149b71bb59a80d222f"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("37c87f140af6166419fe4c1f1305b2b8"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("df849df04cd828b4489f7827dbbf1dcd"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("cb20c297b1db1cd4ea9430578c90246d"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("2312fb9314d9a99489ec32f8be57a87c")
                },
                DamageType = GetDamageType(e1: DamageEnergyType.Cold)
            };

            #endregion

            #region Elements Composite

            Composite_Metal = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("ccd26825e04f8044c881cfcef49f1872"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("ad20bc4e586278c4996d4a81b2448998"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("6276881783962284ea93298c1fe54c48"),
                Parent1 = Earth,
                Parent2 = null,
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("360a3adea35bfca4c9bd6dac151705fc"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("ea2b3e7e3b8726d4c94ba58118749742"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("e72caa96c32ca3f4d8b736b97b067f58"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("9cef404da5745314b88f49c1ee9fbab1"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("b66add7c13a8398488ed3e915ade09d3")
                },
                DamageType = GetDamageType(PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing)
            };

            Composite_BlueFlame = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("cdf2a117e8a2ccc4ebabd2fcee1e4d09"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("89dfce413170db049b0386fff333e9e1"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("d29186edb20be6449b23660b39435398"),
                Parent1 = Fire,
                Parent2 = null,
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("f8934aab37bd99f4285cfa1e9d998f23"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("4005fc2cd91860142ba55a369fbbec23"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("5b0f10876af4fe54e989cc4d93bd0545"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("a975a40b710833a468476564fa673cee"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("1c35c032bb452014090d05130fa653df")
                },
                DamageType = GetDamageType(e1: DamageEnergyType.Fire)
            };

            Composite_Plasma = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("953fe61325983f244adbd7384903393d"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("93d8bc401accfe6489ea3797e316e5d9"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("9afdc3eeca49c594aa7bf00e8e9803ac"),
                Parent1 = Air,
                Parent2 = Fire,
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("b947cdebf1c0d3945a138283b22937f2"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("acc31b4666e923b49b3ab85b2304f26c"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("878f68ff160c8fa42b05ade8b2d12ea5"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("fc22c06d63a95154291272577daa0b4d"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("c9262ac06266bc64990ee98e528d8eed")
                },
                DamageType = GetDamageType(PhysicalDamageForm.Bludgeoning, DamageEnergyType.Fire)
            };

            Composite_Sand = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("f05a7bde1b2bf9e4e927b3b1aeca8bfb"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("af70dce0745f91f4b8aa99a98620e45b"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("b93e1f0540a4fa3478a6b47ae3816f32"),
                Parent1 = Air,
                Parent2 = Earth,
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("30a92d0b741ac8547a24a4ec2561c6dd"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("dc6f0b906566aca4d8b86729855959cb"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("4934f54691fa90941b04341d457f4f96"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("a41bfd708a7677f46aede02715f3100d"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("54c7ff613923d304bb39e163959435fb")
                },
                DamageType = GetDamageType(PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing)
            };

            Composite_Thunder = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("33217c1678c30bd4ea2748decaced223"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("295080cf4691df9438f58ff5ce79ee65"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("b813ceb82d97eed4486ddd86d3f7771b"),
                Parent1 = Air,
                Parent2 = Electric,
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("785d708a6464709499b9021b53200356"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("287e0c88af08f3e4ba4aca52566f33a7"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("a8cd6e691ad7ee44dbdd4a255bf304d8"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("833e3c01a1492d74588430249e6431af"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("4f6847c9d896da946b6d86bd513e76a9")
                },
                DamageType = GetDamageType(PhysicalDamageForm.Bludgeoning, DamageEnergyType.Electricity)
            };

            Composite_Blizzard = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("747a1f33ed0a17442b3273adc7797661"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("52292a32bb5d0ab45a86621bac2c4c9a"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("16617b8c20688e4438a803effeeee8a6"),
                Parent1 = Air,
                Parent2 = Cold,
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("3ae835b50a65fe744af4f5f652c3b724"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("55790f1d270297f4a998292e1573a09e"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("6f121ff0644a2804d8239d4dfe0ace11"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("027ce0b3842170748a63ea04cb02cab7"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("b066761047bbe0348a3a0b2f1debbd34")
                },
                DamageType = GetDamageType(PhysicalDamageForm.Piercing, DamageEnergyType.Cold)
            };

            Composite_Ice = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("d6375ba9b52eca04a805a54765310976"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("a8cc34ca1a5e55a4e8aa5394efe2678e"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("403bcf42f08ca70498432cf62abee434"),
                Parent1 = Cold,
                Parent2 = Water,
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("b48284a3a6e71894a8101afe81b3f0b8"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("3f68b8bdd90ccb0428acd38b84934d30"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("a1eee0a2735401546ba2b442e1a9d25d"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("8c8dd4e7c07e468498a6f5ed2c01063f"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("9b8ea70f14970f946ad6c26694062a3f")
                },
                DamageType = GetDamageType(PhysicalDamageForm.Piercing, DamageEnergyType.Cold)
            };

            Composite_Magma = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("cb19d1cbf6daf7a46bf38c05af1c2fb0"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("408b25c6d9f223b41b935e6ec550e88d"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("8c25f52fce5113a4491229fd1265fc3c"),
                Parent1 = Earth,
                Parent2 = Fire,
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("6a442223b7c775647b2f96235ad79e70"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("cf1085900220be5459273282389aa9c2"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("f58bc29b252308242a81b3f84a1d176a"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("c49d2ddf72adf85478d6b3e09f52d32e"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("2f391179f4cdd574b9093e62497a6d7e")
                },
                DamageType = GetDamageType(PhysicalDamageForm.Bludgeoning, DamageEnergyType.Fire)
            };

            Composite_Mud = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("648d3c01bcab7614595facd302e88184"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("6e33cde96209b5a4f9596a6e509de532"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("e2610c88664e07343b4f3fb6336f210c"),
                Parent1 = Earth,
                Parent2 = Water,
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("6f0c1a5dd9b25f84e82db651a92c3825"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("5639fadad8b45e2418b356327d072789"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("64885226d77f2bd408dde84fb8ccacc2"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("f82cfcf11b94bef49bf1a8f57aad5c13"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("c6334b1a104de294dba47ce56c74640f")
                },
                DamageType = GetDamageType(PhysicalDamageForm.Bludgeoning)
            };

            Composite_ChargedWater = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("e717ae6647573bf4195ea168693c7be0"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("9b7bf2754e2012e4dac135fd6c782fac"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("4e2e066dd4dc8de4d8281ed5b3f4acb6"),
                Parent1 = Electric,
                Parent2 = Water,
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("988e3aed25d921144903fbe5a6cc2a8a"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("371b160cbb2ce9c4a8d6c28e61393f6d"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("7b413fc4f99050349ab5488f83fe25df"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("ff24a4ac444afeb4bab5699828aa4e77"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("f2b96598bcfba72469852b6480bf1397")
                },
                DamageType = GetDamageType(PhysicalDamageForm.Bludgeoning, DamageEnergyType.Electricity)
            };

            Composite_Steam = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("985fa6f168ea663488956713bc44a1e8"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("29e4076127a404e4ab1cde7e967e1047"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("3baf01649a92ae640927b0f633db7c11"),
                Parent1 = Fire,
                Parent2 = Water,
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("e80f7c9cf46be8a47a280c3af51557a5"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("66028030b96875b4c97066525ff75a27"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("2e72609caf23e4843b246bec80550f06"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("77dc27ae2f48ffe4a8ab17154145f1d8"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("c74117665610ddb4cb8a525c2ec93039")
                },
                DamageType = GetDamageType(PhysicalDamageForm.Bludgeoning, DamageEnergyType.Fire)
            };

            Composite_Blood = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("535a9c4dbe912924396ae50cc7fba8c4"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("79b5d7184efe7034a863ae612c429306"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("ba2113cfed0c2c14b93c20e7625a4c74"),
                Parent1 = Water,
                Parent2 = null,
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("4038e2f9b6cb4144bb017c7b5910ec51"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("98f0da4bf25a34a4caffa6b8a2d33ef6"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("92f9a719ffd652947ab37363266cc0a6"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("0a386b1c2b4ae9b4f81ddf4557155810"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("15278f2a9a5eaa441a261ec033b60b57")
                },
                DamageType = GetDamageType(PhysicalDamageForm.Bludgeoning)
            };

            #endregion

            #region Elements Modded

            Telekinetic = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("6ce72cb2bf0244b0bd0e5e0a552a6a4a"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("e86649f76cba4483bed7f01859c6b425"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("ac038f9898ef4ba7b46bfcafdbc77818"),
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("1f782a21a45a4d73be6068430cfb2e58"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("d3251ffc4e054f3db2c2260fa9ae4fe2"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("45b1ace89f03422199a394089e3dfc8c"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("4623d7cc61c34d7190cc315695821e61"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("5d81270056d24a2e88df79dfb983cbcd")
                },
                DamageType = GetDamageType(PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing)
            };

            Composite_Force = new()
            {
                Selection = null,
                Progession = null,
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("c6e4201d7b674cc78a2c95bae61b9d25"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("8dacff62b4a8413bbfb299458cf94839"),
                Parent1 = Telekinetic,
                Parent2 = null,
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("9716097909724993811e54ed191429e7"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("2b345c3493964de4affa6cea8327e88a"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("110bb5800599469ebb5f50b400b860d6"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("4e2d7b4eebc348b2bdf4968053c76af9"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("0b8bc0ee998a41508052ca7ff31c14f8")
                },
                DamageType = new DamageTypeDescription() { Type = DamageType.Force }.ObjToArray()
            };

            Gravity = new()
            {
                Selection = Helper.ToRef<BlueprintFeatureSelectionReference>("2b5ad478d5874bd48cdf7be60e4a92b6"),
                Progession = Helper.ToRef<BlueprintProgressionReference>("da0d241e1c63441e8d9ee50f61de8c1f"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("969553ee365642bf9389604cf52b6035"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("d927b99fb8a946c0831edfb97eb749a4"),
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("8be447a7330d4c748c38f124d64915a5"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("6d4210715a824d34a283fc5a8ba1d1df"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("f1ae02e54af04cfd802b652816ce4996"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("c545106ff205431fa7936155728f7490"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("1ec348ed9dcb437eb601f20b98f25181")
                },
                DamageType = GetDamageType(PhysicalDamageForm.Bludgeoning)
            };

            Negative = new()
            {
                Selection = Gravity.Selection,
                Progession = Helper.ToRef<BlueprintProgressionReference>("21b063289b4f4c7783a24b179a0ea3c0"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("b8d46890ceaa4b6c878d9cce68894ff4"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("036de5238a1447a293e4e4749b6724eb"),
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("8d539f95823f4e109a4cac2f0e1e8565"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("dd2f227668514e41a283717bae4517f1"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("7f45d4a741ee4ba685b91e4640191de8"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("96e914a61b6948a5baade3290c0260d3"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("4471efdc8c1440faba7110675ddb31af")
                },
                DamageType = GetDamageType(e1: DamageEnergyType.NegativeEnergy)
            };

            Composite_Void = new()
            {
                Selection = null,
                Progession = null,
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("69d4bdab76bd4288ba7d06c762403b02"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("ec30ffe2d10543d9a2bb04b11e5b1e3d"),
                Parent1 = Telekinetic,
                Parent2 = null,
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("59bc225ce1394ca8a37acfef9829356f"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("8be4ef811d614cac8e9764108457cb68"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("e4fdafb5594e435db1aeb814964fe239"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("506c8ef74b6546c7a44b2547eacdd16d"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("6b2bed26fcd847c1a571b5f2ea6cea0a")
                },
                DamageType = GetDamageType(PhysicalDamageForm.Bludgeoning, DamageEnergyType.NegativeEnergy)
            };

            Wood = new()
            {
                Selection = Helper.ToRef<BlueprintFeatureSelectionReference>("959431c21e414452ada0ce3c45ed49e6"),
                Progession = Helper.ToRef<BlueprintProgressionReference>("736473267be3455bad091a5138423175"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("b8d12c67c67a44c383382b5e62b4460d"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("c7451e98f60f4f4fa46b0a5fedea29c1"),
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("293c5321fdc44f319ebd2ac34e98e9c9"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("1d89d4f9793e4dbea5ed7cce02757352"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("469e82d66a2e4ece94be280935da805a"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("9040572d07d1402abbcbe095f49b58cc"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("e7c2a4e7dcae40b09dda30a879123483")
                },
                DamageType = GetDamageType(PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing)
            };

            Positive = new()
            {
                Selection = Wood.Selection,
                Progession = Helper.ToRef<BlueprintProgressionReference>("d0d8d2bb86d44473bd24ceb34f0ef6ea"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("e61724bb0727488f97b84164590846cd"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("64e7aabc2b55441d8a2513533fff7eb8"),
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("83a07f00521f48cfb0f73d81ab089a48"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("378d8d54b21743c3a32f26b95f768d44"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("7777b5f1d5c24de4a7a42cb4f7752067"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("53ba7977e3ed44b193c31c7f510c52e2"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("22d5001563c74bdfa6e0c5602fd11eef")
                },
                DamageType = GetDamageType(e1: DamageEnergyType.PositiveEnergy)
            };

            Composite_Verdant = new()
            {
                Selection = null,
                Progession = null,
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("46703587324247a08bc45992f77c8b16"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("815b096429ca401faa7c5979833a37de"),
                Parent1 = Wood,
                Parent2 = Positive,
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("21bf644d6c71430d84b16df14d5fc01d"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("b6732b8fcba545e58b5923710435ac42"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("226251dfa5d54a36a89b754cf9a4adde"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("c7c9e1c2d5b9436fa37111ecf4da3477"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("2846063b5b6e4fea9bee612c1e24dd60")
                },
                DamageType = GetDamageType(PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, DamageEnergyType.PositiveEnergy)
            };

            Composite_Spring = new()
            {
                Selection = null,
                Progession = null,
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("7b74373443c145a4b08e71c2bd8d5699"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("b0ea56ad77fa4742b0b226d92dbdfe9e"),
                Parent1 = Wood,
                Parent2 = Air,
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("cc5189f4990a43c5a353a68ac8ca54a8"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("64f5bf967b5e440fa8cf648c532479ec"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("5bdbf44b60104461934da737825a09bf"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("a5c899151b4943a0a6d0aeb853e8fc88"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("9e26ea79e09b4e1b9ee09cbd25ae1405")
                },
                DamageType = GetDamageType(PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing)
            };

            Composite_Summer = new()
            {
                Selection = null,
                Progession = null,
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("133df13895884033b4e77b595c48ce68"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("e7aea805765c428d89060b1ff749edc5"),
                Parent1 = Wood,
                Parent2 = Fire,
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("59f934afdc804d4d9894858905e519ca"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("6958f833102148fe97fc665c607d2372"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("6558f16df5b44709b9dc130671c0bb96"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("2ca9d84f2ece4df3951fe0c84879b164"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("b5c7051ae9c8450da9769c955971f0c2")
                },
                DamageType = GetDamageType(PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, DamageEnergyType.Fire)
            };

            Composite_Autumn = new()
            {
                Selection = null,
                Progession = null,
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("47d82c872f87444bbe7a109fa086ba87"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("08291e4931d14d08b3f7b1dc77e7ec44"),
                Parent1 = Wood,
                Parent2 = Earth,
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("b30a4f38cae4459eb0d3d880b0f0de08"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("d37cf8e066b64e4d8437ab2b3805c521"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("2f8dac5035524c08b97d554c9ddee710"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("8186a1b2087b44519791bb059fe43ff8"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("beb6066e7de74ff0a7eb9d0eb0f6ff36")
                },
                DamageType = GetDamageType(PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing)
            };

            Composite_Winter = new()
            {
                Selection = null,
                Progession = null,
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("b8cb2c99f1194de5af1109835ce7645b"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("555bb22937234202b2dbcd927a7a2676"),
                Parent1 = Wood,
                Parent2 = Cold,
                Blade = new()
                {
                    Feature = Helper.ToRef<BlueprintFeatureReference>("0e3854a031bf4655ac20e28e592bab7d"),
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("ece22aa5b84b4e60957fdabd494e074f"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("639d3c53dd1948ff9c503fda59b8ebab"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("e5945f8fdddc4e0585352a5f12bf1a99"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("380b9f337f2248dc82d4b1c7af8bd507")
                },
                DamageType = GetDamageType(PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, DamageEnergyType.Cold)
            };

            Boost_Aetheric = new()
            {
                Selection = null,
                Progession = null,
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("34f4ddbc9f3542d598a0cc9e0ae019bb"),
                BaseAbility = null,
                BoostActivatable = Helper.ToRef<BlueprintActivatableAbilityReference>("799fd079b82a40e0a2c29061fd0c2182"),
                Parent1 = Telekinetic,
                Parent2 = null,
                Blade = null,
                DamageType = GetDamageType(PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing),
                ModifiesSimple = true,
                ModifiesComposite = false,
                ModifiesEnergy = true,
                ModifiesPhysical = true,
            };

            Boost_AethericGreater = new()
            {
                Selection = null,
                Progession = null,
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("4e2d370657b24dd78239a64b1949010d"),
                BaseAbility = null,
                BoostActivatable = Helper.ToRef<BlueprintActivatableAbilityReference>("0167ebded6f145b681ee94adf02dd052"),
                Parent1 = Telekinetic,
                Parent2 = null,
                Blade = null,
                DamageType = GetDamageType(PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing),
                ModifiesSimple = true,
                ModifiesComposite = true,
                ModifiesEnergy = true,
                ModifiesPhysical = true,
            };

            Boost_Gravitic = new()
            {
                Selection = null,
                Progession = null,
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("60f9f403494546bba5866f456e799b2d"),
                BaseAbility = null,
                BoostActivatable = Helper.ToRef<BlueprintActivatableAbilityReference>("a9cf8aa916e4420d8c5eacbfd794a1b7"),
                Parent1 = Gravity,
                Parent2 = null,
                Blade = null,
                DamageType = GetDamageType(PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing),
                ModifiesSimple = true,
                ModifiesComposite = false,
                ModifiesEnergy = false,
                ModifiesPhysical = true,
            };

            Boost_GraviticGreater = new()
            {
                Selection = null,
                Progession = null,
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("a8d83675d00544469f1fea2cd371f4e1"),
                BaseAbility = null,
                BoostActivatable = Helper.ToRef<BlueprintActivatableAbilityReference>("2f2fd6c377f54912aee78917b9e1d3cd"),
                Parent1 = Gravity,
                Parent2 = null,
                Blade = null,
                DamageType = GetDamageType(PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing),
                ModifiesSimple = true,
                ModifiesComposite = true,
                ModifiesEnergy = false,
                ModifiesPhysical = true,
            };

            Boost_NegativeAdmixture = new()
            {
                Selection = null,
                Progession = null,
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("b0b71503bb864c78adc0a3cbdfa6fc51"),
                BaseAbility = null,
                BoostActivatable = Helper.ToRef<BlueprintActivatableAbilityReference>("2e62673f2e63442197b1f71e5a886205"),
                Parent1 = Negative,
                Parent2 = null,
                Blade = null,
                DamageType = GetDamageType(e1: DamageEnergyType.NegativeEnergy),
                ModifiesSimple = true,
                ModifiesComposite = false,
                ModifiesEnergy = true,
                ModifiesPhysical = false,
            };

            Boost_PositiveAdmixture = new()
            {
                Selection = null,
                Progession = null,
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("dcacf5d46c814dbfbb8cf82c0b25c518"),
                BaseAbility = null,
                BoostActivatable = Helper.ToRef<BlueprintActivatableAbilityReference>("0f1c8705825b494ba25525a7f05c0fb6"),
                Parent1 = Positive,
                Parent2 = null,
                Blade = null,
                DamageType = GetDamageType(e1: DamageEnergyType.PositiveEnergy),
                ModifiesSimple = true,
                ModifiesComposite = false,
                ModifiesEnergy = true,
                ModifiesPhysical = false,
            };

            #endregion

            #region Focus

            FocusAir = new()
            {
                First = Helper.ToRef<BlueprintProgressionReference>("2bd0d44953a536f489082534c48f8e31"),
                Second = Helper.ToRef<BlueprintProgressionReference>("659c39542b728c04b83e969c834782a9"),
                Third = Helper.ToRef<BlueprintProgressionReference>("651570c873e22b84f893f146ce2de502"),
                Knight = Helper.ToRef<BlueprintProgressionReference>("93bd14dd916cfd1429c11ad66adf5e2b"),
                Defense = Helper.ToRef<BlueprintFeatureReference>("bb0de2047c448bd46aff120be3b39b7a"),
                Element1 = Air,
                Element2 = Electric,
                Composite = Composite_Thunder
            };

            FocusEarth = new()
            {
                First = Helper.ToRef<BlueprintProgressionReference>("c6816ad80a3df9c4ea7d3b012b06bacd"),
                Second = Helper.ToRef<BlueprintProgressionReference>("956b65effbf37e5419c13100ab4385a3"),
                Third = Helper.ToRef<BlueprintProgressionReference>("c43d9c2d23e56fb428a4eb60da9ba1cb"),
                Knight = Helper.ToRef<BlueprintProgressionReference>("d2a93ab18fcff8c419b03a2c3d573606"),
                Defense = Helper.ToRef<BlueprintFeatureReference>("a275b35f282601944a97e694f6bc79f8"),
                Element1 = Earth,
                Element2 = null,
                Composite = Composite_Metal
            };

            FocusFire = new()
            {
                First = Helper.ToRef<BlueprintProgressionReference>("3d8d3d6678b901444a07984294a1bc24"),
                Second = Helper.ToRef<BlueprintProgressionReference>("caa7edca64af1914d9e14785beb6a143"),
                Third = Helper.ToRef<BlueprintProgressionReference>("56e2fc3abed8f2247a621ac37e75f303"),
                Knight = Helper.ToRef<BlueprintProgressionReference>("d4a2a75d01d1e77489ff692636a538bf"),
                Defense = Helper.ToRef<BlueprintFeatureReference>("8ad77685e64842c45a6f5b19f9086c6c"),
                Element1 = Fire,
                Element2 = null,
                Composite = Composite_BlueFlame
            };

            FocusWater = new()
            {
                First = Helper.ToRef<BlueprintProgressionReference>("7ab8947ce2e19c44a9edcf5fd1466686"),
                Second = Helper.ToRef<BlueprintProgressionReference>("faa5f1233600d864fa998bc0afe351ab"),
                Third = Helper.ToRef<BlueprintProgressionReference>("86eff374d040404438ad97fedd7218bc"),
                Knight = Helper.ToRef<BlueprintProgressionReference>("5e839c743c6da6649a43cdeb70b6018f"),
                Defense = Helper.ToRef<BlueprintFeatureReference>("29ec36fa2a5b8b94ebce170bd369083a"),
                Element1 = Water,
                Element2 = Cold,
                Composite = Composite_Ice
            };

            // modded
            FocusAether = new()
            {
                First = Helper.ToRef<BlueprintProgressionReference>("6AA8A023-FC1D-4DAD-B6C2-7CC01B7BF48D"),
                Second = Helper.ToRef<BlueprintProgressionReference>("ff967af2a4634048be9d4beab75d86be"),
                Third = Helper.ToRef<BlueprintProgressionReference>("ff2e26f01237404f8a820f61212b3917"),
                Knight = Helper.ToRef<BlueprintProgressionReference>("0af9c49df79a469cbfc29fa469d97a64"),
                Defense = Helper.ToRef<BlueprintFeatureReference>("772d6eb030d547d6b9f85e599ec9fef1"),
                Element1 = Telekinetic,
                Element2 = null,
                Composite = Composite_Force
            };

            FocusVoid = new()
            {
                First = Helper.ToRef<BlueprintProgressionReference>("8993ff38adce4d758e9f48cc010b930f"),
                Second = Helper.ToRef<BlueprintProgressionReference>("ace3846cf5324cd080f0e4cfd68b26e7"),
                Third = Helper.ToRef<BlueprintProgressionReference>("9f02317d4e5e476ba1678d0ccad13ef9"),
                Knight = Helper.ToRef<BlueprintProgressionReference>("65e6ed019bc742b78e7a203f8ab45aac"),
                Defense = Helper.ToRef<BlueprintFeatureReference>("92de8ee602184f1eb81434edc204a7b5"),
                Element1 = Gravity,
                Element2 = Negative,
                Composite = Composite_Void
            };

            FocusWood = new()
            {
                First = Helper.ToRef<BlueprintProgressionReference>("738e456aa18543a88027e4e8459d3b87"),
                Second = Helper.ToRef<BlueprintProgressionReference>("c8355b680ec040efb4e1e1741df662c3"),
                Third = Helper.ToRef<BlueprintProgressionReference>("1a48f56f45584e619490210319330d4d"),
                Knight = Helper.ToRef<BlueprintProgressionReference>("ade87cba8c0e4883be35d1b5563f28e1"),
                Defense = Helper.ToRef<BlueprintFeatureReference>("12617b1537b749a0b7a4e30d2627ba7a"),
                Element1 = Wood,
                Element2 = Positive,
                Composite = Composite_Verdant
            };

            #endregion

            #region Infusions

            DefaultAbility = new()
            {
                Feature = new(),
                Buff = null,
                RequiresRangedAttackRoll = true,
                Variants = new()
                {
                    Helper.ToRef<BlueprintAbilityReference>("31f668b12011e344aa542aa07ab6c8d9"),
                    Helper.ToRef<BlueprintAbilityReference>("519e36decde7c964d87c2ffe4d3d8459"),
                    Helper.ToRef<BlueprintAbilityReference>("3236a9e26e23b364e8951ee9e92554e8"),
                    Helper.ToRef<BlueprintAbilityReference>("b28c336c10eb51c4a8ded0258d5742e1"),
                    Helper.ToRef<BlueprintAbilityReference>("e3f41966c2d662a4e9582a0497621c46"),
                    Helper.ToRef<BlueprintAbilityReference>("bc3c9fe311294c0dbc8fee81b326fb5f"),
                    Helper.ToRef<BlueprintAbilityReference>("f75f44162a7e48fa860e0c98c6429c0a"),
                    Helper.ToRef<BlueprintAbilityReference>("97e5b81c049147418ab2faa05c45d19e"),
                    Helper.ToRef<BlueprintAbilityReference>("24f26ac07d21a0e4492899085d1302f6"),
                    Helper.ToRef<BlueprintAbilityReference>("7b4f0c9a06db79345b55c39b2d5fb510"),
                    Helper.ToRef<BlueprintAbilityReference>("f6d32ecd20ebacb4e964e2ece1c70826"),
                    Helper.ToRef<BlueprintAbilityReference>("f6319358af7843c180838d223a19d053"),
                    Helper.ToRef<BlueprintAbilityReference>("54beb778985445a0a89113ede52b313d"),
                    Helper.ToRef<BlueprintAbilityReference>("665cfd3718c4f284d80538d85a2791c9"),
                    Helper.ToRef<BlueprintAbilityReference>("a5631955254ae5c4d9cc2d16870448a2"),
                    Helper.ToRef<BlueprintAbilityReference>("7b8a4a256d4f3dc4d99192bbaabcb307"),
                    Helper.ToRef<BlueprintAbilityReference>("fc432e7a63f5a3545a93118af13bcb89"),
                    Helper.ToRef<BlueprintAbilityReference>("27f582dcef8206142b01e27ad521e6a4"),
                    Helper.ToRef<BlueprintAbilityReference>("a0f05637428cbca4bab8bc9122b9e3b9"),
                    Helper.ToRef<BlueprintAbilityReference>("40681ea748d98f54ba7f5dc704507f39"),
                    Helper.ToRef<BlueprintAbilityReference>("08eb2ade31670b843879d8841b32d629"),
                    Helper.ToRef<BlueprintAbilityReference>("ab6e3f470fba2d349b7b7ef0990b5476"),
                    Helper.ToRef<BlueprintAbilityReference>("ff8b6c23fe4e4b3b816d1819372f2668"),
                    Helper.ToRef<BlueprintAbilityReference>("26a4c8ea8ee6434caf19bae6e588b721"),
                    Helper.ToRef<BlueprintAbilityReference>("2862a629a1ad4ddb9439d287cbb2a48d"),
                    Helper.ToRef<BlueprintAbilityReference>("0e8eef22108d45849f46e8bb512f1148"),
                    Helper.ToRef<BlueprintAbilityReference>("2e838c456419460c9e50a50a4b9d3daa"),
                    Helper.ToRef<BlueprintAbilityReference>("b2a2f0376759441cba623407b094b14b"),
                    Helper.ToRef<BlueprintAbilityReference>("cab7dca922f94e3d971ce6b076b23750"),
                    Helper.ToRef<BlueprintAbilityReference>("322911b79eabdb64f8b079c7a2d95e68"),
                },
            };
            BladeWhirlwind = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("80fdf049d396c33408a805d9e21a42e1"),
                Buff = null,
                RequiresMeleeAttackRoll = true,
                Activator = Helper.ToRef<BlueprintAbilityReference>("80f10dc9181a0f64f97a9f7ac9f47d65"),
            };
            KineticBlade = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("9ff81732daddb174aa8138ad1297c787"),
                Buff = null,
                RequiresMeleeAttackRoll = true,
            };
            Bleeding = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("75cbe35e4ada12441a0270d541c12c64"),
                Buff = Helper.ToRef<BlueprintBuffReference>("492a8156ecede6345a8e82475eed85ac"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("6d35b4f39de9eb446b2d0a65b931246b"),
            };
            Cloud = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("3c53ee4965a13d74e81b37ae34f0861b"),
                Buff = null,
                Variants = new()
                {
                    Helper.ToRef<BlueprintAbilityReference>("251af7913c0a0f442a38bc85ed5737a8"),
                    Helper.ToRef<BlueprintAbilityReference>("6462a12f53252aa4fbd3b18f99c9d1a8"),
                    Helper.ToRef<BlueprintAbilityReference>("c6b747b7a087ed942b743e3911018464"),
                    Helper.ToRef<BlueprintAbilityReference>("ba303565ad91ae542ac7eba89f59a9c4"),
                },
            };
            Cyclone = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("f2fa7541f18b8af4896fbaf9f2a21dfe"),
                Buff = null,
                Variants = new()
                {
                    Helper.ToRef<BlueprintAbilityReference>("9fbc4fe045472984aa4a2d15d88bdaf9"),
                    Helper.ToRef<BlueprintAbilityReference>("2d1f3ad47ce421745b80495b9ed8ddc9"),
                    Helper.ToRef<BlueprintAbilityReference>("3e5996148b4ff634ea7033e112710402"),
                    Helper.ToRef<BlueprintAbilityReference>("cca552f27c6ea4f458858fb857212df7"),
                },
            };
            DeadlyEarth = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("061f5d7e659432b478668b70f6d4caae"),
                Buff = null,
                Variants = new()
                {
                    Helper.ToRef<BlueprintAbilityReference>("e29cf5372f89c40489227edc9ffc52be"),
                    Helper.ToRef<BlueprintAbilityReference>("44804ca6ba7d495439cc9d5ad6d6cfcf"),
                    Helper.ToRef<BlueprintAbilityReference>("c0704daaf6e4c5840a94e7db6d7dbe0e"),
                    Helper.ToRef<BlueprintAbilityReference>("0be97d0e752060f468bbf62ce032b9f5"),
                    Helper.ToRef<BlueprintAbilityReference>("0e3b058a1c0042bb86d5c39264f387ce"),
                },
            };
            Detonation = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("77c24cc95ce319c44a0e5fc6ff466d5b"),
                Buff = null,
                Variants = new()
                {
                    Helper.ToRef<BlueprintAbilityReference>("d651db4ffb7441548a06b11de5f163a1"),
                    Helper.ToRef<BlueprintAbilityReference>("2ca478c57073c9f469beef873b001503"),
                },
            };
            Eruption = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("00f8e4b846c367141afcd133f4a1c816"),
                Buff = null,
                Variants = new()
                {
                    Helper.ToRef<BlueprintAbilityReference>("5b69fce8b7890de4b8b9ab973158fed8"),
                    Helper.ToRef<BlueprintAbilityReference>("7cc353f52000d4742a2710fa38de7357"),
                    Helper.ToRef<BlueprintAbilityReference>("f42bf8d4379d1b641b6163aa317ec80e"),
                    Helper.ToRef<BlueprintAbilityReference>("0f70d9349ef23bf4089387edac18317c"),
                },
            };
            ExtendedRange = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("cb2d9e6355dd33940b2bef49e544b0bf"),
                Buff = null,
                RequiresRangedAttackRoll = true,
                Variants = new()
                {
                    Helper.ToRef<BlueprintAbilityReference>("cae4cb39eb87a5d47b8ff35fd948dc4f"),
                    Helper.ToRef<BlueprintAbilityReference>("7d4712812818f094297f7d7920d130b1"),
                    Helper.ToRef<BlueprintAbilityReference>("11eba1184c7108846a665d8ca317963f"),
                    Helper.ToRef<BlueprintAbilityReference>("1c5025eb4b674a7c9fa35df66ffc9096"),
                    Helper.ToRef<BlueprintAbilityReference>("e90886e324ec41428886d2876343adc7"),
                    Helper.ToRef<BlueprintAbilityReference>("c9192f1ef1a34b1bbf7afb49b3901b2d"),
                    Helper.ToRef<BlueprintAbilityReference>("3af9f0b8c187f1d44874f71685da7678"),
                    Helper.ToRef<BlueprintAbilityReference>("7bc1270b5bb78834192215bc03f161cc"),
                    Helper.ToRef<BlueprintAbilityReference>("b6b4836858298a2499ea8f7748fb9511"),
                    Helper.ToRef<BlueprintAbilityReference>("822ff77f031a4d749db8c24a32f8a087"),
                    Helper.ToRef<BlueprintAbilityReference>("55d634f355ca41e0a4c19c606df9599d"),
                    Helper.ToRef<BlueprintAbilityReference>("d88c351a3425ee64f80e2fb836a8acf7"),
                    Helper.ToRef<BlueprintAbilityReference>("f238bef4aa0a7514f9f96fb17ec61261"),
                    Helper.ToRef<BlueprintAbilityReference>("8ebb22bc257c01b489a20836a2c71792"),
                    Helper.ToRef<BlueprintAbilityReference>("ae0fbfd4d646d34439512b44f9d9ffd5"),
                    Helper.ToRef<BlueprintAbilityReference>("db6b0fd6a1337814e9d9868b30b1495b"),
                    Helper.ToRef<BlueprintAbilityReference>("0a6c7f854285c834f81bf90eb1421b37"),
                    Helper.ToRef<BlueprintAbilityReference>("77ed869ab8012df40b64a68ca5125960"),
                    Helper.ToRef<BlueprintAbilityReference>("12e1aa0f2cc5ca34c80055110190eafe"),
                    Helper.ToRef<BlueprintAbilityReference>("79da95d61c5b2de40a8f82a3b5d88928"),
                    Helper.ToRef<BlueprintAbilityReference>("2f37688defd32d740be8bfc21b3b00fe"),
                    Helper.ToRef<BlueprintAbilityReference>("3e746cc9c2419bd4faa8b221f21b5a1b"),
                    Helper.ToRef<BlueprintAbilityReference>("10dc28858a8d41f18c3048d0d8d3a72d"),
                    Helper.ToRef<BlueprintAbilityReference>("939419ac32694ef485b2d50185e7c867"),
                    Helper.ToRef<BlueprintAbilityReference>("8b6ddcff509948e8854724592527d3b2"),
                    Helper.ToRef<BlueprintAbilityReference>("97ccbdcdeec9449599e00dccd1dd2346"),
                    Helper.ToRef<BlueprintAbilityReference>("fe6383b44fd0457c8d61656ab023149e"),
                    Helper.ToRef<BlueprintAbilityReference>("3f36b4f2590846e8b2bbabf7e1b5182a"),
                    Helper.ToRef<BlueprintAbilityReference>("0bcf1c4c9e0144b6acdd6c82a590d9be"),
                    Helper.ToRef<BlueprintAbilityReference>("2dea7d6d304e49418b446d543ec557c8"),
                    Helper.ToRef<BlueprintAbilityReference>("cb8c6e1c78e29444285e6fd97d9ef6ee"),
                },
            };
            FanOfFlames = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("fde466d2c24705641bcd97d04a323566"),
                Buff = null,
                Variants = new()
                {
                    Helper.ToRef<BlueprintAbilityReference>("a240a6d61e1aee040bf7d132bfe1dc07"),
                    Helper.ToRef<BlueprintAbilityReference>("e3b3c7747e14f54458d27163f19761ae"),
                },
            };
            Fragmentation = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("88ae936abf296894695a282f49214718"),
                Buff = null,
                RequiresRangedAttackRoll = true,
                Variants = new()
                {
                    Helper.ToRef<BlueprintAbilityReference>("d859e796f6177cf449679c677076c577"),
                    Helper.ToRef<BlueprintAbilityReference>("3cf0a759bc612264fb9b03aa2f90b24b"),
                    Helper.ToRef<BlueprintAbilityReference>("415ce928decc2ac4fa551be49de86ceb"),
                },
            };
            Spindle = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("c4f4a62a325f7c14dbcace3ce34782b5"),
                Buff = null,
                Variants = new()
                {
                    Helper.ToRef<BlueprintAbilityReference>("a28e54e4e5fafd1449dd9e926be85160"),
                    Helper.ToRef<BlueprintAbilityReference>("44d37b2230390b24e8060fe821068984"),
                    Helper.ToRef<BlueprintAbilityReference>("7021bbe4dca437440a41da4552dce28e"),
                    Helper.ToRef<BlueprintAbilityReference>("e2f045c512a146eab57b0f0fcaf5fe71"),
                    Helper.ToRef<BlueprintAbilityReference>("aedaf82fe4f34b4d8f101fb142794c59"),
                    Helper.ToRef<BlueprintAbilityReference>("deeacc16df744fa6828804ed1afcc466"),
                    Helper.ToRef<BlueprintAbilityReference>("b333573557f496746b754d0af246c0fe"),
                    Helper.ToRef<BlueprintAbilityReference>("6f299bc4320299c49a291f43a667496d"),
                    Helper.ToRef<BlueprintAbilityReference>("920e4edc2df510444b016dd18038f2b7"),
                    Helper.ToRef<BlueprintAbilityReference>("0b43513ea6144f4bbe505fe9e09ba648"),
                    Helper.ToRef<BlueprintAbilityReference>("2aa9c451dc674681a5fcf8db26d0dc01"),
                    Helper.ToRef<BlueprintAbilityReference>("ff829a11544db914d89761c676397ef8"),
                    Helper.ToRef<BlueprintAbilityReference>("1e2cff4d83b74ca468d4cea21665db2e"),
                    Helper.ToRef<BlueprintAbilityReference>("ecb202fa5e1d0c84095b6604a62884cb"),
                    Helper.ToRef<BlueprintAbilityReference>("ad985f8975b9986409eae00ea87225ca"),
                    Helper.ToRef<BlueprintAbilityReference>("e4624a8398c3bdc44bbcbf2fb20fae47"),
                    Helper.ToRef<BlueprintAbilityReference>("49246be3e43efc845a5c7ba5d6d5a353"),
                    Helper.ToRef<BlueprintAbilityReference>("300bcc4bac44b4a489c919590256b625"),
                    Helper.ToRef<BlueprintAbilityReference>("bcce0961438aa524ebf0d6992c5bede1"),
                    Helper.ToRef<BlueprintAbilityReference>("680fe1162cff5294a8375f6eb32652ce"),
                    Helper.ToRef<BlueprintAbilityReference>("6d91306bce5524c4090c417efe7c538f"),
                    Helper.ToRef<BlueprintAbilityReference>("ec3741322559fc449ad1ace45d1ec58a"),
                    Helper.ToRef<BlueprintAbilityReference>("47cd259ec778444bab253540f4a3e428"),
                    Helper.ToRef<BlueprintAbilityReference>("eb53604802d44af1acd2039b0957b3e8"),
                    Helper.ToRef<BlueprintAbilityReference>("27eef6ef208c4b5ca6da357e4e9ecb6c"),
                    Helper.ToRef<BlueprintAbilityReference>("22c6f2a6c1ae4d7597f95aa3608f4684"),
                    Helper.ToRef<BlueprintAbilityReference>("e84f5f51a0714665a4474c509617f2bf"),
                    Helper.ToRef<BlueprintAbilityReference>("c6c92fc3541b4d6fbcb6b25a1d4bcc5d"),
                    Helper.ToRef<BlueprintAbilityReference>("3442485bd9cdfeb4fb7faf1984dec5bb"),
                },
            };
            Spray = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("b5852e8287f12d34ca6f84fcc7019f07"),
                Buff = null,
                Variants = new()
                {
                    Helper.ToRef<BlueprintAbilityReference>("963da934d652bdc41900ed68f63ca1fa"),
                    Helper.ToRef<BlueprintAbilityReference>("53b701d71c0cde64e887f3b81a094682"),
                    Helper.ToRef<BlueprintAbilityReference>("48ae2d5a6105bdb4abb9c23a3809f1c1"),
                },
            };
            Torrent = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("2aad85320d0751340a0786de073ee3d5"),
                Buff = null,
                Variants = new()
                {
                    Helper.ToRef<BlueprintAbilityReference>("51ede1faa3cdb3b47a46f7579ca02b0a"),
                    Helper.ToRef<BlueprintAbilityReference>("93cc42235edc6824fa7d54b83ed4e1fe"),
                    Helper.ToRef<BlueprintAbilityReference>("a87fd82362ff7d247b998e68eecc087b"),
                    Helper.ToRef<BlueprintAbilityReference>("5e4c7cb990de4034bbee9fb99be2e15d"),
                    Helper.ToRef<BlueprintAbilityReference>("459dfd4225ac2fe48bdcb401b0f1dcc0"),
                    Helper.ToRef<BlueprintAbilityReference>("82db79a0b4e91dc4ea2938192e6fc7af"),
                    Helper.ToRef<BlueprintAbilityReference>("c073af2846b8e054fb28e6f72bc02749"),
                    Helper.ToRef<BlueprintAbilityReference>("d02fba9ae78f12642b4111a4bbbdc023"),
                    Helper.ToRef<BlueprintAbilityReference>("2ae4a1c73e8c6ca4d8b20d0e6eb730bd"),
                    Helper.ToRef<BlueprintAbilityReference>("32a018b283bc9c3428ec66b745bd0b27"),
                    Helper.ToRef<BlueprintAbilityReference>("3bbc16ca68378af4f88d33dbd364a9d9"),
                    Helper.ToRef<BlueprintAbilityReference>("4d2e60cfd9902724d999758551020288"),
                    Helper.ToRef<BlueprintAbilityReference>("e5b1f4d8995f3f0489a4fed250a178a0"),
                    Helper.ToRef<BlueprintAbilityReference>("cc514f4604da850409f1af291e848e3a"),
                },
            };
            Wall = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("c684335918896ce4ab13e96cec929796"),
                Buff = null,
                Variants = new()
                {
                    Helper.ToRef<BlueprintAbilityReference>("d0390bd9ff12cd242a40c384445546cd"),
                    Helper.ToRef<BlueprintAbilityReference>("f493e7b18b2a22c438df7ced760dd5b0"),
                    Helper.ToRef<BlueprintAbilityReference>("1ab8c76ac4983174dbffa35e2a87e582"),
                    Helper.ToRef<BlueprintAbilityReference>("180bcce7ae384a0da0d51b69b290ef08"),
                    Helper.ToRef<BlueprintAbilityReference>("d5d9e3fe29314b0db4a1b17277f78300"),
                    Helper.ToRef<BlueprintAbilityReference>("2a5da3077b8b44ccb2e5ac05ebaa3b81"),
                    Helper.ToRef<BlueprintAbilityReference>("139558a1389f7034e88dca5bfa6d4d3b"),
                    Helper.ToRef<BlueprintAbilityReference>("19309b5551a28d74288f4b6f7d8d838d"),
                    Helper.ToRef<BlueprintAbilityReference>("c8dda5accb6354b40aa3618484e91029"),
                    Helper.ToRef<BlueprintAbilityReference>("7f4db78377f44a58b7d0f2d8aba46c55"),
                    Helper.ToRef<BlueprintAbilityReference>("4fe700383def4425bd6233d05bc16900"),
                    Helper.ToRef<BlueprintAbilityReference>("6551795d81a0e744ebc5785c1264b788"),
                    Helper.ToRef<BlueprintAbilityReference>("0436c08f986a9064aa7b9117d5ab97a3"),
                    Helper.ToRef<BlueprintAbilityReference>("9652dec009183db4d8c29c6a196200e8"),
                    Helper.ToRef<BlueprintAbilityReference>("11cb007605def4546a596bd582f746fc"),
                    Helper.ToRef<BlueprintAbilityReference>("a34c55992021031438ca3f1a0406a9ef"),
                    Helper.ToRef<BlueprintAbilityReference>("bbe6903c268f1104692c6d62d3e4858e"),
                    Helper.ToRef<BlueprintAbilityReference>("05822aa2552ba01459fa32614fbc4631"),
                    Helper.ToRef<BlueprintAbilityReference>("c25f56632bd43e240b4349fef841efa2"),
                    Helper.ToRef<BlueprintAbilityReference>("d7f06e36bff449d468ce8a1621b494a3"),
                    Helper.ToRef<BlueprintAbilityReference>("63e2e1ad84b436e4cbb2c6d1c5ebf041"),
                    Helper.ToRef<BlueprintAbilityReference>("97e1009a2e708eb4cb8b79bab253d32a"),
                    Helper.ToRef<BlueprintAbilityReference>("0990b364196b4fc1ac8b2de6a61c868e"),
                    Helper.ToRef<BlueprintAbilityReference>("566c6ea88fb64b2d9bfc679e00bb8ec9"),
                    Helper.ToRef<BlueprintAbilityReference>("66e0eaf00fe046daaf8a9c965c767d61"),
                    Helper.ToRef<BlueprintAbilityReference>("a477ff1705424d759d9bd94a09f8db49"),
                    Helper.ToRef<BlueprintAbilityReference>("fa35dacfe2554d7fb757df051b0e27cd"),
                    Helper.ToRef<BlueprintAbilityReference>("370794259a9a4ca8828dca53140672bd"),
                    Helper.ToRef<BlueprintAbilityReference>("70cefed4b3754a9eb26407bd831d3e0e"),
                    Helper.ToRef<BlueprintAbilityReference>("f185a0cf96cb6034f8fbcd9b349382af"),
                },
            };
            Wrack = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("9e7f94f8d42a74240b9329f5e68121ee"),
                Buff = null,
                RequiresRangedAttackRoll = true,
            };
            Bowling = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("b3bd080eed83a9940abd97e4aa2a7341"),
                Buff = Helper.ToRef<BlueprintBuffReference>("918b2524af5c3f647b5daa4f4e985411"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("88c37d8a7d808a844ba0116dd37e4059"),
            };
            Chilling = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("6ac87a3af9ccf014787c49745df75e6a"),
                Buff = Helper.ToRef<BlueprintBuffReference>("49fc69c05ff7c5d46b61745d361a72fb"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("fb426ea002abbbc4198b1cd6b99f1be8"),
            };
            Dazzling = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("037460f7ae3e21943b237007f2b1a5d5"),
                Buff = Helper.ToRef<BlueprintBuffReference>("ee8d9f5631c53684d8d627d715eb635c"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("abf5c26910fda5949abbc285c60416f9"),
            };
            Entangling = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("607539d018d03454aaac0a2c1522f7ac"),
                Buff = Helper.ToRef<BlueprintBuffReference>("738120aad01eedb4f891eca5b784646a"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("091b297f43ac5be43af31979c00ade57"),
            };
            Flash = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("37f3cfca29073e142a80c3b8e7c54b05"),
                Buff = Helper.ToRef<BlueprintBuffReference>("50cf40b1cb3115546a3e9b44d7687384"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("323be9d573657374da4e3f1456a2366c"),
            };
            Foxfire = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("ae21c5369252ec74aa1fee89f1bc1b21"),
                Buff = Helper.ToRef<BlueprintBuffReference>("e671f173fcb75bf4aa78a4078d075792"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("d0007fed20710ae4a96cebd2ba99f08b"),
            };
            Grappling = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("e2d70b95e80549b439d30df29a79cb58"),
                Buff = Helper.ToRef<BlueprintBuffReference>("f69a66c0feaa4374b8ca2732ee91a373"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("2816fad233e15a54c86729cee6e8969d"),
            };
            Magnetic = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("d6b95ac99e3004b499d750835864e053"),
                Buff = Helper.ToRef<BlueprintBuffReference>("696a0eafc6a21334580174a461079841"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("b2d91bac690b74140b4fa3eec443edee"),
            };
            PureFlame = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("1dfd9b8e1439e4a4ab4b6b11f5ea676a"),
                Buff = Helper.ToRef<BlueprintBuffReference>("1b9f7db78467ff34ab2e1c0f86cdaa77"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("06e3ac0ec6341744eb87f1f70a11576b"),
            };
            Pushing = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("fbb97f35a41b71c4cbc36c5f3995b892"),
                Buff = Helper.ToRef<BlueprintBuffReference>("f795bede8baefaf4d9d7f404ede960ba"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("bc5665a318bc4eb46a0537455509851a"),
            };
            RareMetal = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("6700ce7f56ddc30488e45b049f4ee475"),
                Buff = Helper.ToRef<BlueprintBuffReference>("417086d2c99b60f4c911de6712bc76a7"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("097c209e378144045ab97f4d54876959"),
            };
            Synaptic = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("53c80f136f2bf65409d358f28b0c5bb4"),
                Buff = Helper.ToRef<BlueprintBuffReference>("67fc7492f198c8d4aace14d28e0ad438"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("db3ccc72faeac0343891ba71bb692a42"),
            };
            Unraveling = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("79339d57d491d824ba0aa4ed0c114b2f"),
                Buff = Helper.ToRef<BlueprintBuffReference>("cebd08ab72f1baa4eaacdd836207873a"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("59303d0eb693cd2438fc89f91e29ab19"),
            };

            #endregion

            #region Infusions Modded

            // Modded: DarkCodex
            BladeRush = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("350b164b8fd942f4a37af9fe31d6b97a"),
                Buff = null,
                RequiresMeleeAttackRoll = true,
                Activator = Helper.ToRef<BlueprintAbilityReference>("e6b5691631c7417696af64bdef75f706"),
            };
            Whip = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("00df2fabfc3f40a4aaf4a066dcd78b80"),
                Buff = Helper.ToRef<BlueprintBuffReference>("26983c7ac7f1465fab8ec646f366b9f7"),
                RequiresMeleeAttackRoll = true,
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("a95f76e9cb344bef8d8bc3839d1a75dd"),
            };
            Chain = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("4b6884729a46432ea9b5e1a873e8efa6"),
                Buff = null,
                RequiresRangedAttackRoll = true,
                Variants = new()
                {
                    Helper.ToRef<BlueprintAbilityReference>("2279885fa7be4cd98fff498f99cf4699"),
                    Helper.ToRef<BlueprintAbilityReference>("00c9bd3f9b9e489cae0f27f79047b131"),
                },
            };
            Impale = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("611f666629f7451c98618d62b16ed62e"),
                Buff = null,
                RequiresMeleeAttackRoll = true,
                Variants = new()
                {
                    Helper.ToRef<BlueprintAbilityReference>("b699d93443d843aeb00ecf4fa8f52f0e"),
                    Helper.ToRef<BlueprintAbilityReference>("ce9903778fde46bfa63b88a29ce543fa"),
                    Helper.ToRef<BlueprintAbilityReference>("beae9019794349d7a5263c19f1800d74"),
                },
            };
            Venom = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("637329bf936244668c55d81985c4eaf8"),
                Buff = Helper.ToRef<BlueprintBuffReference>("41b2d6e118034bd0bf3edc214a895d70"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("0d66e6c2ab8d4aa38b8e4aa5e0795120"),
            };
            VenomGreater = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("e9021412e87847b3b104d4d31bfe5403"),
                Buff = Helper.ToRef<BlueprintBuffReference>("ec75b0fc5e484c1eabb1fd15eb204b99"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("7c825af055e04ccf87916838800cf18b"),
            };
            KineticFist = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("5e3db32b4e244abfa642ee03276d88ed"),
                Buff = Helper.ToRef<BlueprintBuffReference>("07e18f24bda7444a9b34405227b31b60"),
                RequiresMeleeAttackRoll = true,
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("a909d31742af4c7788c0e401de191617"),
            };
            EnergizeWeapon = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("fb9fe27f13934807bcd62dfeec477758"),
                Buff = Helper.ToRef<BlueprintBuffReference>("f5fbde9e5fbe4973bc421315c723d9fe"),
                RequiresMeleeAttackRoll = true,
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("5572beb8ca8c4efaa43c514190fe2f13"),
            };
            HurricaneQueen = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("f90ab62ccb6349a3be43adbdacb34dff"),
                Activator = Helper.ToRef<BlueprintFeatureReference>("bbba1600582cf8446bb515a33bd89af8"),
            };
            MindShield = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("c15c4e5e91804f2abd19ff7628fc62e2"),
            };

            // Modded: Kineticist Elements Expanded
            Disintegrating = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("96360bedde8648a8a6762e2de41b60a5"),
                Buff = Helper.ToRef<BlueprintBuffReference>("321e49800199496e829f6876d34fce47"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("fc07e1226c0e46f98ec1d49e6a68086d"),
            };
            ForceHook = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("000706ddb53e468a926a3c36e1889213"),
                Buff = null,
                RequiresRangedAttackRoll = true,
                Variants = new()
                {
                    Helper.ToRef<BlueprintAbilityReference>("8e8a85d2adc541e98b35f43555238b27"),
                },
            };
            FoeThrow = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("cba7fb8cef0c4160b500850d0c58d1d9"),
                Buff = null,
                RequiresRangedAttackRoll = true,
            };
            ManyThrow = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("ae785f510e4c4ed2991b59b421c0a2e5"),
                Buff = null,
                RequiresRangedAttackRoll = true,
                Variants = new()
                {
                    Helper.ToRef<BlueprintAbilityReference>("b32b471e39b3464d87a00ad2a5483b72"),
                },
            };
            Dampening = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("e9cf588e2ef64fb68d0ec8c566e8b294"),
                Buff = Helper.ToRef<BlueprintBuffReference>("611d7a60d13943249bc706dc965cdce0"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("bf2b31a8c97a4ea5b3048ccc01d9def3"),
            };
            Enervating = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("8662ccb8dd484a2f8139d46621c641fd"),
                Buff = Helper.ToRef<BlueprintBuffReference>("94fc7da86b1e4049974f46580dbecb9f"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("8fb731ddd0a1426582b7916509defd35"),
            };
            Pulling = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("3ae954ad56a2497b92fada3dc493b4e1"),
                Buff = Helper.ToRef<BlueprintBuffReference>("28db04531698400a98da7fb965a519e7"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("36ddb2e7184343a49992e65a27a89412"),
            };
            Unnerving = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("e53160d091914d50bfc1d8d4fa482e30"),
                Buff = Helper.ToRef<BlueprintBuffReference>("d5c72f8725f74109abb4c0ca516da805"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("f40c87a27401428e9133f757d157f6f2"),
            };
            VoidVampire = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("922fd10e3e994d7793821da1583cdfea"),
                Buff = Helper.ToRef<BlueprintBuffReference>("d6c523157463453aa40a8b8bbfd40447"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("d9e3b600a10e4edc97fb3a1702831b4a"),
            };
            Weighing = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("987dc633dbae49b0adc11cd9c5672553"),
                Buff = Helper.ToRef<BlueprintBuffReference>("98147fa8bb4049ab9e7a6c8b55eb47bf"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("0bde03bda7ba49c1a72e960857a64b9c"),
            };
            Singularity = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("2a8b8823924245aa9c9494679b311866"),
                Buff = null,
                Variants = new()
                {
                    Helper.ToRef<BlueprintAbilityReference>("13ef7dd6d6cf404f86d7c1bedf17df06"),
                    Helper.ToRef<BlueprintAbilityReference>("55649aab43a84b618faebfee15420292"),
                    Helper.ToRef<BlueprintAbilityReference>("489e9e94b39c4bd09aa657884ea0a331"),
                },
            };
            Spore = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("1cd0cd60997d4288be1fc85f753e53de"),
                Buff = Helper.ToRef<BlueprintBuffReference>("dcf5ebb5dbd34f5dbea8c3699ce0054b"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("6b7f98a52f54418490988d0fc0d67cf5"),
            };
            Toxic = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("fdc63fd61b794e40ba3c5446ba8ea1c2"),
                Buff = Helper.ToRef<BlueprintBuffReference>("d337687cefcb4c7e80898b0008ce4e63"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("bcb32c5793974426a6da4fbdd8d780f1"),
            };
            ToxicGreater = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("56567887f604473797dc8223c68999da"),
                Buff = Helper.ToRef<BlueprintBuffReference>("848ab4144cb14b4499167bcbfd372796"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("189ce662afdb441baa45716d909ce9ed"),
            };
            Vampiric = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("66c71846bbb626a4ab73cef60f1c8bbf"),
                Buff = Helper.ToRef<BlueprintBuffReference>("e50e653cff511cd49a55b979346699f1"),
                Activator = Helper.ToRef<BlueprintActivatableAbilityReference>("d9e3b600a10e4edc97fb3a1702831b4a"),
            };

            #endregion

            #region Utility

            AerialAdaptation = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("c8719b3c5c0d4694cb13abcc3b7e893b"),
                Activator = Helper.ToRef<BlueprintBuffReference>("de4f4b6aa7f62204c95f5d03ac0bc459"),
            };
            AerialEvasion = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("871e6b0bb9d050743b98b78990ae1cff"),
                Activator = Helper.ToRef<BlueprintAbilityReference>("41281aa38b6b27f4fa3a05c97cc01783"),
            };
            Celerity = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("b182f559f572da342b54bece4404e4e7"),
                Activator = Helper.ToRef<BlueprintAbilityReference>("4e85d35a76525e1439aeb8086af53809"),
            };
            ColdAdaptation = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("1ff5d6e76b7c2fa48be555b77d1ad8b2"),
                Activator = Helper.ToRef<BlueprintBuffReference>("04a414c3d1585a74e9d3f23addc465cc"),
            };
            ElementalWhispersSelection = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("f02525006521bee4eb90ab26b7b9db24"),
            };
            EnduringEarth = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("14359e9c35a42ee48b8c31b4424a8d3f"),
            };
            ExpandedDefense = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("d741f298dfae8fc40b4615aaf83b6548"),
            };
            FiresFury = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("641d15b6c3d3017409e352c558fb0090"),
            };
            FlameShield = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("733bbf836ec575b44b3c88b61a26282e"),
                Activator = Helper.ToRef<BlueprintAbilityReference>("c3a13237b17de5742a2dbf2da46f23d5"),
            };
            FoxfireUtility = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("14c699ccd0564e04a9587b1845d16014"),
                Activator = Helper.ToRef<BlueprintAbilityReference>("27f0127528bd96f44897987f339ae282"),
            };
            HealingBurst = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("c73b37aaa2b82b44686c56db8ce14e7f"),
                Activator = Helper.ToRef<BlueprintAbilityReference>("db611ffeefb8f1e4f88e7d5393fc651d"),
            };
            HeatAdaptation = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("2825e3a53c76ad944a47c5c44fb6109f"),
                Activator = Helper.ToRef<BlueprintBuffReference>("1a74716a5b0217a4eb922856c01e80cc"),
            };
            JaggedFlesh = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("94064ed53b1020247941ac70313b439d"),
                Activator = Helper.ToRef<BlueprintAbilityReference>("b448a9c0ff27b6846b9c676f6839e907"),
            };
            KineticHealer = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("3ef666973adfa8f40af6c0679bd98ba5"),
                Activator = Helper.ToRef<BlueprintAbilityReference>("eff667a3a43a77d45a193bb7c94b3a6c"),
            };
            KineticRestoration = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("ed01d50910ae67b4dadc050f16d93bdf"),
                Activator = Helper.ToRef<BlueprintAbilityReference>("1dc60bdbf5843f342aaa5e838b66e43a"),
            };
            KineticRevification = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("0377fcf4c10871f4187809d273af7f5d"),
                Activator = Helper.ToRef<BlueprintAbilityReference>("0e370822d9e0ff54f897e7fdf24cffb8"),
            };
            SkilledKineticist = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("fd13e9efee08db448974fe0263eb96c8"),
                Activator = Helper.ToRef<BlueprintBuffReference>("56b70109d78b0444cb3ad04be3b1ee9e"),
            };
            Slick = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("1d42456e6113739499e1bda025e0ba03"),
                Activator = Helper.ToRef<BlueprintAbilityReference>("5f3cfbd441529df4f85a97075f299b41"),
            };
            TidalWave = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("7c4bbfe3b089a8a4ebcd2401995230a4"),
                Activator = Helper.ToRef<BlueprintAbilityReference>("d8d451ed3c919a4438cde74cd145b981"),
            };
            Tremorsense = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("6e668702fdc53c343a0363813683346e"),
            };
            WildTalentBonusFeatAir = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("4ff45d291d0ee9c4b8c83a298b0b4969"),
            };
            WildTalentBonusFeatAir1 = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("95ab2bdf0d45b2742a357f5780aac4a3"),
            };
            WildTalentBonusFeatAir2 = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("a8a481c7fbcc9c446a0eecb6e5604405"),
            };
            WildTalentBonusFeatAir3 = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("a82e3b11fc5935d4289c807b241a2bb5"),
            };
            WildTalentBonusFeatEarth = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("28bd446d8aeab1341acc8d2fba91e455"),
            };
            WildTalentBonusFeatEarth1 = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("fc75b0bcfcb5236419d1a47e1bc555a9"),
            };
            WildTalentBonusFeatEarth2 = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("2205fc9ed34368548b1358a781326bab"),
            };
            WildTalentBonusFeatEarth3 = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("f593346da04badb4185a47af8e4c4f7f"),
            };
            WildTalentBonusFeatFire = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("4d14baf0ee4da2a4cb05fb4312921ee4"),
            };
            WildTalentBonusFeatFire1 = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("0e0207491a09a9e428409c4a1b2871a3"),
            };
            WildTalentBonusFeatFire2 = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("cd107bf355f84b64f9f472ca288c208b"),
            };
            WildTalentBonusFeatFire3 = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("0213c0c9062203540bd0365cbde44b99"),
            };
            WildTalentBonusFeatWater = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("31c18eae013b09c4f9ee51da71a2d61c"),
            };
            WildTalentBonusFeatWater1 = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("b6f42b59d000228498445526042dfd1b"),
            };
            WildTalentBonusFeatWater2 = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("b881e2d840eaf6044b0d243b239cccd7"),
            };
            WildTalentBonusFeatWater3 = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("1d341cc7535e64b4b8e2c53fb6726394"),
            };
            WildTalentBonusFeatWater4 = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("ebf90f9f8a5e43f40bee85fd6506b922"),
            };
            WildTalentBonusFeatWater5 = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("40a4fb42aafa7ee4991d3e3140e98856"),
            };
            SkillFocusSelection = new()
            {
                Feature = Helper.ToRef<BlueprintFeatureReference>("c9629ef9eebb88b479b2fbc5e836656a"),
            };

            #endregion

            BaseBasic = GetAll(true, false).Select(s => s.BaseAbility).ToArray();
            BaseComposite = GetAll(false, true).Select(s => s.BaseAbility).ToArray();
            BaseAll = GetAll(true, true, archetype: true).SelectMany(func1).ToArray();
            IEnumerable<BlueprintAbilityReference> func1(Element element)
            {
                yield return element.BaseAbility;
                yield return element.Blade.Burn;
            }
        }

        /// <summary>
        /// Get filtered collection of all elements.
        /// </summary>
        /// <param name="basic">Whenever to include basic elements (granted at level 1).</param>
        /// <param name="composite">Whenever to include composite elements (granted at level 7 or 15).</param>
        /// <param name="boost">This aren't elements. Recommend false.</param>
        /// <param name="onlyPhysical">Remove energy based elements.</param>
        /// <param name="onlyEnergy">Remove physical based elements.</param>
        /// <param name="archetype">Whenever to include blood archetype.</param>
        /// <param name="modded">Whenever to include modded elements. This can return blueprints not yet loaded!</param>
        /// <returns></returns>
        public IEnumerable<Element> GetAll(bool basic = false, bool composite = false, bool boost = false, bool onlyPhysical = false, bool onlyEnergy = false, bool archetype = false, bool modded = true)
        {
            bool mod1 = modded && EnabledElementsExpanded;

            if (basic)
            {
                if (!onlyEnergy)
                {
                    yield return Air;
                    yield return Earth;
                    yield return Water;
                    if (mod1)
                    {
                        yield return Telekinetic;
                        yield return Gravity;
                        yield return Wood;
                    }
                }
                if (!onlyPhysical)
                {
                    yield return Electric;
                    yield return Fire;
                    yield return Cold;
                    if (mod1)
                    {
                        yield return Negative;
                        yield return Positive;
                    }
                }
            }
            if (composite)
            {
                if (!onlyEnergy)
                {
                    yield return Composite_Metal;
                    yield return Composite_Plasma;
                    yield return Composite_Sand;
                    yield return Composite_Thunder;
                    yield return Composite_Blizzard;
                    yield return Composite_Ice;
                    yield return Composite_Magma;
                    yield return Composite_Mud;
                    yield return Composite_ChargedWater;
                    yield return Composite_Steam;
                    if (archetype)
                        yield return Composite_Blood;
                    if (mod1)
                    {
                        yield return Composite_Force;
                        yield return Composite_Void;
                        yield return Composite_Verdant;
                        yield return Composite_Spring;
                        yield return Composite_Summer;
                        yield return Composite_Autumn;
                        yield return Composite_Winter;
                    }
                }
                if (!onlyPhysical)
                {
                    yield return Composite_BlueFlame;
                }
            }
            if (boost) 
            {
                if (!onlyEnergy)
                {
                    if (mod1)
                    {
                        yield return Boost_Gravitic;
                        yield return Boost_GraviticGreater;
                    }
                }
                if (!onlyPhysical)
                {
                    if (mod1)
                    {
                        yield return Boost_NegativeAdmixture;
                        yield return Boost_PositiveAdmixture;
                    }
                }
                if (mod1)
                {
                    yield return Boost_Aetheric;
                    yield return Boost_AethericGreater;
                }
            }
        }

        public IEnumerable<Focus> GetFocus(bool modded = true)
        {
            yield return FocusAir;
            yield return FocusEarth;
            yield return FocusFire;
            yield return FocusWater;
            if (modded && EnabledElementsExpanded)
            {
                yield return FocusAether;
                yield return FocusVoid;
                yield return FocusWood;
            }
        }

        public Focus GetFocus(Func<Focus, bool> predicate)
        {
            return GetFocus().FirstOrDefault(predicate);
        }

        public Focus GetFocus(AnyRef feature)
        {
            foreach (var focus in GetFocus())
            {
                if (feature.Equals(focus.First) || feature.Equals(focus.Second) || feature.Equals(focus.Third) || feature.Equals(focus.Knight))
                    return focus;
            }
            return null;
        }

        /// <summary>
        /// Returns composite of two elements or no elements, if no composite matches.
        /// </summary>
        public IEnumerable<Element> GetComposites(Element element, Element element2 = null)
        {
            foreach (var e in GetAll(false, true))
            {
                if (e.Parent1 == element && (element2 == null || element2 == e.Parent2))
                    yield return e;
                else if (e.Parent2 == element && (element2 == null || element2 == e.Parent1))
                    yield return e;
            }
        }

        [Obsolete("Use GetTalents(form: true, substance: true) instead")]
        private IEnumerable<BlueprintFeature> GetInfusionTalents()
        {
            foreach (var talent in SelectionInfusion.Get().m_AllFeatures)
            {
                var a = talent.Get();
                if (a != null)
                    yield return a;
            }
        }

        [Obsolete("Use GetTalents(utility: true) instead")]
        private IEnumerable<BlueprintFeature> GetWildTalents()
        {
            foreach (var talent in SelectionWildTalent.Get().m_AllFeatures)
            {
                var a = talent.Get();
                if (a != null)
                    yield return a;
            }
        }

        public IEnumerable<Infusion> GetTalents(bool form = false, bool substance = false, bool utility = false, bool modded = true)
        {
            if (form)
            {
                yield return KineticBlade;
                yield return BladeWhirlwind;
                yield return Cloud;
                yield return Cyclone;
                yield return DeadlyEarth;
                yield return Detonation;
                yield return Eruption;
                yield return ExtendedRange;
                yield return FanOfFlames;
                yield return Fragmentation;
                yield return Spindle;
                yield return Spray;
                yield return Torrent;
                yield return Wall;
            }
            if (substance)
            {
                yield return Bleeding;
                yield return Wrack;
                yield return Bowling;
                yield return Chilling;
                yield return Dazzling;
                yield return Entangling;
                yield return Flash;
                yield return Foxfire;
                yield return Grappling;
                yield return Magnetic;
                yield return PureFlame;
                yield return Pushing;
                yield return RareMetal;
                yield return Synaptic;
                yield return Unraveling;
            }
            if (utility)
            {
                yield return AerialAdaptation;
                yield return AerialEvasion;
                yield return Celerity;
                yield return ColdAdaptation;
                yield return ElementalWhispersSelection;
                yield return EnduringEarth;
                yield return ExpandedDefense;
                yield return FiresFury;
                yield return FlameShield;
                yield return FoxfireUtility;
                yield return HealingBurst;
                yield return HeatAdaptation;
                yield return JaggedFlesh;
                yield return KineticHealer;
                yield return KineticRestoration;
                yield return KineticRevification;
                yield return SkilledKineticist;
                yield return Slick;
                yield return TidalWave;
                yield return Tremorsense;
                yield return WildTalentBonusFeatAir;
                yield return WildTalentBonusFeatAir1;
                yield return WildTalentBonusFeatAir2;
                yield return WildTalentBonusFeatAir3;
                yield return WildTalentBonusFeatEarth;
                yield return WildTalentBonusFeatEarth1;
                yield return WildTalentBonusFeatEarth2;
                yield return WildTalentBonusFeatEarth3;
                yield return WildTalentBonusFeatFire;
                yield return WildTalentBonusFeatFire1;
                yield return WildTalentBonusFeatFire2;
                yield return WildTalentBonusFeatFire3;
                yield return WildTalentBonusFeatWater;
                yield return WildTalentBonusFeatWater1;
                yield return WildTalentBonusFeatWater2;
                yield return WildTalentBonusFeatWater3;
                yield return WildTalentBonusFeatWater4;
                yield return WildTalentBonusFeatWater5;
                yield return SkillFocusSelection;
            }

            // DarkCodex
            if (modded && EnabledDarkCodex)
            {
                if (form)
                {
                    yield return BladeRush;
                    yield return Whip;
                    yield return Chain;
                    yield return Impale;
                    yield return KineticFist;
                    yield return EnergizeWeapon;
                }
                if (substance)
                {
                    yield return Venom;
                    yield return VenomGreater;
                }
                if (utility)
                {
                    yield return HurricaneQueen;
                    yield return MindShield;
                }
            }

            // Kineticist Elements Expanded
            if (modded && EnabledElementsExpanded)
            {
                if (form)
                {
                    yield return Singularity;
                    yield return FoeThrow;
                    yield return ManyThrow;
                    yield return ForceHook;
                }
                if (substance)
                {
                    yield return Disintegrating;
                    yield return Dampening;
                    yield return Enervating;
                    yield return Pulling;
                    yield return Unnerving;
                    yield return VoidVampire;
                    yield return Weighing;
                    yield return Spore;
                    yield return Toxic;
                    yield return ToxicGreater;
                    yield return Vampiric;
                }
                if (utility)
                {

                }
            }
        }

        /// <summary>Only returns non null values.</summary>
        public IEnumerable<BlueprintAbility> GetBlasts(bool bases = false, bool variants = false, bool bladeburn = false, bool bladedamage = false)
        {
            foreach (var element in GetAll(basic: true, composite: true, archetype: true))
            {
                var b = element.BaseAbility.Get();
                if (b == null)
                    continue;

                if (bases || variants && !b.HasVariants)
                    yield return b;

                if (variants && b.HasVariants)
                    foreach (var variant in b.AbilityVariants.Variants)
                        yield return variant;

                if (bladeburn && element.Blade.Burn.NotEmpty())
                    yield return element.Blade.Burn;

                if (bladedamage && element.Blade.Damage.NotEmpty())
                    yield return element.Blade.Damage;
            }
        }

        public bool EnabledDarkCodex;
        public bool EnabledElementsExpanded;

        public BlueprintCharacterClassReference @Class;
        public BlueprintArchetypeReference KineticKnight;
        public BlueprintArchetypeReference ElementalScion;
        public BlueprintArchetypeReference ElementalAscetic;
        public BlueprintFeatureReference KineticBlast;
        public BlueprintUnitPropertyReference KineticistMainStatProperty;

        public BlueprintFeatureSelectionReference FocusFirst;
        public BlueprintFeatureSelectionReference FocusSecond;
        public BlueprintFeatureSelectionReference FocusThird;
        public BlueprintFeatureSelectionReference FocusKnight;
        public BlueprintFeatureSelectionReference SelectionInfusion;
        public BlueprintFeatureSelectionReference SelectionWildTalent;
        public BlueprintFeatureSelectionReference ExpandedElement;
        public BlueprintFeatureSelectionReference ExtraWildTalent;
        public BlueprintBuffReference CompositeBuff;
        public List<BlueprintBuff> MetakinesisBuffs;

        public BlueprintAbilityReference[] BaseBasic;
        public BlueprintAbilityReference[] BaseComposite;
        public BlueprintAbilityReference[] BaseAll;

        public Focus FocusAir;
        public Focus FocusEarth;
        public Focus FocusFire;
        public Focus FocusWater;

        public Element Air;
        public Element Electric;
        public Element Earth;
        public Element Fire;
        public Element Water;
        public Element Cold;

        public Element Composite_Metal;
        public Element Composite_BlueFlame;
        public Element Composite_Plasma;
        public Element Composite_Sand;
        public Element Composite_Thunder;
        public Element Composite_Blizzard;
        public Element Composite_Ice;
        public Element Composite_Magma;
        public Element Composite_Mud;
        public Element Composite_ChargedWater;
        public Element Composite_Steam;
        public Element Composite_Blood;

        // modded
        public Focus FocusAether;
        public Element Telekinetic;
        public Element Composite_Force;
        public Boost Boost_Aetheric;
        public Boost Boost_AethericGreater;

        public Focus FocusVoid;
        public Element Gravity;
        public Element Negative;
        public Element Composite_Void;
        public Boost Boost_Gravitic;
        public Boost Boost_GraviticGreater;
        public Boost Boost_NegativeAdmixture;

        public Focus FocusWood;
        public Element Wood;
        public Element Positive;
        public Element Composite_Verdant;
        public Element Composite_Autumn;
        public Element Composite_Spring;
        public Element Composite_Summer;
        public Element Composite_Winter;
        public Boost Boost_PositiveAdmixture;

        public Infusion BladeWhirlwind;
        public Infusion KineticBlade;
        public Infusion Bleeding;
        public Infusion Cloud;
        public Infusion Cyclone;
        public Infusion DeadlyEarth;
        public Infusion Detonation;
        public Infusion Eruption;
        public Infusion ExtendedRange;
        public Infusion FanOfFlames;
        public Infusion Fragmentation;
        public Infusion Spindle;
        public Infusion Spray;
        public Infusion Torrent;
        public Infusion Wall;
        public Infusion Wrack;
        public Infusion Bowling;
        public Infusion Chilling;
        public Infusion Dazzling;
        public Infusion Entangling;
        public Infusion Flash;
        public Infusion Foxfire;
        public Infusion Grappling;
        public Infusion Magnetic;
        public Infusion PureFlame;
        public Infusion Pushing;
        public Infusion RareMetal;
        public Infusion Synaptic;
        public Infusion Unraveling;
        public Infusion Vampiric;
        /// <summary>Special case: Feature is empty</summary>
        public Infusion DefaultAbility;

        // Utility Wild Talents
        public Infusion AerialAdaptation;
        public Infusion AerialEvasion;
        public Infusion Celerity;
        public Infusion ColdAdaptation;
        public Infusion ElementalWhispersSelection;
        public Infusion EnduringEarth;
        public Infusion ExpandedDefense;
        public Infusion FiresFury;
        public Infusion FlameShield;
        public Infusion FoxfireUtility;
        public Infusion HealingBurst;
        public Infusion HeatAdaptation;
        public Infusion JaggedFlesh;
        public Infusion KineticHealer;
        public Infusion KineticRestoration;
        public Infusion KineticRevification;
        public Infusion SkilledKineticist;
        public Infusion Slick;
        public Infusion TidalWave;
        public Infusion Tremorsense;
        public Infusion WildTalentBonusFeatAir;
        public Infusion WildTalentBonusFeatAir1;
        public Infusion WildTalentBonusFeatAir2;
        public Infusion WildTalentBonusFeatAir3;
        public Infusion WildTalentBonusFeatEarth;
        public Infusion WildTalentBonusFeatEarth1;
        public Infusion WildTalentBonusFeatEarth2;
        public Infusion WildTalentBonusFeatEarth3;
        public Infusion WildTalentBonusFeatFire;
        public Infusion WildTalentBonusFeatFire1;
        public Infusion WildTalentBonusFeatFire2;
        public Infusion WildTalentBonusFeatFire3;
        public Infusion WildTalentBonusFeatWater;
        public Infusion WildTalentBonusFeatWater1;
        public Infusion WildTalentBonusFeatWater2;
        public Infusion WildTalentBonusFeatWater3;
        public Infusion WildTalentBonusFeatWater4;
        public Infusion WildTalentBonusFeatWater5;
        public Infusion SkillFocusSelection;

        // Modded: DarkCodex
        public Infusion BladeRush;
        public Infusion Whip;
        public Infusion Chain;
        public Infusion Impale;
        public Infusion Venom;
        public Infusion VenomGreater;
        public Infusion KineticFist;
        public Infusion EnergizeWeapon;
        public Infusion HurricaneQueen;
        public Infusion MindShield;

        // Modded: Kineticist Elements Expanded
        public Infusion Disintegrating;
        public Infusion ForceHook;
        public Infusion FoeThrow;
        public Infusion ManyThrow;
        public Infusion Dampening;
        public Infusion Enervating;
        public Infusion Pulling;
        public Infusion Unnerving;
        public Infusion VoidVampire;
        public Infusion Weighing;
        public Infusion Singularity;
        public Infusion Spore;
        public Infusion Toxic;
        public Infusion ToxicGreater;

        public class Element : IUIDataProvider
        {
            /// <summary>can be null</summary>
            [CanBeNull] public BlueprintFeatureSelectionReference Selection;
            /// <summary>only on basics</summary>
            [CanBeNull] public BlueprintProgressionReference Progession;
            public BlueprintFeatureReference BlastFeature;
            public BlueprintAbilityReference BaseAbility;
            public Blade Blade;

            /// <summary>only on composites</summary>
            [CanBeNull] public Element Parent1;
            /// <summary>only on composites other than metal and blueFlame</summary>
            [CanBeNull] public Element Parent2;

            public DamageTypeDescription[] DamageType;

            public bool IsBasic { get => Parent1 == null; }
            public bool IsComposite { get => Parent1 != null; }
            public bool IsMonoComposite { get => Parent1 != null && Parent2 == null; }
            public bool IsDualComposite { get => Parent1 != null && Parent2 != null; }
            public bool IsMixType { get => DamageType.Length >= 2; }

            // IUIDataProvider
            public string Name => BlastFeature?.Get()?.Name;
            public string Description => BlastFeature?.Get()?.Description;
            public Sprite Icon => BlastFeature?.Get()?.Icon;
            public string NameForAcronym => BlastFeature?.Get()?.NameForAcronym;

            public override string ToString()
            {
                return BlastFeature?.Get()?.name;
            }
        }

        public class Boost : KineticistTree.Element
        {
            public BlueprintActivatableAbilityReference BoostActivatable;

            public bool ModifiesSimple;
            public bool ModifiesComposite;
            public bool ModifiesEnergy;
            public bool ModifiesPhysical;
            public bool IsGreaterVersion { get => ModifiesComposite; }
            public bool IsOnlyPhysical { get => ModifiesPhysical && !ModifiesEnergy; }
            public bool IsOnlyEnergy { get => !ModifiesPhysical && ModifiesEnergy; }
        }

        public class Focus
        {
            public BlueprintProgressionReference First;
            public BlueprintProgressionReference Second;
            public BlueprintProgressionReference Third;
            public BlueprintProgressionReference Knight;

            public BlueprintFeatureReference Defense;

            public Element Element1;
            /// <summary>can be null (earth, fire)</summary>
            [CanBeNull] public Element Element2;
            public Element Composite;
        }

        public class Blade
        {
            public BlueprintFeatureReference Feature;
            public BlueprintActivatableAbilityReference Activatable;
            public BlueprintItemWeaponReference Weapon;
            public BlueprintAbilityReference Damage;
            public BlueprintAbilityReference Burn;
        }

        /// <summary>
        /// This should be called WildTalent instead of Infusion.<br/>
        /// It includes all substance, form, and utility wild talents.
        /// </summary>
        public class Infusion
        {
            public BlueprintFeatureReference Feature;
            public bool RequiresMeleeAttackRoll;
            public bool RequiresRangedAttackRoll;
            public bool RequiresAttackRoll => RequiresMeleeAttackRoll || RequiresRangedAttackRoll;

            /// <summary>can be null (only exists on substance infusions)</summary>
            [CanBeNull] public BlueprintBuffReference Buff;

            /// <summary>can be null (only exists on form infusions)</summary>
            [CanBeNull] public List<BlueprintAbilityReference> Variants;

            /// <summary>can be null (BlueprintActivatable, BlueprintAbility, BlueprintBuff, BlueprintFeature)</summary>
            [CanBeNull] public BlueprintReferenceBase Activator;
        }

        #region Helper

        private DamageTypeDescription[] GetDamageType(PhysicalDamageForm? p1 = null, DamageEnergyType? e2 = null, DamageEnergyType? e1 = null)
        {
            var result = new DamageTypeDescription[e2 != null ? 2 : 1];

            if (p1 != null)
                result[0] = new DamageTypeDescription
                {
                    Type = DamageType.Physical,
                    Physical = new DamageTypeDescription.PhysicalData() { Form = p1.Value }
                };

            if (e1 != null)
                result[0] = new DamageTypeDescription
                {
                    Type = DamageType.Energy,
                    Energy = e1.Value
                };

            if (e2 != null)
                result[1] = new DamageTypeDescription
                {
                    Type = DamageType.Energy,
                    Energy = e2.Value
                };

            return result;
        }

        #endregion

        public void Validate()
        {
            try
            {
                Helper.Print("KineticistTree.Validate start");

                Helper.Print("check all top level blueprint references");
                check(typeof(KineticistTree).GetField(nameof(instance), Helper.BindingAll), this);

                Helper.Print("check all top level fields (Element, Focus, Infusion)");
                var fi_blade = typeof(Element).GetField(nameof(Element.Blade));
                var fields = typeof(KineticistTree).GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (var fi in fields)
                {
                    object value = fi.GetValue(this);

                    if (typeof(BlueprintReferenceBase).IsAssignableFrom(fi.FieldType)) // these are already covered
                    { }

                    else if (value is null)
                        Helper.Print($"field is null '{fi.Name}'");

                    else if (object.ReferenceEquals(value, DefaultAbility)) // this has special logic
                    { }

                    #region Element
                    else if (value is Element element)
                    {
                        if (!GetAll(basic: true, composite: true, boost: true, archetype: true).Contains(element))
                            Helper.Print($"field missing in GetAll '{fi.Name}'");

                        check(fi, value);
                        check(fi_blade, element.Blade);
                    }
                    #endregion

                    #region Focus
                    else if (value is Focus focus)
                    {
                        if (!GetFocus().Contains(focus))
                            Helper.Print($"field missing in GetFocus '{fi.Name}'");

                        check(fi, value);
                    }
                    #endregion

                    #region Infusion
                    else if (value is Infusion infusion)
                    {
                        if (!GetTalents(form: true, substance: true, utility: true).Contains(infusion))
                            Helper.Print($"field missing in GetTalents '{fi.Name}'");

                        check(fi, value);

                        // check form infusion
                        if (GetTalents(form: true).Contains(infusion))
                        {
                            // REGEX    log file to variant references
                            // search   .*([\w\d]{32})$
                            // replace  Helper.ToRef<BlueprintAbilityReference>\("$1"\),

                            // check variants
                            if (infusion.Variants != null)
                            {
                                foreach (var variant in infusion.Variants)
                                {
                                    if (variant.Get()?.GetComponent<AbilityShowIfCasterHasFact>()?.m_UnitFact?.Equals(infusion.Feature) != true)
                                        Helper.Print($"wrong variant: {fi.Name} -> {variant.Get()?.name} : {variant.Get()?.AssetGuid}");
                                }
                            }

                            // check missing variants
                            foreach (var blast in GetBlasts(variants: true).Where(a => a.GetComponent<AbilityShowIfCasterHasFact>()?.m_UnitFact?.Equals(infusion.Feature) == true))
                            {
                                if (infusion.Variants == null || !infusion.Variants.Any(a => a.Is(blast)))
                                    Helper.Print($"missing variant: {fi.Name} -> {blast.name} : {blast.AssetGuid}");
                            }
                        }

                        // check activators
                        if (GetTalents(substance: true, utility: true).Contains(infusion))
                        {
                            var addfacts = infusion.Feature?.Get()?.GetComponent<AddFacts>()?.m_Facts?.Distinct();
                            if (addfacts == null)
                                continue;

                            if (addfacts.Count() != 1)
                            {
                                compare<BlueprintReferenceBase>(fi.Name, infusion.Variants, addfacts);
                            }
                            else if (infusion.Activator == null || !infusion.Activator.Equals(addfacts.First()))
                            {
                                Helper.Print($"wrong activator: {fi.Name} : {infusion.Activator?.GetBlueprint()?.AssetGuid} -> {addfacts.First()} : {addfacts.First().GetBlueprint()?.GetType()}");
                            }
                        }
                    }
                    #endregion
                }

                #region check DefaultAbility

                Helper.Print("check DefaultAbility");
                // check missing variants
                foreach (var blast in GetBlasts(variants: true).Where(a => a.GetComponent<AbilityShowIfCasterHasFact>() == null))
                {
                    if (!DefaultAbility.Variants.Any(a => a.Is(blast)))
                        Helper.Print($"missing variant: {nameof(DefaultAbility)} -> {blast.name} : {blast.AssetGuid}");
                }

                #endregion

                Helper.Print("KineticistTree.Validate end");
                return;

                // check all blueprint references on an object for missing CanBeNullAttribute, missing blueprint, and wrong reference typing
                void check(FieldInfo fi, object field)
                {
                    if (fi == null || field == null)
                        return;

                    var type = fi.FieldType;
                    if (!type.IsClass)
                        return;

                    foreach (var fi2 in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (!typeof(BlueprintReferenceBase).IsAssignableFrom(fi2.FieldType))
                            continue;

                        bool canBeNull = fi2.HasAttribute<CanBeNullAttribute>() || fi.Name.StartsWith("Boost_");

                        if (fi2.GetValue(field) is not BlueprintReferenceBase reference)
                        {
                            if (!canBeNull)
                                Helper.Print($"reference check '{fi.Name}.{fi2.Name}' is null");
                            continue;
                        }

                        var bp = reference.GetBlueprint();
                        if (bp == null)
                        {
                            Helper.Print($"reference check '{fi.Name}.{fi2.Name}':{reference.Guid} is null");
                            continue;
                        }

                        var refType = reference.GetType().BaseType?.GenericTypeArguments.FirstOrDefault();
                        if (refType != null && !refType.IsAssignableFrom(bp.GetType()))
                        {
                            Helper.Print($"reference check '{fi.Name}.{fi2.Name}':{reference.Guid} type mismatch ref={refType} bp={bp.GetType()}");
                        }
                    }
                }

                // prints diff in two collections
                void compare<T>(string name, IEnumerable<T> collection1, IEnumerable<T> collection2)
                {
                    if (collection1 == null && collection2 == null)
                        return;

                    if (collection1 == null)
                        Helper.Print($"{name} left empty; right: {collection2.Join()}");
                    else if (collection2 == null)
                        Helper.Print($"{name} right empty; left: {collection1.Join()}");
                    else
                    {
                        foreach (var item in collection1.Where(w => !collection2.Contains(w)))
                            Helper.Print($"{name} only left: {item}");
                        foreach (var item in collection2.Where(w => !collection1.Contains(w)))
                            Helper.Print($"{name} only right: {item}");
                    }
                }

                // finds field name of top level object
                //string resolve(object field)
                //{
                //    var fi = fields.FirstOrDefault(f => ReferenceEquals(f.GetValue(this), field));
                //    if (fi != null)
                //        return fi.Name;
                //    return "";
                //}

            }
            catch (Exception e) { Helper.PrintException(e); }
        }
    }
}
