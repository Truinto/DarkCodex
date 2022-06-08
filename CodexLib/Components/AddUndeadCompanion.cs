using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CodexLib
{
    /// <summary>
    /// TODO: AddUndeadCompanion
    /// </summary>
    public class AddUndeadCompanion : UnitFactComponentDelegate<AddUndeadCompanion.RuntimeData>, IAreaHandler, IUpdatePet
    {
        public bool DestroyPetOnDeactivate;
        public bool ForceAutoLevelup;
        public PetType Type = (PetType)5580;

        public void AddCompanion(BlueprintUnit blueprintUnit)
        {
            this.Data.UnitBlueprint.Add(blueprintUnit);
            this.Data.UnitRef.Add(default);
            TryUpdatePet();
        }

        public void RemoveCompanion(UnitReference unitReference)
        {
            var pet = unitReference.Value;
            if (pet == null)
                return;

            foreach (var itemSlot in pet.Body.EquipmentSlots)
            {
                if (itemSlot.HasItem && itemSlot.CanRemoveItem())
                    itemSlot.RemoveItem(true);
                itemSlot.Lock.Retain();
            }

            pet.RemoveMaster();

            if (this.DestroyPetOnDeactivate)
                pet.MarkForDestroy();

            for (int i = 0; i < this.Data.UnitRef.Count; i++)
            {
                if (this.Data.UnitRef[i] == unitReference)
                {
                    this.Data.UnitBlueprint.RemoveAt(i);
                    this.Data.UnitRef.RemoveAt(i);

                    break;
                }
            }
        }

        public override void OnActivate()
        {
            TryUpdatePet();
        }

        public override void OnDeactivate()
        {
            if (this.IsReapplying)
                return;
            if (this.Data.Disabled)
                return;

            foreach (var pet in this.Data.UnitRef.SelectNotNull(s => s.Value))
            {
                foreach (var itemSlot in pet.Body.EquipmentSlots)
                {
                    if (itemSlot.HasItem && itemSlot.CanRemoveItem())
                        itemSlot.RemoveItem(true);
                    itemSlot.Lock.Retain();
                }

                pet.RemoveMaster();

                if (this.DestroyPetOnDeactivate)
                    pet.MarkForDestroy();
            }

            this.Data.UnitBlueprint.Clear();
            this.Data.UnitRef.Clear();
        }

        public void OnAreaBeginUnloading()
        {
        }

        public void OnAreaDidLoad()
        {
            TryUpdatePet();
        }

        public void TryUpdatePet()
        {
            if (this.Data.Disabled || this.Owner.IsPet || !this.Owner.PreviewOf.IsEmpty)
                return;

            var executionMark = ContextData<AddClassLevels.ExecutionMark>.Current;
            this.Data.AutoLevelup |= (executionMark && executionMark.Unit != this.Owner);
            this.Data.AutoLevelup |= this.ForceAutoLevelup;
            if (this.Owner.HoldingState == null)
            {
                EventBus.Subscribe(new UnitSpawnHandler(this, this.Runtime));
                return;
            }

            var unitPartPetMaster = this.Owner.Ensure<UnitPartPetMaster>();
            for (int i = 0; i < this.Data.UnitBlueprint.Count; i++)
            {
                var blueprint = this.Data.UnitBlueprint[i];
                var spawnedPet = this.Data.UnitRef[i].Value;
                bool flag = spawnedPet != null;

                if (spawnedPet == null && !unitPartPetMaster.IsExPet(this.Type))
                {
                    Vector3 position = this.Owner.Position;
                    if (this.Owner.IsInGame) //AstarPath.active
                    {
                        FreePlaceSelector.PlaceSpawnPlaces(1, 0.5f, this.Owner.Position);
                        position = FreePlaceSelector.GetRelaxedPosition(0, true);
                    }
                    this.Data.UnitRef[i] = spawnedPet = Game.Instance.EntityCreator.SpawnUnit(blueprint, position, Quaternion.Euler(0f, this.Owner.Orientation, 0f), this.Owner.HoldingState, null);
                }
                //else if (spawnedPet != null)
                //{
                //    spawnedPet.MarkForDestroy();
                //    if (!spawnedPet.IsPet)
                //    {
                //        spawnedPet.Descriptor.SwitchFactions(BlueprintRoot.Instance.SystemMechanics.FactionNeutrals, false);
                //    }
                //    this.Data.UnitRef[i] = spawnedPet = null;
                //    this.Data.Disabled = true;
                //    Helper.PrintDebug($"Can't spawn second animal companion: {this.Owner}, {this.Fact}");
                //}

                if (spawnedPet != null)
                {
                    if (!unitPartPetMaster.IsExPet(spawnedPet.UniqueId) && spawnedPet.Master == null)
                    {
                        spawnedPet.SetMaster(this.Owner, this.Type);
                        spawnedPet.IsInGame = this.Owner.IsInGame;
                    }
                    this.TryLevelUpPet();
                    if (!flag && Game.Instance.Player.PartyCharacters.Contains(this.Owner))
                    {
                        EventBus.RaiseEvent<IPartyHandler>(h => h.HandleAddCompanion(spawnedPet), true);
                    }
                }
            }
        }

        public void TryLevelUpPet()
        {
        }

        public class RuntimeData
        {
            [JsonProperty]
            public List<BlueprintUnit> UnitBlueprint = new();

            [JsonProperty]
            public List<UnitReference> UnitRef = new();

            [JsonProperty]
            public bool Disabled;

            public bool AutoLevelup;
        }

        public class UnitSpawnHandler : IUnitHandler, IUnitSpawnHandler, IAreaHandler
        {
            public UnitSpawnHandler(IUpdatePet feature, ComponentRuntime runtime)
            {
                this.Component = feature;
                this.FeatureRuntime = runtime;
            }

            public void HandleUnitSpawned(UnitEntityData entityData)
            {
                if (entityData.Descriptor == this.FeatureRuntime.Owner)
                {
                    if (entityData.View != null)
                    {
                        using (this.FeatureRuntime.RequestEventContext())
                        {
                            this.Component.TryUpdatePet();
                        }
                    }
                    EventBus.Unsubscribe(this);
                }
            }

            public void HandleUnitDestroyed(UnitEntityData entityData)
            {
            }

            public void HandleUnitDeath(UnitEntityData entityData)
            {
            }

            public void OnAreaBeginUnloading()
            {
                EventBus.Unsubscribe(this);
            }

            public void OnAreaDidLoad()
            {
            }

            private readonly IUpdatePet Component;

            private readonly ComponentRuntime FeatureRuntime;
        }
    }
}
