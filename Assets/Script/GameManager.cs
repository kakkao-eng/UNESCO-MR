using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] 
    private int requiredFossilsToComplete = 3; // จำนวนฟอสซิลที่ต้องเก็บเพื่อจบเกม
    
    [SerializeField]
    private TextMeshProUGUI progressText; // แสดงความคืบหน้า
    
    [SerializeField]
    private GameObject gameCompleteUI; // UI แสดงเมื่อจบเกม

    [Header("Events")]
    public UnityEvent onGameComplete; // Event เมื่อจบเกม
    public UnityEvent<int> onFossilCollected; // Event เมื่อเก็บฟอสซิล

    private int fossilsCollected = 0;
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
        fossilsCollected = 0;
        isGameComplete = false;
        if (gameCompleteUI != null)
            gameCompleteUI.SetActive(false);
    }

    // เรียกเมื่อเก็บฟอสซิลได้
    public void CollectFossil()
    {
        if (isGameComplete) return;

        fossilsCollected++;
        onFossilCollected?.Invoke(fossilsCollected);
        
        UpdateUI();
        CheckGameComplete();
    }

    private void UpdateUI()
    {
        if (progressText != null)
        {
            progressText.text = $"ฟอสซิลที่เก็บได้: {fossilsCollected}/{requiredFossilsToComplete}";
        }
    }

    private void CheckGameComplete()
    {
        if (fossilsCollected >= requiredFossilsToComplete && !isGameComplete)
        {
            isGameComplete = true;
            CompleteGame();
        }
    }

    // Change the access modifier of CompleteGame() from private to public
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
    public int GetCollectedFossils() => fossilsCollected;
    public int GetRequiredFossils() => requiredFossilsToComplete;
}
