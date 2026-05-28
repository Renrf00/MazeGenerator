using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class DoorGenerator : MonoBehaviour
{
    private static DoorGenerator instance;

    [SerializeField] private UnityEvent onDoorGeneration;
    [SerializeField] private int doorSize = 2;

    private Room[] rooms;
    private HashSet<RectInt> doors = new();
    private Graph<RectInt> adjacencyList = new();
    private bool generatingDoors = false;

    [SerializeField] private bool generateInstantly = false;
    [SerializeField, Min(0)] private float doorDelay = 0.01f;

    #region Public getters

    public static DoorGenerator Instance { get { return instance; } }
    public HashSet<RectInt> Doors { get { return doors; } }
    public Graph<RectInt> AdjacencyList { get { return adjacencyList; } }

    #endregion

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        foreach (RectInt door in doors)
        {
            AlgorithmsUtils.DebugRectInt(door, Color.cyan);
        }
    }

    #region Door generation
    /// <summary>
    /// Will reset the doors and start generating new doors
    /// </summary>
    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void RunDoorGeneration()
    {
        if (generatingDoors)
            return;

        Reset();
        generatingDoors = true;

        GetRooms();

        StartCoroutine(DoorGenerationCoroutine());
    }

    private IEnumerator DoorGenerationCoroutine()
    {
        for (int roomIndex = 0; roomIndex < rooms.Length; roomIndex++)
        {
            for (int compararisonIndex = roomIndex + 1; compararisonIndex < rooms.Length; compararisonIndex++)
            {
                if (AlgorithmsUtils.Intersects(rooms[roomIndex].rectInt, rooms[compararisonIndex].rectInt))
                {
                    CreateDoor(rooms[roomIndex], rooms[compararisonIndex]);

                    if (!generateInstantly)
                        yield return new WaitForSeconds(doorDelay);
                }
            }
        }

        generatingDoors = false;
        Debug.Log("Finished Generating Doors");

        onDoorGeneration.Invoke();
    }

    /// <summary>
    /// Will erase any existing doors
    /// </summary>
    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void Reset()
    {
        if (!MazeSpliter.Instance.RandomizeSeed)
            Random.InitState(MazeSpliter.Instance.Seed);

        doors = new();
        adjacencyList = new();
    }
    #endregion

    #region Door creation
    /// <summary>
    /// Will generate a door in the intersection between the 2 recieved rooms
    /// </summary>
    private void CreateDoor(Room room1, Room room2)
    {
        if (!SpaceForDoor(room1, room2))
            return;

        RectInt intersect = AlgorithmsUtils.Intersect(room1.rectInt, room2.rectInt);
        RectInt door;

        if (intersect.width > intersect.height)
        {
            door = new RectInt(
                intersect.x + Random.Range(MazeSpliter.Instance.WallThickness, intersect.width - doorSize),
                intersect.y,
                doorSize,
                intersect.height);
        }
        else
        {
            door = new RectInt(
                intersect.x,
                intersect.y + Random.Range(MazeSpliter.Instance.WallThickness, intersect.height - doorSize),
                intersect.width,
                doorSize);
        }

        doors.Add(door);
        adjacencyList.AddEdge(room1.rectInt, door);
        adjacencyList.AddEdge(room2.rectInt, door);
    }
    #endregion

    /// <summary>
    /// Recieves 2 rooms and returns true if a door can be created in the intersection
    /// </summary>
    private bool SpaceForDoor(Room room1, Room room2)
    {
        RectInt intersect = AlgorithmsUtils.Intersect(room1.rectInt, room2.rectInt);

        return !(((intersect.width > intersect.height ? intersect.width : intersect.height) - 2 * MazeSpliter.Instance.WallThickness) < (doorSize + 2 * MazeSpliter.Instance.WallThickness));
    }

    /// <summary>
    /// Set "rooms" based on MazeSpliter's "completedRooms"
    /// </summary>
    private void GetRooms()
    {
        rooms = MazeSpliter.Instance.CompletedRooms.ToArray();
    }
}
