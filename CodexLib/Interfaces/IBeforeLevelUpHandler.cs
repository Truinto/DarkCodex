using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public interface IBeforeLevelUpHandler : IUnitSubscriber
    {
        void BeforeLevelUp();
    }
}
