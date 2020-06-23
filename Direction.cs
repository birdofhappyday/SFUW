using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json.Linq;

public enum HoverEnum
{
    hoverBoard1,
    hoverBoard2,
    hoverCraft1,
    hoverCraft2
}

public abstract class Direction : BaseDirection, Hittable
{
    #region 변수

    [System.Serializable]
    public class FadePattern
    {
        public float m_aniTime = 2f;
        public float m_start = 0f;
        public float m_end = 1f;
        public GameObject m_meshRender;
        public Material m_fadeMaterial;
        public Material m_realMaterial;
    }
    
    public FadePattern[] m_fadepattern;
    public int m_directionid;
    public bool m_bothbool;
    public CustomPath[] m_custompath;
    public DirectionCountCheck[] m_directionCountCheck;
    
    protected int m_onmember;
    protected int m_restornumber;

    private CoroutineCommand m_coroutine;

    #endregion

    #region 프로퍼티
    public int Member
    {
        get { return m_onmember; }
        set { m_onmember = value; }
    }

    public int RestorMember
    {
        get { return m_restornumber; }
        set { m_restornumber = value; }
    }

    public bool Both
    {
        get { return m_bothbool; }
        set { m_bothbool = value; }
    }
    #endregion

    #region 기존 BaseDirection virtual 함수
    protected override void DirectingOnInitialize(params object[] parameters)
    {
        DirectionManager.Instance.AddDirection(this);
    }

    protected override void Directingending()
    {
        DirectionManager.Instance.RemoveDirection(this);
    }
    
    protected override void OnRestore()
    {
        //if (gameObject == null)
        //{
        //    return;
        //}

        base.OnRestore();        
    }

    public virtual void OnEnter(GameObject camerarig, int number, CameraDirection eyechild = null)
    {
        if (-1 != number)
        {
            SendDirectionCheckActive(number, true);
        }
        SendDirectionCountData(1);
    }

    public override void OnExit(GameObject camerarig, int number)
    {
        if (-1 != number)
        {
            SendDirectionCheckActive(number, false);
        }
        SendDirectionCountData(-1);
    }
    #endregion

    #region 추가 virtual 함수
    public virtual void SetTransform(Transform tr)
    {
        transform.position = tr.position;
        transform.rotation = tr.rotation;
    }

    public virtual void Countcheck() { }

    public virtual void CheckActive(int number, bool active)
    {
        m_directionCountCheck[number].OnEnter(active);
    }

    public virtual void RestoreTime() { }
    
    public virtual void DirectionActive() { }

    public virtual void DirectionRestore() { }

    public virtual void DirectionBothActive() { }

    public virtual void DirectionBoarding() { }

    #endregion

    #region direction 사용 함수

    /// <summary>
    /// 객체에서 부모를 바꾸는 경우가 잇는데 그때 사용되는 함수.
    /// </summary>
    public void SetParentInit()
    {
        transform.SetParent(ResourceNode);
    }

    /// <summary>
    /// effect를 지정된 객체에 붙이기 위한 함수.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="traget"></param>
    public void EffectRetrieve(string name, Transform traget)
    {
        Effect effect = AssetManager.Effect.Retrieve(name);
        effect.transform.position = traget.position;
        effect.transform.rotation = traget.rotation;
    }

    /// <summary>
    /// effect를 지정된 위치에 만들기 위한 함수.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="traget"></param>
    public void EffectRetrieve(string name, Vector3 traget)
    {
        Effect effect = AssetManager.Effect.Retrieve(name);
        effect.transform.position = traget;
    }

    /// <summary>
    /// FadeIn을 위한 corutine 함수
    /// </summary>
    /// <param name="m_fade"></param>
    /// <param name="m_realmat"></param>
    /// <param name="m_original"></param>
    public void PlayFadeIn(FadePattern m_fade, Material m_realmat, Direction m_original)
    {
        m_original.m_finish = false;

        m_coroutine = CoroutineManager.Instance.Register(Playfadein(m_fade, m_realmat, m_original));
    }

    /// <summary>
    /// FadeOut을 위한 courutine실행 함수
    /// </summary>
    /// <param name="m_fade"></param>
    /// <param name="m_original"></param>
    public void PlayFadeOut(FadePattern m_fade, Direction m_original)
    {
        m_original.m_finish = false;

        m_coroutine = CoroutineManager.Instance.Register(PlayFadeout(m_fade, m_original));
    }
    
