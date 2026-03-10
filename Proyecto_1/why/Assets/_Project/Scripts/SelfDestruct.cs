using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    [SerializeField] private float _lifetime = 2.0f;

    private void Start()
    {
        Destroy(gameObject, _lifetime);
    }
}