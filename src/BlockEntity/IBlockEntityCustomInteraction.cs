namespace StorageOptions;

public interface IBlockEntityCustomInteraction
{
    bool OnInteract(IPlayer byPlayer, BlockSelection blockSel);
}