/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2010 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/


using System;
using System.Collections.ObjectModel;
using System.Drawing;

namespace OpenHardwareMonitor.GUI
{
    public class Node
    {
        public delegate void NodeEventHandler(Node node);

        private Image image;
        private readonly NodeCollection nodes;
        private Node parent;

        private string text;

        private bool visible;

        public Node() : this(string.Empty)
        {
        }

        public Node(string text)
        {
            this.text = text;
            nodes = new NodeCollection(this);
            visible = true;
        }

        public TreeModel Model { get; set; }

        public Node Parent
        {
            get { return parent; }
            set
            {
                if (value != parent)
                {
                    if (parent != null)
                        parent.nodes.Remove(this);
                    if (value != null)
                        value.nodes.Add(this);
                }
            }
        }

        public Collection<Node> Nodes
        {
            get { return nodes; }
        }

        public virtual string Text
        {
            get { return text; }
            set
            {
                if (text != value)
                {
                    text = value;
                }
            }
        }

        public Image Image
        {
            get { return image; }
            set
            {
                if (image != value)
                {
                    image = value;
                }
            }
        }

        public virtual bool IsVisible
        {
            get { return visible; }
            set
            {
                if (value != visible)
                {
                    visible = value;
                    var model = RootTreeModel();
                    if (model != null && parent != null)
                    {
                        var index = 0;
                        for (var i = 0; i < parent.nodes.Count; i++)
                        {
                            var node = parent.nodes[i];
                            if (node == this)
                                break;
                            if (node.IsVisible || model.ForceVisible)
                                index++;
                        }
                        if (model.ForceVisible)
                        {
                            model.OnNodeChanged(parent, index, this);
                        }
                        else
                        {
                            if (value)
                                model.OnNodeInserted(parent, index, this);
                            else
                                model.OnNodeRemoved(parent, index, this);
                        }
                    }
                    if (IsVisibleChanged != null)
                        IsVisibleChanged(this);
                }
            }
        }

        private TreeModel RootTreeModel()
        {
            var node = this;
            while (node != null)
            {
                if (node.Model != null)
                    return node.Model;
                node = node.parent;
            }
            return null;
        }

        public event NodeEventHandler IsVisibleChanged;
        public event NodeEventHandler NodeAdded;
        public event NodeEventHandler NodeRemoved;

        private class NodeCollection : Collection<Node>
        {
            private readonly Node owner;

            public NodeCollection(Node owner)
            {
                this.owner = owner;
            }

            protected override void ClearItems()
            {
                while (Count != 0)
                    RemoveAt(Count - 1);
            }

            protected override void InsertItem(int index, Node item)
            {
                if (item == null)
                    throw new ArgumentNullException("item");

                if (item.parent != owner)
                {
                    if (item.parent != null)
                        item.parent.nodes.Remove(item);
                    item.parent = owner;
                    base.InsertItem(index, item);

                    var model = owner.RootTreeModel();
                    if (model != null)
                        model.OnStructureChanged(owner);
                    if (owner.NodeAdded != null)
                        owner.NodeAdded(item);
                }
            }

            protected override void RemoveItem(int index)
            {
                var item = this[index];
                item.parent = null;
                base.RemoveItem(index);

                var model = owner.RootTreeModel();
                if (model != null)
                    model.OnStructureChanged(owner);
                if (owner.NodeRemoved != null)
                    owner.NodeRemoved(item);
            }

            protected override void SetItem(int index, Node item)
            {
                if (item == null)
                    throw new ArgumentNullException("item");

                RemoveAt(index);
                InsertItem(index, item);
            }
        }
    }
}