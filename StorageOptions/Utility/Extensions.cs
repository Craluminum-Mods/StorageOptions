namespace StorageOptions;

public static class Extensions
{
    public static void EnsureAttributesNotNull(this CollectibleObject obj) => obj.Attributes ??= new JsonObject(new JObject());

    public static void SetAttribute(this CollectibleObject obj, string key, object val)
    {
        if (val != null) obj.Attributes.Token[key] = JToken.FromObject(val);
    }

    public static bool IsGroundRackable(this ItemSlot slot) => slot?.Itemstack?.ItemAttributes?.IsTrue(GroundRackable) == true;
    public static bool IsGroundRackable(this CollectibleObject obj) => obj?.Attributes?.IsTrue(GroundRackable) == true;

    public static bool IsShelvableOne(this ItemSlot slot) => slot?.Itemstack?.ItemAttributes?.IsTrue(ShelvableOne) == true;
    public static bool IsShelvableOne(this CollectibleObject obj) => obj?.Attributes?.IsTrue(ShelvableOne) == true;

    public static ModelTransform GetTransform(this CollectibleObject obj, Dictionary<string, ModelTransform> transforms)
    {
        string code = obj.Code;
        if (code == null) return null;

        foreach ((string key, ModelTransform transform) in transforms)
        {
            if (WildcardUtil.Match(key, code))
            {
                return transform;
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
}