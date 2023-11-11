namespace StorageOptions;

public class ItemSlotGroundRack : ItemSlot
{
    public override int MaxSlotStackSize => 1;

    public ItemSlotGroundRack(InventoryBase inventory) : base(inventory)
    {
        this.inventory = inventory;
    }

    public override bool CanTakeFrom(ItemSlot sourceSlot, EnumMergePriority priority = EnumMergePriority.AutoMerge)
    {
        return sourceSlot.IsStorable(GroundRackable) && base.CanTakeFrom(sourceSlot, priority);
    }

    public override bool CanHold(ItemSlot fromSlot)
    {
        return fromSlot.IsStorable(GroundRackable) && base.CanHold(fromSlot);
    }
}
