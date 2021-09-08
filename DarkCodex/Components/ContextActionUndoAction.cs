using Kingmaker;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurnBased.Controllers;

namespace DarkCodex.Components
{
    public class ContextActionUndoAction : ContextAction
    {
        public ContextActionUndoAction(UnitCommand.CommandType command = UnitCommand.CommandType.Move, float amount = 1.5f)
        {
            this.Command = command;
        }

        public override string GetCaption()
        {
            return "ContextActionUndoAction";
        }

        public override void RunAction()
        {
            if (this.Context.MaybeCaster == null)
                return;

            TurnController currentTurn = null;

            if (CombatController.IsInTurnBasedCombat()
                && this.Context.MaybeCaster.IsCurrentUnit())
                currentTurn = Game.Instance.TurnBasedCombatController.CurrentTurn;

            Helper.PrintDebug($"pre  UndoAction GainTime={GainTime} TimeMoved={currentTurn?.TimeMoved} GetRemainingTime={currentTurn?.GetRemainingTime()}");

            var cooldown = this.Context.MaybeCaster.CombatState.Cooldown;
            if (GainTime >= 0f) // gain time
                switch (Command)
                {
                    case UnitCommand.CommandType.Standard:
                        cooldown.StandardAction = 0f;
                        break;
                    case UnitCommand.CommandType.Move:
                        cooldown.MoveAction = Math.Max(0, cooldown.MoveAction - GainTime);
                        if (ForceMove && currentTurn != null)
                        {
                            currentTurn.TimeMoved = cooldown.MoveAction;
                            //currentTurn.TrySelectMovementLimit();
                        }
                        break;
                    case UnitCommand.CommandType.Swift:
                        cooldown.SwiftAction = 0f;
                        break;
                }
            else            // lose time
                switch (Command)
                {
                    case UnitCommand.CommandType.Standard:
                        cooldown.StandardAction = 6f;
                        break;
                    case UnitCommand.CommandType.Move:
                        if (ForceMove && currentTurn != null)
                        {
                            //currentTurn.TickMovement()
                            currentTurn.TimeMoved -= GainTime;
                            //currentTurn.TrySelectMovementLimit();
                        }
                        cooldown.MoveAction = Math.Min(6f, cooldown.MoveAction - GainTime);
                        break;
                    case UnitCommand.CommandType.Swift:
                        cooldown.SwiftAction = 6f;
                        break;
                }

            Helper.PrintDebug($"post UndoAction GainTime={GainTime} TimeMoved={currentTurn?.TimeMoved} GetRemainingTime={currentTurn?.GetRemainingTime()}");
        }

        public bool ForceMove = true; // counts as movement
        public float GainTime = 1.5f; // gain half a move action
        public UnitCommand.CommandType Command;
    }
}
