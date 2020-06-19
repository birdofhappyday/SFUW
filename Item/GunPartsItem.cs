using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GunPartsItem : Item
{
    public string m_uiName;
    public string moveItem = null;
    public GunPartsType m_gunPartsType;

    private UIWeaponPack m_uiweaponPack = null;

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
            switch(m_gunPartsType)
            {
                case GunPartsType.FireParts:
                    m_uiweaponPack = UIManager.Instance.Open("UIWeaponFire", true) as UIWeaponPack;
                    break;
                case GunPartsType.GrenadeParts:
                    m_uiweaponPack = UIManager.Instance.Open("UIWeaponGrenade", true) as UIWeaponPack;
                    break;
                case GunPartsType.LaserParts:
                    m_uiweaponPack = UIManager.Instance.Open("UIWeaponPack", true) as UIWeaponPack;
                    break;
            }
            m_uiweaponPack.Init(m_uiName);
        }
    }

    protected override void OnEnd()
    {
        //AssetManager.Effect.Retrieve("FX_Buff_Blue", transform.position);
        AssetManager.Effect.Retrieve("ItemGet", transform.position);
        
        if (null != m_uiweaponPack)
        {
            m_uiweaponPack.Restore();
            m_uiweaponPack = null;
        }

        base.OnEnd();
    }

    protected override void Active(Transform other)
    {
        GunParts item = AssetManager.Items.Retrieve(moveItem, transform, m_gunPartsType, GameData.Instance.Player) as GunParts;
        Network.Instance.SendTCP("item packet", item.MovePartsSpawn());
    }

    private void Update()
    {
        if (m_uiweaponPack != null && rendTransform != null)
        {
            Vector3 _pos = rendTransform.position + Vector3.up * 0.3f;
            m_uiweaponPack.PosUpdate(_pos);
        }
    }
}