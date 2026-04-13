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
    public Dictionary<RectInt, List<RectInt>> adjacencyList = new();

    public bool autoGenerate = true;
    [SerializeField] private bool generateInstantly = false;
    [SerializeField, Min(0)] private float doorDelay = 0.01f;

    private bool generatingDoors = false;

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

    /// <summary>
    /// Will reset the doors and start generating doors again
    /// </summary>
    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public IEnumerator GenerateDoors()
    {
        if (generatingDoors)
            yield break;

        Reset();
        generatingDoors = true;

        GetRooms();

        for (int roomIndex = 0; roomIndex < rooms.Length; roomIndex++)
        {
            for (int compararisonIndex = roomIndex + 1; compararisonIndex < rooms.Length; compararisonIndex++)
            {
                if (AlgorithmsUtils.Intersects(rooms[roomIndex].rectInt, rooms[compararisonIndex].rectInt))
                {
                    GenerateDoor(rooms[roomIndex], rooms[compararisonIndex]);

                    if (!generateInstantly)
                        yield return new WaitForSeconds(doorDelay);
                }
            }
        }

        generatingDoors = false;
        Debug.Log("Finished Generating Doors");
        if (NavigationGraph.instance.autoGenerate)
            StartCoroutine(NavigationGraph.instance.StartSearch());
    }

    /// <summary>
    /// Will erase any existing doors
    /// </summary>
    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void Reset()
    {
        if (!MazeSpliter.instance.randomizeSeed)
            Random.InitState(MazeSpliter.instance.seed);

        doors = new();
        adjacencyList = new();
    }

    /// <summary>
    /// Will recieve 2 rooms and return whether a door can be generated in the intersection
    /// </summary>
    private bool SpaceForDoor(Room room1, Room room2)
    {
        RectInt intersect = AlgorithmsUtils.Intersect(room1.rectInt, room2.rectInt);

        return !(((intersect.width > intersect.height ? intersect.width : intersect.height) - 2 * mazeSpliter.wallThickness) < (doorSize + 2 * mazeSpliter.wallThickness));
    }

    /// <summary>
    /// Will generate a door in the intersection between the 2 recieved rooms
    /// </summary>
    private void GenerateDoor(Room room1, Room room2)
    {
        if (!SpaceForDoor(room1, room2))
            return;
        RectInt intersect = AlgorithmsUtils.Intersect(room1.rectInt, room2.rectInt);
        RectInt door;

        if (intersect.width > intersect.height)
        {
            door = new RectInt(
                intersect.x + Random.Range(mazeSpliter.wallThickness, intersect.width - doorSize),
                intersect.y,
                doorSize,
                intersect.height);
        }
        else
        {
            door = new RectInt(
                intersect.x,
                intersect.y + Random.Range(mazeSpliter.wallThickness, intersect.height - doorSize),
                intersect.width,
                doorSize);
        }
        doors.Add(door);
        ConnectRects(room1.rectInt, door);
        ConnectRects(room2.rectInt, door);
    }

    /// <summary>
    /// Set rooms based on MazeSpliter completed rooms
    /// </summary>
    private void GetRooms()
    {
        rooms = new Room[mazeSpliter.completedRooms.Count];
        int index = 0;

        foreach (Room room in mazeSpliter.completedRooms)
        {
            adjacencyList.Add(room.rectInt, new());
            rooms[index] = room;
            index++;
        }
    }

    private void ConnectRects(RectInt room1, RectInt room2)
    {
        if (!adjacencyList.ContainsKey(room1))
            adjacencyList.Add(room1, new());

        if (!adjacencyList.ContainsKey(room2))
            adjacencyList.Add(room2, new());

        adjacencyList[room1].Add(room2);
        adjacencyList[room2].Add(room1);
    }
}
