using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class ContextActionCoatWeapon : ContextAction
    {
        public BlueprintBuff Poison;

        public ContextActionCoatWeapon(BlueprintBuff poison)
        {
            this.Poison = poison;
        }

        public override string GetCaption()
        {
            return "Coat Weapon";
        }

        public override void RunAction()
        {
            var target = this.Target.Unit;
            var weapon1 = target.Body.PrimaryHand.MaybeWeapon;
            var weapon2 = target.Body.SecondaryHand.MaybeWeapon;

            // todo: implement sticky features
            int sticky = 0;

            // todo: implement bonus DC
            var dc = Poison.GetComponent<ContextSetAbilityParams>()?.DC.Calculate(this.Context) ?? 15;

            if (weapon1 != null && !weapon1.Blueprint.IsNatural)
                Enchant(weapon1, dc, sticky);
            if (weapon2 != null && !weapon2.Blueprint.IsNatural)
                Enchant(weapon2, dc, sticky);
        }

        private void Enchant(ItemEntityWeapon weapon, int dc, int sticky)
        {
            if (weapon.Blueprint.AttackType == AttackType.Ranged)
                sticky = 30;

            foreach (var enchant in weapon.Enchantments.Where(w => w.Blueprint.GetComponent<PoisonEnchantment>()).ToArray())
            {
                if (enchant.IsTemporary)
                    weapon.RemoveEnchantment(enchant);
            }

            var pEnchant = weapon.AddEnchantment(PoisonEnchantment.Enchantment, this.Context);
            if (pEnchant != null)
            {
                pEnchant.CallComponents<PoisonEnchantment>(c => c.CoatWeapon(Poison, dc, sticky));
            }
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
            //var wielder = this.Owner.Wielder;
            var target = evt.Target;
            var data = this.Data;

            var save = new RuleSavingThrow(target, SavingThrowType.Fortitude, data.DC);
            this.Context.TriggerRule(save);
            if (!save.IsPassed)
                target.AddBuff(data.Poison, this.Context);

            if (--this.Data.Sticky <= 0)
                this.Owner.RemoveEnchantment(this.Fact as ItemEnchantment);
        }

        public void CoatWeapon(BlueprintBuff poison, int dc, int sticky = 0)
        {
            var data = this.Data;
            data.Poison = poison;
            data.DC = dc;
            data.Sticky = sticky;
        }

        public class RuntimeData
        {
            public BlueprintBuff Poison;
            public int DC;
            public int Sticky;
        }

        public static BlueprintWeaponEnchantmentReference Enchantment = Helper.ToRef<BlueprintWeaponEnchantmentReference>("fff290919b9d4ec5b86be8c20126b46c");

        public static void Create() // TODO: add this
        {
            if (Enchantment.Cached != null)
                return;

            var result = new BlueprintWeaponEnchantment();
            result.name = "PoisonEnchantmentVariable";
            result.m_EnchantName = "Poisoned".CreateString();
            result.m_Description = "This weapon has been coated with a poison.".CreateString();
            result.m_Prefix = "".CreateString();
            result.m_Suffix = "".CreateString();
            result.m_EnchantmentCost = 0;

            Enchantment.Cached = result;
            Helper.AddAsset(result, Enchantment.deserializedGuid);
        }
    }
}
