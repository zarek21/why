using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Referencias")] [SerializeField]
    private BuildingManager _buildingManager;

    [Header("Altura del piso")] [SerializeField]
    private float _floorHeight = 3.0f;

    [Tooltip("Qué tan suave se mueve")] [SerializeField]
    private float _smoothSpeed = 5.0f;

    [Header("Efecto Game Over")]
    [Tooltip("Velocidad de bajada al finalizar partida")]
    [SerializeField]
    private float _gameOverSmoothSpeed = 2.0f;

    private Vector3 _initialPosition;

    private void Start()
    {
        _initialPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (_buildingManager == null) return;

        int floorsBuilt = _buildingManager.ActiveFloorCount;
        if (floorsBuilt < 0) floorsBuilt = 0;

        float targetZ = _initialPosition.z;
        float targetY = _initialPosition.y + (floorsBuilt * _floorHeight);
        float currentSpeed = _smoothSpeed;

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            targetY = _initialPosition.y;
            currentSpeed = _gameOverSmoothSpeed;
        }

        Vector3 destination = new Vector3(_initialPosition.x, targetY, targetZ);
        transform.position = Vector3.Lerp(transform.position, destination, Time.deltaTime * currentSpeed);
    }

    public void CameraRotation(float rotation)
    {
        if (_buildingManager != null && _buildingManager.ActiveFloorCount < 0)
        {
            transform.rotation = Quaternion.Euler(0, rotation, 0);
        }
    }


    public void RotateCamera(float angle)
    {
        _initialPosition = Quaternion.Euler(0, angle, 0) * _initialPosition;
        
        Vector3 lookAtTarget = new Vector3(0, transform.position.y, 0);
        transform.LookAt(lookAtTarget);
    }
}