    /// <summary>
    /// 유저가 일정된 시간 동안 탑승을 하지 않았을 경우 진행을 위해서 오브젝트를 강제로 움직인다.
    /// 주로 호버보드 탑승의 경우에 유저가 타지 못해서 진행이 안 될 경우를 위해서 가동시킨다.
    /// 먼저 유저가 타서 작동되면 코루틴은 꺼지게 되어있다.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="bothMember"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    public IEnumerator<CoroutinePhase> Emergency(float time, bool bothMember, Direction direction = null)
    {
        yield return Suspend.Do(time);

        if (GameData.Instance.UseNetwork)
        {
            if (!bothMember)
            {
                direction.SendDirectionHoverActive();
            }

            else DirectionManager.Instance.ActiveBothMember();
        }

        else if(!GameData.Instance.UseNetwork)
        {
            if (!bothMember)
            {
                direction.DirectionActive();
            }

            else DirectionManager.Instance.ActiveBothMember();
        }
    }

    /// <summary>
    /// 지정된 시간 후에 오브젝트가 유저의 눈에 보이지 않는 대기 상태로 바꾼다.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="m_direction"></param>
    protected void Directionrestor(float time, Direction m_direction)
    {
        m_coroutine = CoroutineManager.Instance.Register(DirectionRestore(time, m_direction));
    }

    protected IEnumerator<CoroutinePhase> DirectionRestore(float time, Direction m_direction)
    {
        yield return Suspend.Do(time);

        m_direction.Restore();
        CoroutineManager.Instance.Unregister(m_coroutine);
    }

    protected IEnumerator<CoroutinePhase> Crash(Transform m_transform, float high, Vector3 splineCurve, float m_speed)
    {
        float speed = 20.0f;
        float m_amplitude = 5.0f;
        float m_period = 5.0f;
        float m_time = 0.0f;

        while (true)
        {
            Vector3 newPosition = Vector3.zero;
            float sinValue = Mathf.Sin(m_time);

            newPosition.y = sinValue * m_amplitude;
            newPosition.z = speed * Time.deltaTime;

            m_transform.position += newPosition;

            m_time += Time.deltaTime * m_period;
            yield return null;
        }
    }

    private IEnumerator<CoroutinePhase> Playfadein(FadePattern m_fade, Material m_realmat, Direction m_original)
    {
        Color color = m_fade.m_fadeMaterial.color;
        float m_percentage = 0f;
        color.a = Mathf.Lerp(m_fade.m_start, m_fade.m_end, m_percentage);

        while (color.a != m_fade.m_end)
        {
            m_percentage += Time.deltaTime / m_fade.m_aniTime;

            color.a = Mathf.Lerp(m_fade.m_start, m_fade.m_end, m_percentage);

            m_fade.m_fadeMaterial.color = color;

            yield return null;
        }

        MeshRenderer m_temp = m_fade.m_meshRender.GetComponent<MeshRenderer>();
        if (null != m_temp)
        {
            m_temp.material = m_realmat;
        }
        else
        {
            m_fade.m_meshRender.GetComponent<SkinnedMeshRenderer>().material = m_realmat;
        }
        
        m_original.m_finish = true;

        CoroutineManager.Instance.Unregister(m_coroutine);
    }

    private IEnumerator<CoroutinePhase> PlayFadeout(FadePattern m_fade, Direction m_original)
    {
        SkinnedMeshRenderer m_temp = m_fade.m_meshRender.GetComponent<SkinnedMeshRenderer>();
        if (null != m_temp)
        {
            m_temp.material = m_fade.m_fadeMaterial;
        }
        else
        {
            m_fade.m_meshRender.GetComponent<MeshRenderer>().material = m_fade.m_fadeMaterial;
        }
        Color color = m_fade.m_fadeMaterial.color;
        float m_percentage = 0f;
        color.a = Mathf.Lerp(m_fade.m_start, m_fade.m_end, m_percentage);

        while (color.a != m_fade.m_end)
        {
            m_percentage += Time.deltaTime / m_fade.m_aniTime;

            color.a = Mathf.Lerp(m_fade.m_start, m_fade.m_end, m_percentage);

            m_fade.m_fadeMaterial.color = color;

            yield return null;
        }

        m_original.GetComponent<Direction>().m_finish = true;

        CoroutineManager.Instance.Unregister(m_coroutine);
    }

    public override void Hit(Vector3 hitPoint, float damage, HitType hittype = HitType.NONE, bool isWeakPoint = false)
    {
        if (GameData.Instance.UserType == UserType.Host) return;
        HitEvent(hitPoint, damage);
    }

    public virtual void HitEvent(Vector3 hitPoint, float damage)
    {
        JObject directionHitPacket = new JObject();
        directionHitPacket.Add("type",      (int)DirectionState.Hit);
        directionHitPacket.Add("id",        m_directionid);
        directionHitPacket.Add("damage",    damage);

        Network.Instance.SendTCP("direction packet", directionHitPacket);
    }

    public virtual void HitFromServer(float nDamage, Vector3 hitPosition)
    {
    }

    #endregion

