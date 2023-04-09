using Kingmaker.Designers.EventConditionActionSystem.Actions;
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
            var cache = EntityPartsCacheAccessor<TPartBase>.Get(unit.Parts.m_Cache);
            if (cache.Part is TPart ctpart)
                return ctpart;

            var parts = unit.Parts.m_Parts;
            for (int i = 0; i < parts.Count; i++)
            {
                if (parts[i] is not TPartBase tbase)
                    continue;

                if (tbase is TPart tpart)
                {
                    EntityPartsCacheAccessor<TPartBase>.Set(unit.Parts.m_Cache, tpart);
                    return tpart;
                }

                tbase.RemoveSelf();
                tpart = add();
                if (tpart is IUpgrade<TPartBase> upgrade)
                    upgrade.Upgrade(tbase);
                return tpart;
            }

            return add();

            TPart add()
            {
                var val = new TPart();
                parts.Add(val);
                //unit.Parts.AddToCache(val); // this seems unecessary
                EntityPartsCacheAccessor<TPartBase>.Set(unit.Parts.m_Cache, val);
                val.AttachToEntity(unit);
                try
                {
                    unit.Parts.Delegate?.OnPartAdded(val);
                }
                catch (Exception e) { PrintException(e); }
                return val;
            }
        }

        /// <summary>
        /// Works like <see cref="EntityDataBase.Get{TPart}"/>, but for inherited UnitParts.
        /// </summary>
        public static TPart Get<TPartBase, TPart>(this EntityDataBase unit) where TPartBase : EntityPart where TPart : TPartBase, new()
        {
            return unit.Get<TPartBase>() as TPart;
        }
    }
}
