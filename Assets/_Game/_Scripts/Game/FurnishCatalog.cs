public sealed class FurnishEntry
{
    public readonly string Id;
    public readonly string Title;

    public FurnishEntry(string id, string title)
    {
        Id = id;
        Title = title;
    }
}

public sealed class FurnishSection
{
    public readonly string RoomId;
    public readonly string Title;
    public readonly FurnishEntry[] Items;

    public FurnishSection(string roomId, string title, FurnishEntry[] items)
    {
        RoomId = roomId;
        Title = title;
        Items = items;
    }
}

public static class FurnishCatalog
{
    public static readonly FurnishSection[] Sections =
    {
        new FurnishSection("BedRoom", "Спальня", new[]
        {
            new FurnishEntry("Bed", "Кровать"),
            new FurnishEntry("ComputerDesk", "Компьютерный стол")
        }),
        new FurnishSection("MainRoom", "Гостиная", new[]
        {
            new FurnishEntry("Sofa", "Диван"),
            new FurnishEntry("LoungeChair", "Кресло"),
            new FurnishEntry("CoffeeTable", "Журнальный стол"),
            new FurnishEntry("Television", "Телевизор"),
            new FurnishEntry("Bookcase", "Книжный шкаф")
        }),
        new FurnishSection("Kitchen", "Кухня", new[]
        {
            new FurnishEntry("DiningSet", "Обеденный стол"),
            new FurnishEntry("KitchenCounter", "Кухонный гарнитур"),
            new FurnishEntry("Fridge", "Холодильник")
        }),
        new FurnishSection("Bath", "Ванная", new[]
        {
            new FurnishEntry("Bathtub", "Ванна"),
            new FurnishEntry("Toilet", "Унитаз"),
            new FurnishEntry("Vanity", "Раковина")
        })
    };
}
