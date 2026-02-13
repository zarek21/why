using UnityEngine;
using UnityEngine.SceneManagement; 
using TMPro; 

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Configuración")]
    public LevelData levelData; 

    [Header("Interfaz (UI)")]
    [SerializeField] private GameObject resultsPanel; 
    [SerializeField] private TextMeshProUGUI resultText; 
    [SerializeField] private TextMeshProUGUI livesText;
    private int currentLives;

    private int currentFloors = 0;
    public bool isGameOver = false; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    private void Start()
    {
        if (levelData != null)
        {
           
            currentLives = levelData.maxLives;
        }
        UpdateLivesUI();
    }

    public void AddFloorScore()
    {
        if (isGameOver) return; // Si ya terminó, ignorar

        currentFloors++;
        
        // Chequeo de Victoria
        if (levelData != null && currentFloors >= levelData.targetFloors)
        {
            ShowGameOver(true);
        }
    }

    public void RemoveFloorScore()
    {
        if (isGameOver) return;

        currentFloors--;
        
        // Chequeo de Derrota
        if (currentFloors < 0)
        {
            ShowGameOver(false);
        }
    }
    
    public void LoseLife()
    {
        if (isGameOver) return;

        currentLives--;
        UpdateLivesUI();

        if (currentLives <= 0)
        {
            ShowGameOver(false); 
        }
    }

    private void UpdateLivesUI()
    {
        if (livesText != null)
        {
            livesText.text = $"VIDAS: {currentLives}";
        }
    }

    private void ShowGameOver(bool victory)
    {
        isGameOver = true;
        
        if (resultsPanel != null) resultsPanel.SetActive(true);

        if (resultText != null)
        {
            if (victory)
            {
                resultText.text = "¡CONSTRUIDO!";
                resultText.color = Color.green;
            }
            else
            {
                resultText.text = "COLAPSO";
                resultText.color = Color.red;
            }
        }
    }
    
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
}