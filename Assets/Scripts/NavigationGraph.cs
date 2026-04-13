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

    [SerializeField] private SearchType searchType = SearchType.BFS;
    private bool searching = false;
    public bool autoGenerate;
    [SerializeField] private bool generateInstantly = false;
    [SerializeField] private float NavigationDelay = 0.01f;

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
        if (searching)
            yield break;

        Reset();
        RectInt startNode = GetRandomNode();
        searching = true;

        switch (searchType)
        {
            case SearchType.BFS:
                StartCoroutine(BFS(startNode));
                break;
            case SearchType.DFS:
                StartCoroutine(DFS(startNode));
                break;
        }

        yield return new WaitWhile(() => searching);

        if (done.Count == adjacencyList.Keys.Count)
        {
            Debug.Log("The entire dungeon is connected");
        }
        else
        {
            Debug.Log("Some rooms are not connected");
        }
    }

    private IEnumerator BFS(RectInt startNode)
    {
        searching = true;
        Queue<RectInt> toDo = new();

        toDo.Enqueue(startNode);
        done.Add(startNode);

        while (toDo.Count > 0)
        {
            RectInt node = toDo.Dequeue();

            foreach (RectInt connection in adjacencyList[node])
            {
                if (!done.Contains(connection))
                {
                    toDo.Enqueue(connection);
                    done.Add(connection);
                    if (!generateInstantly)
                    {
                        yield return new WaitForSeconds(NavigationDelay);
                    }
                }
            }
        }

        searching = false;
    }

    private IEnumerator DFS(RectInt startNode)
    {
        Stack<RectInt> toDo = new();

        toDo.Push(startNode);
        done.Add(startNode);

        while (toDo.Count > 0)
        {
            RectInt node = toDo.Pop();

            Debug.Log(node);

            foreach (RectInt connection in adjacencyList[node])
            {
                if (!done.Contains(connection))
                {
                    toDo.Push(connection);
                    done.Add(connection);
                    if (!generateInstantly)
                    {
                        yield return new WaitForSeconds(NavigationDelay);
                    }
                }
            }
        }

        searching = false;
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void Reset()
    {
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

    private enum SearchType
    {
        BFS,
        DFS
    }
}

