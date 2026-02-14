using UnityEngine;
using UnityEngine.SceneManagement; 
using TMPro; 
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Configuración")]
    public LevelData levelData; 

    [Header("Interfaz (UI)")]
    [SerializeField] private GameObject resultsPanel; 
    [SerializeField] private TextMeshProUGUI resultText; 
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI progressText;

    private int currentLives;
    private int currentFloors = 0;
    public int currentCombo = 0;
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
        UpdateComboUI(false);
        UpdateProgressUI(); // Llamada inicial
    }

    public void AddFloorScore()
    {
        if (isGameOver) return; 
        
        currentFloors++;
        Debug.Log($"🧱 PISO CONSTRUIDO. Total Lógico: {currentFloors}");
        
        UpdateProgressUI(); // <--- Faltaba llamar la actualización aquí
        
        // Animación de rebote al construir
        if (progressText != null)
        {
            progressText.transform.DOKill(true);
            progressText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
        }
        
        if (levelData != null && currentFloors >= levelData.targetFloors)
        {
            ShowGameOver(true);
        }
    }

    public void RemoveFloorScore()
    {
        if (isGameOver) return;

        currentFloors--;
        UpdateProgressUI();
        
        Debug.Log($"📉 PISO PERDIDO. Total Lógico: {currentFloors}");

        // Animación de error al perder un piso
        if (progressText != null)
        {
            progressText.transform.DOKill(true);
            progressText.transform.DOPunchPosition(Vector3.down * 5f, 0.3f);
            progressText.color = Color.red;
            progressText.DOColor(Color.white, 0.3f);
        }

        if (currentFloors < 0)
        {
            ShowGameOver(false);
        }
    }

    // --- ESTA ES LA FUNCIÓN QUE FALTABA ---
    private void UpdateProgressUI()
    {
        if (progressText != null && levelData != null)
        {
            // Nos aseguramos de que no muestre números negativos visualmente
            int displayFloors = Mathf.Max(0, currentFloors);
            progressText.text = $"PISOS: {displayFloors} / {levelData.targetFloors}";
        }
    }
    // --------------------------------------
    
    public void AddCombo()
    {
        currentCombo++;

        if (comboText != null)
        {
            comboText.transform.DOKill(); 
            comboText.transform.localScale = Vector3.one; 
            comboText.transform.DOPunchScale(Vector3.one * 0.5f, 0.2f, 10, 1); 
        }

        UpdateComboUI(true);

        if (currentCombo % 5 == 0)
        {
            RecoverLife();
        }
    }

    public void ResetCombo()
    {
        if (currentCombo > 0)
        {
             Debug.Log("Combo Breaker!");
        }
        
        currentCombo = 0;
        UpdateComboUI(false); 
    }

    private void UpdateComboUI(bool visible)
    {
        if (comboText == null) return;

        if (currentCombo > 1) 
        {
            comboText.gameObject.SetActive(true);
            comboText.text = $"x{currentCombo}!";
            
            if (currentCombo >= 10) comboText.color = Color.red;       
            else if (currentCombo >= 5) comboText.color = Color.magenta; 
            else comboText.color = Color.yellow;                      
        }
        else
        {
            comboText.gameObject.SetActive(false); 
        }
    }

    public void LoseLife()
    {
        if (isGameOver) return;
        
        ResetCombo(); 

        currentLives--;
        UpdateLivesUI();

        if (currentLives <= 0) ShowGameOver(false);
    }

    private void RecoverLife()
    {
        currentLives++;
        UpdateLivesUI();
        
        if (livesText != null)
        {
            livesText.transform.DOPunchScale(Vector3.one * 0.5f, 0.3f);
            livesText.color = Color.green;
            livesText.DOColor(Color.white, 0.5f);
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
        if (comboText != null) comboText.gameObject.SetActive(false);

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