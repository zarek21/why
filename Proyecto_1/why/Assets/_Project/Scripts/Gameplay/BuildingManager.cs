using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; 
using MoreMountains.Feedbacks; 

public class BuildingManager : MonoBehaviour
{
    [Header("Configuración de Alturas")]
    [Tooltip("Altura exacta del bloque de concreto base")]
    [SerializeField] private float _foundationHeight = 4; 
    
    [Tooltip("Altura exacta de cada piso residencial")]
    [SerializeField] private float _roomHeight = 3.7f; 

    [Header("Referencias a Prefabs")]
    [SerializeField] private GameObject _foundationPrefab; 
    [SerializeField] private GameObject _roomPrefab;       
    
    [Header("Efectos Visuales y Sonoros")]
    [SerializeField] private GameObject _dustEffectPrefab; 
    [Tooltip("Feedback de Feel que se reproducirá al construir un piso")]
    [SerializeField] private MMF_Player _floorBuiltFeedback; 
    

    [Header("Configuración de Spawn")]
    [Tooltip("Altura desde donde cae el piso al instanciarse")]
    [SerializeField] private float _spawnHeightOffset = 5f;

    private List<GameObject> _activeFloors = new List<GameObject>(); 
    public int ActiveFloorCount => _activeFloors.Count;
    private Stack<GameObject> _floorPool = new Stack<GameObject>();
    private GameObject _foundationInstance; 

    private void Start()
    {
        SpawnFoundation();
    }

    private void SpawnFoundation()
    {
        _foundationInstance = Instantiate(_foundationPrefab, transform.position, Quaternion.identity);
        _foundationInstance.transform.SetParent(transform);
    }

    [ContextMenu("Test Add Floor")] 
    public void AddFloor()
    {
        float currentY = transform.position.y + _foundationHeight + (_activeFloors.Count * _roomHeight);
        Vector3 spawnPos = new Vector3(transform.position.x, currentY, transform.position.z);

        GameObject newFloor = GetFloorFromPool(spawnPos);
        newFloor.transform.SetParent(transform);

        newFloor.transform.position += Vector3.up * _spawnHeightOffset; 
        newFloor.transform.DOMoveY(spawnPos.y, 0.4f).SetEase(Ease.OutBounce).SetLink(newFloor);

        if (_dustEffectPrefab != null)
        {
            Instantiate(_dustEffectPrefab, spawnPos, Quaternion.identity);
        }

        if (_floorBuiltFeedback != null)
        {
            _floorBuiltFeedback.PlayFeedbacks();
        }

        _activeFloors.Add(newFloor);
        
        if (GameManager.Instance != null) GameManager.Instance.AddFloorScore();
    }

    [ContextMenu("Test Remove Floor")]
    public void RemoveTopFloor()
    {
        if (GameManager.Instance != null) 
        {
            GameManager.Instance.RemoveFloorScore();
        }

        if (_activeFloors.Count == 0)
        {
            return;
        }

        GameObject floorToRemove = _activeFloors[_activeFloors.Count - 1];
        
        if (_dustEffectPrefab != null)
        {
            Instantiate(_dustEffectPrefab, floorToRemove.transform.position, Quaternion.identity);
        }

       
        _activeFloors.RemoveAt(_activeFloors.Count - 1);


        floorToRemove.transform.DOShakePosition(0.2f, 0.5f).SetLink(floorToRemove).OnComplete(() => ReturnFloorToPool(floorToRemove));
    }

    private GameObject GetFloorFromPool(Vector3 position)
    {
        if (_floorPool.Count > 0)
        {
            GameObject floor = _floorPool.Pop();
            floor.transform.position = position;
            floor.transform.rotation = Quaternion.identity;
            floor.SetActive(true);
            return floor;
        }
        else
        {
            return Instantiate(_roomPrefab, position, Quaternion.identity);
        }
    }

    private void ReturnFloorToPool(GameObject floor)
    {
        floor.SetActive(false);
        _floorPool.Push(floor);
    }
}