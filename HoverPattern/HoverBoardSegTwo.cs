using UnityEngine;
using System.Collections.Generic;

public class HoverBoardSegTwo : DirectionHoverPattern
{
    private CoroutineCommand m_order;
    private CoroutineCommand m_movecoroutine;
    private CoroutineCommand m_emergency;
    private Animator m_animator;
    private Direction m_direction;
    private UIRide _ride;

    public Transform[] m_turbinePos;

    public override void OnInitialize(params object[] parameters)
    {
        m_direction = GetComponent<Direction>();
        m_animator = GetComponent<Animator>();
        for (int i = 0; i < m_direction.m_directionCountCheck.Length; ++i)
        {
            m_direction.m_directionCountCheck[i].gameObject.SetActive(false);
        }

        if ("hoverboardSeg2_1" == gameObject.name)
        {
            GameData.Instance.Hover1 = m_direction;
            AssetManager.Items.RestoreAllAsset();
        }
        else if ("hoverboardSeg2_2" == gameObject.name)
        {
            GameData.Instance.Hover2 = m_direction;
        }
    }

    public override void OnStart()
    {
        m_order = CoroutineManager.Instance.Register(Appear());
    }

    public override void OnUpdate() { }

    public override void Active()
    {
        if (UserType.Host == GameData.Instance.UserType)
        {
            if (null != m_emergency)
            {
                CoroutineManager.Instance.Unregister(m_emergency);
            }
        }

        m_direction.Active = true;
        m_animator.SetBool("Active", true);
        m_direction.Finish = false;

        for (int i = 0; i < m_direction.m_directionCountCheck.Length; ++i)
        {
            m_direction.m_directionCountCheck[i].gameObject.SetActive(false);
        }

        if (_ride != null)
        {
            if (AssetState.Using == _ride.AssetState)
            {
                _ride.Restore();
            }
        }

        if (GetComponent<Direction>() == GameData.Instance.Hover1)
        {
            GameData.Instance.m_infiniteMap.m_moveDirection = Vector3.down;
            GameData.Instance.m_infiniteMap.m_moveSpeed = 0.5f;
            GameData.Instance.HoverCraftSegTwo.HoverCraftFall();

        }
    }

    public override void CountCheck()
    {
        if (DirectionManager.Instance.BothMember == PlayerManager.Instance.GetPlayerList.Count)
        {
            DirectionManager.Instance.ActiveBothMember();
        }
    }

    public override void OnEnd()
    {
        if (null != m_order)
        {
            CoroutineManager.Instance.Unregister(m_order);
        }
        m_order = null;

        if (null != m_movecoroutine)
        {
            CoroutineManager.Instance.Unregister(m_movecoroutine);
        }
        m_movecoroutine = null;

        GameData.Instance.Hover1 = null;
        GameData.Instance.Hover2 = null;
    }

    public override void OnEnter(GameObject cameraRig, CameraDirection cameraDirection)
    {
        if (AssetState.Using == _ride.AssetState)
        {
            _ride.Restore();
        }
    }

    public override void OnExit(GameObject cameraRig)
    {
        if (AssetState.Waiting == _ride.AssetState)
        {
            _ride = UIManager.Instance.Open("UIRide", true) as UIRide;
            Vector3 _vec = transform.position;
            _vec.y += 1f;
            _ride.transform.position = _vec;
        }
    }

    #region 사용함수.

    private IEnumerator<CoroutinePhase> Appear()
    {
        m_animator.SetBool("Active", true);

        m_direction.Finish = false;
        m_movecoroutine = CoroutineManager.Instance.Register(m_direction.m_custompath[0].PutOnPath(transform, m_direction.m_custompath[0], false));

        while (!m_direction.Finish)
        {
            yield return null;
        }

        m_direction.Finish = false;
        m_movecoroutine = CoroutineManager.Instance.Register(m_direction.m_custompath[1].PutOnPath(transform, m_direction.m_custompath[1], false));

        for (int i = 0; i < m_turbinePos.Length; ++i)
        {
            AssetManager.Effect.Retrieve("FX_BoardLanding", m_turbinePos[i]);
        }

        yield return Suspend.Do(1.0f);

        AssetManager.Effect.Retrieve("DirectionSound_Hoverlanding", transform);

        while (!m_direction.Finish)
        {
            yield return null;
        }

        _ride = UIManager.Instance.Open("UIRide", true) as UIRide;
        Vector3 _vec = transform.position;
        _vec.y += 1f;
        _ride.transform.position = _vec;

        if ("hoverboardSeg2_1" == gameObject.name)
        {
            if (UserType.Host == GameData.Instance.UserType)
            {
                m_emergency = CoroutineManager.Instance.Register(m_direction.Emergency(m_emergencyTime, true));
            }
        }

        for (int i = 0; i < m_direction.m_directionCountCheck.Length; ++i)
        {
            m_direction.m_directionCountCheck[i].gameObject.SetActive(true);
        }
    }
    #endregion
}

