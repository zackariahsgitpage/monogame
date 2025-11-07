
using System.ComponentModel;
using System.Configuration.Assemblies;
DynamicQueue<int> dynafo = new DynamicQueue<int>();
dynafo.Add(10);
Console.WriteLine(dynafo.Remove());


class DynamicQueue<T>
{
    Node<T> _front;
    Node<T> _rear;
    int _size;
    private class Node<T>
    {
        public T _data;
        public Node<T> _pointer;
        public Node(T data)
        {
            _data = data;
            _pointer = null;
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
    public void Add(T data)
    {
        Node<T> newNode = new Node<T>(data);
        if (_size == 0)
            _front = newNode;
        else
            _rear._pointer = newNode;
        _rear = newNode; 
        _size++;
    }
    public T Peek()
    {
        if (_size == 0)
            throw new Exception("Queue is empty");
        return _front._data;
    }
    public T Remove()
    {
        if (_size == 0)
            throw new Exception("Queue is empty");
        _size--;
        T temp = _front._data;
        _front = _front._pointer;
        return temp;
    }
}

//Stack lifo = new Stack();
//CircularQueue<int> fifo = new CircularQueue<int>();
//fifo.Add(10);
//fifo.Add(20);
//fifo.Add(30);
//fifo.Add(40);
//Console.WriteLine(fifo.IsFull());
//Console.WriteLine(fifo.Remove());
//Console.WriteLine(fifo.Remove());
//Console.WriteLine(fifo.Remove());
//fifo.Add(50);
//fifo.Add(60);
//fifo.Add(70);
//Console.WriteLine(fifo.Peek());
//lifo.Push(10);
//lifo.Push(20);
//lifo.Push(30);
//Console.WriteLine(lifo.Pop());
//Console.WriteLine(lifo.Pop());
//Console.WriteLine(lifo.Pop());
class Stack
{
    private int[] _array;
    private int _stackpointer;
    public Stack()
    {
        _array = new int[4];
        _stackpointer = -1;
    }
    public void Push(int value)
    {
        _array[++_stackpointer] = value;
    }
    public int Pop()
    {
        return _array[_stackpointer--];
    }
}
 public interface IQueue<I>
    {
        bool IsFull();
        bool IsEmpty();
        void Add(I item);
        I Remove();
        I Peek();
    }
public class Queue<T> : IQueue<T>
{
    private T[] _array;
    private int _front;
    private int _rear;

    public Queue()
    {
        _array = new T[4];
        _front = 0;
        _rear = -1;
    }
    public int Size()
    {
        return _rear - _front + 1;
    }
    public bool IsFull()
    {
        return _rear >= _array.Length - 1;
    }
    public bool IsEmpty()
    {
        return _rear == -1;
    }
    public void Add(T item)
    {
        if (IsFull())
            throw new InvalidOperationException("Queue is full");
        else
            _array[++_rear] = item;
    }
    public T Remove()
    {
        if (IsEmpty())
            throw new InvalidOperationException("Queue is empty");
        else
        {
            T temp = _array[_front];
            for (int i = _front; i < _rear; i++)
            {
                _array[i] = _array[i + 1];
            }
            _rear--;
            return temp;
        }
    }
    public T Peek()
    {
        if (IsEmpty())
            throw new InvalidOperationException("Queue is empty");
        else
            return _array[_front];
    }

}
public class CircularQueue<T> : IQueue<T>
{
    private const int _arraySize = 4;
    private T[] _array;
    private int _front;
    private int _rear;

    public CircularQueue()
    {
        _array = new T[_arraySize];
        _front = 0;
        _rear = -1;
    }
    public int Size()
    {
        return _rear - _front + 1;
    }
    public bool IsFull()
    {
        return _rear >= _arraySize;
    }
    public bool IsEmpty()
    {
        return _rear == -1;
    }
    public void Add(T item)
    {
        if (IsFull())
            throw new InvalidOperationException("Queue is full");
        else
            _array[++_rear % (_arraySize)] = item;
    }
    public T Remove()
    {
        T temp = _array[_front];
        if (IsEmpty())
            throw new InvalidOperationException("Queue is empty");
        else
        {
            _front = (_front % _arraySize) +1;
        }
        return temp;
    }
    public T Peek()
    {
        if (IsEmpty())
            throw new InvalidOperationException("Queue is empty");
        else
            return _array[_front];
    }

}