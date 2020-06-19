using UnityEngine;
using System.Collections.Generic;

public class HoverCraftSegOne : DirectionHoverPattern
{
    public GameObject m_ceilingObject;
    public GameObject m_projecttile;

    [Tooltip("다 타고 출발에 걸리는 시간")]
    public float m_activeTime;

    private CoroutineCommand m_order;
    private CoroutineCommand m_movecoroutine;
    private CoroutineCommand m_hostcheck;
    private Direction m_direction;
    private bool m_hostEndCheck;

    #region 상속함수

    public override void OnInitialize( params object[] parameters )
    {
        m_direction = GetComponent<Direction>();
        m_ceilingObject.SetActive(false);
        m_projecttile.SetActive(false);
        m_hostEndCheck = false;
        //ItemMananger.Instance.AllitemRestore();
    }

    public override void OnStart()
    {
        m_order = CoroutineManager.Instance.Register(Appear());
    }

    public override void OnEnd()
    {
        if(null != m_order)
        {
            CoroutineManager.Instance.Unregister(m_order);
            m_order = null;
        }
    }

    public override void OnEnter(GameObject cameraRig, CameraDirection cameraDirection)
    {
        //if(UserType.Host == GameData.Instance.UserType)
        //{
        //    m_emergency = CoroutineManager.Instance.Register(m_direction.Emergency(10f, false, m_direction));
        //}
    }

    public override void OnExit( GameObject cameraRig ) { }

    public override void Active()
    {
        if (m_direction.Active) return;

        m_direction.Active = true;

        if (UserType.Host == GameData.Instance.UserType)
        {
            //if (null != m_emergency)
            //{
            //    CoroutineManager.Instance.Unregister(m_emergency);
            //}

            if (null != m_hostcheck)
            {
                CoroutineManager.Instance.Unregister(m_hostcheck);
            }
        }

        if (null != m_order)
        {
            CoroutineManager.Instance.Unregister(m_order);
            m_order = null;
        }
        
        m_order = CoroutineManager.Instance.Register(ActiveOrder());
    }

    public override void CountCheck()
    {
        if(m_direction.Member == PlayerManager.Instance.GetPlayerList.Count)
        {
            m_hostcheck = CoroutineManager.Instance.Register(HoverHostCheck());
        }
    }

    public override void OnUpdate()
    {
        if (GameData.Instance.UseNetwork)
        {
            if (null != GameData.Instance.Hover1 && UserType.Host != GameData.Instance.UserType)
            {
                if (GameData.Instance.Hover1.ActiveEnd)
                {
                    GameData.Instance.Hover1.ActiveEnd = false;
                    m_direction.OnEnter(GameData.Instance.CameraRig, -1);
                }
            }

            else if (null != GameData.Instance.Hover1 && UserType.Host == GameData.Instance.UserType)
            {
                if (GameData.Instance.Hover1.ActiveEnd)
                {
                    GameData.Instance.Hover1.ActiveEnd = false;
                    m_hostEndCheck = true;
                }
            }
        }

        else
        {
            if (null != GameData.Instance.Hover1)
            {
                if (GameData.Instance.Hover1.ActiveEnd)
                {
                    GameData.Instance.Hover1.ActiveEnd = false;
                    Active();
                }
            }
        }
    }

    #endregion

    #region 사용함수

    private IEnumerator<CoroutinePhase> Appear()
    {
        m_direction.Finish = false;
        m_movecoroutine = CoroutineManager.Instance.Register(m_direction.m_custompath[0].Startmove(m_direction.m_custompath[0], gameObject));

        while(!m_direction.Finish)
        {
            yield return null;
        }
        CoroutineManager.Instance.Unregister(m_movecoroutine);

        HoverMake();

        transform.SetParent(GameData.Instance.Map.transform);
    }

    private IEnumerator<CoroutinePhase> ActiveOrder()
    {
        DirectionManager.Instance.OpenNarration(4009);
        m_direction.SetParentInit();
        m_ceilingObject.SetActive(true);
        AssetManager.Effect.Retrieve("DirectionSound_HoverShield", m_ceilingObject.transform);
        yield return Suspend.Do(m_activeTime);
        AssetManager.Effect.Retrieve("DirectionSound_HoverAccel", transform);
        GameData.Instance.Map.Move(1);
    }

    private void HoverMake()
    {
        if(GameData.Instance.UserType != UserType.Host)
            return;

        Direction hover1 = AssetManager.Direction.Retrieve("hoverboardSeg1_1") as Direction;
        Network.Instance.SendTCP("direction packet", hover1.GetDirectionSpawanData());

        Direction hover2 = AssetManager.Direction.Retrieve("hoverboardSeg1_2") as Direction;
        Network.Instance.SendTCP("direction packet", hover2.GetDirectionSpawanData());
    }

    private IEnumerator<CoroutinePhase> HoverHostCheck()
    {
        while(true)
        {
            if (m_hostEndCheck) break;
            yield return null;
        }

        m_direction.SendDirectionHoverActive();
    }

    private void ProjectTile()
    {
        if(m_projecttile != null)
        {
            m_projecttile.SetActive(false);
            m_projecttile.SetActive(true);
        }
    }

    #endregion
}
