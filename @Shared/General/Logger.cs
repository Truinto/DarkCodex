using System;
using System.Runtime.CompilerServices;

namespace Shared.Loggers
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
            System.Console.Write($"[DEBUG] [{caller}] ");
            System.Console.WriteLine(msg);
        }

        /// <summary>
        /// Prints to standard output.
        /// </summary>
        public static void Print(string msg)
        {
            System.Console.Write($"[INFO] ");
            System.Console.WriteLine(msg);
        }

        /// <summary>
        /// Prints to standard output.
        /// </summary>
        public static void PrintWarning(string msg, [CallerMemberName] string caller = "")
        {
            System.Console.Write($"[WARNING] [{caller}] ");
            System.Console.WriteLine(msg);
        }

        /// <summary>
        /// Prints to standard output.
        /// </summary>
        public static void PrintError(string msg, [CallerMemberName] string caller = "")
        {
            System.Console.Write($"[Exception/Error] [{caller}] ");
            System.Console.WriteLine(msg);
        }

        /// <summary>
        /// Prints to standard output.
        /// </summary>
        public static void PrintException(Exception exception)
        {
            System.Console.WriteLine(exception.ToString());
        }
    }
}
