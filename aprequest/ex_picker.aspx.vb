Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Net
Imports System.Net.Mime

Partial Public Class ex_picker
    Inherits ZPageSecure
    Private m_strExtraSQL = ""

    Private Function FillEntityDropdown(ByRef tgt As DropDownList)
        Dim strCN As String = ConfigurationManager.ConnectionStrings("SAGE_DBConnectionString").ConnectionString
        Dim cn As New SqlClient.SqlConnection
        Dim ds As New DataSet
        Dim dt As New DataTable
        cn.ConnectionString = strCN
        Dim cmd As New SqlClient.SqlCommand
        cmd.Connection = cn
        cmd.CommandType = CommandType.Text
        Dim strSQL As String = "SELECT [sTitle], [sCodeID] FROM [tblAcctCode_1] " & _
                "ORDER BY [sTitle] ASC"
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
            tgt.Items.Add(New ListItem(row("sCodeID").ToString() & " - " & row("sTitle").ToString(), row("sCodeID").ToString()))
        Next
        Return txtItems
    End Function

    Private Function FillRegionDropdown(ByRef tgt As DropDownList)
        Dim strCN As String = ConfigurationManager.ConnectionStrings("SAGE_DBConnectionString").ConnectionString
        Dim cn As New SqlClient.SqlConnection
        Dim ds As New DataSet
        Dim dt As New DataTable
        cn.ConnectionString = strCN
        Dim cmd As New SqlClient.SqlCommand
        cmd.Connection = cn
        cmd.CommandType = CommandType.Text
        Dim strSQL As String = "SELECT [sTitle], [sCodeID] FROM [tblAcctCode_2] " & _
                "ORDER BY [sTitle] ASC"
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
            tgt.Items.Add(New ListItem(row("sCodeID").ToString() & " - " & row("sTitle").ToString(), row("sCodeID").ToString()))
        Next
        Return txtItems
    End Function

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.Toplinks1.GUID = Me.GUID
        Me.datedisp.Text = Format(Date.Now, "ddd MMM d, yyyy h:mmtt")

        If Not IsPostBack Then
            Dim zTExt As Dictionary(Of String, String) = GetTxnExtent()
            ddMonth.DataSource = zTExt
            ddMonth.DataTextField = "value"
            ddMonth.DataValueField = "Key"
            ddMonth.DataBind()

            Me.ddEntity.Items.Add(New ListItem("All", ""))
            FillEntityDropdown(Me.ddEntity)
            Me.ddRegion.Items.Add(New ListItem("Any", ""))
            FillRegionDropdown(Me.ddRegion)
        End If
    End Sub

    Private Sub btnGo_Command(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.CommandEventArgs) Handles btnGo.Command
        Dim strYrMo$ = String.Empty
        'For Each lsi As ListItem In ddMonth.Items
        '    If lsi.Selected Then strYrMo = lsi.Value
        'Next
        strYrMo = ddMonth.SelectedItem.Value

        Dim strYr$ = Mid(strYrMo, 1, 4)
        Dim strMo$ = Mid(strYrMo, 5, 2)

        Dim strEnty$ = Me.ddEntity.SelectedItem.Value
        Dim strRegn$ = Me.ddRegion.SelectedItem.Value
        Dim strQS$ = "mo=" & strMo & "&yr=" & strYr
        If strEnty <> "" Then strQS &= "&en=" & strEnty
        If strRegn <> "" Then strQS &= "&rn=" & strRegn
        Response.Redirect("~/exporter2.aspx?" & strQS)
    End Sub
End Class