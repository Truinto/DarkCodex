# CodexLib
A shared library

How to add to your project
-----------
* Add a reference to your project.
* Important! Set local copy to false. Manual copy CodexLib.dll into your mod folder. This way it will trigger AssemblyResolve while load.
* Important! Before patching blueprints set the scope to your project. This is where guids will be resolved. If you use CodexLib's components, you should also run MasterPatch.
```
using var scope = new Scope(Main.ModPath, Main.logger);
MasterPatch.Run();
```
* If your mod does not require other mods that already implement CodexLib, add this event to your EntryMethod:
```
AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

[...]

private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
{
    try
    {
        PrintDebug("Requested " + args.Name);

        if (ModPath != null && args.Name.StartsWith("CodexLib, "))
        {
            string path = null;
            Version version = null;

            foreach (string cPath in Directory.GetFiles(Directory.GetParent(ModPath).FullName, "CodexLib.dll"))
            {
                var cVersion = new Version(FileVersionInfo.GetVersionInfo(cPath).FileVersion);
                if (version == null || cVersion > version)
                {
                    path = cPath;
                    version = cVersion;
                }
            }

            if (path != null)
            {
                Print("AssemblyResolve " + path);
                return Assembly.LoadFrom(path);
            }
        }
    }
    catch (Exception ex) { logger?.LogException(ex); }
    return null;
}
```

Things to note
-----------
* This assembly will only be loaded once.
* Only the most recent assembly should be loaded.
* Method signatures should not changed during versions. If there is a major change, you will need to recompile your project.
* PS: I plan to rework or remove the ToRef methods.
