using System.ComponentModel;
using System.Text;
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
        public List<GNode> _edges = new List<GNode>();
        public GNode(T data)
        {
        _data = data;
        _edges = new List<GNode>();
        }
    }
 public void DFS(T current, ref List<T> visited)
 {
    visited.Add(current);
       foreach (var edge in _nodes[current]._edges)
       {
                if (!visited.Contains(edge._data))
                {
                     DFS(edge._data,ref visited);
                }       
        }
    }
public void BFS(T start, ref List<T> visited)
    {
        List<T> Queue = new List<T>();
        Queue.Add(start);
        for (int i = 0; i < _nodes.Count(); i++)
        {
            foreach (var edge in _nodes[Queue[i]]._edges)
            {
                if (!Queue.Contains(edge._data) && !visited.Contains(edge._data))
                {
                    Queue.Add(edge._data);
                }
            }
            visited.Add(Queue[i]);
        }
    }
    public void Add(T nodeName)
    {
        _nodes.Add(nodeName, new GNode(nodeName));
    }
    public void AddEdge(T source, T destination)
    {
         _nodes[source]._edges.Add(_nodes[destination]);
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
    public List<GNode> GetEdges(GNode currentNode)
    {
        return currentNode._edges;
    }
}