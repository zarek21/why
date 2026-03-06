using UnityEngine;
using DG.Tweening;

public class MenuKeyboardAnim : MonoBehaviour
{
    [Header("Configuración de Flotación")]
    [SerializeField] private float floatHeight = 0.5f;
    [SerializeField] private float floatDuration = 2f;

    [Header("Rotación Automática (Idle)")]
    [SerializeField] private Vector3 rotationAmount = new Vector3(5f, 10f, 5f);
    [SerializeField] private float rotationDuration = 4f;

    [Header("Estado Final")]
    [SerializeField] private Vector3 finalPosition = new Vector3(-0.03f, 0.42f, 3.07f); 
    [SerializeField] private Vector3 finalRotation = new Vector3(186.304f, -360.361f, 180.241f);
    [SerializeField] private Vector3 finalScale = new Vector3(400f, 400f, 400f);
    [SerializeField] private float transitionDuration = 1.0f;

    [Header("Rotación con el Mouse (Hover)")]
    [SerializeField] private float maxRotX = 20f; 
    [SerializeField] private float maxRotY = 25f; 
    [SerializeField] private float hoverSmoothness = 5f;

    private Quaternion baseRotation;
    private Quaternion targetHoverRotation;
    private bool isHovering = false;
    
    private bool isLocked = false; 
    private bool isInFinalMenu = false; 

    private Tween floatTween; 
    private Tween idleRotationTween;
    private Tween returnRotationTween;

    private void Start()
    {
        baseRotation = transform.rotation;
        targetHoverRotation = baseRotation;

        floatTween = transform.DOMoveY(transform.position.y + floatHeight, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        StartIdleRotation();
    }

    private void Update()
    {
        if (isLocked) return;

        if (isHovering || isInFinalMenu) 
        {
            Vector3 mousePos = Input.mousePosition;
            
            float normalizedX = (mousePos.x / Screen.width) * 2f - 1f;
            float normalizedY = (mousePos.y / Screen.height) * 2f - 1f;

            float targetX = -normalizedY * maxRotX;
            float targetY = normalizedX * maxRotY;

            targetHoverRotation = baseRotation * Quaternion.Euler(targetX, targetY, 0);
            
            transform.rotation = Quaternion.Slerp(transform.rotation, targetHoverRotation, Time.deltaTime * hoverSmoothness);
        }
    }

    private void StartIdleRotation()
    {
        idleRotationTween = transform.DORotate(rotationAmount, rotationDuration, RotateMode.LocalAxisAdd)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnMouseEnter()
    {
        if (isLocked) return; 

        isHovering = true;
        idleRotationTween?.Kill();
        returnRotationTween?.Kill();
    }

    private void OnMouseExit()
    {
        if (isLocked) return; 

        if (isInFinalMenu) return;

        isHovering = false;
        
        returnRotationTween = transform.DORotateQuaternion(baseRotation, 0.5f).OnComplete(() => {
            StartIdleRotation();
        });
    }

    public Tween AnimateToFinalState()
    {
        isLocked = true;
        isHovering = false;
        isInFinalMenu = true;

        floatTween?.Kill(); 
        idleRotationTween?.Kill();
        returnRotationTween?.Kill();

        Sequence finalSequence = DOTween.Sequence();
        
        finalSequence.Append(transform.DOMove(finalPosition, transitionDuration).SetEase(Ease.InOutSine));
        
        Quaternion targetFinalRotation = Quaternion.Euler(finalRotation);
        finalSequence.Join(transform.DORotateQuaternion(targetFinalRotation, transitionDuration).SetEase(Ease.InOutSine));

        finalSequence.Join(transform.DOScale(finalScale, transitionDuration).SetEase(Ease.InOutSine));

        finalSequence.AppendCallback(() => {
            baseRotation = targetFinalRotation;
            targetHoverRotation = baseRotation;
            isLocked = false;
        });

        return finalSequence;
    }
}