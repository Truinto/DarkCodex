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
    public class AddTemporaryWeapon : UnitBuffComponentDelegate<AddKineticistBladeData>, IAreaActivationHandler
    {
        public BlueprintItemWeaponReference Weapon;

        /// <param name="weapon">type: <b>BlueprintItemWeapon</b></param>
        public AddTemporaryWeapon(AnyRef weapon)
        {
            this.Weapon = weapon;
        }

        public override void OnActivate()
        {
            this.Data.Applied = this.Weapon.Get().CreateEntity<ItemEntityWeapon>();
            var itemPart = this.Data.Applied.Ensure<CraftedItemPart>();
            itemPart.CasterLevel = this.Context.Params.CasterLevel;
            itemPart.SpellLevel = this.Context.Params.SpellLevel;
            itemPart.MetamagicData ??= new();
            itemPart.MetamagicData.Add(this.Context.Params.Metamagic);
            this.Data.Applied.MakeNotLootable();
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
                this.OnActivate();
                this.OnTurnOn();
            }
        }
    }
}
