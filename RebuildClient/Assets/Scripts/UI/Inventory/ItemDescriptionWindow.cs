using Assets.Scripts.PlayerControl;
using Assets.Scripts.Sprites;
using Assets.Scripts.Utility;
using RebuildSharedData.ClientTypes;
using RebuildSharedData.Data;
using RebuildSharedData.Enum;
using RebuildSharedData.Enum.EntityStats;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Inventory
{
    public class ItemDescriptionWindow : WindowBase
    {
        public Sprite DefaultItemPortrait;
        public Image PortraitContainer;
        public TextMeshProUGUI ItemName;
        public TextMeshProUGUI ItemType;
        public TextMeshProUGUI ItemDescription;
        public TextMeshProUGUI ItemWeight;
        public TextMeshProUGUI ItemLevel;
        public RectTransform WindowRect;
        public Image ItemWeightIcon;
        public Image ItemLevelIcon;

        public RectTransform ItemInfo;
        public GameObject CardSocketPanel;

        public Button ShowIllustrationButton;

        public Sprite CardSlotOpen;
        public Sprite CardSlotClosed;

        public List<DraggableItem> CardSocketEntries;

        private InventoryItem inventoryItem;
        private bool isInit;

        public int GetInventoryItemBagId() => inventoryItem.BagSlotId;
        private void Init()
        {
            if (isInit)
                return;

            CardSocketEntries[0].OnRightClick = () => RightClickCardSlot(0);
            CardSocketEntries[1].OnRightClick = () => RightClickCardSlot(1);
            CardSocketEntries[2].OnRightClick = () => RightClickCardSlot(2);
            CardSocketEntries[3].OnRightClick = () => RightClickCardSlot(3);

            isInit = true;
        }

        public void ClickCardIllustrationButton()
        {
            var win = UiManager.Instance.CardIllustrationWindow;
            if (win == null)
            {
                var go = Resources.Load<GameObject>("Card Illustration");
                var go2 = Instantiate(go, UiManager.Instance.PrimaryUserWindowContainer);
                win = go2.GetComponent<CardIllustrationWindow>();
                win.CenterWindow();
                win.HideWindow();
                UiManager.Instance.CardIllustrationWindow = win;
            }

            win.DisplayCard(inventoryItem.ItemData);
        }

        private void DisplayDescription(Sprite collection)
        {
            var item = inventoryItem.ItemData;
            var sb = new StringBuilder();
            switch(item.ItemClass)
            {
                case ItemClass.Equipment:
                    ItemLevel.enabled = true;
                    ItemLevelIcon.enabled = true;
                    ItemType.text = EquipPositionToTypeText(item.Position);
                    if (item.Defense > 0)
                        sb.AppendLine($"<color=#606060>Defense:</color> {item.Defense}");
                    if (item.MagicDef > 0)
                        sb.AppendLine($"<color=#606060>Magic Defense:</color> {item.MagicDef}");
                    if (item.Flee > 0)
                        sb.AppendLine($"<color=#606060>Flee:</color> {item.Flee}");
                    for (var i = 0; i < 8; i++)
                    {
                        var modId = inventoryItem.UniqueItem.GetModifierIdAt(i);
                        ModDescription modDescription = ClientDataLoader.Instance.GetModDescription(modId);
                        if(modDescription == null)
                        {
                            sb.AppendLine($"<color=#000060>Unknown Modifier {modId}</color>");
                            continue;
                        }
                        var modValue = (float)inventoryItem.UniqueItem.GetModifierValueAt(i) / modDescription.DisplayScale;
                        if (modId <= 0 || modValue == 0)
                            continue;
                        string signedValue = modValue.ToString("+0.##;-0.##;0", CultureInfo.InvariantCulture); //> 0 ? $"+{modValue}" : modValue.ToString();
                        sb.AppendLine("<color=#000060>" + modDescription.Description.Replace("{modifierValue}", signedValue) + "</color>");
                    }
                    ItemLevel.text = item.MinLvl.ToString();
                    break;
                case ItemClass.Weapon:
                    ItemLevel.enabled = true;
                    ItemLevelIcon.enabled = true;
                    ItemType.text = WeaponClassToTypeText(item.WeaponClass);
                    var weaponDamagePercent = 0;
                    var weaponDamage = 0;
                    (weaponDamage, weaponDamagePercent) = inventoryItem.UniqueItem.FetchWeaponModifications();
                    if (inventoryItem.UniqueItem.Refine > 0 || weaponDamagePercent > 0 || weaponDamage > 0)
                    {
                        var attackValue = (int)((item.Attack + weaponDamage) * (1f + 3f * inventoryItem.UniqueItem.Refine / 100f) * (1f + weaponDamagePercent / 100f));
                        sb.AppendLine($"<color=#606060>Attack: </color><color=#000060>{attackValue}</color> ({item.Attack})");
                    }
                    else
                        sb.AppendLine($"<color=#606060>Attack:</color> {item.Attack}");
                    sb.AppendLine($"<color=#606060>Attacks per Second:</color> {item.AttackSpeed.ToString("0.0#", CultureInfo.InvariantCulture)}");
                    if (item.AttackElement is not AttackElement.Neutral and not AttackElement.None and not AttackElement.Special)
                        sb.AppendLine($"<color=#606060>Element:</color> {item.AttackElement.ToString()}");
                    if (item.Range > 1)
                        sb.AppendLine($"<color=#606060>Range:</color> {item.Range}");
                    //if (item.MinLvl > 1)
                    //    sb.AppendLine($"<color=#606060>Required Level:</color> {item.MinLvl}");
                    for (var i = 0; i < 8; i++)
                    {
                        var modId = inventoryItem.UniqueItem.GetModifierIdAt(i);
                        ModDescription modDescription = ClientDataLoader.Instance.GetModDescription(modId);
                        if (modDescription == null)
                        {
                            sb.AppendLine($"<color=#000060>Unknown Modifier {modId}</color>");
                            continue;
                        }
                        var modValue = (float)inventoryItem.UniqueItem.GetModifierValueAt(i) / modDescription.DisplayScale;
                        if (modId <= 0 || modValue == 0)
                            continue;
                        string signedValue = modValue.ToString("+0.##;-0.##;0", CultureInfo.InvariantCulture); //> 0 ? $"+{modValue}" : modValue.ToString();
                        sb.AppendLine("<color=#000060>" + modDescription.Description.Replace("{modifierValue}", signedValue) + "</color>");
                    }
                    ItemLevel.text = item.MinLvl.ToString();
                    break;
                default:
                    ItemLevel.enabled = false;
                    ItemLevelIcon.enabled = false;
                    ItemType.text = item.ItemClass.ToString();
                    break;
            }

            ItemName.text = inventoryItem.ToString();
            ItemWeight.text = (item.Weight / 10).ToString();
            ItemDescription.text = ClientDataLoader.Instance.GetItemDescription(item.Code) + sb.ToString();
            PortraitContainer.sprite = collection;

            ShowWindow();
            MoveToTop();


            ShowIllustrationButton.gameObject.SetActive(item.ItemClass == ItemClass.Card);



            if (!item.IsUnique || (item.Slots <= 0 && inventoryItem.UniqueItem.SlotData(0) <= 0) || CardSocketEntries == null || CardSocketEntries.Count == 0)
            {
                CardSocketPanel.SetActive(false);
            }
            else
            {
                CardSocketPanel.SetActive(true);
                var hasSlots = item.Slots > 0 && (inventoryItem.UniqueItem.Flags & (byte)UniqueItemFlags.CraftedItem) == 0;

                for (var i = 0; i < CardSocketEntries.Count; i++)
                {
                    var slot = inventoryItem.UniqueItem.SlotData(i);
                    if (slot > 1)
                    {
                        if (!ClientDataLoader.Instance.TryGetItemById(slot, out var socketed))
                            socketed = item; //lol
                        var sprite = ClientDataLoader.Instance.GetIconAtlasSprite(socketed.Sprite);
                        CardSocketEntries[i].Assign(DragItemType.SocketedItem, sprite, socketed.Id, 1);
                    }
                    else
                    {
                        if (i < item.Slots && hasSlots)
                            CardSocketEntries[i].Assign(DragItemType.SocketedItem, CardSlotOpen, -1, 0);
                        else
                            CardSocketEntries[i].Assign(DragItemType.SocketedItem, CardSlotClosed, -1, 0);
                    }
                }
            }

            ItemDescription.ForceMeshUpdate();
            Vector2 preferredDimensions = ItemDescription.GetPreferredValues(415, 0); //300 minus 20 for margins
            WindowRect.sizeDelta = new Vector2(626, Mathf.Max(260, preferredDimensions.y + 70));
        }

        public string EquipPositionToTypeText(EquipPosition equipPosition)
        {
            switch (equipPosition)
            {
                case EquipPosition.HeadUpper:
                    return "Upper Head";
                case EquipPosition.HeadMid:
                    return "Middle Head";
                case EquipPosition.HeadLower:
                    return "Lower Head";
                case EquipPosition.HeadUpper | EquipPosition.HeadMid:
                    return "Upper & Middle Head";
                case EquipPosition.HeadMid | EquipPosition.HeadLower:
                    return "Middle & Lower Head";
                case EquipPosition.HeadUpper | EquipPosition.HeadLower:
                    return "Upper & Lower Head";
                case EquipPosition.HeadUpper | EquipPosition.HeadMid | EquipPosition.HeadLower:
                    return "Head (All Slots)";
                case EquipPosition.Body:
                    return "Armor";
                case EquipPosition.OffHand:
                    return "Shield";
                case EquipPosition.Garment:
                    return "Garment";
                case EquipPosition.Footgear:
                    return "Footgear";
                case EquipPosition.Accessory:
                    return "Accessory";
                default:
                    return "Unknown";
            }
        }

        public string WeaponClassToTypeText(string WeaponClass)
        {
            switch (WeaponClass)
            {
                case "2HSword":
                    return "Two-Handed Sword";
                case "2HSpear":
                    return "Two-Handed Spear";
                case "2HRod":
                    return "Two-Handed Rod";
                case "2HAxe":
                    return "Two-Handed Axe";
                default:
                    return WeaponClass;
            }
        }

        public void RightClickCardSlot(int slot)
        {
            var id = CardSocketEntries[slot].ItemId;
            if (id > 0)
                UiManager.Instance.SubDescriptionWindow.ShowItemDescription(id);
        }

        public void ShowItemDescription(InventoryItem item)
        {
            Init();

            inventoryItem = item;
            var collectionPath = $"Assets/Sprites/Imported/Collections/{item.ItemData.Sprite}.png";
            ShowIllustrationButton.gameObject.SetActive(false); //depending on how long it takes to load you could hit view illustration on an invalid item
            if (!ClientDataLoader.DoesAddressableExist<Sprite>(collectionPath))
                DisplayDescription(DefaultItemPortrait);
            else
                AddressableUtility.LoadSprite(gameObject, collectionPath, DisplayDescription);
        }

        public void RefreshItemDescription()
        {
            ShowItemDescription(inventoryItem);
        }
        public void ShowItemDescription(int itemId)
        {
            //we aren't related to an inventory item, so we'll have to fake it.
            var data = ClientDataLoader.Instance.GetItemById(itemId);
            ShowItemDescription(new InventoryItem() { BagSlotId = -1, ItemData = data });
        }
    }
}