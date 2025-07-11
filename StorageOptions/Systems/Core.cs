global using Newtonsoft.Json.Linq;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using Vintagestory.API.Client;
global using Vintagestory.API.Common;
global using Vintagestory.API.Config;
global using Vintagestory.API.Datastructures;
global using Vintagestory.API.MathTools;
global using Vintagestory.API.Util;
global using Vintagestory.Client.NoObf;
global using Vintagestory.GameContent;
global using static StorageOptions.Constants;

namespace StorageOptions;

public class Core : ModSystem
{
    public Transformations Transformations { get; } = new();

    public override void Start(ICoreAPI api)
    {
        api.RegisterBlockBehaviorClass("StorageOptions.BbName", typeof(BlockBehaviorName));
        api.RegisterBlockClass("StorageOptions.BlockGroundRack", typeof(BlockGroundRack));
        api.RegisterBlockClass("StorageOptions.BlockShelfOne", typeof(BlockShelfOne));
        api.RegisterBlockEntityClass("StorageOptions.BlockEntityGroundRack", typeof(BlockEntityGroundRack));
        api.RegisterBlockEntityClass("StorageOptions.BlockEntityShelfOne", typeof(BlockEntityShelfOne));
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        foreach (TransformConfig config in TransformConfigs)
        {
            if (!GuiDialogTransformEditor.extraTransforms.Contains(config))
            {
                GuiDialogTransformEditor.extraTransforms.Add(config);
            }
        }
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        Transformations.OnGroundRackTransform = api.Assets.Get<Dictionary<string, ModelTransform>>("storageoptions:config/transformations/groundrack.json");
        Transformations.OnShelfOneTransform = api.Assets.Get<Dictionary<string, ModelTransform>>("storageoptions:config/transformations/shelfone.json");
    }

    public override void AssetsFinalize(ICoreAPI api)
    {
        foreach (CollectibleObject obj in api.World.Collectibles)
        {
            PatchGroundRackable(obj);
            PatchShelvableOne(obj);
        }

        Transformations.OnGroundRackTransform.Clear();
        Transformations.OnShelfOneTransform.Clear();

        Mod.Logger.Event("started '{0}' mod", Mod.Info.Name);
    }

    public override void Dispose()
    {
        Transformations.OnGroundRackTransform.Clear();
        Transformations.OnShelfOneTransform.Clear();
    }

    private void PatchGroundRackable(CollectibleObject obj)
    {
        ModelTransform transform = obj.GetTransform(Transformations.OnGroundRackTransform);

        if (WildcardUtil.Match(GroundRackableCodes, obj.Code) || GroundRackableTypes.Contains(obj.GetType()) || obj.Tool != null)
        {
            // obj.AddToCreativeInv(tab: GroundRackable); // for testing

            obj.EnsureAttributesNotNull();
            obj.SetAttribute(GroundRackable, true);

            if (obj.Attributes?.KeyExists(OnGroundRackTransform) == false && transform != null)
            {
                obj.SetAttribute(OnGroundRackTransform, transform);
            }
        }
    }

    private void PatchShelvableOne(CollectibleObject obj)
    {
        ModelTransform transform = obj.GetTransform(Transformations.OnShelfOneTransform);

        if (WildcardUtil.Match(ShelvableOneCodes, obj.Code) || ShelvableOneTypes.Contains(obj.GetType()) || obj?.Attributes?.KeyExists("backpack") == true)
        {
            // obj.AddToCreativeInv(tab: ShelvableOne); // for testing

            obj.EnsureAttributesNotNull();
            obj.SetAttribute(ShelvableOne, true);

            if (obj.Attributes?.KeyExists(OnShelfOneTransform) == false && transform != null)
            {
                obj.SetAttribute(OnShelfOneTransform, transform);
            }
        }
    }
}
