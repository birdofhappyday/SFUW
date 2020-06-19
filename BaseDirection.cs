using UnityEngine;

public abstract class BaseDirection : CachedAsset
{
    public virtual void OnExit(GameObject camerairg, int number) { }
    public virtual void OnFall(GameObject camerarig) { }
    public virtual void Hit(Vector3 hitPoint, float damage, HitType hittype = HitType.NONE, bool isWeakPoint = false) { }

    protected abstract void DirectingOnInitialize(params object[] parameters);
    protected abstract void Directingsetting();
    protected abstract void Directingending();

    protected bool m_finish;
    protected bool m_active;
    protected bool m_activeend;

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

    private Quaternion m_rot;

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

    protected override void OnUse()
    {
        gameObject.SetActive(true);
        Directingsetting();
    }
}