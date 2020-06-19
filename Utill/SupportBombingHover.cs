using System.Collections;
using UnityEngine;

public class SupportBombingHover : Direction
{
    public int restoreTime;
    private CoroutineCommand coroutine;

    protected override void Directingsetting()
    {
        StartCoroutine(moveStart());
    }

    private IEnumerator moveStart()
    {
        coroutine = CoroutineManager.Instance.Register(m_custompath[0].Startmove(m_custompath[0], this.gameObject));
        while (!m_finish)
        {
            yield return null;
        }
        CoroutineManager.Instance.Unregister(coroutine);
        coroutine = null;
        StartCoroutine(DelayRestore());
    }

    private IEnumerator DelayRestore()
    {
        yield return new WaitForSeconds(restoreTime);
        m_finish = false;
        coroutine = CoroutineManager.Instance.Register(m_custompath[1].Startmove(m_custompath[1], this.gameObject));

        while (!m_finish)
        {
            yield return null;
        }
        Restore();
    }

    protected override void Directingending()
    {
        if(coroutine != null)
        {
            CoroutineManager.Instance.Unregister(coroutine);
            coroutine = null;
        }
    }
}