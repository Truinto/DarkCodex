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

        public static Sprite X()
        {
            //https://www.csharp411.com/embedded-image-resources/
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MyNamespace.SubFolder.MyImage.png");
            var mem = new MemoryStream();
            stream.CopyTo(mem);
            var bytes = mem.ToArray();

            var texture = new Texture2D(64, 64);
            texture.LoadImage(bytes);
            return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0, 0));
        }
    }
}
