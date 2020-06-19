using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class HoverBoardSegOne : DirectionHoverPattern
{
    private CoroutineCommand m_order;
    private CoroutineCommand m_movecoroutine;
    private CoroutineCommand m_emergency;
    private Animator m_animator;
    private Direction m_direction;
    private HoverEnum hovertype;

    public Transform[] landingEffectPos;
    private UIRide _ride;

    #region 상속함수

    public override void OnInitialize(params object[] parameters)
    {
        m_direction = GetComponent<Direction>();
        m_animator = GetComponent<Animator>();
        for (int i = 0; i < m_direction.m_directionCountCheck.Length; ++i)
        {
            m_direction.m_directionCountCheck[i].gameObject.SetActive(false);
        }
    }

    public override void OnStart()
    {
        m_animator.SetBool("Active", true);

        if (gameObject.name == "hoverboardSeg1_1")
        {
            hovertype = HoverEnum.hoverBoard1;
            GameData.Instance.Hover1 = GetComponent<Direction>();
            AssetManager.Items.RestoreAllAsset();
        }
        else if (gameObject.name == "hoverboardSeg1_2")
        {
            hovertype = HoverEnum.hoverBoard2;
            GameData.Instance.Hover2 = GetComponent<Direction>();
        }

        m_order = CoroutineManager.Instance.Register(Appear());
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

    public override void Active()
    {
        m_animator.SetBool("Active", true);

        if (UserType.Host == GameData.Instance.UserType)
        {
            if (null != m_emergency)
            {
                CoroutineManager.Instance.Unregister(m_emergency);
            }
        }
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

        for (int i = 0; i < landingEffectPos.Length; ++i)
        {
            AssetManager.Effect.Retrieve("FX_BoardLanding", landingEffectPos[i]); //터빈에서 나올 화염이펙트
        }

        AssetManager.Effect.Retrieve("DirectionSound_HoverDepart", transform);

        if (hovertype == HoverEnum.hoverBoard1)
        {
            GameData.Instance.Map.Seg1HoverMove();
        }
    }

    public override void CountCheck()
    {
        if (DirectionManager.Instance.BothMember == PlayerManager.Instance.GetPlayerList.Count)
        {
            DirectionManager.Instance.ActiveBothMember();
        }
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

    public override void OnUpdate() { }

    #endregion

    #region 사용함수

    public void HoverEndActive()
    {
        StartCoroutine(HoverEnd());
    }

    private IEnumerator<CoroutinePhase> Appear()
    {
        m_direction.Finish = false;
        m_movecoroutine = CoroutineManager.Instance.Register(m_direction.m_custompath[0].PutOnPath(transform, m_direction.m_custompath[0], false));

        while (!m_direction.Finish)
        {
            yield return null;
        }

        CoroutineManager.Instance.Unregister(m_movecoroutine);
        m_movecoroutine = null;
        /////////////////////////////////////위까지 호버 첫번째 패스/////////////////////////////////////////////////////////////

        for (int i = 0; i < landingEffectPos.Length; ++i)
        {
            AssetManager.Effect.Retrieve("FX_BoardLanding", landingEffectPos[i]); //터빈에서 나올 화염이펙트
        }

        switch (hovertype)
        {
            case HoverEnum.hoverBoard1:
                AssetManager.Effect.Retrieve("FX_BoardLandingDust", GameData.Instance.hoverLandingPos[0]);
                break;
            case HoverEnum.hoverBoard2:
                AssetManager.Effect.Retrieve("FX_BoardLandingDust", GameData.Instance.hoverLandingPos[1]);
                break;
        }

        yield return Suspend.Do(0.5f);
        /////////////////////////////////////위까지 하강연출/////////////////////////////////////////////////////////////////////////////////////////////
        m_direction.Finish = false;
        m_movecoroutine = CoroutineManager.Instance.Register(m_direction.m_custompath[1].PutOnPath(transform, m_direction.m_custompath[1], false));

        yield return Suspend.Do(4.2f);

        AssetManager.Effect.Retrieve("DirectionSound_Hoverlanding", transform);

        while (!m_direction.Finish)
        {
            yield return null;
        }

        CoroutineManager.Instance.Unregister(m_movecoroutine);
        m_movecoroutine = null;
        ////////////////////////////////////위까지 마지막 등장path///////////////////////////////////////////////////////////////////////////////////////////////////////////
        m_animator.SetBool("Active", false);

        _ride = UIManager.Instance.Open("UIRide", true) as UIRide;
        Vector3 _vec = transform.position;
        _vec.y += 1f;
        _ride.transform.position = _vec;

        if (gameObject.name == "hoverboardSeg1_1")
        {
            DirectionManager.Instance.OpenNarration(4011);
            if (UserType.Host == GameData.Instance.UserType)
            {
                m_emergency = CoroutineManager.Instance.Register(m_direction.Emergency(m_emergencyTime, true));
            }
        }

        for (int i = 0; i < m_direction.m_directionCountCheck.Length; ++i)
        {
            m_direction.m_directionCountCheck[i].gameObject.SetActive(true);
        }

        GetComponent<AudioSource>().volume = 0.0f;
    }

    private IEnumerator HoverEnd()
    {
        m_animator.SetBool("Active", false);
        m_direction.Active = false;

        m_direction.PlayFadeOut(m_direction.m_fadepattern[0], m_direction);

        while (!m_direction.Finish)
        {
            yield return null;
        }

        if (m_direction.gameObject.activeSelf)
        {
            m_direction.Restore();
        }
    }

    #endregion
}

