using UnityEngine;
using DG.Tweening;

public class MenuKeyboardAnim : MonoBehaviour
{
    [Header("Configuración de Flotación")]
    [SerializeField] private float _floatHeight = 0.5f;
    [SerializeField] private float _floatDuration = 2f;

    [Header("Rotación Automática (Idle)")]
    [SerializeField] private Vector3 _rotationAmount = new Vector3(5f, 0f, 0f);
    [SerializeField] private float _rotationDuration = 4f;

    [Header("Estado Final")]
    [SerializeField] private Vector3 _finalPosition = new Vector3(-0.03f, 0.42f, 3.07f); 
    [SerializeField] private Vector3 _finalRotation = new Vector3(186.304f, -360.361f, 180.241f);
    [SerializeField] private Vector3 _finalScale = new Vector3(400f, 400f, 400f);
    [SerializeField] private float _transitionDuration = 1.0f;

    [Header("Rotación con el Mouse (Hover)")]
    [SerializeField] private float _maxRotX = 20f; 
    [SerializeField] private float _maxRotY = 25f; 
    [SerializeField] private float _hoverSmoothness = 5f;

    private Quaternion _baseRotation;
    private Quaternion _targetHoverRotation;
    private bool _isHovering = false;
    
    private bool _isLocked = false; 
    private bool _isInFinalMenu = false; 

    private Tween _floatTween; 
    private Tween _idleRotationTween;
    private Tween _returnRotationTween;

    private void Start()
    {
        _baseRotation = transform.rotation;
        _targetHoverRotation = _baseRotation;

        _floatTween = transform.DOMoveY(transform.position.y + _floatHeight, _floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        StartIdleRotation();
    }

    private void Update()
    {
        if (_isLocked) return;

        if (_isHovering || _isInFinalMenu) 
        {
            Vector3 mousePos = Input.mousePosition;
            
            float normalizedX = (mousePos.x / Screen.width) * 2f - 1f;
            float normalizedY = (mousePos.y / Screen.height) * 2f - 1f;

            float targetX = -normalizedY * _maxRotX;
            float targetY = normalizedX * _maxRotY;

            _targetHoverRotation = _baseRotation * Quaternion.Euler(targetX, targetY, 0);
            
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetHoverRotation, Time.deltaTime * _hoverSmoothness);
        }
    }

    private void StartIdleRotation()
    {
        _idleRotationTween = transform.DORotate(_rotationAmount, _rotationDuration, RotateMode.LocalAxisAdd)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnMouseEnter()
    {
        if (_isLocked) return; 

        _isHovering = true;
        _idleRotationTween?.Kill();
        _returnRotationTween?.Kill();
    }

    private void OnMouseExit()
    {
        if (_isLocked) return; 

        if (_isInFinalMenu) return;

        _isHovering = false;
        
        _returnRotationTween = transform.DORotateQuaternion(_baseRotation, 0.5f).OnComplete(() => {
            StartIdleRotation();
        });
    }

    public Tween AnimateToFinalState()
    {
        _isLocked = true;
        _isHovering = false;
        _isInFinalMenu = true;

        _floatTween?.Kill(); 
        _idleRotationTween?.Kill();
        _returnRotationTween?.Kill();

        Sequence finalSequence = DOTween.Sequence();
        
        finalSequence.Append(transform.DOMove(_finalPosition, _transitionDuration).SetEase(Ease.InOutSine));
        
        Quaternion targetFinalRotation = Quaternion.Euler(_finalRotation);
        finalSequence.Join(transform.DORotateQuaternion(targetFinalRotation, _transitionDuration).SetEase(Ease.InOutSine));

        finalSequence.Join(transform.DOScale(_finalScale, _transitionDuration).SetEase(Ease.InOutSine));

        finalSequence.AppendCallback(() => {
            _baseRotation = targetFinalRotation;
            _targetHoverRotation = _baseRotation;
            _isLocked = false;
        });

        return finalSequence;
    }
}