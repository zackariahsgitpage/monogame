using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Transactions;
using System.Xml;
Graph<char> graph = new Graph<char>();
graph.Add('A');
graph.Add('B');
graph.Add('E');
graph.Add('F');
graph.Add('G');
graph.Add('H');
graph.Add('K');
graph.Add('M');
graph.AddEdge('A', 'B');graph.AddEdge('B', 'A');
graph.AddEdge('A', 'F');graph.AddEdge('F', 'A');
graph.AddEdge('A', 'H');graph.AddEdge('H', 'A');
graph.AddEdge('B', 'E');graph.AddEdge('E', 'B');
graph.AddEdge('B', 'G');graph.AddEdge('G', 'B');
graph.AddEdge('B', 'H');graph.AddEdge('H', 'B');
graph.AddEdge('F', 'E');graph.AddEdge('E', 'F');
graph.AddEdge('G', 'H');graph.AddEdge('H', 'G');
graph.AddEdge('G', 'M');graph.AddEdge('M', 'G');
graph.AddEdge('H', 'K');graph.AddEdge('K', 'H');
graph.AddEdge('K', 'M');graph.AddEdge('M', 'K');
List<char> visited = new List<char>();
//graph.DFS('H', ref visited);
graph.BFS('A', ref visited);

Console.Write(graph.OutputGraph());
for (int i = 0; i < visited.Count(); i++)
{
    Console.Write(visited[i]);
}
public class Graph<T>
{
    private Dictionary<T,GNode> _nodes;
    public Graph()
    {
       _nodes = new Dictionary<T,GNode>(); 
    }
    public class GNode
    {
        public T _data;
        public Dictionary<GNode, int> _edges = new Dictionary<GNode, int>();
        public GNode(T data)
        {
        _data = data;
        _edges = new Dictionary<GNode, int>();
        }
    }
 public void DFS(T current, ref List<T> visited)
 {
    visited.Add(current);
       foreach (var edge in _nodes[current]._edges)
       {
            //    if (!visited.Contains(edge._data))
                {
              //       DFS(edge._data,ref visited);
                }       
        }
    }
public void BFS(T start, ref List<T> visited)
    {
        List<T> Queue = new List<T>();
        Queue.Add(start);
        while (Queue.Count !=0)
        {
            foreach (var edge in _nodes[Queue[0]]._edges)
            {
 //               if (!Queue.Contains(edge._data) && !visited.Contains(edge._data))
                {
//                    Queue.Add(edge._data);
                }
            }
            visited.Add(Queue[0]);
            Queue.RemoveAt(0);
        }
    }
    public Dictionary<T, int> ShortestPath(T start)
    {
        Dictionary<T,int> Visited = new Dictionary<T, int>();
        PriorityQueue<T> priorityQueue = new PriorityQueue<T>();
        priorityQueue.Add(start,0);
        foreach (var node in _nodes)
        {
        priorityQueue.Add(node.Key, int.MaxValue);
        }
        while (priorityQueue.Size() != 0)
        {
            T current;
            int distance;
            (current,distance) = priorityQueue.Remove();
            Visited.Add(current, distance);
            foreach (var edge in _nodes[current]._edges)
            {
              if (!Visited.ContainsKey(edge.Key._data))
                {
                    if (distance + edge.Value <= edge.Value)
                    {
                    priorityQueue.ChangePriority(edge.Key._data,edge.Value);
                    }
                }
            }
        }
        return Visited;
    }
    public void OutputShortestPath(Dictionary<T,int> visited)
    {
        
    }
    public void Add(T nodeName)
    {
        _nodes.Add(nodeName, new GNode(nodeName));
    }
    public void AddEdge(T source, T destination, int weight)
    {
        _nodes[source]._edges.Add(_nodes[destination], weight);
    }
    public StringBuilder OutputGraph()
    {
        StringBuilder emptystring = new StringBuilder();
        foreach (var KVpair in _nodes)
        {
            emptystring.Append($"{KVpair.Value._data}: ");
            var edges = KVpair.Value._edges;
            for (int i = 0; i < edges.Count; i++)
            {
            emptystring.Append($"{edges[i]._data}, ");
            }
            emptystring.AppendLine(" ");
        }
        return emptystring;
    }
   // public List<GNode> GetEdges(GNode currentNode)
   // {
//        return currentNode._edges;
   // }
    class PriorityQueue<T> : DynamicQueue<T>
{
    Node<T> _front;
    Node<T> _rear;
    int _size;
    public PriorityQueue() : base()
    { }
    public override void Add(T data, int priority = 0)
    {

        Node<T> newNode = new Node<T>(data, priority);
        if (_size == 0) //insert if empty
        {
            _rear = newNode;
            _front = newNode;
        }
        else
        {
            Node<T> _tempPointer = _front;
            while (_tempPointer._pointer != newNode)
            {
                if (_tempPointer._pointer != null && _tempPointer._pointer._priority > priority) //insert in middle
                {
                    newNode._pointer = _tempPointer._pointer;
                    _tempPointer._pointer = newNode;
                }
                else if (priority < _front._priority) //insert at beginning
                {
                    newNode._pointer = _front;
                    _front = newNode;
                }
                else if (_tempPointer == _rear) //insert at end
                {
                    _tempPointer._pointer = newNode;
                    _rear = newNode;
                }
                else //check next node
                {
                    _tempPointer = _tempPointer._pointer;
                }
            }
        }
        _size++;
    }
  
    public void ChangePriority(T selectedNode, int inputPriority)
        {
            Node<T> Current = _front;
            Node<T> Previous = null;
            bool Found = false;
            if (_size == 1 && Equals(selectedNode, _front._data))
            {
                _front = null;
                _rear = null;
                _size--;
                Add(selectedNode, inputPriority);
            }
            else
            {
            while (!Found)
            {
                if (Equals(Current._data, selectedNode))
                {
                    Found = true;
                    Previous._pointer = Current._pointer;
                    Add(selectedNode, inputPriority);
                }
                else
                {
                    Previous = Current;
                    Current = Current._pointer;
                }
            }
            }
        }
}

class DynamicQueue<T>
{
    Node<T> _front;
    Node<T> _rear;
    int _size;
    protected class Node<T>
    {
        public T _data;
        public Node<T> _pointer;
        public int _priority;
        public Node(T data, int priority = 0)
        {
            _data = data;
            _pointer = null;
            _priority = priority;
        }
    }
    public DynamicQueue()
    {
        _front = null;
        _rear = null;
        _size = 0;
    }
    public bool IsEmpty
    {
        get
        {
            return _size == 0;
        }
    }
    public int Size()
    {
        return _size;
    }
    public virtual void Add(T data, int priority = 0)
    {
        Node<T> newNode = new Node<T>(data);
        if (_size == 0)
            _front = newNode;
        else
        {     
        _rear._pointer = newNode;
        }
        _rear = newNode;
        _size++;
    }
    public T Peek()
    {
        if (_size == 0)
            throw new Exception("Queue is empty");
        return _front._data;
    }
    public virtual (T,int) Remove()
    {
        if (_size == 0)
            throw new Exception("Queue is empty");
        _size--;
        T tempData = _front._data;
        int tempPriority = _front._priority;
        _front = _front._pointer;
        return (tempData,tempPriority);
    }
    public bool Contains()
        {
            Node<T> tempPointer = _front;
            bool contains = false;
            while(tempPointer._pointer != null)
            {
                if ()
            }
        }
}
}