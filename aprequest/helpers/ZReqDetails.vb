Imports System.Data.SqlClient
Imports System.Object
Imports System.Attribute
Imports System.ComponentModel
'http://highoncoding.com/Articles/139_GridView_With_ObjectDataSource.aspx

<DataObjectAttribute()> _
Public Class ZReqDetails
    Private _DetailRows As List(Of ZDetailRow)
    Private _ConnStr As String
    Private _InvoiceNo As String

    Public Property InvoiceNo() As String
        Get
            Return Me._InvoiceNo
        End Get
        Set(ByVal value As String)
            Me._InvoiceNo = value
        End Set
    End Property

    Public ReadOnly Property Details() As List(Of ZDetailRow)
        Get
            Return _DetailRows
        End Get
    End Property

    Public Sub New()
        Me._DetailRows = New List(Of ZDetailRow)
        Me._InvoiceNo = ""
        Me._ConnStr = ConfigurationManager.ConnectionStrings("aprequestConnectionString").ConnectionString
        'Dim connections As ConnectionStringSettingsCollection = ConfigurationManager.ConnectionStrings
    End Sub

    Public Sub New(ByVal Inv_no As String)
        Me._InvoiceNo = Inv_no
    End Sub

    Public Function LoadDetails(ByVal inv_no As String) As List(Of ZDetailRow)
        Me._DetailRows = New List(Of ZDetailRow)
        Me._InvoiceNo = inv_no
        Dim myConn As New SqlConnection(Me._ConnStr)
        Dim myCmd As New SqlCommand("usp_GetDetailsByInvoice", myConn)
        myCmd.CommandType = CommandType.StoredProcedure

        myCmd.Parameters.AddWithValue("@inv_id", inv_no)
        myConn.Open()
        Dim reader As SqlDataReader = myCmd.ExecuteReader()
        While (reader.Read())
            Dim row As New ZDetailRow(reader("guid"), reader("inv_id"), _
                    Convert.ToDouble(reader("amount")), reader("ordinal"), _
                    reader("deptcode"), reader("deptitle"), _
                    reader("servcode"), reader("servtitle"), _
                    reader("glacctcode"), reader("glaccttitle"), _
                    reader("payercode"), reader("payertitle"), _
                    reader("person"), reader("persontitle"), _
                    Convert.ToDateTime(reader("last_at")))
            Me._DetailRows.Add(row)
        End While
        reader.Close()
        myConn.Close()
        myCmd.Dispose()
        If (Not (Me._DetailRows Is Nothing)) And (Me._DetailRows.Count > 0) Then
            Return _DetailRows
        Else
            Return Nothing
        End If
    End Function

    Public Function GetDetails(ByVal inv_no As String) As List(Of ZDetailRow)
        Me._InvoiceNo = inv_no
        If Me._DetailRows.Count < 1 Then
            Return Me.LoadDetails(inv_no)
        End If

        If (Not (Me._DetailRows Is Nothing)) And (Me._DetailRows.Count > 0) Then
            Return Me._DetailRows
        Else
            Return Nothing
        End If
    End Function

    Public Function AddDetailRow(ByVal Amount As Double, _
            ByVal DeptCode As String, ByVal DeptTitle As String, _
            ByVal ServCode As String, ByVal ServTitle As String, _
            ByVal GLAcctCode As String, ByVal GLAcctTitle As String, _
            ByVal PayerCode As String, ByVal PayerTitle As String, _
            ByVal Person As String, ByVal PersonTitle As String) As Boolean
        Dim zz As New zHashes

        If Me._DetailRows Is Nothing Then
            Me._DetailRows = New List(Of ZDetailRow)
        End If
        Dim row As New ZDetailRow(zz.MkGUID, Me._InvoiceNo, Amount, Me._DetailRows.Count, _
                DeptCode, DeptTitle, ServCode, ServTitle, _
                GLAcctCode, GLAcctTitle, PayerCode, PayerTitle, _
                Person, PersonTitle, Date.Now())
        Me._DetailRows.Add(row)
        Return False
        '       Dim myConn As New SqlConnection(ConfigurationManager.ConnectionStrings("aprequestConnectionString").ConnectionString)
        '       Dim myCmd As New SqlCommand("usp_AddDetail", myConn)
        '       With myCmd.Parameters
        '           .AddWithValue("@GUID",  varchar(36),
        '   @Inv_ID varchar(36),
        '   @Ordinal int,
        '   @Amount decimal(10,2),
        '   @DeptCode char(3),
        '   @ServCode char(4),
        '   @GLAcctCode char(5),
        '   @PayerCode char(6),
        '   @Person char(8),
        '   @DeptTitle char(3),
        '   @ServTitle varchar(60),
        '   @GLAcctTitle varchar(60),
        '   @PayerTitle varchar(60),
        '   @PersonTitle varchar(60),
        '   @User varchar(128)
        '       End With
        '       Return False
    End Function

    Public Function DeleteDetail(ByVal GUID As String) As Boolean
        Dim row As ZDetailRow
        For Each row In Me._DetailRows
            If row.GUID = GUID Then
                row.Delete()
            End If
        Next
        Return False
    End Function

    Public Function UpdateDetails(ByVal GUID As String, ByVal DeptCode As String, ByVal ServCode As String, _
            ByVal GLAcCode As String, ByVal PayrCode As String, ByVal PernCode As String) As Boolean
        ' TODO: dammit!
        Dim row As ZDetailRow
        For Each row In Me._DetailRows
            If row.GUID = GUID Then
                row.DeptartmentCode = DeptCode
                row.ServiceCode = ServCode
                row.GLAcct = GLAcCode
                row.PayerCode = PayrCode
                row.PersonCode = PernCode

                'FetchSageData()
            End If
        Next
        Return False
    End Function

    Private Function FetchSageData(ByVal Code As String, ByVal fldCode As String, ByVal fldTitle As String, _
            ByVal tblName As String) As String
        Dim strCN As String = ConfigurationManager.ConnectionStrings("SAGE_DBConnectionString").ConnectionString
        Dim cn As New SqlClient.SqlConnection
        Dim ds As New DataSet
        Dim dt As New DataTable

        cn.ConnectionString = strCN
        Dim cmd As New SqlClient.SqlCommand
        cmd.Connection = cn
        cmd.CommandType = CommandType.Text
        Dim strSQL As String = "SELECT [" & fldTitle & "] FROM [" & tblName & "] " & _
                "WHERE [" & fldCode & "] = '" & Code & "'"
        cmd.CommandText = strSQL
        Try
            cn.Open()
            cmd.ExecuteNonQuery()
            Dim da As New SqlDataAdapter(cmd)
            da.Fill(ds)
        Catch ex As Exception
        Finally
            cn.Close()
        End Try

        dt = ds.Tables(0)
        Dim txtItems As String = ""
        For Each row As DataRow In dt.Rows
            txtItems = row(fldTitle).ToString()
            If txtItems.Length > 0 Then Exit For
        Next
        Return txtItems
    End Function
End Class
