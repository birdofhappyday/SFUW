using UnityEngine;
using System.Collections.Generic;

public class PlayerPositionSetting : DirectionHoverPattern
{
    private Direction m_direction;
    private CoroutineCommand m_coroutine;
    private UIMovement m_UIMovement;

    #region 상속함수

    public override void OnInitialize(params object[] parameters)
    {
        m_direction = GetComponent<Direction>();
    }

    public override void OnStart()
    {
        if (UserType.Host == GameData.Instance.UserType)
        {
            m_coroutine = CoroutineManager.Instance.Register(Emergency());
        }
        else
        {
            m_UIMovement = UIManager.Instance.Open("UIMovement") as UIMovement;
        }

        Invoke("CheckActive", 1f);
    }

    public override void CountCheck()
    {
        if (m_direction.Member == PlayerManager.Instance.GetPlayerList.Count)
        {
            m_direction.SendDirectionHoverActive();
        }
    }

    public override void Active()
    {
        CheckEnd();

        if (UserType.Host == GameData.Instance.UserType)
        {
            m_direction.Active = true;
            TriggerActionManager.Instance.m_triggerState = TriggerState.Go;
            CoroutineManager.Instance.Unregister(m_coroutine);
        }

        if (UserType.User == GameData.Instance.UserType)
        {
            m_UIMovement.Restore();
        }

        m_direction.Restore();
    }

    public override void OnEnd() { }

    public override void OnEnter(GameObject cameraRig, CameraDirection cameraDirection)
    {
        if (UserType.User == GameData.Instance.UserType)
        {
            m_UIMovement.Restore();
        }
    }

    public override void OnExit(GameObject cameraRig) { }

    public override void OnUpdate() { }

    #endregion

    #region 코루틴

    private IEnumerator<CoroutinePhase> Emergency()
    {
        yield return Suspend.Do(m_emergencyTime);

        m_direction.SendDirectionHoverActive();
    }

    #endregion

    public void CheckActive()
    {
        if (GameData.Instance.UseNetwork)
        {
            if (0 == PlayerManager.Instance.GetPlayerList[0].Team)
            {
                for (int i = 0; i < PlayerManager.Instance.GetPlayerList.Count; ++i)
                {
                    m_direction.m_directionCountCheck[i].m_kgPlayer = PlayerManager.Instance.GetPlayerList[i];
                    m_direction.m_directionCountCheck[i].gameObject.SetActive(true);

                    ParticleSystem[] list = m_direction.m_directionCountCheck[i].gameObject.GetComponentsInChildren<ParticleSystem>();

                    for (int index = 0; index < list.Length; ++index)
                    {
                        ParticleSystem.ColorOverLifetimeModule module = list[index].colorOverLifetime;
                        module.color = PlayerManager.Instance.GetPlayerList[i].Color;
                    }
                }
            }
            else
            {
                for (int i = 0; i < PlayerManager.Instance.GetPlayerList.Count; ++i)
                {
                    m_direction.m_directionCountCheck[i].m_kgPlayer = PlayerManager.Instance.GetPlayerList[i];
                    m_direction.m_directionCountCheck[i].gameObject.SetActive(true);

                    ParticleSystem[] list = m_direction.m_directionCountCheck[i].gameObject.GetComponentsInChildren<ParticleSystem>();

                    for (int index = 0; index < list.Length; ++index)
                    {
                        ParticleSystem.ColorOverLifetimeModule module = list[index].colorOverLifetime;
                        if (GameData.Instance.Nation == Nation.Korea)
                        {
                            if (1 == PlayerManager.Instance.GetPlayerList[i].Team)
                            {
                                module.color = Color.red;
                            }
                            else if (2 == PlayerManager.Instance.GetPlayerList[i].Team)
                            {
                                module.color = Color.blue;
                            }
                        }
                    }
                }
            }
        }

        else
        {
            for (int i = 0; i < m_direction.m_directionCountCheck.Length; ++i)
            {
                m_direction.m_directionCountCheck[i].gameObject.SetActive(true);
            }
        }
    }

    public void CheckEnd()
    {
        for (int i = 0; i < PlayerManager.Instance.GetPlayerList.Count; ++i)
        {
            m_direction.m_directionCountCheck[i].gameObject.SetActive(false);
        }
    }
}

