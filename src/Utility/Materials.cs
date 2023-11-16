namespace StorageOptions;

public class Materials
{
    public Dictionary<string, string> Elements { get; set; } = new();

    public Materials() { }

    public Materials(Dictionary<string, string> types) => Elements = types;

    public bool Full => Elements.Count != 0;

    public void OutputTranslatedDescription(StringBuilder dsc, CollectibleObject obj = null)
    {
        Dictionary<string, List<string>> langKeys = obj?.Attributes?["langKeys"]?.AsObject<Dictionary<string, List<string>>>();
        {
            foreach ((var key, var val) in Elements)
            {
                if (langKeys != null && langKeys.Count != 0 && langKeys.ContainsKey(key))
                {

                    var translatedText = langKeys.GetValueSafe(key).ToList().ConvertAll(x => Lang.GetMatching(x.Replace($"{{{key}}}", val)));
                    dsc.Append(string.Join("", translatedText));
                }
                else
                {
                    dsc.AppendLine();
                    dsc.Append(key);
                    dsc.Append(": ");
                    dsc.AppendLine(val);
                }
            }
        }
    }

    public void FromTreeAttribute(ITreeAttribute tree)
    {
        ITreeAttribute materialsTree = tree.GetOrAddTreeAttribute("materials");
        foreach (var keyVal in materialsTree.Where(keyVal => !Elements.ContainsKey(keyVal.Key)))
        {
            Elements.Add(keyVal.Key, materialsTree.GetString(keyVal.Key));
        }
    }

    public void ToTreeAttribute(ITreeAttribute tree)
    {
        ITreeAttribute materialsTree = tree.GetOrAddTreeAttribute("materials");
        foreach ((string key, string val) in Elements)
        {
            materialsTree.SetString(key, val);
        }
    }

    public override string ToString()
    {
        if (Elements.Count != 0)
        {
            var val = string.Join("-", Elements);
            return val;
        }
        return string.Empty;
    }
}