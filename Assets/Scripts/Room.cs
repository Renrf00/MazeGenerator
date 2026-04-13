using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public RectInt rectInt;
    public bool widthLimit = false;
    public bool heightLimit = false;
    public bool hasDoors = false;

    public Room(int posX, int posY, int width, int height)
    {
        rectInt = new RectInt(posX, posY, width, height);
    }
}
