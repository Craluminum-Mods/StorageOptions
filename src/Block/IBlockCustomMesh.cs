namespace StorageOptions;

public interface IBlockCustomMesh
{
    MeshData GetOrCreateMesh(Materials materials, ITexPositionSource overrideTexturesource = null);
}