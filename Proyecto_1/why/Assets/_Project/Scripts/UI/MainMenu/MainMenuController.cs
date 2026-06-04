using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement; 
using DG.Tweening;
using MoreMountains.Feedbacks;

public class MainMenuController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private MenuKeyboardAnim _keyboardAnim;

    [Header("Transición")]
    [SerializeField] private float _finalTextFadeDuration = 0.5f;

    [Header("Efectos UI (Feel)")]
    [Tooltip("Feedback que sonará al dar clic en los botones del menú")]
    [SerializeField] private MMF_Player _buttonClickFeedback; 

    [Header("Navegación y Carga")]
    [SerializeField] private string _gameSceneName = "GameScene";
    [SerializeField] private float _fadeToBlackDuration = 1.0f;
    [Tooltip("Tiempo de espera artificial (en segundos) para ver la pantalla de carga")]
    [SerializeField] private float _artificialLoadTime = 4f;

    [Header("Niveles")]
    [SerializeField] private List<LevelData> _levels = new List<LevelData>();

    private UIDocument _uiDocument;
    private Label _subtitleLabel; 
    private VisualElement _finalTextContainer;
    
    private VisualElement _loadingScreen; 
    private VisualElement _loadingSpinner;
    private Button _playButton;
    private Button _settingsButton;
    private Button _exitButton;

    // Modal de selección de modo
    private VisualElement _modeSelectionOverlay;
    private Button _levelsButton;
    private Button _infiniteButton;
    private Button _modeBackButton;

    // Modal de settings
    private VisualElement _settingsOverlay;
    private Button _fps60Button;
    private Button _fps75Button;
    private Button _fps144Button;
    private Button _settingsBackButton;

    // Scoreboard
    private Button _scoreboardButton;
    private VisualElement _scoreboardOverlay;
    private Button _scoreboardBackButton;
    private Button _tabLevelsButton;
    private Button _tabInfiniteButton;
    private ScrollView _scoreboardList;
    private string _currentScoreboardTab = "Levels";

    // Level Selector Overlay
    private VisualElement _levelSelectorOverlay;
    private ScrollView _levelsList;
    private Button _levelSelectorBackButton;

    private bool _isTransitioning = false;
    private void OnEnable()
    {
        _uiDocument = GetComponent<UIDocument>();
        VisualElement root = _uiDocument.rootVisualElement;

        _subtitleLabel = root.Q<Label>("SubtitleLabel");
        _finalTextContainer = root.Q<VisualElement>("finalTextContainer");
        
        _playButton = root.Q<Button>("PlayButton");
        _settingsButton = root.Q<Button>("SettingsButton");
        _exitButton = root.Q<Button>("ExitButton"); 
        
        _loadingScreen = root.Q<VisualElement>("LoadingScreen");
        _loadingSpinner = root.Q<VisualElement>("LoadingSpinner"); 

        // Modal de selección de modo
        _modeSelectionOverlay = root.Q<VisualElement>("ModeSelectionOverlay");
        _levelsButton = root.Q<Button>("LevelsButton");
        _infiniteButton = root.Q<Button>("InfiniteButton");
        _modeBackButton = root.Q<Button>("BackButton");

        // Modal de settings
        _settingsOverlay = root.Q<VisualElement>("SettingsOverlay");
        _fps60Button = root.Q<Button>("FPS60Button");
        _fps75Button = root.Q<Button>("FPS75Button");
        _fps144Button = root.Q<Button>("FPS144Button");
        _settingsBackButton = root.Q<Button>("SettingsBackButton");

        // Scoreboard
        _scoreboardButton = root.Q<Button>("ScoreboardButton");
        _scoreboardOverlay = root.Q<VisualElement>("ScoreboardOverlay");
        _scoreboardBackButton = root.Q<Button>("ScoreboardBackButton");
        _tabLevelsButton = root.Q<Button>("TabLevelsButton");
        _tabInfiniteButton = root.Q<Button>("TabInfiniteButton");
        _scoreboardList = root.Q<ScrollView>("ScoreboardList");

        // Level Selector Overlay
        _levelSelectorOverlay = root.Q<VisualElement>("LevelSelectorOverlay");
        _levelsList = root.Q<ScrollView>("LevelsList");
        _levelSelectorBackButton = root.Q<Button>("LevelSelectorBackButton");

        if (_playButton != null) _playButton.clicked += OnPlayClicked;
        if (_settingsButton != null) _settingsButton.clicked += OnSettingsClicked;
        if (_exitButton != null) _exitButton.clicked += OnExitClicked;
        if (_levelsButton != null) _levelsButton.clicked += OnLevelsClicked;
        if (_infiniteButton != null) _infiniteButton.clicked += OnInfiniteClicked;
        if (_modeBackButton != null) _modeBackButton.clicked += OnModeBackClicked;
        if (_fps60Button != null) _fps60Button.clicked += () => SetFPS(60);
        if (_fps75Button != null) _fps75Button.clicked += () => SetFPS(75);
        if (_fps144Button != null) _fps144Button.clicked += () => SetFPS(144);
        if (_settingsBackButton != null) _settingsBackButton.clicked += OnSettingsBackClicked;

        // Scoreboard listeners
        if (_scoreboardButton != null) _scoreboardButton.clicked += OnScoreboardClicked;
        if (_scoreboardBackButton != null) _scoreboardBackButton.clicked += OnScoreboardBackClicked;
        if (_tabLevelsButton != null) _tabLevelsButton.clicked += OnTabLevelsClicked;
        if (_tabInfiniteButton != null) _tabInfiniteButton.clicked += OnTabInfiniteClicked;

        // Level Selector listeners
        if (_levelSelectorBackButton != null) _levelSelectorBackButton.clicked += OnLevelSelectorBackClicked;

        if (_finalTextContainer != null)
        {
            _finalTextContainer.style.opacity = 0f;
            _finalTextContainer.style.display = DisplayStyle.None;
        }

        if (_loadingScreen != null)
        {
            _loadingScreen.style.opacity = 0f;
            _loadingScreen.style.display = DisplayStyle.None;
        }

        if (_modeSelectionOverlay != null)
        {
            _modeSelectionOverlay.style.opacity = 0f;
            _modeSelectionOverlay.style.display = DisplayStyle.None;
        }

        if (_settingsOverlay != null)
        {
            _settingsOverlay.style.opacity = 0f;
            _settingsOverlay.style.display = DisplayStyle.None;
        }

        if (_scoreboardOverlay != null)
        {
            _scoreboardOverlay.style.opacity = 0f;
            _scoreboardOverlay.style.display = DisplayStyle.None;
        }

        if (_levelSelectorOverlay != null)
        {
            _levelSelectorOverlay.style.opacity = 0f;
            _levelSelectorOverlay.style.display = DisplayStyle.None;
        }
        


        // Sincronizar el estado visual de los botones FPS
        SyncFPSButtons(_fps60Button, _fps75Button, _fps144Button);
    }

    private void OnDisable() 
    {
        if (_playButton != null) _playButton.clicked -= OnPlayClicked;
        if (_settingsButton != null) _settingsButton.clicked -= OnSettingsClicked;
        if (_exitButton != null) _exitButton.clicked -= OnExitClicked;
        if (_levelsButton != null) _levelsButton.clicked -= OnLevelsClicked;
        if (_infiniteButton != null) _infiniteButton.clicked -= OnInfiniteClicked;
        if (_modeBackButton != null) _modeBackButton.clicked -= OnModeBackClicked;
        if (_settingsBackButton != null) _settingsBackButton.clicked -= OnSettingsBackClicked;

        if (_scoreboardButton != null) _scoreboardButton.clicked -= OnScoreboardClicked;
        if (_scoreboardBackButton != null) _scoreboardBackButton.clicked -= OnScoreboardBackClicked;
        if (_tabLevelsButton != null) _tabLevelsButton.clicked -= OnTabLevelsClicked;
        if (_tabInfiniteButton != null) _tabInfiniteButton.clicked -= OnTabInfiniteClicked;

        if (_levelSelectorBackButton != null) _levelSelectorBackButton.clicked -= OnLevelSelectorBackClicked;
    }

    private void Start()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        UnityEngine.WebGLInput.captureAllKeyboardInput = false;
