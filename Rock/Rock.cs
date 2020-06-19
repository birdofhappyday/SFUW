using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class Rock : Direction
{
    public RockType m_rockType;
    public GameObject m_physicsObjects;
    public float m_restoreTime;
    public GameObject m_effect = null;

    private Quaternion rot;
    private Rigidbody m_rig;
    private MeshRenderer m_mshRend;
    private CoroutineCommand m_coroutineMove;
    private CoroutineCommand m_coroutineRestortime;

    protected override void DirectingOnInitialize(params object[] parameters)
    {
        if (RockType.Armageddon == m_rockType) return;

        foreach (object element in parameters)
        {
            if (element is CustomPath)
            {
                m_custompath[0] = (CustomPath)element;
            }
            else if (element is Quaternion)
            {
                rot = (Quaternion)element;
            }
        }

        m_coroutineRestortime = null;
        m_physicsObjects.SetActive(false);
        m_mshRend = GetComponent<MeshRenderer>();
        m_mshRend.enabled = true;
        m_rig = GetComponent<Rigidbody>();
        m_rig.isKinematic = false;
    }

    protected override void Directingsetting()
    {
        //Effect soundEffect = AssetManager.Effect.Retrieve("DirectionSound_Rockfall", gameObject.transform);

        if (SceneManager.GetActiveScene().name == GameData.Instance.Segment01 && RockType.Rock == m_rockType)
        {
            gameObject.GetComponent<AudioSource>().enabled = false;
        }

        if (SceneManager.GetActiveScene().name == GameData.Instance.Segment02)
        {
            transform.rotation = rot;
            gameObject.GetComponent<AudioSource>().pitch = Random.Range(0.7f, 1.3f);
            StartCoroutine(SegTwo());
            return;
        }

        if (RockType.Armageddon == m_rockType)
        {
            transform.eulerAngles = new Vector3(130, 90, 90);
            GetComponent<AudioSource>().enabled = false;
            transform.SetParent(GameData.Instance.Map.transform);
            StartCoroutine(ArmageddonSound());
        }

        m_finish = false;
        m_coroutineMove = CoroutineManager.Instance.Register(m_custompath[0].Startmove(m_custompath[0], gameObject));
    }
    
    protected override void Directingending()
    {
        if (null != m_coroutineMove)
        {
            CoroutineManager.Instance.Unregister(m_coroutineMove);
            m_coroutineMove = null;
        }

        if (null != m_coroutineRestortime)
        {
            CoroutineManager.Instance.Unregister(m_coroutineRestortime);
            m_coroutineRestortime = null;
        }
    }

    public IEnumerator ArmageddonSound()
    {
        while (transform.position.y >= 90f)
        {
            yield return null;
        }

        GetComponent<AudioSource>().enabled = true;
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    switch (m_rockType)
    //    {
    //        case RockType.Armageddon:
    //            if (isFirst)
    //            {
    //                if (collision.gameObject.name == "Rooftop")
    //                {
    //                    RockCrash();
    //                    isFirst = false;
    //                }
    //            }
    //            break;

    //        case RockType.Rock:
    //            if (collision.transform.tag == "Map" || collision.transform.tag == "Direction")
    //            {
    //                RockCrash();
    //            }
    //            break;
    //    }
    //}

    public void OnTriggerEnter(Collider other)
    {
        switch(m_rockType)
        {
            case RockType.Rock:
                if (other.CompareTag("Map") || other.CompareTag("Monster") || other.CompareTag("Direction"))
                {
                    RockCrash();
                }
                break;

            case RockType.Armageddon:
                if (other.gameObject.name == "Rooftop")
                {
                    RockCrash();
                }
                break;
        }
    }

    public void RockCrash()
    {
        if (RockType.Rock == m_rockType)
        {
            m_physicsObjects.SetActive(true);

            CoroutineManager.Instance.Unregister(m_coroutineMove);
            m_mshRend.enabled = false;
            
            Effect m_dust = AssetManager.Effect.Retrieve("RockDust");
            m_dust.transform.position = this.transform.position;
            m_dust.transform.rotation = this.transform.rotation;

            m_rig.useGravity = true;
            m_rig.isKinematic = true;

            CoroutineManager.Instance.Register(RestorTime(m_restoreTime));

            return;
        }

        if (m_effect != null)
        {
            m_effect.SetActive(false);
        }
    }

    private IEnumerator<CoroutinePhase> RestorTime(float time)
    {
        yield return Suspend.Do(time);
        base.Restore();
    }

    public IEnumerator SegTwo()
    {
        m_finish = false;
        m_coroutineMove = CoroutineManager.Instance.Register(m_custompath[0].Startmove(m_custompath[0], gameObject));
        
        while (!m_finish)
        {
            yield return null;
        }

        m_rig.useGravity = true;

        RockCrash();
    }
}