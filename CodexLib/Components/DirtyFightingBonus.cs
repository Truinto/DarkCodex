using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class DirtyFightingBonus : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateCMB>
    {
        public static Dictionary<BlueprintAbilityReference, BlueprintUnitFactReference> List = new()
        {
            { Helper.ToRef<BlueprintAbilityReference>("6fd05c4ecfebd6f4d873325de442fc17"), Helper.ToRef<BlueprintUnitFactReference>("0f15c6f70d8fb2b49aa6cc24239cc5fa") }, // Trip
            { Helper.ToRef<BlueprintAbilityReference>("45d94c6db453cfc4a9b99b72d6afe6f6"), Helper.ToRef<BlueprintUnitFactReference>("25bc9c439ac44fd44ac3b1e58890916f") }, // Disarm
            { Helper.ToRef<BlueprintAbilityReference>("fa9bfb9fd997faf49a108c4b17a00504"), Helper.ToRef<BlueprintUnitFactReference>("9719015edcbf142409592e2cbaab7fe1") }, // Sunder
            { Helper.ToRef<BlueprintAbilityReference>("7ab6f70c996fe9b4597b8332f0a3af5f"), Helper.ToRef<BlueprintUnitFactReference>("b3614622866fe7046b787a548bbd7f59") }, // Bullrush
            { Helper.ToRef<BlueprintAbilityReference>("8b7364193036a8d4a80308fbe16c8187"), Helper.ToRef<BlueprintUnitFactReference>("ed699d64870044b43bb5a7fbe3f29494") }, // DirtyTrick Blindness
            { Helper.ToRef<BlueprintAbilityReference>("5f22daa9460c5844992bf751e1e8eb78"), Helper.ToRef<BlueprintUnitFactReference>("ed699d64870044b43bb5a7fbe3f29494") }, // DirtyTrick Entangle
            { Helper.ToRef<BlueprintAbilityReference>("4921b86ee42c0b54e87a2f9b20521ab9"), Helper.ToRef<BlueprintUnitFactReference>("ed699d64870044b43bb5a7fbe3f29494") }, // DirtyTrick Sickened
        };

        public void OnEventAboutToTrigger(RuleCalculateCMB evt)
        {
            var sourceAbility = this.Context.SourceAbility;
            if (sourceAbility == null)
                return;

            bool flanked = evt.Target.CombatState.IsFlanked;
            bool hasFeat = true;
            foreach (var pair in List)
            {
                if (!pair.Key.Is(sourceAbility))
                    continue;
                hasFeat = evt.Initiator.HasFact(pair.Value);
                break;
            }

            if (flanked && hasFeat)
                evt.AddModifier(2, this.Fact, ModifierDescriptor.UntypedStackable);
            else if (flanked)
                evt.AddModifier(-2, this.Fact, ModifierDescriptor.UntypedStackable);
            else if (!hasFeat)
                evt.AddModifier(-4, this.Fact, ModifierDescriptor.UntypedStackable);
        }

        public void OnEventDidTrigger(RuleCalculateCMB evt)
        {
        }
    }
}
