namespace StorageOptions;

public class BlockGroundRack : Block
{
    private string[] woodTypes;

    private Dictionary<string, CompositeTexture> textures;
    private CompositeShape cshape;

    private WorldInteraction[] interactions;

    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);
        LoadTypes();

        if (api.Side == EnumAppSide.Client)
        {
            interactions = (api as ICoreClientAPI)?.GetOrCreateToolrackInteractions("groundRackBlockInteractions", EnumStorageOption.GroundRackable);
        }

        PlacedPriorityInteract = true;
    }

    public void LoadTypes()
    {
        cshape = Attributes["shape"].AsObject<CompositeShape>();
        textures = Attributes["textures"].AsObject<Dictionary<string, CompositeTexture>>();

        woodTypes = api.ResolveVariants(this, "wood");

        List<JsonItemStack> stacks = new();
        foreach (string wood in woodTypes)
        {
            JsonItemStack jstack = api.CreateJStack(this, $"{{ \"materials\": {{ \"wood\": \"{wood}\" }} }}");
            stacks.Add(jstack);
        }

        if (Attributes["creativeInventoryTabs"].Exists)
        {
            string[] tabs = Attributes["creativeInventoryTabs"].AsObject<string[]>();
            this.AddToCreativeInv(stacks, tabs);
        }
    }

    public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
    {
        return GetBlockEntity<BlockEntityGroundRack>(pos)?.GetOrCreateSelectionBoxes() ?? base.GetSelectionBoxes(blockAccessor, pos);
    }

    public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
    {
        bool num = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);
        if (num && world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityGroundRack bect)
        {
            BlockPos targetPos = blockSel.DidOffset ? blockSel.Position.AddCopy(blockSel.Face.Opposite) : blockSel.Position;
            float num2 = (float)Math.Atan2(byPlayer.Entity.Pos.X - ((double)targetPos.X + blockSel.HitPosition.X), (double)(float)byPlayer.Entity.Pos.Z - ((double)targetPos.Z + blockSel.HitPosition.Z));
            float intervalRad = (float)Math.PI / 2f;
            bect.MeshAngleRad = (float)(int)Math.Round(num2 / intervalRad) * intervalRad;
            bect.OnBlockPlaced(byItemStack);
        }
        return num;
    }

    public virtual MeshData GetOrCreateMesh(Materials materials, ITexPositionSource overrideTexturesource = null)
    {
        Dictionary<string, MeshData> cMeshes = ObjectCacheUtil.GetOrCreate(api, this + "Meshes", () => new Dictionary<string, MeshData>());
        ICoreClientAPI capi = api as ICoreClientAPI;
        string key = materials.ToString();

        if (string.IsNullOrEmpty(key))
        {
            return null;
        }

        if (overrideTexturesource != null || !cMeshes.TryGetValue(key, out MeshData mesh))
        {
            mesh = new MeshData(4, 3);
            CompositeShape rcshape = cshape.Clone();
            rcshape.Base.Path = rcshape.Base.Path.Replace("{wood}", materials.Wood);
            rcshape.Base.WithPathAppendixOnce(".json").WithPathPrefixOnce("shapes/");
            Shape shape = capi.Assets.TryGet(rcshape.Base)?.ToObject<Shape>();
            ITexPositionSource texSource = overrideTexturesource;
            if (texSource == null)
            {
                ShapeTextureSource stexSource = new(capi, shape, rcshape.Base.ToString());
                texSource = stexSource;
                foreach (KeyValuePair<string, CompositeTexture> val in textures)
                {
                    CompositeTexture ctex = val.Value.Clone();
                    ctex.Base.Path = ctex.Base.Path.Replace("{wood}", materials.Wood);
                    ctex.Bake(capi.Assets);
                    stexSource.textures[val.Key] = ctex;
                }
            }
            if (shape == null)
            {
                return mesh;
            }
            capi.Tesselator.TesselateShape("Groundrack block", shape, out mesh, texSource, null, 0, 0, 0);
            if (overrideTexturesource == null)
            {
                cMeshes[key] = mesh;
            }
        }
        return mesh;
    }

    public override void GetDecal(IWorldAccessor world, BlockPos pos, ITexPositionSource decalTexSource, ref MeshData decalModelData, ref MeshData blockModelData)
    {
        BlockEntityGroundRack beb = GetBlockEntity<BlockEntityGroundRack>(pos);
        if (beb != null)
        {
            float[] mat = Matrixf.Create().Translate(0.5f, 0.5f, 0.5f).RotateY(beb.MeshAngleRad)
                .Translate(-0.5f, -0.5f, -0.5f)
                .Values;
            blockModelData = GetOrCreateMesh(beb.Materials).Clone().MatrixTransform(mat);
            decalModelData = GetOrCreateMesh(beb.Materials, decalTexSource).Clone().MatrixTransform(mat);
        }
        else
        {
            base.GetDecal(world, pos, decalTexSource, ref decalModelData, ref blockModelData);
        }
    }

    public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
    {
        base.OnBeforeRender(capi, itemstack, target, ref renderinfo);
        Dictionary<string, MultiTextureMeshRef> meshRefs = ObjectCacheUtil.GetOrCreate(capi, this + "MeshesInventory", () => new Dictionary<string, MultiTextureMeshRef>());
        string wood = itemstack.Attributes.GetOrAddTreeAttribute("materials").GetString("wood");
        Materials materials = new(wood);
        string key = materials.ToString();

        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        if (!meshRefs.TryGetValue(key, out MultiTextureMeshRef meshref))
        {
            MeshData mesh = GetOrCreateMesh(materials);
            meshref = meshRefs[key] = capi.Render.UploadMultiTextureMesh(mesh);
        }
        renderinfo.ModelRef = meshref;
    }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

        string wood = inSlot.Itemstack.Attributes.GetOrAddTreeAttribute("materials").GetString("wood");
        new Materials(wood).OutputTranslatedDescription(dsc);
    }

    public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
    {
        ItemStack stack = base.OnPickBlock(world, pos);
        if (world.BlockAccessor.GetBlockEntity(pos) is BlockEntityGroundRack be)
        {
            stack.Attributes.GetOrAddTreeAttribute("materials").SetString("wood", be.Materials.Wood);
        }
        return stack;
    }

    public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
    {
        return new ItemStack[1] { OnPickBlock(world, pos) };
    }

    public override BlockDropItemStack[] GetDropsForHandbook(ItemStack handbookStack, IPlayer forPlayer)
    {
        BlockDropItemStack[] drops = base.GetDropsForHandbook(handbookStack, forPlayer);
        drops[0] = drops[0].Clone();
        drops[0].ResolvedItemstack.SetFrom(handbookStack);
        return drops;
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
