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

    private string currentWord = "";
    private string typedWord = "";
    
    private float maxTime;
    private float currentTime;

    private List<string> wordBank = new List<string>() 
    { 
        "CASA", "LUNA", "GATO", "ROJO", "AUTO", "PATO", "TREN",
        "NUBE", "SOL", "MAR", "PEZ", "LUZ", "FLOR", "RISA"
    };

    private void Start()
    {
        // 1. Cargar Configuración de Tiempo
        if (currentLevelData != null)
        {
            maxTime = currentLevelData.baseTimePerWord;

            // 2. ¡AQUÍ ESTÁ LA MAGIA! 
            // Pedimos palabras que coincidan con la dificultad del nivel
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
            
            if (c == '\b') 
            {
                if (typedWord.Length > 0)
                {
                    typedWord = typedWord.Substring(0, typedWord.Length - 1);
                    UpdateDisplay();
                }
                continue;
            }

            CheckLetter(charUpper);
        }
    }

    private void CheckLetter(char letter)
    {
        if (currentWord[typedWord.Length] == letter)
        {
            typedWord += letter;
            

            if (typedWord.Length == currentWord.Length)
            {
                WordCompleted();
            }
            else
            {
                UpdateDisplay();
            }
        }
        else
        {
            // Error de dedo
            HandleMistake();
        }
    }
    
    private void HandleMistake()
    {
        // 1. Castigo Físico
        buildingManager.RemoveTopFloor();
        
        // 2. Castigo Global (Vida)
        if (GameManager.Instance != null) GameManager.Instance.LoseLife();

        // 3. Feedback Visual (Shake + Rojo)
        wordDisplay.color = errorColor;
        
        // Efecto de Shake 
        wordDisplay.rectTransform.DOShakeAnchorPos(0.3f, 20f, 15, 90, false, true);

        // Regresar color a la normalidad
        Invoke("ResetColor", 0.2f);
        
        // 4. Reiniciar palabra y TIEMPO
        SetNewWord(); 
    }

    private void WordCompleted()
    {
        buildingManager.AddFloor();
        SetNewWord();
    }

    private void SetNewWord()
    {
        typedWord = "";
        
        if (wordBank.Count > 0)
            currentWord = wordBank[Random.Range(0, wordBank.Count)];
        
        currentTime = maxTime; 
        
        UpdateDisplay();
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