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

namespace CodexLib
{
    public class KineticistTree
    {
        private static KineticistTree instance;
        public static KineticistTree Instance { get => instance ??= new(); }

        public KineticistTree()
        {
            // Expanded Defense: d741f298dfae8fc40b4615aaf83b6548

            @Class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9ec1ad924d889272429eb8391");
            ElementalScion = Helper.ToRef<BlueprintArchetypeReference>("180c6e3574aa4c938e73952cb02d1535");
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
            KineticBlade = Helper.ToRef<BlueprintFeatureReference>("9ff81732daddb174aa8138ad1297c787");
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
                DamageType = new DamageTypeDescription { Type = DamageType.Force }.ObjToArray()
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

            #endregion

            #region Focus

            FocusAir = new()
            {
                First = Helper.ToRef<BlueprintProgressionReference>("2bd0d44953a536f489082534c48f8e31"),
                Second = Helper.ToRef<BlueprintProgressionReference>("659c39542b728c04b83e969c834782a9"),
                Third = Helper.ToRef<BlueprintProgressionReference>("651570c873e22b84f893f146ce2de502"),
                Knight = Helper.ToRef<BlueprintProgressionReference>("93bd14dd916cfd1429c11ad66adf5e2b"),
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
                Element1 = Wood,
                Element2 = Positive,
                Composite = Composite_Verdant
            };

            #endregion

            BaseBasic = GetAll(true, false).Select(s => s.BaseAbility).ToArray();
            BaseComposite = GetAll(false, true).Select(s => s.BaseAbility).ToArray();
            BaseAll = GetAll(true, true, archetype: true).Select(s => s.BaseAbility).ToArray();
        }

        public IEnumerable<Element> GetAll(bool basic = false, bool composite = false, bool onlyPhysical = false, bool onlyEnergy = false, bool archetype = false, bool modded = true)
        {
            bool mod1 = modded && UnityModManagerNet.UnityModManager.FindMod("KineticistElementsExpanded")?.Active == true;

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
        }

        public IEnumerable<Focus> GetFocus(bool modded = true)
        {
            bool mod1 = modded && UnityModManagerNet.UnityModManager.FindMod("KineticistElementsExpanded")?.Active == true;

            yield return FocusAir;
            yield return FocusEarth;
            yield return FocusFire;
            yield return FocusWater;
            if (mod1)
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

        public IEnumerable<BlueprintFeature> GetInfusionTalents()
        {
            foreach (var talent in SelectionInfusion.Get().m_AllFeatures)
            {
                var a = talent.Get();
                if (a != null)
                    yield return a;
            }
        }

        public IEnumerable<BlueprintFeature> GetWildTalents()
        {
            foreach (var talent in SelectionWildTalent.Get().m_AllFeatures)
            {
                var a = talent.Get();
                if (a != null)
                    yield return a;
            }
        }

        public IEnumerable<BlueprintAbility> GetBlasts(bool bases = false, bool variants = false, bool bladeburn = false, bool bladedamage = false)
        {
            foreach (var element in GetAll(true, true, archetype: true))
            {
                var b = element.BaseAbility.Get();
                if (b == null)
                    continue;

                if (bases)
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

        public BlueprintCharacterClassReference @Class;
        public BlueprintArchetypeReference ElementalScion;
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
        public BlueprintFeatureReference KineticBlade;
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

        public Focus FocusVoid;
        public Element Gravity;
        public Element Negative;
        public Element Composite_Void;

        public Focus FocusWood;
        public Element Wood;
        public Element Positive;
        public Element Composite_Verdant;
        public Element Composite_Autumn;
        public Element Composite_Spring;
        public Element Composite_Summer;
        public Element Composite_Winter;

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

        public class Focus
        {
            public BlueprintProgressionReference First;
            public BlueprintProgressionReference Second;
            public BlueprintProgressionReference Third;
            public BlueprintProgressionReference Knight;

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

        public class Infusion
        {
            public BlueprintFeatureReference Feature;
            /// <summary>can be null (form infusions)</summary>
            [CanBeNull] public BlueprintBuffReference Buff;
        }

        #region Helper

        private DamageTypeDescription[] GetDamageType(PhysicalDamageForm? p1 = null, DamageEnergyType? e2 = null, DamageEnergyType? e1 = null)
        {
            var result = new DamageTypeDescription[e2 != null ? 2 : 1];

            if (p1 != null)
                result[0] = new DamageTypeDescription
                {
                    Type = DamageType.Physical,
                    Physical = new DamageTypeDescription.PhysicalData { Form = p1.Value }
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
    }
}
