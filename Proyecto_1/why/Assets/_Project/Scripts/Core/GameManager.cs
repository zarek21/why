using UnityEngine;
using DG.Tweening;

// MODOS DE JUEGO
public enum GameMode
{
    Levels,
    Infinite
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static GameMode SelectedMode = GameMode.Levels;

    [Header("Configuración")]
    public LevelData LevelData; 

    [Header("UI Toolkit (Frontend)")]
    [SerializeField] private GameUIManager _uiManager;

    private int _currentLives;
    private int _currentFloors = 0;

    public int CurrentCombo = 0;

    public bool IsGameOver = false; 

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            DOTween.SetTweensCapacity(500, 50);

            // TECLAS EN WEB GL 
            #if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput = true;
            #endif
        }
        else 
        {
            Destroy(gameObject);
        }


    }
    
    private void Start()
    {
        if (LevelData != null)
        {
            _currentLives = LevelData.MaxLives;
        }
        
        UpdateLivesUI();
        UpdateProgressUI(); 
        if (_uiManager != null) _uiManager.MostrarCombo(CurrentCombo);
    }

    public void AddFloorScore()
    {
        if (IsGameOver) return; 
        
        _currentFloors++;
        UpdateProgressUI(); 
        
        // Solo verificar victoria en modo Niveles
        if (SelectedMode == GameMode.Levels && LevelData != null && _currentFloors >= LevelData.TargetFloors)
        {
            ShowGameOver(true);
        }
    }

    public void RemoveFloorScore()
    {
        if (IsGameOver) return;

        _currentFloors--;
        UpdateProgressUI();
        
        if (_currentFloors < 0)
        {
            ShowGameOver(false);
        }
    }

    private void UpdateProgressUI()
    {
        if (_uiManager != null && LevelData != null)
        {
            int displayFloors = Mathf.Max(0, _currentFloors);
            _uiManager.ActualizarPisos(displayFloors, LevelData.TargetFloors);
        }
    }
    
    public void AddCombo()
    {
        CurrentCombo++;
        if (_uiManager != null) _uiManager.MostrarCombo(CurrentCombo);

        if (CurrentCombo > 0 && CurrentCombo % 5 == 0)
        {
            RecoverLife();
        }
    }

    public void ResetCombo()
    {
        CurrentCombo = 0;
        if (_uiManager != null) _uiManager.MostrarCombo(CurrentCombo);
    }

    public void LoseLife()
    {
        if (IsGameOver) return;
        
        ResetCombo(); 
        _currentLives--;
        UpdateLivesUI();

        if (_currentLives <= 0) ShowGameOver(false);
    }

    private void RecoverLife()
    {
        _currentLives++;
        UpdateLivesUI();
    }

    private void UpdateLivesUI()
    {
        if (_uiManager != null) _uiManager.ActualizarVidas(_currentLives);
    }

    private void ShowGameOver(bool isVictory)
    {
        IsGameOver = true;
        if (_uiManager != null) _uiManager.MostrarGameOver(isVictory);
    }
}