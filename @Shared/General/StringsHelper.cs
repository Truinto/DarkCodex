using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Shared
{
    public static class StringsHelper
    {
        #region Args

        /// <summary>
        /// Joins an array of arguments into a single string, which can be used for commands.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string JoinArgs(string[] args)
        {
            if (args == null || args.Length == 0)
                return "";

            var sb = new StringBuilder();

            foreach (string arg in args)
            {
                if (arg == null)
                    continue;

                if (arg.Contains(' '))
                {
                    sb.Append('\"');
                    sb.Append(arg);
                    sb.Append('\"');
                }
                else
                {
                    sb.Append(arg);
                }

                sb.Append(' ');
            }

            return sb.ToString();
        }

        #endregion

        #region Conversions

        public static ulong TryToUInt64(this object obj)
        {
            try
            {
                return Convert.ToUInt64(obj);
            }
            catch (Exception)
            {
                return 0UL;
            }
        }

        public static string GetDiskSize(this object obj)
        {
            ulong size = obj.TryToUInt64();

            switch (size)
            {
                case < 1024UL:
                    return $"{size} bytes";
                case < 10485760UL:
                    return $"{size / 1024UL:0,0} KiB";
                case < 10737418240UL:
                    return $"{size / 1048576UL:0,0} MiB";
                default:
                    return $"{size / 1073741824UL:0,0} GiB";
            }
        }

        #endregion

        #region Path

        /// <summary>
        /// Converts string into valid absolute path.<br/>
        /// Returns null, if invalid path (e.g. illegal characters).
        /// </summary>
        public static string FilterPath(this string path)
        {
            try
            {
                path = path.Trim();
                var dir = new DirectoryInfo(path);
                //Console.WriteLine($"{dir.FullName}, {dir.Name}, {dir.Parent}");
                return dir.FullName;
            }
            catch (Exception) { return default; }
        }

        public static void GetDirCompletion(this string path, List<string> dirs)
        {
            try
            {
                dirs.Clear();
                var info = new DirectoryInfo(path);
                var parent = info.Parent;
                string search = $"{info.Name}*";

                if (parent == null || info.Exists)
                {
                    search = "*";
                    parent = info;
                }

                var subs = parent.GetDirectories(search, SearchOption.TopDirectoryOnly);
                dirs.AddRange(subs.Where(w => !w.Attributes.HasFlag(FileAttributes.Hidden)).Select(s => s.FullName));
                //Debug.WriteLine($"list={dirs.Join()}");
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Returns to windows style volume expanded string "C:\".
        /// Otherwise returns input string.
        /// </summary>
        public static string ExpandWindowsVolume(this string letter)
        {
            if (letter == null || letter.Length < 1 || letter.Length > 2)
                return letter;
            if (!letter[0].IsLetter())
                return letter;
            if (letter.Length == 2 && letter[1] != ':')
                return letter;
            return $"{letter[0]}:\\";
        }

        public static string MergePaths(IEnumerable<string> paths)
        {
            if (paths == null)
                return "";
            try
            {
                var sb = GetSb();
                bool first = true;
                foreach (string path in paths)
                {
                    string full = FilterPath(path);
                    if (full.IsEmpty())
                        continue;

                    if (!first)
                        sb.Append(Path.PathSeparator);
                    else
                        first = false;
                    sb.Append(full);
                }
            }
            catch (Exception) { }
            return FlushSb();
        }

        #endregion

        #region Enum

        #endregion

        public static readonly char[] InvalidFileNameChars = new char[41]
        {
            '"', '<', '>', '|', '\0', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005',
            '\u0006', '\a', '\b', '\t', '\n', '\v', '\f', '\r', '\u000e', '\u000f',
            '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017', '\u0018', '\u0019',
            '\u001a', '\u001b', '\u001c', '\u001d', '\u001e', '\u001f', ':', '*', '?', '\\',
            '/'
        };

        /// <summary>
        /// Checks for files or directories.
        /// </summary>
        public static bool PathExists(string path)
        {
            if (File.Exists(path))
                return true;
            if (Directory.Exists(path))
                return true;
            return false;
        }

        private static Regex _rxFileBrackets;
        private static Regex _rxFileExtension;
        /// <summary>
        /// Returns file path that does not exist. Appends (1) or increases existing numberation, if file already exists.
        /// </summary>
        public static string GetUniqueFilename(string path)
        {
            _rxFileBrackets ??= new Regex(@"(.*)\((\d+)\)(?!.*[\/\\])(.*)", RegexOptions.Compiled | RegexOptions.RightToLeft);
            _rxFileExtension ??= new Regex(@"(.*)(?!.*[\/\\])(\..*)", RegexOptions.Compiled | RegexOptions.RightToLeft);

            while (PathExists(path))
            {
                Match match;
                if ((match = _rxFileBrackets.Match(path)).Success && int.TryParse(match.Groups[2].Value, out int number))
                    path = match.Result($"$1({number + 1})$3");
                else if ((match = _rxFileExtension.Match(path)).Success)
                    path = match.Result("$1(1)$2");
                else
                    path += "(1)";
            }

            return path;
        }

        public static string FilterFilename(this string filename)
        {
            if (filename == null)
                return null;
            try
            {
                var sb = GetSb();
                foreach (char c in filename.Trim())
                {
                    if (!InvalidFileNameChars.Contains(c))
                        sb.Append(c);
                }
            }
            catch (Exception) { }
            return FlushSb();
        }

        /// <summary>
        /// Escapes Unicode and ASCII non printable characters
        /// </summary>
        /// <param name="input">The string to convert</param>
        /// <returns>An escaped string literal</returns>
        public static string ToLiteral(this string input)
        {
            var sb = new StringBuilder(input.Length + 2);
            foreach (char c in input)
            {
                switch (c)
                {
                    case '\'': sb.Append(@"\'"); break;
                    case '\"': sb.Append("\\\""); break;
                    case '\\': sb.Append(@"\\"); break;
                    case '\0': sb.Append(@"\0"); break;
                    case '\a': sb.Append(@"\a"); break;
                    case '\b': sb.Append(@"\b"); break;
                    case '\f': sb.Append(@"\f"); break;
                    case '\n': sb.Append(@"\n"); break;
                    case '\r': sb.Append(@"\r"); break;
                    case '\t': sb.Append(@"\t"); break;
                    case '\v': sb.Append(@"\v"); break;
                    default:
                        if (c >= 0x20 && c <= 0x7e)
                        {
                            sb.Append(c);
                        }
                        else
                        {
                            sb.Append(@"\u");
                            sb.Append(((int)c).ToString("x4"));
                        }
                        break;
                }
            }
            return sb.ToString();
        }

        /// <summary>Joins an enumeration with a value converter and a delimiter to a string</summary>
        /// <typeparam name="T">The inner type of the enumeration</typeparam>
        /// <param name="enumeration">The enumeration</param>
        /// <param name="converter">An optional value converter (from T to string)</param>
        /// <param name="delimiter">An optional delimiter</param>
        /// <returns>The values joined into a string</returns>
        ///
        public static string Join<T>(this IEnumerable<T> enumeration, Func<T, string> converter = null, string delimiter = ", ")
        {
            converter ??= t => t.ToString();
            return enumeration.Aggregate("", (prev, curr) => prev + (prev.Length > 0 ? delimiter : "") + converter(curr));
        }

        /// <summary>Returns substring. Always excludes char 'c'. Returns null, if index is out of range or char not found.</summary>
        /// <param name="str">source string</param>
        /// <param name="c">char to search for</param>
        /// <param name="start">start index; negative number search last index instead</param>
        /// <param name="tail">get tail instead of head</param>
        public static string TrySubstring(this string str, char c, int start = 0, bool tail = false)
        {
            try
            {
                if (tail)
                {
                    if (start < 0)
                        return str.Substring(str.LastIndexOf(c) + 1);
                    return str.Substring(str.IndexOf(c, start) + 1);
                }

                if (start < 0)
                    return str.Substring(0, str.LastIndexOf(c));
                return str.Substring(start, str.IndexOf(c, start));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool IsNumber(this char c)
        {
            return c >= 48 && c <= 57;
        }

        public static bool IsUppercase(this char c)
        {
            return c >= 65 && c <= 90;
        }

        public static bool IsLowercase(this char c)
        {
            return c >= 97 && c <= 122;
        }

        public static bool IsLetter(this char c)
        {
            return c >= 65 && c <= 90 || c >= 97 && c <= 122;
        }

        public static bool IsAlphanumeric(this char c)
        {
            return c >= 48 && c <= 57 || c >= 65 && c <= 90 || c >= 97 && c <= 122;
        }

        public static bool IsNotSpaced(this StringBuilder sb)
        {
            if (sb.Length == 0)
                return false;

            return sb[sb.Length - 1] != ' ';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWithO(this string source, string value) => source.StartsWith(value, StringComparison.Ordinal);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWithOI(this string source, string value) => source.StartsWith(value, StringComparison.OrdinalIgnoreCase);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWithO(this string source, string value) => source.EndsWith(value, StringComparison.Ordinal);

        public static bool ContainsOI(this IEnumerable<string> collection, string value)
        {
            foreach (var item in collection)
                if (value.Equals(item, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        public static bool IsEmpty(this string str)
        {
            return str == null || str.Length == 0;
        }

        /// <summary>
        /// Evaluator for Regex.Replace extension.
        /// </summary>
        public delegate string RegexEvaluator(Match match, int index, int count);

        /// <summary>
        /// Regex.Replace, but with additional index and count values.
        /// </summary>
        public static string Replace(this Regex rx, string input, RegexEvaluator evaluator)
        {
            var matches = rx.Matches(input);
            if (matches.Count <= 0)
                return input;
            try
            {
                var sb = GetSb();
                int index = 0;
                for (int i = 0; i < matches.Count; i++)
                {
                    var match = matches[i];
                    if (index < match.Index)
                    {
                        sb.Append(input, index, match.Index - index);
                        index = match.Index + match.Length;
                    }
                    sb.Append(evaluator(match, i, matches.Count));
                }

                if (index < input.Length)
                {
                    sb.Append(input, index, input.Length - index);
                }
            }
            catch (Exception) { }
            return FlushSb();
        }

        #region Buffer

        private static readonly StringBuilder _sb = new();
        private static StringBuilder GetSb()
        {
            System.Threading.Monitor.Enter(_sb);
            //_sb.Clear();
            return _sb;
        }
        private static string FlushSb()
        {
            string text = _sb.ToString();
            _sb.Clear();
            System.Threading.Monitor.Exit(_sb);
            return text;
        }

        #endregion
    }
}
