using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement; 
using DG.Tweening;

public class MainMenuController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private MenuKeyboardAnim keyboardAnim;

    [Header("Transición")]
    [SerializeField] private float finalTextFadeDuration = 0.5f;

    [Header("Navegación y Carga")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private float fadeToBlackDuration = 1.0f;
    [Tooltip("Tiempo de espera artificial (en segundos) para ver la pantalla de carga")]
    [SerializeField] private float artificialLoadTime = 6f;
    [Tooltip("Velocidad de escritura del texto 'CARGANDO...'")]
    [SerializeField] private float loadingTypeSpeed = 0.1f;

    private UIDocument uiDocument;
    private Label subtitleLabel; 
    private VisualElement finalTextContainer;
    
    private VisualElement loadingScreen; 
    private Label loadingTextLabel; // <-- NUEVO: Referencia al texto animado
    private Button playButton;
    private Button exitButton;

    private bool isTransitioning = false;
    private string fullLoadingText = "LOADING..."; 

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        VisualElement root = uiDocument.rootVisualElement;

        subtitleLabel = root.Q<Label>("SubtitleLabel");
        finalTextContainer = root.Q<VisualElement>("finalTextContainer");
        
        playButton = root.Q<Button>("PlayButton");
        exitButton = root.Q<Button>("ExitButton"); 
        
        loadingScreen = root.Q<VisualElement>("LoadingScreen");
        // Buscamos el texto por el nombre que le pusimos en el UI Builder
        loadingTextLabel = root.Q<Label>("LoadingTextLabel"); 

        if (playButton != null) playButton.clicked += OnPlayClicked;
        if (exitButton != null) exitButton.clicked += OnExitClicked;

        if (finalTextContainer != null)
        {
            finalTextContainer.style.opacity = 0f;
            finalTextContainer.style.display = DisplayStyle.None;
        }

        if (loadingScreen != null)
        {
            loadingScreen.style.opacity = 0f;
            loadingScreen.style.display = DisplayStyle.None;
        }
        
        // Vaciamos el texto de carga al iniciar por si acaso
        if (loadingTextLabel != null) loadingTextLabel.text = "";
    }

    void OnDisable() 
    {
        if (playButton != null) playButton.clicked -= OnPlayClicked;
        if (exitButton != null) exitButton.clicked -= OnExitClicked;
    }

    void Update()
    {
        if (!isTransitioning && Input.anyKeyDown)
        {
            isTransitioning = true;
            
            Tween keyboardTween = keyboardAnim?.AnimateToFinalState();

            if (subtitleLabel != null)
            {
                DOTween.To(() => subtitleLabel.resolvedStyle.opacity, x => subtitleLabel.style.opacity = x, 0f, 0.5f)
                       .OnComplete(() => subtitleLabel.style.display = DisplayStyle.None);
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
        if (finalTextContainer != null)
        {
            finalTextContainer.style.display = DisplayStyle.Flex;
            DOTween.To(() => finalTextContainer.resolvedStyle.opacity, x => finalTextContainer.style.opacity = x, 1f, finalTextFadeDuration);
        }
    }

    // --- LÓGICA DEL BOTÓN DE JUGAR ---

    private void OnPlayClicked()
    {
        if (string.IsNullOrEmpty(gameSceneName)) return;

        playButton.SetEnabled(false);
        exitButton.SetEnabled(false);

        if (loadingScreen != null)
        {
            loadingScreen.style.display = DisplayStyle.Flex;
            
            DOTween.To(() => loadingScreen.resolvedStyle.opacity, x => loadingScreen.style.opacity = x, 1f, fadeToBlackDuration)
                   .OnComplete(() => {
                       StartCoroutine(LoadSceneAsync());
                   });
        }
        else
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }

    // --- CORRUTINA DE CARGA ASÍNCRONA ---
    private IEnumerator LoadSceneAsync()
    {
        // 1. Iniciamos la animación de escritura en bucle
        Coroutine typingCoroutine = null;
        if (loadingTextLabel != null)
        {
            typingCoroutine = StartCoroutine(LoadingTextTypingEffect());
        }

        // 2. Aplicamos el retraso artificial de 10 segundos
        yield return new WaitForSeconds(artificialLoadTime);

        // 3. Cargamos la escena de verdad de fondo
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameSceneName);

        // Esperamos a que la escena esté lista
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Cuando termina de cargar, detenemos la animación 
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
    }

    // --- BUCLE DE ANIMACIÓN DE TEXTO ---
    private IEnumerator LoadingTextTypingEffect()
    {
        while (true) // Bucle infinito hasta que la escena cambie y destruya este objeto
        {
            // Tipado letra por letra
            for (int i = 0; i <= fullLoadingText.Length; i++)
            {
                loadingTextLabel.text = fullLoadingText.Substring(0, i);
                yield return new WaitForSeconds(loadingTypeSpeed);
            }
            
            // Pausa con el texto completo
            yield return new WaitForSeconds(0.6f);
            
            // Borrado del texto para reiniciar el bucle
            loadingTextLabel.text = ""; 
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void OnExitClicked()
    {
        Application.Quit();
    }
}