using HarmonyLib;
using Kingmaker;
using Kingmaker.Achievements;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Modding;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.View.MapObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TurnBased.Controllers;
using Kingmaker.UI.MVVM._VM.ActionBar;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.MVVM._VM.Tooltip.Templates;
using Newtonsoft.Json;
using UnityEngine;
using DarkCodex.Components;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Allow Achievements", "clears the 'has used mods before' flag and also pretends that no mods are active", false)]
    [HarmonyPatch(typeof(OwlcatModificationsManager), nameof(OwlcatModificationsManager.IsAnyModActive), MethodType.Getter)]
    public class Patch_AllowAchievements    // note: must be manually patched; it crashes with PatchAll
    {
        public static bool Patched;
        public static bool Prefix(ref bool __result)
        {
            if (!Settings.StateManager.State.allowAchievements)
                return true;

            if (Game.Instance?.Player != null)
                Game.Instance.Player.ModsUser = false;
            __result = false;
            return false;
        }
    }

    #region Activatables

    // Note: CallOfTheWild also patches this, but in a different manner. Not sure if there is potential in error. So far I haven't found any bugs in playtesting.
    // Also, the enum will change, if the order of calls are changed, e.g. new mods are added. I think that's not a problem. I can't think of a reason why it would be saved in the save file. Needs further testing.
    //[HarmonyPatch(typeof(EnumUtils), nameof(EnumUtils.GetMaxValue))] since this is a generic method, we need to patch this manually, see Main.Load
    public static class Patch_ActivatableAbilityGroup
    {
        public static int ExtraGroups = 0;
        public static bool GameAlreadyRunning = false;

        ///<summary>Calls this to register a new group. Returns your new enum.</summary>
        public static ActivatableAbilityGroup GetNewGroup()
        {
            if (GameAlreadyRunning)
                return 0;

            ExtraGroups++;
            Helper.PrintDebug("GetNewGroup new: " + (Enum.GetValues(typeof(ActivatableAbilityGroup)).Cast<int>().Max() + ExtraGroups).ToString());
            return (ActivatableAbilityGroup)(Enum.GetValues(typeof(ActivatableAbilityGroup)).Cast<int>().Max() + ExtraGroups);
        }

        public static void Postfix(ref int __result)
        {
            __result += ExtraGroups;
        }
    }

    [PatchInfo(Severity.Harmony, "Patch: Activatable OnNewRound", "uses up move action when triggered; deactivates activatable if no action left", false)]
    [HarmonyPatch(typeof(ActivatableAbility), nameof(ActivatableAbility.OnNewRound))]
    public static class Patch_ActivatableOnNewRound
    {
        public static void Postfix(ActivatableAbility __instance)
        {
            var cd = __instance.Owner.Unit.CombatState.Cooldown;
            if (cd.StandardAction > 0f && cd.MoveAction > 3f || cd.MoveAction > 6f)
                __instance.IsOn = false;
        }
    }

    [PatchInfo(Severity.Harmony, "Patch: Activatable HandleUnitRunCommand", "fixes move actions disabling the activatable (since we have 2 of them)", false)]
    [HarmonyPatch(typeof(ActivatableAbility), nameof(ActivatableAbility.HandleUnitRunCommand))]
    public static class Patch_ActivatableHandleUnitRunCommand
    {
        public static bool Prefix(UnitCommand command, ActivatableAbility __instance)
        {
            if (command.Type == UnitCommand.CommandType.Move) // skip this logic, if the trigger came from a move action
                return false;

            return true;
        }
    }

    [PatchInfo(Severity.Harmony, "Patch: Activatable OnTurnOn", "fixes activatable not being allowed to be active when they have the same action (like 2 move actions)", false)]
    [HarmonyPatch(typeof(ActivatableAbility), nameof(ActivatableAbility.OnDidTurnOn))]
    public static class Patch_ActivatableOnTurnOn
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
        {
            List<CodeInstruction> list = instr.ToList();
            MethodInfo original = AccessTools.Method(typeof(EntityFact), nameof(EntityFact.GetComponent), null, typeof(ActivatableAbilityUnitCommand).ObjToArray());
            MethodInfo replacement = AccessTools.Method(typeof(Patch_ActivatableOnTurnOn), nameof(NullReplacement));

            for (int i = 0; i < list.Count; i++)
            {
                var mi = list[i].operand as MethodInfo;
                if (mi != null && mi == original)
                {
                    Helper.PrintDebug("ActivatableAbility_OnTurnOnPatch at " + i);
                    list[i].operand = replacement;
                }
            }

            return list;
        }

        public static object NullReplacement(object something)
        {
            return null;
        }
    }

    [PatchInfo(Severity.Harmony, "Patch: Activatable ApplyValidation", "removes validation", false)]
    [HarmonyPatch(typeof(ActivatableAbilityUnitCommand), nameof(ActivatableAbilityUnitCommand.ApplyValidation))]
    public static class Patch_ActivatableApplyValidation
    {
        public static bool Prefix()
        {
            return false;
        }
    }

    [PatchInfo(Severity.Harmony, "Patch: Activatable TryStart", "fixes activatable not starting the second time, while being outside of combat", false)]
    [HarmonyPatch(typeof(ActivatableAbility), nameof(ActivatableAbility.TryStart))]
    public static class Patch_ActivatableTryStart
    {
        public static void Prefix(ActivatableAbility __instance)
        {
            if (!__instance.Owner.Unit.IsInCombat)
            {
                __instance.Owner.Unit.CombatState.Cooldown.SwiftAction = 0f;
                __instance.Owner.Unit.CombatState.Cooldown.MoveAction = 0f;
            }

        }
    }

    [PatchInfo(Severity.Harmony, "Patch: Activatable ActionBar", "adds logic for automatic-only activatable", false)]
    [HarmonyPatch(typeof(MechanicActionBarSlotActivableAbility), nameof(MechanicActionBarSlotActivableAbility.OnClick))]
    public static class Patch_ActivatableActionBar
    {
        public static readonly int NoManualOn = 788704819;
        public static readonly int NoManualOff = 788704820;
        public static readonly int NoManualAny = 788704821;

        public static bool Prefix(MechanicActionBarSlotActivableAbility __instance)
        {
            if (!__instance.ActivatableAbility.IsOn && __instance.ActivatableAbility.Blueprint.WeightInGroup == NoManualOn)
            {
                return false;
            }
            if (__instance.ActivatableAbility.IsOn && __instance.ActivatableAbility.Blueprint.WeightInGroup == NoManualOff)
            {
                return false;
            }
            if (__instance.ActivatableAbility.Blueprint.WeightInGroup == NoManualAny)
            {
                return false;
            }
            return true;
        }
    }

    #endregion

    /// <summary>If a component causes an exception, it will try to resolve the asset name.
    /// If that name is null or wrong formated, it will crash the report and hide any meaningful log entries.
    /// This patch fills the missing data with nonsense to prevent that bug.</summary>
    [PatchInfo(Severity.Harmony, "Patch: Debug Report", "fixes error log crashes due to unnamed components", false)]
    [HarmonyPatch(typeof(Element), nameof(Element.AssetGuidShort), MethodType.Getter)]
    public class Patch_DebugReport
    {
        public static void Prefix(Element __instance)
        {
            if (__instance.name == null)
                __instance.name = "$$empty";
        }
    }

    //[PatchInfo(Severity.Harmony | Severity.Event | Severity.WIP, "Patch: Fix Area Effects", "enforces a 6 second cooldown on area effects")]
    //[HarmonyPatch(typeof(AbilityAreaEffectRunAction))]
    public class Unused_Patch_FixAreaEffectDamage : IPartyCombatHandler, IGlobalSubscriber, ISubscriber
    {
        //base.Data.LastUseTime + 1.Rounds().Seconds > Game.Instance.TimeController.GameTime
        public static Dictionary<AbilityAreaEffectRunAction, Dictionary<UnitEntityData, TimeSpan>> times = new();

        [HarmonyPatch(nameof(AbilityAreaEffectRunAction.OnUnitEnter))]
        [HarmonyPostfix]
        public static void OnUnitEnter(MechanicsContext context, AreaEffectEntityData areaEffect, UnitEntityData unit, AbilityAreaEffectRunAction __instance)
        {
            Helper.PrintDebug($"AreaRunAction OnEnter unit={unit.CharacterName} area={areaEffect.Blueprint.name}");

            if (!__instance.UnitEnter.HasActions || !__instance.Round.HasActions)
                return;

            if (context.SourceAbility?.EffectOnAlly == AbilityEffectOnUnit.Helpful)
                return;

            if (!times.TryGetValue(__instance, out var units))
                times.Add(__instance, units = new());
            units[unit] = Game.Instance.TimeController.GameTime;
        }

        [HarmonyPatch(nameof(AbilityAreaEffectRunAction.OnRound))]
        [HarmonyPrefix]
        public static bool OnRound(MechanicsContext context, AreaEffectEntityData areaEffect, AbilityAreaEffectRunAction __instance)
        {
            if (!__instance.Round.HasActions)
                return false;

            using (ContextData<AreaEffectContextData>.Request().Setup(areaEffect))
            {
                var nowminus = Game.Instance.TimeController.GameTime - 1.Rounds().Seconds;

                foreach (UnitEntityData unit in areaEffect.InGameUnitsInside)
                {
                    if (times.TryGetValue(__instance, out var units) && units.TryGetValue(unit, out var lastuse) && lastuse > nowminus)
                    {
                        Helper.PrintDebug($"skipped OnRound unit={unit.CharacterName} lastuse={lastuse} nowminus={nowminus} area={areaEffect.Blueprint.name}");
                        continue;
                    }

                    using (context.GetDataScope(unit))
                    {
                        Helper.PrintDebug($"AreaRunAction OnRound unit={unit.CharacterName} nowminus={nowminus} area={areaEffect.Blueprint.name}");
                        __instance.Round.Run();
                    }
                }
            }
            return false;
        }

        public void HandlePartyCombatStateChanged(bool inCombat)
        {
            if (!inCombat)
                times.Clear();
        }
    }

    [PatchInfo(Severity.Harmony, "Patch: Fix Area Effect Double Damage", "fixes area effects triggering twice when cast", false)]
    [HarmonyPatch]
    public class Patch_FixAreaDoubleDamage
    {
        [HarmonyPatch(typeof(AreaEffectEntityData), MethodType.Constructor, typeof(AreaEffectView), typeof(MechanicsContext), typeof(BlueprintAbilityAreaEffect), typeof(TargetWrapper), typeof(TimeSpan), typeof(TimeSpan?), typeof(bool))]
        [HarmonyPostfix]
        public static void OnCast(ref float ___m_TimeToNextRound)
        {
            // start the OnRound timer at 6 seconds; this will fix the immediate trigger of OnEnter and OnRound at the same time
            ___m_TimeToNextRound = 6f;
        }
    }

    [PatchInfo(Severity.Harmony | Severity.WIP, "Patch: Area Effect End Of Turn", "in turn-based mode area effects happen at the end of each unit's round, instead of all at once at the start of the caster's round", false)]
    [HarmonyPatch]
    public class Patch_FixAreaEndOfTurn
    {
        /*
        Affected areas:
        AcidFogArea.f4dc3f53090627945b83f16ebf3146a6
        AeonHPGazeArea.04c12124fb99cdc4786d9306a041c67e
        AngelHaloArea.7aa654dc15f051b4e865dc82b6765b0c
        AngelRadiantGroundArea.a27c2b75c96aacb448e6e861eb960b8f
        AngelWallOfLightArea.a28d640657feaa645afe7fa545c4e593
        ArmyACAgainstRangeAura.3653e2b741f34d4b971db84df126e688
        ArmyAuraOfCourageArea.fbcdb04c6a584734809aaa8d1130643c
        ArmyBloodLustAura.7913a3d9077e4004aad5450375a56a54
        ArmyDerakniDroneAura.2d93a18b0dd645f0b362ab92923b82cf
        ArmyFearAura.1b4fa62c786844b2a64b014468aacb62
        ArmyHolyAuraArea.2518e538341540fa84cb67b71e1c8112
        ArmyMoraleDecreaseAura.2132b87cf145456696ed42ad26c4a1b5
        ArmyNabasuDeathStealingGazeAura.db5abcdaeeb44ac6b689a54b660ece02
        ArmyStatBonus2AuraBardriud.2b98c05437e7422ba37ad508c24522b1
        ArmyStinkingCloudArea.33955f636d434a2da77f3f65c52221b8
        ArmyUnholyAura.41c1f8751e4c4a49a1fba09a481f039b
        Artifact_AeonCloakArea.1e3f209f70fc4ca696a40d474945ddd1
        Artifact_RingOfSummonsArea.54f391fa1b927414fb1fadf63ec2c773
        ArtificeDomainBaseAura.f042f2d62e6785d4e8612a027de1f298
        Azata1SongOfSeasonsAbilityArea.257a806b4d2b452d9fa9f603d0d1c8a2
        AzataGhaelGazeAreaEffect.b78b7adcab49c4949ac0ad4d55544f04
        BalorCommandInfantryAreaEffect.3a651bab2b8e4be4b8211ff3ccf00769
        BlackDragonFrightfulPresenceArea.2aedecde815f18f4db88227dcb1df9e5
        BladeBarrierArea.cae4347a512809e4388fb3949dc0bc67
        BlightMiasmaArea.6dfb57c97f3023640b1cbbc9391031df
        BloodragerAbyssalDemonicAuraAura.408578c9ca390bf4dadd84c962c558df
        BodakDeathGazeAreaEffect.d6c0ab2f2828dc0479867fe173984016
        BrestplateOfWhiteDragonArea.9596b5c0200a99842a1636879bf2957a
        BrimorakSmokeBreathAreaEffect.900988051cd6682469e25f1a6cd6723b
        BythosConfusionGazeAreaEffect.56e599a2fb95286408e703b5376e0ff7
        CacklePassiveArea.2e6ff34b598a4d9796533f129816bc30
        CallOfTheWildBeastShapeIArea.f6297625a40c0aa47a725432862fcd69
        CarnivorousCrystal_AreaEffect_SubsonicHum.a89a5b1edba9c614b92a7ba7ab3f5a1d
        ChantPassiveArea.d2ff15c7cbfa4cd08ee599a881279172
        CloakofDreamsArea.6c116b31887c6284fbd41c070f6422f6
        CloakOfLoneWolfArea.4bb2cc002f3a4fd89d2431bbeb1db038
        CloudBlizzardBlastArea.6ea87a0ff5df41c459d641326f9973d5
        CloudSandstormBlastArea.48aa66d1a15515e40b07bc1f5fb80f64
        CloudSteamBlastArea.35a62ad81dd5ae3478956c61d6cd2d2e
        CloudThunderstormBlastArea.3659ce23ae102ca47a7bf3a30dd98609
        CorruptedHerald_AuraOfFearArea.fd8c7927eb03db348abffc32e1abff70
        CR3Gelatinous_Cube_Stench_Area_Effect.afa985c0ac4e4fb9bdb32e396190f9b7
        DeadlyEarthEarthBlastArea.4b19dd893a4b80a49905903bcd56b9e2
        DeadlyEarthMagmaBlastArea.c26aa67475bdb64449b0e0be6a9ea823
        DeadlyEarthMetalBlastArea.38a2979db34ad0f45a449e5eb174729f
        DeadlyEarthMetalBlastAreaRare.267f19ba174b21e4d9baf30afd589068
        DeadlyEarthMudBlastArea.0af604484b5fcbb41b328750797e3948
        DeepestFearAreaEffect.a11513803d0449343a3f1e9a6737bce6
        DemodandStringyEntanglingFoldsArea.06b07906bd79cf04582b00a8f27214a5
        DemonicFormIINabasu_GazeAreaEffect.25a7636b26f045cd8cc0945352436ec2
        DeskariBreathWeaponArea.caa687990c347af4b9a7bfac7d4229a8
        Despair_AreaEffect.fa30d87159984ac382ab5333563c8c21
        DomainOfTheHungryFleshArea.6df8ed6ff8ac4974aba16eca1b7b5cbe
        Ecorche_AuraAreaEffect_FrightfulPresence.66600e8871bd9ca4eb42affd2cdc0bf0
        ElementalAirGreaterWhirlwindArea.a8fe5d003be628f43b8ad9dcb5cc5d57
        ElementalEngineColdArea.278318d1cf614ad1b4042151d7dea11a
        ElementalWallAcidArea.2a9cebe780b6130428f3bf4b18270021
        ElementalWallColdArea.608d84e25f42d6044ba9b96d9f60722a
        ElementalWallElectricityArea.2175d68215aa61644ad1d877d4915ece
        ElementalWallFireArea.ac8737ccddaf2f948adf796b5e74eee7
        EntangleArea.bcb6329cefc66da41b011299a43cc681
        EritriceCacophonyAreaEffect.47e1291d9c57d524599adaff09a0d35c
        EyesOfTheBodakArea.4d05e8decac186940961882647e03c93
        FieldOfFlowersArea.d8f51712fce75fe42aa4cc6d3d1cf7d0
        FiendTotemGreaterArea.9ebde3219b12ca84c8e734a8430a57f8
        FireDamageAreaEffect.54c5599609ea96e4886d5e3cd0bf9f05
        FormOfTheDragonIIIFrightfulPresenceArea.5d44a8408f6907a4eb19a28ae24fb5fe
        Gallu_AreaEffect_AuraOfHavoc.a2f0adf9e5004894cb070f4f37d2fa1e
        Gibrileth_StenchAreaEffect.d7a38ef5bd1fffa4aa85a69ff6fe23d4
        GolemAutumnAura.bc1077a5cd1f5d945a6fc5fb3cd2c86a
        GolemSummerAura.034ad7d17998e7046b02be1a34e03517
        Halaseliax_FireAuraArea.de0b34e1564d48cabf078046963eb389
        Halaseliax_FrightfulPresenceArea.b2114357604b47809a3808ea6973ce72
        HeartOfIraArea.3a636a7438e92a14386fc460f466be1b
        HokugolColdAuraAreaEffect.a0a68b62640fda14da15804dcd30fca9
        IncendiaryCloudArea.a892a67daaa08514cb62ad8dcab7bd90
        InspiredRageArea.ac783feed9dea91469d61ebb5f9d2e97
        MelazmeraFrightfulPresenceArea.7ef31f5d8857ba04a9f273983b0eceff
        MephistoHellishMagicSummonAreaEffect.d85c07ae63ac4930999f8579f1c68dbd
        NabasuDeathStealingGazeAreaEffect.609913ecbb69c63428f3db841173eca6
        NereidBeguilingAuraAreaEffect.466ec47d91f209646aa1b66a797b7d8d
        Nocticula_AreaEffect_SeductivePresence.7af69ec90ad24cf38beea09594cdf9c3
        NymphBlindingBeautyAreaEffect.28bdf6d83c0ec214ebf8de668aeedc2b
        OracleRevelationBlizzardArea.0b1e873ec2045294991d2ae8e11065e4
        OracleRevelationFirestormArea.955ec5730fa203142a4fdc3f8af6195b
        PlagueStormBlindingSicknessArea.b342e42d2ed58484c8dff9150d18f4e4
        PlagueStormBubonicPlagueArea.ba09d51375db5f34790184443416d84b
        PlagueStormCackleFeverArea.6cae3c64989f3684bb078efcfa9021a1
        PlagueStormMindFireArea.6fa0adacca8d00f4aaba1e8df77a318f
        PlagueStormShakesArea.706df636208b2864aa80032b72e0aabd
        PoisonousHydraCloudArea.1416eff7edbb1e04c9fc79ce3a519d6e
        PolarMidnightArea.5afa2db8ceb8bee4980b321d036b781b
        RampageExtraEffect20DamageArea.15203a3c2d6741018ad535a0c94633a8
        RampageExtraEffect20DamageArea.1c2dff0ebe9647b2a8ea8c16f1566316
        RampageExtraEffect20DamageArea.3183ed0726714f95b59200bdfb1a45fe
        RangerHolyBombArea.43b723de2fcf4a619bb1b5907ebc8e84
        RealmProtectorArea.0e5102ce4d899284181302e3757f9705
        RiftDrakeFriedTastyAromaAreaEffect.4c494a9f873c1e64d8af9e23fc8250e0
        ScarecrowGazeAreaEffect.efb248866c6675843b25621a6db23dfa
        Sevalros_ColdAuraArea.73671f06d0af4f8e9f6b56259b85f811
        Sevalros_FrightfulPresenceArea.623560b3e3a44dc5836ff309b0b19086
        ShamblingMoundPoisonAreaEffect.9f3484751a54de145b5188ff33ebbbd4
        SickeningEntanglementArea.72328360f1eeeb94d8a43d51db96eccb
        SiroccoArea.b21bc337e2beaa74b8823570cd45d6dd
        SoothingMudArea.e4ca25707cc10614ba3e3d32a0d80e10
        StewardOfTheSkeinGazeAreaEffect.603e9b9d5486e4c46bf9659dc28c1882
        StinkingCloudArea.aa2e0a0fe89693f4e9205fd52c5ba3e5
        StormCallArea.85c1ea0021ce2714f8559fb618bf7ff6
        SunDomainGreaterAura.cfe8c5683c759f047a56a4b5e77ac93f
        SupernovaArea.165a01a3597c0bf44a8b333ac6dd631a
        SwarmFormDamageAuraAreaEffect.3b5af05fce2b8d8438ea6ea65a729638
        SymbolOfRevelationRuneTriggered.76c3ad8865d958d4f97d935b98bd368e
        TarPoolArea.d59890fb468915946bae085439bd0881
        ThanadaemonFearGazeAreaEffect.97b5fd30155a2f842a177ecbc0f3d749
        TricksterHallucinogenicCloudArea.053c8fd966bbcb94bb8db6582d990650
        TricksterPhantasmalHealerArea.937e118d436137b4fbf31f5715c505f0
        VavakiaAspectAura.5502f8413bb602e41a825bfe9b0d3628
        Vavakia_AuraAreaEffect_FrightfulPresence.8c835debb7e1ae94ea863cfe583ea1ca
        VescavorQueenGibberAreaEffect.acbb8f87c5d98164dbdc1aee0f9eda2b
        VescavorSwarmGibberAreaEffect.a80c90f3223d8324ea0c1d75c45bd331
        Vrolikai_AreaEffect_DeathStealingGaze.9fd8c97fa994e5b4da2ff5a9606faab5
        WebArea.fd323c05f76390749a8555b13156813d
        WildGazeAreaEffect.04f6b9af9fa6fac479e655f2331b1716
        WildShapeElementalAirHugeWhirlwindArea.0fb8d185085539e41b11b780bc7d9b9e
        WildShapeElementalAirLargeWhirlwindArea.ebd2fe081029a6b438aed873607e6375
        WildShapeElementalAirMediumWhirlwindArea.c73f9a028b78f6e4d8a709b62f6344e0
        WildShapeElementalAirSmallWhirlwindArea.91f4541e0eb353b4681289cc9615a79d
        WoundWormsLair_BlackDragonFrightfulPresenceArea.382910feb429e1449b3f8f2a633e3244
        */

        //RootUIContext.Instance.IsTacticalCombat

        [HarmonyPatch(typeof(CombatController), nameof(CombatController.ChooseNextUnit))]
        [HarmonyPrefix]
        public static void OnEndTurn(CombatController __instance)
        {
            // if end turn and in turn based, apply OnRound effect

            if (!CombatController.IsInTurnBasedCombat())
                return;

            var unit = __instance.CurrentTurn?.Rider;
            if (unit == null)
                return;

            foreach (var area in Game.Instance.State.AreaEffects)
            {
                var actions = area.Blueprint.GetComponent<AbilityAreaEffectRunAction>();
                if (actions == null || !actions.Round.HasActions)
                    continue;

                if (!area.m_UnitsInside.Any(f => f.Reference.Value == unit))
                    continue;

                using (ContextData<AreaEffectContextData>.Request().Setup(area))
                {
                    using (area.m_Context.GetDataScope(unit))
                    {
                        Helper.PrintDebug($"Patch_FixAreaEffectDamage3 applying OnRound {unit.CharacterName}");
                        actions.Round.Run();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(AbilityAreaEffectRunAction), nameof(AbilityAreaEffectRunAction.OnRound))]
        [HarmonyPrefix]
        public static bool OnRound()
        {
            // if in turn based, skip dealing damage, was already handled by OnEndTurn
            if (CombatController.IsInTurnBasedCombat())
                return false;
            return true;
        }

        public static void GetTTTSetting()
        {
            var Fixes = AccessTools.Field(Type.GetType("TabletopTweaks.Config.ModSettings, TabletopTweaks"), "Fixes").GetValue(null);
            var BaseFixes = AccessTools.Field(Type.GetType("TabletopTweaks.Config.SettingGroup, TabletopTweaks"), "BaseFixes").GetValue(Fixes);
            var flag = (bool)AccessTools.Method(Type.GetType("TabletopTweaks.Config.SettingGroup, TabletopTweaks"), "IsEnabled").Invoke(BaseFixes, new object[] { "FixShadowSpells" });
        }
    }

    [PatchInfo(Severity.Harmony, "Patch: Polymorph", "allows debug flags to keep inventory or model during polymorph", false)]
    public class Patch_Polymorph
    {
        [HarmonyPatch(typeof(Polymorph), nameof(Polymorph.OnActivate))]
        [HarmonyPrefix]
        public static void KeepSlots(Polymorph __instance)
        {
            __instance.m_KeepSlots = __instance.m_KeepSlots || Settings.StateManager.State.polymorphKeepInventory;
        }

        [HarmonyPatch(typeof(Polymorph), nameof(Polymorph.TryReplaceView))]
        [HarmonyPrefix]
        public static bool KeepModel(Polymorph __instance)
        {
            return !Settings.StateManager.State.polymorphKeepModel;
        }
    }

    [PatchInfo(Severity.Harmony | Severity.WIP, "Patch: Ability Groups", "merges similar abilities into foldable categories", false)]
    [HarmonyPatch]
    public class Patch_AbilityGroups
    {
        // improvements:
        // - fix doesn't show resource count
        // - fix cannot remove from toolbar

        public static void AddGroup(string title, string description, Sprite icon, params string[] guids)
        {
            if (title == null)
                return;

            var group = Groups.FirstOrDefault(f => f.Title == title);
            if (group == null)
                Groups.Add(group = new(title, description ?? "", icon));

            group.Guids.AddRange(guids.Select(s => BlueprintGuid.Parse(s)));
        }

        #region DefGroup
        public static List<DefGroup> Groups = new()
        {
            new DefGroup(
                "Kinetic Blades",
                "Kinetic Blade Category",
                Helper.StealIcon("41e9a0626aa54824db9293f5de71f23f"),
                "89acea313b9a9cb4d86bbbca01b90346", //KineticBladeAirBlastAbility
                "55790f1d270297f4a998292e1573a09e", //KineticBladeBlizzardBlastAbility
                "98f0da4bf25a34a4caffa6b8a2d33ef6", //KineticBladeBloodBlastAbility
                "4005fc2cd91860142ba55a369fbbec23", //KineticBladeBlueFlameBlastAbility
                "371b160cbb2ce9c4a8d6c28e61393f6d", //KineticBladeChargeWaterBlastAbility
                "37c87f140af6166419fe4c1f1305b2b8", //KineticBladeColdBlastAbility
                "77d9c04214a9bd84bbc1eefabcd98220", //KineticBladeEarthBlastAbility
                "b9e9011e24abcab4996e6bd3228bd60b", //KineticBladeElectricBlastAbility
                "41e9a0626aa54824db9293f5de71f23f", //KineticBladeFireBlastAbility
                "3f68b8bdd90ccb0428acd38b84934d30", //KineticBladeIceBlastAbility
                "cf1085900220be5459273282389aa9c2", //KineticBladeMagmaBlastAbility
                "ea2b3e7e3b8726d4c94ba58118749742", //KineticBladeMetalBlastAbility
                "5639fadad8b45e2418b356327d072789", //KineticBladeMudBlastAbility
                "acc31b4666e923b49b3ab85b2304f26c", //KineticBladePlasmaBlastAbility
                "dc6f0b906566aca4d8b86729855959cb", //KineticBladeSandstormBlastAbility
                "66028030b96875b4c97066525ff75a27", //KineticBladeSteamBlastAbility
                "287e0c88af08f3e4ba4aca52566f33a7", //KineticBladeThunderstormBlastAbility
                "70524e9d61b22e948aee1dfe11dc67c8"),//KineticBladeWaterBlastAbility)
            new DefGroup(
                "Demon Aspects",
                "Demon Aspects and other demon related abilities.",
                Helper.StealIcon("e24fbd97558f06b45a09c7fbe7170a55"),
                "0b57876f5dbbc784186b8b1f7d678602", //DemonFirstAscensionAbility, this is the gore attack
                "7b63a532fe1ad654fa1aa8f5ebf3cefb", //DemonClawsAbility
                "b17352531cb25d64fbf4078b856383c5", //DemonSummonAbilityTier2
                "9365979e813d90f4db1579dd36f0a3c9", //DemonSummonAbilityTier3
                "375089aeb3bcfa4479de8476b1589996", //DemonSummonAbilityTier4
                "fd1669c290212484894bc276d79bc63f", //DemonSummonAbilityTier5
                "745734402784ef34894aac64e35d46f0", //DemonSummonAbilityTier6
                "a693ad7d3783f8a4680ab410d9858525", //DemonSummonAbilityTier7
                "7a6f84f3df641d64e8f59e8fccf00568", //DemonSummonAbilityTier8
                "cf6355be6d63541418279a560039a866", //DemonSummonAbilityTier9
                "54d981871c4241844b7dcfc5d4893025", //DemonSummonAbilityTier10
                "b968988d6c0e830458fd49efbfb86202", //NocticulaAspectAbility
                "e24fbd97558f06b45a09c7fbe7170a55", //BabauAspectAbility
                "3070984d4c8bd4f48877337da6c7535d", //BalorAspectAbility
                "e642444d21a4dab4ea07cd042e6f9dc0", //BrimorakAspectAbility
                "49e1df551bc9cdc499930be39a3fc8cf", //ColoxusAspectAbility
                "55c6e91192b92b8479fa66d6aee33074", //IncubusAspectAbility
                "37bfe9e5535e54c49b248bd84305ebd5", //KalavakusAspectAbility
                "868c4957c5671114eaaf8e0b6b55ad3f", //NabasuAspectAbility
                "e305991cb9461a04a97e4f5b02b8b767", //OmoxAspectAbility
                "fae00e8f4de9cd54da800d383ede7812", //SchirAspectAbility
                "0d817aa4f8bc00541a43ea2f822d124b", //ShadowDemonAspectAbility
                "8a474cae6e2788a498f616d36b56b5d2", //SuccubusAspectAbility
                "b6dc815e86a12654eb7f78c5f14008df", //VavakiaAspectAbility
                "600cf1ff1d381d8488faed4e7fbda865", //VrockAspectAbility
                "df9e7bbc606b0cd4087ee2d08cc2c09b"),//VrolikaiAspectAbility
            new DefGroup(
                "Metakinesis",
                "Metakinesis",
                Helper.StealIcon("bb4369a9be4406147ad4f1a1f05adccf"),
                "2dc9630110d0434ba7df785777b67be7", //MetakinesisAutoAbility
                "c93c0217b3e0b4441a4f789dfb95fc8b", //MetakinesisEmpowerAbility
                "b65ad9782f697f245baeb90cd5670546", //MetakinesisEmpowerCheaperAbility
                "bb4369a9be4406147ad4f1a1f05adccf", //MetakinesisMaximizedAbility
                "5c9f2b38404f118479a44234777e1ea8", //MetakinesisMaximizedCheaperAbility
                "990dd3388df6a8d4cad1429380e71853", //MetakinesisQuickenAbility
                "53c7b6accfa1e8e4eba7004b17f61ac1", //MetakinesisQuickenCheaperAbility
                "031d823e0b804b3c868bd031e539cac3"),//MetakinesisSelectiveAbility
            new DefGroup(
                "Infusions",
                "Infusions",
                Helper.StealIcon("88c37d8a7d808a844ba0116dd37e4059"),
                "6d35b4f39de9eb446b2d0a65b931246b", //BleedingInfusionAbility
                "88c37d8a7d808a844ba0116dd37e4059", //BowlingInfusionAbility
                "96b3fc11991f2664080c7c5e41417f48", //BurningInfusionAbility
                "fb426ea002abbbc4198b1cd6b99f1be8", //ChillingInfusionAbility
                "abf5c26910fda5949abbc285c60416f9", //DazzlingInfusionAbility
                "091b297f43ac5be43af31979c00ade57", //EntanglingInfusionAbility
                "323be9d573657374da4e3f1456a2366c", //FlashInfusionAbility
                "d0007fed20710ae4a96cebd2ba99f08b", //FoxfireInfusionAbility
                "2816fad233e15a54c86729cee6e8969d", //GrapplingInfusionAbility
                "e2e3ce12bdfc9d14a9ca4d51696dc8db", //GutWrenchingInfusionAbility
                "b2d91bac690b74140b4fa3eec443edee", //MagneticInfusionAbility
                "06e3ac0ec6341744eb87f1f70a11576b", //PureFlameInfusionAbility
                "bc5665a318bc4eb46a0537455509851a", //PushingInfusionAbility
                "097c209e378144045ab97f4d54876959", //RareMetalInfusionAbility
                "db3ccc72faeac0343891ba71bb692a42", //SynapticInfusionAbility
                "59303d0eb693cd2438fc89f91e29ab19", //UnravelingInfusionAbility
                "30c81aff8e5293d418759d10f193f347"),//VampiricInfusionAbility
            new DefGroup(
                "Metamagic Rods",
                "Metamagic Rods",
                Helper.StealIcon("485ffd3bd7877fb4d81409b120a41076"),
                "ccffef1193d04ad1a9430a8009365e81", //MetamagicRodGreaterBolsterToggleAbility
                "cc266cfb106a5a3449b383a25ab364f0", //MetamagicRodGreaterEmpowerToggleAbility
                "c137a17a798334c4280e1eb811a14a70", //MetamagicRodGreaterExtendToggleAbility
                "78b5971c7a0b7f94db5b4d22c2224189", //MetamagicRodGreaterMaximizeToggleAbility
                "5016f110e5c742768afa08224d6cde56", //MetamagicRodGreaterPersistentToggleAbility
                "fca35196b3b23c346a7d1b1ce20c6f1c", //MetamagicRodGreaterQuickenToggleAbility
                "cc116b4dbb96375429107ed2d88943a1", //MetamagicRodGreaterReachToggleAbility
                "f0d798f5139440a8b2e72fe445678d29", //MetamagicRodGreaterSelectiveToggleAbility
                "056b9f1aa5c54a7996ca8c4a00a88f88", //MetamagicRodLesserBolsterToggleAbility
                "ed10ddd385a528944bccbdc4254f8392", //MetamagicRodLesserEmpowerToggleAbility
                "605e64c0b4586a34494fc3471525a2e5", //MetamagicRodLesserExtendToggleAbility
                "868673cd023f96945a2ee61355740a96", //MetamagicRodLesserKineticToggleAbility
                "485ffd3bd7877fb4d81409b120a41076", //MetamagicRodLesserMaximizeToggleAbility
                "5a87350fcc6b46328a2b345f23bbda44", //MetamagicRodLesserPersistentToggleAbility
                "b8b79d4c37981194fa91771fc5376c5e", //MetamagicRodLesserQuickenToggleAbility
                "7dc276169f3edd54093bf63cec5701ff", //MetamagicRodLesserReachToggleAbility
                "66e68fd0b661413790e3000ede141f16", //MetamagicRodLesserSelectiveToggleAbility
                "afb2e1f96933c22469168222f7dab8fb", //MetamagicRodMasterpieceToggleAbility
                "6cc31148ae2d48359c02712308cb4167", //MetamagicRodNormalBolsterToggleAbility
                "077ec9f9394b8b347ba2b9ec45c74739", //MetamagicRodNormalEmpowerToggleAbility
                "69de70b88ca056440b44acb029a76cd7", //MetamagicRodNormalExtendToggleAbility
                "3b5184a55f98f264f8b39bddd3fe0e88", //MetamagicRodNormalMaximizeToggleAbility
                "9ae2e56b24404144bd911378fe541597", //MetamagicRodNormalPersistentToggleAbility
                "1f390e6f38d3d5247aacb25ab3a2a6d2", //MetamagicRodNormalQuickenToggleAbility
                "f0b05e39b82c3be408009e26be40bc91", //MetamagicRodNormalReachToggleAbility
                "04f768c59bb947e3948ce2e7e72feecb"),//MetamagicRodNormalSelectiveToggleAbility
        };
        #endregion

        [HarmonyPatch(typeof(ActionBarSlotVM), nameof(ActionBarSlotVM.SetMechanicSlot))]
        [HarmonyPostfix]
        public static void ShowButton(ActionBarSlotVM __instance)
        {
            // makes the unfold button visible
            if (__instance?.MechanicActionBarSlot is MechanicActionBarSlotGroup)
                __instance.HasConvert.Value = true;
        }

        [HarmonyPatch(typeof(ActionBarVM), nameof(ActionBarVM.CollectAbilities))]
        [HarmonyPostfix]
        public static void GroupAbilities(UnitEntityData unit, ActionBarVM __instance)
        {
            var all = new Dictionary<int, List<MechanicActionBarSlot>>();

            for (int i = __instance.GroupAbilities.Count - 1; i >= 0; i--)
            {
                BlueprintGuid guid;
                var slot = __instance.GroupAbilities[i].MechanicActionBarSlot;
                if (slot is MechanicActionBarSlotActivableAbility act)
                    guid = act.ActivatableAbility.Blueprint.AssetGuid;
                else if (slot is MechanicActionBarSlotAbility ab)
                    guid = ab.Ability.Blueprint.AssetGuid;
                else
                    continue;

                int index = Groups.FindIndex(f => f.Guids.Contains(guid));
                if (index < 0)
                    continue;

                if (!all.TryGetValue(index, out var list))
                    all.Add(index, list = new());

                list.Add(slot);
                __instance.GroupAbilities.RemoveAt(i);
            }

            foreach (var list in all)
            {
                // add new toolbar group
                __instance.GroupAbilities.Add(new ActionBarSlotVM(new MechanicActionBarSlotGroup(unit, list.Key, list.Value)));

                // update existing toolbar slots
                foreach (var slot in __instance.Slots)
                {
                    if (slot.MechanicActionBarSlot is MechanicActionBarSlotGroup group && group.Index == list.Key)
                        group.Slots = list.Value;
                }
            }
        }

        [HarmonyPatch(typeof(ActionBarSlotVM), nameof(ActionBarSlotVM.OnShowConvertRequest))]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPrefix]
        public static bool OnShowConvert(ActionBarSlotVM __instance)
        {
            if (__instance.MechanicActionBarSlot is not MechanicActionBarSlotGroup group)
                return true;

            if (__instance.ConvertedVm.Value != null && !__instance.ConvertedVm.Value.IsDisposed)
            {
                __instance.CloseConvert();
                return false;
            }

            __instance.ConvertedVm.Value = new ActionBarConvertedActivableVM(group.Slots, __instance.CloseConvert); // if null is used, it won't close; possible useful for nesting
            return false;
        }

        [HarmonyPatch(typeof(ActionBarSlotVM), nameof(ActionBarSlotVM.OnMainClick))]
        [HarmonyPostfix]
        public static void OnClick(ActionBarSlotVM __instance)
        {
            if (__instance.MechanicActionBarSlot is MechanicActionBarSlotGroup)
                __instance.OnShowConvertRequest();
        }

        [HarmonyPatch(typeof(ActionBarVM), nameof(ActionBarVM.OnUnitChanged))]
        [HarmonyPrefix]
        public static void KeepOpen1(ActionBarVM __instance, out int __state)
        {
            __state = -1;

            foreach (var slot in __instance.Slots)
            {
                if (slot.ConvertedVm?.Value != null && slot.MechanicActionBarSlot is MechanicActionBarSlotGroup group)
                {
                    __state = group.Index;
                    break;
                }
            }
        }

        [HarmonyPatch(typeof(ActionBarVM), nameof(ActionBarVM.OnUnitChanged))]
        [HarmonyPostfix]
        public static void KeepOpen2(ActionBarVM __instance, int __state)
        {
            if (__state < 0)
                return;

            foreach (var slot in __instance.Slots)
            {
                if (slot.MechanicActionBarSlot is MechanicActionBarSlotGroup group && group.Index == __state)
                {
                    Helper.PrintDebug("reopend OnShowConvertRequest after refresh");
                    slot.OnShowConvertRequest();
                    break;
                }
            }
        }

        //[HarmonyPatch(typeof(ActionBarVM), nameof(ActionBarVM.ClearSlot))]
        //[HarmonyPrefix]
        public static void Debug1(ActionBarSlotVM viewModel, ActionBarVM __instance)
        {
            Helper.Print($"clearing slot index={viewModel?.Index} unit-select={__instance.SelectedUnitValue?.CharacterName} unit-bar={viewModel?.MechanicActionBarSlot?.Unit?.CharacterName} mechanic={viewModel?.MechanicActionBarSlot?.GetType()?.Name}");
        }

        public class MechanicActionBarSlotGroup : MechanicActionBarSlot
        {
            public int Index;
            [JsonProperty]
            public List<MechanicActionBarSlot> Slots;

            [JsonConstructor]
            public MechanicActionBarSlotGroup(List<MechanicActionBarSlot> slots)
            {
                this.Slots = slots;

                // find and set Index
                var guid = BlueprintGuid.Empty;
                var slot = this.Slots.FirstOrDefault();
                if (slot is MechanicActionBarSlotActivableAbility act)
                    guid = act.ActivatableAbility.Blueprint.AssetGuid;
                else if (slot is MechanicActionBarSlotAbility ab)
                    guid = ab.Ability.Blueprint.AssetGuid;
                if (guid != BlueprintGuid.Empty)
                {
                    this.Index = Groups.FindIndex(f => f.Guids.Contains(guid));
                }
            }

            public MechanicActionBarSlotGroup(UnitEntityData unit, int index, List<MechanicActionBarSlot> slots)
            {
                this.Unit = unit;
                this.Index = index;
                this.Slots = slots;
            }

            public override bool CanUseIfTurnBasedInternal() => true;
            public override object GetContentData() => this;
            public override Color GetDecorationColor() => default; //new Color(0f, 0f, 0f, 0f);
            public override Sprite GetDecorationSprite() => null;
            public override string GetTitle() => Groups.ElementAtOrDefault(this.Index)?.Title;
            public override string GetDescription() => Groups.ElementAtOrDefault(this.Index)?.Description;
            public override Sprite GetIcon() => Groups.ElementAtOrDefault(this.Index)?.Icon;
            public override int GetResource()
            {
                int count = 0;
                foreach (var slot in Slots)
                    if (slot.IsActive())
                        count++;
                if (count == 0)
                    return -1;
                return count;
            }
            public override bool IsCasting() => false;

            public override string KeyName => GetTitle();
            public override bool IsActive() => Slots.Any(a => a.IsActive()); // active border
            public override bool IsDisabled(int resourceCount) => false;
            public override bool IsPossibleActive(int? resource = null) => true;
            public override void UpdateSlotInternal(ActionBarSlot slot)
            {
                if (slot.ActiveMark != null && IsActive())
                {
                    slot.ActiveMark.color = slot.RunningColor;
                    slot.ActiveMark.gameObject.SetActive(true);
                }
            }
            public override bool IsBad() => Index < 0; // use this to remove invalid entries

            public override TooltipBaseTemplate GetTooltipTemplate()
            {
                string title = "";
                string description = "";
                Sprite icon = null;

                var info = Groups.ElementAtOrDefault(this.Index);
                if (info != null)
                {
                    title = info.Title;
                    description = info.Description;
                    icon = info.Icon;
                }

                return new TooltipTemplateDataProvider(new UIData(title, description, icon));
            }

        }

        public class DefGroup
        {
            public HashSet<BlueprintGuid> Guids;
            public string Title;
            public string Description;
            public Sprite Icon;

            public DefGroup(string title, string description, Sprite icon, params string[] guids)
            {
                this.Title = title;
                this.Description = description;
                this.Icon = icon;
                this.Guids = guids.Select(s => BlueprintGuid.Parse(s)).ToHashSet();
            }
        }

        public class ActionBarConvertedActivableVM : ActionBarConvertedVM
        {
            public ActionBarConvertedActivableVM(List<MechanicActionBarSlot> list, Action onClose) : base(new(), onClose)
            {
                foreach (var item in list)
                    this.Slots.Add(new ActionBarSlotVM(item));
            }
        }
    }

    public class Patch_Prebuilds
    {
        /* goal: export player build into file; import file to apply as auto-level-up
         * milestones:
         * - find entry into LevelPlan
         * - save persistance
         * - multiclass support / auto choose class
         * - pet companion support
         * - mythic class support
         * 
         * findings:
         * - prebuild is defined by AddClassLevels, for players mostly on BlueprintFeature
         * - player class's are named "Prebuild{class}FeatureList"
         * - story companion's are named "{name}PregenFeatureList"
         * 
         */

        //LevelUpController.ApplyPlanAsFarAsPossible
        //if (this.HasNextLevelPlan)
        //{
        //    this.ApplyLevelUpPlan(true);
        //    this.Commit();
        //}

        //UnitProgressionData.GetNextLevelPlan
        //UnitProgressionData.m_LevelPlans
        // - is saved in json (PostLoad); not entry
        // - can be copied from other units (CopyFrom); probably irrelevant
        // - can be imported (AddLevelPlan)

        //UnitProgressionData.AddLevelPlan
        // - CreateImportedCompanion
        // -- ActionCreateImportedCompanion.RunAction
        // --- PlayerDungeonState.ImportedCompanions:Levels
        // - AddClassLevels.LevelUp
    }
}
