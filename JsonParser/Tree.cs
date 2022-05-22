using System;
using System.Collections.Generic;

namespace JsonParser
{
    [Serializable]
    public class TreeNode<T>
    {
        private T _data;
        private List<TreeNode<T>> _children;
        private int depth;
        private TreeNode<T> _parent = null;

        public TreeNode(T data)
        {
            _data = data;

            _children = new List<TreeNode<T>>();
        }

        public bool IsRoot { get { return _parent == null; } }

        public void AddChildNode(TreeNode<T> parent, T child_data)
        {
            TreeNode<T> child = new TreeNode<T>(child_data);
            child.SetParent(parent);
            parent.AddRawChildNode(child);
        }

        public void AddChildNode(TreeNode<T> parent, TreeNode<T> child_node)
        {
            child_node.SetParent(parent);
            parent.AddRawChildNode(child_node);
        }

        public void AddRawChildNode(TreeNode<T> child_node)
        {
            _children.Add(child_node);
        }

        public void AddRawChildrenNodes(params TreeNode<T>[] nodes)
        {
            _children.AddRange(nodes);
        }

        public void AddChildren(TreeNode<T> parent, params TreeNode<T>[] nodes)
        {
            foreach (var node in nodes)
            {
                node.SetParent(parent);
            }

            parent.AddRawChildrenNodes(nodes);
        }

        public void SetDepth(int depth)
        {
            this.depth = depth;
        }

        public int GetDepth()
        {
            return this.depth;
        }

        public T GetData()
        {
            return _data;
        }

        public void SetData(T new_data)
        {
            _data = new_data;
        }

        public TreeNode<T> GetParent()
        {
            return _parent;
        }

        public void SetParent(TreeNode<T> node)
        {
            this._parent = node;
            SetDepth(this._parent.GetDepth() + 1);
        }

        public List<TreeNode<T>> GetChildren()
        {
            return _children;
        }

        public void Visit(TreeNode<T> root, Action<TreeNode<T>> func)
        {
            func(root);
            foreach (TreeNode<T> child in root.GetChildren())
            {
                Visit(child, func);
            }
        }

        public int Count //Children count
        {
            get { return _children.Count; }
        }

        public int CountAll(TreeNode<T> tree) //Entire tree count
        {
            int count = tree.Count;

            if (tree.Count <= 0)
                return 0;
            else
            {
                foreach (TreeNode<T> child in tree.GetChildren())
                {
                    count += CountAll(child);
                }

            }
            return count;
        }
    }
}
