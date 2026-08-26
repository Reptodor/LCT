public sealed class FoodItem
{
    public readonly string Title;
    public readonly string Hint;
    public readonly int Cost;
    public readonly int Hunger;

    public FoodItem(string title, string hint, int cost, int hunger)
    {
        Title = title;
        Hint = hint;
        Cost = cost;
        Hunger = hunger;
    }
}

public static class FoodCatalog
{
    public static readonly FoodItem[] All =
    {
        new FoodItem("Яблоко", "Полезно и недорого", 8, 12),
        new FoodItem("Бутерброд", "Сытный перекус на ходу", 15, 22),
        new FoodItem("Конфеты", "Дёшево, но почти не кормит", 5, 4),
        new FoodItem("Суп", "Горячее и сытное", 20, 30),
        new FoodItem("Обед", "Полноценная еда", 28, 42)
    };
}
