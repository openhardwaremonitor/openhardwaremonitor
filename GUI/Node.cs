/*
  
  Version: MPL 1.1/GPL 2.0/LGPL 2.1

  The contents of this file are subject to the Mozilla Public License Version
  1.1 (the "License"); you may not use this file except in compliance with
  the License. You may obtain a copy of the License at
 
  http://www.mozilla.org/MPL/

  Software distributed under the License is distributed on an "AS IS" basis,
  WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
  for the specific language governing rights and limitations under the License.

  The Original Code is the Open Hardware Monitor code.

  The Initial Developer of the Original Code is 
  Michael Möller <m.moeller@gmx.ch>.
  Portions created by the Initial Developer are Copyright (C) 2009-2010
  the Initial Developer. All Rights Reserved.

  Contributor(s):

  Alternatively, the contents of this file may be used under the terms of
  either the GNU General Public License Version 2 or later (the "GPL"), or
  the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
  in which case the provisions of the GPL or the LGPL are applicable instead
  of those above. If you wish to allow use of your version of this file only
  under the terms of either the GPL or the LGPL, and not to allow others to
  use your version of this file under the terms of the MPL, indicate your
  decision by deleting the provisions above and replace them with the notice
  and other provisions required by the GPL or the LGPL. If you do not delete
  the provisions above, a recipient may use your version of this file under
  the terms of any one of the MPL, the GPL or the LGPL.
 
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using Aga.Controls.Tree;

namespace OpenHardwareMonitor.GUI {
  public class Node {

    private TreeModel treeModel;
    private Node parent;
    private NodeCollection nodes;

    private string text;
    private Image image;
    private bool visible;

    private TreeModel RootTreeModel() {
      Node node = this;
      while (node != null) {
        if (node.Model != null)
          return node.Model;
        node = node.parent;
      }
      return null;
    }

    public Node() : this(string.Empty) { }

    public Node(string text) {
      this.text = text;
      this.nodes = new NodeCollection(this);
      this.visible = true;
    }

    public TreeModel Model {
      get { return treeModel; }
      set { treeModel = value; }
    }

    public Node Parent {
      get { return parent; }
      set {
        if (value != parent) {
          if (parent != null)
            parent.nodes.Remove(this);
          if (value != null)
            value.nodes.Add(this);
        }
      }
    }

    public Collection<Node> Nodes {
      get { return nodes; }
    }

    public virtual string Text {
      get { return text; }
      set {
        if (text != value) {
          text = value;
        }
      }
    }

    public Image Image {
      get { return image; }
      set {
        if (image != value) {
          image = value;
        }
      }
    }

    public virtual bool IsVisible {
      get { return visible; }
      set {
        if (value != visible) {
          visible = value;          
          TreeModel model = RootTreeModel();
          if (model != null && parent != null) {
            int index = 0;
            for (int i = 0; i < parent.nodes.Count; i++) {
              Node node = parent.nodes[i];
              if (node == this)
                break;
              if (node.IsVisible || model.ForceVisible)
                index++;
            }
            if (model.ForceVisible) {
                model.OnNodeChanged(parent, index, this);
            } else {              
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

    public delegate void NodeEventHandler(Node node);

    public event NodeEventHandler IsVisibleChanged;
    public event NodeEventHandler NodeAdded;
    public event NodeEventHandler NodeRemoved;

    private class NodeCollection : Collection<Node> {
      private Node owner;

      public NodeCollection(Node owner) {
        this.owner = owner;
      }

      protected override void ClearItems() {
        while (this.Count != 0)
          this.RemoveAt(this.Count - 1);
      }

      protected override void InsertItem(int index, Node item) {
        if (item == null)
          throw new ArgumentNullException("item");

        if (item.parent != owner) {
          if (item.parent != null)
            item.parent.nodes.Remove(item);
          item.parent = owner;
          base.InsertItem(index, item);

          TreeModel model = owner.RootTreeModel();
          if (model != null)
            model.OnStructureChanged(owner);
          if (owner.NodeAdded != null)
            owner.NodeAdded(item);
        }
      }

      protected override void RemoveItem(int index) {
        Node item = this[index];
        item.parent = null;
        base.RemoveItem(index);

        TreeModel model = owner.RootTreeModel();
        if (model != null) 
          model.OnStructureChanged(owner);
        if (owner.NodeRemoved != null)
          owner.NodeRemoved(item);
      }

      protected override void SetItem(int index, Node item) {
        if (item == null)
          throw new ArgumentNullException("item");

        RemoveAt(index);
        InsertItem(index, item);
      }
    }
  }
}
