using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    [HarmonyPatch]
    internal class Patch_ModifiableValue
    {
        [HarmonyPatch(typeof(ModifiableValue), nameof(ModifiableValue.HandleModifierAdded))]
        [HarmonyPostfix]
        public static void Postfix1(ModifiableValue.Modifier mod, ModifiableValue __instance)
        {
            __instance.Owner?.Unit.Get<UnitPartModifierBonus>()?.HandleModifierChanged(__instance, mod);
        }

        [HarmonyPatch(typeof(ModifiableValue), nameof(ModifiableValue.PrepareForRemoval))]
        [HarmonyPostfix]
        public static void Postfix2(ModifiableValue.Modifier mod, ModifiableValue __instance)
        {
            __instance.Owner?.Unit.Get<UnitPartModifierBonus>()?.HandleModifierChanged(__instance, mod);
        }
    }

    /// <summary>Unfinished; use IncreaseModifierBonus instead.</summary>
    internal class UnitPartModifierBonus : UnitPart
    {
        [JsonProperty]
        public List<Data> Bonuses = new();

        public void AddBonus(int value, ModifierDescriptor descriptor, EntityFactComponent source)
        {
            Bonuses.Add(new Data(value, descriptor, source.Fact, source.SourceBlueprintComponentName));

            // to do apply missing bonuses
        }

        public void RemoveBonus(EntityFactComponent source)
        {
            foreach (var stat in this.Owner.Stats.AllStats)
                stat.RemoveModifiersFrom(source);

            Bonuses.RemoveAll(f => f.Source == source.Fact);
            if (Bonuses.Count == 0)
                RemoveSelf();
        }

        public override void OnApplyPostLoadFixes()
        {
            // review ModifiableValue.CleanupModifiers
            // to do reapply all missing bonuses
        }

#pragma warning disable IDE0060 // Nicht verwendete Parameter entfernen
        public void HandleModifierChanged(ModifiableValue modifiableValue, ModifiableValue.Modifier modifier)
        {
            if (modifier.StackMode == ModifiableValue.StackMode.ForceStack)
                return;
        }

        public struct Data
        {
            [JsonProperty]
            public int Value;
            [JsonProperty]
            public ModifierDescriptor Descriptor;
            [JsonProperty]
            public EntityFact Source;
            [JsonProperty]
            public string SourceComponentName;

            public Data(int value, ModifierDescriptor descriptor, EntityFact source, string sourceComponentName)
            {
                this.Value = value;
                this.Descriptor = descriptor;
                this.Source = source;
                this.SourceComponentName = sourceComponentName;
            }
        }
    }
}
