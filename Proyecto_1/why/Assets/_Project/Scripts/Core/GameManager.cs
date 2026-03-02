using UnityEngine;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Configuración")]
    public LevelData levelData; 

    [Header("UI Toolkit (Frontend)")]
    [SerializeField] private GameUIManager uiManager;

    private int currentLives;
    private int currentFloors = 0;
    public int currentCombo = 0;
    public bool isGameOver = false; 

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            DOTween.SetTweensCapacity(500, 50); 
        }
        else 
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (levelData != null)
        {
            currentLives = levelData.maxLives;
        }
        
        UpdateLivesUI();
        UpdateProgressUI(); 
        if (uiManager != null) uiManager.MostrarCombo(currentCombo);
    }

    public void AddFloorScore()
    {
        if (isGameOver) return; 
        
        currentFloors++;
        UpdateProgressUI(); 
        
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
        
        if (currentFloors < 0)
        {
            ShowGameOver(false);
        }
    }

    private void UpdateProgressUI()
    {
        if (uiManager != null && levelData != null)
        {
            int displayFloors = Mathf.Max(0, currentFloors);
            uiManager.ActualizarPisos(displayFloors, levelData.targetFloors);
        }
    }
    
    public void AddCombo()
    {
        currentCombo++;
        if (uiManager != null) uiManager.MostrarCombo(currentCombo);

        if (currentCombo > 0 && currentCombo % 5 == 0)
        {
            RecoverLife();
        }
    }

    public void ResetCombo()
    {
        currentCombo = 0;
        if (uiManager != null) uiManager.MostrarCombo(currentCombo);
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
    }

    private void UpdateLivesUI()
    {
        if (uiManager != null) uiManager.ActualizarVidas(currentLives);
    }

    private void ShowGameOver(bool victory)
    {
        isGameOver = true;
        if (uiManager != null) uiManager.MostrarGameOver(victory);
    }
}