using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class NavigationGraph : MonoBehaviour
{
    private static NavigationGraph instance;

    [SerializeField] private UnityEvent onSearch;

    private Graph<RectInt> adjacencyList;
    private List<RectInt> done = new();

    private enum SearchType { BFS, DFS }
    [SerializeField] private SearchType searchType = SearchType.BFS;
    [SerializeField] private bool generateInstantly = false;
    [SerializeField] private float NavigationDelay = 0.01f;
    private Queue<RectInt> toDo;
    private bool searched = false;
    private bool connected = false;

    #region Public getters

    public static NavigationGraph Instance { get { return instance; } }
    public bool Searched { get { return searched; } }
    public bool Connected { get { return connected; } }

    #endregion

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        foreach (RectInt node in done)
        {
            foreach (RectInt connection in adjacencyList.GetNeighbors(node))
                Debug.DrawLine(new Vector3(node.x + node.width / 2, 0, node.y + node.height / 2), new Vector3(connection.x + connection.width / 2, 0, connection.y + connection.height / 2), Color.red, 0f);
        }
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void StartSearch()
    {
        StartCoroutine(Search());
    }

    /// <summary>
    /// Checks if the entire dungeon is connected by DFS or BFS (based on "searchType")
    /// </summary>
    private IEnumerator Search()
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

        if (done.Count == adjacencyList.GetNodeCount())
        {
            Debug.Log("The entire dungeon is connected");
            connected = true;
        }
        else
        {
            Debug.Log("Some rooms are not connected");
            connected = false;
        }

        searched = true;

        if (!MazeSpliter.Instance.Removing)
            onSearch.Invoke();
    }

    private IEnumerator BFS()
    {
        if (toDo.Count == 0)
            yield break;

        if (!generateInstantly)
            yield return new WaitForSeconds(NavigationDelay);

        RectInt node = toDo.Dequeue();

        foreach (RectInt connection in adjacencyList.GetNeighbors(node))
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

        foreach (RectInt connection in adjacencyList.GetNeighbors(node))
        {
            if (!done.Contains(connection))
            {
                if (!generateInstantly)
                    yield return new WaitForSeconds(NavigationDelay);
                yield return StartCoroutine(DFS(connection));
            }
        }
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void Reset()
    {
        if (!MazeSpliter.Instance.RandomizeSeed)
            Random.InitState(MazeSpliter.Instance.Seed);

        done = new();
        adjacencyList = DoorGenerator.Instance.AdjacencyList;
        searched = false;
        connected = false;
    }

    private RectInt GetRandomNode()
    {
        List<RectInt> list = new();
        foreach (RectInt node in adjacencyList.GetNodes())
        {
            list.Add(node);
        }
        return list[Random.Range(0, list.Count)];
    }
}

