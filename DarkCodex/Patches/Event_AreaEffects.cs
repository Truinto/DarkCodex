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

namespace DarkCodex
{
    [PatchInfo(Severity.Event, "Event: Area Effects", "mute player area effects while in dialog", false)]
    public class Event_AreaEffects : IDialogStartHandler, IDialogFinishHandler, IPartyCombatHandler, ICutsceneHandler, ICutsceneDialogHandler, IGlobalSubscriber, ISubscriber
    {
        private static readonly List<AreaEffectEntityData> paused = new();

        public static void Stop()
        {
            foreach (var effect in Game.Instance.State.AreaEffects)
            {
                if (effect.Destroyed || effect.DestroyMark)
                    continue;

                var caster = (effect.Context.ParentContext as AbilityExecutionContext)?.MaybeCaster;
                if (caster == null || !caster.IsPlayerFaction)
                    continue;

                var fx = effect.View?.m_SpawnedFx;
                if (fx == null || !fx.activeSelf)
                    continue;

                //fx.SetActive(false);
                FxHelper.Destroy(fx, false);
                effect.View.m_SpawnedFx = null;

                paused.Add(effect);
                Helper.PrintDebug(" pausing effect " + effect);
            }
        }

        public static void Continue(bool force = false)
        {
            if (!force && (Game.Instance.IsModeActive(GameModeType.Dialog) || Game.Instance.IsModeActive(GameModeType.Cutscene)))
                return;

            for (int i = paused.Count - 1; i >= 0; i--)
            {
                if (!paused[i].Destroyed && !paused[i].DestroyMark)
                {
                    //paused[i].View.m_SpawnedFx?.SetActive(true);
                    paused[i].View?.SpawnFxs();
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

            Continue(force: true);

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

        public void HandleDialogVisible(bool state)
        {
            Helper.PrintDebug("HandleDialogVisible: " + state);
        }

        public void HandleCutsceneStarted(CutscenePlayerData cutscene, bool queued)
        {
            Helper.PrintDebug("Cutscene started...");

            if (Settings.StateManager.State.stopAreaEffectsDuringCutscenes)
                Stop();
        }

        public void HandleCutsceneRestarted(CutscenePlayerData cutscene)
        {
            Helper.PrintDebug("Cutscene restarted...");

            if (Settings.StateManager.State.stopAreaEffectsDuringCutscenes)
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
            Helper.PrintDebug("Cutscene stopped...");
            Continue();
        }
    }
}
