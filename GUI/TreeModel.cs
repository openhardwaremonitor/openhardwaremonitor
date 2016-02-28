/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2010 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Aga.Controls.Tree;

namespace OpenHardwareMonitor.GUI
{
    public class TreeModel : ITreeModel
    {
        private bool forceVisible;

        private readonly Node root;

        public TreeModel()
        {
            root = new Node();
            root.Model = this;
        }

        public Collection<Node> Nodes
        {
            get { return root.Nodes; }
        }

        public bool ForceVisible
        {
            get { return forceVisible; }
            set
            {
                if (value != forceVisible)
                {
                    forceVisible = value;
                    OnStructureChanged(root);
                }
            }
        }

        public IEnumerable GetChildren(TreePath treePath)
        {
            var node = GetNode(treePath);
            if (node != null)
            {
                foreach (var n in node.Nodes)
                    if (forceVisible || n.IsVisible)
                        yield return n;
            }
            else
            {
            }
        }

        public bool IsLeaf(TreePath treePath)
        {
            return false;
        }

        public TreePath GetPath(Node node)
        {
            if (node == root)
                return TreePath.Empty;
            var stack = new Stack<object>();
            while (node != root)
            {
                stack.Push(node);
                node = node.Parent;
            }
            return new TreePath(stack.ToArray());
        }

        private Node GetNode(TreePath treePath)
        {
            var parent = root;
            foreach (var obj in treePath.FullPath)
            {
                var node = obj as Node;
                if (node == null || node.Parent != parent)
                    return null;
                parent = node;
            }
            return parent;
        }

        public void OnNodeChanged(Node parent, int index, Node node)
        {
            if (NodesChanged != null && parent != null)
            {
                var path = GetPath(parent);
                if (path != null)
                    NodesChanged(this, new TreeModelEventArgs(
                        path, new[] {index}, new object[] {node}));
            }
        }

        public void OnStructureChanged(Node node)
        {
            if (StructureChanged != null)
                StructureChanged(this,
                    new TreeModelEventArgs(GetPath(node), new object[0]));
        }

        public void OnNodeInserted(Node parent, int index, Node node)
        {
            if (NodesInserted != null)
            {
                var args = new TreeModelEventArgs(GetPath(parent),
                    new[] {index}, new object[] {node});
                NodesInserted(this, args);
            }
        }

        public void OnNodeRemoved(Node parent, int index, Node node)
        {
            if (NodesRemoved != null)
            {
                var args = new TreeModelEventArgs(GetPath(parent),
                    new[] {index}, new object[] {node});
                NodesRemoved(this, args);
            }
        }

#pragma warning disable 67
        public event EventHandler<TreeModelEventArgs> NodesChanged;
        public event EventHandler<TreePathEventArgs> StructureChanged;
        public event EventHandler<TreeModelEventArgs> NodesInserted;
        public event EventHandler<TreeModelEventArgs> NodesRemoved;
#pragma warning restore 67
    }
}