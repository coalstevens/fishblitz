public interface IGiftReceiving : UseItemInput.IUsableTarget
{
    public bool TryGiveGift(Inventory.Item gift);
}
