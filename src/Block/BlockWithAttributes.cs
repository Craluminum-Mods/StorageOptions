namespace StorageOptions;

public class BlockWithAttributes : Block, IBlockCustomMesh
{
    private string[] woodTypes;

    private Dictionary<string, CompositeTexture> textures;
    private CompositeShape cshape;

    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);
        LoadTypes();
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

    public virtual MeshData GetOrCreateMesh(Materials materials, ITexPositionSource overrideTexturesource = null)
    {
        Dictionary<string, MeshData> cMeshes = ObjectCacheUtil.GetOrCreate(api, ToString() + "Meshes", () => new Dictionary<string, MeshData>());
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

            rcshape.Base.Path = rcshape.Base.Path.Replace("{wood}", materials.Wood); // should be {type}
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
            capi.Tesselator.TesselateShape("Groundrack block", shape, out mesh, texSource, null, 0, 0, 0, selectiveElements: cshape.SelectiveElements);
            if (overrideTexturesource == null)
            {
                cMeshes[key] = mesh;
            }
        }
        return mesh;
    }

    public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
    {
        base.OnBeforeRender(capi, itemstack, target, ref renderinfo);
        Dictionary<string, MultiTextureMeshRef> meshRefs = ObjectCacheUtil.GetOrCreate(capi, ToString() + "MeshesInventory", () => new Dictionary<string, MultiTextureMeshRef>());
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

    public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
    {
        return GetInterface<IBlockEntityCustomShapeTextures>(api.World, pos)?.GetOrCreateSelectionBoxes() ?? base.GetSelectionBoxes(blockAccessor, pos);
    }

    public override Cuboidf[] GetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
    {
        return GetInterface<IBlockEntityCustomShapeTextures>(api.World, pos)?.GetOrCreateCollisionBoxes() ?? base.GetCollisionBoxes(blockAccessor, pos);
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        return GetInterface<IBlockEntityCustomInteraction>(world, blockSel.Position)?.OnInteract(byPlayer, blockSel)
        ?? base.OnBlockInteractStart(world, byPlayer, blockSel);
    }

    public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
    {
        bool placed = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);

        IBlockEntityCustomShapeTextures customInterface = GetInterface<IBlockEntityCustomShapeTextures>(world, blockSel.Position);

        if (placed && customInterface != null)
        {
            BlockPos targetPos = blockSel.DidOffset ? blockSel.Position.AddCopy(blockSel.Face.Opposite) : blockSel.Position;
            float num2 = (float)Math.Atan2(byPlayer.Entity.Pos.X - ((double)targetPos.X + blockSel.HitPosition.X), (double)(float)byPlayer.Entity.Pos.Z - ((double)targetPos.Z + blockSel.HitPosition.Z));
            float intervalRad = (float)Math.PI / 2f;
            float meshAngleRad = (float)(int)Math.Round(num2 / intervalRad) * intervalRad;
            customInterface.SetMeshAngleRad(meshAngleRad);
            customInterface.OnBlockPlaced(byItemStack);
        }
        return placed;
    }

    public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
    {
        ItemStack stack = base.OnPickBlock(world, pos);
        IBlockEntityCustomShapeTextures customInterface = GetInterface<IBlockEntityCustomShapeTextures>(world, pos);
        if (customInterface != null && customInterface?.Materials.Full == true)
        {
            stack.Attributes.GetOrAddTreeAttribute("materials").SetString("wood", customInterface.Materials.Wood);
        }
        return stack;
    }

    public override void GetDecal(IWorldAccessor world, BlockPos pos, ITexPositionSource decalTexSource, ref MeshData decalModelData, ref MeshData blockModelData)
    {
        IBlockEntityCustomShapeTextures customInterface = GetInterface<IBlockEntityCustomShapeTextures>(world, pos);
        if (customInterface != null && customInterface?.Materials.Full == true)
        {
            float[] mat = Matrixf.Create().Translate(0.5f, 0.5f, 0.5f).RotateY(customInterface.MeshAngleRad)
                .Translate(-0.5f, -0.5f, -0.5f)
                .Values;
            blockModelData = GetOrCreateMesh(customInterface.Materials).Clone().MatrixTransform(mat);
            decalModelData = GetOrCreateMesh(customInterface.Materials, decalTexSource).Clone().MatrixTransform(mat);
        }
        else
        {
            base.GetDecal(world, pos, decalTexSource, ref decalModelData, ref blockModelData);
        }
    }
}