using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public enum MapState
{
    Move,
    Stop,
}

public class Map : MonoBehaviour
{
    public CustomPath[] m_customPath;

    private int m_currentPathNum = 0;
    private CoroutineCommand m_coroutineCommand;

    private void Awake()
    {
        GameData.Instance.Map = this;
    }

    public void Initialize() { }

    public void Move(int num, bool burst = false)
    {
        m_currentPathNum = num;

        if (burst)
        {
            m_coroutineCommand = CoroutineManager.Instance.Register(m_customPath[num].Startmove(m_customPath[num], this.gameObject, false, 15));
            GameData.Instance.HoverCraftSegTwo.m_wind.SetActive(false);
            StartCoroutine(WindCheck(num));
        }
        else
        {
            m_coroutineCommand = CoroutineManager.Instance.Register(m_customPath[num].Startmove(m_customPath[num], this.gameObject, false, 7));
        }
    }

    public void Stop(bool slowStop = false)
    {
        if (slowStop)
        {
            CoroutineManager.Instance.Unregister(m_coroutineCommand);
            m_coroutineCommand = null;
            StartCoroutine(SlowStop());
        }
        else
        {
            CoroutineManager.Instance.Unregister(m_coroutineCommand);
            m_coroutineCommand = null;
        }
    }

    public void Seg1HoverMove()
    {
        GameData.Instance.Hover1.Finish = false;
        m_coroutineCommand = CoroutineManager.Instance.Register(m_customPath[0].Startmove(m_customPath[0], this.gameObject, true, 2, GameData.Instance.Hover1.transform));

        for (int i = 0; i < GameData.Instance.hoverLandingPos.Length; ++i)
        {
            AssetManager.Effect.Retrieve("FX_BoardLandingDust", GameData.Instance.hoverLandingPos[i]);
        }

        


        StartCoroutine(Seg1HoverEndActive(GameData.Instance.Hover1));
    }

    public void Seg2HoverEnd()
    {
        StartCoroutine(Seg2HoverEndActive(GameData.Instance.Hover1));
    }

    #region 코루틴

    private IEnumerator WindCheck(int num)
    {
        if (null != GameData.Instance.HoverCraftSegTwo)
        {
            while (true)
            {
                if (m_customPath[num].DirectionOption == false)
                {
                    GameData.Instance.HoverCraftSegTwo.m_wind.SetActive(false);
                    break;
                }
                yield return null;
            }

            if (1 == num)
            {
                if (GameData.Instance.UserType != UserType.Host)
                {
                    DirectionManager.Instance.OpenNarration(5004);
                }
            }
            else if (2 == num)
            {
                if (GameData.Instance.UserType != UserType.Host)
                {
                    DirectionManager.Instance.OpenNarration(5007);
                }
            }
        }
    }

    private IEnumerator Seg1HoverEndActive(Direction direction)
    {
        while (!direction.Finish)
        {
            yield return null;
        }
        GameData.Instance.Hover1.ActiveEnd = true;
        GameData.Instance.Hover1.transform.GetComponent<HoverBoardSegOne>().HoverEndActive();
        GameData.Instance.Hover2.transform.GetComponent<HoverBoardSegOne>().HoverEndActive();

        CoroutineManager.Instance.Unregister(m_coroutineCommand);
        m_coroutineCommand = null;
    }

    private IEnumerator Seg2HoverEndActive(Direction direction)
    {
        GameData.Instance.HoverCraftSegTwo.HoverCraftFall();

        float temp_time = 0.0f;

        while (3.0 > temp_time)
        {
            temp_time += Time.deltaTime;
            transform.position += new Vector3(0, -0.3f, 0);
            yield return null;
        }
        temp_time = 0.0f;

        DirectionManager.Instance.OpenNarration(5014);

        while (true)
        {
            transform.position += new Vector3(0, 0.2f, 0);
            yield return null;
        }
    }

    private IEnumerator SlowStop()
    {
        GameData.Instance.Map.Stop();

        float m_speed = 7.0f;
        float m_percentage = 0.0f;
        Vector3 destination = transform.position + new Vector3(-12, 0, 0);

        while (m_percentage <= 1.0f)
        {
            m_percentage += m_speed * Time.deltaTime * 0.01f;
            transform.position = Vector3.Lerp(transform.position, destination, m_percentage);
            yield return null;
        }
    }

    #endregion 

    #region network

    public void SendMapMove(int num)
    {
        if (GameData.Instance.UserType != UserType.Host) return;

        JObject packetData = new JObject();

        packetData.Add("type", (int)MapState.Move);
        packetData.Add("pathnum", num);

        Network.Instance.SendTCP("map packet", packetData);
    }

    public void SendMapStop(bool slowStop)
    {
        if (GameData.Instance.UserType != UserType.Host) return;

        JObject packetData = new JObject();

        packetData.Add("type", (int)MapState.Stop);
        packetData.Add("slowstop", (bool)slowStop);

        Network.Instance.SendTCP("map packet", packetData);
    }

    public void ProcessMapPacketData(JObject packetData)
    {
        MapState mapState = (MapState)packetData.Value<int>("type");

        switch (mapState)
        {
            case MapState.Move:
                if (GameData.Instance.UserType == UserType.Host) return;

                int pathNum = packetData.Value<int>("pathnum");
                Move(pathNum);
                break;

            case MapState.Stop:
                if (GameData.Instance.UserType == UserType.Host) return;

                bool slowStop = packetData.Value<bool>("slowstop");
                Stop(slowStop);
                break;
        }
    }

    #endregion

    #region Unity

    #endregion
}
