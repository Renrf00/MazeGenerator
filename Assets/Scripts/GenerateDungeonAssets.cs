using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Events;

public class GeneratePrefabs : MonoBehaviour
{
    private static GeneratePrefabs instance;

    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private UnityEvent OnGeneration;
    [SerializeField] private bool generateInstantly;
    [SerializeField] private float generationDelay = 0.01f;
    [SerializeField] private GameObject[] wallPrefabs;
    [SerializeField] private GameObject floorPrefab;

    private Room[] rooms;
    private RectInt[] doors;

    private Graph<Vector2Int> adjacencyList;
    private List<Vector2Int> done;

    private int[,] tilemap;

    private bool generatingDungeon = false;
    private int currentX = 0;
    private int currentY = 0;

    #region Public Getters

    public static GeneratePrefabs Instance { get { return instance; } }

    #endregion

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (!generatingDungeon || generateInstantly) return;
        AlgorithmsUtils.DebugRectInt(new RectInt(new Vector2Int(currentX, currentY), new Vector2Int(2, 2)), Color.yellow);
    }
    #region Dungeon generation
    /// <summary>
    /// Starts wall generation process
    /// </summary>
    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void GenerateDungeon()
    {
        StartCoroutine(SimpleDungeon());
    }

    /// <summary>
    /// Generates walls and starts floor generation
    /// </summary>
    private IEnumerator SimpleDungeon()
    {
        Reset();
        GetRooms();
        GenerateTilemap();
        generatingDungeon = true;

        // Walls
        for (int y = 0; y < MazeSpliter.Instance.MaxMazeY; y++)
        {
            for (int x = 0; x < MazeSpliter.Instance.MaxMazeX; x++)
            {
                currentX = x;
                currentY = y;

                int nPrefab = tilemap[y, x] + tilemap[y + 1, x] * 2 + tilemap[y + 1, x + 1] * 4 + tilemap[y, x + 1] * 8;

                if (nPrefab == 0) continue;

                Instantiate(wallPrefabs[nPrefab - 1], new Vector3(x + 1, 0, y + 1), wallPrefabs[nPrefab - 1].transform.rotation, transform);
                if (!generateInstantly) yield return new WaitForSeconds(generationDelay);
            }
        }

        // Floor
        yield return StartCoroutine(FloorFill(GetRandomEmptyTile()));

        navMeshSurface.BuildNavMesh();

        Debug.Log("Finished generation");
        generatingDungeon = false;
    }

    /// <summary>
    /// Generates floor
    /// </summary>
    private IEnumerator FloorFill(Vector2Int tile)
    {
        done.Add(tile);
        Instantiate(floorPrefab, new Vector3(tile.x + 0.5f, 0, tile.y + 0.5f), floorPrefab.transform.rotation, transform);

        foreach (Vector2Int connection in adjacencyList.GetNeighbors(tile))
        {
            if (!done.Contains(connection) && tilemap[connection.y, connection.x] == 0)
            {
                if (!generateInstantly) yield return new WaitForSeconds(generationDelay);
                yield return StartCoroutine(FloorFill(connection));
            }
        }
    }

    public void Reset()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        adjacencyList = new();
        done = new();

        navMeshSurface.RemoveData();
        generatingDungeon = false;
    }
    #endregion

    #region Other functions
    [Button(enabledMode: EButtonEnableMode.Playmode)]
    private void GenerateTilemap()
    {
        tilemap = new int[MazeSpliter.Instance.MaxMazeY + 1, MazeSpliter.Instance.MaxMazeX + 1];

        // Wall
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

        // Doors
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

        GenerateAdjacentcyList();
        // PrintTilemap();
    }

    private void GetRooms()
    {
        rooms = MazeSpliter.Instance.CompletedRooms.ToArray();
        doors = DoorGenerator.Instance.Doors.ToArray();
    }

    private void GenerateAdjacentcyList()
    {
        for (int x = 0; x < MazeSpliter.Instance.MaxMazeX; x++)
        {
            for (int y = 0; y < MazeSpliter.Instance.MaxMazeX; y++)
            {
                if (x >= 1)
                    adjacencyList.AddEdge(new Vector2Int(x, y), new Vector2Int(x - 1, y));
                if (y >= 1)
                    adjacencyList.AddEdge(new Vector2Int(x, y), new Vector2Int(x, y - 1));
                if (x <= MazeSpliter.Instance.MaxMazeX - 1)
                    adjacencyList.AddEdge(new Vector2Int(x, y), new Vector2Int(x + 1, y));
                if (y <= MazeSpliter.Instance.MaxMazeY - 1)
                    adjacencyList.AddEdge(new Vector2Int(x, y), new Vector2Int(x, y + 1));
            }
        }
    }

    private Vector2Int GetRandomEmptyTile()
    {
        List<Vector2Int> list = new();
        foreach (Room room in rooms)
        {
            list.Add(room.rectInt.position += new Vector2Int(1, 1));
        }
        return list[Random.Range(0, list.Count)];
    }
    #endregion
}
