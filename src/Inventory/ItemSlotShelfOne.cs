namespace StorageOptions;

public class ItemSlotShelfOne : ItemSlot
{
    public override int MaxSlotStackSize => 1;

    public ItemSlotShelfOne(InventoryBase inventory) : base(inventory)
    {
        this.inventory = inventory;
    }

    public override bool CanTakeFrom(ItemSlot sourceSlot, EnumMergePriority priority = EnumMergePriority.AutoMerge)
    {
        return sourceSlot.IsStorable(ShelvableOne) && base.CanTakeFrom(sourceSlot, priority);
    }

    public override bool CanHold(ItemSlot fromSlot)
    {
        return fromSlot.IsStorable(ShelvableOne) && base.CanHold(fromSlot);
    }
}
