using UnityEngine;

public abstract class DirectionHoverPattern : MonoBehaviour
{
    public float m_emergencyTime;

    public abstract void OnInitialize(params object[] parameters);
    public abstract void OnStart();
    public abstract void OnEnd();

    public abstract void OnEnter(GameObject cameraRig, CameraDirection cameraDirection = null);
    public abstract void OnExit(GameObject cameraRig);

    public abstract void Active();
    public abstract void CountCheck();
    public abstract void OnUpdate();
}