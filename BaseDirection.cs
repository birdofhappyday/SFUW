using UnityEngine;

public abstract class BaseDirection : CachedAsset
{
    #region 변수
    public virtual void OnExit(GameObject camerairg, int number) { }
    public virtual void OnFall(GameObject camerarig) { }
    public virtual void Hit(Vector3 hitPoint, float damage, HitType hittype = HitType.NONE, bool isWeakPoint = false) { }

    protected abstract void DirectingOnInitialize(params object[] parameters);
    protected abstract void Directingsetting();
    protected abstract void Directingending();

    protected bool m_finish;
    protected bool m_active;
    protected bool m_activeend;

    private Quaternion m_rot;
    #endregion
    #region 프로퍼티

    public bool Active
    {
        get { return m_active; }
        set { m_active = true; }
    }

    public bool Finish
    {
        get { return m_finish; }
        set { m_finish = value; }
    }

    public bool ActiveEnd
    {
        get { return m_activeend; }
        set { m_activeend = true; }
    }

    #endregion
    /// <summary>
    /// 개체 생성전의 초기화 구문
    /// 개체를 지정된 위치나 Transform객체에 붙인다.
    /// 상속된 개체의 초기화가 이때 진행된다.
    /// </summary>
    /// <param name="parameters"></param>
    internal override void OnInitialize(params object[] parameters)
    {
        m_rot = transform.rotation;

        foreach (object element in parameters)
        {
            if (element is Transform)
            {
                Transform trans_position = element as Transform;
                transform.position = trans_position.position;
                transform.localRotation = trans_position.localRotation;
            }
            else if (element is Vector3)
            {
                Vector3 position = (Vector3)element;
                transform.position = position;
            }
        }

        DirectingOnInitialize(parameters);
    }

    /// <summary>
    /// 오브젝트 사용이 끝나서 유저 눈에 안 보이게 바꾸어 준다.
    /// </summary>
    protected override void OnRestore()
    {
        if (gameObject == null || !gameObject.activeSelf)
        {
            return;
        }

        if (null == transform.parent)
        {
            transform.SetParent(ResourceNode);
        }

        Directingending();
        gameObject.transform.rotation = m_rot;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 객체가 초기화가 끝나고 유저 눈에 보이면서 오브젝트가 움직이게 한다.
    /// 상속된 개체에 지정된 setting이 이때 실행된다.
    /// </summary>
    protected override void OnUse()
    {
        gameObject.SetActive(true);
        Directingsetting();
    }
}