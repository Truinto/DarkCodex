using Kingmaker.Settings;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Like AddFacts, but will disable Activatables before level up.
    /// </summary>
    public class AddFactsSafe : UnitFactComponentDelegate<AddFactsData>, IBeforeLevelUpHandler
    {
        public BlueprintUnitFactReference[] Facts;
        public int CasterLevel;
        public bool DoNotRestoreMissingFacts;

        /// <param name="facts">type: <b>BlueprintUnitFact</b> but not BlueprintParametrizedFeature, BlueprintFeatureSelection</param>
        public AddFactsSafe(params AnyRef[] facts)
        {
            this.Facts = facts.ToRef<BlueprintUnitFactReference>();
        }

        public override void OnActivate()
        {
            if (this.IsReapplying)
            {
                foreach (var fact in this.Data.AppliedFacts)
                    fact?.Reapply();
            }
            else
            {
                UpdateFacts(false);
            }
        }

        public override void OnDeactivate()
        {
            if (!this.IsReapplying)
                Clear();
        }

        public override void OnRecalculate()
        {
            foreach (var fact in this.Data.AppliedFacts)
                fact?.Reapply();
        }

        public override void OnApplyPostLoadFixes()
        {
            if (this.Fact.Active)
                UpdateFacts(true);
        }

        private void Clear()
        {
            this.Data.AppliedFacts.ForEach(f => this.Owner.RemoveFact(f));
            this.Data.AppliedFacts.Clear();
        }

        private void UpdateFacts(bool postLoad)
        {
            this.Data.AppliedFacts.RemoveAll(f => f == null || f.IsDisposed || !f.IsAttached);

            var toRemove = TempList.Get<UnitFact>();
            foreach (var fact in this.Data.AppliedFacts)
            {
                if (!this.Facts.HasReference(fact.Blueprint))
                {
                    toRemove.Add(fact);
                }
            }

            var toAdd = TempList.Get<BlueprintUnitFact>();
            if (!postLoad || !this.DoNotRestoreMissingFacts)
            {
                foreach (var fact in this.Facts)
                {
                    if (!this.Data.AppliedFacts.HasItem(f => fact.Is(f.Blueprint)))
                    {
                        toAdd.Add(fact);
                    }
                }
            }

            foreach (var fact in toRemove)
            {
                this.Data.AppliedFacts.Remove(fact);
                this.Owner.RemoveFact(fact);
            }

            foreach (var fact in toAdd)
            {
                var unitFact = fact.CreateFact(null, this.Owner.Descriptor, null);
                if (this.CasterLevel > 0 && unitFact.MaybeContext != null)
                    unitFact.MaybeContext.Params.CasterLevel = this.CasterLevel;

                unitFact = this.Owner.Facts.Add(unitFact);

                if (this.Context.Root.SourceItem != null)
                    unitFact.SetSourceItem(this.Context.Root.SourceItem);

                this.Data.AppliedFacts.Add(unitFact);
            }
        }

        public void BeforeLevelUp()
        {
            foreach (var fact in this.Facts)
            {
                if (fact.Get() is BlueprintActivatableAbility bp)
                {
                    var act = this.Owner.ActivatableAbilities.GetFact(bp);
                    if (act == null)
                        continue;

                    Helper.PrintDebug("AddFactsSafe BeforeLevelUp " + act.Name);
                    act.IsOn = false;
                }
            }
        }
    }
}
