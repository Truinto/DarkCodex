using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using Shared;
using static Shared.SerializationTool;

namespace LocalizationApp
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            int i; 
            string text;
            string inputDarkCodex = args.ElementAtOrDefault(0);
            string inputTranslation = args.ElementAtOrDefault(1);

            i = 0;
            while (!File.Exists(inputDarkCodex))
            {
                if (i++ != 0)
                    Console.WriteLine("Could not find file, try again.\n");
                Console.Write("Path to new localization file: ");
                inputDarkCodex = Console.ReadLine();
            }

            i = 0;
            while (!File.Exists(inputTranslation))
            {
                if (i++ != 0)
                    Console.WriteLine("Could not find file, try again.\n");
                Console.Write("Path to your existing localization file: ");
                inputTranslation = Console.ReadLine();
            }

            // get language code
            string language = new FileInfo(inputTranslation).Name;
            language = language.TrySubstring('-') ?? language.TrySubstring('.') ?? language;

            // parse inputs
            var mapDarkCodex = Deserialize<Dictionary<string, string>>(path: inputDarkCodex);
            var mapTranslation = Deserialize<Dictionary<string, string>>(path: inputTranslation);

            // check for new keys
            var keysNew = mapDarkCodex.Keys.Except(mapTranslation.Keys); // keys in mapDarkCodex, but not in mapTranslation
            if (!keysNew.Any())
            {
                Console.Write("\nThere are no new strings. Press any key to close. ");
                Console.ReadKey();
                return;
            }

            // list new keys
            Console.Write($"\nThere are {keysNew.Count()} new strings. Do you want to process in the console now? (y/n) ");
            if (ReadYesNo())
            {
                foreach (var key in keysNew)
                {
                    text = mapDarkCodex[key];
                    Console.Write($"\nstring: {text}\ntranslation: ");
                    text = Console.ReadLine();
                    mapTranslation[key] = text;
                }
            }
            else
            {
                foreach (var key in keysNew)
                    mapTranslation[key] = mapDarkCodex[key];
            }

            // remove obsolete keys
            var keysObsolete = mapTranslation.Keys.Except(mapDarkCodex.Keys); // keys in mapTranslation, but not in mapDarkCodex
            var mapObsolete = new Dictionary<string, string>(50);
            foreach (var key in keysObsolete)
            {
                mapTranslation.Remove(key, out string old);
                mapObsolete[key] = old;
            }
            if (mapObsolete.Count > 0)
            {
                text = new FileInfo($"{language}-obsolete.json").FullName;
                Console.WriteLine($"\n{mapObsolete.Count} strings are obsolete and have been removed. Updating file {text}");

                if (File.Exists(text))
                {
                    var mapAdditions = mapObsolete;
                    mapObsolete = Deserialize<Dictionary<string, string>>(text);
                    foreach (var item in mapAdditions)
                        mapObsolete[item.Key] = item.Value;
                }
                mapObsolete.Serialize(path: text);
            }

            text = new FileInfo($"{language}{DateTime.Now:-yyyy-MM-dd-HH.mm}.json").FullName;
            Console.WriteLine($"\nSaving new file {text}");
            mapTranslation.Serialize(path: text);

            Console.Write("\nAll done. Press any key to close. ");
            Console.ReadKey();
        }

        public static bool ReadYesNo()
        {
            while (true)
            {
                var button = Console.ReadKey(true);
                if (button.Key == ConsoleKey.Y)
                {
                    Console.WriteLine("y");
                    return true;
                }
                if (button.Key == ConsoleKey.N)
                {
                    Console.WriteLine("n");
                    return false;
                }
            }
        }
    }
}