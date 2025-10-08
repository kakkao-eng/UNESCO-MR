using UnityEngine;
using UnityEngine.UI;

public class PlayAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    public Animator animator;                  // Animator ของโมเดล
    public string animationStateName = "Idle"; // ชื่อ State ที่ต้องการเล่น

    [Header("Rotation Settings")]
    public float rotationSpeed = 30f;          // องศาต่อวินาที
    public bool rotate = false;                // เริ่มไม่หมุน

    [Header("UI Settings")]
    public string buttonName = "PlayAnimation";   // ชื่อ Button ใน Scene

    private Button playButton;
    private bool isPlaying = false;

    void Start()
    {
        // หา Button อัตโนมัติใน Scene
        GameObject btnObj = GameObject.Find(buttonName);
        if (btnObj != null)
        {
            playButton = btnObj.GetComponent<Button>();
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayButtonPressed);
        }
        else
        {
            Debug.LogWarning($"Button ชื่อ {buttonName} ไม่พบใน Scene");
        }
    }

    void Update()
    {
        if (isPlaying && rotate)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    void OnPlayButtonPressed()
    {
        if (animator != null && !string.IsNullOrEmpty(animationStateName))
        {
            animator.Play(animationStateName);
        }

        rotate = true;
        isPlaying = true;
        SoundManager.PlaySound(SoundType.ClickButton);

        if (playButton != null)
            playButton.gameObject.SetActive(false);
    }
}
