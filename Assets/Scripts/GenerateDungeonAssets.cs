using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using NUnit.Framework;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class GeneratePrefabs : MonoBehaviour
{
    [HideInInspector] public static GeneratePrefabs instance;
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private UnityEvent OnGeneration;

    private Room[] rooms;
    private RectInt[] doors;

    [SerializeField] private GameObject[] wallPrefabs;
    [SerializeField] private GameObject floorPrefab;
    private int[,] tilemap;

    private void Awake()
    {
        instance = this;
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void GenerateDungeon()
    {
        SimpleDungeon();
    }

    private void SimpleDungeon()
    {
        Reset();
        GetRooms();
        GenerateTilemap();



        // Generation

        // foreach (Vector3Int wall in wallPositions)
        // {
        //     Instantiate(wallPrefab, wall, wallPrefab.transform.rotation, transform);
        // }

        // foreach (Vector3Int floor in floorPositions)
        // {
        //     Instantiate(floorPrefab, floor, wallPrefab.transform.rotation, transform);
        // }

        navMeshSurface.BuildNavMesh();
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    private void GenerateTilemap()
    {
        tilemap = new int[MazeSpliter.Instance.MaxMazeY + 1, MazeSpliter.Instance.MaxMazeX + 1];

        // wall
        foreach (var room in rooms)
        {
            for (int side = 1; side <= 4; side++)
            {
                for (int length = 0; length < (side < 3 ? room.rectInt.height : room.rectInt.width - 1); length++)
                {
                    tilemap[
                        side < 3 ? room.rectInt.position.y + 1 * length : (side == 3 ? room.rectInt.position.y : room.rectInt.position.y + room.rectInt.height - 1),
                        side > 2 ? room.rectInt.position.x + 1 * length : (side == 1 ? room.rectInt.position.x : room.rectInt.position.x + room.rectInt.width - 1)] = 1;
                }
            }
        }

        // doors
        foreach (RectInt door in doors)
        {
            for (int x = 0; x < door.width; x++)
            {
                tilemap[door.position.y, door.position.x + x] = 0;
            }

            for (int y = 1; y < door.height; y++)
            {
                tilemap[door.position.y + y, door.position.x] = 0;
            }
        }

        // PrintTilemap();
    }

    private void PrintTilemap()
    {
        string tiles = "";
        for (int y = 0; y < MazeSpliter.Instance.MaxMazeY + 1; y++)
        {
            for (int x = 0; x < MazeSpliter.Instance.MaxMazeX + 1; x++)
            {
                tiles += tilemap[MazeSpliter.Instance.MaxMazeY - y, x] == 0 ? "0" : "8";
            }
            tiles += "\n";
        }

        Debug.Log(tiles);
    }

    public void Reset()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        navMeshSurface.RemoveData();
    }

    private void GetRooms()
    {
        rooms = MazeSpliter.Instance.CompletedRooms.ToArray();
        doors = DoorGenerator.Instance.Doors.ToArray();
    }
}
