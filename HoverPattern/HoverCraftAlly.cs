using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HoverCraftAlly : DirectionHoverPattern
{
    public GameObject[]         m_original;
    public GameObject           m_brokenObject;
    public GameObject           m_bokenFx;
    public bool                 m_makeCeiling;
    
    private CoroutineCommand    m_coroutineCommand;
    private Direction           m_direction;

    public override void OnInitialize(params object[] parameters)
    {
        m_coroutineCommand = null;
        m_direction = GetComponent<Direction>();
    }

    public override void OnStart()
    {
        m_direction.Finish = false;

        m_coroutineCommand = CoroutineManager.Instance.Register(m_direction.m_custompath[0].Startmove(m_direction.m_custompath[0], this.gameObject));

        StartCoroutine(RotRecovery());
    }

    public override void OnUpdate() { }

    public override void Active() { }

    public override void CountCheck() { }

    public override void OnEnd()
    {
        if(null != m_coroutineCommand)
        {
            CoroutineManager.Instance.Unregister(m_coroutineCommand);
            m_coroutineCommand = null;
        }
    }

    public override void OnEnter(GameObject cameraRig, CameraDirection cameraDirection) { }

    public override void OnExit(GameObject cameraRig) { }

    #region Unity

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == "Rock")
        {
            for(int i = 0; i < m_original.Length; ++i)
            {
                m_original[i].SetActive(false);
            }
            m_brokenObject.SetActive(true);
            m_bokenFx.SetActive(true);

            StartCoroutine(HoverRestore());

            //if (m_makeCeiling) GameData.Instance.HoverCraftSegTwo.MakeCeiling();
        }
    }

    #endregion

    #region Coroutine

    private IEnumerator RotRecovery()
    {
        while (!m_direction.Finish)
        {
            yield return null;
        }

        while(true)
        {
            DirectionRotate(transform);
            if (transform.rotation == Quaternion.Euler(0, 90, 0)) break;
            yield return null;
        }
    }

    private IEnumerator HoverRestore()
    {
        yield return new WaitForSeconds(10f);

        m_direction.Restore();
    }

    private void DirectionRotate(Transform my)
    {
        float t = 0.0f;
        t += Time.deltaTime;
        my.transform.rotation = Quaternion.Slerp(my.transform.rotation, Quaternion.Euler(0,90,0), t);
    }

    #endregion
}