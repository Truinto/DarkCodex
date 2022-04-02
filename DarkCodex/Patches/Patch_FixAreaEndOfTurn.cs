using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.Utility;
using System;
using System.Linq;
using TurnBased.Controllers;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Area Effect End Of Turn", "in turn-based mode area effects happen at the end of each unit's round, instead of all at once at the start of the caster's round", false)]
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
}
