using System.Collections.Generic;

public enum ShopCategory
{
    Required,
    Optional
}

public sealed class ShopItem
{
    public readonly string Id;
    public readonly string Title;
    public readonly int Cost;
    public readonly int Effect;
    public readonly ShopCategory Category;
    public readonly string RoomId;
    public readonly string RoomTitle;

    public ShopItem(string id, string title, int cost, int effect, ShopCategory category, string roomId = "", string roomTitle = "")
    {
        Id = id;
        Title = title;
        Cost = cost;
        Effect = effect;
        Category = category;
        RoomId = roomId ?? "";
        RoomTitle = roomTitle ?? "";
    }

    public string PriceText => Cost + " монет";

    public string EffectText => Category == ShopCategory.Required
        ? "+" + Effect + " насыщенности"
        : "+" + Effect + " настроения";

    public string CategoryText => Category == ShopCategory.Required
        ? "Обязательная покупка"
        : "Необязательная покупка";
}

public static class ShopCatalog
{
    public static readonly ShopItem[] Food =
    {
        new ShopItem("apple", "Яблоко", 8, 12, ShopCategory.Required),
        new ShopItem("sandwich", "Бутерброд", 15, 22, ShopCategory.Required),
        new ShopItem("candy", "Конфеты", 5, 4, ShopCategory.Required),
        new ShopItem("soup", "Суп", 20, 30, ShopCategory.Required),
        new ShopItem("lunch", "Обед", 28, 42, ShopCategory.Required)
    };

    public static readonly ShopItem[] Furniture =
    {
        new ShopItem("Bed", "Кровать", 120, 4, ShopCategory.Optional, "BedRoom", "Спальня"),
        new ShopItem("ComputerDesk", "Компьютерный стол", 150, 5, ShopCategory.Optional, "BedRoom", "Спальня"),
        new ShopItem("Sofa", "Диван", 100, 3, ShopCategory.Optional, "MainRoom", "Гостиная"),
        new ShopItem("LoungeChair", "Кресло", 70, 2, ShopCategory.Optional, "MainRoom", "Гостиная"),
        new ShopItem("CoffeeTable", "Журнальный стол", 40, 1, ShopCategory.Optional, "MainRoom", "Гостиная"),
        new ShopItem("Television", "Телевизор", 180, 6, ShopCategory.Optional, "MainRoom", "Гостиная"),
        new ShopItem("Bookcase", "Книжный шкаф", 90, 2, ShopCategory.Optional, "MainRoom", "Гостиная"),
        new ShopItem("DiningSet", "Обеденный стол", 80, 2, ShopCategory.Optional, "Kitchen", "Кухня"),
        new ShopItem("KitchenCounter", "Кухонный гарнитур", 140, 4, ShopCategory.Optional, "Kitchen", "Кухня"),
        new ShopItem("Fridge", "Холодильник", 110, 3, ShopCategory.Optional, "Kitchen", "Кухня"),
        new ShopItem("Bathtub", "Ванна", 100, 3, ShopCategory.Optional, "Bath", "Ванная"),
        new ShopItem("Toilet", "Унитаз", 50, 1, ShopCategory.Optional, "Bath", "Ванная"),
        new ShopItem("Vanity", "Раковина", 75, 2, ShopCategory.Optional, "Bath", "Ванная")
    };

    public static bool TryFind(string id, out ShopItem item)
    {
        item = null;
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        for (int i = 0; i < Furniture.Length; i++)
        {
            if (Furniture[i].Id == id)
            {
                item = Furniture[i];
                return true;
            }
        }

        for (int i = 0; i < Food.Length; i++)
        {
            if (Food[i].Id == id)
            {
                item = Food[i];
                return true;
            }
        }

        return false;
    }

    public static FurnishSection[] FurnitureRooms()
    {
        var order = new List<string>();
        var titles = new List<string>();
        for (int i = 0; i < Furniture.Length; i++)
        {
            string roomId = Furniture[i].RoomId;
            if (order.Contains(roomId))
            {
                continue;
            }

            order.Add(roomId);
            titles.Add(Furniture[i].RoomTitle);
        }

        var built = new FurnishSection[order.Count];
        for (int r = 0; r < order.Count; r++)
        {
            var items = new List<ShopItem>();
            for (int i = 0; i < Furniture.Length; i++)
            {
                if (Furniture[i].RoomId == order[r])
                {
                    items.Add(Furniture[i]);
                }
            }

            built[r] = new FurnishSection(order[r], titles[r], items.ToArray());
        }

        return built;
    }
}
