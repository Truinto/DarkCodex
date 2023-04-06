using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Craft;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Class.Kineticist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Based on <see cref="AddKineticistBlade"/>. Doesn't impose restrictions on attacks of opportunity. Force equips weapons with proficiency. Add parameters to <see cref="CraftedItemPart"/>.
    /// </summary>
    public class AddTemporaryWeapon : UnitBuffComponentDelegate<AddKineticistBladeData>, IAreaActivationHandler
    {
        public BlueprintItemWeaponReference Weapon;

        /// <inheritdoc cref="AddTemporaryWeapon"/>
        /// <param name="weapon">type: <b>BlueprintItemWeapon</b></param>
        public AddTemporaryWeapon(AnyRef weapon)
        {
            this.Weapon = weapon;
        }

        public override void OnActivate()
        {
            this.Owner.MarkNotOptimizableInSave();
            this.Data.Applied = this.Weapon.Get().CreateEntity<ItemEntityWeapon>();
            this.Data.Applied.MakeNotLootable();
            this.Data.Applied.VisualSourceItemBlueprint = this.Weapon.Get();
            var itemPart = this.Data.Applied.Ensure<CraftedItemPart>();
            itemPart.CasterLevel = this.Context.Params.CasterLevel;
            itemPart.SpellLevel = this.Context.Params.SpellLevel;
            itemPart.MetamagicData ??= new();
            itemPart.MetamagicData.Add(this.Context.Params.Metamagic);

            using (ContextData<ItemEntity.CanBeEquippedForce>.Request())
            {
                if (!this.Owner.Body.PrimaryHand.CanInsertItem(this.Data.Applied))
                {
                    this.Data.Applied = null;
                    return;
                }
                using (ContextData<ItemsCollection.SuppressEvents>.Request())
                {
                    this.Owner.Body.PrimaryHand.InsertItem(this.Data.Applied);
                }
            }
        }

        public override void OnDeactivate()
        {
            if (this.Data.Applied != null)
            {
                this.Data.Applied.HoldingSlot?.RemoveItem();

                using (ContextData<ItemsCollection.SuppressEvents>.Request())
                {
                    this.Data.Applied.Collection?.Remove(this.Data.Applied);
                }
                this.Data.Applied = null;
            }
        }

        public override void OnTurnOn()
        {
            this.Data.Applied?.HoldingSlot.Lock.Retain();
        }

        public override void OnTurnOff()
        {
            this.Data.Applied?.HoldingSlot.Lock.Release();
        }

        public void OnAreaActivated()
        {
            if (this.Data.Applied == null)
            {
                OnActivate();
                OnTurnOn();
            }
        }
    }
}
