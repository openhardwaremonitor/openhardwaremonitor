Public Class VBMultiColTreeUserControl
    Private _Model As TreeModel

#Region "Initialisation"
    Private Sub VBMultiColTreeUserControl_Enter(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Enter
        ' set up the combo with a list of images 
        For Each k As String In ImageList24.Images.Keys()
            cbImages.Items.Add(k)
        Next
        cbImages.SelectedIndex = 0

        ' set up the tree with the model
        _Model = New TreeModel()
        Tree.Model = _Model
    End Sub
#End Region

#Region "User Events"
    Private Sub cbImages_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbImages.SelectedIndexChanged
        Dim IconName As String = cbImages.Text
        PictureBox1.Image = ImageList24.Images(IconName)
    End Sub

    Private Sub bClearTree_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bClearTree.Click
        _Tree.BeginUpdate()
        _Model.Nodes.Clear()
        _Tree.EndUpdate()
    End Sub

    Private Sub bExpandAll_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bExpandAll.Click
        _Tree.ExpandAll()
        Tree.Select()
    End Sub

    Private Sub bAddRoot_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bAddRoot.Click
        Dim n As Node = New VBMultiColTreeNode(txtCommand.Text, txtValue.Text, txtInformation.Text, ImageList24, cbImages.Text)

        _Model.Nodes.Add(n)

        _Tree.ClearSelection()

        'This is also possible, but less effective
        'Dim node As TreeNodeAdv = _Tree.FindNodeByTag(n)

        Dim node As TreeNodeAdv = _Tree.FindNode(New TreePath(n))
        node.IsSelected = True
        _Tree.SelectedNode.ExpandAll()
        Tree.Select()
    End Sub

    Private Sub bAddParent_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bAddParent.Click
        If Tree.SelectedNode IsNot Nothing Then
            Dim Mynode As Node = Tree.SelectedNode.Tag
            Dim Parent As Node = Mynode.Parent
            Dim GrandParent As Node = Parent.Parent
            If GrandParent IsNot Nothing Then
                Dim n As Node = New VBMultiColTreeNode(txtCommand.Text, txtValue.Text, txtInformation.Text, ImageList24, cbImages.Text)
                GrandParent.Nodes.Add(n)
                Tree.SelectedNode = Tree.CurrentNode.Parent.Parent.Children(Tree.CurrentNode.Parent.Parent.Children.Count - 1)
                Tree.Select()
            End If
        End If
    End Sub

    Private Sub bAddChild_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bAddChild.Click
        If Tree.SelectedNode IsNot Nothing Then
            Dim MyNode As Node = Tree.SelectedNode.Tag
            Dim n As Node = New VBMultiColTreeNode(txtCommand.Text, txtValue.Text, txtInformation.Text, ImageList24, cbImages.Text)
            MyNode.Nodes.Add(n)
            Tree.SelectedNode = Tree.CurrentNode.Children(Tree.CurrentNode.Children.Count - 1)
            Tree.Select()
        End If
    End Sub

    Private Sub bAddSibling_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bAddSibling.Click
        If Tree.SelectedNode IsNot Nothing Then
            Dim Mynode As Node = Tree.SelectedNode.Tag
            Dim Parent As Node = Mynode.Parent
            If Parent IsNot Nothing Then
                Dim n As Node = New VBMultiColTreeNode(txtCommand.Text, txtValue.Text, txtInformation.Text, ImageList24, cbImages.Text)
                Parent.Nodes.Add(n)
                Tree.SelectedNode = Tree.CurrentNode.Parent.Children(Tree.CurrentNode.Parent.Children.Count - 1)
                Tree.Select()
            End If
        End If
    End Sub

    Private Sub bMoveLeft_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bMoveLeft.Click
        If Tree.SelectedNode IsNot Nothing Then
            Dim Mynode As Node = Tree.SelectedNode.Tag
            Dim index As Integer = -1
            Dim Parent As Node = Mynode.Parent
            If Parent IsNot Nothing Then
                Dim TreeParent As TreeNodeAdv = Tree.CurrentNode.Parent.Parent
                Dim GrandParent As Node = Parent.Parent
                Tree.BeginUpdate()
                Mynode.Parent = GrandParent
                Tree.EndUpdate()
                Tree.SelectedNode = TreeParent.Children(TreeParent.Children.Count - 1)
                Tree.Select()
            End If

        End If
    End Sub

    Private Sub bMoveUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bMoveUp.Click
        If Tree.SelectedNode IsNot Nothing Then
            Dim NodeToMove As VBMultiColTreeNode = Tree.SelectedNode.Tag
            Dim ParentNode As Node = NodeToMove.Parent
            Dim nChildren As Integer = ParentNode.Nodes.Count
            'locate the node to move
            Dim Index As Integer = ParentNode.Nodes.IndexOf(NodeToMove)
            ' if index is 0 already at the top or if nChildren =1 then nowhere to go 
            If nChildren > 1 And Index > 0 Then
                Dim TreeParent As TreeNodeAdv = Tree.CurrentNode.Parent
                ' yes it possible to move this node up
                Tree.BeginUpdate()
                Dim NewNode As VBMultiColTreeNode = NodeToMove.Clone()
                ' now move the children across
                For i As Integer = NodeToMove.Nodes.Count - 1 To 0 Step -1
                    NodeToMove.Nodes(i).Parent = NewNode
                Next
                ' now can delete the old node
                NodeToMove.Parent.Nodes.Remove(NodeToMove)
                ParentNode.Nodes.Insert(Index - 1, NewNode)
                Tree.EndUpdate()
                Tree.SelectedNode = TreeParent.Children(Index - 1)
            End If
            Tree.Select()
        End If
    End Sub

    Private Sub bMoveDown_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bMoveDown.Click
        If Tree.SelectedNode IsNot Nothing Then
            Dim NodeToMove As VBMultiColTreeNode = Tree.SelectedNode.Tag
            Dim ParentNode As Node = NodeToMove.Parent
            Dim nChildren As Integer = ParentNode.Nodes.Count

            Dim Index As Integer = ParentNode.Nodes.IndexOf(NodeToMove)
            ' if index is last child already at the bottom and the item was found
            ' if nChildren =1 then nowhere to go 
            If nChildren > 1 And Index >= 0 And Index < nChildren - 1 Then
                Dim TreeParent As TreeNodeAdv = Tree.CurrentNode.Parent
                ' yes it possible to move this node down
                Tree.BeginUpdate()
                Dim NewNode As VBMultiColTreeNode = NodeToMove.Clone()
                ' now move the children across
                For i As Integer = NodeToMove.Nodes.Count - 1 To 0 Step -1
                    NodeToMove.Nodes(i).Parent = NewNode
                Next
                ' now can delete the old node
                NodeToMove.Parent.Nodes.Remove(NodeToMove)
                ParentNode.Nodes.Insert(Index + 1, NewNode)
                Tree.EndUpdate()
                Tree.SelectedNode = TreeParent.Children(Index + 1)
            End If
            Tree.Select()
        End If

    End Sub

    Private Sub bDelete_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bDelete.Click
        If Tree.SelectedNode IsNot Nothing Then
            Dim ThisNode As Node = DirectCast(Tree.SelectedNode.Tag, Node)
            ThisNode.Parent.Nodes.Remove(ThisNode)
        End If
    End Sub

    Private Sub bExpandNode_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bExpandNode.Click
        _Tree.SelectedNode.Expand()
    End Sub

    Private Sub bCollapseNode_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bCollapseNode.Click
        _Tree.SelectedNode.Collapse()
    End Sub

    Private Sub bChangeIcon_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bChangeIcon.Click
        If _Tree.SelectedNode IsNot Nothing Then
            Dim mn As VBMultiColTreeNode = DirectCast(_Tree.CurrentNode.Tag, VBMultiColTreeNode)
            mn.nImageKey = cbImages.Text
        End If
    End Sub

    Private Sub bCollapseAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bCollapseAll.Click
        _Tree.CollapseAll()
    End Sub

    Private Sub bExpandCollapse_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bExpandCollapse.Click
        If _Tree.Root.Children.Count > 0 Then
            If _Tree.Root.Children(0).IsExpanded Then
                _Tree.CollapseAll()
            Else
                _Tree.ExpandAll()
            End If
        End If
    End Sub

    Private Sub bRefreshTree_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bRefreshTree.Click
        _Tree.FullUpdate()
    End Sub

    Private Sub bClearSelection_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bClearSelection.Click
        _Tree.ClearSelection()
    End Sub
#End Region

#Region "Form Events"
    Private Sub SplitContainer1_Resize(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SplitContainer1.Resize
        If SplitContainer1.Width > 0 Then
            Dim NewWidth As Integer = SplitContainer1.Width - GroupBox1.Width
            If NewWidth < 300 Then NewWidth = 300

            SplitContainer1.SplitterDistance = NewWidth
        End If
    End Sub
#End Region

#Region "XML Export Import"
    Private Sub bSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bSave.Click
        Dim dlg As New SaveFileDialog()

        Me.Cursor = Cursors.WaitCursor
        dlg.Title = "Open XML Document"
        dlg.Filter = "XML Files (*.xml)|*.xml"
        Dim x1 As Integer = Application.StartupPath.LastIndexOf("\")
        x1 = Application.StartupPath.Substring(0, x1 - 1).LastIndexOf("\")
        dlg.FileName = Application.StartupPath.Substring(0, x1) + "\example.xml"
        If dlg.ShowDialog() = DialogResult.OK Then
            XMLExportTree(Tree, dlg.FileName)
        End If
        Me.Cursor = Cursors.Default
    End Sub
    Private Sub bLoad_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bLoad.Click
        Dim dlg As New OpenFileDialog()
        Me.Cursor = Cursors.WaitCursor
        dlg.Title = "Open XML Document"
        dlg.Filter = "XML Files (*.xml)|*.xml"
        Dim x1 As Integer = Application.StartupPath.LastIndexOf("\")
        x1 = Application.StartupPath.Substring(0, x1 - 1).LastIndexOf("\")
        dlg.FileName = Application.StartupPath.Substring(0, x1) + "\example.xml"
        If dlg.ShowDialog() = DialogResult.OK Then
            _Tree.BeginUpdate()
            _Model.Nodes.Clear()
            _Tree.EndUpdate()
            _Tree.Select()
            XMLImportTree(Tree, dlg.FileName)
            Tree.ExpandAll()
        End If
        Me.Cursor = Cursors.Default
    End Sub
    Private Sub XMLExportTree(ByVal tv As TreeViewAdv, ByVal XMLFileName As String)
        'exports my tree to an xml fie
        Dim xtw As XmlTextWriter = New XmlTextWriter(XMLFileName, Nothing)
        xtw.Formatting = Formatting.Indented
        xtw.WriteStartDocument()
        xtw.WriteStartElement("TreeViewAdv")
        xtw.WriteComment("Exported: " + Format(Now, "yyyy MMM dd HH:mm:ss"))
        xtw.WriteComment("Computer generated - do not edit")
        '-----
        For Each RootNode As TreeNodeAdv In tv.Root.Children
            xtw.WriteStartElement("VBMultiColTreeNode")
            XMLExportNodes(RootNode.Tag, xtw)
            xtw.WriteEndElement()
        Next
        '-----
        xtw.WriteEndElement()
        xtw.WriteEndDocument()
        xtw.Close()
    End Sub
    Private Sub XMLExportNodes(ByVal n As VBMultiColTreeNode, ByVal xtw As XmlTextWriter)
        'exports a node from my tree to an xml fie
        n.ExportXML(xtw)
        '-----
        If n.Nodes.Count > 0 Then
            For Each ChildNode As VBMultiColTreeNode In n.Nodes
                xtw.WriteStartElement("VBMultiColTree")
                XMLExportNodes(ChildNode, xtw)
                xtw.WriteEndElement()
            Next
        End If
        '-----
    End Sub
    Private Sub XMLImportTree(ByVal tv As TreeViewAdv, ByVal XMLFileName As String)
        'imports XML structure into my tree from an xml fie
        Dim xtr As XmlTextReader = New XmlTextReader(XMLFileName)
        ' Parse the file and display each of the nodes.
        While xtr.Read()
            Select Case xtr.NodeType
                Case XmlNodeType.Element
                    'this should be a TreeView item
                    'followed by some nodes
                    If xtr.Name = "VBMultiColTreeNode" Then
                        ' read the entire element into a new xml reader structure
                        Dim inner As XmlReader = xtr.ReadSubtree()
                        ' and recursively porocess this sub-structure
                        XMLImportNodes(Nothing, inner)
                    End If
                    'Case XmlNodeType.Text
                    'Case XmlNodeType.CDATA
                    'Case XmlNodeType.ProcessingInstruction
                    'Case XmlNodeType.Comment
                    'Case XmlNodeType.XmlDeclaration
                    'Case XmlNodeType.Document
                    'Case XmlNodeType.DocumentType
                    'Case XmlNodeType.EntityReference
                    'Case XmlNodeType.EndElement
            End Select
        End While

        xtr.Close()

    End Sub
    Private Sub XMLImportNodes(ByVal n As VBMultiColTreeNode, ByVal xtr As XmlReader)
        'analyse the reader for a top level node and N children (could be zero)
        ' uses a subtree of the original tree
        xtr.Read()  'step into the data
        Select Case xtr.NodeType    ' it should be an element but we test anyway
            Case XmlNodeType.Element
                'this shoud be a Node item - we ignore anything else
                If xtr.Name = "VBMultiColTreeNode" Then
                    Dim ReadNode As VBMultiColTreeNode = New VBMultiColTreeNode(ImageList24)
                    Dim inner As XmlReader = xtr.ReadSubtree()
                    xtr.Read()
                    ReadNode.ImportXML(inner)   'populate the node from the xml
                    If n Is Nothing Then 'add a root item 
                        _Model.Nodes.Add(ReadNode)
                    Else  'add a child item 
                        n.Nodes.Add(ReadNode)
                    End If
                    'now there may be children.... as a list of elements.
                    'so process each element as we find them - recursively
                    While xtr.Read()
                        If xtr.NodeType = XmlNodeType.Element Then
                            Dim innerChildren As XmlReader = xtr.ReadSubtree()
                            XMLImportNodes(ReadNode, innerChildren)
                        End If
                    End While
                End If
        End Select

        xtr.Close()  'closing this substream advances the pointer in the parent stream
    End Sub
#End Region

    Private Sub Tree_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Tree.Click
        If Tree.SelectedNode IsNot Nothing Then
            Dim MyNode As VBMultiColTreeNode = DirectCast(Tree.SelectedNode.Tag, VBMultiColTreeNode)
            txtCommand.Text = MyNode.nCommand
            txtValue.Text = MyNode.nValue
            txtInformation.Text = MyNode.nInformation
            cbImages.Text = MyNode.nImageKey
        End If
    End Sub
End Class
