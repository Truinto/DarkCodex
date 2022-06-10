using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public interface IUpdateCompanion
    { 
        public List<UnitReference> Companions { get; }

        public void AddCompanion(BlueprintUnit blueprintUnit);

        public void RemoveCompanion(UnitReference unitReference);
    }
}
