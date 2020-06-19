using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemMakeBox : MonoBehaviour, IEventItemRestore
{
    [System.Serializable]
    public class ItemTransform
    {
        public Transform m_transform;

        private Item item;

        public Item Item
        {
            get { return item; }
            set { item = value; }
        }
    }

    [Header("------------- Base Property -------------")]

    public List<ItemTransform> m_itemTransform;

    #region HealPack

    [Header("------------- Heal Pack -------------")]
    [Tooltip("소환할 개수 선택")]
    public int m_healNumber;
    [Tooltip("처음 소환되는 시간")]
    public float m_healStartTime;
    [Tooltip("반복되는 시간 체크")]
    public float m_healTime;

    #endregion

    #region Coin

    [Header("------------- Coin -------------")]
    [Tooltip("소환할 개수 선택")]
    public int m_coinNumber;
    [Tooltip("처음 소환되는 시간")]
    public float m_coinStartTime;
    [Tooltip("반복되는 시간 체크")]
    public float m_coinTime;

    #endregion

    #region BuffItem

    [Header("------------- Buff -------------")]
    [Tooltip("소환할 개수 선택")]
    public int m_buffNumber;
    [Tooltip("소환되는 버프 아이템 이름")]
    public string[] m_buffName;
    [Tooltip("처음 소환되는 시간")]
    public float m_buffStartTime;
    [Tooltip("반복되는 시간 체크")]
    public float m_buffTime;

    #endregion
    
    private List<Item> m_healList;
    private List<Item> m_coinList;
    private List<Item> m_buffList;

    private List<ItemTransform> m_remainTransform;
    private List<ItemTransform> m_useTransform;

    private CoroutineCommand m_makeBuffCoroutine;
    private CoroutineCommand m_makeHealCoroutine;
    private CoroutineCommand m_makeCoinCoroutine;
    private CoroutineCommand m_itemRestoreCoroutine;

    public void ItemRestore()
    {
        UseCheck();
    }

    private void Start()
    {
        if (UserType.Host != GameData.Instance.UserType) return;

        EventManager.Instance.AddReceiver<IEventItemRestore>(this);

        m_healList = new List<Item>();
        m_coinList = new List<Item>();
        m_buffList = new List<Item>();

        m_remainTransform = new List<ItemTransform>();
        m_useTransform = new List<ItemTransform>();

        if (m_itemTransform.Count < m_coinNumber + m_healNumber + m_buffNumber)
        {
            Log.Error("아이템 위치 트랜스폼 갯수가 아이템들 모두 더한 수보다 작습니다.");
            return;
        }

        m_remainTransform = m_itemTransform;

        if (0 != m_coinNumber)
        {
            m_makeCoinCoroutine = CoroutineManager.Instance.Register(MakeCoin());
        }

        if (0 != m_healNumber)
        {
            m_makeHealCoroutine = CoroutineManager.Instance.Register(MakeHealPack());
        }

        if (0 != m_buffNumber)
        {
            m_makeBuffCoroutine = CoroutineManager.Instance.Register(MakeBuffItem());
        }
    }

    private void OnDisable()
    {
        //ItemMananger.Instance.AllitemRestore();
        //ItemMananger.Instance.Initialize();

        if (UserType.Host == GameData.Instance.UserType)
        {
            EventManager.Instance.RemoveReceiver<IEventItemRestore>(this);

            if (null != m_makeBuffCoroutine)
            {
                CoroutineManager.Instance.Unregister(m_makeBuffCoroutine);
                m_makeBuffCoroutine = null;
            }

            if (null != m_makeCoinCoroutine)
            {
                CoroutineManager.Instance.Unregister(m_makeCoinCoroutine);
                m_makeCoinCoroutine = null;
            }

            if (null != m_makeHealCoroutine)
            {
                CoroutineManager.Instance.Unregister(m_makeHealCoroutine);
                m_makeHealCoroutine = null;
            }

            m_useTransform.Clear();
            m_remainTransform.Clear();

            if (0 != m_healList.Count)
            {
                m_healList.Clear();
            }

            if (0 != m_coinList.Count)
            {
                m_coinList.Clear();
            }

            if (0 != m_buffList.Count)
            {
                m_buffList.Clear();
            }
            
            m_itemRestoreCoroutine = CoroutineManager.Instance.Register(RemainItemRestore());
        }
    }

    #region 사용함수

    /// <summary>
    /// 아이템중에서 사용된 아이템을 걸러낸다.
    /// </summary>
    public void ItemRestoreCheck(List<Item> itemList)
    {
        for (int i = 0; i < itemList.Count;)
        {
            if (AssetState.Waiting == itemList[i].AssetState)
            {
                itemList.Remove(itemList[i]);
            }
            else
            {
                ++i;
            }
        }
    }

    /// <summary>
    /// 사용한 transform중에서 item이 없어진 transform을 걸러낸다.
    /// 사용이 끝난 transform은 다시 m_remainTransform로 보낸다.
    /// </summary>
    private void UseCheck()
    {
        for (int i = 0; i < m_useTransform.Count; ++i)
        {
            if (AssetState.Waiting == m_useTransform[i].Item.AssetState)
            {
                m_remainTransform.Add(m_useTransform[i]);
                m_useTransform.Remove(m_useTransform[i]);
            }
        }
    }

    /// <summary>
    /// m_useTransform List를 관리한다.
    /// </summary>
    /// <param name="itemTransform"></param>
    public void TransformSetting(ItemTransform itemTransform)
    {
        m_useTransform.Add(itemTransform);
        m_remainTransform.Remove(itemTransform);
    }

    #endregion

    private IEnumerator<CoroutinePhase> RemainItemRestore()
    {
        yield return Suspend.Do(3.0f);
        ItemMananger.Instance.AllitemRestore();
    }

    private IEnumerator<CoroutinePhase> MakeHealPack()
    {
        yield return Suspend.Do(m_healStartTime);
        for (int i = m_healList.Count; i < m_healNumber; ++i)
        {
            if (0 == m_remainTransform.Count) break;
            int transforem_temp = Random.Range(0, m_remainTransform.Count);
            float hp_temp = Random.Range(5, 11);
            hp_temp *= 10;
            Item item = AssetManager.Items.Retrieve("FirstAidKit_prefab", hp_temp, m_remainTransform[transforem_temp].m_transform) as Item;
            m_remainTransform[transforem_temp].Item = item;
            TransformSetting(m_remainTransform[transforem_temp]);
            m_healList.Add(item);
            Network.Instance.SendTCP("item packet", item.GetHealItemSpwanData());
            yield return null;
        }

        while (true)
        {
            yield return Suspend.Do(m_healTime);

            ItemRestoreCheck(m_healList);
            if (m_healNumber != m_healList.Count)
            {
                for (int i = m_healList.Count; i < m_healNumber; ++i)
                {
                    if (0 == m_remainTransform.Count) break;
                    int transforem_temp = Random.Range(0, m_remainTransform.Count);
                    float hp_temp = Random.Range(5, 11);
                    hp_temp *= 10;
                    Item item = AssetManager.Items.Retrieve("FirstAidKit_prefab", hp_temp, m_remainTransform[transforem_temp].m_transform) as Item;
                    m_remainTransform[transforem_temp].Item = item;
                    TransformSetting(m_remainTransform[transforem_temp]);
                    m_healList.Add(item);
                    Network.Instance.SendTCP("item packet", item.GetHealItemSpwanData());
                    yield return null;
                }
            }
        }
    }
    
    private IEnumerator<CoroutinePhase> MakeCoin()
    {
        yield return Suspend.Do(m_coinStartTime);

        for (int i = m_coinList.Count; i < m_coinNumber; ++i)
        {
            if (0 == m_remainTransform.Count) break;
            int transforem_temp = Random.Range(0, m_remainTransform.Count);
            Item item = AssetManager.Items.Retrieve("Coin", m_remainTransform[transforem_temp].m_transform) as Item;
            m_remainTransform[transforem_temp].Item = item;
            TransformSetting(m_remainTransform[transforem_temp]);
            m_coinList.Add(item);
            Network.Instance.SendTCP("item packet", item.GetBuffItemSpwanData());
            yield return null;
        }

        while (true)
        {
            yield return Suspend.Do(m_coinTime);
            ItemRestoreCheck(m_coinList);
            if (m_coinNumber != m_coinList.Count)
            {
                for (int i = m_coinList.Count; i < m_coinNumber; ++i)
                {
                    if (0 == m_remainTransform.Count) break;
                    int transforem_temp = Random.Range(0, m_remainTransform.Count);
                    Item item = AssetManager.Items.Retrieve("Coin", m_remainTransform[transforem_temp].m_transform) as Item;
                    m_remainTransform[transforem_temp].Item = item;
                    TransformSetting(m_remainTransform[transforem_temp]);
                    m_coinList.Add(item);
                    Network.Instance.SendTCP("item packet", item.GetBuffItemSpwanData());
                    yield return null;
                }
            }
        }
    }
    private IEnumerator<CoroutinePhase> MakeBuffItem()
    {
        yield return Suspend.Do(m_buffStartTime);

        for (int i = m_buffList.Count; i < m_buffNumber; ++i)
        {
            if (0 == m_remainTransform.Count) break;
            int transforem_temp = Random.Range(0, m_remainTransform.Count);
            int buffname_temp = Random.Range(0, m_buffName.Length);
            Item item = AssetManager.Items.Retrieve(m_buffName[buffname_temp], m_remainTransform[transforem_temp].m_transform) as Item;
            m_remainTransform[transforem_temp].Item = item;
            TransformSetting(m_remainTransform[transforem_temp]);
            m_buffList.Add(item);
            Network.Instance.SendTCP("item packet", item.GetBuffItemSpwanData());
            yield return null;
        }

        while (true)
        {
            yield return Suspend.Do(m_buffTime);
            ItemRestoreCheck(m_buffList);
            if (m_buffNumber != m_buffList.Count)
            {
                for (int i = m_buffList.Count; i < m_buffNumber; ++i)
                {
                    if (0 == m_remainTransform.Count) break;
                    int transforem_temp = Random.Range(0, m_remainTransform.Count);
                    int buffname_temp = Random.Range(0, m_buffName.Length);
                    Item item = AssetManager.Items.Retrieve(m_buffName[buffname_temp], m_remainTransform[transforem_temp].m_transform) as Item;
                    m_remainTransform[transforem_temp].Item = item;
                    TransformSetting(m_remainTransform[transforem_temp]);
                    m_buffList.Add(item);
                    Network.Instance.SendTCP("item packet", item.GetBuffItemSpwanData());
                    yield return null;
                }
            }
        }
    }
}