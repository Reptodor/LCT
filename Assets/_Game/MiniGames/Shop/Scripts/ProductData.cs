using UnityEngine;


namespace LCT.MiniGames.Shop
{
    [System.Serializable]
    public class ProductData
    {
        public int id;
        public string productName;
        public int price;
        public Sprite icon;
        public bool isRequired;
    }
}
