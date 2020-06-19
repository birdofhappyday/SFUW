using UnityEngine;
using System.Collections.Generic;

public class BuffItem : Item
{
    public string m_uiName;
    public GunBuffType m_gunBuffType;

    private UIWeaponPack m_uiweaponPack;
    private CoroutineCommand m_coroutine;

    protected override void OnInit(params object[] parameters)
    {
        base.OnInit(parameters);
    }

    protected override void OnStart()
    {
        AssetManager.Effect.Retrieve("ItemAppearEffect", transform);
        m_uiweaponPack = UIManager.Instance.Open("UIWeaponPack", true) as UIWeaponPack;
        m_uiweaponPack.Init(m_uiName);
        m_coroutine = CoroutineManager.Instance.Register(Coroutine(0.1f));
    }

    protected override void OnEnd()
    {
        //AssetManager.Effect.Retrieve("FX_Buff_Blue", transform.position);
        AssetManager.Effect.Retrieve("ItemGet", transform.position);
        if (null != m_coroutine)
        {
            CoroutineManager.Instance.Unregister(m_coroutine);
        }
        if (null != m_uiweaponPack)
        {
            m_uiweaponPack.Restore();
        }
        base.OnEnd();
    }

    protected override void Active(Transform other) //Item클래스의 OnTriggerEnter에서 들어옴. ItemType이 buff일때 그리고 isBuffon이 false일때 이걸 실행하고 아이템을 Restore 시킴.
    {
        Rifle temp = GameData.Instance.Player.CurrentWeapon as Rifle;
        if (null != temp)
        {
            temp.SetGunReinforce(m_gunBuffType);
        }
    }

    IEnumerator<CoroutinePhase> Coroutine(float time)
    {
        yield return Suspend.Do(time);
        Transform _childTrans = transform.GetComponentInChildren<Animator>().transform;
        while (true)
        {
            Vector3 _pos = _childTrans.position;
            _pos.y += 0.3f;
            m_uiweaponPack.PosUpdate(_pos);
            yield return null;
        }
    }
}

