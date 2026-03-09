using NaughtyAttributes;
using UnityEngine;

public class CameraUpdater : MonoBehaviour
{
    public static CameraUpdater instance;

    private MazeSpliter mazeSpliter;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        mazeSpliter = MazeSpliter.instance;

        UpdateCameraLocation();
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void UpdateCameraLocation()
    {
        Vector2 labrinthSize = new Vector2(mazeSpliter.maxMazeX, mazeSpliter.maxMazeY);
        transform.position = new Vector3(labrinthSize.x / 2, GetComponent<Camera>().transform.position.y, labrinthSize.y / 2);
        GetComponent<Camera>().orthographicSize = labrinthSize.x > labrinthSize.y ? labrinthSize.x / 2 + 10 : labrinthSize.y / 2 + 10;
    }
}
