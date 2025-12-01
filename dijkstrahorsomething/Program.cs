using System.ComponentModel.Design.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
BTree bt = new BTree();
bt.Add("Rod", ref bt._root);
bt.Add("Jane", ref bt._root);
bt.Add("Freddy", ref bt._root);
bt.Add("Bungle", ref bt._root);
bt.Add("Zippy",ref bt._root);
bt.Add("George",ref bt._root);
bt.Add("Jeffrey", ref bt._root);
bt.InOrderTraverse(ref bt._root);

class BTree
{
        int _size;
       public Node _root;
public class Node
    {

        public string _data;
        public Node _leftPointer;
        public Node _rightPointer;
        public Node(string data)
        {
             _data = data;
            _leftPointer = null;
            _rightPointer = null;
        }
    }
    public BTree()
    {
        _root = null;
    }
    public void Add(string data, ref Node currentNode)
    {
        Node newNode = new Node(data);
        if (currentNode == null)
        {
            _root = newNode;
        }
        else if(string.Compare(data, currentNode._data) == -1)    
            {
            Add(data, ref currentNode._leftPointer);
            }
            else
            {
            Add(data, ref currentNode._rightPointer);
            }
    
    }
   // public void Add(string data)
   // {
   //     Add(data, _root);
   // }
    
    public void InOrderTraverse(ref Node current)
    {
        if (current._leftPointer !=null);
        InOrderTraverse(ref current._leftPointer);
        Console.WriteLine(current._data);
        if (current._rightPointer != null);
        InOrderTraverse(ref current._rightPointer);
    }
}
