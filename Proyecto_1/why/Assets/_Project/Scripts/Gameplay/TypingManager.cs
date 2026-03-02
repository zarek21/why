using UnityEngine;
using UnityEngine.UIElements; 
using System.Collections.Generic;

public class TypingManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private BuildingManager buildingManager;
    [SerializeField] private LevelData currentLevelData; 
    [SerializeField] private GameUIManager uiManager; 

    // --- NUEVAS VARIABLES PARA UI TOOLKIT ---
    private Label wordDisplay;
    private VisualElement timerFill;

    [Header("Feedback Visual")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color typedColor  = new Color32(173, 186, 152,255);
    [SerializeField] private Color errorColor =new Color32(137, 76, 76,255);

    private string currentWord = "";
    private string typedWord = "";
    private int pisosConstruidos = 0; 
    private float maxTime;
    private float currentTime;

    private List<string> wordBag = new List<string>(); 
    private List<string> masterList = new List<string>();  

    private void Start()
    {
        // 1. OBTENER REFERENCIAS DE UI TOOLKIT
        if (uiManager != null)
        {
            UIDocument uiDoc = uiManager.GetComponent<UIDocument>();
            VisualElement root = uiDoc.rootVisualElement;

            wordDisplay = root.Q<Label>("WordDisplay");
            timerFill = root.Q<VisualElement>("TimerFill");
            
            // Asegurarnos de que el rich text esté activado por código
            if (wordDisplay != null) wordDisplay.enableRichText = true;
        }

        // 2. CONFIGURAR NIVEL
        if (currentLevelData != null)
        {
            maxTime = currentLevelData.baseTimePerWord;
            masterList = new List<string>(currentLevelData.wordPool);
            
            if (masterList.Count == 0) masterList.Add("ERROR");

            RefillBag(); 

            if (uiManager != null)
            {
                uiManager.ActualizarPisos(pisosConstruidos, currentLevelData.targetFloors);
                uiManager.ActualizarVidas(currentLevelData.maxLives); 
            }
        }

        SetNewWord();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;
        if (Time.timeScale == 0f) return; 

        HandleTimer();
        DetectInput();
    }

    private void HandleTimer()
    {
        currentTime -= Time.deltaTime;

        // --- NUEVA LÓGICA DE BARRA DE TIEMPO CSS ---
        if (timerFill != null)
        {
            float percent = (currentTime / maxTime) * 100f;
            timerFill.style.width = new Length(Mathf.Max(0, percent), LengthUnit.Percent);
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

            bool stopProcessing = CheckLetter(charUpper);
            if (stopProcessing) return; 
        }
    }

    private bool CheckLetter(char letter)
    {
        if (currentWord[typedWord.Length] == letter)
        {
            typedWord += letter;

            if (typedWord.Length == currentWord.Length)
            {
                WordCompleted();
                return true; 
            }
            else
            {
                UpdateDisplay();
                return false; 
            }
        }
        else
        {
            HandleMistake();
            return true; 
        }
    }
    
    private void HandleMistake()
    {
        if (buildingManager != null) 
        {
            buildingManager.RemoveTopFloor();
            if (pisosConstruidos > 0) pisosConstruidos--;
            if (uiManager != null) uiManager.ActualizarPisos(pisosConstruidos, currentLevelData.targetFloors);
        }
        
        if (GameManager.Instance != null) 
        {
            GameManager.Instance.LoseLife();
        }

        // --- NUEVO FEEDBACK VISUAL UI TOOLKIT ---
        if (wordDisplay != null)
        {
            wordDisplay.style.color = new StyleColor(errorColor);
            Invoke("ResetColor", 0.3f);
        }
        
        SetNewWord(); 
    }

    private void WordCompleted()
    {
        if (GameManager.Instance != null) GameManager.Instance.AddCombo();

        int pisosAConstruir = 1; 
        if (GameManager.Instance != null)
        {
            int combo = GameManager.Instance.currentCombo;
            if (combo >= 15) pisosAConstruir = 3; 
            else if (combo >= 10) pisosAConstruir = 2; 
        }

        for (int i = 0; i < pisosAConstruir; i++)
        {
            if (buildingManager != null) buildingManager.AddFloor();
            pisosConstruidos++;
        }
        
        if (uiManager != null) uiManager.ActualizarPisos(pisosConstruidos, currentLevelData.targetFloors);
        
        SetNewWord();
    }

    private void SetNewWord()
    {
        typedWord = "";
        
        if (wordBag.Count == 0) RefillBag();

        if (wordBag.Count > 0)
        {
            int lastIndex = wordBag.Count - 1;
            currentWord = wordBag[lastIndex].ToUpper();
            wordBag.RemoveAt(lastIndex);
        }
        
        currentTime = maxTime; 
        
        UpdateDisplay();
        ResetColor(); 
    }

    private void RefillBag()
    {
        wordBag = new List<string>(masterList);
        Shuffle(wordBag);
    }

    private void Shuffle(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            string temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }

    private void UpdateDisplay()
    {
        if (wordDisplay == null) return;
        
        string typedPart = $"<color=#{ColorUtility.ToHtmlStringRGB(typedColor)}>{typedWord}</color>";
        string remainingPart = currentWord.Substring(typedWord.Length);
        wordDisplay.text = typedPart + remainingPart;
    }

    private void ResetColor()
    {
        if (wordDisplay != null) wordDisplay.style.color = new StyleColor(normalColor);
    }
}