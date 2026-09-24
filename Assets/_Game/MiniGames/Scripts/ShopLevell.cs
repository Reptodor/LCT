using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ShopLevel
{
    public string shopName;
    public Sprite shopIcon;
    public string parentName;
    public string taskDescription;
    public int budget;
    public List<ProductData> products;
}