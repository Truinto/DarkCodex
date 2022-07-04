using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// WIP; suspect to change
    /// </summary>
    /// 
    /// RuleCollectMetamagic, AddMetamagicFeat
    /// RuleApplyMetamagic
    /// MetamagicData
    /// MetamagicBuilder 
    /// SpellBookMetamagicMixer?
    /// UIUtilityTexts .GetMetamagicList .GetMetamagicName
    /// Game.Instance.BlueprintRoot.LocalizedTexts.MetamagicNames
    /// 
    public class MetamagicExt
    {
        public string Id;
        public Metamagic Enum;
        public int DefaultCost;
        public Func<BlueprintAbility, bool> FuncAvailable;
        public Func<UnitEntityData, int> FuncCost;

        public MetamagicExt(string id, int defaultCost, Func<BlueprintAbility, bool> funcAvailable = null, Func<UnitEntityData, int> funcCost = null)
        {
            this.Id = id;
            this.DefaultCost = defaultCost;
            this.FuncAvailable = funcAvailable;
            this.FuncCost = funcCost;

            this.Enum = Get(id);
        }

        public bool IsAvailable(BlueprintAbility spell)
        {
            return FuncAvailable == null || FuncAvailable(spell);
        }

        public int GetCost(UnitEntityData unit = null)
        {
            if (unit == null || FuncCost == null)
                return DefaultCost;
            return FuncCost(unit);
        }

        public const int Offset = 1 << 24;
        public const Metamagic ExtPlaceholder = (Metamagic)int.MinValue;

        private static List<string> _ids;
        public static Metamagic Get(string id)
        {
            if (_ids == null)
            {
                // to do load from disk
                _ids = new();
            }

            int index = _ids.IndexOf(id);
            if (index < 0)
            {
                index = _ids.Count;
                _ids.Add(id);
                // to do save to disk
            }

            return (Metamagic)(Offset + index);
        }
    }
}
