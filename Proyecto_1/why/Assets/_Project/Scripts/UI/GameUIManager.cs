using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement; 
using System.Collections.Generic;
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
    private Button _resultsQuitButton;
    private static bool _hasSeenTutorial = false; 

    // Scoreboard Elements
    private Label _resultScoreLabel;
    private VisualElement _highScoreContainer;
    private TextField _highScoreNameInput;
    private Button _submitHighScoreButton;

    [Header("Pausa")]
    private VisualElement _pauseOverlay;
    private Button _resumeButton;
    private Button _settingsButton;
    private Button _quitButton;
    private bool _isPaused = false;

    // Modal de Settings en pausa
    private VisualElement _settingsOverlay;
    private Button _settingsBackButton;

    // Scoreboard en pausa
    private Button _scoreboardButton;
    private VisualElement _scoreboardOverlay;
    private Button _scoreboardBackButton;
    private Button _tabLevelsButton;
    private Button _tabInfiniteButton;
    private ScrollView _scoreboardList;
    private string _currentScoreboardTab = "Levels";

    // FPS en pausa
    private Button _pauseFps60;
    private Button _pauseFps75;
    private Button _pauseFps144;

    private int _oldFloors = -1;
    private int _oldLives = -1;
    private int _oldCombo = -1;
    private float _gameOverTime = -1f;

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
        _resultsQuitButton = root.Q<Button>("ResultsQuitButton");

        // Scoreboard elements
        _resultScoreLabel = root.Q<Label>("ResultScoreLabel");
        _highScoreContainer = root.Q<VisualElement>("HighScoreContainer");
        _highScoreNameInput = root.Q<TextField>("HighScoreNameInput");
        _submitHighScoreButton = root.Q<Button>("SubmitHighScoreButton");

        _pauseOverlay = root.Q<VisualElement>("PauseOverlay");
        _resumeButton = root.Q<Button>("ResumeButton");
        _settingsButton = root.Q<Button>("SettingsButton");
        _quitButton = root.Q<Button>("QuitButton");

        // Modal de Settings
        _settingsOverlay = root.Q<VisualElement>("SettingsOverlay");
        _settingsBackButton = root.Q<Button>("SettingsBackButton");

        // Scoreboard en pausa
        _scoreboardButton = root.Q<Button>("ScoreboardButton");
        _scoreboardOverlay = root.Q<VisualElement>("ScoreboardOverlay");
        _scoreboardBackButton = root.Q<Button>("ScoreboardBackButton");
        _tabLevelsButton = root.Q<Button>("TabLevelsButton");
        _tabInfiniteButton = root.Q<Button>("TabInfiniteButton");
        _scoreboardList = root.Q<ScrollView>("ScoreboardList");

        // FPS en pausa
        _pauseFps60 = root.Q<Button>("PauseFPS60");
        _pauseFps75 = root.Q<Button>("PauseFPS75");
        _pauseFps144 = root.Q<Button>("PauseFPS144");

        if (_startLevelButton != null) _startLevelButton.clicked += OnStartPlayingClicked;
        if (_restartButton != null) _restartButton.clicked += OnRestartClicked;
        if (_resumeButton != null) _resumeButton.clicked += ResumeGame;
        if (_settingsButton != null) _settingsButton.clicked += ShowSettings;
        if (_quitButton != null) _quitButton.clicked += QuitToMenu;
        if (_resultsQuitButton != null) _resultsQuitButton.clicked += QuitToMenu;
        if (_settingsBackButton != null) _settingsBackButton.clicked += HideSettings;
        if (_pauseFps60 != null) _pauseFps60.clicked += () => SetPauseFPS(60);
        if (_pauseFps75 != null) _pauseFps75.clicked += () => SetPauseFPS(75);
        if (_pauseFps144 != null) _pauseFps144.clicked += () => SetPauseFPS(144);
        if (_submitHighScoreButton != null) _submitHighScoreButton.clicked += OnSubmitHighScoreClicked;

        // Scoreboard en pausa events
        if (_scoreboardButton != null) _scoreboardButton.clicked += ShowScoreboard;
        if (_scoreboardBackButton != null) _scoreboardBackButton.clicked += HideScoreboard;
        if (_tabLevelsButton != null) _tabLevelsButton.clicked += OnTabLevelsClicked;
        if (_tabInfiniteButton != null) _tabInfiniteButton.clicked += OnTabInfiniteClicked;

        if (_resultsOverlay != null) _resultsOverlay.style.display = DisplayStyle.None;
        if (_pauseOverlay != null) _pauseOverlay.style.display = DisplayStyle.None;
        if (_settingsOverlay != null) _settingsOverlay.style.display = DisplayStyle.None;
        if (_scoreboardOverlay != null) _scoreboardOverlay.style.display = DisplayStyle.None;
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
        if (_settingsButton != null) _settingsButton.clicked -= ShowSettings;
        if (_quitButton != null) _quitButton.clicked -= QuitToMenu;
        if (_resultsQuitButton != null) _resultsQuitButton.clicked -= QuitToMenu;
        if (_settingsBackButton != null) _settingsBackButton.clicked -= HideSettings;
        if (_submitHighScoreButton != null) _submitHighScoreButton.clicked -= OnSubmitHighScoreClicked;

        if (_scoreboardButton != null) _scoreboardButton.clicked -= ShowScoreboard;
        if (_scoreboardBackButton != null) _scoreboardBackButton.clicked -= HideScoreboard;
        if (_tabLevelsButton != null) _tabLevelsButton.clicked -= OnTabLevelsClicked;
        if (_tabInfiniteButton != null) _tabInfiniteButton.clicked -= OnTabInfiniteClicked;
        // Nota: los lambdas de FPS no se pueden desuscribir, pero se limpian con OnDestroy
    }

    private void Start()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        UnityEngine.WebGLInput.captureAllKeyboardInput = false;
