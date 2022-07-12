using CodexLib;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    public class KineticistTree
    {
        public KineticistTree()
        {
            // Expanded Defense: d741f298dfae8fc40b4615aaf83b6548

            @Class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9ec1ad924d889272429eb8391");
            KineticBlast = Helper.ToRef<BlueprintFeatureReference>("93efbde2764b5504e98e6824cab3d27c");
            KineticistMainStatProperty = Helper.ToRef<BlueprintUnitPropertyReference>("f897845bbbc008d4f9c1c4a03e22357a");

            FocusFirst = Helper.ToRef<BlueprintFeatureSelectionReference>("1f3a15a3ae8a5524ab8b97f469bf4e3d");
            FocusSecond = Helper.ToRef<BlueprintFeatureSelectionReference>("4204bc10b3d5db440b1f52f0c375848b");
            FocusThird = Helper.ToRef<BlueprintFeatureSelectionReference>("e2c1718828fc843479f18ab4d75ded86");
            FocusKnight = Helper.ToRef<BlueprintFeatureSelectionReference>("b1f296f0bd16bc242ae35d0638df82eb");
            ExpandedElement = new();
            CompositeBuff = Helper.ToRef<BlueprintBuffReference>("cb30a291c75def84090430fbf2b5c05e");

            #region Elements
            Air = new()
            {
                Selection = Helper.ToRef<BlueprintFeatureSelectionReference>("49e55e8f24e1ad24e910fefc0258adba"),
                Progession = Helper.ToRef<BlueprintProgressionReference>("6f1d86ae43adf1049834457ce5264003"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("cb09e292ad9acc3428fa0dfdcbb83883"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("0ab1552e2ebdacf44bb7b20f5393366d")
            };

            Electric = new()
            {
                Selection = Air.Selection,
                Progession = Helper.ToRef<BlueprintProgressionReference>("ba7767cb03f7f3949ad08bd3ff8a646f"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("c2c28b6f6f000314eb35fff49bb99920"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("45eb571be891c4c4581b6fcddda72bcd")
            };

            Earth = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("d945ac76fc6a06e44b890252824db30a"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("7f5f82c1108b961459c9884a0fa0f5c4"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("e53f34fb268a7964caf1566afb82dadd")
            };

            Fire = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("fbed3ca8c0d89124ebb3299ccf68c439"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("cbc88c4c166a0ce4a95375a0a721bd01"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("83d5873f306ac954cad95b6aeeeb2d8c")
            };

            Water = new()
            {
                Selection = Helper.ToRef<BlueprintFeatureSelectionReference>("53a8c2f3543147b4d913c6de0c57c7e8"),
                Progession = Helper.ToRef<BlueprintProgressionReference>("e4027e0fec48e8048a172c6627d4eba9"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("560887b5187098b428364de03e628b53"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("d663a8d40be1e57478f34d6477a67270")
            };

            Cold = new()
            {
                Selection = Water.Selection,
                Progession = Helper.ToRef<BlueprintProgressionReference>("dbb1159b0e8137c4ea20434a854ae6a8"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("ce625487d909b154c9305e60e4fc7d60"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("7980e876b0749fc47ac49b9552e259c1")
            };

            Composite_Metal = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("ccd26825e04f8044c881cfcef49f1872"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("ad20bc4e586278c4996d4a81b2448998"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("6276881783962284ea93298c1fe54c48"),
                Parent1 = Earth,
                Parent2 = null
            };

            Composite_BlueFlame = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("cdf2a117e8a2ccc4ebabd2fcee1e4d09"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("89dfce413170db049b0386fff333e9e1"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("d29186edb20be6449b23660b39435398"),
                Parent1 = Fire,
                Parent2 = null
            };

            Composite_Plasma = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("953fe61325983f244adbd7384903393d"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("93d8bc401accfe6489ea3797e316e5d9"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("9afdc3eeca49c594aa7bf00e8e9803ac"),
                Parent1 = Air,
                Parent2 = Fire
            };

            Composite_Sand = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("f05a7bde1b2bf9e4e927b3b1aeca8bfb"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("af70dce0745f91f4b8aa99a98620e45b"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("b93e1f0540a4fa3478a6b47ae3816f32"),
                Parent1 = Air,
                Parent2 = Earth
            };

            Composite_Thunder = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("33217c1678c30bd4ea2748decaced223"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("295080cf4691df9438f58ff5ce79ee65"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("b813ceb82d97eed4486ddd86d3f7771b"),
                Parent1 = Air,
                Parent2 = Electric
            };

            Composite_Blizzard = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("747a1f33ed0a17442b3273adc7797661"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("52292a32bb5d0ab45a86621bac2c4c9a"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("16617b8c20688e4438a803effeeee8a6"),
                Parent1 = Air,
                Parent2 = Cold
            };

            Composite_Ice = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("d6375ba9b52eca04a805a54765310976"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("a8cc34ca1a5e55a4e8aa5394efe2678e"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("403bcf42f08ca70498432cf62abee434"),
                Parent1 = Cold,
                Parent2 = Water
            };

            Composite_Magma = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("cb19d1cbf6daf7a46bf38c05af1c2fb0"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("408b25c6d9f223b41b935e6ec550e88d"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("8c25f52fce5113a4491229fd1265fc3c"),
                Parent1 = Earth,
                Parent2 = Fire
            };

            Composite_Mud = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("648d3c01bcab7614595facd302e88184"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("6e33cde96209b5a4f9596a6e509de532"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("e2610c88664e07343b4f3fb6336f210c"),
                Parent1 = Earth,
                Parent2 = Water
            };

            Composite_ChargedWater = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("e717ae6647573bf4195ea168693c7be0"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("9b7bf2754e2012e4dac135fd6c782fac"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("4e2e066dd4dc8de4d8281ed5b3f4acb6"),
                Parent1 = Electric,
                Parent2 = Water
            };

            Composite_Steam = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("985fa6f168ea663488956713bc44a1e8"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("29e4076127a404e4ab1cde7e967e1047"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("3baf01649a92ae640927b0f633db7c11"),
                Parent1 = Fire,
                Parent2 = Water
            };

            Composite_Blood = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("535a9c4dbe912924396ae50cc7fba8c4"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("79b5d7184efe7034a863ae612c429306"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("ba2113cfed0c2c14b93c20e7625a4c74"),
                Blade = new() // TODO: add other blades as well
                {
                    Activatable = Helper.ToRef<BlueprintActivatableAbilityReference>("98f0da4bf25a34a4caffa6b8a2d33ef6"),
                    Weapon = Helper.ToRef<BlueprintItemWeaponReference>("92f9a719ffd652947ab37363266cc0a6"),
                    Damage = Helper.ToRef<BlueprintAbilityReference>("0a386b1c2b4ae9b4f81ddf4557155810"),
                    Burn = Helper.ToRef<BlueprintAbilityReference>("15278f2a9a5eaa441a261ec033b60b57")
                }
            };

            // modded
            Telekinetic = new()
            {
                Selection = null,
                Progession = Helper.ToRef<BlueprintProgressionReference>("6ce72cb2bf0244b0bd0e5e0a552a6a4a"),
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("e86649f76cba4483bed7f01859c6b425"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("ac038f9898ef4ba7b46bfcafdbc77818")
            };

            Composite_Force = new()
            {
                Selection = null,
                Progession = null,
                BlastFeature = Helper.ToRef<BlueprintFeatureReference>("c6e4201d7b674cc78a2c95bae61b9d25"),
                BaseAbility = Helper.ToRef<BlueprintAbilityReference>("8dacff62b4a8413bbfb299458cf94839"),
                Parent1 = Telekinetic,
                Parent2 = null
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
                Element2 = Electric
            };
            FocusEarth = new()
            {
                First = Helper.ToRef<BlueprintProgressionReference>("c6816ad80a3df9c4ea7d3b012b06bacd"),
                Second = Helper.ToRef<BlueprintProgressionReference>("956b65effbf37e5419c13100ab4385a3"),
                Third = Helper.ToRef<BlueprintProgressionReference>("c43d9c2d23e56fb428a4eb60da9ba1cb"),
                Knight = Helper.ToRef<BlueprintProgressionReference>("d2a93ab18fcff8c419b03a2c3d573606"),
                Element1 = Earth,
                Element2 = null
            };
            FocusFire = new()
            {
                First = Helper.ToRef<BlueprintProgressionReference>("3d8d3d6678b901444a07984294a1bc24"),
                Second = Helper.ToRef<BlueprintProgressionReference>("caa7edca64af1914d9e14785beb6a143"),
                Third = Helper.ToRef<BlueprintProgressionReference>("56e2fc3abed8f2247a621ac37e75f303"),
                Knight = Helper.ToRef<BlueprintProgressionReference>("d4a2a75d01d1e77489ff692636a538bf"),
                Element1 = Fire,
                Element2 = null
            };
            FocusWater = new()
            {
                First = Helper.ToRef<BlueprintProgressionReference>("7ab8947ce2e19c44a9edcf5fd1466686"),
                Second = Helper.ToRef<BlueprintProgressionReference>("faa5f1233600d864fa998bc0afe351ab"),
                Third = Helper.ToRef<BlueprintProgressionReference>("86eff374d040404438ad97fedd7218bc"),
                Knight = Helper.ToRef<BlueprintProgressionReference>("5e839c743c6da6649a43cdeb70b6018f"),
                Element1 = Water,
                Element2 = Cold
            };
            #endregion

            BaseBasic = GetAll(true, false).Select(s => s.BaseAbility).ToArray();
            BaseComposite = GetAll(false, true).Select(s => s.BaseAbility).ToArray();
            BaseAll = GetAll(true, true).Select(s => s.BaseAbility).ToArray();
        }

        public IEnumerable<Element> GetAll(bool basic = false, bool composites = false, bool modded = true)
        {
            bool mod1 = modded && UnityModManagerNet.UnityModManager.FindMod("KineticistElementsExpanded")?.Active == true;

            if (basic)
            {
                yield return Air;
                yield return Electric;
                yield return Earth;
                yield return Fire;
                yield return Water;
                yield return Cold;
                if (mod1) yield return Telekinetic;
            }
            if (composites)
            {
                yield return Composite_Metal;
                yield return Composite_BlueFlame;
                yield return Composite_Plasma;
                yield return Composite_Sand;
                yield return Composite_Thunder;
                yield return Composite_Blizzard;
                yield return Composite_Ice;
                yield return Composite_Magma;
                yield return Composite_Mud;
                yield return Composite_ChargedWater;
                yield return Composite_Steam;
                yield return Composite_Blood;
                if (mod1) yield return Composite_Force;
            }
        }

        public IEnumerable<Focus> GetFocus()
        {
            yield return FocusAir;
            yield return FocusEarth;
            yield return FocusFire;
            yield return FocusWater;
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

        public BlueprintCharacterClassReference @Class;
        public BlueprintFeatureReference KineticBlast;
        public BlueprintUnitPropertyReference KineticistMainStatProperty;

        public BlueprintFeatureSelectionReference FocusFirst;
        public BlueprintFeatureSelectionReference FocusSecond;
        public BlueprintFeatureSelectionReference FocusThird;
        public BlueprintFeatureSelectionReference FocusKnight;
        public BlueprintFeatureSelectionReference ExpandedElement;
        public BlueprintBuffReference CompositeBuff;

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
        public Element Telekinetic;
        public Element Composite_Force;

        public class Element
        {
            [CanBeNull] public BlueprintFeatureSelectionReference Selection;
            [CanBeNull] public BlueprintProgressionReference Progession;     // only on basics
            public BlueprintFeatureReference BlastFeature;
            public BlueprintAbilityReference BaseAbility;
            public Blade Blade;

            [CanBeNull] public Element Parent1; // only on composites
            [CanBeNull] public Element Parent2; // only on composites other than metal and blueFlame
        }

        public class Focus
        {
            public BlueprintProgressionReference First;
            public BlueprintProgressionReference Second;
            public BlueprintProgressionReference Third;
            public BlueprintProgressionReference Knight;

            public Element Element1;
            [CanBeNull] public Element Element2; // other than earth and fire
        }

        public class Blade
        {
            public BlueprintActivatableAbilityReference Activatable;
            public BlueprintItemWeaponReference Weapon;
            public BlueprintAbilityReference Damage;
            public BlueprintAbilityReference Burn;
        }
    }
}
