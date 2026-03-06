using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement; 
using MoreMountains.Feedbacks; // <-- NUEVO: Para usar Feel en la UI

public class GameUIManager : MonoBehaviour
{
    private UIDocument uiDocument;
    
    private VisualElement tutorialOverlay;
    private Button startLevelButton;
    private VisualElement hudContainer;
    private Label scoreLabel;
    private Label livesLabel;

    private Label comboLabel;
    private VisualElement resultsOverlay;
    private Label resultTitleLabel;
    private Button restartButton;
    private static bool hasSeenTutorial = false; 

    private int oldPisos = -1;
    private int oldVidas = -1;
    private int oldCombo = -1;

    [Header("Efectos de Sonido UI (Feel)")]
    [Tooltip("Sonido general al hacer clic en botones principales")]
    [SerializeField] private MMF_Player buttonClickFeedback; 

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        VisualElement root = uiDocument.rootVisualElement;

        tutorialOverlay = root.Q<VisualElement>("TutorialOverlay");
        startLevelButton = root.Q<Button>("StartLevelButton");
        hudContainer = root.Q<VisualElement>("HUDContainer");
        scoreLabel = root.Q<Label>("ScoreLabel");
        livesLabel = root.Q<Label>("LivesLabel");

        comboLabel = root.Q<Label>("ComboLabel");
        resultsOverlay = root.Q<VisualElement>("ResultsOverlay");
        resultTitleLabel = root.Q<Label>("ResultTitleLabel");
        restartButton = root.Q<Button>("RestartButton");

        if (startLevelButton != null) startLevelButton.clicked += OnStartPlayingClicked;
        if (restartButton != null) restartButton.clicked += OnRestartClicked;

        if (resultsOverlay != null) resultsOverlay.style.display = DisplayStyle.None;
        if (comboLabel != null) comboLabel.style.display = DisplayStyle.None;

        if (!hasSeenTutorial)
        {
            Time.timeScale = 0f; 
            if (tutorialOverlay != null) tutorialOverlay.style.display = DisplayStyle.Flex;
            if (hudContainer != null) hudContainer.style.display = DisplayStyle.None; 
        }
        else
        {
            Time.timeScale = 1f;
            if (tutorialOverlay != null) tutorialOverlay.style.display = DisplayStyle.None;
            if (hudContainer != null) hudContainer.style.display = DisplayStyle.Flex; 
        }
    }

    private void OnDisable()
    {
        if (startLevelButton != null) startLevelButton.clicked -= OnStartPlayingClicked;
        if (restartButton != null) restartButton.clicked -= OnRestartClicked;
    }

    private void OnStartPlayingClicked()
    {
        if (buttonClickFeedback != null) buttonClickFeedback.PlayFeedbacks(); // <-- Sonido de botón
        
        hasSeenTutorial = true; 
        if (tutorialOverlay != null) tutorialOverlay.style.display = DisplayStyle.None;
        if (hudContainer != null) hudContainer.style.display = DisplayStyle.Flex; 
        Time.timeScale = 1f;
    }

    private void OnRestartClicked()
    {
        if (buttonClickFeedback != null) buttonClickFeedback.PlayFeedbacks(); // <-- Sonido de botón
        
        // Wait a tiny bit (ignore timescale) or just play it normally (it survives scene loads if persistent)
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void ActualizarPisos(int pisosConstruidos, int metaDePisos)
    {
        if (scoreLabel != null && pisosConstruidos != oldPisos) 
        {
            scoreLabel.text = $"PISOS: {pisosConstruidos} / {metaDePisos}";
            oldPisos = pisosConstruidos;
        }
    }

    public void ActualizarVidas(int vidasRestantes)
    {
        if (livesLabel != null && vidasRestantes != oldVidas) 
        {
            livesLabel.text = $"VIDAS: {vidasRestantes}";
            oldVidas = vidasRestantes;
        }
    }


    public void MostrarCombo(int comboActual)
    {
        if (comboLabel == null || comboActual == oldCombo) return;
        oldCombo = comboActual;

        if (comboActual > 1)
        {
            comboLabel.style.display = DisplayStyle.Flex;
            comboLabel.text = $"x{comboActual}!";

            if (comboActual >= 10)
                comboLabel.style.color = new StyleColor(Color.red);
            else if (comboActual >= 5)
                comboLabel.style.color = new StyleColor(Color.magenta);
            else
                comboLabel.style.color = new StyleColor(Color.yellow);

            comboLabel.AddToClassList("combo-pop");
            
            Invoke("ResetComboPop", 0.15f);
        }
        else
        {
            comboLabel.style.display = DisplayStyle.None;
        }
    }

    private void ResetComboPop()
    {
        if (comboLabel != null) comboLabel.RemoveFromClassList("combo-pop");
    }

    public void MostrarGameOver(bool victoria)
    {
        if (resultsOverlay == null || resultTitleLabel == null) return;

        if (comboLabel != null) comboLabel.style.display = DisplayStyle.None;
        if (hudContainer != null) hudContainer.style.display = DisplayStyle.None;

        Time.timeScale = 0f;

        resultsOverlay.style.display = DisplayStyle.Flex;

        if (victoria)
        {
            resultTitleLabel.text = "¡CONSTRUIDO!";
            resultTitleLabel.style.color = new StyleColor(Color.green);
        }
        else
        {
            resultTitleLabel.text = "COLAPSO";
            resultTitleLabel.style.color = new StyleColor(Color.red);
        }
    }
}