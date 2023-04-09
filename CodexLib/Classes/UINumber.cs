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
        public Sprite Icon { get; set; }
        public string NameForAcronym { get; set; }

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
            new UINumber() { Value = 0, Name = "0", Description = "", Icon = GetSprite(0) },
            new UINumber() { Value = 1, Name = "1", Description = "", Icon = GetSprite(1) },
            new UINumber() { Value = 2, Name = "2", Description = "", Icon = GetSprite(2) },
            new UINumber() { Value = 3, Name = "3", Description = "", Icon = GetSprite(3) },
            new UINumber() { Value = 4, Name = "4", Description = "", Icon = GetSprite(4) },
            new UINumber() { Value = 5, Name = "5", Description = "", Icon = GetSprite(5) },
            new UINumber() { Value = 6, Name = "6", Description = "", Icon = GetSprite(6) },
            new UINumber() { Value = 7, Name = "7", Description = "", Icon = GetSprite(7) },
            new UINumber() { Value = 8, Name = "8", Description = "", Icon = GetSprite(8) },
            new UINumber() { Value = 9, Name = "9", Description = "", Icon = GetSprite(9) },
        };
    }
}
