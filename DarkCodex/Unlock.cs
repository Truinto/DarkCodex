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

namespace DarkCodex
{
    public class Unlock
    {
        [PatchInfo(Severity.Create | Severity.WIP, "Unlock Kineticist", "adds infusion, wild talent, and element focus up to level 40", true, Requirement: typeof(Patch_UnlockClassLevels))]
        public static void UnlockKineticist()
        {
            var t = Kineticist.Tree;

            // replace original with simplified
            if (t.ExpandedElement != null)
            {
                t.FocusSecond.m_AllFeatures = t.ExpandedElement.m_AllFeatures;
                t.FocusThird.m_AllFeatures = t.ExpandedElement.m_AllFeatures;
            }

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
