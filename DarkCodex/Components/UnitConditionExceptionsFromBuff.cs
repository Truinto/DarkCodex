using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex.Components
{
    /// <summary>Don't apply condition, if its source is in Exceptions.</summary>
    public class UnitConditionExceptionsFromBuff : UnitConditionExceptions
    {
        public BlueprintBuffReference[] Exceptions;

        public override string GetCaption()
        {
            return "exempting [" + Exceptions.Select(s => s.Get().name).Join() + "]";
        }

        public bool IsException(Buff source)
        {
            return this.Exceptions.HasReference(source.Blueprint);
        }
    }
}
