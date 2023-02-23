using System;
using System.Runtime.CompilerServices;

namespace Shared
{
    /// <summary>
    /// Methods to print to standard output.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Only prints in DEBUG.
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void PrintDebug(string msg, [CallerMemberName] string caller = "")
        {
            Console.Write($"[DEBUG] [{caller}] ");
            Console.WriteLine(msg);
        }

        /// <summary>
        /// Prints to standard output.
        /// </summary>
        public static void Print(string msg)
        {
            Console.Write($"[INFO] ");
            Console.WriteLine(msg);
        }

        /// <summary>
        /// Prints to standard output.
        /// </summary>
        public static void PrintWarning(string msg, [CallerMemberName] string caller = "")
        {
            Console.Write($"[WARNING] [{caller}] ");
            Console.WriteLine(msg);
        }

        /// <summary>
        /// Prints to standard output.
        /// </summary>
        public static void PrintError(string msg, [CallerMemberName] string caller = "")
        {
            Console.Write($"[Exception/Error] [{caller}] ");
            Console.WriteLine(msg);
        }

        /// <summary>
        /// Prints to standard output.
        /// </summary>
        public static void PrintException(Exception exception)
        {
            Console.WriteLine(exception.ToString());
        }
    }
}
