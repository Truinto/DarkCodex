using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Class.Kineticist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    /// <summary>
    /// Add components to "Components" to add new entries automatically.
    /// </summary>
    public class AbilityRegister : List<BlueprintAbilityReference>
    {
        public AbilityRegister() : base() { }

        public AbilityRegister(IEnumerable<BlueprintAbilityReference> collection, params BlueprintComponent[] comps) : base(collection)
        {
            this.Components = comps.ToList();
            _cache = collection.ToArray();
        }

        /// <param name="guids">Blueprints to update. guids[0] defines starting values.</param>
        public AbilityRegister(params string[] guids)
        {
            if (guids == null || guids.Length == 0)
                new ArgumentException("Must supply at least one guid");

            this.Components = new List<BlueprintComponent>();
            foreach (var guid in guids)
            {
                var bp = ResourcesLibrary.TryGetBlueprint<BlueprintScriptableObject>(guid);
                Register(bp);
            }

            base.AddRange(Get(this.Components[0]));
        }

        private BlueprintAbilityReference[] _cache;

        /// <summary>Components that contain and should be updated with a list of BlueprintAbility.</summary>
        public List<BlueprintComponent> Components;

        /// <summary>Filter components to keep updated.</summary>
        public void Register(BlueprintScriptableObject blueprint)
        {
            if (blueprint == null)
                Helper.PrintError("AbilityRegister blueprint is null");

            bool found = false;
            foreach (var comp in blueprint.ComponentsArray)
            {
                if (Allowed(comp))
                {
                    this.Components.Add(comp);
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
                Set(comp);
        }

        private bool Allowed(BlueprintComponent comp)
        {
            return comp is AddKineticistBurnModifier || comp is AutoMetamagic;
        }

        private void Set(BlueprintComponent comp)
        {
            if (comp is AddKineticistBurnModifier comp1)
                comp1.m_AppliableTo = _cache;
            else if (comp is AutoMetamagic comp2)
                comp2.Abilities = this;
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
            return null;
        }
    }
}
