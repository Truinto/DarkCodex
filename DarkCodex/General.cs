using DarkCodex.Components;
using HarmonyLib;
using Kingmaker;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.Scenes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.Common;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.View.MapObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace DarkCodex
{
    public class General
    {
        [HarmonyPatch(typeof(BlueprintParametrizedFeature), nameof(BlueprintParametrizedFeature.CanSelect))]
        public class DEBUGTEST // todo: bugfix ability focus
        {
            public static bool Prefix(BlueprintParametrizedFeature __instance, ref bool __result, UnitDescriptor unit, LevelUpState state, FeatureSelectionState selectionState, IFeatureSelectionItem item)
            {
                if (__instance.ParameterType != FeatureParameterType.Custom)
                    return true;

                if (item.Param == null)
                    __result = false;
                //else if (__instance.Items.FirstOrDefault(i => i.Feature == item.Feature && i.Param == item.Param) == null)
                //    __result = false;
                else if (unit.GetFeature(__instance, item.Param) != null)
                    __result = false;
                else if (!unit.HasFact(item.Param.Blueprint as BlueprintFact))
                    __result = false;
                else
                    __result = true;

                return false;
            }
        }

        [PatchInfo(Severity.Create, "Ability Focus", "basic feat: Ability Focus, increase DC of one ability by +2", false)]
        public static void createAbilityFocus()
        {
            Resource.Cache.Ensure();

            var abilities = Resource.Cache.Ability.Where(ability =>
            {
                if (ability.Type == AbilityType.Spell)
                    return false;

                if (ability.m_DisplayName.IsEmpty())
                    return false;

                if (ability.HasVariants)
                    return false;

                var run = ability.GetComponent<AbilityEffectRunAction>();
                if (run == null)
                    return false;

                return run.SavingThrowType != SavingThrowType.Unknown;
            }).ToArray();

            var feat = Helper.CreateBlueprintParametrizedFeature(
                "AbilityFocusCustom",
                "Ability Focus",
                "Choose one special attack. Add +2 to the DC for all saving throws against the special attack on which you focus.",
                blueprints: abilities.ToRef3()
                ).SetComponents(
                new AbilityFocusParametrized()
                );
            feat.Ranks = 10;

            return;

            Helper.AddFeats(feat);
        }
        
        [PatchInfo(Severity.Extend, "Empower Angels Light", "'Light of the Angels' give temporary HP equal to character level", true)]
        public static void patchAngelsLight()
        {
            var angelbuff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("e173dc1eedf4e344da226ffbd4d76c60"); // AngelMinorAbilityEffectBuff

            var temphp = angelbuff.GetComponent<TemporaryHitPointsFromAbilityValue>();
            temphp.Value = Helper.CreateContextValue(AbilityRankType.Default);
            angelbuff.AddComponents(Helper.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel, type: AbilityRankType.Default)); // see FalseLifeBuff
        }

        [PatchInfo(Severity.Extend, "Basic Freebie Feats", "reduced feat tax, inspired from https://michaeliantorno.com/feat-taxes-in-pathfinder/", true)]
        public static void patchBasicFreebieFeats()
        {
            //https://michaeliantorno.com/feat-taxes-in-pathfinder/

            var basics = ResourcesLibrary.TryGetBlueprint<BlueprintProgression>("5b72dd2ca2cb73b49903806ee8986325"); //BasicFeatsProgression
            basics.AddComponents(
                Helper.CreateAddFactOnlyParty(Helper.ToRef<BlueprintUnitFactReference>("9972f33f977fc724c838e59641b2fca5")), //PowerAttackFeature
                Helper.CreateAddFactOnlyParty(Helper.ToRef<BlueprintUnitFactReference>("0da0c194d6e1d43419eb8d990b28e0ab")), //PointBlankShot
                Helper.CreateAddFactOnlyParty(Helper.ToRef<BlueprintUnitFactReference>("4c44724ffa8844f4d9bedb5bb27d144a")), //CombatExpertiseFeature
                Helper.CreateAddFactOnlyParty(Helper.ToRef<BlueprintUnitFactReference>("90e54424d682d104ab36436bd527af09")), //WeaponFinesse
                Helper.CreateAddFactOnlyParty(Helper.ToRef<BlueprintUnitFactReference>("f47df34d53f8c904f9981a3ee8e84892")) //DeadlyAimFeature
                );

            var powerattack = ResourcesLibrary.TryGetBlueprint<BlueprintActivatableAbility>("a7b339e4f6ff93a4697df5d7a87ff619"); //PowerAttackToggleAbility
            powerattack.IsOnByDefault = false;
            powerattack.DoNotTurnOffOnRest = true;
            var combatexpertise = ResourcesLibrary.TryGetBlueprint<BlueprintActivatableAbility>("a75f33b4ff41fc846acbac75d1a88442"); //CombatExpertiseToggleAbility
            combatexpertise.IsOnByDefault = false;
            combatexpertise.DoNotTurnOffOnRest = true;
            combatexpertise.DeactivateIfCombatEnded = false;
            combatexpertise.DeactivateAfterFirstRound = false;
            combatexpertise.ActivationType = AbilityActivationType.Immediately;
            combatexpertise.DeactivateIfOwnerDisabled = true;
            var deadlyaim = ResourcesLibrary.TryGetBlueprint<BlueprintActivatableAbility>("ccde5ab6edb84f346a74c17ea3e3a70c"); //DeadlyAimToggleAbility
            deadlyaim.IsOnByDefault = false;
            deadlyaim.DoNotTurnOffOnRest = true;

            var mobility = Helper.ToRef<BlueprintUnitFactReference>("2a6091b97ad940943b46262600eaeaeb"); //Mobility
            var dodge = Helper.ToRef<BlueprintUnitFactReference>("97e216dbb46ae3c4faef90cf6bbe6fd5"); //Dodge
            mobility.Get().AddComponents(Helper.CreateAddFactOnlyParty(dodge));
            dodge.Get().AddComponents(Helper.CreateAddFactOnlyParty(mobility));

            var twf = Helper.ToRef<BlueprintUnitFactReference>("ac8aaf29054f5b74eb18f2af950e752d"); //TwoWeaponFighting
            var twfi = Helper.ToRef<BlueprintUnitFactReference>("9af88f3ed8a017b45a6837eab7437629"); //TwoWeaponFightingImproved
            var twfg = Helper.ToRef<BlueprintUnitFactReference>("c126adbdf6ddd8245bda33694cd774e8"); //TwoWeaponFightingGreater
            var multi = Helper.ToRef<BlueprintUnitFactReference>("8ac319e47057e2741b42229210eb43ed"); //Multiattack
            twf.Get().AddComponents(Helper.CreateAddFactOnlyParty(multi));
            twfi.Get().AddComponents(Helper.CreateAddFactOnlyParty(twfg));

            //Deft Maneuvers
            //ImprovedTrip.0f15c6f70d8fb2b49aa6cc24239cc5fa
            //ImprovedDisarm.25bc9c439ac44fd44ac3b1e58890916f
            //ImprovedDirtyTrick.ed699d64870044b43bb5a7fbe3f29494

            //Powerful Maneuvers
            //ImprovedBullRush.b3614622866fe7046b787a548bbd7f59
            //ImprovedSunder.9719015edcbf142409592e2cbaab7fe1
        }

        [PatchInfo(Severity.Create, "Preferred Spell", "basic feat: Preferred Spell, spontaneously cast a specific spell", false, Requirement: typeof(Patch_PreferredSpellMetamagic))]
        public static void createPreferredSpell()
        {
            var specialization = ResourcesLibrary.TryGetBlueprint<BlueprintParametrizedFeature>("f327a765a4353d04f872482ef3e48c35"); //SpellSpecializationFirst
            var wizard = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("8c3102c2ff3b69444b139a98521a4899"); //WizardFeatSelection
            var heighten = Helper.ToRef<BlueprintFeatureReference>("2f5d1e705c7967546b72ad8218ccf99c"); //HeightenSpellFeat

            var feat = Helper.CreateBlueprintParametrizedFeature(
                "PreferredSpellFeature",
                "Preferred Spell",
                "Choose one spell which you have the ability to cast. You can cast that spell spontaneously by sacrificing a prepared spell or spell slot of equal or higher level. You can apply any metamagic feats you possess to this spell when you cast it. This increases the minimum level of the prepared spell or spell slot you must sacrifice in order to cast it but does not affect the casting time.\nSpecial: You can gain this feat multiple times.Its effects do not stack. Each time you take the feat, it applies to a different spell.",
                icon: null,
                parameterType: FeatureParameterType.SpellSpecialization,
                blueprints: specialization.BlueprintParameterVariants
                ).SetComponents(
                new PreferredSpell(),
                Helper.CreatePrerequisiteFullStatValue(StatType.SkillKnowledgeArcana, 5),
                Helper.CreatePrerequisiteFeature(heighten)
                );
            feat.Groups = new FeatureGroup[] { FeatureGroup.Feat, FeatureGroup.WizardFeat };
            feat.Ranks = 10;

            Helper.AddFeats(feat);
            Helper.AppendAndReplace(ref wizard.m_AllFeatures, feat.ToRef());
        }

        [PatchInfo(Severity.Extend, "Hide Buffs", "unclogs UI by hidding a few buffs", false)]
        public static void patchHideBuffs()
        {
            string[] guids = new string[] {
                "359e8fc68f81b5d4e96fae22be5e439f", //Artifact_RingOfSummonsBuff
                "4677cfde5b184a94e898425d88a4665a", //MetamagicRodLesserKineticBuff
                "7c4ebf464651bbe4798f25e839cead25", //HatOfHearteningSongEffectBuff
            };

            foreach (var guid in guids)
            {
                try
                {
                    var buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>(guid);
                    buff.Flags(hidden: true);
                }
                catch (Exception)
                {
                    Helper.Print(" error: couldn't load " + guid);
                }
            }
        }
        
        [PatchInfo(Severity.Extend, "Various Tweaks", "removed PreciousTreat penalty, extend protection from X to 10 minutes", true)]
        public static void patchVarious()
        {
            // remove penalty on Precious Treat item
            var buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("ee8ee3c5c8f055e48a1ec1bfb92778f1"); //PreciousTreatBuff
            buff.RemoveComponents(default(AddStatBonus));

            // extend protection from X to 10 minutes
            var pfa = new string[] {
                "2cadf6c6350e4684baa109d067277a45", //ProtectionFromAlignmentCommunal
                "8b8ccc9763e3cc74bbf5acc9c98557b9", //ProtectionFromLawCommunal
                "0ec75ec95d9e39d47a23610123ba1bad", //ProtectionFromChaosCommunal
                "93f391b0c5a99e04e83bbfbe3bb6db64", //ProtectionFromEvilCommunal
                "5bfd4cce1557d5744914f8f6d85959a4", //ProtectionFromGoodCommunal
                "3026de673d4d8fe45baf40e0b5edd718", //ProtectionFromChaosEvilCommunal
                "b6da529f710491b4fa789a5838c1ae8f", //ProtectionFromChaosCommunalChaosEvil
                "224f03e74d1dd4648a81242c01e65f41", //ProtectionFromEvilCommunalChaosEvil
            };
            foreach (var guid in pfa)
            {
                try
                {
                    var ab = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(guid);
                    ab.LocalizedDuration = Resource.Strings.TenMinutes;
                    if (ab != null)
                        (ab.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionApplyBuff).DurationValue.BonusValue = 10;
                }
                catch (Exception) { }
            }

            // demon graft respects immunities
            var discordRage = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("5d3029eb16956124da2c6b79ed32c675"); //SongOfDiscordEnragedBuff
            discordRage.AddComponents(Helper.CreateSpellDescriptorComponent(SpellDescriptor.MindAffecting | SpellDescriptor.Compulsion));

            // fix BloodlineUndeadArcana and DirgeBardSecretsOfTheGraveFeature not allowing shaken/confusion
            var undeadImmunities = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("8a75eb16bfff86949a4ddcb3dd2f83ae"); //UndeadImmunities
            undeadImmunities.GetComponents<BuffDescriptorImmunity>().First(f => f.IgnoreFeature == null)
                .Descriptor &= ~SpellDescriptor.Shaken & ~SpellDescriptor.Confusion;
            undeadImmunities.GetComponents<SpellImmunityToSpellDescriptor>().First(f => f.CasterIgnoreImmunityFact == null)
                .Descriptor &= ~SpellDescriptor.Shaken & ~SpellDescriptor.Confusion;

            // add icon to CompletelyNormal metamagic
            if (UIRoot.Instance.SpellBookColors.MetamagicCompletelyNormal == null)
            {
                var cnfeat = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("094b6278f7b570f42aeaa98379f07cf2"); //CompletelyNormalSpellFeat
                cnfeat.m_Icon = Helper.CreateSprite("CompletelyNormal.png");
                UIRoot.Instance.SpellBookColors.MetamagicCompletelyNormal = cnfeat.m_Icon;
            }

            // add confusion descriptor to song of discord
            var songDiscord = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("2e1646c2449c88a4188e58043455a43a"); //SongOfDiscordBuff
            songDiscord.GetComponent<SpellDescriptorComponent>().Descriptor |= SpellDescriptor.Confusion;

            // disable area effects during cutscenes?
            // AreaEffectView.SpawnFxs()
            //AreaEffectsController.Spawn
            //AreaEffectEntityData a = null;
            //a.IsDisposed
            //a.View.m_SpawnedFx.SetActive(false);

            // attempt to have symbol stay activated
            var holysymbol = ResourcesLibrary.TryGetBlueprint<BlueprintActivatableAbility>("e67dc14ff73014547920dc92bc8e1bfc"); //Artifact_HolySymbolOfIomedaeToggleAbility
            holysymbol.DoNotTurnOffOnRest = true;
        }
        
        [PatchInfo(Severity.Extend, "Dispel Magic", "fix Destructive Dispel, apply bonus", true)]
        public static void patchDispelMagic()
        {
            //DispellingAttack.1b92146b8a9830d4bb97ab694335fa7c FEATURE
            //DispellingBomb.f80896af0e10d7c4f9454cf1ce50ada4
            var destructive = Helper.ToRef<BlueprintUnitFactReference>("d298e64e14398e848a54db5a2619ba42"); //DestructiveDispel
            var sickened = Helper.ToRef<BlueprintBuffReference>("4e42460798665fd4cb9173ffa7ada323"); //Sickened
            var stunned = Helper.ToRef<BlueprintBuffReference>("09d39b38bb7c6014394b6daced9bacd3"); //Stunned

            var dispel_target = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("143775c49ae6b7446b805d3b2e702298"); //DispelMagicTarget
            var dispel_point = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("9f6daa93291737c40b8a432c374226a7"); //DispelMagicPoint
            var dispel_target_greater = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("6d490c80598f1d34bb277735b52d52c1"); //DispelMagicGreaterTarget
            var dispel_point_greater = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("b9be852b03568064b8d2275a6cf9e2de"); //DispelMagicGreaterArea

            var contextbonus = new ContextValue() { ValueType = ContextValueType.CasterCustomProperty, m_CustomProperty = Resource.Cache.PropertyMythicDispel };
            var onsuccess = Helper.CreateActionList(Helper.CreateConditional(Helper.CreateContextConditionCasterHasFact(destructive),
                ifTrue: Helper.MakeContextActionSavingThrow(SavingThrowType.Fortitude,
                    succeed: Helper.CreateContextActionApplyBuff(sickened, 1),
                    failed: Helper.CreateContextActionApplyBuff(stunned, 1))
                ));

            var action = (ContextActionDispelMagic)dispel_target.GetComponent<AbilityEffectRunAction>().Actions.Actions[0];
            action.ContextBonus = contextbonus;
            action.OnSuccess = onsuccess;

            action = (ContextActionDispelMagic)dispel_point.GetComponent<AbilityEffectRunAction>().Actions.Actions[0];
            action.OnSuccess = onsuccess;

            action = (ContextActionDispelMagic)dispel_target_greater.GetComponent<AbilityEffectRunAction>().Actions.Actions[0];
            action.ContextBonus = contextbonus;
            action.OnSuccess = onsuccess;

            action = (ContextActionDispelMagic)(dispel_point_greater.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as Conditional).IfFalse.Actions[0];
            action.ContextBonus = contextbonus;
            action.OnSuccess = onsuccess;
        }

        [PatchInfo(Severity.Create, "Stop Activatable", "adds ability to stop any activatable immediately", false)]
        public static void createBardStopSong()
        {
            //ContextActionStopActivatables
            Helper.CreateBlueprintAbility(
                "StopActivatables",
                "Stop Activatables",
                "Immediately stop all disabled activatables. Useful for bardic performance.",
                null,
                icon: Helper.CreateSprite("StopSong.png"),
                AbilityType.Special,
                UnitCommand.CommandType.Free,
                AbilityRange.Personal
                ).TargetSelf().SetComponents(
                Helper.CreateAbilityExecuteActionOnCast(new ContextActionStopActivatables())
                );
        }
    }

    [PatchInfo(Severity.Event, "Event: Area Effects", "mute player area effects while in dialog", false)]
    public class Control_AreaEffects : IDialogStartHandler, IDialogFinishHandler, IPartyCombatHandler, IGlobalSubscriber, ISubscriber //ICutsceneHandler, ICutsceneDialogHandler
    {
        //Game.Instance.IsModeActive(GameModeType.Dialog) || Game.Instance.IsModeActive(GameModeType.Cutscene);

        private static readonly List<AreaEffectEntityData> paused = new List<AreaEffectEntityData>();

        public static void Stop()
        {
            foreach (var effect in Game.Instance.State.AreaEffects)
            {
                var caster = (effect.Context.ParentContext as AbilityExecutionContext)?.MaybeCaster;
                if (caster == null || !caster.IsPlayerFaction)
                    continue;

                if (effect.Destroyed || effect.DestroyMark)
                    continue;

                var fx = effect.View.m_SpawnedFx;
                if (fx == null || !fx.activeSelf)
                    continue;

                fx.SetActive(false);
                paused.Add(effect);
                Helper.PrintDebug(" pausing effect " + effect);
            }
        }

        public static void Continue()
        {
            for (int i = paused.Count - 1; i >= 0; i--)
            {
                if (!paused[i].Destroyed && !paused[i].DestroyMark)
                {
                    paused[i].View.m_SpawnedFx?.SetActive(true);
                    Helper.PrintDebug(" continuing effect " + paused[i]);
                }

                paused.RemoveAt(i);
            }
        }

        public void HandleDialogStarted(BlueprintDialog dialog)
        {
            Helper.PrintDebug("Dialog started...");

            if (Settings.StateManager.State.stopAreaEffectsDuringCutscenes)
                Stop();
        }

        public void HandleDialogFinished(BlueprintDialog dialog, bool success)
        {
            Helper.PrintDebug("Dialog finished...");
            Continue();
        }

        public void HandlePartyCombatStateChanged(bool inCombat)
        {
            if (!inCombat)
                return;

            foreach (var unit in Game.Instance.Player.PartyAndPets)
            {
                foreach (var buff in unit.Buffs)
                {
                    var areaeffect = buff.GetComponent<AddAreaEffect>();
                    if (areaeffect == null)
                        continue;

                    if (areaeffect.Data.AreaEffectInstance == null)
                    {
                        Helper.PrintDebug("Enabling missing area effect for " + areaeffect.AreaEffect.NameSafe());
                        areaeffect.OnActivate();
                        continue;
                    }

                    if (!AreaService.Instance.CurrentAreaPart.Bounds.MechanicBounds.Contains(areaeffect.Data.AreaEffectInstance.Entity.Position))
                    {
                        Helper.PrintDebug("Restarting area effect stuck in wrong area " + areaeffect.AreaEffect.NameSafe());
                        areaeffect.OnDeactivate();
                        areaeffect.OnActivate();
                    }
                }
            }
        }
    }

    [PatchInfo(Severity.Harmony, "Patch: Fix Load Crash", "removes trap data during save load to prevent a specific crash", false)]
    [HarmonyPatch(typeof(AreaDataStash), nameof(AreaDataStash.GetJsonForArea))]
    public class Patch_FixLoadCrash1
    {
        public static void Postfix(ref string __result)
        {
            if (!Settings.StateManager.State.debug_2)
                return;

            if (__result == null || __result == "")
                return;

            Helper.Print(__result);
            __result = Regex.Replace(__result, @"{""\$id"":""[0-9]+"",""\$type"":""Kingmaker\.View\.MapObjects\.Traps\.Simple\.SimpleTrapObjectData, Assembly-CSharp"",.*?""UniqueId"":.*?},?", "");
        }
    }

    #region Patches

    [PatchInfo(Severity.Harmony, "Patch: Preferred Spell Metamagic", "necessary patches for Preferred Spell", false)]
    [HarmonyPatch]
    public class Patch_PreferredSpellMetamagic
    {
        [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.GetConversions))]
        [HarmonyPostfix]
        public static void Postfix1(AbilityData __instance, ref IEnumerable<AbilityData> __result)
        {
            if (__instance.Spellbook == null)
                return;

            var list = __result.ToList();

            try
            {
                foreach (var conlist in __instance.Spellbook.m_SpellConversionLists)
                {
                    if (!conlist.Key.StartsWith("PreferredSpell#"))
                        continue;

                    var targetspell = conlist.Value.Last();

                    list.FirstOrDefault(f => f.Blueprint == targetspell)?.MetamagicData?.Clear(); // clear metamagic copied from donor spell

                    var metamagics = __instance.Spellbook.GetCustomSpells(__instance.SpellLevel).Where(w => w.Blueprint == targetspell); //__instance.Spellbook.GetSpellLevel(__instance)
                    foreach (var metamagic in metamagics)
                    {
                        var variants = targetspell.GetComponent<AbilityVariants>()?.Variants;
                        if (variants == null)
                        {
                            list.Add(new AbilityData(targetspell, __instance.Caster)
                            {
                                m_ConvertedFrom = __instance,
                                MetamagicData = metamagic.MetamagicData?.Clone(),
                                DecorationBorderNumber = metamagic.DecorationBorderNumber,
                                DecorationColorNumber = metamagic.DecorationColorNumber
                            });
                        }
                        else
                        {
                            foreach (var variant in variants)
                            {
                                list.Add(new AbilityData(variant, __instance.Caster)
                                {
                                    m_ConvertedFrom = __instance,
                                    MetamagicData = metamagic.MetamagicData?.Clone(),
                                    DecorationBorderNumber = metamagic.DecorationBorderNumber,
                                    DecorationColorNumber = metamagic.DecorationColorNumber
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Helper.PrintException(e);
            }

            __result = list;
        }

        [HarmonyPatch(typeof(MechanicActionBarSlotSpontaneusConvertedSpell), nameof(MechanicActionBarSlotSpontaneusConvertedSpell.GetDecorationSprite))]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        public static void Postfix2(MechanicActionBarSlotSpontaneusConvertedSpell __instance, ref Sprite __result)
        {
            if (__instance.Spell?.ConvertedFrom != null)
                __result = UIUtility.GetDecorationBorderByIndex(__instance.Spell.DecorationBorderNumber);
        }

        [HarmonyPatch(typeof(MechanicActionBarSlotSpontaneusConvertedSpell), nameof(MechanicActionBarSlotSpontaneusConvertedSpell.GetDecorationColor))]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        public static void Postfix3(MechanicActionBarSlotSpontaneusConvertedSpell __instance, ref Color __result)
        {
            if (__instance.Spell?.ConvertedFrom != null)
                __result = UIUtility.GetDecorationColorByIndex(__instance.Spell.DecorationColorNumber);
        }
    }

    [PatchInfo(Severity.Harmony, "Patch: Spell Selection Parametrized", "fix spell selection for Preferred Spell", false)]
    [HarmonyPatch(typeof(BlueprintParametrizedFeature), nameof(BlueprintParametrizedFeature.ExtractItemsFromSpellbooks))]
    public class Patch_SpellSelectionParametrized
    {
        public static bool Prefix(UnitDescriptor unit, BlueprintParametrizedFeature __instance, ref IEnumerable<FeatureUIData> __result)
        {
            if (__instance.Prerequisite != null)
                return true;

            var list = new List<FeatureUIData>();

            foreach (Spellbook spellbook in unit.Spellbooks)
            {
                foreach (SpellLevelList spellLevel in spellbook.Blueprint.SpellList.SpellsByLevel)
                {
                    if (spellLevel.SpellLevel <= spellbook.MaxSpellLevel)
                    {
                        foreach (BlueprintAbility blueprintAbility in spellLevel.SpellsFiltered)
                        {
                            list.Add(new FeatureUIData(__instance, blueprintAbility, blueprintAbility.Name, blueprintAbility.Description, blueprintAbility.Icon, blueprintAbility.name));
                        }
                    }
                }
            }
            __result = list;
            return false;
        }
    }

    #endregion
}
