using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public RectInt rectInt;
    public List<Room> adjacentRooms = new();
    public bool widthLimit = false;
    public bool heightLimit = false;
    public bool hasDoors = false;

    public Room(int posX, int posY, int width, int height)
    {
        rectInt = new RectInt(posX, posY, width, height);
    }

    public void ConnectRooms(Room room2)
    {
        if (!adjacentRooms.Contains(room2))
            adjacentRooms.Add(room2);
        if (!room2.adjacentRooms.Contains(this))
            room2.adjacentRooms.Add(this);
    }
}
