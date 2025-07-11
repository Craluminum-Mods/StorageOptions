namespace StorageOptions;

public class BlockBehaviorName : BlockBehavior
{
    private string main;
    private List<string> parts;

    public BlockBehaviorName(Block block) : base(block)
    {
    }

    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);

        main = properties["main"].AsString();
        parts = properties["parts"].AsObject<List<string>>();
    }

    public override void GetPlacedBlockName(StringBuilder sb, IWorldAccessor world, BlockPos pos)
    {
        string name = main.ConstructName(parts);
        if (!string.IsNullOrEmpty(name))
        {
            sb.Clear();
            sb.Append(name);
        }
    }

    public override void GetHeldItemName(StringBuilder sb, ItemStack itemStack)
    {
        string name = main.ConstructName(parts);
        if (!string.IsNullOrEmpty(name))
        {
            sb.Clear();
            sb.Append(name);
        }
    }
}