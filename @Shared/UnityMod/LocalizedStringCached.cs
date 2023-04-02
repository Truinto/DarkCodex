using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    /// <summary>
    /// Flexible LocalizedString whose logic depends on <see cref="Resolver"/>.
    /// </summary>
    public struct LocalizedStringCached
    {
        private string cache;
        /// <summary>Original string value.</summary>
        public readonly string Default;
        /// <summary>Function to resolve key to localized string.</summary>
        public static Func<LocalizedStringCached, string> Resolver;

        /// <inheritdoc cref="LocalizedStringCached"/>
        public LocalizedStringCached(string value)
        {
            Default = value;
        }

        /// <summary>
        /// Clear cache. Next access will pull from Resolver again.
        /// </summary>
        public void Clear()
        {
            cache = null;
        }

        /// <summary>
        /// Pulls string from Resolver and caches result.
        /// </summary>
        public override string ToString()
        {
            if (Resolver == null)
                return Default;
            return cache ??= Resolver(this);
        }

        /// <inheritdoc cref="ToString"/>
        public static implicit operator string(LocalizedStringCached lsc)
        {
            return lsc.ToString();
        }

        /// <inheritdoc cref="LocalizedStringCached"/>
        public static implicit operator LocalizedStringCached(string str)
        {
            return new LocalizedStringCached(str);
        }
    }
}
