using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class GameManager : MonoBehaviour
{
    // ลบ requiredFossilsToComplete เพราะไม่จำเป็นต้องใช้แล้ว
    
    [SerializeField]
    private TextMeshProUGUI progressText; // อาจเปลี่ยนเป็นแสดงข้อความอื่นแทน
    
    [SerializeField]
    private GameObject gameCompleteUI;

    [Header("Events")]
    public UnityEvent onGameComplete;
    
    // ลบ onFossilCollected เพราะไม่ต้องนับจำนวนฟอสซิลแล้ว
    
    private bool isGameComplete = false;

    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }

        InitializeGame();
    }

    void Start()
    {
        UpdateUI();
    }

    public void InitializeGame()
    {
        isGameComplete = false;
        if (gameCompleteUI != null)
            gameCompleteUI.SetActive(false);
    }

    private void UpdateUI()
    {
        if (progressText != null)
        {
            progressText.text = "ขุดดินรอบๆ ฟอสซิลให้หมดเพื่อทำให้ฟอสซิลตกลงมา";
        }
    }

    public void CompleteGame()
    {
        if (isGameComplete) return;

        isGameComplete = true;
        if (gameCompleteUI != null)
            gameCompleteUI.SetActive(true);
        onGameComplete?.Invoke();
        Debug.Log("Game Complete!");
    }

    // สำหรับเช็คสถานะเกม
    public bool IsGameComplete() => isGameComplete;
}