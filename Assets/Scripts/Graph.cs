using System.Collections.Generic;
using UnityEngine;

public class Graph<T>
{
    private Dictionary<T, List<T>> adjacencyList;

    public Graph()
    {
        adjacencyList = new Dictionary<T, List<T>>();
    }

    public void Clear()
    {
        adjacencyList.Clear();
    }

    public void RemoveNode(T node)
    {
        if (adjacencyList.ContainsKey(node))
        {
            adjacencyList.Remove(node);
        }

        foreach (var key in adjacencyList.Keys)
        {
            adjacencyList[key].Remove(node);
        }
    }

    public List<T> GetNodes()
    {
        return new List<T>(adjacencyList.Keys);
    }

    public void AddNode(T node)
    {
        if (!adjacencyList.ContainsKey(node))
        {
            adjacencyList[node] = new List<T>();
        }
    }

    public void RemoveEdge(T fromNode, T toNode)
    {
        if (adjacencyList.ContainsKey(fromNode))
        {
            adjacencyList[fromNode].Remove(toNode);
        }
        if (adjacencyList.ContainsKey(toNode))
        {
            adjacencyList[toNode].Remove(fromNode);
        }
    }

    public void AddEdge(T fromNode, T toNode)
    {
        if (!adjacencyList.ContainsKey(fromNode))
        {
            AddNode(fromNode);
        }
        if (!adjacencyList.ContainsKey(toNode))
        {
            AddNode(toNode);
        }

        adjacencyList[fromNode].Add(toNode);
        adjacencyList[toNode].Add(fromNode);
    }

    public List<T> GetNeighbors(T node)
    {
        return new List<T>(adjacencyList[node]);
    }

    public int GetNodeCount()
    {
        return adjacencyList.Count;
    }

    public void PrintGraph()
    {
        foreach (var node in adjacencyList)
        {
            Debug.Log($"{node.Key}: {string.Join(", ", node.Value)}");
        }
    }

    // Breadth-First Search (BFS)
    public void BFS(T startNode)
    {
        // Debug.Log("TODO: Print every node in the graph using breadth first order starting from startNode");

        Queue<T> toDo = new();
        List<T> done = new();

        toDo.Enqueue(startNode);
        done.Add(startNode);

        while (toDo.Count > 0)
        {
            T node = toDo.Dequeue();

            Debug.Log(node);

            foreach (T connection in adjacencyList[node])
            {
                if (!done.Contains(connection))
                {
                    toDo.Enqueue(connection);
                    done.Add(connection);
                }
            }
        }
    }

    // Depth-First Search (DFS)
    public void DFS(T startNode)
    {
        // Debug.Log("TODO: Print every node in the graph using depth first order starting from startNode");

        Stack<T> toDo = new();
        List<T> done = new();

        toDo.Push(startNode);
        done.Add(startNode);

        while (toDo.Count > 0)
        {
            T node = toDo.Pop();

            Debug.Log(node);

            foreach (T connection in adjacencyList[node])
            {
                if (!done.Contains(connection))
                {
                    toDo.Push(connection);
                    done.Add(connection);
                }
            }
        }
    }

}