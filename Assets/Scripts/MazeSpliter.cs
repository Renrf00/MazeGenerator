using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

using Random = UnityEngine.Random;

public class MazeSpliter : MonoBehaviour
{
    [HideInInspector] public static MazeSpliter instance;

    [Header("Random")]
    public bool randomizeSeed = true;
    public int seed = 0;

    [Header("Maze dimensions")]
    public int maxMazeX = 100;
    public int maxMazeY = 100;
    [Min(0)] public int minRoomLength = 5;
    [Min(0)] public int wallThickness = 2;

    [Header("Room generation")]
    [SerializeField] private bool generateInstantly = false;
    [SerializeField, Min(0)] private float splitDelay = 0.2f;

    private List<Room> rooms = new();
    [HideInInspector] public List<Room> completedRooms = new();

    [Header("State variables")]
    private bool splitingRooms = false;
    private bool addedWalls = false;
    [HideInInspector] public bool finishedSpliting = false;

    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        rooms.Add(new Room(0, 0, maxMazeX, maxMazeY));
    }

    void Update()
    {
        foreach (Room room in completedRooms)
        {
            AlgorithmsUtils.DebugRectInt(room.rectInt, Color.blue);
        }
        if (!generateInstantly)
            foreach (Room room in rooms)
            {
                AlgorithmsUtils.DebugRectInt(room.rectInt, Color.yellow);
            }
    }

    #region Controling generation
    /// <summary>
    /// Will reset the rooms and start spliting until all rooms cannot be split
    /// </summary>
    [Button(enabledMode: EButtonEnableMode.Playmode)]
    private IEnumerator StartSpliting()
    {
        if (splitingRooms)
            yield break;

        Reset();
        splitingRooms = true;

        while (rooms.Count > 0)
        {
            if (Random.Range(0, 1) == 1)
            {
                SplitHorizontal();
            }
            else
            {
                SplitVertical();
            }
            if (!generateInstantly)
                yield return new WaitForSeconds(splitDelay);
        }
        // completedRooms.Sort(CompareRoomsY);
        AddWalls();
        splitingRooms = false;

        Debug.Log("Finished Spliting");

        if (DoorGenerator.instance.autoGenerate)
            StartCoroutine(DoorGenerator.instance.GenerateDoors());
    }

    /// <summary>
    /// Will erase any existing rooms and create a room based on both maxMazeX/Y
    /// </summary>
    [Button(enabledMode: EButtonEnableMode.Playmode)]
    private void Reset()
    {
        if (!MazeSpliter.instance.randomizeSeed)
            Random.InitState(MazeSpliter.instance.seed);

        addedWalls = false;
        finishedSpliting = false;
        CameraUpdater.instance.UpdateCameraLocation();
        DoorGenerator.instance.Reset();
        NavigationGraph.instance.Reset();

        rooms.Clear();
        completedRooms.Clear();
        rooms.Add(new Room(0, 0, maxMazeX, maxMazeY));
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    private void StopAllGeneration()
    {
        StopAllCoroutines();
        splitingRooms = false;

    }
    #endregion

    #region Spliting functions
    /// <summary>
    /// Will try to split the room horizontaly if its not possible, it will try to do so verticaly
    /// </summary>
    private void SplitHorizontal()
    {
        if (rooms.Count <= 0)
        {
            Debug.Log("No more rooms to split");
            return;
        }

        List<Room> tempRooms = new List<Room>();
        int splitingRoom = Random.Range(0, rooms.Count);
        int randomSplitDistance = Random.Range(minRoomLength, rooms[splitingRoom].rectInt.width - minRoomLength);

        if (rooms[splitingRoom].widthLimit)
        {
            SplitVertical();
            return;
        }

        tempRooms.Add(new Room(
            rooms[splitingRoom].rectInt.x,
            rooms[splitingRoom].rectInt.y,
            randomSplitDistance,
            rooms[splitingRoom].rectInt.height
            ));

        tempRooms.Add(new Room(
            rooms[splitingRoom].rectInt.x + randomSplitDistance,
            rooms[splitingRoom].rectInt.y,
            rooms[splitingRoom].rectInt.width - randomSplitDistance,
            rooms[splitingRoom].rectInt.height
            ));

        rooms.RemoveAt(splitingRoom);

        rooms.Insert(splitingRoom, tempRooms[0]);
        rooms.Insert(splitingRoom + 1, tempRooms[1]);

        CheckSplitLimits(rooms);
    }

    /// <summary>
    /// Will try to split the room verticaly if its not possible, it will try to do so horizontaly
    /// </summary>
    private void SplitVertical()
    {
        if (rooms.Count <= 0)
        {
            Debug.Log("No more rooms to split");
            return;
        }

        List<Room> tempRooms = new List<Room>();
        int splitingRoom = Random.Range(0, rooms.Count);
        int randomSplitDistance = Random.Range(minRoomLength, rooms[splitingRoom].rectInt.height - minRoomLength);

        if (rooms[splitingRoom].heightLimit)
        {
            SplitHorizontal();
            return;
        }

        tempRooms.Add(new Room(
            rooms[splitingRoom].rectInt.x,
            rooms[splitingRoom].rectInt.y,
            rooms[splitingRoom].rectInt.width,
            randomSplitDistance
            ));

        tempRooms.Add(new Room(
            rooms[splitingRoom].rectInt.x,
            rooms[splitingRoom].rectInt.y + randomSplitDistance,
            rooms[splitingRoom].rectInt.width,
            rooms[splitingRoom].rectInt.height - randomSplitDistance
            ));

        rooms.RemoveAt(splitingRoom);

        rooms.Insert(splitingRoom, tempRooms[0]);
        rooms.Insert(splitingRoom + 1, tempRooms[1]);

        CheckSplitLimits(rooms);
    }
    #endregion

    #region AddWalls function
    /// <summary>
    /// Can only be executed after all rooms cannot be split anymore, will move all rooms southwest by (wallThickness / 2), and increase the size by (wallThickness)
    /// </summary>
    private void AddWalls()
    {
        if (rooms.Count > 0)
        {
            Debug.Log("You can only add walls when all rooms are split");
            return;
        }
        if (addedWalls)
        {
            Debug.Log("You can only split walls once");
            return;
        }

        foreach (Room room in completedRooms)
        {
            room.rectInt.x -= wallThickness / 2;
            room.rectInt.y -= wallThickness / 2;
            room.rectInt.width += wallThickness;
            room.rectInt.height += wallThickness;
        }
        addedWalls = true;
    }
    #endregion

    #region Additional functions
    /// <summary>
    /// Will check whether all input rooms can be split verticaly or horizontaly, if not, move the room to completedRooms
    /// </summary>
    private void CheckSplitLimits(List<Room> input)
    {
        List<Room> roomsToRemove = new List<Room>();

        foreach (Room room in input)
        {
            if (room.rectInt.width <= minRoomLength * 2)
            {
                room.widthLimit = true;
            }

            if (room.rectInt.height <= minRoomLength * 2)
            {
                room.heightLimit = true;
            }

            if (room.widthLimit && room.heightLimit)
            {
                completedRooms.Add(room);
                roomsToRemove.Add(room);
            }
        }
        foreach (Room room in roomsToRemove)
        {
            rooms.Remove(room);
        }

        if (rooms.Count == 0)
        {
            finishedSpliting = true;
        }
    }
    #endregion

    // <summary>
    // Compares room1 and room2 first on their Y position and if its the same then in their X position.
    // Returns -1 if room1 is closer to the origin, 1 if room1 is farther away from the origin1 and 0 if their position is the same
    // </summary>
    // public int CompareRoomsY(Room room1, Room room2)
    // {
    //     if (room1.posY == room2.posY)
    //     {
    //         if (room1.posX == room2.posX)
    //         {
    //             return 0;
    //         }
    //         else if (room1.posX < room2.posX)
    //         {
    //             return -1;
    //         }
    //         else
    //         {
    //             return 1;
    //         }
    //     }
    //     else if (room1.posY < room2.posY)
    //     {
    //         return -1;
    //     }
    //     else
    //     {
    //         return 1;
    //     }
    // }

    // <summary>
    // Compares room1 and room2 first on their X position and if its the same then in their Y position.
    // Returns -1 if room1 is closer to the origin, 1 if room1 is farther away from the origin1 and 0 if their position is the same
    // </summary>
    // public int CompareRoomsX(Room room1, Room room2)
    // {
    //     if (room1.rectInt.x == room2.rectInt.x)
    //     {
    //         if (room1.rectInt.y == room2.rectInt.y)
    //         {
    //             return 0;
    //         }
    //         else if (room1.rectInt.y < room2.rectInt.y)
    //         {
    //             return -1;
    //         }
    //         else
    //         {
    //             return 1;
    //         }
    //     }
    //     else if (room1.rectInt.x < room2.rectInt.x)
    //     {
    //         return -1;
    //     }
    //     else
    //     {
    //         return 1;
    //     }
    // }
}