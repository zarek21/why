using UnityEngine;

public class LimitFPS : MonoBehaviour
{
    public static LimitFPS Instance;

    [SerializeField] private int _targetFPS = 60;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = _targetFPS;
    }

    public void SetFPS(int fps)
    {
        _targetFPS = fps;
        Application.targetFrameRate = _targetFPS;
    }

    public int GetCurrentFPS()
    {
        return _targetFPS;
    }
}
