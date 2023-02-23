using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    /// <summary>
    /// Tool to handle all path operations.
    /// </summary>
    public static class PathTool
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
}
