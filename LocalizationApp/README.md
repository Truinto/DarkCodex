# LocalizationApp
Console application to merge two string directories into one

How to use
-----------
The app is pretty self explanatory. If you still need help, read this.
* Start the application.
* Provide the path to the new (from the updated mod) and old (what you translated originally) translation files. You can type an absolute or relative path. You can also just Drag&Drop the individual files onto the console.
* (Optional) If you call from console, then the first and second argument are read as new and old file respectively.
* New strings requiring translation are added to a list. You are prompted to either translate them right now by typing the new translation in the console. Or just have the English text copied. You can always just reopen the file and find new entries at the bottom.
* Strings that are no longer present will be archived to a separate json file.
* All new files are saved in your current work directory, usually the app's folder. Filenames are equal to the old input file plus current date and time.

Thanks
-----------
Thanks to Geremy Good for putting the icon into the public domain [Source](https://commons.wikimedia.org/wiki/File:Translation_-_Noun_project_987.svg)
