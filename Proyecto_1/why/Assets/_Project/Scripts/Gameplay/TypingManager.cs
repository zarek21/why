using UnityEngine;
using UnityEngine.UIElements; 
using System.Collections.Generic;
using MoreMountains.Feedbacks; 
using DG.Tweening;

public class TypingManager : MonoBehaviour
{
    private float _wordDisplayScale = 1f;
    private Tween _scaleTween;
    [Header("Referencias")]
    [SerializeField] private BuildingManager _buildingManager;
    [SerializeField] private LevelData _currentLevelData; 
    [SerializeField] private GameUIManager _uiManager; 

    [Header("Modo Infinito")]
    [SerializeField] private float _infiniteTimePerWord = 10f; 

    private Label _wordDisplay;
    private VisualElement _timerFill;

    [Header("Feedback Visual")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _typedColor  = new Color32(173, 186, 152,255);
    [SerializeField] private Color _errorColor =new Color32(137, 76, 76,255);

    [Header("Efectos de Sonido de Escritura (Feel)")]
    [Tooltip("Feedback cuando el jugador presiona la tecla correcta")]
    [SerializeField] private MMF_Player _typeSuccessFeedback;
    [Tooltip("Feedback cuando el jugador se equivoca de tecla")]
    [SerializeField] private MMF_Player _typeErrorFeedback;

    private string _currentWord = "";
    private string _typedWord = "";
    private int _floorsBuilt = 0; 
    private float _maxTime;
    private float _currentTime;
    private string _cachedTypedColorHex;

    private List<string> _wordBag = new List<string>(); 
    private List<string> _masterList = new List<string>();  

    private void Start()
    {
        _cachedTypedColorHex = ColorUtility.ToHtmlStringRGB(_typedColor);

        if (_uiManager != null)
        {
            UIDocument uiDoc = _uiManager.GetComponent<UIDocument>();
            VisualElement root = uiDoc.rootVisualElement;

            _wordDisplay = root.Q<Label>("WordDisplay");
            _timerFill = root.Q<VisualElement>("TimerFill");
            
            if (_wordDisplay != null)
            {
                _wordDisplay.enableRichText = true;
                _wordDisplay.style.transformOrigin = new StyleTransformOrigin(
                    new TransformOrigin(
                        new Length(50f, LengthUnit.Percent),
                        new Length(50f, LengthUnit.Percent),
                        0f
                    )
                );
            }
        }

        if (GameManager.Instance != null && GameManager.Instance.LevelData != null)
        {
            _currentLevelData = GameManager.Instance.LevelData;
        }

        if (_currentLevelData != null)
        {
            if (GameManager.SelectedMode == GameMode.Infinite)
            {
                _maxTime = _infiniteTimePerWord;
                _masterList = _currentLevelData.GetAllWordsCombined();
            }
            else
            {
                _maxTime = _currentLevelData.BaseTimePerWord;
                _masterList = new List<string>(_currentLevelData.WordPool);
            }
            
            if (_masterList.Count == 0) _masterList.Add("ERROR");

            RefillBag(); 

            if (_uiManager != null)
            {
                _uiManager.ActualizarPisos(_floorsBuilt, _currentLevelData.TargetFloors);
                _uiManager.ActualizarVidas(_currentLevelData.MaxLives); 
            }
        }

        SetNewWord();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        if (Time.timeScale == 0f) return; 

        HandleTimer();
        DetectInput();
    }

    private void HandleTimer()
    {
        _currentTime -= Time.deltaTime;

        if (_timerFill != null)
        {
            float percent = (_currentTime / _maxTime) * 100f;
            _timerFill.style.width = new Length(Mathf.Max(0, percent), LengthUnit.Percent);
        }

        if (_currentTime <= 0)
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
                if (_typedWord.Length > 0)
                {
                    _typedWord = _typedWord.Substring(0, _typedWord.Length - 1);
                    UpdateDisplay();
                }
                continue;
            }

            bool shouldStopProcessing = CheckLetter(charUpper);
            if (shouldStopProcessing) return; 
        }
    }

    private bool CheckLetter(char letter)
    {
        if (_currentWord[_typedWord.Length] == letter)
        {
            if (_typeSuccessFeedback != null) _typeSuccessFeedback.PlayFeedbacks();
            
            _typedWord += letter;

            TriggerScaleEffect(letter == ' ');

            if (_typedWord.Length == _currentWord.Length)
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
            if (_typeErrorFeedback != null) _typeErrorFeedback.PlayFeedbacks();
            
            HandleMistake();
            return true; 
        }
    }
    
    private void HandleMistake()
    {
        if (_buildingManager != null) 
        {
            _buildingManager.RemoveTopFloor();
            if (_floorsBuilt > 0) _floorsBuilt--;
            if (_uiManager != null) _uiManager.ActualizarPisos(_floorsBuilt, _currentLevelData.TargetFloors);
        }
        
        if (GameManager.Instance != null) 
        {
            GameManager.Instance.LoseLife();
        }

        if (_wordDisplay != null)
        {
            _wordDisplay.style.color = new StyleColor(_errorColor);
            Invoke("ResetColor", 0.3f);
        }
        
        SetNewWord(); 
    }

    private void WordCompleted()
    {
        if (GameManager.Instance != null) GameManager.Instance.AddCombo();

        int floorsToAdd = 1; 
        if (GameManager.Instance != null)
        {
            int combo = GameManager.Instance.CurrentCombo;
            if (combo >= 15) floorsToAdd = 3; 
            else if (combo >= 10) floorsToAdd = 2; 
        }

        for (int i = 0; i < floorsToAdd; i++)
        {
            if (_buildingManager != null) _buildingManager.AddFloor();
            _floorsBuilt++;
        }
        
        if (_uiManager != null) _uiManager.ActualizarPisos(_floorsBuilt, _currentLevelData.TargetFloors);
        
        SetNewWord();
    }

    private void SetNewWord()
    {
        _typedWord = "";
        
        if (_wordBag.Count == 0) RefillBag();

        if (_wordBag.Count > 0)
        {
            int lastIndex = _wordBag.Count - 1;
            _currentWord = _wordBag[lastIndex].ToUpper();
            _wordBag.RemoveAt(lastIndex);
        }
        
        if (GameManager.SelectedMode == GameMode.Infinite)
        {
            int combo = GameManager.Instance != null ? GameManager.Instance.CurrentCombo : 0;
            float reduction = (combo / 5) * 0.25f;
            _maxTime = Mathf.Max(1.5f, _infiniteTimePerWord - reduction);
        }
        else if (_currentLevelData != null)
        {
            _maxTime = _currentLevelData.BaseTimePerWord;
        }
        
        _currentTime = _maxTime; 
        
        UpdateDisplay();
        ResetColor(); 
    }

    private void RefillBag()
    {
        _wordBag.Clear();
        _wordBag.AddRange(_masterList);
        Shuffle(_wordBag);
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
        if (_wordDisplay == null) return;
        
        string typedPart = $"<color=#{_cachedTypedColorHex}>{_typedWord}</color>";
        string remainingPart = _currentWord.Substring(_typedWord.Length);
        _wordDisplay.text = typedPart + remainingPart;
    }

    private void ResetColor()
    {
        if (_wordDisplay != null) _wordDisplay.style.color = new StyleColor(_normalColor);
    }

    private void TriggerScaleEffect(bool isSpace)
    {
        if (!isSpace) return;
        if (_wordDisplay == null) return;

        _scaleTween?.Kill();
        _wordDisplayScale = 1f;

        float targetScale = 1.08f;
        float scaleDuration = 0.15f;
        float returnDuration = 0.5f;

        _scaleTween = DOTween.To(() => _wordDisplayScale, x => {
            _wordDisplayScale = x;
            if (_wordDisplay != null)
            {
                _wordDisplay.style.scale = new StyleScale(new Scale(new Vector3(_wordDisplayScale, _wordDisplayScale, 1f)));
            }
        }, targetScale, scaleDuration)
        .SetEase(Ease.OutSine)
        .OnComplete(() => {
            _scaleTween = DOTween.To(() => _wordDisplayScale, x => {
                _wordDisplayScale = x;
                if (_wordDisplay != null)
                {
                    _wordDisplay.style.scale = new StyleScale(new Scale(new Vector3(_wordDisplayScale, _wordDisplayScale, 1f)));
                }
            }, 1f, returnDuration)
            .SetEase(Ease.OutBack);
        });
    }

    private void OnDestroy()
    {
        _scaleTween?.Kill();
    }
}