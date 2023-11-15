namespace StorageOptions;

public class BlockEntityGroundRack : BlockEntityDisplay, IRotatable, IBlockEntityCustomShapeTextures, IBlockEntityCustomInteraction
{
    private readonly InventoryGeneric inventory;
    private const int slotCount = 4;

    private MeshData mesh;
    private float[] mat;
    private Cuboidf[] SelectionBoxes;
    private Cuboidf[] CollisionBoxes;

    public Materials Materials { get; } = new();
    public float MeshAngleRad { get; set; }

    public override InventoryBase Inventory => inventory;
    public override string InventoryClassName => Block?.Attributes?["inventoryClassName"].AsString();
    public override string AttributeTransformCode => Block?.Attributes?["attributeTransformCode"].AsString();
    public string AttributeCode => Block?.Attributes?["attributeCode"].AsString();

    public BlockEntityGroundRack()
    {
        inventory = new InventoryGeneric(slotCount, "groundrack-0", Api, (_, inv) => new ItemSlotGroundRack(inv));
    }

    private void Init()
    {
        if (Api == null || Api.Side != EnumAppSide.Client || !Materials.Full)
        {
            return;
        }

        IBlockCustomMesh customInterface = Block.GetInterface<IBlockCustomMesh>(Api.World, Pos);
        if (customInterface == null)
        {
            return;
        }

        mesh = customInterface.GetOrCreateMesh(Materials);
        mat = Matrixf.Create().Translate(0.5f, 0.5f, 0.5f).RotateY(MeshAngleRad)
            .Translate(-0.5f, -0.5f, -0.5f)
            .Values;
    }

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        if (mesh == null)
        {
            Init();
        }
    }

    public override void OnBlockPlaced(ItemStack byItemStack = null)
    {
        base.OnBlockPlaced(byItemStack);
        Materials.FromTreeAttribute(byItemStack?.Attributes.GetOrAddTreeAttribute("materials"));
        Init();
    }

    public bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
    {
        ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
        if (slot.Empty)
        {
            return TryTake(byPlayer, blockSel);
        }
        if (slot.IsStorable(AttributeCode))
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
        int i = blockSel.SelectionBoxIndex;

        if (inventory[i].Empty)
        {
            int amount = slot.TryPutInto(Api.World, inventory[i]);
            updateMeshes();
            MarkDirty(redrawOnClient: true);
            (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
            return amount > 0;
        }
        return false;
    }

    private bool TryTake(IPlayer byPlayer, BlockSelection blockSel)
    {
        int i = blockSel.SelectionBoxIndex;

        if (!inventory[i].Empty)
        {
            ItemStack stack = inventory[i].TakeOut(1);
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

    protected override float[][] genTransformationMatrices()
    {
        float[][] tfMatrices = new float[slotCount][];
        const float y = 0.125f;
        const float z = 0.25f;

        for (int index = 0; index < slotCount; index++)
        {
            float x = (index == 1 || index == 2) ? 0.625f : 0.25f;

            Matrixf matrix = new Matrixf()
                .Translate(0.5f, 0f, 0.5f)
                .RotateY(MeshAngleRad + (index == 2 || index == 3 ? (float)Math.PI : 0));

            tfMatrices[index] = matrix
                .Translate(x - 0.5f, y, z - 0.5f)
                .Translate(-0.5f, 0f, -0.5f)
                .Values;
        }

        return tfMatrices;
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        base.ToTreeAttributes(tree);
        Materials.ToTreeAttribute(tree.GetOrAddTreeAttribute("materials"));
        tree.SetFloat("meshAngleRad", MeshAngleRad);
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
    {
        base.FromTreeAttributes(tree, worldForResolving);
        Materials.FromTreeAttribute(tree.GetOrAddTreeAttribute("materials"));
        MeshAngleRad = tree.GetFloat("meshAngleRad");
        Init();
    }

    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
    {
        mesher.AddMeshData(mesh, mat);
        base.OnTesselation(mesher, tessThreadTesselator);
        return true;
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
    {
        int i = forPlayer.CurrentBlockSelection.SelectionBoxIndex;

        Materials.OutputTranslatedDescription(sb, Block);

        ItemSlot slot = inventory[i];
        if (!slot.Empty)
        {
            ItemStack stack = slot.Itemstack;
            sb.AppendLine(stack.GetName());
        }
    }

    public void OnTransformed(ITreeAttribute tree, int degreeRotation, EnumAxis? flipAxis)
    {
        MeshAngleRad = tree.GetFloat("meshAngleRad");
        MeshAngleRad -= degreeRotation * ((float)Math.PI / 180f);
        tree.SetFloat("meshAngleRad", MeshAngleRad);
    }

    public Cuboidf[] GetOrCreateSelectionBoxes()
    {
        if (SelectionBoxes == null)
        {
            Cuboidf[] _selectionBoxes = Block.SelectionBoxes;
            SelectionBoxes = new Cuboidf[_selectionBoxes.Length];
            for (int i = 0; i < _selectionBoxes.Length; i++)
            {
                SelectionBoxes[i] = _selectionBoxes[i].RotatedCopy(0f, MeshAngleRad * (180f / (float)Math.PI), 0f, new Vec3d(0.5, 0.5, 0.5));
            }
        }
        return SelectionBoxes;
    }

    public Cuboidf[] GetOrCreateCollisionBoxes()
    {
        if (CollisionBoxes == null)
        {
            Cuboidf[] _collisionBoxes = Block.CollisionBoxes;
            CollisionBoxes = new Cuboidf[_collisionBoxes.Length];
            for (int i = 0; i < _collisionBoxes.Length; i++)
            {
                CollisionBoxes[i] = _collisionBoxes[i].RotatedCopy(0f, MeshAngleRad * (180f / (float)Math.PI), 0f, new Vec3d(0.5, 0.5, 0.5));
            }
        }
        return CollisionBoxes;
    }
}