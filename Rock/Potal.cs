using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public enum RockType
{
    Seg1,
    Seg2,
    Armageddon,
    Rock,
}

public class Potal : Direction
{
    public RockType m_potalType;
    public string rock;
    public Transform m_transformPos;
    public GameObject openPotal;
    public GameObject closePotal;
    public GameObject potalFX;

    private CoroutineCommand m_coroutine;
    private int pathnum;
    private Effect loopSound = null;

    protected override void DirectingOnInitialize(params object[] parameters)
    {
        base.DirectingOnInitialize(parameters);
    }

    protected override void Directingsetting()
    {
        pathnum = 0;
        openPotal.SetActive(true);
        closePotal.SetActive(false);
        AssetManager.Effect.Retrieve("DirectionSound_PotalOpen", gameObject.transform);

        if (SceneManager.GetActiveScene().name == GameData.Instance.Segment02)
        {
            m_potalType = RockType.Seg2;
            potalFX.SetActive(false);
            transform.localScale = new Vector3(5f, 5f, 5f);
        }
        else
        {
            m_potalType = RockType.Seg1;
        }

        switch (m_potalType)
        {
            case RockType.Seg1:
                m_coroutine = CoroutineManager.Instance.Register(RockMove());
                break;

            case RockType.Seg2:
                m_coroutine = CoroutineManager.Instance.Register(RockMoveSegTwo());
                break;
        }
    }

    protected override void Directingending()
    {
        if (null != m_coroutine)
        {
            CoroutineManager.Instance.Unregister(m_coroutine);
            m_coroutine = null;
        }
    }

    private IEnumerator<CoroutinePhase> RockMove()
    {
        yield return Suspend.Do(6f);
        AssetManager.Direction.Retrieve("Armageddon", m_transformPos, m_custompath[pathnum]);
        loopSound = AssetManager.Effect.Retrieve("DirectionSound_PortalLoop", transform);
        yield return Suspend.Do(10f);

        while (true)
        {
            yield return Suspend.Do(3f);
            ++pathnum;
            AssetManager.Direction.Retrieve(rock, m_transformPos, m_custompath[pathnum]);
        }
    }

    private IEnumerator<CoroutinePhase> RockMoveSegTwo()
    {
        int rockCount = 0;

        Quaternion rot = transform.localRotation;

        yield return Suspend.Do(5f);
        loopSound = AssetManager.Effect.Retrieve("DirectionSound_PortalLoop", transform);
        pathnum = 11;

        while (rockCount < 65)
        {
            yield return Suspend.Do(1f);
            //pathnum = Random.Range(11, 36);

            switch(rockCount)
            {
                case 2:
                    DirectionManager.Instance.OpenNarration(5002);
                    break;
                case 3:
                    AssetManager.Effect.Retrieve("Direction_ceiling_sound", GameData.Instance.HoverCraftSegTwo.transform);
                    break;
                case 4:
                    GameData.Instance.HoverCraftSegTwo.MakeCeiling();
                    break;
            }
            
            AssetManager.Direction.Retrieve(rock, m_transformPos, rot, m_custompath[pathnum]);
            ++rockCount;
            ++pathnum;
        }

        ///아래부터 포탈이 닫힐때///
        yield return Suspend.Do(2f);

        openPotal.SetActive(false);
        closePotal.SetActive(true);

        if (loopSound != null)
        {
            loopSound.Restore();
        }

        yield return Suspend.Do(5f);

        Restore();
    }
}
