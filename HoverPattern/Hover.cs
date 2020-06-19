using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Hover : Direction
{
    private DirectionHoverPattern m_directionHoverPattern;
    private GameObject m_camerarigobject;

    protected override void DirectingOnInitialize(params object[] parameters)
    {
        base.DirectingOnInitialize(parameters);
        Member = 0;
        m_finish = false;
        m_active = false;
        m_activeend = false;
        m_camerarigobject = null;
        m_directionHoverPattern = GetComponent<DirectionHoverPattern>();

        m_directionHoverPattern.OnInitialize(parameters);
    }

    protected override void Directingsetting()
    {
        m_directionHoverPattern.OnStart();
    }

    protected override void Directingending()
    {
        base.Directingending();
        m_directionHoverPattern.OnEnd();
    }

    public override void Countcheck()
    {
        m_directionHoverPattern.CountCheck();
    }

    public override void DirectionActive()
    {
        m_directionHoverPattern.Active();
    }

    public override void OnEnter(GameObject cameraRig, int number, CameraDirection cameraDirection = null)
    {
        if (!m_active)
        {
            if (null == m_camerarigobject)
            {
                base.OnEnter(cameraRig, number, cameraDirection);
                m_directionHoverPattern.OnEnter(cameraRig, cameraDirection);
                m_camerarigobject = cameraRig;
            }
        }
    }

    public override void OnExit(GameObject camerarRig, int number)
    {
        if (!m_active)
        {
            if (camerarRig == m_camerarigobject)
            {
                base.OnExit(camerarRig, number);
                m_directionHoverPattern.OnExit(camerarRig);
                m_camerarigobject = null;
            }
        }
    }

    #region unity

    private void LateUpdate()
    {
        m_directionHoverPattern.OnUpdate();
    }

    #endregion
}