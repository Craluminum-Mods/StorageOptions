namespace StorageOptions;

public class BlockEntityShelfOne : BlockEntityDisplay
{
    private readonly InventoryGeneric inventory;
    private const int slotCount = 1;

    public override InventoryBase Inventory => inventory;
    public override string InventoryClassName => Block?.Attributes?["inventoryClassName"].AsString();
    public override string AttributeTransformCode => Block?.Attributes?["attributeTransformCode"].AsString();

    public BlockEntityShelfOne()
    {
        inventory = new InventoryGeneric(slotCount, "shelfone-0", Api, (_, inv) => new ItemSlotShelfOne(inv));
    }

    internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
    {
        ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
        if (slot.Empty)
        {
            return TryTake(byPlayer, blockSel);
        }
        if (slot.IsShelvableOne())
        {
            AssetLocation sound = slot.Itemstack?.Block?.Sounds?.Place;
            if (TryPut(slot, blockSel))
            {
                Api.World.PlaySoundAt(sound ?? DefaultPlaceSound, byPlayer.Entity, byPlayer, randomizePitch: true, 16f);
                updateMeshes();
                return true;
            }
        }
        return false;
    }

    private bool TryPut(ItemSlot slot, BlockSelection blockSel)
    {
        int index = blockSel.SelectionBoxIndex;

        if (inventory[index].Empty)
        {
            int amount = slot.TryPutInto(Api.World, inventory[index]);
            updateMeshes();
            MarkDirty(redrawOnClient: true);
            (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
            return amount > 0;
        }
        return false;
    }

    private bool TryTake(IPlayer byPlayer, BlockSelection blockSel)
    {
        int index = blockSel.SelectionBoxIndex;

        if (!inventory[index].Empty)
        {
            ItemStack stack = inventory[index].TakeOut(1);
            if (byPlayer.InventoryManager.TryGiveItemstack(stack))
            {
                AssetLocation sound = stack.Block?.Sounds?.Place;
                Api.World.PlaySoundAt(sound ?? DefaultPlaceSound, byPlayer.Entity, byPlayer, randomizePitch: true, 16f);
            }
            if (stack.StackSize > 0)
            {
                Api.World.SpawnItemEntity(stack, Pos.ToVec3d().Add(0.5, 0.5, 0.5));
            }
            (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
            MarkDirty(redrawOnClient: true);
            updateMeshes();
            return true;
        }
        return false;
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
    {
        foreach (BlockEntityBehavior behavior in Behaviors)
        {
            behavior.GetBlockInfo(forPlayer, dsc);
        }

        dsc.AppendLine();

        if (!inventory[0].Empty)
        {
            ItemStack stack = inventory[0].Itemstack;
            dsc.AppendLine(stack.GetName());
        }
    }

    protected override float[][] genTransformationMatrices()
    {
        float[][] tfMatrices = new float[slotCount][];

        const float x = 0.5f;
        const float y = 0.125f;
        const float z = 0.5f;
        tfMatrices[0] = new Matrixf()
            .Translate(0.5f, 0f, 0.5f)
            .RotateYDeg(Block.Shape.rotateY)
            .Translate(x - 0.5f, y, z - 0.5f)
            .Translate(-0.5f, 0f, -0.5f)
            .Values;

        return tfMatrices;
    }
}