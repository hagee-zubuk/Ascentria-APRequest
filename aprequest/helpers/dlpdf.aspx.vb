Imports System.IO
Imports System.Net
Imports System.Net.Mime
Imports System.Web.HttpResponse

Partial Public Class dlpdf
    Inherits ZPageSecure

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim strTxn = Trim(Request("txn") & "")
        If strTxn = "" Then
            Response.Write("The remote server returned an error: 404 Not found")
            Exit Sub
        End If
        Dim strUrl As String = My.Settings.PDFurl.ToString().Trim()
        Dim strFld$
        If IsNumeric(strTxn) Then
            strFld = "TXN-" & strTxn & ".pdf"
            strUrl &= "?txn=" & strTxn

            Dim result() As Byte
            Dim buffer(4097) As Byte
            Dim wr As WebRequest
            Dim wresp As WebResponse
            Dim respStream As Stream
            Dim memoryStream As New MemoryStream
            Dim count As Integer = 0

            Try
                wr = WebRequest.Create(strUrl)
                wresp = wr.GetResponse()
                respStream = wresp.GetResponseStream()

                Do
                    count = respStream.Read(buffer, 0, buffer.Length)
                    memoryStream.Write(buffer, 0, count)
                    If count = 0 Then Exit Do
                Loop While True

                result = memoryStream.ToArray

                If (result.Length > 0) Then
                    Dim cd As New ContentDisposition
                    cd.Inline = False
                    cd.FileName = strFld
                    Response.AppendHeader("Content-Disposition", cd.ToString())
                    Response.OutputStream.Write(result, 0, result.Length)
                    Response.Flush()
                    Response.End()
                    Response.Close()
                Else
                    Response.Write("Remote server error: 404 Not found")
                End If
            Catch ex As Exception
                Response.Write(ex.Message)
            End Try
        End If
    End Sub
End Class