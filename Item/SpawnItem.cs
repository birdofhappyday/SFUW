using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnItem : MonoBehaviour
{
    [System.Serializable]
    public class TransformItem
    {
        public Transform itemPos;
        public string spawnItemName;
        public float respawnTime;
        public bool isLoop;
        public ItemType itemType;

        private bool isSpawnItemCall = false;
        private Item item = null;

        public bool IsSpawnItemCall { get { return isSpawnItemCall; } set { isSpawnItemCall = value; } }
        public Item Item { get { return item; } set { item = value; } }
    }

    public List<TransformItem> itemList;

    private Coroutine checkCoroutine;
    private Coroutine[] spawnCoroutine;

    public void Start()
    {
        if (UserType.User == GameData.Instance.UserType) return;

        //Vector3 avg = transform.position;
        //float temp = 0;

        //for (int i = 0; i < PlayerManager.Instance.GetPlayerList.Count; ++i)
        //{
        //    temp += PlayerManager.Instance.GetPlayerList[i].m_tracker.Head.transform.position.y;
        //}

        //temp /= PlayerManager.Instance.GetPlayerList.Count;
        //avg.y = temp / 2;
        //transform.position = avg;

        spawnCoroutine = new Coroutine[itemList.Count];

        for (int i = 0; i < itemList.Count; ++i)
        {
            if (string.IsNullOrEmpty(itemList[i].spawnItemName) || itemList[i].itemPos == null) continue;

            switch (itemList[i].itemType)
            {
                case ItemType.GunParts:
                    itemList[i].Item = AssetManager.Items.Retrieve(itemList[i].spawnItemName, itemList[i].itemPos) as Item;
                    Network.Instance.SendTCP("item packet", itemList[i].Item.GetGunPartsSpawnData());
                    break;
                case ItemType.Heal:
                    float plusHP = Random.Range(5, 11);
                    plusHP *= 10f;
                    itemList[i].Item = AssetManager.Items.Retrieve(itemList[i].spawnItemName, itemList[i].itemPos, plusHP) as Item;
                    Network.Instance.SendTCP("item packet", itemList[i].Item.GetHealItemSpwanData());
                    break;
                case ItemType.Coin:
                    itemList[i].Item = AssetManager.Items.Retrieve(itemList[i].spawnItemName, itemList[i].itemPos) as Item;
                    Network.Instance.SendTCP("item packet", itemList[i].Item.GetBuffItemSpwanData());
                    break;
            }
        }
        checkCoroutine = StartCoroutine(ItemCheck());
    }

    public IEnumerator ItemCheck()
    {
        if (UserType.User == GameData.Instance.UserType) yield break;

        while (true)
        {
            yield return new WaitForSeconds(2f);
            for (int i = 0; i < itemList.Count; ++i)
            {
                if (itemList[i].Item == null || string.IsNullOrEmpty(itemList[i].spawnItemName) || !itemList[i].isLoop || itemList[i].Item.gameObject.activeSelf || itemList[i].IsSpawnItemCall) continue;
                itemList[i].IsSpawnItemCall = true;
                spawnCoroutine[i] = StartCoroutine(ItemSpawn(i, itemList[i].itemType));
            }
        }
    }

    private IEnumerator ItemSpawn(int num, ItemType itemType)
    {
        yield return new WaitForSeconds(itemList[num].respawnTime);

        switch (itemType)
        {
            case ItemType.GunParts:
                itemList[num].Item = AssetManager.Items.Retrieve(itemList[num].spawnItemName, itemList[num].itemPos) as Item;
                Network.Instance.SendTCP("item packet", itemList[num].Item.GetGunPartsSpawnData());
                break;
            case ItemType.Heal:
                float plusHP = Random.Range(5, 11);
                plusHP *= 10f;
                itemList[num].Item = AssetManager.Items.Retrieve(itemList[num].spawnItemName, itemList[num].itemPos, plusHP) as Item;
                Network.Instance.SendTCP("item packet", itemList[num].Item.GetHealItemSpwanData());
                break;
            case ItemType.Coin:
                itemList[num].Item = AssetManager.Items.Retrieve(itemList[num].spawnItemName, itemList[num].itemPos) as Item;
                Network.Instance.SendTCP("item packet", itemList[num].Item.GetBuffItemSpwanData());
                break;
        }

        itemList[num].IsSpawnItemCall = false;
    }

    public void OnDisable()
    {
        if (UserType.User == GameData.Instance.UserType) return;

        if (checkCoroutine != null)
        {
            StopCoroutine(checkCoroutine);
            //checkCoroutine = null;
        }

        for (int i = 0; i < spawnCoroutine.Length; ++i)
        {
            if (spawnCoroutine[i] != null)
            {
                StopCoroutine(spawnCoroutine[i]);
                //spawnCoroutine[i] = null;
            }
        }

        ItemMananger.Instance.AllitemRestore();
    }
}