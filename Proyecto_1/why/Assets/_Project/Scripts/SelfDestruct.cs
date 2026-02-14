using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    [Tooltip("Tiempo en segundos antes de destruir este objeto")]
    [SerializeField] private float lifetime = 2.0f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}