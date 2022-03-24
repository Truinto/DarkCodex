using JetBrains.Annotations;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UnitLogic.Abilities.Blueprints;
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

            @Class = Helper.Get<BlueprintCharacterClass>("42a455d9ec1ad924d889272429eb8391");

            FocusFirst = Helper.Get<BlueprintFeatureSelection>("1f3a15a3ae8a5524ab8b97f469bf4e3d");
            FocusSecond = Helper.Get<BlueprintFeatureSelection>("4204bc10b3d5db440b1f52f0c375848b");
            FocusThird = Helper.Get<BlueprintFeatureSelection>("e2c1718828fc843479f18ab4d75ded86");
            FocusKnight = Helper.Get<BlueprintFeatureSelection>("b1f296f0bd16bc242ae35d0638df82eb");
            CompositeBuff = Helper.Get<BlueprintBuff>("cb30a291c75def84090430fbf2b5c05e");

            #region Elements
            Air = new()
            {
                Selection = Helper.Get<BlueprintFeatureSelection>("49e55e8f24e1ad24e910fefc0258adba"),
                Progession = Helper.Get<BlueprintProgression>("6f1d86ae43adf1049834457ce5264003"),
                BlastFeature = Helper.Get<BlueprintFeature>("cb09e292ad9acc3428fa0dfdcbb83883"),
                BaseAbility = Helper.Get<BlueprintAbility>("0ab1552e2ebdacf44bb7b20f5393366d")
            };

            Electric = new()
            {
                Selection = Air.Selection,
                Progession = Helper.Get<BlueprintProgression>("ba7767cb03f7f3949ad08bd3ff8a646f"),
                BlastFeature = Helper.Get<BlueprintFeature>("c2c28b6f6f000314eb35fff49bb99920"),
                BaseAbility = Helper.Get<BlueprintAbility>("45eb571be891c4c4581b6fcddda72bcd")
            };

            Earth = new()
            {
                Selection = null,
                Progession = Helper.Get<BlueprintProgression>("d945ac76fc6a06e44b890252824db30a"),
                BlastFeature = Helper.Get<BlueprintFeature>("7f5f82c1108b961459c9884a0fa0f5c4"),
                BaseAbility = Helper.Get<BlueprintAbility>("e53f34fb268a7964caf1566afb82dadd")
            };

            Fire = new()
            {
                Selection = null,
                Progession = Helper.Get<BlueprintProgression>("fbed3ca8c0d89124ebb3299ccf68c439"),
                BlastFeature = Helper.Get<BlueprintFeature>("cbc88c4c166a0ce4a95375a0a721bd01"),
                BaseAbility = Helper.Get<BlueprintAbility>("83d5873f306ac954cad95b6aeeeb2d8c")
            };

            Water = new()
            {
                Selection = Helper.Get<BlueprintFeatureSelection>("53a8c2f3543147b4d913c6de0c57c7e8"),
                Progession = Helper.Get<BlueprintProgression>("e4027e0fec48e8048a172c6627d4eba9"),
                BlastFeature = Helper.Get<BlueprintFeature>("560887b5187098b428364de03e628b53"),
                BaseAbility = Helper.Get<BlueprintAbility>("d663a8d40be1e57478f34d6477a67270")
            };

            Cold = new()
            {
                Selection = Water.Selection,
                Progession = Helper.Get<BlueprintProgression>("dbb1159b0e8137c4ea20434a854ae6a8"),
                BlastFeature = Helper.Get<BlueprintFeature>("ce625487d909b154c9305e60e4fc7d60"),
                BaseAbility = Helper.Get<BlueprintAbility>("7980e876b0749fc47ac49b9552e259c1")
            };

            Composite_Metal = new()
            {
                Selection = null,
                Progession = Helper.Get<BlueprintProgression>("ccd26825e04f8044c881cfcef49f1872"),
                BlastFeature = Helper.Get<BlueprintFeature>("ad20bc4e586278c4996d4a81b2448998"),
                BaseAbility = Helper.Get<BlueprintAbility>("6276881783962284ea93298c1fe54c48"),
                Parent1 = Earth,
                Parent2 = null
            };

            Composite_BlueFlame = new()
            {
                Selection = null,
                Progession = Helper.Get<BlueprintProgression>("cdf2a117e8a2ccc4ebabd2fcee1e4d09"),
                BlastFeature = Helper.Get<BlueprintFeature>("89dfce413170db049b0386fff333e9e1"),
                BaseAbility = Helper.Get<BlueprintAbility>("d29186edb20be6449b23660b39435398"),
                Parent1 = Fire,
                Parent2 = null
            };

            Composite_Plasma = new()
            {
                Selection = null,
                Progession = Helper.Get<BlueprintProgression>("953fe61325983f244adbd7384903393d"),
                BlastFeature = Helper.Get<BlueprintFeature>("93d8bc401accfe6489ea3797e316e5d9"),
                BaseAbility = Helper.Get<BlueprintAbility>("9afdc3eeca49c594aa7bf00e8e9803ac"),
                Parent1 = Air,
                Parent2 = Fire
            };

            Composite_Sand = new()
            {
                Selection = null,
                Progession = Helper.Get<BlueprintProgression>("f05a7bde1b2bf9e4e927b3b1aeca8bfb"),
                BlastFeature = Helper.Get<BlueprintFeature>("af70dce0745f91f4b8aa99a98620e45b"),
                BaseAbility = Helper.Get<BlueprintAbility>("b93e1f0540a4fa3478a6b47ae3816f32"),
                Parent1 = Air,
                Parent2 = Earth
            };

            Composite_Thunder = new()
            {
                Selection = null,
                Progession = Helper.Get<BlueprintProgression>("33217c1678c30bd4ea2748decaced223"),
                BlastFeature = Helper.Get<BlueprintFeature>("295080cf4691df9438f58ff5ce79ee65"),
                BaseAbility = Helper.Get<BlueprintAbility>("b813ceb82d97eed4486ddd86d3f7771b"),
                Parent1 = Air,
                Parent2 = Electric
            };

            Composite_Blizzard = new()
            {
                Selection = null,
                Progession = Helper.Get<BlueprintProgression>("747a1f33ed0a17442b3273adc7797661"),
                BlastFeature = Helper.Get<BlueprintFeature>("52292a32bb5d0ab45a86621bac2c4c9a"),
                BaseAbility = Helper.Get<BlueprintAbility>("16617b8c20688e4438a803effeeee8a6"),
                Parent1 = Air,
                Parent2 = Cold
            };

            Composite_Ice = new()
            {
                Selection = null,
                Progession = Helper.Get<BlueprintProgression>("d6375ba9b52eca04a805a54765310976"),
                BlastFeature = Helper.Get<BlueprintFeature>("a8cc34ca1a5e55a4e8aa5394efe2678e"),
                BaseAbility = Helper.Get<BlueprintAbility>("403bcf42f08ca70498432cf62abee434"),
                Parent1 = Cold,
                Parent2 = Water
            };

            Composite_Magma = new()
            {
                Selection = null,
                Progession = Helper.Get<BlueprintProgression>("cb19d1cbf6daf7a46bf38c05af1c2fb0"),
                BlastFeature = Helper.Get<BlueprintFeature>("408b25c6d9f223b41b935e6ec550e88d"),
                BaseAbility = Helper.Get<BlueprintAbility>("8c25f52fce5113a4491229fd1265fc3c"),
                Parent1 = Earth,
                Parent2 = Fire
            };

            Composite_Mud = new()
            {
                Selection = null,
                Progession = Helper.Get<BlueprintProgression>("648d3c01bcab7614595facd302e88184"),
                BlastFeature = Helper.Get<BlueprintFeature>("6e33cde96209b5a4f9596a6e509de532"),
                BaseAbility = Helper.Get<BlueprintAbility>("e2610c88664e07343b4f3fb6336f210c"),
                Parent1 = Earth,
                Parent2 = Water
            };

            Composite_ChargedWater = new()
            {
                Selection = null,
                Progession = Helper.Get<BlueprintProgression>("e717ae6647573bf4195ea168693c7be0"),
                BlastFeature = Helper.Get<BlueprintFeature>("9b7bf2754e2012e4dac135fd6c782fac"),
                BaseAbility = Helper.Get<BlueprintAbility>("4e2e066dd4dc8de4d8281ed5b3f4acb6"),
                Parent1 = Electric,
                Parent2 = Water
            };

            Composite_Steam = new()
            {
                Selection = null,
                Progession = Helper.Get<BlueprintProgression>("985fa6f168ea663488956713bc44a1e8"),
                BlastFeature = Helper.Get<BlueprintFeature>("29e4076127a404e4ab1cde7e967e1047"),
                BaseAbility = Helper.Get<BlueprintAbility>("3baf01649a92ae640927b0f633db7c11"),
                Parent1 = Fire,
                Parent2 = Water
            };
            #endregion

            #region Focus
            FocusAir = new()
            {
                First = Helper.Get<BlueprintProgression>("2bd0d44953a536f489082534c48f8e31"),
                Second = Helper.Get<BlueprintProgression>("659c39542b728c04b83e969c834782a9"),
                Third = Helper.Get<BlueprintProgression>("651570c873e22b84f893f146ce2de502"),
                Knight = Helper.Get<BlueprintProgression>("93bd14dd916cfd1429c11ad66adf5e2b"),
                Element1 = Air,
                Element2 = Electric
            };
            FocusEarth = new()
            {
                First = Helper.Get<BlueprintProgression>("c6816ad80a3df9c4ea7d3b012b06bacd"),
                Second = Helper.Get<BlueprintProgression>("956b65effbf37e5419c13100ab4385a3"),
                Third = Helper.Get<BlueprintProgression>("c43d9c2d23e56fb428a4eb60da9ba1cb"),
                Knight = Helper.Get<BlueprintProgression>("d2a93ab18fcff8c419b03a2c3d573606"),
                Element1 = Earth,
                Element2 = null
            };
            FocusFire = new()
            {
                First = Helper.Get<BlueprintProgression>("3d8d3d6678b901444a07984294a1bc24"),
                Second = Helper.Get<BlueprintProgression>("caa7edca64af1914d9e14785beb6a143"),
                Third = Helper.Get<BlueprintProgression>("56e2fc3abed8f2247a621ac37e75f303"),
                Knight = Helper.Get<BlueprintProgression>("d4a2a75d01d1e77489ff692636a538bf"),
                Element1 = Fire,
                Element2 = null
            };
            FocusWater = new()
            {
                First = Helper.Get<BlueprintProgression>("7ab8947ce2e19c44a9edcf5fd1466686"),
                Second = Helper.Get<BlueprintProgression>("faa5f1233600d864fa998bc0afe351ab"),
                Third = Helper.Get<BlueprintProgression>("86eff374d040404438ad97fedd7218bc"),
                Knight = Helper.Get<BlueprintProgression>("5e839c743c6da6649a43cdeb70b6018f"),
                Element1 = Water,
                Element2 = Cold
            };
            #endregion

        }

        public IEnumerable<Element> GetAll(bool basic = false, bool composites = false)
        {
            if (basic)
            {
                yield return Air;
                yield return Electric;
                yield return Earth;
                yield return Fire;
                yield return Water;
                yield return Cold;
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

        public Focus GetFocus(BlueprintFeature feature)
        {
            foreach (var focus in GetFocus())
            {
                if (focus.First == feature || focus.Second == feature || focus.Third == feature || focus.Knight == feature)
                    return focus;
            }
            return null;
        }

        public BlueprintCharacterClass @Class;

        public BlueprintFeatureSelection FocusFirst;
        public BlueprintFeatureSelection FocusSecond;
        public BlueprintFeatureSelection FocusThird;
        public BlueprintFeatureSelection FocusKnight;
        public BlueprintFeatureSelection ExpandedElement;
        public BlueprintBuff CompositeBuff;

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

        public class Element
        {
            [CanBeNull] public BlueprintFeatureSelection Selection;
            [CanBeNull] public BlueprintProgression Progession;     // only on basics
            public BlueprintFeature BlastFeature;
            public BlueprintAbility BaseAbility;

            [CanBeNull] public Element Parent1; // only on composites
            [CanBeNull] public Element Parent2; // only on composites other than metal and blueFlame
        }

        public class Focus
        {
            public BlueprintProgression First;
            public BlueprintProgression Second;
            public BlueprintProgression Third;
            public BlueprintProgression Knight;

            public Element Element1;
            [CanBeNull] public Element Element2; // other than earth and fire
        }
    }
}
