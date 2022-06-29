using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    public class PoisonBuff : UnitBuffComponentDelegate<PoisonBuff.RuntimeData>, ITickEachRound, IPoisonStack
    {
        public void OnNewRound()
        {
            throw new NotImplementedException();
        }

        public void Stack() // probably bad; need manual stacking?
        {
            throw new NotImplementedException();
        }

        public class RuntimeData
        {
            public PoisonType Poison;
        }

        public static BlueprintBuffReference Blueprint = Helper.ToRef<BlueprintBuffReference>("cd6ec0216549416e981f1c21c1adef4b");

        public static void Create()
        {
            var buff = new BlueprintBuff();
            buff.name = "PoisonVariableBuff";
            buff.m_DisplayName = "Poison".CreateString();
            buff.m_Description = "This creature got poisoned.".CreateString();  // todo: patch Tooltip Buff to display the PoisonType
            buff.m_Icon = Helper.StealIcon("d797007a142a6c0409a74b064065a15e");
            buff.FxOnStart = new();
            buff.FxOnRemove = new();
            buff.IsClassFeature = true;
            buff.Stacking = StackingType.Stack;
            buff.SetComponents(new PoisonBuff());

            Helper.AddAsset(buff, Blueprint.deserializedGuid);

            Blueprint.Cached = buff;
        }
    }

    public class ContextActionApplyPoison : ContextAction
    {
        public override string GetCaption()
        {
            return "Apply Poison";
        }

        public override void RunAction()
        {
            throw new NotImplementedException();
        }
    }


    [AllowedOn(typeof(BlueprintWeaponEnchantment), false)]
    public class PoisonEnchantment : EntityFactComponentDelegate<ItemEntity, PoisonEnchantment.RuntimeData>, IInitiatorRulebookHandler<RuleDealDamage>
    {
        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
            var wielder = this.Owner.Wielder;
            var target = evt.Target;
            var poison = this.Data.Poison;

            var buff = target.AddBuff(PoisonBuff.Blueprint, this.Context);
            // set PoisonType to buff
            buff.CallComponents<PoisonBuff>(c => c.Data.Poison = poison);

            if (--this.Data.Sticky <= 0)
                this.Owner.RemoveEnchantment(this.Fact as ItemEnchantment);
        }

        public class RuntimeData
        {
            public int BonusDC;
            public int Sticky;
            public PoisonType Poison;
        }

        public static BlueprintBuffReference BuffPoison;
    }
}
