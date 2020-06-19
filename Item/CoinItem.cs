using UnityEngine;
using System.Collections.Generic;

public class CoinItem : Item
{
    private UICoin m_Coin;

    protected override void OnInit(params object[] parameters)
    {
        base.OnInit(parameters);
    }

    protected override void OnStart()
    {
        isUIActive = false;
        AssetManager.Effect.Retrieve("ItemAppearEffect", transform);

        if (!isUIActive)
        {
            isUIActive = true;
            m_Coin = UIManager.Instance.Open("UICoin", true) as UICoin;
            m_Coin.Init("Coin");
        }
    }

    protected override void OnEnd()
    {
        //AssetManager.Effect.Retrieve("FX_Buff_Blue", transform.position);
        AssetManager.Effect.Retrieve("ItemGet", transform.position);

        if (null != m_Coin)
        {
            m_Coin.Restore();
            m_Coin = null;
        }

        base.OnEnd();
    }

    protected override void Active(Transform other)
    {
        GameData.Instance.Player.ItemCoin();
    }

    private void Update()
    {
        if (m_Coin != null && rendTransform != null)
        {
            Vector3 _pos = rendTransform.position + Vector3.up * 0.3f;
            m_Coin.PosUpdate(_pos);
        }
    }
}

