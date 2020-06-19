using UnityEngine;
using System.Collections;

public class PositionReset : MonoBehaviour
{
    Transform t;
    Vector3 startPosition;
    Quaternion startRotation;
    Vector3 startScale;
    bool isInitialized;

    void OnEnable()
    {
        t = transform;
        startPosition = t.localPosition;
        startRotation = t.localRotation;
        startScale = t.localScale;

        if (!isInitialized)
        {
            isInitialized = true;
            t = transform;
            startPosition = t.localPosition;
            startRotation = t.localRotation;
            startScale = t.localScale;
        }
        else
        {
            t.localPosition = startPosition;
            t.localRotation = startRotation;
            t.localScale = startScale;
        }
    }

    void OnDisable()
    {
        if (!isInitialized)
        {
            isInitialized = true;
            t = transform;
            startPosition = t.localPosition;
            startRotation = t.localRotation;
            startScale = t.localScale;
        }
        else
        {
            t.localPosition = startPosition;
            t.localRotation = startRotation;
            t.localScale = startScale;
        }
    }
}
