using UnityEngine;

public abstract class BaseItem : CachedAsset
{
    public int m_itemID;

    #region 프로퍼티
        
    #endregion

    #region 상속시킬 함수들

    protected abstract void OnInit(params object[] parameters);
    protected abstract void OnStart();
    protected abstract void OnEnd();
    protected abstract void OnUpdate(Transform other);

    #endregion

    #region 기본 상속 함수들

    internal override void OnInitialize(params object[] parameters)
    {
        foreach (object element in parameters)
        {
            if (element is Transform)
            {
                Transform m_startTransform = element as Transform;
                transform.position = m_startTransform.position;
                transform.forward = m_startTransform.forward;
            }
        }

        OnInit(parameters);
    }

    protected override void OnUse()
    {
        gameObject.SetActive(true);
        OnStart();
    }

    protected override void OnRestore()
    {
        OnEnd();
        gameObject.SetActive(false);
    }

    #endregion
    
    #region Unity

    private void LateUpdate()
    {
        OnUpdate(transform);
    }

    #endregion
}
