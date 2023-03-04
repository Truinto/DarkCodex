using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    /// <summary>
    /// Tool to handle path operations.
    /// </summary>
    public class PathTool
    {
        private static void Sandbox()
        {
            Directory.Delete("");
            Directory.Exists("");
            Directory.CreateDirectory("");

            File.Delete("");
            File.Exists("");
            File.CreateText("");

            Path.ChangeExtension("", "");
            Path.Combine("", "");
            Path.GetDirectoryName(""); // warning, this is inconsistent
            Path.GetExtension("");
            Path.GetFileName("");
            Path.GetFileNameWithoutExtension("");
            Path.GetFullPath("");
            Path.GetInvalidFileNameChars();
            Path.GetPathRoot("");
            Path.GetRandomFileName();
            Path.GetTempFileName();
            Path.GetTempPath();
            Path.HasExtension("");
            Path.IsPathRooted("");

            var di = new DirectoryInfo("");
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

            var fi = new FileInfo("");
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
