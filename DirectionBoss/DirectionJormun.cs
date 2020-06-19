using UnityEngine;
using System.Collections;

public class DirectionJormun : Direction
{
    public GameObject hoverCeiling;

    private Animator m_animator;

    protected override void DirectingOnInitialize(params object[] parameters)
    {
        m_active = false;
    }

    protected override void Directingsetting()
    {
        hoverCeiling.SetActive(false);

        m_animator = GetComponent<Animator>();

        DirectionManager.Instance.OpenNarration(5005);
    }

    private void OnEnable()
    {
        AssetManager.Effect.Retrieve("DirectionSound_BuildingJormunAppear", transform);
    }

    protected override void Directingending()
    {
    }

    #region Animator Call Method

    public void JormunFirstActiveEnd() //마지막 애니메이션후 애니메이터에 들어간 스크립트에서 호출해줌. 6초후 리스토어
    {
        StartCoroutine(JormunEnding());
    }

    public void JormunAppearSound() //요루문이 쓰는 애니메이터에 들어간 스크립트에서 호출(애니메이션 도중 사운드가 들어가야함)
    {
        StartCoroutine(DelayJormunAppearSound());
    }

    public IEnumerator DelayJormunAppearSound()
    {
        yield return new WaitForSeconds(1.7f);

        AssetManager.Effect.Retrieve("DirectionSound_DetachShildJormunSound", transform);
    }

    public void JormunEndSound()
    {
        AssetManager.Effect.Retrieve("DirectionSound_JormunOff", transform);
    }
    #endregion

    #region Coroutine

    private IEnumerator JormunEnding()
    {
        yield return new WaitForSeconds(6f);

        Restore();
    }

    #endregion

    #region Unity
    #endregion
}