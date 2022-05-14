using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using UnityEngine.Serialization;

namespace CodexLib
{
    public class ContextActionChangeRankValue : ContextAction
    {
        public AbilityRankChangeType Type;
        public AbilityRankType RankType;
        public ContextValue Value;

        public ContextActionChangeRankValue()
        {
        }

        public ContextActionChangeRankValue(AbilityRankChangeType type, AbilityRankType rankType, ContextValue value)
        {
            this.Type = type;
            this.RankType = rankType;
            this.Value = value;
        }

        public override void RunAction()
        {
            int value = this.Context[this.RankType];
            switch (this.Type)
            {
                case AbilityRankChangeType.Set:
                    value = this.Value.Calculate(this.Context);
                    break;
                case AbilityRankChangeType.Add:
                    value += this.Value.Calculate(this.Context);
                    break;
                case AbilityRankChangeType.Multiply:
                    value *= this.Value.Calculate(this.Context);
                    break;
                case AbilityRankChangeType.Div2:
                    value /= 2;
                    break;
                case AbilityRankChangeType.Div4:
                    value /= 4;
                    break;
                case AbilityRankChangeType.SubHD:
                    value -= this.Target.Unit.Descriptor.Progression.CharacterLevel;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Type");
            }
            this.Context[this.RankType] = value;
        }
        public override string GetCaption()
        {
            return $"Change Rank {this.RankType}: {this.Type}";
        }
    }

    public enum AbilityRankChangeType
    {
        Set, Add, Multiply, Div2, Div4, SubHD
    }
}
