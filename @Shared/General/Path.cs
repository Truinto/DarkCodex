using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Paths
{
    public enum FileType
    {
        Undefined,
        Directory,
        File,
    }

    /// <summary>
    /// WIP. Tool to handle path operations.
    /// </summary>
    public class PathInfo
    {
        public static char PathSeparator => System.IO.Path.DirectorySeparatorChar;

        public static bool IsValidPath(string path)
        {
            try
            {
                System.IO.Path.GetFullPath(path);
                return true;
            }
            catch
            {
                return false;
            }
            //return name.IndexOfAny(Path.GetInvalidPathChars()) == -1;
        }

        private bool isDirty;

        public PathInfo(string path)
        {
            if (string.IsNullOrEmpty(path))
                path = ".";
            path = path.Replace(PathSeparator == '/' ? '\\' : '/', PathSeparator);

            int indexSlash = path.LastIndexOf(PathSeparator);
            int indexDot = path.LastIndexOf('.');
            bool hasNoExtension = indexDot <= indexSlash + 1;

            this.FullName = path;
            this.Root = System.IO.Path.GetPathRoot(path);
            this.IsAbsolute = this.Root.Length > 0;
            this.Directory = path.Substring(0, indexSlash + 1);
            if (hasNoExtension)
            {
                this.FileNameNoExtension = path.Substring(indexSlash + 1);
                if (this.FileNameNoExtension.EndsWith("."))
                    this.FileNameNoExtension = this.FileNameNoExtension.Substring(1);
                this.Extension = "";
                this.FileName = this.FileNameNoExtension;
            }
            else
            {
                this.FileNameNoExtension = path.Substring(indexSlash + 1, indexDot - indexSlash - 1);
                this.Extension = path.Substring(indexDot + 1);
                this.FileName = $"{this.FileNameNoExtension}.{this.Extension}";
            }
        }

        public bool IsValid { get; } //WIP

        public string FullName { get; }

        public string Root { get; }

        public string Directory { get; }

        public string FileNameNoExtension { get; }

        public string Extension { get; }

        public string FileName { get; }

        public FileType Type { get; } //WIP

        public bool IsAbsolute { get; } //WIP

        public string PathAbsolute(string workingDirectory = null)
        {
            if (this.IsAbsolute && !string.IsNullOrEmpty(workingDirectory))
                Trace.WriteLine($"[Warning] {typeof(PathInfo).FullName}.PathAbsolute trying to set working directory on an absolute path '{FullName}'");

            if (this.IsAbsolute)
                return this.FullName;

            workingDirectory ??= System.IO.Directory.GetCurrentDirectory(); //System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)
            return workingDirectory + this.FullName;

            throw new NotImplementedException();
        }

        private static void Sandbox()
        {
            System.IO.Directory.Delete("");
            System.IO.Directory.Exists("");
            System.IO.Directory.CreateDirectory("");

            System.IO.File.Delete("");
            System.IO.File.Exists("");
            System.IO.File.CreateText("");

            System.IO.Path.ChangeExtension("", "");
            System.IO.Path.Combine("", "");
            System.IO.Path.GetDirectoryName(""); // warning, this is inconsistent
            System.IO.Path.GetExtension("");
            System.IO.Path.GetFileName("");
            System.IO.Path.GetFileNameWithoutExtension("");
            System.IO.Path.GetFullPath("");
            System.IO.Path.GetInvalidFileNameChars();
            System.IO.Path.GetPathRoot("");
            System.IO.Path.GetRandomFileName();
            System.IO.Path.GetTempFileName();
            System.IO.Path.GetTempPath();
            System.IO.Path.HasExtension("");
            System.IO.Path.IsPathRooted("");

            var di = new System.IO.DirectoryInfo("");
            _ = di.Parent;
            _ = di.Exists;
            _ = di.FullName;
            di.Create();
            di.Delete();
            di.EnumerateDirectories();
            di.EnumerateFiles();
            di.GetDirectories();
            di.GetFiles();
            di.MoveTo("");
            di.CreateSubdirectory("");

            var fi = new System.IO.FileInfo("");
            _ = fi.Exists;
            fi.CreateText();
            fi.Delete();

        }
    }

    public static class PathHelper
    {
        /// <summary>%username%</summary>
        public static string Username => Environment.UserName;

        /// <summary>C:\Users\%username%</summary>
        public static string UserProfile => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\Users\%username%\Desktop</summary>
        public static string DesktopDirectory => Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\Program Files</summary>
        public static string ProgramFiles => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\Program Files (x86)</summary>
        public static string ProgramFilesX86 => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\Users\%username%\OneDrive - Bruker Physik GmbH\Documents</summary>
        public static string MyDocuments => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\Users\%username%\Pictures</summary>
        public static string MyPictures => Environment.GetFolderPath(Environment.SpecialFolder.MyPictures, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\Users\%username%\Music</summary>
        public static string MyMusic => Environment.GetFolderPath(Environment.SpecialFolder.MyMusic, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\Users\%username%\Videos</summary>
        public static string MyVideos => Environment.GetFolderPath(Environment.SpecialFolder.MyVideos, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\Users\%username%\AppData\Roaming</summary>
        public static string ApplicationData => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\Users\%username%\AppData\Local</summary>
        public static string LocalApplicationData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\ProgramData</summary>
        public static string CommonApplicationData => Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\WINDOWS\system32</summary>
        public static string System => Environment.GetFolderPath(Environment.SpecialFolder.System, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\WINDOWS\SysWOW64</summary>
        public static string SystemX86 => Environment.GetFolderPath(Environment.SpecialFolder.SystemX86, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\Users\%username%\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup</summary>
        public static string Startup => Environment.GetFolderPath(Environment.SpecialFolder.Startup, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\Users\%username%\Desktop<br/>Prefer DesktopDirectory instead.</summary>
        public static string Desktop => Environment.GetFolderPath(Environment.SpecialFolder.Desktop, Environment.SpecialFolderOption.DoNotVerify);
    }
}
