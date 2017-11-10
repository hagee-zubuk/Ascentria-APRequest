Imports System.IO
Imports System.Net

Partial Public Class ProcPDF
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Private Sub ContactPDFRenderer()
        Dim request As WebRequest = WebRequest.Create("http://webserv6/pdfrender")
        Dim response As WebResponse = request.GetResponse()
        Dim data As Stream = response.GetResponseStream()

        Dim html As String = String.Empty

        Using sr As New StreamReader(data)
            html = sr.ReadToEnd
        End Using
    End Sub

End Class