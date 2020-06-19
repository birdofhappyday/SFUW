using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum HoverCraftSegTwoTranoAttackDisplay
{
    Right,
    Left,
}

public class HoverCraftSegTwo : DirectionHoverPattern, IEventDirection
{
    public CameraShake m_cameraShake;
    public Transform bombHoverPos;
    public Transform[] m_bombingPos;
    public GameObject m_smoke;
    public GameObject m_ceiling;
    public GameObject m_wind;
    public GameObject m_fracObject;
    public GameObject m_originObject;
    public GameObject[] m_ceilingfx;
    public GameObject[] m_engine_Booster;
    public GameObject[] m_explosion;
    public GameObject[] m_attackedFx;

    //private int m_directionOrder;
    //private bool m_rockFirst;
    private bool m_tyranopodeFireEnter;
    private Direction m_direction;
    private CoroutineCommand m_coroutineCommand;

    public void DirectionAction()
    {
        OutCeiling();
    }

    public override void OnInitialize(params object[] parameters)
    {
        //m_directionOrder = 0;
        m_tyranopodeFireEnter = true;
        m_originObject.SetActive(true);
        m_fracObject.SetActive(false);
        m_direction = GetComponent<Direction>();

        GameData.Instance.HoverCraftSegTwo = this;
    }

    public override void OnStart()
    {
        EventManager.Instance.AddReceiver<IEventDirection>(this);
    }

    public override void OnUpdate() { }

    public override void Active() { }

    public override void CountCheck() { }

    public override void OnEnd()
    {
        GameData.Instance.HoverCraftSegTwo = null;
        if (m_coroutineCommand != null)
        {
            CoroutineManager.Instance.Unregister(m_coroutineCommand);
            m_coroutineCommand = null;
        }
        EventManager.Instance.RemoveReceiver<IEventDirection>(this);
    }

    public override void OnEnter(GameObject cameraRig, CameraDirection cameraDirection) { }

    public override void OnExit(GameObject cameraRig) { }

    #region 사용함수

    public void RocketBombingReady()
    {
        //AssetManager.Direction.Retrieve("hovercraftBombing", bombHoverPos);

        Projectile[] temp;

        temp = new Projectile[m_bombingPos.Length];

        for (int i = 0; i < m_bombingPos.Length; ++i)
        {
            temp[i] = AssetManager.Projectile.Retrieve("DirectionSupportBomb", m_bombingPos[i], gameObject);
        }

        for (int i = 0; i < temp.Length; ++i)
        {
            temp[i].Restore();
        }
    }

    public void RocketBombing()
    {
        AssetManager.Direction.Retrieve("hovercraftBombing");
        StartCoroutine(SupportBombing());
    }

    public void HoverCraftFall()
    {
        m_smoke.SetActive(true);

        StartCoroutine(HoverCraftFallBlend());

        for (int i = 0; i < m_engine_Booster.Length; ++i)
        {
            m_engine_Booster[i].SetActive(false);
        }
        StartCoroutine(FallRotate());
        StartCoroutine(Explosion());
        m_coroutineCommand = CoroutineManager.Instance.Register(m_direction.m_custompath[0].Startmove(m_direction.m_custompath[0], this.gameObject));
    }
    #endregion

    #region Coroutine

    private IEnumerator HoverCraftFallBlend()
    {
        AudioSource audio = GetComponent<AudioSource>();

        while (audio.spatialBlend <= 1.0f)
        {
            audio.spatialBlend += 0.01f;

            yield return null;
        }
    }

    private IEnumerator SupportBombing()
    {
        DirectionManager.Instance.OpenNarration(5019);

        Projectile rocket = null;

        yield return new WaitForSeconds(3f);

        for (int i = 0; i < m_bombingPos.Length; ++i)
        {
            rocket = AssetManager.Projectile.Retrieve("DirectionSupportBomb", m_bombingPos[i], gameObject);

            if(i == m_bombingPos.Length / 2)
            {
                Effect sound = AssetManager.Effect.Retrieve("DirectionSound_SupportBombing");

                sound.transform.position = new Vector3(30f, 23.51f, 0);
            }

            yield return new WaitForSeconds(0.3f);
        }
    }

