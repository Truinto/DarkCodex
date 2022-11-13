using Kingmaker.ElementsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class ContextActionAttack : ContextAction
    {
        public ActionList OnSuccess;
        public ActionList OnFailure;

        public ContextActionAttack(ActionList onSuccess = null, ActionList onFailure = null)
        {
            this.OnSuccess = onSuccess;
            this.OnFailure = onFailure;
        }

        public ContextActionAttack(params GameAction[] onSuccess)
        {
            this.OnSuccess = Helper.CreateActionList(onSuccess);
        }

        public override string GetCaption() => nameof(ContextActionAttack);

        public override void RunAction()
        {
            var caster = this.Context.MaybeCaster;
            if (caster == null)
                return;
            var target = this.Target.Unit;
            if (target == null)
                return;
            var weapon = caster.GetThreatHandMelee()?.Weapon;
            if (weapon == null)
                return;

            var rule = new RuleAttackWithWeapon(caster, target, weapon, 0);
            this.Context.TriggerRule(rule);

            if (rule.AttackRoll.IsHit)
                OnSuccess?.Run();
            else
                OnFailure?.Run();
        }
    }
}
