namespace StorageOptions;

public interface IBlockEntityCustomMesh
{
    MeshData GetOrCreateMesh(Materials materials, ITexPositionSource overrideTexturesource = null);
}