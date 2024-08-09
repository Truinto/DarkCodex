using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace AutoCompress // TODO: update
{
    public class CommandTool
    {
        public delegate void OnKeyPressDelegate(object sender, ConsoleKeyInfo key, out bool consumed);

        public string FilePath;
        public string Args;
        public IEnumerable<string> ArgsList;
        public Action<string> OnStandard;
        public Action<string> OnError;
        public OnKeyPressDelegate OnKeyPress;
        public readonly StringBuilder Sb_Output = new();
        public readonly StringBuilder Sb_Error = new();
        public int ExitCode;
        public Process Process;
        public bool EatCancel;

        public CommandTool()
        {
            _handler = new EventHandler(Handler);
        }

        public CommandTool(string filePath, string args, Action<string> onStandard = null, Action<string> onError = null, OnKeyPressDelegate onKeyPress = null) : this()
        {
            this.FilePath = filePath;
            this.Args = Regex.Replace(args, @"[\\^]\n", "");
            this.OnStandard = onStandard;
            this.OnError = onError;
            this.OnKeyPress = onKeyPress;
        }

        public CommandTool(string filePath, IEnumerable<string> argsList, Action<string> onStandard = null, Action<string> onError = null, OnKeyPressDelegate onKeyPress = null) : this()
        {
            this.FilePath = filePath;
            this.ArgsList = argsList;
            this.OnStandard = onStandard;
            this.OnError = onError;
            this.OnKeyPress = onKeyPress;
        }

        ~CommandTool()
        {
            SetConsoleCtrlHandler(_handler, false);
        }

        /// <summary>
        /// Synchronous process execution. Redirects input. Returns exit code and Standard/Error strings.
        /// </summary>
        public int Execute(out string output, out string error)
        {
            Execute();
            output = Sb_Output.ToString();
            error = Sb_Error.ToString();
            return ExitCode;
        }

        /// <summary>
        /// Synchronous process execution. Redirects input. Returns exit code.
        /// </summary>
        public int Execute()
        {
            Debug.WriteLine($"run-command {this.FilePath} {this.Args}");

            SetConsoleCtrlHandler(_handler, true);

            ExitCode = -1;
            try
            {
                Sb_Output.Clear();
                Sb_Error.Clear();

                Process = new Process();
                Process.StartInfo = new()
                {
                    FileName = this.FilePath,
                    Arguments = this.Args,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    //StandardOutputEncoding = Encoding.UTF8,
                    //StandardErrorEncoding = Encoding.UTF8,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                foreach (var arg in this.ArgsList ?? [])
                    Process.StartInfo.ArgumentList.Add(arg);

                Process.EnableRaisingEvents = true;
                Process.OutputDataReceived += (sender, args) =>
                {
                    lock (this)
                    {
                        OnStandard?.Invoke(args.Data);
                        Debug.WriteLine(args.Data);
                        Sb_Output.AppendLine(args.Data);
                    }
                };
                Process.ErrorDataReceived += (sender, args) =>
                {
                    lock (this)
                    {
                        OnError?.Invoke(args.Data);
                        Debug.WriteLine(args.Data);
                        Sb_Error.AppendLine(args.Data);
                    }
                };

                Process.Start();
                Process.BeginOutputReadLine();
                Process.BeginErrorReadLine();
                Process.PriorityClass = ProcessPriorityClass.BelowNormal;

                while (true)
                {
                    if (OnKeyPress != null && Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        Debug.WriteLine($"key-press {key.Modifiers} {key.Key}");
                        OnKeyPress(this, key, out bool consumed);
                        if (!consumed)
                            Process?.StandardInput.Write(key.KeyChar);
                        continue;
                    }

                    //Thread.Sleep(200);
                    //if (_process.HasExited)
                    //    break;

                    if (Process?.WaitForExit(200) != false)
                        break;
                }

                Process?.WaitForExit(); // do not remove this!
                ExitCode = Process?.ExitCode ?? -1;
                Process = null;
            }
            catch (Exception)
            {
#if DEBUG
                throw;
#endif
            }

            SetConsoleCtrlHandler(_handler, false);

            return ExitCode;
        }

        /// <summary>
        /// Asynchronous process execution. Run <see cref="WaitForExit"/>.
        /// </summary>
        public void Start()
        {
            Debug.WriteLine($"run-command {this.FilePath} {this.Args}");

            SetConsoleCtrlHandler(_handler, true);

            ExitCode = -1;
            try
            {
                var sb_standard = new StringBuilder();
                var sb_error = new StringBuilder();

                Process = new Process();
                Process.StartInfo = new()
                {
                    FileName = this.FilePath,
                    Arguments = this.Args,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    //StandardOutputEncoding = Encoding.UTF8,
                    //StandardErrorEncoding = Encoding.UTF8,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                Process.EnableRaisingEvents = true;
                Process.OutputDataReceived += (sender, args) =>
                {
                    lock (this)
                    {
                        OnStandard?.Invoke(args.Data);
                        Debug.WriteLine(args.Data);
                        sb_standard.AppendLine(args.Data);
                    }
                };
                Process.ErrorDataReceived += (sender, args) =>
                {
                    lock (this)
                    {
                        OnError?.Invoke(args.Data);
                        Debug.WriteLine(args.Data);
                        sb_error.AppendLine(args.Data);
                    }
                };

                Process.Start();
                Process.BeginOutputReadLine();
                Process.BeginErrorReadLine();
                Process.PriorityClass = ProcessPriorityClass.BelowNormal;
            }
            catch (Exception)
            {
#if DEBUG
                throw;
#endif
            }
        }

        /// <summary>
        /// Asynchronous check if process is finished. If true calls <see cref="WaitForExit"/>.
        /// </summary>
        public bool HasExited()
        {
            bool hasExited = Process?.HasExited ?? true;

            if (hasExited)
                WaitForExit(); // this makes sure the streams are flushed

            return hasExited;
        }

        /// <summary>
        /// Waits for asynchronous to finish.
        /// </summary>
        public void WaitForExit()
        {
            Process?.WaitForExit();
            ExitCode = Process?.ExitCode ?? -1;
            Process = null;

            SetConsoleCtrlHandler(_handler, false);
        }

        /// <summary>
        /// Synchronous process execution.
        /// </summary>
        public static int RunNow(string filePath, string args, Action<string> onStandard = null, Action<string> onError = null, OnKeyPressDelegate onKeyPress = null)
        {
            return new CommandTool(filePath, args, onStandard, onError, onKeyPress).Execute();
        }

        /// <summary>
        /// Synchronous process execution.
        /// </summary>
        public static int RunNow(string filePath, string args, out string output, out string error, Action<string> onStandard = null, Action<string> onError = null, OnKeyPressDelegate onKeyPress = null)
        {
            return new CommandTool(filePath, args, onStandard, onError, onKeyPress).Execute(out output, out error);
        }

        #region Kernel32
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        private EventHandler _handler;

        private enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private bool Handler(CtrlType sig)
        {
            try
            {
                Debug.WriteLine("trigger command exit handle");
                Process?.Kill();
            }
            catch (Exception) { }
            return EatCancel;
        }
        #endregion
    }
}
