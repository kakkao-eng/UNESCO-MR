using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;

/// <summary>
/// จัดการ UI และการเริ่มเกมใน MR
/// รองรับการกด ToolBox ผ่าน MRTK3 Interactable แทน Mouse Input
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("ToolBox Settings")]
    public GameObject toolBoxObject; // วัตถุ ToolBox ในโลก MR (มี Interactable)
    public GameObject toolPanel;     // UI Panel ที่มีปุ่ม Start + วิธีใช้เครื่องมือ

    [Header("Game UI")]
    public GameObject GameOver;   //หน้าตอนแพ้ (เวลาหมด)
    public GameObject WinGame;
    public GameObject ButtonUI;
    public Button startButton;       // ปุ่มเริ่มเกม (UI)
    public Button TryagainButton;
    public Button ExitButton;
    public Image BgTimeText;
    public TMP_Text timerText;       // ข้อความเวลา (TMP)
    public GameObject starUI;
    public Image[] stars;            // ไอคอนดาวแสดงคะแนน

    [Header("Warning Effect")]
    public Image warningOverlay;   // Image เต็มจอสีแดง (Canvas Overlay)
    private Coroutine warningCoroutine;

    [Header("Game Settings")]
    public float gameTime = 120f;    // เวลารวม (2 นาที)
    private float timeLeft;
    private bool gameRunning = false;
    private bool fossilCompleted = false;

    private FossilSpawner fossilSpawner;
    [SerializeField] private SoilGenerator soilGenerator;

    void Start()
    {
        // เริ่มต้นให้แสดงเฉพาะ ToolBox
        toolPanel.SetActive(false);
        timerText.text = FormatTime(gameTime);

        // หา FossilSpawner ในฉาก
        fossilSpawner = FindObjectOfType<FossilSpawner>();

        // ผูก Event ปุ่ม Start
        startButton.onClick.AddListener(StartGame);
        TryagainButton.onClick.AddListener(RestartGame);
        ExitButton.onClick.AddListener(exitGame);

        GameOver.SetActive(false);

        if (warningOverlay != null)
            warningOverlay.gameObject.SetActive(false); // ปิด overlay ตอนเริ่มเกม
    }

    /// <summary>
    /// เรียกจาก MRTK Interactable (OnClicked)
    /// ให้ ToolBox เปิดและแสดงปุ่ม Start
    /// </summary>
    public void OpenToolBoxFromMR()
    {
        toolPanel.SetActive(true);           // แสดง UI เครื่องมือ
        startButton.gameObject.SetActive(true); // แสดงปุ่ม Start
        toolBoxObject.SetActive(false);      // ซ่อน ToolBox ออกจากฉาก
        Debug.Log("ToolBox opened - Start button shown");
    }

    /// <summary>
    /// เริ่มเกมเมื่อต้องการ
    /// </summary>
    void StartGame()
    {
        timeLeft = gameTime;
        gameRunning = true;

        // สร้างฟอสซิลทันทีเมื่อเริ่มเกม
        if (fossilSpawner != null)
        {
            fossilSpawner.Spawn();
        }
        if (soilGenerator != null)
            soilGenerator.SpawnSoil();

        BgTimeText.gameObject.SetActive(true);
        toolPanel.SetActive(false);

        if (warningOverlay != null)
        {
            warningOverlay.gameObject.SetActive(false);
            var color = warningOverlay.color;
            color.a = 0f;
            warningOverlay.color = color;
        }
    }

    void Update()
    {
        if (gameRunning)
        {
            // ลดเวลาลง
            timeLeft -= Time.deltaTime;
            if (timeLeft < 0) timeLeft = 0;

            // อัปเดต UI เวลา
            timerText.text = FormatTime(timeLeft);

            // เตือนเมื่อใกล้หมดเวลา
            if (timeLeft <= 5 && !fossilCompleted && warningCoroutine == null)
            {
                ShowWarningEffect();
                warningCoroutine = StartCoroutine(BlinkWarningOverlay());
            }
            // หมดเวลาแล้วยังไม่เสร็จ
            if (timeLeft <= 0 && !fossilCompleted)
            {
                MissionFail();
            }
        }

        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
            CompleteFossil();
    }

    /// <summary>
    /// เรียกเมื่อขุดฟอสซิลเสร็จ
    /// </summary>
    public void CompleteFossil()
    {
        fossilCompleted = true;
        gameRunning = false;
        starUI.SetActive(true);
        WinGame.SetActive(true);
        ButtonUI.SetActive(true);

        ShowStarScore();

        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
            warningCoroutine = null;
        }

        if (warningOverlay != null)
            warningOverlay.gameObject.SetActive(false);
    }

    /// <summary>
    /// แสดงคะแนนเป็นดาวตามเวลาที่เหลือ
    /// </summary>
    void ShowStarScore()
    {
        int starCount = 0;
        if (timeLeft > 10) starCount = 3;
        else if (timeLeft > 5) starCount = 2;
        else if (timeLeft > 0) starCount = 1;

        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].enabled = (i < starCount);
        }
    }

    /// <summary>
    /// เปลี่ยนสีตัวหนังสือเมื่อใกล้หมดเวลา
    /// </summary>
    void ShowWarningEffect()
    {
        timerText.color = Color.red;
    }

    IEnumerator BlinkWarningOverlay()
    {
        if (warningOverlay == null) yield break;

        warningOverlay.gameObject.SetActive(true);
        Color c = warningOverlay.color;

        while (timeLeft > 0 && timeLeft <= 5 && !fossilCompleted)
        {
            // กระพริบ alpha 0 ↔ 0.4
            c.a = (c.a < 0.2f) ? 0.4f : 0f;
            warningOverlay.color = c;

            yield return new WaitForSeconds(0.5f);
        }

        // ปิด overlay หลังจากหมดเวลา/เกมเสร็จ
        c.a = 0f;
        warningOverlay.color = c;
        warningOverlay.gameObject.SetActive(false);
    }

    /// <summary>
    /// เมื่อหมดเวลาและภารกิจล้มเหลว
    /// </summary>
    void MissionFail()
    {
        gameRunning = false;
        GameOver.SetActive(true);
        ButtonUI.SetActive(true);
        Debug.Log("Mission Fail!");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void exitGame()
    {
        Application.Quit();
    }
    /// <summary>
    /// แปลงเวลาเป็นรูปแบบ mm:ss
    /// </summary>
    string FormatTime(float seconds)
    {
        int min = Mathf.FloorToInt(seconds / 60);
        int sec = Mathf.FloorToInt(seconds % 60);
        return $"{min:00}:{sec:00}";
    }
}
