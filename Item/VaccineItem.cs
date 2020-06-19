using UnityEngine;
using System.Collections;

public class VaccineItem : Item
{
    public GameObject originVaccine;
    public GameObject vaccineEffect;
    public GameObject[] vaccineLight;

    private Coroutine cor;
    private Effect[] effect = null;

    protected override void OnInit(params object[] parameters)
    {
        base.OnInit(parameters);

        transform.position += Vector3.up * 2f;
    }

    protected override void OnStart()
    {
        AssetManager.Effect.Retrieve("ItemAppearEffect", transform);

        effect = new Effect[PlayerManager.Instance.GetPlayerList.Count];

        StartCoroutine(MoveVaccine());
    }

    protected override void OnEnd()
    {
        base.OnEnd();
    }

    public IEnumerator MoveVaccine()
    {
        yield return new WaitForSeconds(1f);

        cor = StartCoroutine(VaccineRotate());

        KGPlayer[] player = new KGPlayer[PlayerManager.Instance.GetPlayerList.Count];
        
        float speed = 0.5f;
        float elapsedTime = 0f;

        Vector3 target = transform.position + Vector3.up * 2.5f;

        while (elapsedTime <= 5f)
        {
            yield return new WaitForSeconds(0.01f);

            elapsedTime += Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);

            if (transform.position.y == target.y) break;
        }

        yield return new WaitForSeconds(2f);

        DirectionManager.Instance.OpenNarration(220070);

        //StartCoroutine(DelayNarr());

        StopCoroutine(cor);

        //originVaccine.SetActive(false);

        (originVaccine.GetComponent("MfxController2") as MonoBehaviour).enabled = true;

        yield return new WaitForSeconds(2f);

        vaccineEffect.SetActive(false);

        for (int i = 0; i < player.Length; ++i)
        {
            player[i] = PlayerManager.Instance.GetPlayerList[i];
            vaccineLight[i].SetActive(true);
        }

        int count = player.Length;

        while (count > 0)
        {
            yield return new WaitForSeconds(0.01f);
            speed += 0.06f;
            
            for (int i = 0; i < player.Length; ++i)
            {
                if (!vaccineLight[i].activeSelf) continue;

                //float playerCenter = (int)player[i].Height / 2;
                Vector3 pos = player[i].transform.position + Vector3.up * 1.2f;

                vaccineLight[i].transform.position = Vector3.MoveTowards(vaccineLight[i].transform.position, pos, Time.deltaTime * speed);
                
                if (vaccineLight[i].transform.position == pos)
                {
                    effect[i] = AssetManager.Effect.Retrieve("FX_Direction_VaccineShield", pos);
                    vaccineLight[i].SetActive(false);
                    --count;
                }
            }
        }

        StartCoroutine(ShieldTrackingPlayer());

        yield return new WaitForSeconds(3f);

        if (GameData.Instance.UserType == UserType.Host)
        {
            EventManager.Instance.Notify<IEventAfterDirecting>((receiver) => receiver.AfterDirecting());
        }

        //Restore();
    }

    public IEnumerator VaccineRotate()
    {
        while (true)
        {
            yield return null;
            if (originVaccine.activeSelf)
            {
                originVaccine.transform.Rotate(Vector3.up * Time.deltaTime * 30f, Space.World);

                if (originVaccine.transform.localRotation.y >= 360f)
                {
                    originVaccine.transform.localRotation = Quaternion.identity;
                }
            }
        }
    }

    public IEnumerator ShieldTrackingPlayer()
    {
        //쉴드가 플레이어 따라다니게 하기 
        while (true)
        {
            yield return null;

            for (int i = 0; i < effect.Length; ++i)
            {
                if (effect[i] == null) continue;

                Vector3 pos = PlayerManager.Instance.GetPlayerList[i].transform.position + Vector3.up * 1.2f;
                effect[i].transform.position = pos;
            }
        }
    }

    //public IEnumerator DelayNarr()
    //{
    //    yield return new WaitForSeconds(1f);

    //    DirectionManager.Instance.OpenNarration(21010);
    //}
}