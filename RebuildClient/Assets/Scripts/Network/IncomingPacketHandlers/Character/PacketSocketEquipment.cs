using Assets.Scripts.Network.HandlerBase;
using Assets.Scripts.PlayerControl;
using Assets.Scripts.UI.RefineItem;
using RebuildSharedData.Data;
using RebuildSharedData.Networking;
using static UnityEditor.Progress;

namespace Assets.Scripts.Network.IncomingPacketHandlers.Character
{
    [ClientPacketHandler(PacketType.SocketEquipment)]
    public class PacketSocketEquipment : ClientPacketHandlerBase
    {
        public override void ReceivePacket(ClientInboundMessage msg)
        {
            var bagId = msg.ReadInt32();
            var updatedItem = UniqueItem.Deserialize(msg);

            State.Inventory.ReplaceUniqueItem(bagId, updatedItem);

            UiManager.InventoryWindow.UpdateActiveVisibleBag();
            UiManager.EquipmentWindow.RefreshEquipmentWindow();
            if (bagId == UiManager.Instance.ItemDescriptionWindow.GetInventoryItemBagId())
                UiManager.Instance.ItemDescriptionWindow.ShowItemDescription(PlayerState.Instance.Inventory.GetInventoryItem(bagId));
            if (RefineItemWindow.Instance != null)
                RefineItemWindow.Instance.RevealAndRefresh();
        }
    }
}