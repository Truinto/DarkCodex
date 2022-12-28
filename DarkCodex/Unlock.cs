using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.EventConditionActionSystem.Events;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using CodexLib;

namespace DarkCodex
{
    public class Unlock
    {
        [PatchInfo(Severity.Create, "Unlock Spells", "unlocks some spells: Transformation", true)]
        public static void UnlockSpells()
        {
            var TransformationBuff = Helper.Get<BlueprintBuff>("287682389d2011b41b5a65195d9cbc84"); //TransformationBuff
            TransformationBuff.GetComponents<ContextRankConfig>().ForEach(noMax());

            static Action<ContextRankConfig> noMax() => f => f.m_UseMax = false;
        }

        [PatchInfo(Severity.Create | Severity.WIP, "Unlock Animal Companion", "allows animal companions to reach up to level 40", true, Requirement: typeof(Patch_UnlockClassLevels))]
        public static void UnlockAnimalCompanion()
        {
            var animalRank = Helper.Get<BlueprintFeature>("1670990255e4fe948a863bafd5dbda5d"); //AnimalCompanionRank
            animalRank.Ranks = 40;

            var list = new List<string>
            {
                "908623b96863e8344b64544ccce32957", //MadDogAnimalCompanionProgression
                "2cb79bb1bbeace04197c007ad149dde5", //BloodriderMountProgression
                "775af7b6cd98b0e4a99b51898c97777d", //CavalierMountProgression
                "3853d5405ebfc0f4a86930bb7082b43b", //DruidAnimalCompanionProgression
                "446fe89490cab7d44957efebeb8cc2b1", //HunterAnimalCompanionProgression
                "56e99ebd958e4abdae91ba028a666728", //DivineHoundAnimalCompanionProgression
                "924fb4b659dcb4f4f906404ba694b690", //SacredHuntsmasterAnimalCompanionProgression
                "0138fd1cfbe644f9ae60c436395950bf", //ArcaneRiderMountProgression
                "8700dca0d91c9964ba7e4e6cae7637fb", //MonkAnimalCompanionProgression
                "7d1c29c3101dd7643a625448fbbaa919", //OracleRevelationBondedMountProgression
                "b82ac7a5661a3044596398a203188c22", //PaladinDivineMountProgression
                "152450aedc0788e41b4f9e745c091437", //RangerAnimalCompanionProgression
                "41836efffaa346a091686e990b122ca4", //NomadMountProgression
                "edd41ea87f1740d4be2de56961a80bb5", //WildlandShamanAnimalCompanionProgression
                "693879b0f26d7e04280510d4dcbf3de1", //ShamanAnimalCompanionProgression
                "09c91f959fb737f4289d121e595c657c", //SylvanSorcererAnimalCompanionProgression
                "125af359f8bc9a145968b5d8fd8159b8", //DomainAnimalCompanionProgression
            };

            foreach (string guid in list)
            {
                var prog = Helper.Get<BlueprintProgression>(guid);
                for (int i = 21; i <= 40; i++)
                    prog.AddFeature(i, animalRank);
            }

        }

        [PatchInfo(Severity.Create, "Unlock Kineticist", "adds infusion, wild talent, and element focus up to level 40", true, Requirement: typeof(Patch_UnlockClassLevels))]
        public static void UnlockKineticist()
        {
            var t = KineticistTree.Instance;

            // replace original with simplified
            //if (t.ExpandedElement.NotEmpty())
            //{
            //    t.FocusSecond.Get().m_AllFeatures = t.ExpandedElement.Get().m_AllFeatures;
            //    t.FocusThird.Get().m_AllFeatures = t.ExpandedElement.Get().m_AllFeatures;
            //}

            // repeat progression
            var list = new List<LevelEntry>();
            var progression = Helper.Get<BlueprintProgression>("b79e92dd495edd64e90fb483c504b8df"); //KineticistProgression
            var entry_wildtalent = progression.GetLevelEntry(2);
            var entry_infusion = progression.GetLevelEntry(3);
            for (int i = 21; i <= 40; i++)
            {
                if (i % 2 == 0)
                    list.Add(entry_wildtalent.Clone(a => a.Level = i));
                else if (i % 8 != 7)
                    list.Add(entry_infusion.Clone(a => a.Level = i));
                else
                    list.Add(Helper.CreateLevelEntry(i, t.ExpandedElement));
            }
            progression.AddEntries(list);

            list.Clear();
            var blast = Helper.Get<BlueprintProgression>("30a5b8cf728bd4a4d8d90fc4953e322e"); //KineticBlastProgression
            var entry_blast = blast.GetLevelEntry(1);
            for (int i = 21; i <= 40; i++)
            {
                if (i % 2 == 1)
                    list.Add(entry_blast.Clone(a => a.Level = i));
            }
            blast.AddEntries(list);

            list.Clear();
            var specialist = Helper.Get<BlueprintProgression>("1f86ce843fbd2d548a8d88ea1b652452"); //InfusionSpecializationProgression
            var entry_specialist = specialist.GetLevelEntry(5);
            for (int i = 21; i <= 40; i++)
            {
                if (i % 3 == 2)
                    list.Add(entry_specialist.Clone(a => a.Level = i));
            }
            specialist.AddEntries(list);
        }
    }
}
