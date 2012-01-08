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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Aga.Controls.Tree;

namespace OpenHardwareMonitor.GUI {
  public class TreeModel : ITreeModel {

    private Node root;
    private bool forceVisible = false;

    public TreeModel() {
      root = new Node();
      root.Model = this;
    }

    public TreePath GetPath(Node node) {
      if (node == root)
        return TreePath.Empty;
      else {
        Stack<object> stack = new Stack<object>();
        while (node != root) {
          stack.Push(node);
          node = node.Parent;
        }
        return new TreePath(stack.ToArray());
      }
    }

    public Collection<Node> Nodes {
      get { return root.Nodes; }
    }

    private Node GetNode(TreePath treePath) {
      Node parent = root;
      foreach (object obj in treePath.FullPath) {
        Node node = obj as Node;
        if (node == null || node.Parent != parent)
          return null;
        parent = node;
      }
      return parent;
    }

    public IEnumerable GetChildren(TreePath treePath) {
      Node node = GetNode(treePath);
      if (node != null) {
        foreach (Node n in node.Nodes)
          if (forceVisible || n.IsVisible)
            yield return n;
      } else {
        yield break;
      }
    }

    public bool IsLeaf(TreePath treePath) {
      return false;
    }

    public bool ForceVisible {
      get {
        return forceVisible;
      }
      set {
        if (value != forceVisible) {
          forceVisible = value;
          OnStructureChanged(root);
        }
      }
    }

    #pragma warning disable 67
    public event EventHandler<TreeModelEventArgs> NodesChanged;
    public event EventHandler<TreePathEventArgs> StructureChanged;
    public event EventHandler<TreeModelEventArgs> NodesInserted;
    public event EventHandler<TreeModelEventArgs> NodesRemoved;
    #pragma warning restore 67

    public void OnNodeChanged(Node parent, int index, Node node) {
      if (NodesChanged != null && parent != null) {
        TreePath path = GetPath(parent);
        if (path != null) 
          NodesChanged(this, new TreeModelEventArgs(
            path, new int[] { index }, new object[] { node }));
      }
    }

    public void OnStructureChanged(Node node) {
      if (StructureChanged != null)
        StructureChanged(this,
          new TreeModelEventArgs(GetPath(node), new object[0]));
    }

    public void OnNodeInserted(Node parent, int index, Node node) {
      if (NodesInserted != null) {
        TreeModelEventArgs args = new TreeModelEventArgs(GetPath(parent),
          new int[] { index }, new object[] { node });
        NodesInserted(this, args);
      }

    }

    public void OnNodeRemoved(Node parent, int index, Node node) {
      if (NodesRemoved != null) {
        TreeModelEventArgs args = new TreeModelEventArgs(GetPath(parent), 
          new int[] { index }, new object[] { node });
        NodesRemoved(this, args);
      }
    }

  }
}
