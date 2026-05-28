using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class MazeSpliter : MonoBehaviour
{
    private static MazeSpliter instance;

    [SerializeField] private UnityEvent onRoomGeneration;
    [Header("Random")]
    [SerializeField] private bool randomizeSeed = true;
    [SerializeField] private int seed = 0;

    [Header("Maze parameters")]
    [SerializeField] private int maxMazeX = 100;
    [SerializeField] private int maxMazeY = 100;
    [SerializeField, Min(0)] private int minRoomLength = 5;
    [SerializeField, Min(0)] private int wallThickness = 2;
    [SerializeField, Range(0, 50)] private int removePercent = 10;

    [Header("Room generation")]
    [SerializeField] private bool autoGenerate;
    [SerializeField] private bool generateInstantly = false;
    [SerializeField, Min(0)] private float splitDelay = 0.2f;
    private Queue<Room> rooms = new();
    private HashSet<Room> completedRooms = new();

    [Header("State variables")]
    private bool splitingRooms = false;
    private bool addedWalls = false;

    #region Public getters

    public static MazeSpliter Instance { get { return instance; } }
    public bool RandomizeSeed { get { return randomizeSeed; } }
    public int Seed { get { return seed; } }
    public int MaxMazeX { get { return maxMazeX; } }
    public int MaxMazeY { get { return maxMazeY; } }
    public int MinRoomLength { get { return minRoomLength; } }
    public int WallThickness { get { return wallThickness; } }
    public int RemovePercent { get { return removePercent; } }
    public HashSet<Room> CompletedRooms { get { return completedRooms; } }

    #endregion

    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        if (autoGenerate)
        {
            StartCoroutine(StartSpliting());
        }
    }

    void Update()
    {
        foreach (Room room in completedRooms)
        {
            AlgorithmsUtils.DebugRectInt(room.rectInt, Color.blue);
        }

        if (generateInstantly) return;

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
            Room splitingRoom = rooms.Dequeue();

            if (SplitLimit(splitingRoom))
            {
                completedRooms.Add(splitingRoom);
                continue;
            }

            if (splitingRoom.heightLimit || splitingRoom.widthLimit)
            {
                if (splitingRoom.widthLimit)
                    SplitVertical(splitingRoom);
                else
                    SplitHorizontal(splitingRoom);
            }
            else
            {
                if (Random.Range(0, 1) == 1)
                    SplitHorizontal(splitingRoom);
                else
                    SplitVertical(splitingRoom);
            }

            if (!generateInstantly) yield return new WaitForSeconds(splitDelay);
        }

        AddWalls();
        splitingRooms = false;

        onRoomGeneration.Invoke();

        Debug.Log("Finished spliting");
    }

    /// <summary>
    /// Will erase the entire maze and create a room based on both maxMazeX/Y
    /// </summary>
    [Button(enabledMode: EButtonEnableMode.Playmode)]
    private void Reset()
    {
        if (!MazeSpliter.instance.randomizeSeed)
            Random.InitState(MazeSpliter.instance.seed);

        addedWalls = false;
        CameraUpdater.Instance.UpdateCameraLocation();
        DoorGenerator.Instance.Reset();
        NavigationGraph.Instance.Reset();

        rooms.Clear();
        completedRooms.Clear();
        rooms.Enqueue(new Room(0, 0, maxMazeX, maxMazeY));
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
    /// Will split the given room horizontaly
    /// </summary>
    private void SplitHorizontal(Room splitingRoom)
    {
        int randomSplitDistance = Random.Range(minRoomLength, splitingRoom.rectInt.width - minRoomLength);

        Room room1 = new Room(
            splitingRoom.rectInt.x,
            splitingRoom.rectInt.y,
            randomSplitDistance,
            splitingRoom.rectInt.height
            );

        Room room2 = new Room(
            splitingRoom.rectInt.x + randomSplitDistance,
            splitingRoom.rectInt.y,
            splitingRoom.rectInt.width - randomSplitDistance,
            splitingRoom.rectInt.height
            );

        rooms.Enqueue(room1);
        rooms.Enqueue(room2);
    }

    /// <summary>
    /// Will split the given room verticaly
    /// </summary>
    private void SplitVertical(Room splitingRoom)
    {
        int randomSplitDistance = Random.Range(minRoomLength, splitingRoom.rectInt.height - minRoomLength);

        Room room1 = new Room(
            splitingRoom.rectInt.x,
            splitingRoom.rectInt.y,
            splitingRoom.rectInt.width,
            randomSplitDistance
            );

        Room room2 = new Room(
            splitingRoom.rectInt.x,
            splitingRoom.rectInt.y + randomSplitDistance,
            splitingRoom.rectInt.width,
            splitingRoom.rectInt.height - randomSplitDistance
            );

        rooms.Enqueue(room1);
        rooms.Enqueue(room2);
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
    /// Checks if the given room can be split verticaly or horizontaly, changing its limit values accordingly. If it can still be split return true 
    /// </summary>
    private bool SplitLimit(Room room)
    {
        if (room.rectInt.width <= minRoomLength * 2)
        {
            room.widthLimit = true;
        }

        if (room.rectInt.height <= minRoomLength * 2)
        {
            room.heightLimit = true;
        }

        return room.widthLimit && room.heightLimit;
    }
    #endregion
}