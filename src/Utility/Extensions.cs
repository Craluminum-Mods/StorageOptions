namespace StorageOptions;

public static class Extensions
{
    public static T LoadAsset<T>(this ICoreAPI api, string path) => api.Assets.Get(new AssetLocation(path)).ToObject<T>();

    public static void EnsureAttributesNotNull(this CollectibleObject obj) => obj.Attributes ??= new JsonObject(new JObject());

    public static void SetAttribute(this CollectibleObject obj, string key, object val)
    {
        if (val != null) obj.Attributes.Token[key] = JToken.FromObject(val);
    }

    public static bool IsGroundRackable(this ItemSlot slot) => slot?.Itemstack?.Collectible?.Attributes?[GroundRackable].AsBool() == true;
    public static bool IsGroundRackable(this CollectibleObject obj) => obj?.Attributes?[GroundRackable].AsBool() == true;

    public static bool IsShelvableOne(this ItemSlot slot) => slot?.Itemstack?.Collectible?.Attributes?[ShelvableOne].AsBool() == true;
    public static bool IsShelvableOne(this CollectibleObject obj) => obj?.Attributes?[ShelvableOne].AsBool() == true;

    public static ModelTransform GetTransform(this CollectibleObject obj, Dictionary<string, ModelTransform> transforms)
    {
        foreach (KeyValuePair<string, ModelTransform> _transform in transforms)
        {
            if (WildcardUtil.Match(_transform.Key, obj.Code.ToString()))
            {
                return _transform.Value;
            }
        }

        return null;
    }

    public static void AddToCreativeInv(this CollectibleObject obj, string tab)
    {
        if (obj.CreativeInventoryTabs != null && obj.CreativeInventoryTabs.Length != 0 && !string.IsNullOrEmpty(obj?.CreativeInventoryTabs?[0]))
        {
            obj.CreativeInventoryTabs = obj.CreativeInventoryTabs.Append(tab).ToArray();
        }
    }

    public static WorldInteraction[] GetOrCreateToolrackInteractions(this ICoreClientAPI capi, string key, EnumStorageOption option)
    {
        return ObjectCacheUtil.GetOrCreate(capi, key, delegate
        {
            List<ItemStack> list = capi.CollectStacksForInteraction(option);
            return new WorldInteraction[2]
            {
                new()
                {
                    ActionLangCode = "blockhelp-toolrack-place",
                    MouseButton = EnumMouseButton.Right,
                    Itemstacks = list.ToArray()
                },
                new()
                {
                    ActionLangCode = "blockhelp-toolrack-take",
                    MouseButton = EnumMouseButton.Right
                }
            };
        });
    }

    public static List<ItemStack> CollectStacksForInteraction(this ICoreClientAPI capi, EnumStorageOption option)
    {
        List<ItemStack> list = new();
        foreach (CollectibleObject obj in capi.World.Collectibles)
        {
            if ((option is EnumStorageOption.GroundRackable && !obj.IsGroundRackable())
                || (option is EnumStorageOption.ShelvableOne && !obj.IsShelvableOne()))
            {
                continue;
            }

            List<ItemStack> handBookStacks = obj.GetHandBookStacks(capi);
            if (handBookStacks != null)
            {
                list.AddRange(handBookStacks);
            }
        }

        return list;
    }

    public static string ConstructName(this CollectibleObject obj)
    {
        if (!obj.Attributes.KeyExists("customName"))
        {
            return obj.Code.ToString();
        }

        string main = obj.Attributes["customName"]["main"].AsString();

        StringBuilder sb = new();
        sb.Append(Lang.GetMatchingIfExists(main));

        if (!obj.Attributes["customName"].KeyExists("parts"))
        {
            return sb.ToString();
        }

        List<string> parts = obj.Attributes["customName"]["parts"].AsObject<List<string>>();
        foreach (string part in parts)
        {
            sb.Append(Lang.GetMatchingIfExists(part) ?? part);
        }

        return sb.ToString();
    }
}