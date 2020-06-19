using UnityEngine;
using System.Collections;

public class MonsterSpawnPotal : MonoBehaviour
{
    public GameObject openPotal;
    public GameObject closePotal;
    public float closePotalTime;
    public bool m_useDestroy = true;

    private void Awake()
    {
        DirectionManager.Instance.OpenNarration(5009);
        openPotal.SetActive(true);
        closePotal.SetActive(false);
        AssetManager.Effect.Retrieve("DirectionSound_PotalOpen", gameObject.transform);
        StartCoroutine(OpenClosePotal());
    }
    
    public IEnumerator OpenClosePotal()
    {
        yield return new WaitForSeconds(closePotalTime);
        openPotal.SetActive(false);
        closePotal.SetActive(true);
        yield return new WaitForSeconds(3f);

        if (m_useDestroy)
            gameObject.SetActive(false);
    }
}
