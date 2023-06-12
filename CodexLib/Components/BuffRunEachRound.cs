using Kingmaker.Controllers.Optimization;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.View.MapObjects.SriptZones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurnBased.Controllers;

namespace CodexLib
{
    /// <summary>
    /// Buff component to run actions each round.
    /// </summary>
    [AllowedOn(typeof(BlueprintBuff), false)]
    public class BuffRunEachRound : UnitBuffComponentDelegate, ITickEachRound
    {
        /// <summary>Action to execute each round.</summary>
        public ActionList Actions;
        /// <summary>If greater than 0, the radius to look for targets.</summary>
        public Feet Radius;
        /// <summary>Whenever to affect allies.</summary>
        public bool AffectAlly;
        /// <summary>Whenever to affect enemies.</summary>
        public bool AffectEnemy;
        /// <summary>If true, then actions are only run on owner.</summary>
        public bool AffectOnlyOwner;

        ///<inheritdoc cref="BuffRunEachRound"/>
        public BuffRunEachRound(ActionList actions, Feet radius = default, bool affectAlly = false, bool affectEnemy = false, bool affectOnlyOwner = false)
        {
            this.Actions = actions;
            this.Radius = radius;
            this.AffectAlly = affectAlly;
            this.AffectEnemy = affectEnemy;
            this.AffectOnlyOwner = affectOnlyOwner;
        }

        /// <summary>
        /// Interface implementation to execute on each round.
        /// </summary>
        public void OnNewRound()
        {
            var context = this.Context;
            var owner = this.Owner;
            bool isInCombat = Game.Instance.Player?.IsInCombat == true;
            //bool turnBased = isInCombat && Game.Instance.Player.IsTurnBasedModeOn();
            float radius = this.Radius.Meters;
            bool checkDistance = radius > 0f;

            if (AffectOnlyOwner)
            {
                using (context.GetDataScope(owner))
                {
                    this.Actions.Run();
                }
                return;
            }

            foreach (var unit in Game.Instance.Player.AllCharacters)
            {
                if (unit == null)
                    continue;

                bool isAlly = unit.IsAlly(owner);
                if (!(AffectAlly && isAlly || AffectEnemy && isInCombat && !isAlly))
                    continue;

                if (checkDistance && unit.DistanceTo(owner) < radius)
                    continue;

                using (context.GetDataScope(unit))
                {
                    this.Actions.Run();
                }
            }
        }
    }
}
