using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Collections.Generic;

public class ItemMananger : SingleTon<ItemMananger>
{
    private List<Item> m_itemList;
    private int m_itemNumber;

    private ItemMananger()
    {
        m_itemList = new List<Item>();
        m_itemNumber = 0;
    }

    public void Initialize()
    {
        m_itemList.Clear();
        m_itemNumber = 0;
    }

    public void AllitemRestore()
    {
        for (int i = 0; i < m_itemList.Count; ++i)
        {
            if (m_itemList[i].name == "GunParts" || m_itemList[i].name == "VaccineItem") continue;
            m_itemList[i].SendItemRestore();
        }
    }

    public void AddItem(Item item)
    {
        if (UserType.Host == GameData.Instance.UserType)
        {
            m_itemNumber++;
            item.m_itemID = m_itemNumber;
        }

        if (m_itemList.Find(rhs => rhs.m_itemID == item.m_itemID))
        {
            return;
        }

        m_itemList.Add(item);
    }

    public void RemoveItem(Item item)
    {
        if (!m_itemList.Find(rhs => rhs.m_itemID == item.m_itemID))
        {
            return;
        }

        m_itemList.Remove(item);
    }

    public Item FindItemByID(int id)
    {
        return m_itemList.Find(rhs => rhs.m_itemID == id);
    }

    public void ProcessDirectionPacketData(JObject packetData)
    {
        ItemState itemState = (ItemState)packetData.Value<int>("type");
        int itemID = packetData.Value<int>("id");
        Item item = FindItemByID(itemID);

        string PartsGetplayer = packetData.Value<string>("playerid");
        KGPlayer GetPlayer = PlayerManager.Instance.FindPlayerByID(PartsGetplayer) as KGPlayer;

        switch (itemState)
        {
            case ItemState.Spwan:
                string itemName = packetData.Value<string>("name");

                if (GetPlayer == null) //파츠가 아닐때
                {
                    if (item != null) return;
                }
                else //파츠일때만 겟플레이어가 있음. 플레이어 아이디가 같으면 리턴
                {
                    if (GameData.Instance.UserType == UserType.User)
                    {
                        if (GetPlayer.m_id == GameData.Instance.Player.m_id) return;
                    }
                }
                
                if (GameData.Instance.UserType == UserType.Host && itemName != "GunParts") return;

                ItemType itemType = (ItemType)packetData.Value<int>("itemtype");

                if (ItemType.Heal == itemType)
                {
                    float hp = packetData.Value<float>("heal");
                    item = AssetManager.Items.Retrieve(itemName, hp) as Item;
                }
                else if (ItemType.GunParts == itemType && GetPlayer != null)
                {
                    GunPartsType type = (GunPartsType)packetData.Value<int>("gunpartsType");
                    item = AssetManager.Items.Retrieve(itemName, GetPlayer, type) as Item;
                }
                else
                {
                    item = AssetManager.Items.Retrieve(itemName) as Item;
                }

                item.m_itemID = itemID;
                item.SetItemTransformFromPacket(packetData);

                break;

            case ItemState.Restore:
                if (null == item) return;
                item.Restore();
                break;

            case ItemState.GunPartsSet:

                GunPartsType partsType = (GunPartsType)packetData.Value<int>("parts");
                //GunPartsType partsType = packetData.Value<int>("")

                Rifle rifle = GetPlayer.CurrentWeapon as Rifle;

                switch (partsType)
                {
                    case GunPartsType.None:
                        break;

                    case GunPartsType.LaserParts:
                        rifle.m_laserPart.gameObject.SetActive(true);
                        rifle.m_laserPart.Init(GetPlayer, 20f);
                        break;

                    case GunPartsType.FireParts:
                        rifle.m_firePart.gameObject.SetActive(true);
                        rifle.m_firePart.Init(GetPlayer, 20f);
                        break;

                    case GunPartsType.GrenadeParts:
                        rifle.m_grenadePart.gameObject.SetActive(true);
                        rifle.m_grenadePart.Init(GetPlayer, 20f);
                        break;
                }

                rifle.DissolveStart();
                break;
        }
    }
}

