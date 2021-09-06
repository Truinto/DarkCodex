using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var cooldown = this.Context.MaybeCaster?.CombatState.Cooldown;

            if (Amount >= 0f)
                switch (Command)
                {
                    case UnitCommand.CommandType.Standard:
                        if (cooldown.StandardAction > 0)
                            cooldown.StandardAction = 0f;
                        else if (!Strict)
                            cooldown.MoveAction = Math.Max(0, cooldown.MoveAction - Amount);
                        break;
                    case UnitCommand.CommandType.Move:
                        cooldown.MoveAction = Math.Max(0, cooldown.MoveAction - Amount);
                        break;
                    case UnitCommand.CommandType.Swift:
                        cooldown.SwiftAction = 0f;
                        break;
                }
            else
                switch (Command)
                {
                    case UnitCommand.CommandType.Standard:
                        if (cooldown.StandardAction <= 0)
                            cooldown.StandardAction = 6f;
                        else if (!Strict)
                            cooldown.MoveAction = Math.Min(6, cooldown.MoveAction - Amount);
                        break;
                    case UnitCommand.CommandType.Move:
                        cooldown.MoveAction = Math.Max(cooldown.MoveAction, Math.Min(Strict ? 6 : 3, cooldown.MoveAction - Amount));
                        break;
                    case UnitCommand.CommandType.Swift:
                        cooldown.SwiftAction = 6f;
                        break;
                }
        }

        public bool Strict;
        public float Amount = 1.5f;
        public UnitCommand.CommandType Command;
    }
}
