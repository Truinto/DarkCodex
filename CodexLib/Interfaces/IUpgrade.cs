using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Simple interface to Upgrade UnitParts.
    /// </summary>
    public interface IUpgrade<T>
    {
        /// <summary></summary>
        public void Upgrade(T upgradeFrom);
    }
}
