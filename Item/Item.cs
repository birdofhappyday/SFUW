using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public enum ItemType
{
    Heal,
    Buff,
    Coin,
    GunParts,
    Vaccine,
}

public enum ItemState
{
    Spwan,
    Restore,
    GunPartsSet,
}

public class Item : BaseItem
{
    public ItemType m_itemType;
    public Transform rendTransform;
    
    private CoroutineCommand m_coroutine;
    protected bool isUIActive = false;

    protected virtual void Active(Transform other) { }

    protected override void OnInit(params object[] parameters)
    {
        ItemMananger.Instance.AddItem(this);
    }

    protected override void OnStart() { }

    protected override void OnEnd()
    {
        ItemMananger.Instance.RemoveItem(this);

        if (UserType.Host == GameData.Instance.UserType && StageNum.Stage1 == GameData.Instance.StageNum)
        {
            EventManager.Instance.Notify<IEventItemRestore>((receiver) => receiver.ItemRestore());
        }
    }

    protected override void OnUpdate(Transform other) { }

    #region Unity

    private void OnTriggerEnter(Collider other)
    {
        if (UserType.Host == GameData.Instance.UserType) return;

        if (GameData.Instance.Player.IsDead) return;

        CameraDirection cameraDirection = other.GetComponent<CameraDirection>();

        if (null == cameraDirection) return;
        
        switch(m_itemType)
        {
            case ItemType.GunParts:
                if (!GameData.Instance.Player.IsPartsOn)
                {
                    GameData.Instance.Player.IsPartsOn = true;
                    Active(other.transform);
                    SendItemRestore();
                }
                break;

            case ItemType.Buff:
                bool isBuffOn = GameData.Instance.Player.IsBuffOn;
                if (isBuffOn == false)
                {
                    Active(other.transform);
                    SendItemRestore();
                }
                break;

            case ItemType.Coin:
            case ItemType.Heal:
                Active(other.transform);
                SendItemRestore();
                break;
        }
    }

    #endregion

    #region 코루틴

    protected void ItemActive(float time, Action beforeAction = null, Action afterAction = null)
    {
        m_coroutine = CoroutineManager.Instance.Register(ItemSetActive(time, beforeAction, afterAction));
    }

    private IEnumerator<CoroutinePhase> ItemSetActive(float time, Action beforeAction = null, Action afterAction = null)
    {
        yield return Suspend.Do(time);

        if (null != beforeAction)
        {
            beforeAction();
        }

        gameObject.SetActive(true);

        if (null != afterAction)
        {
            afterAction();
        }

        CoroutineManager.Instance.Unregister(m_coroutine);
    }

    #endregion

    #region Network

    public void SendItemRestore()
    {
        JObject packetData = new JObject();

        packetData.Add("type", (int)ItemState.Restore);
        packetData.Add("itemtype", (int)m_itemType);
        packetData.Add("id", m_itemID);
        Network.Instance.SendTCP("item packet", packetData);
    }

    public JObject GetBuffItemSpwanData()
    {
        Transform dpr = transform;
        JObject itemData = new JObject();

        itemData.Add("type", (int)ItemState.Spwan);
        itemData.Add("itemtype", (int)ItemType.Buff);
        itemData.Add("name", gameObject.name);
        itemData.Add("id", m_itemID);

        itemData.Add("dpx", dpr.position.x);
        itemData.Add("dpy", dpr.position.y);
        itemData.Add("dpz", dpr.position.z);

        itemData.Add("drx", dpr.rotation.x);
        itemData.Add("dry", dpr.rotation.y);
        itemData.Add("drz", dpr.rotation.z);
        itemData.Add("drw", dpr.rotation.w);

        return itemData;
    }

    public JObject GetGunPartsSpawnData()
    {
        Transform dpr = transform;
        JObject itemData = new JObject();

        itemData.Add("type", (int)ItemState.Spwan);
        itemData.Add("itemtype", (int)ItemType.GunParts);
        itemData.Add("name", gameObject.name);
        itemData.Add("id", m_itemID);

        itemData.Add("dpx", dpr.position.x);
        itemData.Add("dpy", dpr.position.y);
        itemData.Add("dpz", dpr.position.z);

        itemData.Add("drx", dpr.rotation.x);
        itemData.Add("dry", dpr.rotation.y);
        itemData.Add("drz", dpr.rotation.z);
        itemData.Add("drw", dpr.rotation.w);

        return itemData;
    }

    public JObject GetHealItemSpwanData()
    {
        Transform dpr = transform;
        JObject itemData = new JObject();

        itemData.Add("type", (int)ItemState.Spwan);
        itemData.Add("itemtype", (int)ItemType.Heal);
        itemData.Add("name", gameObject.name);
        itemData.Add("heal", dpr.GetComponent<RandomHealPack>().HP);
        itemData.Add("id", m_itemID);

        itemData.Add("dpx", dpr.position.x);
        itemData.Add("dpy", dpr.position.y);
        itemData.Add("dpz", dpr.position.z);

        itemData.Add("drx", dpr.rotation.x);
        itemData.Add("dry", dpr.rotation.y);
        itemData.Add("drz", dpr.rotation.z);
        itemData.Add("drw", dpr.rotation.w);

        return itemData;
    }

    public JObject GetVaccineItemSpawnData()
    {
        Transform dpr = transform;
        JObject itemData = new JObject();

        itemData.Add("type", (int)ItemState.Spwan);
        itemData.Add("itemtype", (int)ItemType.Vaccine);
        itemData.Add("name", gameObject.name);
        itemData.Add("id", m_itemID);

        itemData.Add("dpx", dpr.position.x);
        itemData.Add("dpy", dpr.position.y);
        itemData.Add("dpz", dpr.position.z);

        itemData.Add("drx", dpr.rotation.x);
        itemData.Add("dry", dpr.rotation.y);
        itemData.Add("drz", dpr.rotation.z);
        itemData.Add("drw", dpr.rotation.w);

        return itemData;
    }


    public void SetItemTransformFromPacket(JObject packetData)
    {
        if (GameData.Instance.UserType == UserType.Host && transform.name != ("GunParts")) return;

        m_itemID = packetData.Value<int>("id");

        Vector3 itemPos = new Vector3(packetData.Value<float>("dpx"), packetData.Value<float>("dpy"), packetData.Value<float>("dpz"));
        transform.position = itemPos;

        Quaternion itemRot = new Quaternion(packetData.Value<float>("drx"), packetData.Value<float>("dry"), packetData.Value<float>("drz"), packetData.Value<float>("drw"));
        transform.rotation = itemRot;
    }

    #endregion
}
