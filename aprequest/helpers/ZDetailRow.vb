Public Class ZDetailRow
#Region "Privates"
    Private _DeptTitle As String
    Private _ServTitle As String
    Private _GLAcctTitle As String
    Private _PayerTitle As String
    Private _PersonTitle As String

    Private _InvoiceID As String    ' hopefully this is a GUID! &$%#!
    Private _guid As String
    Private _Ordinal As Integer
    Private _Amount As Double

    Private _DeptCode As String     ' tblAcctCode_5
    Private _ServCode As String     ' tblAcctCode_4
    Private _GLAcctCode As Long     ' tblAcctCode_0
    Private _PayerCode As Long      ' tblAcctCode_3
    Private _Person As Long         ' tblAcctCode_9

    Private _ConnStr As String

    Private _Username As String
#End Region
#Region "Properites"
    ' Identifiers
    Public Property Username() As String
        Get
            Return Me._Username
        End Get
        Set(ByVal value As String)
            Me._Username = value
        End Set
    End Property

    Public Property GUID() As String
        Get
            Return _guid
        End Get
        Set(ByVal value As String)
            Me._guid = value
        End Set
    End Property

    Public Property Invoice_GUID() As String
        Get
            Return Me._InvoiceID
        End Get
        Set(ByVal value As String)
            Me._InvoiceID = value
        End Set
    End Property

    Public Property Ordinal() As Long
        Get
            Return Me._Ordinal
        End Get
        Set(ByVal value As Long)
            Me._Ordinal = value
        End Set
    End Property

    ' Amount
    Public Property Amount() As Double
        Get
            Return Convert.ToDouble(Me._Amount.ToString("C"))
        End Get
        Set(ByVal value As Double)
            Me._Amount = value
        End Set
    End Property

    ' The codes
    Public Property DeptartmentCode() As String
        Get
            Return Me._DeptCode
        End Get
        Set(ByVal value As String)
            Me._DeptCode = value
        End Set
    End Property

    Public Property ServiceCode() As String
        Get
            Return Me._ServCode
        End Get
        Set(ByVal value As String)
            Me._ServCode = value
        End Set
    End Property

    Public Property GLAcct() As String
        Get
            Return Me._GLAcctCode
        End Get
        Set(ByVal value As String)
            Me._GLAcctCode = value
        End Set
    End Property

    Public Property PayerCode() As String
        Get
            Return Me._PayerCode
        End Get
        Set(ByVal value As String)
            Me._PayerCode = value
        End Set
    End Property

    Public Property PersonCode() As String
        Get
            Return Me._Person
        End Get
        Set(ByVal value As String)
            Me._Person = value
        End Set
    End Property

    ' Now all the text
    Public Property Department() As String
        Get
            Return Me._DeptTitle
        End Get
        Set(ByVal value As String)
            Me._DeptTitle = value
        End Set
    End Property

    Public Property Service() As String
        Get
            Return Me._ServTitle
        End Get
        Set(ByVal value As String)
            Me._ServTitle = value
        End Set
    End Property

    Public Property GLAccount() As String
        Get
            Return Me._GLAcctTitle
        End Get
        Set(ByVal value As String)
            Me._GLAcctTitle = value
        End Set
    End Property

    Public Property Payer() As Boolean
        Get
            Return Me._PayerTitle
        End Get
        Set(ByVal value As Boolean)
            Me._PayerTitle = value
        End Set
    End Property

    Public Property Person() As String
        Get
            Return Me._PersonTitle
        End Get
        Set(ByVal value As String)
            Me._PersonTitle = value
        End Set
    End Property

#End Region

    Public Sub New()
        Dim zz As New zHashes
        Me._guid = zz.MkGUID
        Me._ConnStr = ConfigurationManager.ConnectionStrings("aprequestConnectionString").ConnectionString
        Me._InvoiceID = ""          ' should change
        Me._Amount = 0

        Me._DeptCode = ""
        Me._ServCode = ""
        Me._GLAcctCode = ""
        Me._PayerCode = ""
        Me._Person = ""

        Me._DeptTitle = ""
        Me._ServTitle = ""
        Me._GLAcctTitle = ""
        Me._PayerTitle = ""
        Me._PersonTitle = ""
    End Sub

    Public Sub New(ByVal GUID As String, ByVal Inv_ID As String, _
            ByVal Amount As Double, ByVal Ordinal As Integer, _
            ByVal DeptCode As String, ByVal DeptTitle As String, _
            ByVal ServCode As String, ByVal ServTitle As String, _
            ByVal GLAcctCode As String, ByVal GLAcctTitle As String, _
            ByVal PayerCode As String, ByVal PayerTitle As String, _
            ByVal Person As String, ByVal PersonTitle As String, _
            ByVal Last_At As Date)
        Me._ConnStr = ConfigurationManager.ConnectionStrings("aprequestConnectionString").ConnectionString
        Me._guid = GUID
        Me._InvoiceID = Inv_ID
        Me._Amount = Amount
        Me._Ordinal = Ordinal

        Me._DeptCode = DeptCode
        Me._ServCode = ServCode
        Me._GLAcctCode = GLAcctCode
        Me._PayerCode = PayerCode
        Me._Person = Person

        Me._DeptTitle = DeptTitle
        Me._ServTitle = ServTitle
        Me._GLAcctTitle = GLAcctTitle
        Me._PayerTitle = PayerTitle
        Me._PersonTitle = PersonTitle
    End Sub

    Public Sub SaveToDB()
        Dim dsDR As New dsRequestDetailsTableAdapters.requestdetailsTableAdapter
        Dim lngCnt As Integer = Convert.ToInt32(dsDR.GetNumDetailsByGUID(Me._guid))
        With Me
            If lngCnt > 0 Then
                dsDR.UpdateByGUID(_InvoiceID, _Ordinal, _Amount, _
                                _DeptCode, _ServCode, _GLAcctCode, _PayerCode, _Person, _
                                _DeptTitle, _ServTitle, _GLAcctTitle, _PayerTitle, _PersonTitle, _
                                0, _guid)

            Else
                Dim strUser = ""

                dsDR.usp_AddDetail(_guid, _InvoiceID, _Ordinal, _Amount, _
                                   _DeptCode, _ServCode, _GLAcctCode, _PayerCode, _Person, _
                                   _DeptTitle, _ServTitle, _GLAcctTitle, _PayerTitle, _Person, _
                                   strUser)
            End If
        End With
    End Sub

    Public Sub Delete()
        Dim dsDR As New dsRequestDetailsTableAdapters.requestdetailsTableAdapter
        Dim lngCnt As Integer = Convert.ToInt32(dsDR.GetNumDetailsByGUID(Me._guid))
        If lngCnt > 0 Then
        Else
            With Me
                dsDR.UpdateByGUID(_InvoiceID, _Ordinal, _Amount, _
                                _DeptCode, _ServCode, _GLAcctCode, _PayerCode, _Person, _
                                _DeptTitle, _ServTitle, _GLAcctTitle, _PayerTitle, _PersonTitle, _
                                0, _guid)
            End With
        End If
    End Sub
End Class
