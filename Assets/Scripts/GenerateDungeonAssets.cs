using System.Collections.Generic;
using NaughtyAttributes;
using NUnit.Framework;
using Unity.AI.Navigation;
using UnityEngine;

public class GeneratePrefabs : MonoBehaviour
{
    [HideInInspector] public static GeneratePrefabs instance;
    [SerializeField] private NavMeshSurface navMeshSurface;

    private List<Room> rooms;
    private HashSet<RectInt> doors;

    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject floorPrefab;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {

    }

    private void Update()
    {

    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    private void SimpleDungeon()
    {
        GetRooms();

        HashSet<Vector3Int> wallPositions = new();
        HashSet<Vector3Int> floorPositions = new();

        // Walls

        foreach (var room in rooms)
        {
            for (int i = 1; i <= 4; i++)
            {
                for (int j = 0; j < (i < 3 ? room.rectInt.height : room.rectInt.width - 1); j++)
                {
                    wallPositions.Add(new Vector3Int(
                        i > 2 ? room.rectInt.position.x + 1 * j : (i == 1 ? room.rectInt.position.x : room.rectInt.position.x + room.rectInt.width - 1),
                        0,
                        i < 3 ? room.rectInt.position.y + 1 * j : (i == 3 ? room.rectInt.position.y : room.rectInt.position.y + room.rectInt.height - 1))
                    );
                }
            }
        }

        // Floors

        foreach (var room in rooms)
        {
            for (int x = 1; x < room.rectInt.width - 1; x++)
            {
                for (int y = 1; y < room.rectInt.height - 1; y++)
                {
                    floorPositions.Add(new Vector3Int(
                        room.rectInt.position.x + x,
                        0,
                        room.rectInt.position.y + y
                    ));
                }
            }
        }

        // Doors

        foreach (RectInt door in doors)
        {
            for (int x = 0; x < door.width; x++)
            {
                wallPositions.Remove(new Vector3Int(door.position.x + x, 0, door.position.y));
                floorPositions.Add(new Vector3Int(door.position.x + x, 0, door.position.y));
            }

            for (int y = 1; y < door.height; y++)
            {
                wallPositions.Remove(new Vector3Int(door.position.x, 0, door.position.y + y));
                floorPositions.Add(new Vector3Int(door.position.x, 0, door.position.y + y));
            }
        }

        // Generation

        foreach (Vector3Int wall in wallPositions)
        {
            Instantiate(wallPrefab, wall, wallPrefab.transform.rotation, transform);
        }

        foreach (Vector3Int floor in floorPositions)
        {
            Instantiate(floorPrefab, floor, wallPrefab.transform.rotation, transform);
        }

        navMeshSurface.BuildNavMesh();
    }

    private void GetRooms()
    {
        rooms = MazeSpliter.instance.completedRooms;
        doors = DoorGenerator.instance.doors;
    }
}
