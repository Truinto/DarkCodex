using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public static partial class Helper
    {
        /// <summary>
        /// Works like <see cref="EntityDataBase.Ensure{TPart}"/>, but for inherited UnitParts.<br/>
        /// Removes <typeparamref name="TPartBase"/> and adds <typeparamref name="TPart"/> as necessary.<br/>
        /// If you need to transfer data, use IUpgrade interface.
        /// </summary>
        public static TPart Ensure<TPartBase, TPart>(this EntityDataBase unit) where TPartBase : EntityPart where TPart : TPartBase, new()
        {
            var cache = EntityPartsCacheAccessor<TPart>.Get(unit.Parts.m_Cache);
            if (cache.Part != null)
                return (TPart)cache.Part;

            var parts = unit.Parts.m_Parts;
            for (int i = 0; i < parts.Count; i++)
            {
                if (parts[i] is not TPartBase tbase)
                    continue;

                if (tbase is TPart tpart)
                {
                    EntityPartsCacheAccessor<TPart>.Set(unit.Parts.m_Cache, tpart);
                    return tpart;
                }

                unit.Parts.Remove(tbase);
                tpart = unit.Parts.Add<TPart>();
                if (tpart is IUpgrade<TPartBase> upgrade)
                    upgrade.Upgrade(tbase);
                return tpart;
            }

            return unit.Parts.Add<TPart>();
        }
    }
}
