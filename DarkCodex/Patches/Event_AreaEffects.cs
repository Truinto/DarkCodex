using HarmonyLib;
using Kingmaker;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Visual.Particles;
using System.Collections.Generic;
using Shared;
using CodexLib;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;

namespace DarkCodex
{
    [PatchInfo(Severity.Event, "Event: Area Effects", "mute player area effects while in dialog", false)]
    public class Event_AreaEffects : IDialogStartHandler, IDialogFinishHandler, IPartyCombatHandler, ICutsceneHandler, ICutsceneDialogHandler, IGlobalSubscriber, ISubscriber
    {
        private static readonly List<AreaEffectEntityData> paused = [];

        public static void Stop()
        {
            try
            {
                foreach (var effect in Game.Instance.State.AreaEffects)
                {
                    if (effect.Destroyed || effect.DestroyMark)
                        continue;

                    var caster = (effect.Context.ParentContext as AbilityExecutionContext)?.MaybeCaster;
                    if (caster == null || !caster.IsPlayerFaction)
                        continue;

                    var fx = effect.View?.m_SpawnedFx;
                    if (fx == null || !fx.IsDestroyed)
                        continue;

                    //fx.SetActive(false);
                    FxHelper.Destroy(fx, false);
                    effect.View.m_SpawnedFx = null;

                    paused.Add(effect);
                    Main.PrintDebug(" pausing effect " + effect);
                }
            }
            catch (Exception e) { Main.PrintException(e); }
        }

        public static void Continue(bool force = false)
        {
            try
            {
                if (!force && (Game.Instance.IsModeActive(GameModeType.Dialog) || Game.Instance.IsModeActive(GameModeType.Cutscene)))
                    return;

                for (int i = paused.Count - 1; i >= 0; i--)
                {
                    if (!paused[i].Destroyed && !paused[i].DestroyMark)
                    {
                        //paused[i].View.m_SpawnedFx?.SetActive(true);
                        paused[i].View?.SpawnFxs();
                        Main.PrintDebug(" continuing effect " + paused[i]);
                    }

                    paused.RemoveAt(i);
                }
            }
            catch (Exception e) { Main.PrintException(e); }
        }

        public void HandleDialogStarted(BlueprintDialog dialog)
        {
            Main.PrintDebug("Dialog started...");

            if (Settings.State.stopAreaEffectsDuringCutscenes)
                Stop();
        }

        public void HandleDialogFinished(BlueprintDialog dialog, bool success)
        {
            Main.PrintDebug("Dialog finished...");
            Continue();
        }

        public void HandlePartyCombatStateChanged(bool inCombat)
        {
            if (!inCombat)
                return;

            Continue(force: true);
#if DEBUG
            var contextPool = ContextData<EntityFactComponentDelegate<UnitEntityData, AddAreaEffectData>.ComponentEventContext>.Pool;
            foreach (var runtime in contextPool)
            {
                var data = runtime.m_Runtime?.MaybeData;
                if (data is null || runtime.m_Runtime.IsDisposed  || runtime.m_Runtime.Fact.IsDisposed)
                    continue;

                if (data.AreaEffectInstance != null || !runtime.m_Runtime.Owner.IsPlayerFaction)
                    continue;

                Main.PrintDebug($"found empty AreaEffectInstance for {runtime.m_Runtime.SourceBlueprintComponent.OwnerBlueprint.name}");
            }
#endif
#if false
            // data context is always missing!
            foreach (var unit in Game.Instance.Player.PartyAndPets)
            {
                foreach (var buff in unit.Buffs)
                {
                    var areaeffect = buff.GetComponent<AddAreaEffect>();
                    if (areaeffect == null)
                        continue;

                    if (areaeffect.Data.AreaEffectInstance == null)
                    {
                        Main.PrintDebug("Enabling missing area effect for " + areaeffect.AreaEffect.NameSafe());
                        areaeffect.OnActivate();
                        continue;
                    }

                    if (!AreaService.Instance.CurrentAreaPart.Bounds.MechanicBounds.Contains(areaeffect.Data.AreaEffectInstance.Entity.Position))
                    {
                        Main.PrintDebug("Restarting area effect stuck in wrong area " + areaeffect.AreaEffect.NameSafe());
                        areaeffect.OnDeactivate();
                        areaeffect.OnActivate();
                    }
                }
            }
#endif
        }

        public void HandleDialogVisible(bool state)
        {
            Main.PrintDebug("HandleDialogVisible: " + state);
        }

        public void HandleCutsceneStarted(CutscenePlayerData cutscene, bool queued)
        {
            Main.PrintDebug("Cutscene started...");

            if (Settings.State.stopAreaEffectsDuringCutscenes)
                Stop();
        }

        public void HandleCutsceneRestarted(CutscenePlayerData cutscene)
        {
            Main.PrintDebug("Cutscene restarted...");

            if (Settings.State.stopAreaEffectsDuringCutscenes)
                Stop();
        }

        public void HandleCutscenePaused(CutscenePlayerData cutscene, CutscenePauseReason reason)
        {
        }

        public void HandleCutsceneResumed(CutscenePlayerData cutscene)
        {
        }

        public void HandleCutsceneStopped(CutscenePlayerData cutscene)
        {
            Main.PrintDebug("Cutscene stopped...");
            Continue();
        }
    }
}
