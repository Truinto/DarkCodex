using Kingmaker.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CodexLib
{
    public class UINumber : IUIDataProvider
    {
        public int Value;

        public string Name { get; set; }
        public string Description { get; set; }
        public Sprite Icon { get => icon ??= GetSprite(Value); }
        public string NameForAcronym { get; set; }

        private Sprite icon;

        private static Sprite GetSprite(int num)
        {
            try
            {
                //https://www.csharp411.com/embedded-image-resources/
                var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"CodexLib.Resources.Num{num}.png");
                var mem = new MemoryStream();
                stream.CopyTo(mem);
                var bytes = mem.ToArray();

                var texture = new Texture2D(64, 64);
                texture.LoadImage(bytes);
                return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0, 0));
            }
            catch (Exception e)
            {
                Helper.PrintDebug(e.ToString());
                return null;
            }
        }

        public static UINumber Get(int num)
        {
            return Numbers[num.MinMax(0, Numbers.Length - 1)];
        }

        public static UINumber[] Numbers = new UINumber[]
        {
            new UINumber() { Value = 0, Name = "Level 0", Description = "" },
            new UINumber() { Value = 1, Name = "Level 1", Description = "" },
            new UINumber() { Value = 2, Name = "Level 2", Description = "" },
            new UINumber() { Value = 3, Name = "Level 3", Description = "" },
            new UINumber() { Value = 4, Name = "Level 4", Description = "" },
            new UINumber() { Value = 5, Name = "Level 5", Description = "" },
            new UINumber() { Value = 6, Name = "Level 6", Description = "" },
            new UINumber() { Value = 7, Name = "Level 7", Description = "" },
            new UINumber() { Value = 8, Name = "Level 8", Description = "" },
            new UINumber() { Value = 9, Name = "Level 9", Description = "" },
        };
    }
}
