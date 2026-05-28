using NaughtyAttributes;
using UnityEngine;

public class CameraUpdater : MonoBehaviour
{
    private static CameraUpdater instance;
    public static CameraUpdater Instance { get { return instance; } }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        UpdateCameraLocation();
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void UpdateCameraLocation()
    {
        Vector2 labrinthSize = new Vector2(MazeSpliter.Instance.MaxMazeX, MazeSpliter.Instance.MaxMazeY);
        transform.position = new Vector3(labrinthSize.x / 2, GetComponent<Camera>().transform.position.y, labrinthSize.y / 2);
        GetComponent<Camera>().orthographicSize = labrinthSize.x > labrinthSize.y ? labrinthSize.x / 2 + 10 : labrinthSize.y / 2 + 10;
    }
}
