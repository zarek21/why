using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; 
using MoreMountains.Feedbacks; // <-- NUEVO: Para usar Feel

public class BuildingManager : MonoBehaviour
{
    [Header("Configuración de Alturas")]
    [Tooltip("Altura exacta del bloque de concreto base")]
    [SerializeField] private float foundationHeight = 0.5f; 
    
    [Tooltip("Altura exacta de cada piso residencial")]
    [SerializeField] private float roomHeight = 3.0f; 

    [Header("Referencias a Prefabs")]
    [SerializeField] private GameObject foundationPrefab; 
    [SerializeField] private GameObject roomPrefab;       
    
    [Header("Efectos Visuales y Sonoros")]
    [SerializeField] private GameObject dustEffectPrefab; 
    [Tooltip("Feedback de Feel que se reproducirá al construir un piso")]
    [SerializeField] private MMF_Player floorBuiltFeedback; 
    [Tooltip("Feedback de Feel que se reproducirá al destruirse un piso (error)")]
    [SerializeField] private MMF_Player floorDestroyedFeedback; 

    [Header("Configuración de Spawn")]
    [Tooltip("Altura desde donde cae el piso al instanciarse")]
    [SerializeField] private float spawnHeightOffset = 5f;

    private List<GameObject> activeFloors = new List<GameObject>(); 
    public int ActiveFloorCount => activeFloors.Count;
    private Stack<GameObject> floorPool = new Stack<GameObject>();
    private GameObject foundationInstance; 

    private void Start()
    {
        SpawnFoundation();
    }

    private void SpawnFoundation()
    {
        foundationInstance = Instantiate(foundationPrefab, transform.position, Quaternion.identity);
        foundationInstance.transform.SetParent(transform);
    }

    [ContextMenu("Test Add Floor")] 
    public void AddFloor()
    {
        float currentY = transform.position.y + foundationHeight + (activeFloors.Count * roomHeight);
        Vector3 spawnPos = new Vector3(transform.position.x, currentY, transform.position.z);

        GameObject newFloor = GetFloorFromPool(spawnPos);
        newFloor.transform.SetParent(transform);

        newFloor.transform.position += Vector3.up * spawnHeightOffset; 
        newFloor.transform.DOMoveY(spawnPos.y, 0.4f).SetEase(Ease.OutBounce).SetLink(newFloor);

        if (dustEffectPrefab != null)
        {
            Instantiate(dustEffectPrefab, spawnPos, Quaternion.identity);
        }

        // --- NUEVO: Reproducir Feedback de Feel ---
        if (floorBuiltFeedback != null)
        {
            floorBuiltFeedback.PlayFeedbacks();
        }

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
        
        if (dustEffectPrefab != null)
        {
            Instantiate(dustEffectPrefab, floorToRemove.transform.position, Quaternion.identity);
        }

        // --- NUEVO: Reproducir Feedback de Destrucción (Feel) ---
        if (floorDestroyedFeedback != null)
        {
            floorDestroyedFeedback.PlayFeedbacks();
        }

        activeFloors.RemoveAt(activeFloors.Count - 1);


        floorToRemove.transform.DOShakePosition(0.2f, 0.5f).SetLink(floorToRemove).OnComplete(() => ReturnFloorToPool(floorToRemove));
    }

    private GameObject GetFloorFromPool(Vector3 position)
    {
        if (floorPool.Count > 0)
        {
            GameObject floor = floorPool.Pop();
            floor.transform.position = position;
            floor.transform.rotation = Quaternion.identity;
            floor.SetActive(true);
            return floor;
        }
        else
        {
            return Instantiate(roomPrefab, position, Quaternion.identity);
        }
    }

    private void ReturnFloorToPool(GameObject floor)
    {
        floor.SetActive(false);
        floorPool.Push(floor);
    }
}