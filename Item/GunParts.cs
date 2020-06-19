using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;

public class GunParts : Item
{
    public GameObject laserParts;
    public GameObject fireParts;
    public GameObject GrenadeParts;

    private GunPartsType m_gunPartsType;
    private Renderer rend;
    private Material mat;
    private LineRenderer lineRend;
    private Rifle currentGun;
    private KGPlayer GetPlayer = null;
    private int lineCount = 2;
    private int outLineID = 0;

    protected override void OnInit(params object[] parameters)
    {
        base.OnInit(parameters);

        foreach (object element in parameters)
        {
            if (element is GunPartsType)
            {
                m_gunPartsType = (GunPartsType)element;
            }
            if (element is KGPlayer)
            {
                GetPlayer = (KGPlayer)element;
            }
        }
    }

    protected override void OnStart()
    {
        if (GetPlayer == null) Restore();

        if (outLineID == 0)
        {
            outLineID = Shader.PropertyToID("_Outline");
        }

        laserParts.SetActive(false);
        fireParts.SetActive(false);
        GrenadeParts.SetActive(false);

        switch (m_gunPartsType)
        {
            case GunPartsType.None:
                break;
            case GunPartsType.LaserParts:
                laserParts.SetActive(true);
                rend = laserParts.GetComponent<Renderer>();
                break;
            case GunPartsType.FireParts:
                fireParts.SetActive(true);
                rend = fireParts.GetComponent<Renderer>();
                break;
            case GunPartsType.GrenadeParts:
                GrenadeParts.SetActive(true);
                rend = GrenadeParts.GetComponent<Renderer>();
                break;
        }
        mat = rend.materials[0];

        lineRend = transform.GetComponentInChildren<LineRenderer>();
        if (lineRend != null)
        {
            lineRend.enabled = false;
        }

        currentGun = GetPlayer.CurrentWeapon as Rifle;
        lineRend.positionCount = lineCount;

        StartCoroutine(PartsMove());
    }

    public IEnumerator PartsMove()
    {
        Vector3 target = GetPlayer.transform.position + GetPlayer.transform.forward * 2f;
        Vector3 targetTemp = target;
        targetTemp.y += 2f;
        target = targetTemp;

        float elapsedTime = 0;
        float finish1Time = 2f;
        float finish2Time = 5f;
        float moveSpeed = 0.5f;
        
        while (elapsedTime <= finish1Time)
        {
            yield return new WaitForSeconds(0.02f);

            elapsedTime += Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * 2f);
            if (transform.position.y >= target.y) break;
        }

        if (lineRend != null)
        {
            lineRend.enabled = true;
        }

        yield return new WaitForSeconds(1f);

        //AssetManager.Effect.Retrieve("DirectionSound_PartsEquip", transform.position);

        while (elapsedTime <= finish2Time)
        {
            yield return new WaitForSeconds(0.01f);

            elapsedTime += Time.deltaTime;
            moveSpeed += 0.3f;
            transform.position = Vector3.MoveTowards(transform.position, currentGun.m_gunPartPos.position, Time.deltaTime * moveSpeed);
            if (transform.position == currentGun.m_gunPartPos.position)
            {
                break;
            }
        }

        //if (UserType.Host != GameData.Instance.UserType)
        //{
        //    if (GameData.Instance.Player.m_id == GetPlayer.m_id)
        //    {
        //        Network.Instance.SendTCP("item packet", GetGunPartsStartData());
        //    }
        //}
        float partDuration = 20f;

        if(GetPlayer.IsDead)
        {
            partDuration = 0.01f;
        }

        switch (m_gunPartsType)
        {
            case GunPartsType.None:
                break;

            case GunPartsType.LaserParts:
                currentGun.m_laserPart.gameObject.SetActive(true);
                currentGun.m_laserPart.Init(GetPlayer, partDuration);
                break;

            case GunPartsType.FireParts:
                currentGun.m_firePart.gameObject.SetActive(true);
                currentGun.m_firePart.Init(GetPlayer, partDuration);
                break;

            case GunPartsType.GrenadeParts:
                currentGun.m_grenadePart.gameObject.SetActive(true);
                currentGun.m_grenadePart.Init(GetPlayer, partDuration);
                break;
        }

        currentGun.DissolveStart();

        //Effect attachSound = AssetManager.Effect.Retrieve("DirectionSound_PartsEquip", currentGun.m_gunPartPos.position);

        //attachSound.GetComponent<AudioSource>().time = 2.0f;

        Restore();
    }

    private void Update()
    {
        if (mat != null)
        {
            //float value = Random.Range(0.005f, 0.03f);
            float value = Random.Range(0.001f, 0.005f);
            mat.SetFloat(outLineID, value);
        }

        if (currentGun != null)
        {
            transform.rotation = currentGun.m_gunPartPos.rotation;
        }

        if (lineRend.enabled)
        {
            lineRend.SetPosition(0, transform.position);
            lineRend.SetPosition(1, currentGun.m_gunPartPos.position);
        }
    }

    public JObject GetGunPartsStartData()
    {
        JObject itemData = new JObject();

        itemData.Add("playerid", (string)GameData.Instance.Player.m_id);
        itemData.Add("type", (int)ItemState.GunPartsSet);
        itemData.Add("id", m_itemID);
        itemData.Add("parts", (int)m_gunPartsType);

        return itemData;
    }

    public JObject MovePartsSpawn()
    {
        Transform dpr = transform;
        JObject itemData = new JObject();

        itemData.Add("type", (int)ItemState.Spwan);
        itemData.Add("itemtype", (int)ItemType.GunParts);
        itemData.Add("gunpartsType", (int)m_gunPartsType);
        itemData.Add("name", gameObject.name);
        //itemData.Add("id", m_itemID);
        itemData.Add("playerid", (string)GameData.Instance.Player.m_id);

        itemData.Add("dpx", dpr.position.x);
        itemData.Add("dpy", dpr.position.y);
        itemData.Add("dpz", dpr.position.z);

        itemData.Add("drx", dpr.rotation.x);
        itemData.Add("dry", dpr.rotation.y);
        itemData.Add("drz", dpr.rotation.z);
        itemData.Add("drw", dpr.rotation.w);

        return itemData;
    }
}