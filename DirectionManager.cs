using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public enum DirectionState
{
    Spwan,
    Count,
    DirectionActive,
    Hit,
    CheckActive,
    Position,
    Restore,
    HoverGo,
    HoverboardSegTwo,
    HoverBoarding,
    HoverBoth,

}

/// <summary>
/// 호버보드 관리를 위해서 따로 매니저를 설정.
/// </summary>
public class DirectionManager : SingleTon<DirectionManager>
{
    private List<Direction> m_directionList;
    private List<Direction> m_bothobject;
    private int directionnumber;
    private int bothnumber;
    private UIGameStateText ui;

    #region 프로퍼티

    public int BothMember
    {
        get { return bothnumber; }
    }

    #endregion

    private DirectionManager()
    {
        m_directionList = new List<Direction>();
        m_bothobject = new List<Direction>();
        directionnumber = 0;
        bothnumber = 0;
    }

    /// <summary>
    /// 기본초기화. 설정된 리스트를 비워준다.
    /// </summary>
    public void Initialize()
    {
        m_directionList.Clear();
        m_bothobject.Clear();
        directionnumber = 0;
    }

    /// <summary>
    /// 리스트에서 해당된 id의 객체 반환.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Direction FindDirectionByID(int id)
    {
        return m_directionList.Find(rhs => rhs.m_directionid == id);
    }

    /// <summary>
    /// 설정된 문구를 유저눈에 보이게 하기 위해서 만듬.
    /// </summary>
    /// <param name="index"></param>
    public void OpenNarration(int index)
    {
        if (ui != null)
        {
            ui = null;
        }
        ui = UIManager.Instance.Open("UITextBox") as UIGameStateText;
        ui.ShowNarration(index);
    }

    /// <summary>
    /// 매니저에 등장한 개체를 집어넣어서 관리
    /// </summary>
    /// <param name="direction"></param>
    public void AddDirection(Direction direction)
    {
        if (m_directionList.Find(rhs => rhs.m_directionid == direction.m_directionid))
        {
            return;
        }

        m_directionList.Add(direction);

        directionnumber++;
        direction.m_directionid = directionnumber;

        if (true == direction.m_bothbool)
        {
            m_bothobject.Add(direction);
        }
    }

    /// <summary>
    /// 매니저에서 지켜보는 개체 삭제
    /// </summary>
    /// <param name="direction"></param>
    public void RemoveDirection(Direction direction)
    {
        if (!m_directionList.Find(rhs => rhs.m_directionid == direction.m_directionid))
        {
            return;
        }

        m_directionList.Remove(direction);
        RemoveBothDirection(direction);
    }

    /// <summary>
    /// 2인이 타는 개체는 따로 관리하기에 사용이 끝난 다음에 지워준다.
    /// </summary>
    /// <param name="direction"></param>
    public void RemoveBothDirection(Direction direction)
    {
        if (!m_bothobject.Find(rhs => rhs.m_directionid == direction.m_directionid))
        {
            return;
        }

        m_bothobject.Remove(direction);
    }

    /// <summary>
    /// 개체에 탑승한 인원수를 서버에서 받아와서 늘려주거나 빼준다.
    /// </summary>
    /// <param name="member"></param>
    public void BothMemberCount(int member)
    {
        bothnumber += member;
    }

    /// <summary>
    /// 설정된 인원수가 되었을때 모두 탔다고 판단 되었을 때 작동시킨다.
    /// 인터넷이 사용중이라면 서버에 메시지를 보내고 아닐 경우에는 직접 작동시킨다.
    /// 2인이 타는 개체에만 사용된다.
    /// </summary>
    public void ActiveBothMember()
    {
        if(GameData.Instance.UseNetwork)
        {
            for (int i = 0; i < m_bothobject.Count; ++i)
            {
                m_bothobject[i].SendDirectionHoverActive();
            }
        }
        else if(!GameData.Instance.UseNetwork)
        {
            for (int i = 0; i < m_bothobject.Count; ++i)
            {
                m_bothobject[i].DirectionActive();
            }
        }
        bothnumber = 0;
    }

