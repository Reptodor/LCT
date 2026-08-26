public enum NeedWantKind
{
    Need = 0,
    Want = 1
}

public sealed class NeedWantItem
{
    public readonly string Title;
    public readonly string Prompt;
    public readonly string Why;
    public readonly NeedWantKind Kind;

    public NeedWantItem(string title, string prompt, string why, NeedWantKind kind)
    {
        Title = title;
        Prompt = prompt;
        Why = why;
        Kind = kind;
    }
}
