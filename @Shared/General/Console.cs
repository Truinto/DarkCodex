using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Shared.GeneralNS;
using Shared.StringsNS;

namespace Shared.ConsoleNS
{
    /// <summary>
    /// Functions for use in console applications.<br/>
    /// - manipulate console output<br/>
    /// - manage processes<br/>
    /// - manage drives<br/>
    /// </summary>
    public static class ConsoleTool
    {
        public static char[] ArgsIdentifier = ['-', '/'];

        public static bool ReadArgs(this string[] args, params string[] identifier)
        {
            if (args == null || identifier == null)
                return false;

            for (int i = 0; i < args.Length; i++)
            {
                if (identifier.Contains(args[i]))
                    return true;
            }
            return false;
        }

        public static bool ReadArgs(this string[] args, ref string value, params string[] identifier)
        {
            if (args == null || identifier == null)
                return false;

            bool read = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Length < 1 || ArgsIdentifier.Contains(args[i][0]))
                    read = false;

                if (read)
                {
                    value = args[i];
                    return true;
                }

                if (identifier.Contains(args[i]))
                    read = true;
            }
            return false;
        }

        public static bool ReadArgs(this string[] args, ref List<string> value, params string[] identifier)
        {
            value ??= new();
            if (args == null || identifier == null)
                return false;

            bool read = false;
            bool result = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Length < 1 || ArgsIdentifier.Contains(args[i][0]))
                    read = false;

                if (read)
                {
                    value.Add(args[i]);
                    continue;
                }

                if (identifier.Contains(args[i]))
                    read = result = true;
            }
            return result;
        }

        public static bool DownloadFile(string url, string filePath) => DownloadFile(new Uri(url), filePath);

        public static bool DownloadFile(Uri uri, string filePath)
        {
            try
            {
                if (uri.IsFile)
                    return false;

                //var client = new WebClient();
                //client.DownloadFile(url, filePath);
                //client.UploadFile(url, null, filePath);

                using var sw = new FileStream(filePath, FileMode.CreateNew);
                using var client2 = new HttpClient();
                using var request = client2.GetAsync(uri);
                request.Result.Content.CopyToAsync(sw).Wait();
                sw.Close();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Runs command or program. Blocks execution until finished.
        /// </summary>
        /// <param name="command">Command or program path.</param>
        /// <param name="args">Process arguments to use.</param>
        /// <param name="output">Console text output.</param>
        /// <param name="onData">Data receive action.</param>
        /// <param name="priority">Process prioirty.</param>
        /// <returns>Process exit code</returns>
        public static int RunCommand(string command, string args, out string output, Action<string> onData = null, ProcessPriorityClass priority = ProcessPriorityClass.BelowNormal)
        {
            var sb = new StringBuilder();
            if (onData == null)
                onData = s => sb.AppendLine(s);
            else
                onData += s => sb.AppendLine(s);

            using var p = RunCommandAsync(command: command, args: args, onData: onData, startNow: true, priority: priority);
            p.WaitForExit();

            output = sb.ToString();
            return p.ExitCode;
        }

        /// <summary>
        /// Runs command or program.
        /// </summary>
        /// <param name="command">Command or program path.</param>
        /// <param name="args">Process arguments to use.</param>
        /// <param name="onData">Data receive action. Prints to console, if empty.</param>
        /// <param name="startNow">Whenever to start the process immediately.</param>
        /// <param name="priority">Process prioirty, only if startNow is true.</param>
        /// <returns>Process thread</returns>
        public static Process RunCommandAsync(string command, string args = "", Action<string> onData = null, bool startNow = true, ProcessPriorityClass priority = ProcessPriorityClass.BelowNormal)
        {
            onData ??= System.Console.WriteLine;

            var p = new Process();
            p.StartInfo.FileName = command;
            p.StartInfo.Arguments = args;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            p.StartInfo.StandardErrorEncoding = Encoding.UTF8;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.EnableRaisingEvents = true;
            p.OutputDataReceived += (sender, args) => onData(args.Data);
            p.ErrorDataReceived += (sender, args) => onData(args.Data);
            if (startNow)
            {
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.PriorityClass = priority;
            }
            //p.WaitForExit();
            return p;
        }

        public static void ProcessState(this Process p, out bool start, out bool exit)
        {
            start = false;
            exit = false;
            try { exit = p.HasExited; start = true; } catch (Exception) { }
        }

        /// <summary>
        /// Run the current assembly as admin. Force exists application.
        /// </summary>
        public static void RunAdmin(string args)
        {
            var proc = new ProcessStartInfo
            {
                Arguments = args,
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Assembly.GetEntryAssembly().Location, //Assembly.GetEntryAssembly().CodeBase
                Verb = "runas"
            };
            Process.Start(proc);
        }

        [DllImport("libc", EntryPoint = "getuid")]
        private static extern uint getuid();
        public static bool IsAdmin()
        {
            bool isAdmin;
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    isAdmin = IsUserAnAdmin();
                else
                    isAdmin = getuid() == 0;
            }
            catch (Exception)
            {
                isAdmin = false;
            }
            return isAdmin;
        }

        public static void ConsoleBackspace()
        {
            if (!char.IsWhiteSpace(GetChar()))
            {
                System.Console.CursorLeft += 0;
                System.Console.Write(" ");
                System.Console.CursorLeft += 0;
            }
            else if (System.Console.CursorLeft > 0)
            {
                System.Console.Write("\b \b");
            }
            else if (System.Console.CursorTop > 0)
            {
                System.Console.CursorTop--;
                var line = GetText(System.Console.CursorTop).TrimEnd();
                System.Console.CursorLeft = Math.Min(line.Length, System.Console.BufferWidth - 1);
                System.Console.Write(" ");
                System.Console.CursorLeft -= System.Console.CursorLeft < System.Console.BufferWidth - 1 ? 1 : 0;
            }
        }

        public static int GetDistance(int x, int y)
        {
            int startL = System.Console.CursorLeft;
            int startT = System.Console.CursorTop;
            int width = System.Console.BufferWidth;

            return (y - startT) * width + (x - startL);
        }

        public static void PutText(int x, int y, string text)
        {
            int startL = System.Console.CursorLeft;
            int startT = System.Console.CursorTop;

            System.Console.SetCursorPosition(x, y);
            System.Console.Write(text);
            int dist = GetDistance(startL, startT);
            if (dist > 0)
            {
                startL = System.Console.CursorLeft;
                startT = System.Console.CursorTop;
                System.Console.Write(new string(' ', dist));
                System.Console.SetCursorPosition(startL, startT);
            }
        }

        public static string ReadLineDefault(string @default)
        {
            int startL = System.Console.CursorLeft;
            int startT = System.Console.CursorTop;

            System.Console.Write(@default);

            while (true)
            {
                var c = System.Console.ReadKey(true);

                if (c.Key == ConsoleKey.Enter)
                {
                    break;
                }

                if (c.Key == ConsoleKey.LeftArrow || c.Key == ConsoleKey.Backspace)
                {
                    if (System.Console.CursorTop <= startT && System.Console.CursorLeft <= startL)
                        continue;

                    if ((c.Modifiers & (ConsoleModifiers.Shift | ConsoleModifiers.Control)) != 0)
                    {
                        PutText(startL, startT, "");
                        continue;
                    }

                    ConsoleBackspace();
                    continue;
                }

                if (c.KeyChar != '\0')
                {
                    System.Console.Write(c.KeyChar);
                    continue;
                }
            }

            return GetText(startL, startT, -1);
        }

        private static List<string> _dirs;
        private static int _dirsI;
        public static string ReadLineTabComplete()
        {
            int startL = System.Console.CursorLeft;
            int startT = System.Console.CursorTop;
            string buffer;
            _dirs ??= new();

            //System.Console.WriteLine(Directory.GetCurrentDirectory());
            //System.Console.CursorSize = 25;

            while (true)
            {
                var c = System.Console.ReadKey(true);

                if (c.Key == ConsoleKey.Enter)
                {
                    break;
                }

                if (c.Key == ConsoleKey.Tab)
                {
                    // cycle through dirs
                    if (_dirs.Count > 0)
                    {
                        bool shift = (c.Modifiers & (ConsoleModifiers.Shift | ConsoleModifiers.Control)) != 0;
                        if (!shift && ++_dirsI >= _dirs.Count)
                            _dirsI = 0;
                        if (shift && --_dirsI < 0)
                            _dirsI = _dirs.Count - 1;

                        PutText(startL, startT, _dirs[_dirsI]);
                        continue;
                    }

                    // find new dirs
                    buffer = GetText(startL, startT, -1);

                    // fix input, if only letter is defined
                    var vol = buffer.ExpandWindowsVolume();
                    if (vol != buffer)
                    {
                        PutText(startL, startT, vol);
                        continue;
                    }

                    buffer.GetDirCompletion(_dirs);

                    // set to first find; if only one, clear cycles
                    if (_dirs.Count > 0)
                    {
                        _dirsI = 0;
                        PutText(startL, startT, _dirs[0]);
                        if (_dirs.Count == 1)
                            _dirs.Clear();
                    }

                    continue;
                }

                _dirs.Clear();

                if (c.Key == ConsoleKey.LeftArrow || c.Key == ConsoleKey.Backspace)
                {
                    if (System.Console.CursorTop <= startT && System.Console.CursorLeft <= startL)
                        continue;

                    if ((c.Modifiers & (ConsoleModifiers.Shift | ConsoleModifiers.Control)) != 0)
                    {
                        PutText(startL, startT, "");
                        continue;
                    }

                    ConsoleBackspace();
                    continue;
                }

                if (c.KeyChar != '\0')
                {
                    System.Console.Write(c.KeyChar);
                    continue;
                }
            }

            return GetText(startL, startT, -1);
        }

        public static string GetMountPoint(string path)
        {
            const int bufferlength = 50;
            IntPtr ptr = Marshal.AllocCoTaskMem(bufferlength);
            GetVolumeNameForVolumeMountPointW(path, ptr, bufferlength);
            string r = Marshal.PtrToStringUni(ptr, bufferlength);
            Marshal.FreeCoTaskMem(ptr);
            return r;
        }

        public static bool DelMountPoint(string path)
        {
            return DeleteVolumeMountPointW(path);
        }

        public static int GetProcessHandleCount(IntPtr handle)
        {
            GetProcessHandleCount(handle, out int count);
            return count;
        }

        public static ulong GetProcessCycleTime(IntPtr handle)
        {
            QueryProcessCycleTime(handle, out ulong count);
            return count;
        }

        public static int GetLastErrorCode()
        {
            int errorId = GetLastError();
            SetLastError(errorId); // this doesn't work
            return errorId;
        }

        public static string GetLastErrorMessage() // causes error 1008
        {
            const int bufferlength = 1024;

            int errorId = GetLastError();
            if (errorId == 0)
                return "No error.";

            IntPtr str = Marshal.AllocCoTaskMem(bufferlength);
            _ = FormatMessageW(0x00000200 | 0x00001000, default, errorId, 0, str, bufferlength, default);
            string error = Marshal.PtrToStringUni(str);
            Marshal.FreeCoTaskMem(str);

            SetLastError(errorId); // this doesn't work

            return $"Error {errorId}: {error}";
        }

        public static List<ManagementBaseObject> GetUSBDrives()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new NotImplementedException();

            var list = new List<ManagementBaseObject>();

            try
            {
                foreach (var device in new ManagementObjectSearcher(@"SELECT * FROM Win32_DiskDrive WHERE InterfaceType LIKE 'USB%'").Get())
                {
                    //System.Console.WriteLine((string)device.GetPropertyValue("DeviceID"));
                    //System.Console.WriteLine((string)device.GetPropertyValue("PNPDeviceID"));

                    foreach (var partition in new ManagementObjectSearcher(
                        "ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" + device.Properties["DeviceID"].Value + "'} WHERE AssocClass = Win32_DiskDriveToDiskPartition").Get())
                    {
                        foreach (var disk in new ManagementObjectSearcher(
                                    "ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + partition["DeviceID"] + "'} WHERE AssocClass = Win32_LogicalDiskToPartition").Get())
                        {
                            //if (!list.Contains(disk))
                            list.Add(disk);
                        }
                    }
                }
            }
            catch (Exception) { }

            return list;
        }

        public static ManagementBaseObject GetDriveByVolume(string letter, List<ManagementBaseObject> drives)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new NotImplementedException();

            if (letter == null)
                return null;

            drives ??= GetUSBDrives();
            if (drives == null)
                return null;

            foreach (var drive in drives)
            {
                if (letter.StartsWithO((string)drive["Caption"]))
                    return drive;
            }
            return null;
        }

        #region jrnker © 2020 MIT / edited Truinto
        //source: https://github.com/jrnker/Proxmea.ConsoleHelper/

        /// <summary>
        /// Get current cursor position from console window.
        /// In .Net 5 > use System.Console.GetCursorPosition
        /// </summary>
        /// <returns>Cursor position</returns>
        public static COORD GetCursorPosition()
        {
            // in .net 5 there's System.Console.GetCursorPosition for this
            return GetConsoleInfo(Stdout).dwCursorPosition;
        }

        /// <summary>
        /// Retrieves information about the current screen buffer window
        /// </summary>
        public static CONSOLE_SCREEN_BUFFER_INFO GetConsoleInfo()
        {
            return GetConsoleInfo(Stdout);
        }

        public static CONSOLE_SCREEN_BUFFER_INFO GetConsoleInfo(IntPtr ptr)
        {
            if (!GetConsoleScreenBufferInfo(ptr, out CONSOLE_SCREEN_BUFFER_INFO outInfo))
                throw new Exception();
            return outInfo;
        }

        /// <summary>
        /// Find text in console window
        /// </summary>
        /// <returns>List of found coordinates</returns>
        public static List<COORD> IndexOfInConsole(string text)
        {
            return IndexOfInConsole(new[] { text });
        }

        /// <summary>
        /// Find texts in console window
        /// </summary>
        /// <param name="text"></param>
        /// <returns>List of found coordinates</returns>
        public static List<COORD> IndexOfInConsole(string[] text)
        {
            var coords = new List<COORD>();

            // Get Console Info
            var stdout = Stdout;
            var consoleInfo = GetConsoleInfo(stdout);

            for (int y = 0; y < consoleInfo.dwCursorPosition.Y; y++)
            {
                var line = GetText(y);

                // Search through the line and put the results in coords
                foreach (var t in text)
                {
                    var xPos = 0;
                    while (true)
                    {
                        var pos = line.IndexOf(t, xPos);
                        if (pos == -1)
                            break;
                        coords.Add(new COORD(x: pos, y: y));
                        xPos = pos + 1;
                    }
                }
            }
            return coords;
        }

        public static char GetChar() => GetChar(new COORD(System.Console.CursorLeft, System.Console.CursorTop), Stdout);
        public static char GetChar(int x, int y) => GetChar(new COORD(x, y), Stdout);
        /// <summary>
        /// Retrieve character from console window
        /// </summary>
        public static char GetChar(COORD coordinate, IntPtr ptr)
        {
            if (!ReadConsoleOutputCharacterW(ptr, out char chUnicode, sizeof(char), coordinate, out _))
                throw new Exception();
            return chUnicode;
        }

        /// <summary>
        /// Retrieve character from console window
        /// </summary>
        /// <returns>true if successful</returns>
        public static bool GetChar(int x, int y, out char value)
        {
            return ReadConsoleOutputCharacterW(Stdout, out value, sizeof(char), new COORD(x, y), out var len) && len == sizeof(char);
        }

        public static bool WriteChar(char ch) => WriteChar(new COORD(System.Console.CursorLeft, System.Console.CursorTop), ch, Stdout);
        public static bool WriteChar(int x, int y, char ch) => WriteChar(new COORD(x, y), ch, Stdout);
        public static bool WriteChar(COORD coord, char ch, IntPtr ptr)
        {
            return WriteConsoleOutputCharacterW(ptr, ch, sizeof(char), coord, out var len) && len == sizeof(char);
        }

        public static string GetText(int y) => GetText(0, y, System.Console.BufferWidth - y, Stdout);
        public static string GetText(int x, int y) => GetText(x, y, System.Console.BufferWidth - y, Stdout);
        public static string GetText(int x, int y, int length) => GetText(x, y, length, Stdout);
        /// <summary>
        /// Retrieve text from console window.<br/>
        /// No length will return until end of line.<br/>
        /// Length below zero will return until cursor position.
        /// </summary>
        public static string GetText(int x, int y, int length, IntPtr ptr)
        {
            bool stopAtCursor = length < 0;

            try
            {
                var sb = GetSb();
                int num;
                char ch;

                while (x < System.Console.BufferWidth
                    && y < System.Console.BufferHeight
                    && length != 0)
                {
                    // stop at cursor
                    if (stopAtCursor && x == System.Console.CursorLeft && y == System.Console.CursorTop)
                        break;

                    // read char
                    ReadConsoleOutputCharacterW(ptr, out ch, sizeof(char), new COORD(x, y), out num);
#if DEBUG
                    Debug.WriteLine($"{x},{y}\tch={ch}\tint={(int)ch}\tlength={num}");
#endif
                    // save char
                    sb.Append(ch);
                    length--;

                    // if char is displayed in two cells, skip reading the next one
                    if (num == 1)
                        ++x;

                    // move position by one; wrapping
                    if (++x >= System.Console.BufferWidth)
                    {
                        x = 0;
                        y++;
                    }
                }
            }
            catch (Exception) { }
            return FlushSb();
        }

        #endregion

        #region kernel32

        //list of functions https://www.geoffchappell.com/studies/windows/win32/kernel32/api/

        public const int STD_OUTPUT_HANDLE = -11;
        public const long INVALID_HANDLE_VALUE = -1;

        [StructLayout(LayoutKind.Sequential)]
        public struct CONSOLE_SCREEN_BUFFER_INFO
        {
            public COORD dwSize;
            public COORD dwCursorPosition;
            public short wAttributes;
            public SMALL_RECT srWindow;
            public COORD dwMaximumWindowSize;

            public override string ToString()
            {
                return $"size=[{dwSize}] cursor=[{dwCursorPosition}] attribute=[{wAttributes}] window=[{srWindow}] max=[{dwMaximumWindowSize}]";
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public readonly struct COORD
        {
            public readonly short X;
            public readonly short Y;

            public COORD(int x, int y)
            {
                X = (short)x;
                Y = (short)y;
            }

            public readonly override string ToString()
            {
                return $"x={X},y={Y}";
            }

            public static implicit operator uint(COORD coord)
            {
                return (ushort)coord.X | (uint)coord.Y << 16;
            }

            public static implicit operator COORD(uint integer)
            {
                int x = (int)(integer & 0x0000FFFF);
                int y = (int)((integer & 0xFFFF0000) >> 16);
                return new COORD(x, y);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SMALL_RECT
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;

            public override string ToString()
            {
                return $"x1={Left},y1={Top},x2={Right},y2={Bottom}";
            }
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern void SetLastError(int dwErrCode);

        [DllImport("kernel32.dll", SetLastError = false, CharSet = CharSet.Unicode)]
        private static extern int GetLastError();

        [DllImport("kernel32.dll", SetLastError = false, CharSet = CharSet.Unicode)]
        private static extern int FormatMessageW(int dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId, IntPtr lpBuffer, int nSize, IntPtr Arguments);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool GetConsoleScreenBufferInfo(IntPtr hConsoleOutput, out CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr GetStdHandle(int num = STD_OUTPUT_HANDLE);

        /// <param name="hStdout">result of 'GetStdHandle(-11)'</param>
        /// <param name="ch">A̲N̲S̲I̲ character result</param>
        /// <param name="c_in">set to '1'</param>
        /// <param name="coord_XY"> screen location to read, X:loword, Y:hiword</param>
        /// <param name="c_out">(unwanted, discard)</param>
        /// <returns>false if error</returns>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ReadConsoleOutputCharacterA(IntPtr hStdout, out byte ch, int c_in, COORD coord_XY, out int c_out);

        /// <param name="hStdout">result of 'GetStdHandle(-11)'</param>
        /// <param name="ch">U̲n̲i̲c̲o̲d̲e̲ character result</param>
        /// <param name="c_in">set to 'sizeof(char)'</param>
        /// <param name="coord_XY">screen location to read, X:loword, Y:hiword</param>
        /// <param name="c_out">sizeof(char) or 1 if second part of a wide character</param>
        /// <returns>false if error</returns>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ReadConsoleOutputCharacterW(IntPtr hStdout, out char ch, int c_in, COORD coord_XY, out int c_out);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hStdout">result of 'GetStdHandle(-11)'</param>
        /// <param name="ch">U̲n̲i̲c̲o̲d̲e̲ character to write</param>
        /// <param name="c_in">set to 'sizeof(char)'</param>
        /// <param name="coord_XY">screen location to write, X:loword, Y:hiword</param>
        /// <param name="c_out">returns sizeof(char) if successful</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WriteConsoleOutputCharacterW(IntPtr hStdout, char ch, int c_in, COORD coord_XY, out int c_out);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetConsoleOutputCP(uint wCodePageID = 65001u);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetConsoleCP(uint wCodePageID = 65001u);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetVolumeNameForVolumeMountPointW(string lpszVolumeMountPoint, IntPtr lpszVolumeName, int cchBufferLength);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindFirstVolumeMountPointW(string lpszRootPathName, IntPtr lpszVolumeMountPoint, int cchBufferLength);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FindNextVolumeMountPointW(IntPtr hFindVolumeMountPoint, IntPtr lpszVolumeMountPoint, int cchBufferLength);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FindVolumeMountPointClose(IntPtr hFindVolumeMountPoint);

        /// <param name="lpszVolumeMountPoint">Y:\MountX</param>
        /// <param name="lpszVolumeName">\\?\Volume{00000000-0000-0000-0000-000000000000}\</param>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetVolumeMountPointW(string lpszVolumeMountPoint, string lpszVolumeName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteVolumeMountPointW(string lpszVolumeMountPoint);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetProcessHandleCount(IntPtr hProcess, out int pdwHandleCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool QueryProcessCycleTime(IntPtr processHandle, out ulong cycleTime);

        public static void TEST()
        {
            SetVolumeMountPointW("C:\\mount\\Test\0", @"\\?\Volume{d1c10f04-8df5-11ea-9706-84c5a6fd6e29}\ ");
            Debug.WriteLine(GetLastErrorMessage());

            const int bufferlength = 260;
            IntPtr str = Marshal.AllocCoTaskMem(bufferlength);
            Marshal.WriteInt32(str, 0); // set zero termination

            IntPtr handle = FindFirstVolumeMountPointW("\\\\?\\Volume{d1c10f04-8df5-11ea-9706-84c5a6fd6e29}\\\0", str, bufferlength); //@"\\?\Volume{d1c10f04-8df5-11ea-9706-84c5a6fd6e29}\"
            Debug.WriteLine(GetLastErrorMessage());
            Debug.WriteLine(Marshal.PtrToStringUni(str));

            FindNextVolumeMountPointW(handle, str, bufferlength);
            Debug.WriteLine(GetLastErrorMessage());
            Debug.WriteLine(Marshal.PtrToStringUni(str));

            FindVolumeMountPointClose(handle);
            Marshal.FreeCoTaskMem(str);
        }

        private static IntPtr _stdout;
        public static IntPtr Stdout => _stdout != default ? _stdout : _stdout = GetStdHandle();

        #endregion

        #region shell32

        [DllImport("shell32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsUserAnAdmin();

        #endregion

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
