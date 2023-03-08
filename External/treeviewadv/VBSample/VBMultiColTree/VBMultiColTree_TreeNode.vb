Public Class VBMultiColTreeNode
    Inherits Node

    Private _Command As String = ""
    Private _Value As String = ""
    Private _Information As String = ""
    Private _Icon As Image
    Private _ImageKey As String
    Private _ImageList As ImageList

#Region "Constructors"

    Public Sub New(ByVal myImageList As ImageList)
        _ImageList = myImageList
    End Sub
    Public Sub New(ByVal sCommand As String, _
                    ByVal sValue As String, _
                    ByVal sInfo As String, _
                    ByVal myImageList As ImageList, _
                    ByVal sImageKey As String)
        nCommand = sCommand
        nValue = sValue
        nInformation = sInfo
        _ImageList = myImageList
        nImageKey = sImageKey
    End Sub
#End Region

#Region "Properties"
    Public Property nCommand() As String
        Get
            Return _Command
        End Get
        Set(ByVal value As String)
            _Command = value
            NotifyModel()
        End Set
    End Property
    Public Property nValue() As String
        Get
            Return _Value
        End Get
        Set(ByVal value As String)
            _Value = value
            NotifyModel()
        End Set
    End Property
    Public Property nInformation() As String
        Get
            Return _Information
        End Get
        Set(ByVal value As String)
            _Information = value
            NotifyModel()
        End Set
    End Property
    Public Property nImageKey() As String
        Get
            Return _ImageKey
        End Get
        Set(ByVal value As String)
            _ImageKey = value
            'now generate the bitmap from this key.
            Dim bm As Bitmap = New Bitmap(_ImageList.Images(value))
            _Icon = bm
            NotifyModel()
        End Set
    End Property
    Public Property nIcon() As Bitmap
        Get
            Return _Icon
        End Get
        Set(ByVal value As Bitmap)
            _Icon = value
            NotifyModel()
        End Set
    End Property
#End Region

#Region "Methods"
    Public Function Clone() As VBMultiColTreeNode
        Dim NewNode As New VBMultiColTreeNode(_ImageList)
        NewNode.nCommand = nCommand
        NewNode.nValue = nValue
        NewNode.nInformation = nInformation
        NewNode.nImageKey = nImageKey
        Return NewNode
    End Function
    Public Sub ExportXML(ByVal xtw As XmlTextWriter)
        ' xtw.WriteStartElement("VBMultiColTreeNode")
        xtw.WriteElementString("Command", nCommand)
        xtw.WriteElementString("Value", nValue)
        xtw.WriteElementString("Information", nInformation)
        xtw.WriteElementString("ImageKey", nImageKey)
        ' xtw.WriteEndElement()
    End Sub
    Public Sub ImportXML(ByVal xtr As XmlReader)
        xtr.Read()  'step into the data itself.
        ' read the attributes in the correct order otherwise they get skipped over
        nCommand = xtr.ReadElementString("Command")
        nValue = xtr.ReadElementString("Value")
        nInformation = xtr.ReadElementString("Information")
        nImageKey = xtr.ReadElementString("ImageKey")
    End Sub
#End Region

#Region "OverRides"
    Public Overrides Function ToString() As String
        Return _Command
    End Function
#End Region
End Class
