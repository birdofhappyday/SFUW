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

    protected override void Directingsetting() // 요루문에 우주선 지붕이 붙어있는데 그 객체를 일단 꺼주고 나중에 애니메이터에서 켜준다.
    {
        hoverCeiling.SetActive(false);

        m_animator = GetComponent<Animator>();

        DirectionManager.Instance.OpenNarration(5005);
    }

    private void OnEnable()  //등장시 행동 설정(사운드만 설정되어 있다.)
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

    public void JormunEndSound() // 요루문이 죽을 때 나는 사운드
    {
        AssetManager.Effect.Retrieve("DirectionSound_JormunOff", transform);
    }
    #endregion

    #region Coroutine

    private IEnumerator JormunEnding() //요루문이 지정된 애니메이션 호출 후 사라진다.
    {
        yield return new WaitForSeconds(6f);

        Restore();
    }

    #endregion

    #region Unity
    #endregion
}