    private IEnumerator FallRotate()
    {
        yield return new WaitForSeconds(1f);

        while (transform.eulerAngles.x < 66 || transform.eulerAngles.x > 300)
        {
            Vector3 temp = transform.eulerAngles;
            temp.x += 0.1f;
            temp.z += 0.1f;
            transform.eulerAngles = temp;
            yield return null;
        }
    }

    private IEnumerator Explosion()
    {
        yield return new WaitForSeconds(3f);

        for (int i = 0; i < m_explosion.Length - 1; ++i)
        {
            m_explosion[i].SetActive(true);
            //AssetManager.Effect.Retrieve("DirectionSound_SupportBombing2", transform);
            yield return new WaitForSeconds(3f);
        }

        GameData.Instance.m_infiniteMap.m_moveDirection = Vector3.up;
        GameData.Instance.m_infiniteMap.m_moveSpeed = 2.0f;

        for (int i = 0; i < m_explosion.Length - 1; ++i)
        {
            m_explosion[i].SetActive(false);
            yield return null;
        }

        for (int i = 0; i < GameData.Instance.HoverCraftSegTwo.m_attackedFx.Length; ++i)
        {
            m_attackedFx[i].SetActive(false);

            yield return null;
        }

        m_smoke.SetActive(false);

        AssetManager.Effect.Retrieve("DirectionSound_HoverExplosion_02", transform.position);

        yield return new WaitForSeconds(0.4f);

        AssetManager.Effect.Retrieve("HoverCraft_Explosion", m_explosion[m_explosion.Length - 1].transform.position);

        yield return new WaitForSeconds(0.4f);

        AssetManager.Effect.Retrieve("DirectionSound_HoverExplosion_01", transform.position);

        m_originObject.SetActive(false);
        m_fracObject.SetActive(true);
        //AssetManager.Effect.Retrieve("DirectionSound_BuildingExplosion", transform);

        if (null != m_coroutineCommand)
        {
            CoroutineManager.Instance.Unregister(m_coroutineCommand);
        }
    }

    private void Acceleration()
    {
        AssetManager.Effect.Retrieve("DirectionSound_HoverAccel", transform.position);
        //AssetManager.Effect.Retrieve("DirectionSound_HoverWind", m_wind.transform);
        StartCoroutine(GameData.Instance.m_infiniteMap.Accel(25f, 3f));
    }

    private IEnumerator CeilingSpark()
    {
        yield return new WaitForSeconds(10f);

        for (int i = 0; i < m_ceilingfx.Length; ++i)
        {
            m_ceilingfx[i].SetActive(false);

            yield return new WaitForSeconds(2f);
        }
    }

    public void MakeCeiling()
    {
        m_ceiling.SetActive(true);
    }

    public void OutCeiling()
    {
        AssetManager.Effect.Retrieve("DirectionSound_DetachShild", transform);

        m_ceiling.SetActive(false);

        for (int i = 0; i < m_ceilingfx.Length; ++i)
        {
            m_ceilingfx[i].SetActive(true);
        }

        StartCoroutine(CeilingSpark());

        m_cameraShake.Active();

        DirectionManager.Instance.OpenNarration(5006);

        Invoke("Acceleration", 4.0f);
    }

    public void HoverCraftDamageFX()
    {
        for (int i = 0; i < m_attackedFx.Length; ++i)
        {
            AudioSource audio = m_attackedFx[i].transform.GetComponent<AudioSource>();

            if(audio != null)
            {
                audio.volume = 1.0f;
            }
        }
    }

    #endregion

    #region Unity

    private void OnTriggerEnter(Collider other)
    {
        if (m_tyranopodeFireEnter)
        {
            if ("TyranopodeFireCollider" == other.name)
            {
                m_tyranopodeFireEnter = false;

                for (int i = 0; i < m_attackedFx.Length; ++i)
                {
                    m_attackedFx[i].SetActive(true);
                }
            }
        }
    }
    #endregion
}