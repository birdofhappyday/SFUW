using System;
using System.Collections.Generic;
using UnityEngine;

class CollisionDirection : MonoBehaviour
{
    private bool isFirst = true;

    public void OnTriggerEnter(Collider collider)
    {
        if(isFirst == true)
        {
            if(collider.transform.parent != null)
            {
                HoverCraftSegTwo hover = collider.transform.parent.GetComponent<HoverCraftSegTwo>();

                if(null != hover)
                {
                    GameData.Instance.HoverCraftSegTwo.RocketBombing();
                    isFirst = false;
                }
            }
        }
    }
}
