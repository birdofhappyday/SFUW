using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class DirectionCountCheck : MonoBehaviour
{
    public GameObject m_basic;
    public GameObject m_first;
    public GameObject m_second;
    public int m_number;
    public KGPlayer m_kgPlayer;

    private bool m_active;
    private Direction m_direction;
    private GameObject m_camerarig;
    private UIDirectionPlayerNum m_nickNameUI;
    private CameraDirection m_cameradirection;

    #region 프로퍼티

    public CameraDirection Direction
    {
        get { return m_cameradirection; }
    }

    #endregion
    
    public void OnEnable()
    {
        m_nickNameUI = null;
        m_active = false;
        m_basic.SetActive(true);
        m_first.SetActive(true);
        m_second.SetActive(false);
        m_direction = transform.parent.GetComponent<Direction>();
        if (null != m_kgPlayer)
        {
            m_nickNameUI = UIManager.Instance.Open("UIDirectionPlayerNum", true) as UIDirectionPlayerNum;
            m_nickNameUI.TextSet(m_kgPlayer);
            m_nickNameUI.transform.position = transform.position;
            m_nickNameUI.transform.position += new Vector3(0, 1f, 0);
        }
    }

    private void OnDisable()
    {
        if (null != m_nickNameUI && AssetState.Using == m_nickNameUI.AssetState)
        {
            m_nickNameUI.Restore();
        }
    }

    public void OnEnter(bool temp)
    {
        if (temp)
        {
            m_first.SetActive(false);
            m_second.SetActive(true);
            AssetManager.Effect.Retrieve("CheckEnterSound", m_second.transform.position);
            m_active = true;
        }
        else
        {
            m_first.SetActive(true);
            m_second.SetActive(false);
            m_active = false;
        }
    }

    /// <summary>
    /// 유저가 호버보드에 탑승했는지 여부를 판단한다.
    /// 유저가 탑승할 수 있는 위치가 정해져 있는데 그 부분에 맞게 탑승했을 경우
    /// 이펙트가 발생하면서 유저가 탑승했는지 여부를 상위 개체에 알려준다.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other) 
    {
        if (GameData.Instance.UserType == UserType.Host) return;

        if (other.tag == "Direction")
        {
            if (m_active) return;

            if (m_direction.Active) return;

            if (0 == GameData.Instance.Player.Team)
            {
                if (null != m_kgPlayer && !m_kgPlayer.NickName.Equals(GameData.Instance.Player.NickName)) return;
            }
            else
            {
                if (null != m_kgPlayer && !m_kgPlayer.StoreName.Equals(GameData.Instance.Player.StoreName)) return;
            }

            m_cameradirection = other.GetComponent<CameraDirection>();

            if (null == m_cameradirection) return;

            m_camerarig = m_cameradirection.m_camerarig;
            OnEnter(true);

            m_direction.OnEnter(m_camerarig, m_number, m_cameradirection);
        }
    }

    /// <summary>
    /// 유저가 호버트에서 탑승 상태에서 벗어날 경우 이펙트를 실행하고
    /// 상위개체에 나간 사실을 알려준다.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        if (GameData.Instance.UserType == UserType.Host) return;

        if (other.tag == "Direction")
        {
            if (m_cameradirection != other.GetComponent<CameraDirection>()) return;

            if (m_direction.Active) return;

            OnEnter(false);

            m_direction.OnExit(m_camerarig, m_number);
        }
    }
}

