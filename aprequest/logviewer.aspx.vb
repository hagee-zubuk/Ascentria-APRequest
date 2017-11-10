Imports System.Data
Imports System.Data.SqlClient

Partial Public Class logviewer
    Inherits ZPageSecure

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim cn As New SqlClient.SqlConnection
        Dim ds As New DataSet
        Dim dt As New DataTable

        Dim strCN As String = ConfigurationManager.ConnectionStrings("aprequestConnectionString").ConnectionString
        cn.ConnectionString = strCN
        Dim cmd As New SqlClient.SqlCommand

        cmd.Connection = cn
        cmd.CommandType = CommandType.Text
        Dim lngLimit& = 60

        Dim strSQL As String = "SELECT TOP " & lngLimit & " " & _
                "tstamp, ip_addr, username, reqstr, comment, txn_guid, txn_no " & _
                "FROM activity_log AS l " & _
                "LEFT JOIN transactions AS t ON l.txn_guid=t.guid " & _
                "WHERE [comment] NOT LIKE 'logon' " & _
                "ORDER BY [tstamp] DESC"
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

        Dim tcell As TableCell
        Dim outrow As TableRow
        Dim strLk1$, strLk2$
        Dim lngCnt& = 0
        Try
            dt = ds.Tables(0)
            Dim txtItem As String = ""
            For Each row As DataRow In dt.Rows
                outrow = New TableRow
                lngCnt = lngCnt + 1
                tcell = New TableCell
                Dim dtS As DateTime = row("tstamp")
                tcell.Text = "<div style=""font-size: 7pt;"">" & dtS.ToString("MM/dd/yyyy HH:mm:ss") & "</div>"
                tcell.VerticalAlign = VerticalAlign.Middle
                tcell.HorizontalAlign = HorizontalAlign.Left
                outrow.Cells.Add(tcell)

                tcell = New TableCell
                tcell.VerticalAlign = VerticalAlign.Top
                tcell.HorizontalAlign = HorizontalAlign.Left
                tcell.Text = row("username")
                outrow.Cells.Add(tcell)

                tcell = New TableCell
                tcell.VerticalAlign = VerticalAlign.Top
                tcell.HorizontalAlign = HorizontalAlign.Left
                strLk1 = row("comment")
                If strLk1.StartsWith("Export done: ") Then
                    Dim strParts() = Split(strLk1, ": ")
                    If UBound(strParts) >= 1 Then
                        strLk2 = strParts(1).ToString
                        tcell.Text = "Export Done: <a href=""" & ResolveUrl("~/helpers/dld.aspx") & _
                                "?Type=EXPORT&GUID=" & row("txn_guid") & "&FN=" & _
                                strLk2 & """>" & strLk2 & "</a>"
                    End If
                ElseIf strLk1.StartsWith("logon") Then
                    tcell.Text = strLk1
                Else
                    tcell.Text = strLk1 & " (TXN# " & row("txn_no") & ")"
                End If
                outrow.Cells.Add(tcell)

                tblLog.Rows.Add(outrow)
            Next
        Catch ex As Exception
            ' meh?
        End Try

    End Sub

End Class