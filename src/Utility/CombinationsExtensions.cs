namespace StorageOptions;

public static class CombinationsExtensions
{
    public static List<JsonItemStack> CreateJsonItemStacks(this CollectibleObject obj, ICoreAPI api, List<Dictionary<string, string>> combinations)
    {
        List<JsonItemStack> stacks = new();

        foreach (Dictionary<string, string> combination in combinations)
        {
            JsonObject json = new JsonObject(new JObject());
            json.Token["materials"] = JToken.FromObject(combination);
            JsonItemStack jstack = api.CreateJStack(obj, json.ToString());
            stacks.Add(jstack);
        }

        return stacks;
    }

    public static List<Dictionary<string, string>> GetCombinationsContainingAllKeys(this Dictionary<string, List<string>> materials)
    {
        var combinations = GenerateCombinations(materials);
        var finalResult = new List<Dictionary<string, string>>();

        foreach (List<string> result in combinations)
        {
            finalResult.Add(new());

            int materialIndex = 0;

            foreach ((string material, _) in materials)
            {
                finalResult.Last().Add(material, result[materialIndex]);
                materialIndex++;
            }
        }

        return finalResult;
    }

    public static List<List<string>> GenerateCombinations(this Dictionary<string, List<string>> materials)
    {
        var results = new List<List<string>>();
        CombineMaterials(materials, new List<string>(), new List<string>(materials.Keys), 0, results);
        return results;
    }

    public static void CombineMaterials(Dictionary<string, List<string>> materials, List<string> current, List<string> keys, int index, List<List<string>> results)
    {
        if (index == keys.Count)
        {
            results.Add(new List<string>(current));
            return;
        }

        string key = keys[index];
        foreach (string item in materials[key])
        {
            current.Add(item);
            CombineMaterials(materials, current, keys, index + 1, results);
            current.RemoveAt(current.Count - 1);
        }
    }
}