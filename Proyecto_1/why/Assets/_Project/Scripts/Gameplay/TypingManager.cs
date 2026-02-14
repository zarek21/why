using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening; 

public class TypingManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private BuildingManager buildingManager;
    [SerializeField] private TextMeshProUGUI wordDisplay;
    [SerializeField] private LevelData currentLevelData; 
    [SerializeField] private Slider timerBar;

    [Header("Feedback Visual")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color typedColor = Color.green;
    [SerializeField] private Color errorColor = Color.red;

    // Estado interno
    private string currentWord = "";
    private string typedWord = "";
    
    // Variables de Tiempo
    private float maxTime;
    private float currentTime;

    // Lista temporal (se sobrescribe en Start por el WordBank)
    private List<string> wordBank = new List<string>(); 

    private void Start()
    {
        // 1. Cargar Configuración de Tiempo y Palabras
        if (currentLevelData != null)
        {
            maxTime = currentLevelData.baseTimePerWord;

            // Pedimos palabras al diccionario global según el nivel
            wordBank = WordBank.GetWordsForLevel(
                currentLevelData.minWordLength, 
                currentLevelData.maxWordLength
            );
            
            Debug.Log($"Nivel cargado con {wordBank.Count} palabras disponibles.");
        }
        else
        {
            maxTime = 5f;
            Debug.LogError("¡Falta asignar el LevelData en el Inspector!");
        }

        SetNewWord();
    }

    private void Update()
    {
        // Si el juego terminó, no procesamos nada
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        HandleTimer();
        DetectInput();
    }

    private void HandleTimer()
    {
        currentTime -= Time.deltaTime;

        if (timerBar != null)
        {
            timerBar.value = currentTime / maxTime;
        }

        // Si se acaba el tiempo, cuenta como error
        if (currentTime <= 0)
        {
            HandleMistake(); 
        }
    }

    private void DetectInput()
    {
        string input = Input.inputString;
        if (string.IsNullOrEmpty(input)) return;

        foreach (char c in input)
        {
            char charUpper = char.ToUpper(c);
            
            if (c == '\b') // Backspace
            {
                if (typedWord.Length > 0)
                {
                    typedWord = typedWord.Substring(0, typedWord.Length - 1);
                    UpdateDisplay();
                }
                continue;
            }

            // --- CORRECCIÓN CRÍTICA ---
            // Si CheckLetter devuelve TRUE (significa que hubo error o terminó palabra),
            // detenemos la lectura de teclas en este frame para evitar dobles castigos.
            bool stopProcessing = CheckLetter(charUpper);
            if (stopProcessing) return; 
        }
    }

    // Cambiamos a bool: devuelve True si debemos dejar de procesar input
    private bool CheckLetter(char letter)
    {
        if (currentWord[typedWord.Length] == letter)
        {
            typedWord += letter;

            if (typedWord.Length == currentWord.Length)
            {
                WordCompleted();
                return true; // Palabra terminada -> Stop Input
            }
            else
            {
                UpdateDisplay();
                return false; // Letra correcta, pero faltan más -> Sigue leyendo
            }
        }
        else
        {
            // Error de dedo
            HandleMistake();
            return true; // Hubo error -> Stop Input inmediatamente
        }
    }
    
    private void HandleMistake()
    {
        // 1. Castigo Físico (Piso)
        if (buildingManager != null) buildingManager.RemoveTopFloor();
        
        // 2. Castigo Global (Vida)
        // NOTA: En tu código anterior lo tenías dos veces. Aquí solo UNA vez.
        if (GameManager.Instance != null) GameManager.Instance.LoseLife();

        // 3. Feedback Visual (Shake + Rojo)
        wordDisplay.color = errorColor;
        
        // Efecto de Shake con DOTween
        wordDisplay.rectTransform.DOShakeAnchorPos(0.3f, 20f, 15, 90, false, true);

        // Regresar color a la normalidad
        Invoke("ResetColor", 0.2f);
        
        // 4. Reiniciar palabra y TIEMPO
        SetNewWord(); 
    }

    private void WordCompleted()
    {
        if (GameManager.Instance != null) GameManager.Instance.AddCombo();

        int pisosAConstruir = 1; // Por defecto es 1

        if (GameManager.Instance != null)
        {
            int combo = GameManager.Instance.currentCombo;

            if (combo >= 15)
            {
                pisosAConstruir = 3; 
            }
            else if (combo >= 10)
            {
                pisosAConstruir = 2; 
            }
        }

        
        for (int i = 0; i < pisosAConstruir; i++)
        {
            if (buildingManager != null) buildingManager.AddFloor();
        }
        
        SetNewWord();
    }

    private void SetNewWord()
    {
        typedWord = "";
        
        if (wordBank != null && wordBank.Count > 0)
            currentWord = wordBank[Random.Range(0, wordBank.Count)];
        
        currentTime = maxTime; 
        
        UpdateDisplay();
        ResetColor(); 
    }

    private void UpdateDisplay()
    {
        string typedPart = $"<color=#{ColorUtility.ToHtmlStringRGB(typedColor)}>{typedWord}</color>";
        string remainingPart = currentWord.Substring(typedWord.Length);
        wordDisplay.text = typedPart + remainingPart;
    }

    private void ResetColor()
    {
        wordDisplay.color = normalColor;
    }
}