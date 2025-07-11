namespace StorageOptions;

public class Transformations
{
    public Dictionary<string, ModelTransform> OnGroundRackTransform { get; set; } = new();

    public Dictionary<string, ModelTransform> OnShelfOneTransform { get; set; } = new();
}