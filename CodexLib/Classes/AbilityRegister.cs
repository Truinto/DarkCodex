using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Class.Kineticist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Will keep lists of BlueprintAbilityReference updated. 
    /// Use Register() to add to watchlist and use Add() to add new entries to all registered features.
    /// May override 3rd party changes.
    /// </summary>
    [Obsolete]
    public class AbilityRegister : List<BlueprintAbilityReference>
    {
        public AbilityRegister() : base() { }

        /// <param name="guids">Blueprints to update. guids[0] defines starting values.</param>
        public AbilityRegister(params string[] guids)
        {
            if (guids != null)
            {
                foreach (var guid in guids)
                    Register(Helper.Get<BlueprintScriptableObject>(guid), null);
            }

            if (this.Components.Count > 0)
            {
                base.AddRange(Get(this.Components[0].comp));
                _cache = base.ToArray();
            }

            foreach (var comp in this.Components)
                Set(comp.comp, null);
        }

        private BlueprintAbilityReference[] _cache;

        /// <summary>Components that contain and should be updated with a list of BlueprintAbility.</summary>
        public List<(BlueprintComponent comp, Func<BlueprintAbilityReference, bool> pred)> Components = new();

        /// <summary>Filter components to keep updated.</summary>
        public void Register(BlueprintScriptableObject blueprint, Func<BlueprintAbilityReference, bool> pred = null)
        {
            if (blueprint == null)
                Helper.PrintError("AbilityRegister blueprint is null");

            bool found = false;
            foreach (var comp in blueprint.ComponentsArray)
            {
                if (Allowed(comp))
                {
                    this.Components.Add((comp, pred));

                    if (_cache != null)
                        Set(comp, pred);

                    found = true;
                }
            }
            if (!found)
                Helper.PrintError("AbilityRegister unsupported blueprint: " + blueprint.name);
        }

        /// <summary>Adds this ability to all components.</summary>
        public void Add(BlueprintAbility ability)
        {
            Add(ability.ToRef());
        }

        /// <summary>Adds this ability to all components.</summary>
        public new void Add(BlueprintAbilityReference ability)
        {
            base.Add(ability);
            _cache = base.ToArray();

            foreach (var comp in this.Components)
                Set(comp.comp, comp.pred);
        }

        private bool Allowed(BlueprintComponent comp)
        {
            return comp is AddKineticistBurnModifier || comp is AutoMetamagic;
        }

        private void Set(BlueprintComponent comp, Func<BlueprintAbilityReference, bool> pred = null)
        {
            if (comp is AddKineticistBurnModifier comp1)
                comp1.m_AppliableTo = pred == null ? _cache : this.Where(pred).ToArray();
            else if (comp is AutoMetamagic comp2)
                comp2.Abilities = pred == null ? this : this.Where(pred).ToList();
            else
                Helper.PrintError("Illegal Set component");
        }

        private List<BlueprintAbilityReference> Get(BlueprintComponent comp)
        {
            if (comp is AddKineticistBurnModifier comp1)
                return comp1.m_AppliableTo.ToList();
            else if (comp is AutoMetamagic comp2)
                return comp2.Abilities;
            else
                Helper.PrintError("Illegal Get component");
            return new();
        }
    }
}
