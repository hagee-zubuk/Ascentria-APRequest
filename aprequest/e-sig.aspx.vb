Imports System.IO
Imports System.IO.File

Partial Public Class e_sig
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim strFN As String = Request("FN")
        Dim strMime As String = "application/octet-stream"
        Dim blnImgOK As Boolean = False
        strMime = "image/jpeg"
        Response.ContentType = strMime
        If strFN <> "" Then
            Dim strConn$ = ConfigurationManager.ConnectionStrings("aprequestConnectionString").ConnectionString
            Dim strSQL$ = "SELECT [sig] FROM [userconfig] WHERE [guid]=@guid"
            Dim oConn As New SqlClient.SqlConnection(strConn)
            oConn.Open()
            Try
                Dim oCmd As New SqlClient.SqlCommand(strSQL, oConn)
                oCmd.Parameters.AddWithValue("@guid", strFN)
                Dim oRead As SqlClient.SqlDataReader = oCmd.ExecuteReader()
                oRead.Read()
                If oRead.HasRows Then
                    Response.BinaryWrite(oRead(0))
                    blnImgOK = True
                End If
            Catch ex As Exception
                blnImgOK = False
            Finally
                oConn.Close()
            End Try
            oConn.Dispose()
        End If

        If Not blnImgOK Then
            strFN = Request.MapPath(Request.ApplicationPath & "/images/sig.png")
            Response.WriteFile(strFN)
        End If

        Response.End()
    End Sub

End Class