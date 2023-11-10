namespace StorageOptions;

public interface IBlockEntityCustomShapeTextures
{
    Materials Materials { get; }

    float MeshAngleRad { get; set; }

    Cuboidf[] GetOrCreateSelectionBoxes();

    Cuboidf[] GetOrCreateCollisionBoxes();

    void SetMeshAngleRad(float angleRad) => MeshAngleRad = angleRad;

    void OnBlockPlaced(ItemStack byItemStack);

    bool OnInteract(IPlayer byPlayer, BlockSelection blockSel);
}