    #region 네트워크 관련 함수
    public JObject GetDirectionSpawanData()
    {
        Transform dpr = transform;
        JObject directionData = new JObject();

        directionData.Add("type",   (int)DirectionState.Spwan);
        directionData.Add("name",   gameObject.name);
        directionData.Add("id"  ,   m_directionid);
                     
        directionData.Add("dpx",    dpr.position.x);
        directionData.Add("dpy",    dpr.position.y);
        directionData.Add("dpz",    dpr.position.z);
                               
        directionData.Add("drx",    dpr.rotation.x);
        directionData.Add("dry",    dpr.rotation.y);
        directionData.Add("drz",    dpr.rotation.z);
        directionData.Add("drw",    dpr.rotation.w);

        return directionData;
    }

    public void SendDirectionHit(int damage)
    {
        if (GameData.Instance.UserType == UserType.Host) return;

        JObject packetData = new JObject();

        packetData.Add("type",      (int)DirectionState.Hit);
        packetData.Add("id",        m_directionid);
        packetData.Add("damage",    damage);

        Network.Instance.SendTCP("direction packet", packetData);
    }

    public void SendDirectionRestore()
    {
        JObject packetData = new JObject();

        packetData.Add("type",  (int)DirectionState.Restore);
        packetData.Add("id",    m_directionid);

        Network.Instance.SendTCP("direction packet", packetData);
    }

    public void SendDirectionCountData(int num)
    {
        if (GameData.Instance.UserType == UserType.Host) return;

        JObject packetData = new JObject();

        packetData.Add("type",  (int)DirectionState.Count);
        packetData.Add("id",    m_directionid);
        packetData.Add("count", num);

        Network.Instance.SendTCP("direction packet", packetData);
    }

    public void SendDirectionCheckActive(int num, bool active)
    {
        JObject packetData = new JObject();

        packetData.Add("type",      (int)DirectionState.CheckActive);
        packetData.Add("id",        m_directionid);
        packetData.Add("check num", num);
        packetData.Add("active",    active);

        Network.Instance.SendTCP("direction packet", packetData);
    }

    public void SendDirectionHoverActive()
    {
        if (GameData.Instance.UserType != UserType.Host) return;

        JObject packetData = new JObject();

        packetData.Add("type",  (int)DirectionState.DirectionActive);
        packetData.Add("id",    m_directionid);

        Network.Instance.SendTCP("direction packet", packetData);
    }

    public void SendDirectionHoverBothActive()
    {
        if (GameData.Instance.UserType != UserType.Host) return;

        JObject packetData = new JObject();

        packetData.Add("type",  (int)DirectionState.HoverBoth);
        packetData.Add("id",    m_directionid);

        Network.Instance.SendTCP("direction packet", packetData);
    }

    public void SendDirectionBoth()
    {
        JObject packetData = new JObject();

        packetData.Add("type",  (int)DirectionState.HoverboardSegTwo);
        packetData.Add("id",    m_directionid);

        Network.Instance.SendTCP("direction packet", packetData);
    }

    public void SendDirectionBoarding()
    {
        if (GameData.Instance.UserType != UserType.Host) return;

        JObject packetData = new JObject();

        packetData.Add("type",  (int)DirectionState.HoverBoarding);
        packetData.Add("id",    m_directionid);

        Network.Instance.SendTCP("direction packet", packetData);
    }

    public JObject GetEnemyHoverSpawn(int i)
    {
        Transform dpr = transform;
        JObject directionData = new JObject();

        directionData.Add("type", (int)DirectionState.Spwan);
        directionData.Add("name", gameObject.name);
        directionData.Add("id",   m_directionid);
        directionData.Add("path", i);

        directionData.Add("dpx",  (float)dpr.position.x);
        directionData.Add("dpy",  (float)dpr.position.y);
        directionData.Add("dpz",  (float)dpr.position.z);
                                  
        directionData.Add("drx",  (float)dpr.rotation.x);
        directionData.Add("dry",  (float)dpr.rotation.y);
        directionData.Add("drz",  (float)dpr.rotation.z);
        directionData.Add("drw",  (float)dpr.rotation.w);

        return directionData;
    }

    public void SetDirectionTransformFromPacket(JObject packetData)
    {
        //TransformInfo directionTr = NetUtil.Instance.TransformUnzip(packetData["position"].ToString(), packetData["rotation"].ToString());
        if (GameData.Instance.UserType == UserType.Host) return;

        int type = packetData.Value<int>("type");
        DirectionState temp = (DirectionState)type;

        gameObject.name = packetData.Value<string>("name");

        m_directionid = packetData.Value<int>("id");

        Vector3 directionPos = new Vector3(packetData.Value<float>("dpx"), packetData.Value<float>("dpy"), packetData.Value<float>("dpz"));
        transform.position = directionPos;

        Quaternion directionRot = new Quaternion(packetData.Value<float>("drx"), packetData.Value<float>("dry"), packetData.Value<float>("drz"), packetData.Value<float>("drw"));
        transform.rotation = directionRot;

        //SetTransform(transform);
    }
    #endregion
}

