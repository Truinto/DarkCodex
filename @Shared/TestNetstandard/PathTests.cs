using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared;
using Shared.Paths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace TestNetstandard
{
    [TestClass]
    public class PathTests
    {
        [TestMethod]
        public void Tests()
        {
            (string path, string root, string directory, string fileNameNoExtension, string extension)[] test_strings = [
                (@"", "", "", "", ""),

                (@"C:/Folder/", "C:/", "C:/Folder/", "", ""),
                (@"C:\.Folder/", "C:/", "C:/.Folder/", "", ""),

                (@"C:\Folder\File", "C:/", "C:/Folder/", "File", ""),
                (@"C:\Folder\File.ext", "C:/", "C:/Folder/", "File", "ext"),
                (@"C:\.Folder\.File", "C:/", "C:/.Folder/", ".File", ""),
                (@"C:\.Folder\.File.ext", "C:/", "C:/.Folder/", ".File", "ext"),

                (@"File", "", "", "File", ""),
                (@"File.ext", "", "", "File", "ext"),
                (@".File", "", "", ".File", ""),
                (@".File.ext", "", "", ".File", "ext"),

                (@"\File", "/", "/", "File", ""),
                (@"\File.ext", "/", "/", "File", "ext"),
                (@"\.File", "/", "/", ".File", ""),
                (@"\.File.ext", "/", "/", ".File", "ext"),

                (@"..\File", "", "../", "File", ""),
                (@"..\File.ext", "", "../", "File", "ext"),
                (@"..\.File", "", "../", ".File", ""),
                (@"..\.File.ext", "", "../", ".File", "ext"),
            ];

            char c1 = PathInfo.PathSeparator == '/' ? '\\' : '/';
            char c2 = PathInfo.PathSeparator;

            foreach (var (path, root, directory, fileNameNoExtension, extension) in test_strings)
            {
                var p = new PathInfo(path);
                Assert.IsTrue(p.Root == root.Replace(c1, c2));
                Assert.IsTrue(p.Directory == directory.Replace(c1, c2));
                Assert.IsTrue(p.FileNameNoExtension == fileNameNoExtension.Replace(c1, c2));
                Assert.IsTrue(p.Extension == extension.Replace(c1, c2));
            }
        }
    }
}