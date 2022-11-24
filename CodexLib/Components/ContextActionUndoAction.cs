using Kingmaker;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurnBased.Controllers;

namespace CodexLib
{
    public class ContextActionUndoAction : ContextAction
    {
        public UnitCommand.CommandType Command;
        public float GainTime = 1.5f; // gain half a move action
        public bool ForceMove = true; // counts as movement

        /// <summary>
        /// Restore action time. Cooldown is usually 6 seconds, except for CommandType.Move where 3 seconds is one move action and 6 seconds are 2 move actions.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="amount">Time to refund. Negative values will steal time instead.</param>
        /// <param name="forceMove"></param>
        public ContextActionUndoAction(UnitCommand.CommandType command = UnitCommand.CommandType.Move, float amount = 1.5f, bool forceMove = true)
        {
            this.Command = command;
            this.GainTime = amount;
            this.ForceMove = forceMove;
        }

        public override string GetCaption()
        {
            return "ContextActionUndoAction";
        }

        public override void RunAction()
        {
            var unit = this.Context.MaybeCaster;
            if (unit == null)
                return;

            TurnController currentTurn = null;

            if (CombatController.IsInTurnBasedCombat()
                && this.Context.MaybeCaster.IsCurrentUnit())
                currentTurn = Game.Instance.TurnBasedCombatController.CurrentTurn;

            Helper.PrintDebug($"pre  UndoAction GainTime={this.GainTime} TimeMoved={currentTurn?.m_RiderMovementStats.TimeMoved} GetRemainingTime={currentTurn?.GetRemainingTime(unit)}");

            if (this.ForceMove)
                this.Context.MaybeCaster.CombatState.m_LastUsageOfMoveActionTime = Game.Instance.TimeController.GameTime;

            var cooldown = this.Context.MaybeCaster.CombatState.Cooldown;
            if (this.GainTime >= 0f) // gain time
                switch (this.Command)
                {
                    case UnitCommand.CommandType.Standard:
                        cooldown.StandardAction = 0f;
                        break;
                    case UnitCommand.CommandType.Move:
                        cooldown.MoveAction = Math.Max(0, cooldown.MoveAction - this.GainTime);
                        if (currentTurn != null)
                        {
                            currentTurn.m_RiderMovementStats.TimeMoved = cooldown.MoveAction;
                            //currentTurn.TrySelectMovementLimit();
                        }
                        break;
                    case UnitCommand.CommandType.Swift:
                        cooldown.SwiftAction = 0f;
                        break;
                }
            else            // lose time
                switch (this.Command)
                {
                    case UnitCommand.CommandType.Standard:
                        cooldown.StandardAction = 6f;
                        break;
                    case UnitCommand.CommandType.Move:
                        if (currentTurn != null)
                        {
                            //currentTurn.TickMovement()
                            currentTurn.m_RiderMovementStats.TimeMoved -= this.GainTime;
                            //currentTurn.TrySelectMovementLimit();
                        }
                        cooldown.MoveAction = Math.Min(6f, cooldown.MoveAction - this.GainTime);
                        break;
                    case UnitCommand.CommandType.Swift:
                        cooldown.SwiftAction = 6f;
                        break;
                }

            Helper.PrintDebug($"post UndoAction GainTime={this.GainTime} TimeMoved={currentTurn?.m_RiderMovementStats?.TimeMoved} GetRemainingTime={currentTurn?.GetRemainingTime(unit)}");
        }
    }
}
