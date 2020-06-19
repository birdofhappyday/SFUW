using System.Collections.Generic;
using UnityEngine;

public class RandomHealPack : Item
{
    public string m_uiName;

    private float m_hp = 100;
    private UIHealPack m_uiHealPack;

    #region 프로퍼티

    public float HP
    {
        get { return m_hp; }
        set { m_hp = value; }
    }

    #endregion

    protected override void OnInit(params object[] parameters)
    {
        base.OnInit(parameters);

        foreach (object element in parameters)
        {
            if (element is float)
            {
                m_hp = (float)element;
            }
        }
    }

    protected override void OnStart()
    {
        isUIActive = false;
        rendTransform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        rendTransform.localScale *= (m_hp * 0.01f);

        AssetManager.Effect.Retrieve("ItemAppearEffect", rendTransform);

        if (!isUIActive)
        {
            isUIActive = true;

            m_uiHealPack = UIManager.Instance.Open("UIHealPack", true) as UIHealPack;

            if (m_uiHealPack != null)
            {
                m_uiHealPack.Init(m_uiName);
            }
            else
            {
                Debug.LogWarning(name + " ui null");
            }
        }
    }

    protected override void OnEnd()
    {
        //AssetManager.Effect.Retrieve("FX_Buff_Blue", transform.position);
        AssetManager.Effect.Retrieve("ItemGet", transform.position);
        
        if (null != m_uiHealPack)
        {
            m_uiHealPack.Restore();
            m_uiHealPack = null;
        }

        base.OnEnd();
    }

    protected override void Active(Transform other)
    {
        GameData.Instance.Player.ItemHealth(m_hp);
    }

    private void Update()
    {
        if (m_uiHealPack != null && rendTransform != null)
        {
            Vector3 _pos = rendTransform.position + Vector3.up * 0.3f;
            m_uiHealPack.PosUpdate(_pos);
        }
    }
}

