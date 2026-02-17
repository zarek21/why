using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform target; // Arrastra tu BuildingManager aquí

    [Header("Configuración Simple")]
    [Tooltip("¿Cuánto mide exactamente tu piso? (ej. 3)")]
    [SerializeField] private float alturaPorPiso = 3.0f; 
    
    [Tooltip("Qué tan suave se mueve (5 es buen número)")]
    [SerializeField] private float smoothSpeed = 5.0f; 

    [Header("Efecto Game Over")]
    [Tooltip("Velocidad del paneo final al bajar (más lento = más dramático, ej. 2)")]
    [SerializeField] private float gameOverSmoothSpeed = 2.0f;

    private Vector3 posicionInicial;

    private void Start()
    {
        // Guardamos la posición inicial. 
        // Esta posición Y es exactamente donde se ve perfecto tu "Building_Foundation".
        // La posición Z se mantendrá intacta en todo momento.
        posicionInicial = transform.position;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 1. ¿Cuántos pisos REALES hay construidos?
        int pisosConstruidos = target.childCount - 1;
        if (pisosConstruidos < 0) pisosConstruidos = 0;

        // 2. Variables de destino por defecto (Estado Normal de Juego)
        float targetY = posicionInicial.y + (pisosConstruidos * alturaPorPiso);
        float targetZ = posicionInicial.z; // La Z se queda congelada
        float velocidadActual = smoothSpeed;

        // 3. --- MODO GAME OVER ---
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
        {
            // Solo modificamos la Y para que baje hasta la base (el cimiento)
            targetY = posicionInicial.y;
            
            // Hacemos el movimiento más lento para el toque cinematográfico
            velocidadActual = gameOverSmoothSpeed;
        }

        // 4. Aplicar el movimiento suave (Lerp) a la nueva posición
        Vector3 destino = new Vector3(posicionInicial.x, targetY, targetZ);
        transform.position = Vector3.Lerp(transform.position, destino, Time.deltaTime * velocidadActual);
    }
}