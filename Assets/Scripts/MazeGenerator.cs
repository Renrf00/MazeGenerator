using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NaughtyAttributes;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField] private int mazeMaxX = 100;
    [SerializeField] private int mazeMaxY = 100;
    [SerializeField, Min(0)] private int minRoomLength = 5;
    // has to be an even number
    [SerializeField, Min(0)] private int wallThickness = 2;
    [SerializeField, Min(0)] private float splitDelay = 0.2f;

    [SerializeField] private List<Room> rooms;
    [SerializeField] private List<Room> completedRooms;
    private bool generatingRooms = false;
    private bool addedWalls = false;

    void Start()
    {
        rooms.Add(new Room(0, 0, mazeMaxX, mazeMaxY));
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

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public IEnumerator StartGeneration()
    {
        if (generatingRooms)
            yield break;

        Reset();
        generatingRooms = true;

        while (rooms.Count > 0)
        {
            if (UnityEngine.Random.Range(0, 1) == 1)
            {
                SplitHorizontal();
            }
            else
            {
                SplitVertical();
            }
            yield return new WaitForSeconds(splitDelay);
        }
        AddWalls();
        generatingRooms = false;
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void StopGeneration()
    {
        StopAllCoroutines();
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void Reset()
    {
        addedWalls = false;

        rooms.Clear();
        completedRooms.Clear();
        rooms.Add(new Room(0, 0, mazeMaxX, mazeMaxY));
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void SplitHorizontal()
    {
        if (rooms.Count <= 0)
        {
            Debug.Log("No more rooms to split");
            return;
        }

        List<Room> tempRooms = new List<Room>();
        int splitingRoom = UnityEngine.Random.Range(0, rooms.Count);
        int randomSplitDistance = UnityEngine.Random.Range(minRoomLength, rooms[splitingRoom].width - minRoomLength);

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

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void SplitVertical()
    {
        if (rooms.Count <= 0)
        {
            Debug.Log("No more rooms to split");
            return;
        }

        List<Room> tempRooms = new List<Room>();
        int splitingRoom = UnityEngine.Random.Range(0, rooms.Count);
        int randomSplitDistance = UnityEngine.Random.Range(minRoomLength, rooms[splitingRoom].height - minRoomLength);

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

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void AddWalls()
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

        foreach (Room room in rooms)
        {
            room.posX -= wallThickness / 2;
            room.posY -= wallThickness / 2;
            room.width += wallThickness;
            room.height += wallThickness;
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

    public void CheckSplitLimits(List<Room> input)
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
    }

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
}