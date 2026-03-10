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
    [SerializeField] private float _artificialLoadTime = 6f;
    [Tooltip("Velocidad de escritura del texto 'CARGANDO...'")]
    [SerializeField] private float _loadingTypeSpeed = 0.1f;

    private UIDocument _uiDocument;
    private Label _subtitleLabel; 
    private VisualElement _finalTextContainer;
    
    private VisualElement _loadingScreen; 
    private Label _loadingTextLabel;
    private Button _playButton;
    private Button _exitButton;

    private bool _isTransitioning = false;
    private string _fullLoadingText = "LOADING..."; 

    private void OnEnable()
    {
        _uiDocument = GetComponent<UIDocument>();
        VisualElement root = _uiDocument.rootVisualElement;

        _subtitleLabel = root.Q<Label>("SubtitleLabel");
        _finalTextContainer = root.Q<VisualElement>("finalTextContainer");
        
        _playButton = root.Q<Button>("PlayButton");
        _exitButton = root.Q<Button>("ExitButton"); 
        
        _loadingScreen = root.Q<VisualElement>("LoadingScreen");
        _loadingTextLabel = root.Q<Label>("LoadingTextLabel"); 

        if (_playButton != null) _playButton.clicked += OnPlayClicked;
        if (_exitButton != null) _exitButton.clicked += OnExitClicked;

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
        
        if (_loadingTextLabel != null) _loadingTextLabel.text = "";
    }

    private void OnDisable() 
    {
        if (_playButton != null) _playButton.clicked -= OnPlayClicked;
        if (_exitButton != null) _exitButton.clicked -= OnExitClicked;
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


    private void OnPlayClicked()
    {
        if (_buttonClickFeedback != null) _buttonClickFeedback.PlayFeedbacks();

        if (string.IsNullOrEmpty(_gameSceneName)) return;

        _playButton.SetEnabled(false);
        _exitButton.SetEnabled(false);

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