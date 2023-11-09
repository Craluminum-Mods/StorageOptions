namespace StorageOptions;

public class Materials
{
    public string Wood { get; set; }

    public Materials()
    {
    }

    public Materials(string wood)
    {
        Wood = wood;
    }

    public bool Full => !string.IsNullOrEmpty(Wood);

    public void OutputTranslatedDescription(StringBuilder dsc)
    {
        dsc.Append(Lang.Get("blockmaterial-Wood"));
        dsc.Append(": ");
        dsc.AppendLine(Lang.Get($"material-{Wood}"));
        dsc.AppendLine();
    }

    public override string ToString()
    {
        return Wood;
    }
}