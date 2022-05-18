# CodexShared
A shared code project with basic implementation for 

How to add to your project
-----------
* Create a reference to your project.
* Add a new partial class "Main" to your project.
* Change the namespace to "Shared".
* Open your info.json and change EntryMethod to "Shared.Main.Load".
* Implement partial methods: OnLoad, OnMainMenu, OnBlueprintsLoaded.

How to save settings
-----------
* Create a new class derived of BaseSettings<T> (where T is your new class)
* Add a static instance to your class `public static Settings State = TryLoad(Main.ModPath, "settings.json");`
* When you want to save changes call "TrySave()"
* You can use version control by setting version in the constructor `public Settings() => version = 4;`
When the version changes OnUpdate() will be called. Return true, if you want to save.
