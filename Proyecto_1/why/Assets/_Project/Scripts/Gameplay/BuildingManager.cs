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
    [SerializeField] private GameObject foundationPrefab; 
    [SerializeField] private GameObject roomPrefab;       
    
    [Header("Efectos Visuales")]
    [SerializeField] private GameObject dustEffectPrefab; 

    private List<GameObject> activeFloors = new List<GameObject>(); 
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

        GameObject newFloor = Instantiate(roomPrefab, spawnPos, Quaternion.identity);
        newFloor.transform.SetParent(transform);

        newFloor.transform.position += Vector3.up * 5f; 
        newFloor.transform.DOMoveY(spawnPos.y, 0.4f).SetEase(Ease.OutBounce);

        if (dustEffectPrefab != null)
        {
            Instantiate(dustEffectPrefab, spawnPos, Quaternion.identity);
        }
        // -------------------------------------------

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
        // ---------------------------------------------

        activeFloors.RemoveAt(activeFloors.Count - 1);

        floorToRemove.transform.DOShakePosition(0.2f, 0.5f);
        Destroy(floorToRemove, 0.2f);
    }
}