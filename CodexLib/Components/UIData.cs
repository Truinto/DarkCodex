using Kingmaker.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CodexLib
{
    //UIDataProvider
    public class UIData : IUIDataProvider
    {
        public UIData()
        {
        }
        
        public UIData(string name, string description, Sprite icon = null, string nameForAcronym = null)
        {
            Name = name;
            Description = description;
            Icon = icon;
            NameForAcronym = nameForAcronym;
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public Sprite Icon { get; set; }

        public string NameForAcronym { get; set; }
    }
}
