Imports System.IO
Imports System.IO.File
Imports System.Net.Mime

Partial Public Class dld
    'Inherits System.Web.UI.Page
    Inherits ZPageSecure

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        'If Me.IsApprover Or Me.IsAdmin Then
            Dim strGUID$ = Request("GUID")
            Dim strType$ = Request("Type") & ""
            Dim strMime As String = "application/octet-stream"
            Response.ClearHeaders()
            Response.ClearContent()
            Response.ContentType = strMime
            If strGUID <> "" And strType = "" Then
                Dim strConn$ = ConfigurationManager.ConnectionStrings("aprequestConnectionString").ConnectionString
                Dim strSQL$ = "SELECT [upload], [ofn] FROM [transactions] WHERE [guid]=@guid"
                Dim strFN$
                Dim oConn As New SqlClient.SqlConnection(strConn)
                oConn.Open()
                Try
                    Dim oCmd As New SqlClient.SqlCommand(strSQL, oConn)
                    oCmd.Parameters.AddWithValue("@guid", strGUID)
                    Dim oRead As SqlClient.SqlDataReader = oCmd.ExecuteReader()
                    oRead.Read()
                If oRead.HasRows Then
                    strFN = oRead("ofn")
                    Response.AddHeader("Content-Disposition", "attachment; filename=""" & strFN & """")
                    Response.BinaryWrite(oRead("upload"))
                End If
                    oRead.Close()
                    oCmd.Dispose()
                Catch ex As Exception
                    ' EEEP!
                End Try
                oConn.Close()
                oConn.Dispose()
            ElseIf strGUID <> "" And strType = "EXPORT" Then
                Dim strDLFdr$ = My.Settings.DownloadPath
                Dim strPath = Path.Combine(strDLFdr, strGUID) & ".csv"
                If File.Exists(strPath) = False Then
                    Response.Write("<h1>Error!</h1>That export file does not exist or has been purged")
                Else
                    Dim strFN = Request("FN") & ""
                    Dim dtTmp As DateTime = Now
                    If strFN = "" Then
                        dtTmp = File.GetLastWriteTime(strPath)
                    End If
                    strFN = dtTmp.ToString("yyyyMMddHHmm") & ".csv"
                    Dim cd As New ContentDisposition
                    cd.Inline = False
                    cd.FileName = strFN '"export.xls"
                    Response.AppendHeader("Content-Disposition", cd.ToString())
                    Dim filedata() As Byte = File.ReadAllBytes(strPath)
                    Response.OutputStream.Write(filedata, 0, filedata.Length)
                    'Response.Cookies("MSG").Value = "export done"
                    Response.Flush()
                    Response.End()
                    Response.Close()
                End If
            End If
            Response.End()
        'Else
        '    Response.Write("<h1>Error!</h1>Insufficient privileges for the selected action.<br /><br />Please contact Technical Support")
        'End If
    End Sub
End Class