using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ionic.Zip;

namespace BlueprintPurge
{
    public partial class BlueprintPurge : Form
    {
        private string pathSave;
        private ZipFile zip;
        private HashSet<Guid> blueprints = new();
        private List<PurgeRange> purges = new();

        public BlueprintPurge()
        {
            InitializeComponent();

            radioWhitelist.Enabled = false; // todo: implement whitelist mode -> add type check
        }

        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            path = path.Substring(0, path.LastIndexOf("Roaming")) + "LocalLow";

            openFileDialogSave.InitialDirectory = Path.Combine(path, "Owlcat Games", "Pathfinder Wrath Of The Righteous", "Saved Games");
            if (openFileDialogSave.ShowDialog(this) == DialogResult.OK)
                textBoxSavePath.Text = openFileDialogSave.FileName;
        }

        private void ButtonSearch_Click(object sender, EventArgs e)
        {
            try
            {
                Clear();

                // check target save exists
                pathSave = textBoxSavePath.Text;
                if (pathSave == null || pathSave == "" || !File.Exists(pathSave))
                {
                    MessageBox.Show("File does not exist", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // read blueprints; or if empty, read from file
                string bps = textBoxBlueprints.Text;
                if (bps == "")
                {
                    if (openFileDialogBlueprints.ShowDialog(this) != DialogResult.OK)
                        return;
                    if (!File.Exists(openFileDialogBlueprints.FileName))
                        return;
                    bps = File.ReadAllText(openFileDialogBlueprints.FileName);
                }

                //parse blueprints
                foreach (var bp in bps.Split('\n', '\t', ' ', ';', ','))
                    if (Guid.TryParse(bp, out var guid))
                        blueprints.Add(guid);
                if (blueprints.Count == 0)
                {
                    MessageBox.Show("Could not parse any blueprints", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // open save file
                zip = ZipFile.Read(pathSave);
                foreach (var entry in zip.Entries.ToArray())
                {
                    if (!entry.FileName.EndsWith(".json"))
                        continue;

                    Debug.WriteLine(entry.FileName);
                    Search(entry);
                }
                Cleanup();
                //zip.Save(pathSave + ".TEST.zks");
                buttonPurge.Enabled = true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Search(ZipEntry entry)
        {
            var sw = new MemoryStream();
            entry.Extract(sw);
            byte[] data = sw.ToArray();
            Debug.Print(data.Length.ToString());

            char last = default;
            char last2 = default;
            bool isQuote = false;
            bool isId = false;
            string lastId = "";
            bool isRef = false;
            var refs = new HashSet<string>();
            var sb = new StringBuilder();
            var stack = new Stack<int>();
            int depth = 0;
            var purge = new Stack<PurgeRange>();
            for (int i = 0; i < data.Length; i++)
            {
                char c = (char)data[i];

                if (last == '\\' && last2 != '\\') // filter escape sequence
                {
                    goto end;
                }
                if (c == '\"') // detect quotes
                {
                    isQuote = !isQuote;
                    if (!isQuote) // if end of quote
                    {
                        string quote = sb.ToString();

                        if (isId)
                            lastId = quote;

                        if (isRef && refs.Contains(quote))
                        {
                            var x = purges.First(f => f.Ref == quote);
                            purge.Push(new PurgeRange { File = x.File, Guid = x.Guid, Data = data, Ref = quote });
                        }

                        isId = quote == "$id";
                        isRef = quote == "$ref";

                        if (Guid.TryParse(quote, out var guid)
                           && this.blueprints.Contains(guid) ^ this.radioWhitelist.Checked)
                        {
                            purge.Push(new PurgeRange { File = entry.FileName, Guid = guid, Data = data, Ref = lastId });
                            refs.Add(lastId);
                            depth = stack.Count;
                        }
                        sb.Clear();
                    }
                    goto end;
                }
                if (isQuote) // remember quote
                {
                    sb.Append(c);
                    goto end;
                }

                if (c == '{')
                {
                    stack.Push(i);
                }
                else if (c == '}')
                {
                    if (purge.Count > 0 && stack.Count == depth)
                    {
                        var p = purge.Pop();
                        p.Start = stack.Pop();
                        p.End = i;
                        p.Peek = Encoding.Default.GetString(data[p.Start..(p.End + 1)]);
                        purges.Add(p);
                    }
                    else
                        stack.Pop();
                }

            end:
                last2 = last;
                last = c;
            }
        }

        private Regex rxType = new("([A-z\\.]*), Assembly-CSharp\"");
        private Regex rxId = new("\"\\$id\"[\\s:]*\"([0-9]+)\"");
        private void Cleanup()
        {
            int lastStart = 0;
            int lastEnd = 0;
            for (int i = purges.Count - 1; i >= 0; i--)
            {
                var purge = purges[i];

                // remove entries that are contained by its predecessor
                if (i > 0)
                {
                    lastStart = purges[i - 1].Start;
                    lastEnd = purges[i - 1].End;
                    if (purge.Start <= lastStart && lastEnd <= purge.End)
                    {
                        purges.RemoveAt(i - 1);
                        continue;
                    }
                }

                // should replace with null instead
                for (int j = purge.Start - 1; j >= 0; j--)
                {
                    char c = (char)purge.Data[j];
                    if (char.IsWhiteSpace(c))
                        continue;
                    if (c == ':')
                        purge.NullReplace = true;
                    break;
                }

                // include tailing comma
                for (int j = purge.End + 1; j < purge.Data.Length; j++)
                {
                    char c = (char)purge.Data[j];
                    if (char.IsWhiteSpace(c))
                        continue;
                    if (c == ',')
                        purge.End = j;
                    break;
                }

                // parse Type
                var match = rxType.Match(purge.Peek);
                if (match.Success)
                    purge.Type = match.Groups[1].Value;

                // parse id
                var match2 = rxId.Match(purge.Peek);
                if (match2.Success)
                    purge.Ref = match.Groups[1].Value;

                purge.Enabled = true;
            }

            // TODO: remove refs; sanity json check
        }

        private void Clear()
        {
            zip?.Dispose();
            zip = null;
            blueprints.Clear();
            purges.Clear();
            buttonPurge.Enabled = false;
        }

        private Regex rxHeader = new("(\"AreaNameOverride\":).*?([,\\{\\[\\]\\}])");
        private void UpdateHeader()
        {
            var sw = new MemoryStream();
            zip["header.json"].Extract(sw);

            string header = Encoding.Default.GetString(sw.ToArray());
            header = rxHeader.Replace(header, "$1\"PURGED!\"$2");
            zip.UpdateEntry("header.json", header);
        }

        private void PurgeRefs(string file, byte[] data)
        {
            var list = new List<string>();
            foreach (var purge in purges.Where(w => w.File == file && w.Enabled && w.Ref != null))
                list.Add(purge.Ref);

            for (int i = 0; i < data.Length; i++)
            {
                // TODO:
                // {"$ref": "13"}
            }
        }

        private string Get(byte[] data, int start)
        {
            for (int i = 0; i < data.Length; i++)
            {
                // TODO:
                // {"$ref": "13"}
            }
            return null;
        }

        private void ButtonPurge_Click(object sender, EventArgs e)
        {
            if (zip == null)
                return;

            // process purge entries
            var edited = new Dictionary<string, byte[]>();
            foreach (var purge in purges)
            {
                if (!purge.Enabled)
                    continue;

                edited[purge.File] = purge.Data;
                for (int i = purge.Start; i <= purge.End; i++)
                    purge.Data[i] = (byte)' ';

                if (purge.NullReplace)
                {
                    int i = purge.Start;
                    purge.Data[i++] = (byte)'n';
                    purge.Data[i++] = (byte)'u';
                    purge.Data[i++] = (byte)'l';
                    purge.Data[i++] = (byte)'l';
                }
            }

            // save changes and update header
            if (edited.Count > 0)
            {
                foreach (var (file, data) in edited)
                {
                    PurgeRefs(file, data);
                    zip.UpdateEntry(file, data);
                }

                UpdateHeader();
                zip.Save(pathSave + ".purged.zks");
                MessageBox.Show($"Removed {edited.Count} entries. Saved in new file.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No match found. Canceled.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Clear();
        }

        private void ButtonHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"Warning: This is just a test version. It may create invalid save data which could immediately or during your playthrough fail.\n"
                + "This app will never override your original save. It will instead create a duplicate with the description 'PURGED!'.\n"
                + "How to use: Enter file path to save data you want to purge. List blueprint guids you want to purge from your save. Click 'Search'. Click 'Purge Now!'.\n"
                + "The grey box has currently no function. In the future you will see a preview of the purges there.",

                "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
