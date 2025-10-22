using UnityEngine;
using UnityEngine.XR;

public class Glue : MonoBehaviour
{
    [Header("Glue Settings")]
    public float glueRadius = 0.3f;
    public float repairStrength = 0.2f;
    public Vector3 glueOffset = new Vector3(0, 0, 0.2f);

    [Header("Effects")]
    public ParticleSystem glueEffect;

    [Header("Debug Visualization")]
    public bool showGlueLine = true;
    public Color glueLineColor = Color.cyan;

    private InputDevice rightHand;
    private Vector3 lastUsedPoint;
    private bool isGrabbed = false;
    private bool isUsingGlue = false;

    private void Start()
    {
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    private void Update()
    {
        if (!isGrabbed)
        {
            StopGlueSound();
            return;
        }

        bool triggerPressed = false;
        if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool pressed))
            triggerPressed = pressed;

        if (triggerPressed)
        {
            if (!isUsingGlue)
            {
                isUsingGlue = true;
                PlayGlueSound();
            }

            Vector3 targetPoint = transform.position + transform.TransformDirection(glueOffset);
            UseGlue(targetPoint);
            lastUsedPoint = targetPoint;
        }
        else if (isUsingGlue)
        {
            isUsingGlue = false;
            StopGlueSound();
        }
    }

    /// <summary>
    /// ใช้กาวเพื่อซ่อมฟอสซิล
    /// </summary>
    private void UseGlue(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(position, glueRadius);
        bool repaired = false;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<Fossil>(out var fossil) &&
                fossil.GetCurrentState() == Fossil.FossilState.Damaged)
            {
                fossil.Repair();
                repaired = true;

                if (glueEffect != null)
                {
                    var effect = Instantiate(glueEffect, hit.transform.position, Quaternion.identity);
                    effect.transform.rotation = Quaternion.LookRotation(Vector3.up);
                    Destroy(effect.gameObject, 2f);
                }
            }
        }

        // เสียงซ่อมเล็กๆ (เพิ่มฟีลสมจริง)
        if (repaired)
            SoundManager.PlaySound(SoundType.Glue, 0.6f);
    }

    private void PlayGlueSound()
    {
        SoundManager.PlayLoop(SoundType.Glue);
    }

    private void StopGlueSound()
    {
        // แก้ไข: ใช้ StopLoopSound เพื่อหยุดเสียงกาวที่เล่นวนลูป
        SoundManager.StopLoopSound(); 
    }

    public void OnGrabbed()
    {
        isGrabbed = true;
        Debug.Log("Glue grabbed!");
    }

    public void OnReleased()
    {
        isGrabbed = false;
        StopGlueSound();
        Debug.Log("Glue released!");
    }

    private void OnDrawGizmos()
    {
        if (!showGlueLine) return;

        Vector3 drawPosition = lastUsedPoint != Vector3.zero
            ? lastUsedPoint
            : transform.position + transform.TransformDirection(glueOffset);

        Gizmos.color = new Color(0, 1, 1, 0.2f);
        Gizmos.DrawSphere(drawPosition, glueRadius);
    }
}
