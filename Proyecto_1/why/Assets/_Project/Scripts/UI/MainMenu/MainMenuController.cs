using System.Collections;
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
    [Tooltip("Velocidad de escritura del texto 'CARGANDO...'")]
    [SerializeField] private float _loadingTypeSpeed = 0.2f;

    private UIDocument _uiDocument;
    private Label _subtitleLabel; 
    private VisualElement _finalTextContainer;
    
    private VisualElement _loadingScreen; 
    private Label _loadingTextLabel;
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

    private bool _isTransitioning = false;
    private string _fullLoadingText = "LOADING..."; 

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
        _loadingTextLabel = root.Q<Label>("LoadingTextLabel"); 

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
        
        if (_loadingTextLabel != null) _loadingTextLabel.text = "";

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
        GameManager.SelectedMode = GameMode.Levels;
        StartGameLoad();
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
        Coroutine typingCoroutine = null;
        if (_loadingTextLabel != null)
        {
            typingCoroutine = StartCoroutine(LoadingTextTypingEffect());
        }

        yield return new WaitForSeconds(_artificialLoadTime);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_gameSceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
    }

    private IEnumerator LoadingTextTypingEffect()
    {
        while (true)
        {
            for (int i = 0; i <= _fullLoadingText.Length; i++)
            {
                _loadingTextLabel.text = _fullLoadingText.Substring(0, i);
                yield return new WaitForSeconds(_loadingTypeSpeed);
            }
            
            yield return new WaitForSeconds(0.6f);
            
            _loadingTextLabel.text = ""; 
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void OnExitClicked()
    {
        if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks();
        Application.Quit();
    }
}