namespace StorageOptions;

public static class Constants
{
    public const string ShelvableOne = "shelvableone";
    public const string GroundRackable = "groundrackable";
    public const string OnGroundRackTransform = "onGroundRackTransform";
    public const string OnShelfOneTransform = "onShelfOneTransform";

    public static readonly AssetLocation DefaultPlaceSound = new("sounds/player/build");

    public static readonly List<TransformConfig> TransformConfigs = new()
    {
        new() { AttributeName = OnGroundRackTransform, Title = "In Ground Rack" },
        new() { AttributeName = OnShelfOneTransform, Title = "On Shelf One" },
    };

    public static readonly string[] ShelvableOneCodes = new string[]
    {
        "*ingotmold*",
        "*toolmold*",
        "game:*arcass*",
        "game:helvehammerhead-*",
        "game:pineapple",
        "game:pumpkin-fruit-*",
        "game:sieve-*",
    };

    public static readonly Type[] ShelvableOneTypes = new Type[]
    {
        typeof(BlockAnvil),
        typeof(BlockAnvilPart),
        typeof(BlockBomb),
        typeof(BlockBucket),
        typeof(BlockCookedContainer),
        typeof(BlockCookingContainer),
        typeof(BlockCrock),
        typeof(BlockIngotMold),
        typeof(BlockLantern),
        typeof(BlockLiquidContainerTopOpened),
        typeof(BlockMeal),
        typeof(BlockOmokTable),
        typeof(BlockPan),
        typeof(BlockPie),
        typeof(BlockPlantContainer),
        typeof(BlockResonator),
        typeof(BlockSkep),
        typeof(BlockSmeltedContainer),
        typeof(BlockSmeltingContainer),
        typeof(BlockToolMold),
        typeof(BlockTorch),
        typeof(BlockWateringCan),
        typeof(ItemWorkItem),
    };

    public static readonly string[] GroundRackableCodes = new string[] {
        "@(.*):club-(.*)",
        "@chiseltools:(blockswapper|handplaner|handwedge|laddermaker|paintbrush|pantograph)-.*",
        "aculinaryartillery:rollingpin-*",
        "ancienttools:adze-*",
        "ancienttools:mallet-*",
        "ancienttools:pestle-*",
        "fishing:fishingpole-*",
        "game:bowstave-*",
        "game:solderingiron",
        "game:tongs",
        "glassmaking:glassworkpipe",
        "rustboundmagic:tooladmin-*",
        "rustboundmagic:toolstaff-*",
        "rustyshell:ramrod",
        "rustyshell:shell-*",
        "usefulstuff:climbingpick-*",
     };

    public static readonly Type[] GroundRackableTypes = new Type[]
    {
        typeof(ItemBugnet),
        typeof(ItemCleaver),
        typeof(ItemOar),
        typeof(ItemShield),
        typeof(ItemWrench),
    };
}