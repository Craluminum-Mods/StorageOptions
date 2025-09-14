namespace StorageOptions;

public class BlockGroundRack : BlockGeneric
{
    private WorldInteraction[] interactions;

    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);
        if (api.Side == EnumAppSide.Client)
        {
            interactions = (api as ICoreClientAPI)?.GetOrCreateToolrackInteractions("groundRackBlockInteractions", EnumStorageOption.GroundRackable);
        }

        PlacedPriorityInteract = true;
    }

    public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
    {
        return interactions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
    }

    public override bool DoParticalSelection(IWorldAccessor world, BlockPos pos) => true;

    public override Vec4f GetSelectionColor(ICoreClientAPI capi, BlockPos pos) => ColorUtil.WhiteArgbVec;

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        return world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityGroundRack blockEntity
            ? blockEntity.OnInteract(byPlayer, blockSel)
            : base.OnBlockInteractStart(world, byPlayer, blockSel);
    }
}
