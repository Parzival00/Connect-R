using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Tree
{
    public class TreeNode<T>
    {
        private readonly T _value;
        private readonly List<TreeNode<T>> _children = new List<TreeNode<T>>();


        public TreeNode(T value) { _value = value; }

        public TreeNode<T> this[int i]
        {
            get { return _children[i]; }
        }

        public TreeNode<T> Parent { get; private set; }

        public T Value { get { return _value; } }

        public ReadOnlyCollection<TreeNode<T>> Children { get { return _children.AsReadOnly(); } }

        public TreeNode<T> AddChild(T value)
        {
            var node = new TreeNode<T>(value) { Parent = this };
            _children.Add(node);
            return node;
        }

        public TreeNode<T>[] AddChildren(params T[] values)
        {
            return values.Select(AddChild).ToArray();
        }

        public bool RemoveChild(TreeNode<T> node)
        {
            return _children.Remove(node);
        }

        public void Traverse(Action<T> action)
        {
            action(Value);
            foreach (var child in _children)
            {
                child.Traverse(action);
            }
        }

        //public IEnumerable<T> Flatten()
        //{
        //    return new[] {Value}.Concat(_children.SelectMany(x => x.Flatten()));
        //}
    }

    //class Program
    //{
    //    static void Main(string[] args)
    //    {
    //        Console.WriteLine("Tree Fun!");

    //        TreeNode<int> test = new TreeNode<int>(7);
    //        Console.WriteLine(test.Value);

    //        test.AddChild(5);
    //        Console.WriteLine(test[0].Value);

    //        test.AddChild(3);
    //        Console.WriteLine(test[1].Value);

    //        test.AddChild(1);
    //        Console.WriteLine(test[2].Value);


    //        test[0].AddChildren(15, 17, 19);

    //        Console.WriteLine("=================");

    //        foreach (TreeNode<int> node in test.Children)
    //        {
    //            Console.WriteLine(node.Value);
    //        }

    //        Console.WriteLine("=================");

    //        foreach (TreeNode<int> node in test[0].Children)
    //        {
    //            Console.WriteLine(node.Value);
    //        }
    //    }
    //}
}