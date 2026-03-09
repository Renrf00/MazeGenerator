using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class DoorGenerator : MonoBehaviour
{
    public static DoorGenerator instance;
    private MazeSpliter mazeSpliter;
    [SerializeField] private int doorSize = 2;
    private Room[] rooms;
    private List<RectInt> doors = new();
    public bool autoGenerate = true;
    [SerializeField] private bool generateInstantly = false;
    [SerializeField, Min(0)] private float doorDelay = 0.01f;

    void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        mazeSpliter = MazeSpliter.instance;
    }

    void Update()
    {
        foreach (RectInt door in doors)
        {
            AlgorithmsUtils.DebugRectInt(door, Color.cyan);
        }
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public IEnumerator GenerateDoors()
    {
        Reset();

        GetRooms();

        for (int roomIndex = 0; roomIndex < rooms.Length; roomIndex++)
        {
            for (int compararisonIndex = roomIndex + 1; compararisonIndex < rooms.Length; compararisonIndex++)
            {
                if (AlgorithmsUtils.Intersects(rooms[roomIndex].rectInt, rooms[compararisonIndex].rectInt))
                {
                    rooms[roomIndex].ConnectRooms(rooms[compararisonIndex]);

                    if (SpaceForDoor(rooms[roomIndex], rooms[compararisonIndex]))
                    {
                        GenerateDoor(rooms[roomIndex], rooms[compararisonIndex]);
                    }

                    if (!generateInstantly)
                        yield return new WaitForSeconds(doorDelay);
                }
            }
        }

        Debug.Log("Finished Generating Doors");
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void Reset()
    {
        doors = new();
    }

    private bool SpaceForDoor(Room room1, Room room2)
    {
        RectInt intersect = AlgorithmsUtils.Intersect(room1.rectInt, room2.rectInt);

        if (((intersect.width > intersect.height ? intersect.width : intersect.height) - 2 * mazeSpliter.wallThickness) < (doorSize + 2 * mazeSpliter.wallThickness))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void GenerateDoor(Room room1, Room room2)
    {
        RectInt intersect = AlgorithmsUtils.Intersect(room1.rectInt, room2.rectInt);

        if (intersect.width > intersect.height)
        {
            doors.Add(new RectInt(
                intersect.x + Random.Range(mazeSpliter.wallThickness, intersect.width - doorSize),
                intersect.y,
                doorSize,
                intersect.height));
        }
        else
        {
            doors.Add(new RectInt(
                intersect.x,
                intersect.y + Random.Range(mazeSpliter.wallThickness, intersect.height - doorSize),
                intersect.width,
                doorSize));
        }
    }

    private void GetRooms()
    {
        rooms = new Room[mazeSpliter.completedRooms.Count];
        int index = 0;

        foreach (Room room in mazeSpliter.completedRooms)
        {
            rooms[index] = room;
            index++;
        }
    }
}
