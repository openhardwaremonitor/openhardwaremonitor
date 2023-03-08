<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class VBMultiColTree_MainForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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
        Me.VbMultiColTreeUserControl1 = New VBMultiColTree.VBMultiColTreeUserControl
        Me.SuspendLayout()
        '
        'VbMultiColTreeUserControl1
        '
        Me.VbMultiColTreeUserControl1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.VbMultiColTreeUserControl1.Location = New System.Drawing.Point(0, 0)
        Me.VbMultiColTreeUserControl1.Name = "VbMultiColTreeUserControl1"
        Me.VbMultiColTreeUserControl1.Size = New System.Drawing.Size(910, 462)
        Me.VbMultiColTreeUserControl1.TabIndex = 0
        '
        'VBMultiColTree_MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(910, 462)
        Me.Controls.Add(Me.VbMultiColTreeUserControl1)
        Me.Name = "VBMultiColTree_MainForm"
        Me.Text = "VBMultiColTreeMainForm"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents VbMultiColTreeUserControl1 As VBMultiColTree.VBMultiColTreeUserControl
End Class
