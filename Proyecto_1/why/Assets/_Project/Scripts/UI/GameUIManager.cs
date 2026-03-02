using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement; // <-- IMPORTANTE para reiniciar

public class GameUIManager : MonoBehaviour
{
    private UIDocument uiDocument;
    
    // Tutorial y HUD
    private VisualElement tutorialOverlay;
    private Button startLevelButton;
    private VisualElement hudContainer;
    private Label scoreLabel;
    private Label livesLabel;

    // --- NUEVO: Combo y Resultados ---
    private Label comboLabel;
    private VisualElement resultsOverlay;
    private Label resultTitleLabel;
    private Button restartButton;

    private static bool hasSeenTutorial = false; 

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        VisualElement root = uiDocument.rootVisualElement;

        // Búsquedas
        tutorialOverlay = root.Q<VisualElement>("TutorialOverlay");
        startLevelButton = root.Q<Button>("StartLevelButton");
        hudContainer = root.Q<VisualElement>("HUDContainer");
        scoreLabel = root.Q<Label>("ScoreLabel");
        livesLabel = root.Q<Label>("LivesLabel");

        // Búsquedas Nuevas
        comboLabel = root.Q<Label>("ComboLabel");
        resultsOverlay = root.Q<VisualElement>("ResultsOverlay");
        resultTitleLabel = root.Q<Label>("ResultTitleLabel");
        restartButton = root.Q<Button>("RestartButton");

        // Eventos
        if (startLevelButton != null) startLevelButton.clicked += OnStartPlayingClicked;
        if (restartButton != null) restartButton.clicked += OnRestartClicked;

        // Estados Iniciales
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
        hasSeenTutorial = true; 
        if (tutorialOverlay != null) tutorialOverlay.style.display = DisplayStyle.None;
        if (hudContainer != null) hudContainer.style.display = DisplayStyle.Flex; 
        Time.timeScale = 1f;
    }

    private void OnRestartClicked()
    {
        // Reiniciamos la escena actual
        Time.timeScale = 1f; // Asegurarnos de descongelar
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // =========================================================
    // MÉTODOS PÚBLICOS
    // =========================================================

    public void ActualizarPisos(int pisosConstruidos, int metaDePisos)
    {
        if (scoreLabel != null) scoreLabel.text = $"PISOS: {pisosConstruidos} / {metaDePisos}";
    }

    public void ActualizarVidas(int vidasRestantes)
    {
        if (livesLabel != null) livesLabel.text = $"VIDAS: {vidasRestantes}";
    }

    // --- NUEVAS FUNCIONES PARA EL GAME MANAGER ---

    public void MostrarCombo(int comboActual)
    {
        if (comboLabel == null) return;

        if (comboActual > 1)
        {
            comboLabel.style.display = DisplayStyle.Flex;
            comboLabel.text = $"x{comboActual}!";

            // Lógica de colores basada en tu código anterior
            if (comboActual >= 10)
                comboLabel.style.color = new StyleColor(Color.red);
            else if (comboActual >= 5)
                comboLabel.style.color = new StyleColor(Color.magenta);
            else
                comboLabel.style.color = new StyleColor(Color.yellow);

            // Efecto de "Rebote" usando CSS Toggle
            comboLabel.AddToClassList("combo-pop");
            
            // Quitamos la clase a los 0.15s para que vuelva a su tamaño normal
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

        // Ocultamos elementos innecesarios
        if (comboLabel != null) comboLabel.style.display = DisplayStyle.None;
        if (hudContainer != null) hudContainer.style.display = DisplayStyle.None;

        // Congelamos el tiempo al terminar
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