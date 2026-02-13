using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; 

public class BuildingManager : MonoBehaviour
{
    [Header("Configuración de Alturas")]
    [Tooltip("Altura exacta del bloque de concreto base")]
    [SerializeField] private float foundationHeight = 0.5f; 
    
    [Tooltip("Altura exacta de cada piso residencial")]
    [SerializeField] private float roomHeight = 3.0f; 

    [Header("Referencias a Prefabs")]
    [SerializeField] private GameObject foundationPrefab; // El concreto
    [SerializeField] private GameObject roomPrefab;       // El piso repetible

    // Estado interno
    private List<GameObject> activeFloors = new List<GameObject>(); // Solo guarda los pisos jugables
    private GameObject foundationInstance; // Referencia al cimiento

    private void Start()
    {
        SpawnFoundation();
    }

    // 1. Esto ocurre SOLO una vez al inicio
    private void SpawnFoundation()
    {
        // Instanciamos el concreto en la posición del Manager (generalmente 0,0,0)
        foundationInstance = Instantiate(foundationPrefab, transform.position, Quaternion.identity);
        
        // Lo emparentamos al Manager para tener la jerarquía ordenada
        foundationInstance.transform.SetParent(transform);
    }

    // 2. Esto ocurre cada vez que completas una palabra
    [ContextMenu("Test Add Floor")] // Para probar desde el editor
    public void AddFloor()
    {
        // Cálculo matemático simple para saber dónde va el nuevo piso:
        // Posición Base + Altura Cimiento + (Altura Piso * Cantidad de pisos que ya tengo)
        float currentY = transform.position.y + foundationHeight + (activeFloors.Count * roomHeight);
        
        Vector3 spawnPos = new Vector3(transform.position.x, currentY, transform.position.z);

        // Instanciar
        GameObject newFloor = Instantiate(roomPrefab, spawnPos, Quaternion.identity);
        newFloor.transform.SetParent(transform);

        // Juice: Que caiga del cielo un poco
        newFloor.transform.position += Vector3.up * 5f; 
        newFloor.transform.DOMoveY(spawnPos.y, 0.4f).SetEase(Ease.OutBounce);

        // Agregamos a la lista para poder borrarlo luego si fallas
        activeFloors.Add(newFloor);
        
        if (GameManager.Instance != null) GameManager.Instance.AddFloorScore();
    }

    [ContextMenu("Test Remove Floor")]
    public void RemoveTopFloor()
    {
      
        if (GameManager.Instance != null) 
        {
            GameManager.Instance.RemoveFloorScore();
        }

  
        if (activeFloors.Count == 0)
        {
            return;
        }

     
        GameObject floorToRemove = activeFloors[activeFloors.Count - 1];
        
        activeFloors.RemoveAt(activeFloors.Count - 1);

        floorToRemove.transform.DOShakePosition(0.2f, 0.5f);
        Destroy(floorToRemove, 0.2f);
    }
}