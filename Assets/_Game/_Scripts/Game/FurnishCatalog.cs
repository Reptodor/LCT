using System.Collections.Generic;

public sealed class FurnishSection
{
    public readonly string RoomId;
    public readonly string Title;
    public readonly ShopItem[] Items;

    public FurnishSection(string roomId, string title, ShopItem[] items)
    {
        RoomId = roomId;
        Title = title;
        Items = items ?? new ShopItem[0];
    }
}

public static class FurnishCatalog
{
    public static FurnishSection[] Sections => ShopCatalog.FurnitureRooms();
}
