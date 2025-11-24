using System.Collections;
using System.Collections.Generic;
using Unity.XR.PXR;
using UnityEngine;

public class EnableSeeThrough : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private float enableSeeThroughAfter = 0.5f;

    private void Awake()
    {
        if (mainCamera == null) mainCamera = GetComponent<Camera>();
    }
    private void Start()
    {
        SeeThroughOn();
    }

    public void SeeThroughOn()
    {
        if (mainCamera)
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0, 0, 0, 0);
            StartCoroutine(ToggleSeeThrough(true));
        }
        else
        {
            Debug.LogError("Main camera needs to be referenced or added to this component");
        }
    }

    private IEnumerator ToggleSeeThrough(bool enable)
    {
        yield return new WaitForSeconds(enableSeeThroughAfter);
        PXR_Manager.EnableVideoSeeThrough = enable;
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            PXR_Manager.EnableVideoSeeThrough = true;
        }
    }
}