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

    [Header("Rotación con el Mouse (Hover)")]
    [Tooltip("Cuánto rota máximo hacia arriba/abajo")]
    [SerializeField] private float maxRotX = 20f; 
    [Tooltip("Cuánto rota máximo hacia los lados")]
    [SerializeField] private float maxRotY = 25f; 
    [Tooltip("Qué tan suave sigue al mouse")]
    [SerializeField] private float hoverSmoothness = 5f;

    private Quaternion baseRotation;
    private Quaternion targetHoverRotation;
    private bool isHovering = false;

    // --- EL SECRETO: Referencias para controlar las animaciones ---
    private Tween idleRotationTween;
    private Tween returnRotationTween;

    private void Start()
    {
        baseRotation = transform.rotation;
        targetHoverRotation = baseRotation;

        // La flotación no la guardamos en variable porque NUNCA la detenemos
        transform.DOMoveY(transform.position.y + floatHeight, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        StartIdleRotation();
    }

    private void Update()
    {
        if (isHovering)
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
        // Guardamos la animación en la variable para poder detenerla luego
        idleRotationTween = transform.DORotate(rotationAmount, rotationDuration, RotateMode.LocalAxisAdd)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnMouseEnter()
    {
        isHovering = true;
        
        // MATAMOS cualquier rotación automática que esté corriendo
        // El signo "?" asegura que solo intente matarla si no es nula
        idleRotationTween?.Kill();
        returnRotationTween?.Kill();
    }

    private void OnMouseExit()
    {
        isHovering = false;
        
        // Guardamos la animación de regreso en la variable.
        // Si el jugador vuelve a entrar rápido, OnMouseEnter matará este Tween al instante.
        returnRotationTween = transform.DORotateQuaternion(baseRotation, 0.5f).OnComplete(() => {
            StartIdleRotation();
        });
    }
}