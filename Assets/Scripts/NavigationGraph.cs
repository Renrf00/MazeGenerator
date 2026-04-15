using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class NavigationGraph : MonoBehaviour
{
    [HideInInspector] public static NavigationGraph instance;

    public Dictionary<RectInt, List<RectInt>> adjacencyList;
    private List<RectInt> done = new();

    private enum SearchType { BFS, DFS }
    [SerializeField] private SearchType searchType = SearchType.BFS;
    public bool autoGenerate;
    [SerializeField] private bool generateInstantly = false;
    [SerializeField] private float NavigationDelay = 0.01f;
    [SerializeField] private Queue<RectInt> toDo;

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        foreach (RectInt node in done)
        {
            foreach (RectInt connection in adjacencyList[node])
                Debug.DrawLine(new Vector3(node.x + node.width / 2, 0, node.y + node.height / 2), new Vector3(connection.x + connection.width / 2, 0, connection.y + connection.height / 2), Color.red, 0f);
        }
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public IEnumerator StartSearch()
    {
        Reset();
        RectInt startNode = GetRandomNode();

        switch (searchType)
        {
            case SearchType.BFS:
                toDo = new();
                toDo.Enqueue(startNode);
                done.Add(startNode);
                yield return StartCoroutine(BFS());
                break;
            case SearchType.DFS:
                yield return StartCoroutine(DFS(startNode));
                break;
        }

        if (done.Count == adjacencyList.Keys.Count)
        {
            Debug.Log("The entire dungeon is connected");
        }
        else
        {
            Debug.Log("Some rooms are not connected");
        }
    }

    private IEnumerator BFS()
    {
        if (toDo.Count == 0)
            yield break;


        if (!generateInstantly)
            yield return new WaitForSeconds(NavigationDelay);

        RectInt node = toDo.Dequeue();

        foreach (RectInt connection in adjacencyList[node])
        {
            if (!done.Contains(connection))
            {
                toDo.Enqueue(connection);
                done.Add(connection);
            }
        }

        StartCoroutine(BFS());
    }

    private IEnumerator DFS(RectInt node)
    {
        done.Add(node);

        foreach (RectInt connection in adjacencyList[node])
        {
            if (!done.Contains(connection))
            {
                if (!generateInstantly)
                    yield return new WaitForSeconds(NavigationDelay);
                yield return StartCoroutine(DFS(connection));
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Button(enabledMode: EButtonEnableMode.Playmode)]
    private void RemoveRooms()
    {
        List<RectInt> rectIntList = new();
        foreach (Room room in MazeSpliter.instance.completedRooms)
        {
            rectIntList.Add(room.rectInt);
        }

        rectIntList.Sort(CompareRoomsSize);

        int targetRemoves = MazeSpliter.instance.completedRooms.Count * MazeSpliter.instance.removePercent / 100;

        for (int i = 0; i < targetRemoves; i++)
        {

        }
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void Reset()
    {
        if (!MazeSpliter.instance.randomizeSeed)
            Random.InitState(MazeSpliter.instance.seed);

        done = new();
        adjacencyList = DoorGenerator.instance.adjacencyList;
    }

    private RectInt GetRandomNode()
    {
        List<RectInt> list = new();
        foreach (RectInt node in adjacencyList.Keys)
        {
            list.Add(node);
        }
        return list[Random.Range(0, list.Count)];
    }

    /// <summary>
    /// Compares room1 and room2 on their area,
    /// Returns -1 if room1 is smaller, 1 if room1 is larger and 0 if their size is the same
    /// </summary>
    public int CompareRoomsSize(RectInt room1, RectInt room2)
    {
        int room1Size = room1.width * room1.height;
        int room2Size = room2.width * room2.height;

        if (room1Size == room2Size)
        {
            return 0;
        }
        else if (room1Size < room2Size)
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }
}

