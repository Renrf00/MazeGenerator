using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

using Random = UnityEngine.Random;

public class MazeSpliter : MonoBehaviour
{
    [HideInInspector] public static MazeSpliter instance;

    [Header("Maze dimensions")]
    [SerializeField] private int maxMazeX = 100;
    [SerializeField] private int maxMazeY = 100;
    [Min(0)] public int minRoomLength = 5;
    [SerializeField, Min(0)] private int wallThickness = 2;

    [Header("Room generation")]
    [SerializeField] private bool generateInstantly = false;
    [SerializeField, Min(0)] private float splitDelay = 0.2f;

    [SerializeField] private List<Room> rooms;
    [ReadOnly] public List<Room> completedRooms;

    [Header("State variables")]
    private bool splitingRooms = false;
    private bool addedWalls = false;
    [HideInInspector] public static bool finishedSpliting = false;

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
            if (!room.hide)
                AlgorithmsUtils.DebugRectInt(new RectInt(new Vector2Int(room.posX, room.posY), new Vector2Int(room.width, room.height)), Color.blue);
        }
        foreach (Room room in rooms)
        {
            if (!room.hide)
                AlgorithmsUtils.DebugRectInt(new RectInt(new Vector2Int(room.posX, room.posY), new Vector2Int(room.width, room.height)), Color.yellow);
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
        completedRooms.Sort(CompareRoomsY);
        AddWalls();
        splitingRooms = false;
    }

    /// <summary>
    /// Will stop the StartGeneration corutine
    /// </summary>
    [Button(enabledMode: EButtonEnableMode.Playmode)]
    private void StopSpliting()
    {
        StopCoroutine(StartSpliting());
        splitingRooms = false;
    }

    /// <summary>
    /// Will erase any existing rooms and create a room based on both maxMazeX/Y
    /// </summary>
    [Button(enabledMode: EButtonEnableMode.Playmode)]
    private void Reset()
    {
        addedWalls = false;
        finishedSpliting = false;

        rooms.Clear();
        completedRooms.Clear();
        rooms.Add(new Room(0, 0, maxMazeX, maxMazeY));
    }
    #endregion

    #region Spliting functions
    /// <summary>
    /// Will try to split the room horizontaly if its not possible, it will try to do so verticaly
    /// </summary>
    [Button(enabledMode: EButtonEnableMode.Playmode)]
    private void SplitHorizontal()
    {
        if (rooms.Count <= 0)
        {
            Debug.Log("No more rooms to split");
            return;
        }

        List<Room> tempRooms = new List<Room>();
        int splitingRoom = Random.Range(0, rooms.Count);
        int randomSplitDistance = Random.Range(minRoomLength, rooms[splitingRoom].width - minRoomLength);

        if (rooms[splitingRoom].widthLimit)
        {
            SplitVertical();
            return;
        }

        tempRooms.Add(new Room(
            rooms[splitingRoom].posX,
            rooms[splitingRoom].posY,
            randomSplitDistance,
            rooms[splitingRoom].height
            ));

        tempRooms.Add(new Room(
            rooms[splitingRoom].posX + randomSplitDistance,
            rooms[splitingRoom].posY,
            rooms[splitingRoom].width - randomSplitDistance,
            rooms[splitingRoom].height
            ));

        rooms.RemoveAt(splitingRoom);

        rooms.Insert(splitingRoom, tempRooms[0]);
        rooms.Insert(splitingRoom + 1, tempRooms[1]);

        CheckSplitLimits(rooms);
    }

    /// <summary>
    /// Will try to split the room verticaly if its not possible, it will try to do so horizontaly
    /// </summary>
    [Button(enabledMode: EButtonEnableMode.Playmode)]
    private void SplitVertical()
    {
        if (rooms.Count <= 0)
        {
            Debug.Log("No more rooms to split");
            return;
        }

        List<Room> tempRooms = new List<Room>();
        int splitingRoom = Random.Range(0, rooms.Count);
        int randomSplitDistance = Random.Range(minRoomLength, rooms[splitingRoom].height - minRoomLength);

        if (rooms[splitingRoom].heightLimit)
        {
            SplitHorizontal();
            return;
        }

        tempRooms.Add(new Room(
            rooms[splitingRoom].posX,
            rooms[splitingRoom].posY,
            rooms[splitingRoom].width,
            randomSplitDistance
            ));

        tempRooms.Add(new Room(
            rooms[splitingRoom].posX,
            rooms[splitingRoom].posY + randomSplitDistance,
            rooms[splitingRoom].width,
            rooms[splitingRoom].height - randomSplitDistance
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
    [Button(enabledMode: EButtonEnableMode.Playmode)]
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
            room.posX -= wallThickness / 2;
            room.posY -= wallThickness / 2;
            room.width += wallThickness;
            room.height += wallThickness;
        }
        addedWalls = true;
    }
    #endregion

    #region Additional functions
    /// <summary>
    /// Will check whether all input rooms can be split verticaly or horizontaly, if not, move the room to completedRooms
    /// </summary>
    /// <param name="Rooms to check"></param>
    private void CheckSplitLimits(List<Room> input)
    {
        List<Room> roomsToRemove = new List<Room>();

        foreach (Room room in input)
        {
            if (room.width <= minRoomLength * 2)
            {
                room.widthLimit = true;
            }

            if (room.height <= minRoomLength * 2)
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
            Debug.Log("Finished Spliting");
        }
    }

    /// <summary>
    /// Compares room1 and room2 first on their Y position and if its the same then in their X position.
    /// Returns -1 if room1 is closer to the origin, 1 if room1 is farther away from the origin1 and 0 if their position is the same
    /// </summary>
    public int CompareRoomsY(Room room1, Room room2)
    {
        if (room1.posY == room2.posY)
        {
            if (room1.posX == room2.posX)
            {
                return 0;
            }
            else if (room1.posX < room2.posX)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
        else if (room1.posY < room2.posY)
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }

    /// <summary>
    /// Compares room1 and room2 first on their X position and if its the same then in their Y position.
    /// Returns -1 if room1 is closer to the origin, 1 if room1 is farther away from the origin1 and 0 if their position is the same
    /// </summary>
    public int CompareRoomsX(Room room1, Room room2)
    {
        if (room1.posX == room2.posX)
        {
            if (room1.posY == room2.posY)
            {
                return 0;
            }
            else if (room1.posY < room2.posY)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
        else if (room1.posX < room2.posX)
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }
    #endregion

    #region Room class
    [Serializable]
    public class Room
    {
        public bool hide;
        public int posX;
        public int posY;
        public int width;
        public int height;
        public bool widthLimit = false;
        public bool heightLimit = false;

        public Room(int posX, int posY, int width, int height)
        {
            this.posX = posX;
            this.posY = posY;
            this.width = width;
            this.height = height;
        }
    }
    #endregion
}