    /// <summary>
    /// 서버에서 받아온 데이터를 처리하는 부분.
    /// host부분과 user를 체크해서 동작하게 되어 있다.
    /// 2인이상의 객체에 유저가 모두 탔는지 확인하는 부분과 동작에 관한 부분.
    /// 유저가 공격했는지 여부를 서버와 통신한다.
    /// </summary>
    /// <param name="packetData"></param>
    public void ProcessDirectionPacketData(JObject packetData)
    {
        DirectionState directionState = (DirectionState)packetData.Value<int>("type");
        int directionID = packetData.Value<int>("id");
        Direction direction = FindDirectionByID(directionID);
        if (null == direction && DirectionState.Spwan != directionState)
            Log.Warning("없는 디렉션을 체크하려고 합니다...");
        {
            return;
        }

        switch (directionState)
        {
            //객체를 만들어낼때 사용되는 함수.
            //객체에 직접 접근할 때를 대비해서 GameData라는 곳에 넣어놓고 필요할 때 쓴다.
            case DirectionState.Spwan:
                if (GameData.Instance.UserType == UserType.Host || null != direction) return;

                string directionName = packetData.Value<string>("name");
                
                direction = AssetManager.Direction.Retrieve(directionName) as Direction;

                if (directionName == "hoverboardSeg1_1" || directionName == "hoverboardSeg2_1")
                {
                    GameData.Instance.Hover1 = direction;
                }
                else if (directionName == "hoverboardSeg1_2" || directionName == "hoverboardSeg2_2")
                {
                    GameData.Instance.Hover2 = direction;
                }

                if(direction.gameObject.activeSelf == false)
                {
                    direction.gameObject.SetActive(true);
                }

                direction.m_directionid = directionID;
                direction.SetDirectionTransformFromPacket(packetData);
                break;

            //객체에 인원이 탑승했을 때 수를 늘려주거나 나갔을때 인원을 줄인다.
            case DirectionState.Count:
                if (GameData.Instance.UserType != UserType.Host) return;

                int directionCount = packetData.Value<int>("count");

                if (direction.Both)
                {
                    BothMemberCount(directionCount);
                    direction.Countcheck();
                }
                else
                {
                    direction.Member += directionCount;
                    direction.Countcheck();
                }
                break;

            //객체가 가동할 수 있는 인원수가 되었는지 알려준다.
            case DirectionState.CheckActive:
                if (direction.Active) return;

                int checkNumber = packetData.Value<int>("check num");
                bool checkActive = packetData.Value<bool>("active");
                direction.CheckActive(checkNumber, checkActive);
                break;

            //객체가 가동되게 해준다.
            case DirectionState.DirectionActive:
                if (direction.Active) return;

                direction.DirectionActive();
                break;

            //호스트가 아닌 객체에서 인원수가 설정된 객체가 움직이게 해준다.
            case DirectionState.HoverBoth:
                if (GameData.Instance.UserType == UserType.Host || direction.Active) return;

                ActiveBothMember();
                break;

            //공격을 받았다는 사실을 서버에 알려준다.
            case DirectionState.Hit:
                int damage = packetData.Value<int>("damage");
                direction.HitFromServer(damage, direction.transform.position);
                break;

            //객체가 만들어 진 것을 알려준다.
            case DirectionState.Restore:
                direction.RestorMember++;
                direction.DirectionRestore();
                break;

            //stage2의 호버보드 이동 관련.
            case DirectionState.HoverboardSegTwo:
                if (GameData.Instance.UserType != UserType.Host) return;

                if (m_bothobject.Find(rhs => false == direction.Both))
                {
                    return;
                }

                for (int i = 0; i < m_bothobject.Count; ++i)
                {
                    m_bothobject[i].DirectionBothActive();
                }

                break;

            //호버보드 이동관련해서 알려준다.
            case DirectionState.HoverBoarding:
                if (GameData.Instance.UserType == UserType.Host) return;

                direction.DirectionBoarding();

                break;
        }
    }
}

