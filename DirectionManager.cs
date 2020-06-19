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

    public void Initialize()
    {
        m_directionList.Clear();
        m_bothobject.Clear();
        directionnumber = 0;
    }

    public void OpenNarration(int index)
    {
        if (ui != null)
        {
            ui = null;
        }
        ui = UIManager.Instance.Open("UITextBox") as UIGameStateText;
        ui.ShowNarration(index);
    }

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

    public void RemoveDirection(Direction direction)
    {
        if (!m_directionList.Find(rhs => rhs.m_directionid == direction.m_directionid))
        {
            return;
        }

        m_directionList.Remove(direction);
        RemoveBothDirection(direction);
    }

    public void RemoveBothDirection(Direction direction)
    {
        if (!m_bothobject.Find(rhs => rhs.m_directionid == direction.m_directionid))
        {
            return;
        }

        m_bothobject.Remove(direction);
    }

    public void BothMemberCount(int member)
    {
        bothnumber += member;
    }

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

    public Direction FindDirectionByID(int id)
    {
        return m_directionList.Find(rhs => rhs.m_directionid == id);
    }

    public void ProcessDirectionPacketData(JObject packetData)
    {
        DirectionState directionState = (DirectionState)packetData.Value<int>("type");
        int directionID = packetData.Value<int>("id");
        Direction direction = FindDirectionByID(directionID);
        if (null == direction && DirectionState.Spwan != directionState)
        {
            Log.Warning("없는 디렉션을 체크하려고 합니다...");
            return;
        }

        switch (directionState)
        {
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

            case DirectionState.CheckActive:
                if (direction.Active) return;

                int checkNumber = packetData.Value<int>("check num");
                bool checkActive = packetData.Value<bool>("active");
                direction.CheckActive(checkNumber, checkActive);
                break;
            case DirectionState.DirectionActive:
                if (direction.Active) return;

                direction.DirectionActive();
                break;

            case DirectionState.HoverBoth:
                if (GameData.Instance.UserType == UserType.Host || direction.Active) return;

                ActiveBothMember();
                break;

            case DirectionState.Hit:
                int damage = packetData.Value<int>("damage");
                direction.HitFromServer(damage, direction.transform.position);
                break;

            case
            DirectionState.Restore:
                direction.RestorMember++;
                direction.DirectionRestore();
                break;

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

            case DirectionState.HoverBoarding:
                if (GameData.Instance.UserType == UserType.Host) return;

                direction.DirectionBoarding();

                break;
        }
    }
}

