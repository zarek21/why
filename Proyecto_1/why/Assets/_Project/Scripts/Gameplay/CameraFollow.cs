using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Referencias")] [SerializeField]
    private BuildingManager buildingManager;

    [Header("Configuración Simple")] [Tooltip("¿Cuánto mide exactamente tu piso? (ej. 3)")] [SerializeField]
    private float alturaPorPiso = 3.0f;

    [Tooltip("Qué tan suave se mueve (5 es buen número)")] [SerializeField]
    private float smoothSpeed = 5.0f;

    [Header("Efecto Game Over")]
    [Tooltip("Velocidad del paneo final al bajar (más lento = más dramático, ej. 2)")]
    [SerializeField]
    private float gameOverSmoothSpeed = 2.0f;

    private Vector3 posicionInicial;

    private void Start()
    {
        posicionInicial = transform.position;
    }

    private void LateUpdate()
    {
        if (buildingManager == null) return;

        int pisosConstruidos = buildingManager.ActiveFloorCount;
        if (pisosConstruidos < 0) pisosConstruidos = 0;

        float targetZ = posicionInicial.z;
        float targetY = posicionInicial.y + (pisosConstruidos * alturaPorPiso);
        float velocidadActual = smoothSpeed;

        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
        {
            targetY = posicionInicial.y;
            velocidadActual = gameOverSmoothSpeed;
        }

        Vector3 destino = new Vector3(posicionInicial.x, targetY, targetZ);
        transform.position = Vector3.Lerp(transform.position, destino, Time.deltaTime * velocidadActual);
    }

    // Este método tenía un error de sintaxis ("void float") y "pisosConstruidos" no existía en este contexto.
    public void CameraRotation(float rotation)
    {
        // pisosConstruidos era una variable local en LateUpdate. Para checar los pisos aquí, usamos el BuildingManager.
        // Ojo: los pisos nunca bajan de 0 en el script, así que esto nunca se ejecutará a menos que lo cambies.
        if (buildingManager != null && buildingManager.ActiveFloorCount < 0)
        {
            transform.rotation = Quaternion.Euler(0, rotation, 0);
        }
    }


public void RotateCamera(float angle)
    {
        // Rotamos la posición inicial alrededor del eje Y para que LateUpdate use las nuevas coordenadas X y Z
        posicionInicial = Quaternion.Euler(0, angle, 0) * posicionInicial;
        
        // Hacemos que la cámara mire hacia el centro del edificio (asumiendo que está en X=0, Z=0)
        Vector3 lookAtTarget = new Vector3(0, transform.position.y, 0);
        transform.LookAt(lookAtTarget);
    }
}