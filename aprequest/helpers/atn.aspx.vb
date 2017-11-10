Imports System.Drawing
Imports System.IO

Partial Public Class atn
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim strGUID$ = Request("GUID") & ""
        Dim strTmp = Request("rs") & ""
        Dim intRows As Integer = Convert.ToInt32(strTmp)
        Dim strMime As String = "application/octet-stream"
        Dim blnImgOK As Boolean = False
        Dim strFN$
        Response.ClearHeaders()
        Response.ClearContent()
        Response.ContentType = strMime
        If strGUID <> "" And intRows > 0 Then
            Dim strConn$ = ConfigurationManager.ConnectionStrings("aprequestConnectionString").ConnectionString
            Dim strSQL$ = "SELECT [upload], [ofn] FROM [transactions] WHERE [guid]=@guid"
            'Dim strUpl$
            Dim oConn As New SqlClient.SqlConnection(strConn)
            oConn.Open()
            Try
                Dim oCmd As New SqlClient.SqlCommand(strSQL, oConn)
                'Dim strOut$
                oCmd.Parameters.AddWithValue("@guid", strGUID)
                Dim oRead As SqlClient.SqlDataReader = oCmd.ExecuteReader()
                oRead.Read()
                If oRead.HasRows Then
                    strFN = oRead("ofn")
                    Dim imgByte As Byte() = DirectCast(oRead("upload"), Byte())
                    Dim imgStream As MemoryStream
                    Dim intW&, intH&
                    imgStream = New MemoryStream(imgByte)
                    Dim sigImg1 As Image, sigImgFinal As Image
                    sigImg1 = Image.FromStream(imgStream)
                    intRows *= 36
                    intH = sigImg1.Height
                    intW = sigImg1.Width
                    If intH > intRows Then
                        intH = intRows
                        intW = Convert.ToInt64(140 * sigImg1.Width / sigImg1.Height)
                    End If
                    If intW > 80 Then
                        intW = 80
                    End If
                    sigImgFinal = New Bitmap(sigImg1, intW, intH)
                    Dim CropRect As New Rectangle(0, 0, 80, intH)
                    Dim CropImage As Image = New Bitmap(CropRect.Width, CropRect.Height)
                    Using grp = Graphics.FromImage(CropImage)
                        grp.DrawImage(sigImgFinal, New Rectangle(0, 0, CropRect.Width, CropRect.Height), CropRect, GraphicsUnit.Pixel)
                        sigImgFinal.Dispose()
                    End Using
                    sigImg1.Dispose()
                    Image2Byte(CropImage, imgByte)
                    ' output to browser
                    blnImgOK = True
                    Response.AddHeader("content-disposition", "attachment; filename=" & strFN)
                    Response.BinaryWrite(imgByte)
                End If
                oRead.Close()
                oCmd.Dispose()
            Catch ex As Exception
                ' EEEP!
            End Try
            oConn.Close()
            oConn.Dispose()
        End If
        If Not blnImgOK Then
            strFN = Request.MapPath(Request.ApplicationPath & "/images/redpaperclip.png")
            Response.WriteFile(strFN)
        End If
            Response.End()

    End Sub

End Class