#endif
    }

    private void Update()
    {
        if (!_isTransitioning && Input.anyKeyDown)
        {
            _isTransitioning = true;
            
            Tween keyboardTween = _keyboardAnim?.AnimateToFinalState();

            if (_subtitleLabel != null)
            {
                DOTween.To(() => _subtitleLabel.resolvedStyle.opacity, x => _subtitleLabel.style.opacity = x, 0f, 0.5f)
                       .OnComplete(() => _subtitleLabel.style.display = DisplayStyle.None);
            }

            if (keyboardTween != null)
            {
                keyboardTween.OnComplete(ShowFinalMenu);
            }
            else
            {
                ShowFinalMenu();
            }
        }
    }

    private void ShowFinalMenu()
    {
        if (_finalTextContainer != null)
        {
            _finalTextContainer.style.display = DisplayStyle.Flex;
            DOTween.To(() => _finalTextContainer.resolvedStyle.opacity, x => _finalTextContainer.style.opacity = x, 1f, _finalTextFadeDuration);
        }
    }


    // MODAL DE SELECCIÓN DE MODO 
    private void OnPlayClicked()
    {
        if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks();
        ShowModeSelection();
    }

    private void ShowModeSelection()
    {
        if (_modeSelectionOverlay == null) return;
        _modeSelectionOverlay.style.display = DisplayStyle.Flex;
        DOTween.To(() => _modeSelectionOverlay.resolvedStyle.opacity, x => _modeSelectionOverlay.style.opacity = x, 1f, 0.3f);
    }

    private void HideModeSelection()
    {
        if (_modeSelectionOverlay == null) return;
        DOTween.To(() => _modeSelectionOverlay.resolvedStyle.opacity, x => _modeSelectionOverlay.style.opacity = x, 0f, 0.3f)
               .OnComplete(() => _modeSelectionOverlay.style.display = DisplayStyle.None);
    }

    private void OnLevelsClicked()
    {
        if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks();
        HideModeSelection();
        ShowLevelSelector();
    }

    private void OnInfiniteClicked()
    {
        if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks();
        GameManager.SelectedMode = GameMode.Infinite;
        StartGameLoad();
    }

    private void OnModeBackClicked()
    {
        if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks();
        HideModeSelection();
    }

    // =============================================
    // LEVEL SELECTOR
    // =============================================

    private void OnLevelSelectorBackClicked()
    {
        if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks();
        HideLevelSelector();
        ShowModeSelection();
    }

    private void ShowLevelSelector()
    {
        if (_levelSelectorOverlay == null) return;
        _levelSelectorOverlay.style.display = DisplayStyle.Flex;
        DOTween.To(() => _levelSelectorOverlay.resolvedStyle.opacity, x => _levelSelectorOverlay.style.opacity = x, 1f, 0.3f);
        RenderLevels();
    }

    private void HideLevelSelector()
    {
        if (_levelSelectorOverlay == null) return;
        DOTween.To(() => _levelSelectorOverlay.resolvedStyle.opacity, x => _levelSelectorOverlay.style.opacity = x, 0f, 0.3f)
               .OnComplete(() => _levelSelectorOverlay.style.display = DisplayStyle.None);
    }

    private void RenderLevels()
    {
        if (_levelsList == null) return;
        _levelsList.Clear();

        if (_levels == null || _levels.Count == 0)
        {
            Label noLevelsLabel = new Label("NO LEVELS AVAILABLE");
            noLevelsLabel.AddToClassList("scoreboard-rank-name");
            noLevelsLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            noLevelsLabel.style.marginTop = 50f;
            _levelsList.Add(noLevelsLabel);
            return;
        }

        foreach (LevelData lvl in _levels)
        {
            if (lvl == null) continue;

            VisualElement row = new VisualElement();
            row.AddToClassList("level-row");

            // Info Container
            VisualElement infoContainer = new VisualElement();
            infoContainer.AddToClassList("level-info-container");

            // Level Name
            string displayLvlName = string.IsNullOrEmpty(lvl.LevelName) ? $"LEVEL {lvl.LevelIndex}" : lvl.LevelName.ToUpper();
            Label nameLabel = new Label(displayLvlName);
            nameLabel.AddToClassList("level-name-label");
            infoContainer.Add(nameLabel);

            // Level Details Row
            VisualElement detailsContainer = new VisualElement();
            detailsContainer.AddToClassList("level-details-container");

            // Floors Tag
            Label floorsTag = new Label($"{lvl.TargetFloors} FLOORS");
            floorsTag.AddToClassList("level-detail-tag");
            detailsContainer.Add(floorsTag);

            // Time Tag
            Label timeTag = new Label($"{lvl.BaseTimePerWord}S / WORD");
            timeTag.AddToClassList("level-detail-tag");
            detailsContainer.Add(timeTag);

            // Difficulty Tag
            string diffText = "EASY";
            if (lvl.BaseTimePerWord <= 3f) diffText = "HARD";
            else if (lvl.BaseTimePerWord <= 5f) diffText = "MEDIUM";
            
            Label diffTag = new Label(diffText);
            diffTag.AddToClassList("level-detail-tag");
            diffTag.AddToClassList("level-detail-tag-purple");
            detailsContainer.Add(diffTag);

            infoContainer.Add(detailsContainer);
            row.Add(infoContainer);

            // Play Button
            Button playBtn = new Button();
            playBtn.text = "PLAY";
            playBtn.AddToClassList("level-play-button");
            
            LevelData capturedLvl = lvl;
            playBtn.clicked += () => {
                if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks();
                GameManager.SelectedMode = GameMode.Levels;
                GameManager.SelectedLevelData = capturedLvl;
                HideLevelSelector();
                StartGameLoad();
            };

            row.Add(playBtn);
            _levelsList.Add(row);
        }
    }


    // MODAL DE SETTINGS
    private void OnSettingsClicked()
    {
        if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks();
        ShowSettings();
    }

    private void ShowSettings()
    {
        if (_settingsOverlay == null) return;
        _settingsOverlay.style.display = DisplayStyle.Flex;
        DOTween.To(() => _settingsOverlay.resolvedStyle.opacity, x => _settingsOverlay.style.opacity = x, 1f, 0.3f);
    }

    private void HideSettings()
    {
        if (_settingsOverlay == null) return;
        DOTween.To(() => _settingsOverlay.resolvedStyle.opacity, x => _settingsOverlay.style.opacity = x, 0f, 0.3f)
               .OnComplete(() => _settingsOverlay.style.display = DisplayStyle.None);
    }

    private void OnSettingsBackClicked()
    {
        if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks();
        HideSettings();
    }

    // =============================================
    // SCOREBOARD
    // =============================================

    private void OnScoreboardClicked()
    {
        if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks();
        ShowScoreboard();
    }

    private void ShowScoreboard()
    {
        if (_scoreboardOverlay == null) return;
        _scoreboardOverlay.style.display = DisplayStyle.Flex;
        DOTween.To(() => _scoreboardOverlay.resolvedStyle.opacity, x => _scoreboardOverlay.style.opacity = x, 1f, 0.3f);
        RenderScoreboard();
    }

    private void HideScoreboard()
    {
        if (_scoreboardOverlay == null) return;
        DOTween.To(() => _scoreboardOverlay.resolvedStyle.opacity, x => _scoreboardOverlay.style.opacity = x, 0f, 0.3f)
               .OnComplete(() => _scoreboardOverlay.style.display = DisplayStyle.None);
    }

    private void OnScoreboardBackClicked()
    {
        if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks();
        HideScoreboard();
    }

    private void OnTabLevelsClicked()
    {
        OnScoreboardTabClicked("Levels");
    }

    private void OnTabInfiniteClicked()
    {
        OnScoreboardTabClicked("Infinite");
    }

    private void OnScoreboardTabClicked(string mode)
    {
        if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks();
        
        _currentScoreboardTab = mode;
        if (mode == "Levels")
        {
            _tabLevelsButton?.AddToClassList("fps-active");
            _tabInfiniteButton?.RemoveFromClassList("fps-active");
        }
        else
        {
            _tabLevelsButton?.RemoveFromClassList("fps-active");
            _tabInfiniteButton?.AddToClassList("fps-active");
        }
        RenderScoreboard();
    }

    private void RenderScoreboard()
    {
        if (_scoreboardList == null) return;
        _scoreboardList.Clear();

        List<ScoreEntry> allScores = ScoreboardManager.GetScores();
        List<ScoreEntry> filteredScores = allScores.FindAll(s => s.mode == _currentScoreboardTab);

        // Sort score entries: highest score first
        filteredScores.Sort((a, b) => b.score.CompareTo(a.score));

        if (filteredScores.Count == 0)
        {
            Label noScoresLabel = new Label("NO SCORES YET");
            noScoresLabel.AddToClassList("scoreboard-rank-name");
            noScoresLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            noScoresLabel.style.marginTop = 50f;
            _scoreboardList.Add(noScoresLabel);
            return;
        }

        for (int i = 0; i < filteredScores.Count; i++)
        {
            ScoreEntry entry = filteredScores[i];
            
            VisualElement row = new VisualElement();
            row.AddToClassList("scoreboard-row");

            Label nameLabel = new Label($"{i + 1}. {entry.playerName}");
            nameLabel.AddToClassList("scoreboard-rank-name");

            Label scoreLabel = new Label($"{entry.score} PISOS");
            scoreLabel.AddToClassList("scoreboard-score");

            Label dateLabel = new Label(entry.date);
            dateLabel.AddToClassList("scoreboard-date");

            row.Add(nameLabel);
            row.Add(scoreLabel);
            row.Add(dateLabel);

            _scoreboardList.Add(row);
        }
    }


    // =============================================
    // FPS — Lógica compartida
    // =============================================

    private void SetFPS(int fps)
    {
        if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks();

        if (LimitFPS.Instance != null)
        {
            LimitFPS.Instance.SetFPS(fps);
        }
        else
        {
            Application.targetFrameRate = fps;
        }

        SyncFPSButtons(_fps60Button, _fps75Button, _fps144Button);
    }

    public static void SyncFPSButtons(Button btn60, Button btn75, Button btn144)
    {
        int currentFPS = LimitFPS.Instance != null ? LimitFPS.Instance.GetCurrentFPS() : Application.targetFrameRate;

        btn60?.RemoveFromClassList("fps-active");
        btn75?.RemoveFromClassList("fps-active");
        btn144?.RemoveFromClassList("fps-active");

        if (currentFPS <= 60) btn60?.AddToClassList("fps-active");
        else if (currentFPS <= 75) btn75?.AddToClassList("fps-active");
        else btn144?.AddToClassList("fps-active");
    }

    // =============================================
    // CARGA DE ESCENA
    // =============================================

    private void StartGameLoad()
    {
        if (string.IsNullOrEmpty(_gameSceneName)) return;

        _levelsButton.SetEnabled(false);
        _infiniteButton.SetEnabled(false);
        _modeBackButton.SetEnabled(false);

        if (_loadingScreen != null)
        {
            _loadingScreen.style.display = DisplayStyle.Flex;
            
            DOTween.To(() => _loadingScreen.resolvedStyle.opacity, x => _loadingScreen.style.opacity = x, 1f, _fadeToBlackDuration)
                   .OnComplete(() => {
                       StartCoroutine(LoadSceneAsync());
                   });
        }
        else
        {
            SceneManager.LoadScene(_gameSceneName);
        }
    }

    private IEnumerator LoadSceneAsync()
    {
        Coroutine spinnerCoroutine = null;
        if (_loadingSpinner != null)
        {
            spinnerCoroutine = StartCoroutine(RotateSpinnerEffect());
        }

        yield return new WaitForSeconds(_artificialLoadTime);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_gameSceneName);
        if (asyncLoad == null)
        {
            Debug.LogError($"¡Error! La escena '{_gameSceneName}' no pudo cargarse. Asegúrate de agregarla en File -> Build Settings y que el nombre en el Inspector del MainMenuController coincida.");
            if (spinnerCoroutine != null) StopCoroutine(spinnerCoroutine);
            yield break;
        }

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        if (spinnerCoroutine != null) StopCoroutine(spinnerCoroutine);
    }

    private IEnumerator RotateSpinnerEffect()
    {
        float currentAngle = 0f;
        while (true)
        {
            if (_loadingSpinner != null)
            {
                currentAngle += 300f * Time.deltaTime; // 300 grados por segundo (sentido horario / derecha)
                currentAngle %= 360f;
                _loadingSpinner.transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
            }
            yield return null;
        }
    }

    private void OnExitClicked()
    {
        if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks();
        Application.Quit();
    }
}