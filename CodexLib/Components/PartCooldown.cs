using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurnBased.Controllers;

namespace CodexLib
{
    public class PartCooldown : EntityPart
    {
        public Dictionary<BlueprintAbility, TimeSpan> Cooldowns = new();

        public TimeSpan Now => CombatController.IsInTurnBasedCombat() ? Game.Instance.TurnBasedCombatController.TurnStartTime : Game.Instance.TimeController.GameTime;

        public void Apply(BlueprintAbility blueprint, int cooldownRounds)
        {
            this.Cooldowns[blueprint] = Now + cooldownRounds.Rounds().Seconds;
        }

        public bool IsReady(BlueprintAbility blueprint)
        {
            if (!this.Cooldowns.TryGetValue(blueprint, out var time))
                return true;

            return time <= Now;
        }

        public override void OnTurnOff()
        {
            RemoveSelf();
        }

        public override void OnPreSave()
        {
            RemoveSelf();
        }
    }
}
