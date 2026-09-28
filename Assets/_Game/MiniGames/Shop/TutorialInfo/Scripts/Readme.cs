using System;
using UnityEngine;

namespace LCT.MiniGames.Shop
{
    public class Readme : ScriptableObject
    {
        public Texture2D icon;
        public string title;
        public Section[] sections;
        public bool loadedLayout;

        [Serializable]
        public class Section
        {
            public string heading, text, linkText, url;
        }
    }
}
