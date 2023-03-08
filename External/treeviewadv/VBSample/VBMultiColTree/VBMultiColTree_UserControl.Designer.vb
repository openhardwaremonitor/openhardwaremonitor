<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class VBMultiColTreeUserControl
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(VBMultiColTreeUserControl))
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer
        Me.Tree = New Aga.Controls.Tree.TreeViewAdv
        Me.TreeColumn1 = New Aga.Controls.Tree.TreeColumn
        Me.TreeColumn2 = New Aga.Controls.Tree.TreeColumn
        Me.TreeColumn3 = New Aga.Controls.Tree.TreeColumn
        Me.NodeIcon = New Aga.Controls.Tree.NodeControls.NodeIcon
        Me.NodeCommand = New Aga.Controls.Tree.NodeControls.NodeTextBox
        Me.NodeValue = New Aga.Controls.Tree.NodeControls.NodeTextBox
        Me.NodeInformation = New Aga.Controls.Tree.NodeControls.NodeTextBox
        Me.GroupBox1 = New System.Windows.Forms.GroupBox
        Me.GroupBox3 = New System.Windows.Forms.GroupBox
        Me.bClearSelection = New System.Windows.Forms.Button
        Me.bCollapseAll = New System.Windows.Forms.Button
        Me.bRefreshTree = New System.Windows.Forms.Button
        Me.bExpandCollapse = New System.Windows.Forms.Button
        Me.bClearTree = New System.Windows.Forms.Button
        Me.bExpandAll = New System.Windows.Forms.Button
        Me.bLoad = New System.Windows.Forms.Button
        Me.bSave = New System.Windows.Forms.Button
        Me.GroupBox2 = New System.Windows.Forms.GroupBox
        Me.bDelete = New System.Windows.Forms.Button
        Me.Label3 = New System.Windows.Forms.Label
        Me.Label2 = New System.Windows.Forms.Label
        Me.Label1 = New System.Windows.Forms.Label
        Me.bCollapseNode = New System.Windows.Forms.Button
        Me.bExpandNode = New System.Windows.Forms.Button
        Me.txtInformation = New System.Windows.Forms.TextBox
        Me.txtValue = New System.Windows.Forms.TextBox
        Me.bAddRoot = New System.Windows.Forms.Button
        Me.bMoveLeft = New System.Windows.Forms.Button
        Me.bChangeIcon = New System.Windows.Forms.Button
        Me.txtCommand = New System.Windows.Forms.TextBox
        Me.bMoveDown = New System.Windows.Forms.Button
        Me.bMoveUp = New System.Windows.Forms.Button
        Me.bAddSibling = New System.Windows.Forms.Button
        Me.bAddParent = New System.Windows.Forms.Button
        Me.bAddChild = New System.Windows.Forms.Button
        Me.cbImages = New System.Windows.Forms.ComboBox
        Me.PictureBox1 = New System.Windows.Forms.PictureBox
        Me.TreeColumn4 = New Aga.Controls.Tree.TreeColumn
        Me.TreeColumn5 = New Aga.Controls.Tree.TreeColumn
        Me.TreeColumn6 = New Aga.Controls.Tree.TreeColumn
        Me.ImageList24 = New System.Windows.Forms.ImageList(Me.components)
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'SplitContainer1
        '
        Me.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer1.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.Tree)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.GroupBox1)
        Me.SplitContainer1.Size = New System.Drawing.Size(862, 489)
        Me.SplitContainer1.SplitterDistance = 494
        Me.SplitContainer1.TabIndex = 0
        '
        'Tree
        '
        Me.Tree.AutoRowHeight = True
        Me.Tree.BackColor = System.Drawing.SystemColors.Window
        Me.Tree.Columns.Add(Me.TreeColumn1)
        Me.Tree.Columns.Add(Me.TreeColumn2)
        Me.Tree.Columns.Add(Me.TreeColumn3)
        Me.Tree.DefaultToolTipProvider = Nothing
        Me.Tree.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Tree.DragDropMarkColor = System.Drawing.Color.Black
        Me.Tree.Font = New System.Drawing.Font("Tahoma", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Tree.GridLineStyle = CType((Aga.Controls.Tree.GridLineStyle.Horizontal Or Aga.Controls.Tree.GridLineStyle.Vertical), Aga.Controls.Tree.GridLineStyle)
        Me.Tree.LineColor = System.Drawing.SystemColors.ControlDark
        Me.Tree.Location = New System.Drawing.Point(0, 0)
        Me.Tree.Model = Nothing
        Me.Tree.Name = "Tree"
        Me.Tree.NodeControls.Add(Me.NodeIcon)
        Me.Tree.NodeControls.Add(Me.NodeCommand)
        Me.Tree.NodeControls.Add(Me.NodeValue)
        Me.Tree.NodeControls.Add(Me.NodeInformation)
        Me.Tree.SelectedNode = Nothing
        Me.Tree.Size = New System.Drawing.Size(494, 489)
        Me.Tree.TabIndex = 0
        Me.Tree.Text = "TreeViewAdv1"
        Me.Tree.UseColumns = True
        '
        'TreeColumn1
        '
        Me.TreeColumn1.Header = "Commands"
        Me.TreeColumn1.MinColumnWidth = 10
        Me.TreeColumn1.SortOrder = System.Windows.Forms.SortOrder.None
        Me.TreeColumn1.TooltipText = Nothing
        Me.TreeColumn1.Width = 150
        '
        'TreeColumn2
        '
        Me.TreeColumn2.Header = "Value"
        Me.TreeColumn2.MinColumnWidth = 10
        Me.TreeColumn2.SortOrder = System.Windows.Forms.SortOrder.None
        Me.TreeColumn2.TooltipText = Nothing
        Me.TreeColumn2.Width = 150
        '
        'TreeColumn3
        '
        Me.TreeColumn3.Header = "Information"
        Me.TreeColumn3.MinColumnWidth = 10
        Me.TreeColumn3.SortOrder = System.Windows.Forms.SortOrder.None
        Me.TreeColumn3.TooltipText = Nothing
        Me.TreeColumn3.Width = 150
        '
        'NodeIcon
        '
        Me.NodeIcon.DataPropertyName = "nIcon"
        Me.NodeIcon.LeftMargin = 1
        Me.NodeIcon.ParentColumn = Me.TreeColumn1
        '
        'NodeCommand
        '
        Me.NodeCommand.DataPropertyName = "nCommand"
        Me.NodeCommand.IncrementalSearchEnabled = True
        Me.NodeCommand.LeftMargin = 3
        Me.NodeCommand.ParentColumn = Me.TreeColumn1
        '
        'NodeValue
        '
        Me.NodeValue.DataPropertyName = "nValue"
        Me.NodeValue.IncrementalSearchEnabled = True
        Me.NodeValue.LeftMargin = 3
        Me.NodeValue.ParentColumn = Me.TreeColumn2
        '
        'NodeInformation
        '
        Me.NodeInformation.DataPropertyName = "nInformation"
        Me.NodeInformation.IncrementalSearchEnabled = True
        Me.NodeInformation.LeftMargin = 3
        Me.NodeInformation.ParentColumn = Me.TreeColumn3
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.GroupBox3)
        Me.GroupBox1.Controls.Add(Me.GroupBox2)
        Me.GroupBox1.Dock = System.Windows.Forms.DockStyle.Right
        Me.GroupBox1.Location = New System.Drawing.Point(8, 0)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(356, 489)
        Me.GroupBox1.TabIndex = 0
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Basic Tree Controls"
        '
        'GroupBox3
        '
        Me.GroupBox3.BackColor = System.Drawing.SystemColors.Control
        Me.GroupBox3.Controls.Add(Me.bClearSelection)
        Me.GroupBox3.Controls.Add(Me.bCollapseAll)
        Me.GroupBox3.Controls.Add(Me.bRefreshTree)
        Me.GroupBox3.Controls.Add(Me.bExpandCollapse)
        Me.GroupBox3.Controls.Add(Me.bClearTree)
        Me.GroupBox3.Controls.Add(Me.bExpandAll)
        Me.GroupBox3.Controls.Add(Me.bLoad)
        Me.GroupBox3.Controls.Add(Me.bSave)
        Me.GroupBox3.Location = New System.Drawing.Point(6, 278)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(344, 113)
        Me.GroupBox3.TabIndex = 58
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Tree"
        '
        'bClearSelection
        '
        Me.bClearSelection.Location = New System.Drawing.Point(110, 79)
        Me.bClearSelection.Name = "bClearSelection"
        Me.bClearSelection.Size = New System.Drawing.Size(92, 24)
        Me.bClearSelection.TabIndex = 66
        Me.bClearSelection.Text = "Clear Selection"
        Me.bClearSelection.UseVisualStyleBackColor = True
        '
        'bCollapseAll
        '
        Me.bCollapseAll.Location = New System.Drawing.Point(10, 19)
        Me.bCollapseAll.Name = "bCollapseAll"
        Me.bCollapseAll.Size = New System.Drawing.Size(94, 24)
        Me.bCollapseAll.TabIndex = 65
        Me.bCollapseAll.Text = "Collapse Tree"
        Me.bCollapseAll.UseVisualStyleBackColor = True
        '
        'bRefreshTree
        '
        Me.bRefreshTree.Location = New System.Drawing.Point(110, 49)
        Me.bRefreshTree.Name = "bRefreshTree"
        Me.bRefreshTree.Size = New System.Drawing.Size(94, 24)
        Me.bRefreshTree.TabIndex = 64
        Me.bRefreshTree.Text = "Refresh Tree"
        Me.bRefreshTree.UseVisualStyleBackColor = True
        '
        'bExpandCollapse
        '
        Me.bExpandCollapse.Location = New System.Drawing.Point(10, 79)
        Me.bExpandCollapse.Name = "bExpandCollapse"
        Me.bExpandCollapse.Size = New System.Drawing.Size(94, 24)
        Me.bExpandCollapse.TabIndex = 63
        Me.bExpandCollapse.Text = "Toggle"
        Me.bExpandCollapse.UseVisualStyleBackColor = True
        '
        'bClearTree
        '
        Me.bClearTree.Location = New System.Drawing.Point(110, 19)
        Me.bClearTree.Name = "bClearTree"
        Me.bClearTree.Size = New System.Drawing.Size(94, 24)
        Me.bClearTree.TabIndex = 62
        Me.bClearTree.Text = "Clear Tree"
        Me.bClearTree.UseVisualStyleBackColor = True
        '
        'bExpandAll
        '
        Me.bExpandAll.Location = New System.Drawing.Point(10, 49)
        Me.bExpandAll.Name = "bExpandAll"
        Me.bExpandAll.Size = New System.Drawing.Size(94, 24)
        Me.bExpandAll.TabIndex = 60
        Me.bExpandAll.Text = "Expand Tree"
        Me.bExpandAll.UseVisualStyleBackColor = True
        '
        'bLoad
        '
        Me.bLoad.Location = New System.Drawing.Point(230, 49)
        Me.bLoad.Name = "bLoad"
        Me.bLoad.Size = New System.Drawing.Size(94, 24)
        Me.bLoad.TabIndex = 59
        Me.bLoad.Text = "Load XML"
        Me.bLoad.UseVisualStyleBackColor = True
        '
        'bSave
        '
        Me.bSave.Location = New System.Drawing.Point(230, 19)
        Me.bSave.Name = "bSave"
        Me.bSave.Size = New System.Drawing.Size(94, 24)
        Me.bSave.TabIndex = 58
        Me.bSave.Text = "Save XML"
        Me.bSave.UseVisualStyleBackColor = True
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.bDelete)
        Me.GroupBox2.Controls.Add(Me.Label3)
        Me.GroupBox2.Controls.Add(Me.Label2)
        Me.GroupBox2.Controls.Add(Me.Label1)
        Me.GroupBox2.Controls.Add(Me.bCollapseNode)
        Me.GroupBox2.Controls.Add(Me.bExpandNode)
        Me.GroupBox2.Controls.Add(Me.txtInformation)
        Me.GroupBox2.Controls.Add(Me.txtValue)
        Me.GroupBox2.Controls.Add(Me.bAddRoot)
        Me.GroupBox2.Controls.Add(Me.bMoveLeft)
        Me.GroupBox2.Controls.Add(Me.bChangeIcon)
        Me.GroupBox2.Controls.Add(Me.txtCommand)
        Me.GroupBox2.Controls.Add(Me.bMoveDown)
        Me.GroupBox2.Controls.Add(Me.bMoveUp)
        Me.GroupBox2.Controls.Add(Me.bAddSibling)
        Me.GroupBox2.Controls.Add(Me.bAddParent)
        Me.GroupBox2.Controls.Add(Me.bAddChild)
        Me.GroupBox2.Controls.Add(Me.cbImages)
        Me.GroupBox2.Controls.Add(Me.PictureBox1)
        Me.GroupBox2.Location = New System.Drawing.Point(6, 19)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(344, 253)
        Me.GroupBox2.TabIndex = 57
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Node"
        '
        'bDelete
        '
        Me.bDelete.Location = New System.Drawing.Point(232, 157)
        Me.bDelete.Name = "bDelete"
        Me.bDelete.Size = New System.Drawing.Size(92, 24)
        Me.bDelete.TabIndex = 73
        Me.bDelete.Text = "Delete Node"
        Me.bDelete.UseVisualStyleBackColor = True
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(36, 71)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(25, 13)
        Me.Label3.TabIndex = 72
        Me.Label3.Text = "Info"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(27, 45)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(34, 13)
        Me.Label2.TabIndex = 71
        Me.Label2.Text = "Value"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(7, 19)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(54, 13)
        Me.Label1.TabIndex = 70
        Me.Label1.Text = "Command"
        '
        'bCollapseNode
        '
        Me.bCollapseNode.Location = New System.Drawing.Point(230, 97)
        Me.bCollapseNode.Name = "bCollapseNode"
        Me.bCollapseNode.Size = New System.Drawing.Size(94, 24)
        Me.bCollapseNode.TabIndex = 69
        Me.bCollapseNode.Text = "Collapse Node"
        Me.bCollapseNode.UseVisualStyleBackColor = True
        '
        'bExpandNode
        '
        Me.bExpandNode.Location = New System.Drawing.Point(232, 127)
        Me.bExpandNode.Name = "bExpandNode"
        Me.bExpandNode.Size = New System.Drawing.Size(92, 24)
        Me.bExpandNode.TabIndex = 68
        Me.bExpandNode.Text = "Expand Node"
        Me.bExpandNode.UseVisualStyleBackColor = True
        '
        'txtInformation
        '
        Me.txtInformation.Location = New System.Drawing.Point(72, 71)
        Me.txtInformation.Name = "txtInformation"
        Me.txtInformation.Size = New System.Drawing.Size(252, 20)
        Me.txtInformation.TabIndex = 67
        '
        'txtValue
        '
        Me.txtValue.Location = New System.Drawing.Point(72, 45)
        Me.txtValue.Name = "txtValue"
        Me.txtValue.Size = New System.Drawing.Size(252, 20)
        Me.txtValue.TabIndex = 66
        '
        'bAddRoot
        '
        Me.bAddRoot.BackColor = System.Drawing.SystemColors.Control
        Me.bAddRoot.Location = New System.Drawing.Point(73, 97)
        Me.bAddRoot.Name = "bAddRoot"
        Me.bAddRoot.Size = New System.Drawing.Size(70, 24)
        Me.bAddRoot.TabIndex = 65
        Me.bAddRoot.Text = "Add Root"
        Me.bAddRoot.UseVisualStyleBackColor = False
        '
        'bMoveLeft
        '
        Me.bMoveLeft.Location = New System.Drawing.Point(151, 127)
        Me.bMoveLeft.Name = "bMoveLeft"
        Me.bMoveLeft.Size = New System.Drawing.Size(75, 24)
        Me.bMoveLeft.TabIndex = 64
        Me.bMoveLeft.Text = "Move Left"
        Me.bMoveLeft.UseVisualStyleBackColor = True
        '
        'bChangeIcon
        '
        Me.bChangeIcon.Location = New System.Drawing.Point(149, 187)
        Me.bChangeIcon.Name = "bChangeIcon"
        Me.bChangeIcon.Size = New System.Drawing.Size(77, 24)
        Me.bChangeIcon.TabIndex = 63
        Me.bChangeIcon.Text = "Change Icon"
        Me.bChangeIcon.UseVisualStyleBackColor = True
        '
        'txtCommand
        '
        Me.txtCommand.Location = New System.Drawing.Point(72, 19)
        Me.txtCommand.Name = "txtCommand"
        Me.txtCommand.Size = New System.Drawing.Size(252, 20)
        Me.txtCommand.TabIndex = 62
        '
        'bMoveDown
        '
        Me.bMoveDown.Location = New System.Drawing.Point(149, 157)
        Me.bMoveDown.Name = "bMoveDown"
        Me.bMoveDown.Size = New System.Drawing.Size(77, 24)
        Me.bMoveDown.TabIndex = 61
        Me.bMoveDown.Text = "Move Down"
        Me.bMoveDown.UseVisualStyleBackColor = True
        '
        'bMoveUp
        '
        Me.bMoveUp.Location = New System.Drawing.Point(149, 97)
        Me.bMoveUp.Name = "bMoveUp"
        Me.bMoveUp.Size = New System.Drawing.Size(75, 24)
        Me.bMoveUp.TabIndex = 60
        Me.bMoveUp.Text = "Move Up"
        Me.bMoveUp.UseVisualStyleBackColor = True
        '
        'bAddSibling
        '
        Me.bAddSibling.Location = New System.Drawing.Point(73, 187)
        Me.bAddSibling.Name = "bAddSibling"
        Me.bAddSibling.Size = New System.Drawing.Size(70, 24)
        Me.bAddSibling.TabIndex = 59
        Me.bAddSibling.Text = "Add Sibling"
        Me.bAddSibling.UseVisualStyleBackColor = True
        '
        'bAddParent
        '
        Me.bAddParent.Location = New System.Drawing.Point(73, 127)
        Me.bAddParent.Name = "bAddParent"
        Me.bAddParent.Size = New System.Drawing.Size(70, 24)
        Me.bAddParent.TabIndex = 58
        Me.bAddParent.Text = "Add Parent"
        Me.bAddParent.UseVisualStyleBackColor = True
        '
        'bAddChild
        '
        Me.bAddChild.Location = New System.Drawing.Point(73, 157)
        Me.bAddChild.Name = "bAddChild"
        Me.bAddChild.Size = New System.Drawing.Size(70, 24)
        Me.bAddChild.TabIndex = 57
        Me.bAddChild.Text = "Add Child"
        Me.bAddChild.UseVisualStyleBackColor = True
        '
        'cbImages
        '
        Me.cbImages.FormattingEnabled = True
        Me.cbImages.Location = New System.Drawing.Point(232, 187)
        Me.cbImages.Name = "cbImages"
        Me.cbImages.Size = New System.Drawing.Size(77, 21)
        Me.cbImages.TabIndex = 56
        '
        'PictureBox1
        '
        Me.PictureBox1.Location = New System.Drawing.Point(314, 187)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(24, 24)
        Me.PictureBox1.TabIndex = 55
        Me.PictureBox1.TabStop = False
        '
        'TreeColumn4
        '
        Me.TreeColumn4.Header = ""
        Me.TreeColumn4.SortOrder = System.Windows.Forms.SortOrder.None
        Me.TreeColumn4.TooltipText = Nothing
        '
        'TreeColumn5
        '
        Me.TreeColumn5.Header = ""
        Me.TreeColumn5.SortOrder = System.Windows.Forms.SortOrder.None
        Me.TreeColumn5.TooltipText = Nothing
        '
        'TreeColumn6
        '
        Me.TreeColumn6.Header = ""
        Me.TreeColumn6.SortOrder = System.Windows.Forms.SortOrder.None
        Me.TreeColumn6.TooltipText = Nothing
        '
        'ImageList24
        '
        Me.ImageList24.ImageStream = CType(resources.GetObject("ImageList24.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.ImageList24.TransparentColor = System.Drawing.Color.Transparent
        Me.ImageList24.Images.SetKeyName(0, "Blue.ico")
        Me.ImageList24.Images.SetKeyName(1, "Green.ico")
        Me.ImageList24.Images.SetKeyName(2, "Red.ico")
        Me.ImageList24.Images.SetKeyName(3, "Yellow.ico")
        '
        'VBMultiColTreeUserControl
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.SplitContainer1)
        Me.Name = "VBMultiColTreeUserControl"
        Me.Size = New System.Drawing.Size(862, 489)
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        Me.SplitContainer1.ResumeLayout(False)
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
    Friend WithEvents Tree As Aga.Controls.Tree.TreeViewAdv
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents TreeColumn1 As Aga.Controls.Tree.TreeColumn
    Friend WithEvents TreeColumn2 As Aga.Controls.Tree.TreeColumn
    Friend WithEvents TreeColumn3 As Aga.Controls.Tree.TreeColumn
    Friend WithEvents NodeIcon As Aga.Controls.Tree.NodeControls.NodeIcon
    Friend WithEvents NodeCommand As Aga.Controls.Tree.NodeControls.NodeTextBox
    Friend WithEvents NodeValue As Aga.Controls.Tree.NodeControls.NodeTextBox
    Friend WithEvents NodeInformation As Aga.Controls.Tree.NodeControls.NodeTextBox
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents bCollapseNode As System.Windows.Forms.Button
    Friend WithEvents bExpandNode As System.Windows.Forms.Button
    Friend WithEvents txtInformation As System.Windows.Forms.TextBox
    Friend WithEvents txtValue As System.Windows.Forms.TextBox
    Friend WithEvents bAddRoot As System.Windows.Forms.Button
    Friend WithEvents bMoveLeft As System.Windows.Forms.Button
    Friend WithEvents bChangeIcon As System.Windows.Forms.Button
    Friend WithEvents txtCommand As System.Windows.Forms.TextBox
    Friend WithEvents bMoveDown As System.Windows.Forms.Button
    Friend WithEvents bMoveUp As System.Windows.Forms.Button
    Friend WithEvents bAddSibling As System.Windows.Forms.Button
    Friend WithEvents bAddParent As System.Windows.Forms.Button
    Friend WithEvents bAddChild As System.Windows.Forms.Button
    Friend WithEvents cbImages As System.Windows.Forms.ComboBox
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Friend WithEvents bClearSelection As System.Windows.Forms.Button
    Friend WithEvents bCollapseAll As System.Windows.Forms.Button
    Friend WithEvents bRefreshTree As System.Windows.Forms.Button
    Friend WithEvents bExpandCollapse As System.Windows.Forms.Button
    Friend WithEvents bClearTree As System.Windows.Forms.Button
    Friend WithEvents bExpandAll As System.Windows.Forms.Button
    Friend WithEvents bLoad As System.Windows.Forms.Button
    Friend WithEvents bSave As System.Windows.Forms.Button
    Friend WithEvents bDelete As System.Windows.Forms.Button
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents TreeColumn4 As Aga.Controls.Tree.TreeColumn
    Friend WithEvents TreeColumn5 As Aga.Controls.Tree.TreeColumn
    Friend WithEvents TreeColumn6 As Aga.Controls.Tree.TreeColumn
    Friend WithEvents ImageList24 As System.Windows.Forms.ImageList

End Class
