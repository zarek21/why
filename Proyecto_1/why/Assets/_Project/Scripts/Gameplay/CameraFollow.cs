using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] private Transform target; // Arrastraremos el "BuildingManager" aquí
    [SerializeField] private float smoothSpeed = 2.0f; // Qué tan rápido sigue (menor = más suave)
    
    [Header("Offset")]
    [SerializeField] private float heightOffset = 2.0f; // Para que la cámara mire un poco por encima del último piso
    [SerializeField] private float zoomOutFactor = 0.5f; // Cuánto se aleja por cada piso nuevo

    // Estado
    private float currentHeight = 0f;
    private Vector3 initialPosition;

    private void Start()
    {
        initialPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 1. Calculamos la altura objetivo basada en los pisos actuales
        // Necesitamos que BuildingManager nos diga cuántos pisos hay. 
        // Por ahora, usaremos la posición Y del último hijo del BuildingManager como referencia rápida.
        
        float targetY = 0f;
        if (target.childCount > 0)
        {
            // Busca el piso más alto
            targetY = target.GetChild(target.childCount - 1).position.y;
        }

        // 2. Calculamos la nueva posición de la cámara
        Vector3 targetPosition = initialPosition;
        targetPosition.y += targetY * 0.5f + heightOffset; // Subimos la mitad de lo que crece el edificio
        targetPosition.z -= targetY * zoomOutFactor; // Nos alejamos un poco para mantener la perspectiva

        // 3. Suavizado (Lerp)
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
        
        // 4. Opcional: Que siempre mire un poco hacia arriba
        // transform.LookAt(target.position + Vector3.up * targetY); 
    }
}