#endif
    }

    private void Update()
    {
        if (!_hasSeenTutorial) return;
        
        if (_resultsOverlay != null && _resultsOverlay.style.display == DisplayStyle.Flex)
        {
            // Cooldown de 0.5 segundos para evitar reinicio accidental inmediato
            if (_gameOverTime > 0f && Time.unscaledTime - _gameOverTime < 0.5f)
            {
                return;
            }

            // Permitir reiniciar al presionar Espacio (siempre y cuando el input de High Score no tenga el foco)
            bool nameInputHasFocus = false;
            if (_highScoreContainer != null && _highScoreContainer.style.display == DisplayStyle.Flex)
            {
                Focusable focused = _uiDocument?.rootVisualElement?.focusController?.focusedElement;
                if (focused != null && focused is VisualElement visualFocused)
                {
                    VisualElement curr = visualFocused;
                    while (curr != null)
                    {
                        if (curr == _highScoreNameInput)
                        {
                            nameInputHasFocus = true;
                            break;
                        }
                        curr = curr.parent;
                    }
                }
            }

            if (!nameInputHasFocus && Input.GetKeyDown(KeyCode.Space))
            {
                OnRestartClicked();
            }
            return;
        }

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
            MainMenuController.SyncFPSButtons(_pauseFps60, _pauseFps75, _pauseFps144);
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
        if (_settingsOverlay != null) _settingsOverlay.style.display = DisplayStyle.None;
        if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks(); 
    }

    private void ShowSettings()
    {
        if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks();
        if (_settingsOverlay != null) _settingsOverlay.style.display = DisplayStyle.Flex;
    }

    private void HideSettings()
    {
        if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks();
        if (_settingsOverlay != null) _settingsOverlay.style.display = DisplayStyle.None;
    }

    private void QuitToMenu()
    {
        if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks(); 
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene"); 
    }

    private void SetPauseFPS(int fps)
    {
        if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks();

        if (LimitFPS.Instance != null)
            LimitFPS.Instance.SetFPS(fps);
        else
            Application.targetFrameRate = fps;

        MainMenuController.SyncFPSButtons(_pauseFps60, _pauseFps75, _pauseFps144);
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
            if (GameManager.SelectedMode == GameMode.Infinite)
            {
                _scoreLabel.text = $"PISOS: {floorsBuilt}";
            }
            else
            {
                _scoreLabel.text = $"PISOS: {floorsBuilt} / {targetFloors}";
            }
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
        _gameOverTime = Time.unscaledTime;

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

        // Scoreboard integration
        int finalScore = 0;
        if (GameManager.Instance != null)
        {
            finalScore = GameManager.Instance.CurrentFloors;
        }

        if (_resultScoreLabel != null)
        {
            _resultScoreLabel.text = $"FLOORS BUILT: {finalScore}";
        }

        string currentMode = GameManager.SelectedMode.ToString();

        if (_highScoreContainer != null && ScoreboardManager.IsHighScore(finalScore, currentMode))
        {
            _highScoreContainer.style.display = DisplayStyle.Flex;
            if (_highScoreNameInput != null)
            {
                _highScoreNameInput.value = "";
            }
        }
        else
        {
            if (_highScoreContainer != null)
            {
                _highScoreContainer.style.display = DisplayStyle.None;
            }
        }
    }

    private void OnSubmitHighScoreClicked()
    {
        if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks();

        int finalScore = 0;
        if (GameManager.Instance != null)
        {
            finalScore = GameManager.Instance.CurrentFloors;
        }

        string currentMode = GameManager.SelectedMode.ToString();
        string playerName = "PLAYER";

        if (_highScoreNameInput != null && !string.IsNullOrEmpty(_highScoreNameInput.value))
        {
            playerName = _highScoreNameInput.value.Trim();
        }

        ScoreboardManager.SaveScore(playerName, finalScore, currentMode);

        if (_highScoreContainer != null)
        {
            _highScoreContainer.style.display = DisplayStyle.None;
        }
    }
}