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

    private Vector3 posicionInicial;

    private void Start()
    {
        // 1. Guardamos la posición exacta donde pusiste la cámara en el editor.
        // Esta será nuestra altura "Cero".
        posicionInicial = transform.position;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 2. ¿Cuántos pisos REALES hay construidos?
        // Restamos 1 porque el primer hijo siempre es el cimiento base.
        int pisosConstruidos = target.childCount - 1;

        // Seguridad: Si aún no se crea el cimiento, no bajamos de 0.
        if (pisosConstruidos < 0) pisosConstruidos = 0;

        // 3. Calculamos la altura matemática exacta
        // Altura "Cero" + (Cantidad de pisos extra * Lo que mide un piso)
        float alturaDeseadaY = posicionInicial.y + (pisosConstruidos * alturaPorPiso);

        // 4. Creamos el punto de destino (Solo movemos la Y)
        Vector3 destino = new Vector3(posicionInicial.x, alturaDeseadaY, posicionInicial.z);

        // 5. Movimiento suave hacia el destino
        transform.position = Vector3.Lerp(transform.position, destino, Time.deltaTime * smoothSpeed);
    }
}