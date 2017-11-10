Imports System.Data
Imports System.Data.SqlClient

Partial Public Class SageDBProxy
    Inherits System.Web.UI.Page

    Private cn As New SqlClient.SqlConnection
    Private ds As New DataSet
    Private dt As New DataTable

    Private Function FetchVendorData(ByVal prefixText As String) As String
        Dim strCN As String = ConfigurationManager.ConnectionStrings("SAGE_DBConnectionString").ConnectionString
        cn.ConnectionString = strCN
        Dim cmd As New SqlClient.SqlCommand
        cmd.Connection = cn
        cmd.CommandType = CommandType.Text
        Dim strSQL As String = "SELECT [sName], [sVendorID], [sMainPrintedAddress] FROM [tblAPVendor] " & _
                "WHERE ([sName] LIKE @name OR [sVendorID] LIKE @code) " & _
                "AND [sStatus]='A' "
        cmd.CommandText = strSQL
        cmd.Parameters.AddWithValue("@name", "%" & prefixText & "%")
        cmd.Parameters.AddWithValue("@code", "%" & prefixText & "%")
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
        Dim cnt As Integer = 0
        For Each row As DataRow In dt.Rows
            If row("sMainPrintedAddress").ToString().Length > 2 Then
                txtItems &= "<li style=""height: 40px;"" id=""" & row("sVendorID").ToString() & """>" & _
                        row("sName").ToString() & "[" & row("sVendorID").ToString() & "]" & _
                        "&nbsp;--&nbsp;" & row("sMainPrintedAddress").ToString()
            Else
                txtItems &= "<li id=""" & row("sVendorID").ToString() & """>" & _
                        row("sName").ToString() & " [" & row("sVendorID").ToString() & "]"
            end if
            txtItems &= "</li>" & vbCrLf
            cnt += 1
            If (cnt > 15) then
                txtItems &= "<li style=""text-decoration: italic;"" id="""">&lt;...more...&gt;</li>"
                Exit For
            End If
        Next
        If cnt = 0 then
            txtItems = "<li style=""text-decoration: italic;"" id="""">&lt;...no matches...&gt;</li>"
        End If
        Return txtItems
    End Function


    Private Function FetchTableData(ByVal prefixText As String, _
            ByVal tblName As String, Optional ByVal showCode As Boolean = True, _
            Optional ByVal fld1 As String = "sTitle", _
            Optional ByVal fld2 As String = "sCodeID") As String
        Dim strCN As String = ConfigurationManager.ConnectionStrings("SAGE_DBConnectionString").ConnectionString
        cn.ConnectionString = strCN
        Dim cmd As New SqlClient.SqlCommand
        cmd.Connection = cn
        cmd.CommandType = CommandType.Text
        Dim strSQL As String = "SELECT [" & fld1 & "], [" & fld2 & "] FROM [" & tblName & "] " & _
                "WHERE ([" & fld1 & "] LIKE @name OR [" & fld2 & "] LIKE @code)"
        If tblName = "tblAPVendor" Then
            strSQL &= " AND [sStatus]='A' "
        End If
        cmd.CommandText = strSQL
        'cmd.Parameters.AddWithValue("@name", prefixText & "%")
        'cmd.Parameters.AddWithValue("@code", prefixText & "%")
        cmd.Parameters.AddWithValue("@name", "%" & prefixText & "%")
        cmd.Parameters.AddWithValue("@code", "%" & prefixText & "%")
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
        Dim cnt As Integer = 0
        For Each row As DataRow In dt.Rows
            txtItems &= "<li id=""" & row(fld2).ToString() & """>" & _
                    row(fld1).ToString()
            If showCode Then txtItems &= " [<span class=""idElem"">" & row(fld2).ToString() & "</span>]"
            cnt += 1
            txtItems &= "</li>" & vbCrLf
            If (cnt > 30) then
                txtItems &= "<li style=""text-decoration: italic;"" id="""">&lt;...more...&gt;</li>"
                Exit For
            End If
        Next
        If cnt = 0 then
            txtItems = "<li style=""text-decoration: italic;"" id="""">&lt;no matches!&gt;</li>"
        End If
        Return txtItems
    End Function

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim strType As String = Request("type").ToLower()
        Dim strPref As String = Request("value")
        Dim strRet As String

        Select Case strType
            Case "entity"
                strRet = FetchTableData(strPref, "tblAcctCode_1")
            Case "region"
                strRet = FetchTableData(strPref, "tblAcctCode_2")
            Case "site"
                strRet = FetchTableData(strPref, "tblAcctCode_10")
            Case "vendor"
                strRet = FetchVendorData(strPref)
            Case "dept"
                strRet = FetchTableData(strPref, "tblAcctCode_5")
            Case "serv"
                strRet = FetchTableData(strPref, "tblAcctCode_4")
            Case "glac"
                strRet = FetchTableData(strPref, "tblAcctCode_0")
            Case "payr"
                strRet = FetchTableData(strPref, "tblAcctCode_3")
            Case "pern"
                strRet = FetchTableData(strPref, "tblAcctCode_9")
            Case Else
                strRet = "<li>-</li>"
        End Select

        Response.Write("<ul>")
        Response.Write(strRet)
        Response.Write("</ul>")
    End Sub

End Class