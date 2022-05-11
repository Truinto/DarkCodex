using Kingmaker.Blueprints;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;

namespace CodexLib
{
    public class GuidManager
    {
        public static GuidManager i = new();

#if DEBUG
        public bool allow_guid_generation = true;
#else
		public bool allow_guid_generation = false;
#endif

        public string filepath = Path.Combine(Main.ModPath, "blueprints.txt");
        public string filepath2 = Path.Combine(Main.ModPath, "blueprints_dynamic.txt");
        public Dictionary<string, string> guid_list = new();
        public HashSet<string> guid_dynamic = new();
        public List<string> register = new();

        private bool loaded = false;
        public void TryLoad()
        {
            if (loaded) return;
            else loaded = true;

            try
            {
                if (File.Exists(filepath))
                {
                    string[] lines = File.ReadAllLines(filepath);
                    foreach (string line in lines)
                    {
                        string[] items = line.Split('\t');
                        if (items.Length >= 2)
                            guid_list[items[0]] = items[1];
                    }
                }

                if (File.Exists(filepath2))
                {
                    string[] lines = File.ReadAllLines(filepath2);
                    foreach (string line in lines)
                    {
                        string[] items = line.Split('\t');
                        if (items.Length >= 2)
                            guid_dynamic.Add(items[1]);
                    }
                }
            }
            catch (Exception e)
            {
                Helper.PrintException(e);
            }
        }

        private void Write(string key, string guid)
        {
            try
            {
                using StreamWriter writer = new(filepath, append: true);
                writer.WriteLine(key + '\t' + guid);
            }
            catch (Exception e)
            {
                Helper.PrintException(e);
            }
        }

        ///<summary>Used to dump all guids to file. Use once when needed.</summary>
        public void WriteAll()
        {
            if (!allow_guid_generation) return;
            TryLoad();

            using StreamWriter writer = new(filepath, append: false);

            foreach (KeyValuePair<string, string> pair in guid_list)
            {
                SimpleBlueprint obj = null;
                try { obj = ResourcesLibrary.TryGetBlueprint<BlueprintScriptableObject>(pair.Value); } catch (Exception e) { Helper.Print("WriteAll guid_list: " + e.Message); }
                if (obj != null)
                {
                    writer.WriteLine(pair.Key + '\t' + pair.Value + '\t' + obj.GetType().FullName);
                    if (pair.Key != obj.name) Helper.Print(pair.Key + " != " + obj.name);
                }
                else
                {
                    Helper.Print(pair.Value + " does not exist");
                    writer.WriteLine(pair.Key + '\t' + pair.Value + '\t' + "NULL");
                }
            }

            foreach (string guid in register)
            {
                if (guid_list.ContainsValue(guid))
                    continue;

                BlueprintScriptableObject obj = null;
                try { obj = ResourcesLibrary.TryGetBlueprint<BlueprintScriptableObject>(guid); } catch (Exception e) { Helper.Print("WriteAll register: " + e.Message); }
                if (obj != null)
                {
                    writer.WriteLine(obj.name + '\t' + guid + '\t' + obj.GetType().FullName);
                }
                else
                {
                    Helper.Print(guid + " does not exist");
                    writer.WriteLine("UNKNOWN" + '\t' + guid + '\t' + "NULL");
                }
            }
        }

        ///<summary>When you already have a guid, but want it dumped.</summary>
        public string Reg(string guid)
        {
            if (allow_guid_generation)
                register.Add(guid);
            return guid;
        }

        ///<summary>Gets or makes a new guid.</summary>
        ///<key="key">Blueprint.name</key>
        public string Get(string key)
        {
            TryLoad();

            string result;
            guid_list.TryGetValue(key, out result);

            if (result == null)
            {
                if (!allow_guid_generation)
                    throw new Exception("Tried to generate a new GUID while not allowed! " + key);

                Helper.Print("Warning: Generating new GUID for " + key);
                result = Guid.NewGuid().ToString("N");
                guid_list[key] = result;
                Write(key, result);
            }

            return result;
        }

        /// <summary>Used for guid that are allowed to be generated on the spot.</summary>
        public void AddDynamic(string name, string guid)
        {
            TryLoad();

            if (guid_dynamic.Add(guid))
            {
                try
                {
                    using StreamWriter writer = new(filepath2, append: true);
                    writer.WriteLine(name + "\t" + guid);
                }
                catch (Exception e)
                {
                    Helper.PrintException(e);
                }
            }
        }
    }
}
