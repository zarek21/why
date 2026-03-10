using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement; 
using MoreMountains.Feedbacks;

public class GameUIManager : MonoBehaviour
{
    private UIDocument _uiDocument;
    
    private VisualElement _tutorialOverlay;
    private Button _startLevelButton;
    private VisualElement _hudContainer;
    private Label _scoreLabel;
    private Label _livesLabel;

    private Label _comboLabel;
    private VisualElement _resultsOverlay;
    private Label _resultTitleLabel;
    private Button _restartButton;
    private static bool _hasSeenTutorial = false; 

    [Header("Pausa")]
    private VisualElement _pauseOverlay;
    private Button _resumeButton;
    private Button _quitButton;
    private bool _isPaused = false;

    private int _oldFloors = -1;
    private int _oldLives = -1;
    private int _oldCombo = -1;

    [Header("Efectos de Sonido UI (Feel)")]
    [Tooltip("Sonido general al hacer clic en botones principales")]
    [SerializeField] private MMF_Player _buttonClickFeedback; 

    private void OnEnable()
    {
        _uiDocument = GetComponent<UIDocument>();
        VisualElement root = _uiDocument.rootVisualElement;

        _tutorialOverlay = root.Q<VisualElement>("TutorialOverlay");
        _startLevelButton = root.Q<Button>("StartLevelButton");
        _hudContainer = root.Q<VisualElement>("HUDContainer");
        _scoreLabel = root.Q<Label>("ScoreLabel");
        _livesLabel = root.Q<Label>("LivesLabel");

        _comboLabel = root.Q<Label>("ComboLabel");
        _resultsOverlay = root.Q<VisualElement>("ResultsOverlay");
        _resultTitleLabel = root.Q<Label>("ResultTitleLabel");
        _restartButton = root.Q<Button>("RestartButton");

        _pauseOverlay = root.Q<VisualElement>("PauseOverlay");
        _resumeButton = root.Q<Button>("ResumeButton");
        _quitButton = root.Q<Button>("QuitButton");

        if (_startLevelButton != null) _startLevelButton.clicked += OnStartPlayingClicked;
        if (_restartButton != null) _restartButton.clicked += OnRestartClicked;
        if (_resumeButton != null) _resumeButton.clicked += ResumeGame;
        if (_quitButton != null) _quitButton.clicked += QuitToMenu;

        if (_resultsOverlay != null) _resultsOverlay.style.display = DisplayStyle.None;
        if (_pauseOverlay != null) _pauseOverlay.style.display = DisplayStyle.None;
        if (_comboLabel != null) _comboLabel.style.display = DisplayStyle.None;

        if (!_hasSeenTutorial)
        {
            Time.timeScale = 0f; 
            if (_tutorialOverlay != null) _tutorialOverlay.style.display = DisplayStyle.Flex;
            if (_hudContainer != null) _hudContainer.style.display = DisplayStyle.None; 
        }
        else
        {
            Time.timeScale = 1f;
            if (_tutorialOverlay != null) _tutorialOverlay.style.display = DisplayStyle.None;
            if (_hudContainer != null) _hudContainer.style.display = DisplayStyle.Flex; 
        }
    }

    private void OnDisable()
    {
        if (_startLevelButton != null) _startLevelButton.clicked -= OnStartPlayingClicked;
        if (_restartButton != null) _restartButton.clicked -= OnRestartClicked;
        if (_resumeButton != null) _resumeButton.clicked -= ResumeGame;
        if (_quitButton != null) _quitButton.clicked -= QuitToMenu;
    }

    private void Update()
    {
        if (!_hasSeenTutorial) return;
        
        if (_resultsOverlay != null && _resultsOverlay.style.display == DisplayStyle.Flex) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        _isPaused = !_isPaused;
        if (_isPaused)
        {
            Time.timeScale = 0f;
            if (_pauseOverlay != null) _pauseOverlay.style.display = DisplayStyle.Flex;
            if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks();
        }
        else
        {
            ResumeGame();
        }
    }

    private void ResumeGame()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        if (_pauseOverlay != null) _pauseOverlay.style.display = DisplayStyle.None;
        if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks(); 
    }

    private void QuitToMenu()
    {
        if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks(); 
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene"); 
    }

    private void OnStartPlayingClicked()
    {
        if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks();
        
        _hasSeenTutorial = true; 
        if (_tutorialOverlay != null) _tutorialOverlay.style.display = DisplayStyle.None;
        if (_hudContainer != null) _hudContainer.style.display = DisplayStyle.Flex; 
        Time.timeScale = 1f;
    }

    private void OnRestartClicked()
    {
        if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks();
        
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void ActualizarPisos(int floorsBuilt, int targetFloors)
    {
        if (_scoreLabel != null && floorsBuilt != _oldFloors) 
        {
            _scoreLabel.text = $"PISOS: {floorsBuilt} / {targetFloors}";
            _oldFloors = floorsBuilt;
        }
    }

    public void ActualizarVidas(int remainingLives)
    {
        if (_livesLabel != null && remainingLives != _oldLives) 
        {
            _livesLabel.text = $"VIDAS: {remainingLives}";
            _oldLives = remainingLives;
        }
    }

    public void MostrarCombo(int currentCombo)
    {
        if (_comboLabel == null || currentCombo == _oldCombo) return;
        _oldCombo = currentCombo;

        if (currentCombo > 1)
        {
            _comboLabel.style.display = DisplayStyle.Flex;
            _comboLabel.text = $"x{currentCombo}!";

            _comboLabel.style.color = new StyleColor(Color.yellow);

            _comboLabel.AddToClassList("combo-pop");
            
            Invoke("ResetComboPop", 0.15f);
        }
        else
        {
            _comboLabel.style.display = DisplayStyle.None;
        }
    }

    private void ResetComboPop()
    {
        if (_comboLabel != null) _comboLabel.RemoveFromClassList("combo-pop");
    }

    public void MostrarGameOver(bool isVictory)
    {
        if (_resultsOverlay == null || _resultTitleLabel == null) return;

        if (_comboLabel != null) _comboLabel.style.display = DisplayStyle.None;
        if (_hudContainer != null) _hudContainer.style.display = DisplayStyle.None;

        Time.timeScale = 0f;

        _resultsOverlay.style.display = DisplayStyle.Flex;

        if (isVictory)
        {
            _resultTitleLabel.text = "TYPED LIKE A GOOD BOY";
            _resultTitleLabel.style.color = new StyleColor(Color.black);
        }
        else
        {
            _resultTitleLabel.text = "SKILL ISSUE";
            _resultTitleLabel.style.color = new StyleColor(Color.black);
        }
    }
}