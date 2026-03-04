using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using NUnit.Framework;
using UnityEngine;

public class DoorGenerator : MonoBehaviour
{
    private MazeSpliter mazeSpliter;
    [ReadOnly, SerializeField] private MazeSpliter.Room[] rooms;
    private int currentRoomIndex = 0;

    private void Start()
    {
        mazeSpliter = MazeSpliter.instance;

        StartCoroutine(GenerateDoors());
    }

    private IEnumerator GenerateDoors()
    {
        yield return new WaitUntil(() => MazeSpliter.finishedSpliting == true);


    }

    private List<MazeSpliter.Room> FindAdjacentRooms(MazeSpliter.Room room)
    {
        List<MazeSpliter.Room> adjacentRooms = new List<MazeSpliter.Room>();
        List<MazeSpliter.Room> posibleAdjacentRooms = new List<MazeSpliter.Room>();

        int minY = room.posY - 2 * mazeSpliter.minRoomLength;
        int minX = room.posX - 2 * mazeSpliter.minRoomLength;
        int maxY = room.posY + room.height;
        int maxX = room.posX + room.width;

        for (int index = FindRoomIndex(rooms, minY, true, false); index <= FindRoomIndex(rooms, maxY, true, true); index++)
        {
            if (rooms[index].posX >= minX || rooms[index].posX < maxX)
            {
                posibleAdjacentRooms.Add(rooms[index]);
            }
        }
        posibleAdjacentRooms.Remove(room);

        foreach (MazeSpliter.Room posibleAdjacent in posibleAdjacentRooms)
        {
            if (AlgorithmsUtils.Intersects(new RectInt(new Vector2Int(posibleAdjacent.posX, posibleAdjacent.posY), new Vector2Int(posibleAdjacent.width, posibleAdjacent.height)), new RectInt(new Vector2Int(room.posX, room.posY), new Vector2Int(room.width, room.height))))
            {
                adjacentRooms.Add(posibleAdjacent);
            }
        }

        return adjacentRooms;
    }

    private void GenerateDoor()
    {

    }

    private int FindRoomIndex(MazeSpliter.Room[] list, int targetInt, bool searchY, bool lowerFallback)
    {
        int lowClamp = 0;
        int highClamp = list.Length;
        int mid;

        while (true)
        {
            mid = (int)Mathf.Floor((lowClamp + highClamp) / 2f);

            if ((searchY ? list[mid].posY : list[mid].posX) == targetInt)
            {
                return mid;
            }
            else if ((searchY ? list[mid].posY : list[mid].posX) < targetInt)
            {
                if (highClamp == mid)
                {
                    return mid;
                }
                highClamp = mid;
            }
            else
            {
                if (lowClamp == mid)
                {
                    return mid + (lowerFallback ? 0 : 1);
                }
                lowClamp = mid;
            }
        }
    }

    private void GetRooms()
    {
        rooms = new MazeSpliter.Room[mazeSpliter.completedRooms.Count];
        int index = 0;

        foreach (MazeSpliter.Room room in mazeSpliter.completedRooms)
        {
            rooms[index] = room;
        }
    }
}
