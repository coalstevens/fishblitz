public interface IGiftReceiving : UseItemInput.IUsableTarget
{
    public bool TryGiveGift(Inventory.ItemType gift);
}
