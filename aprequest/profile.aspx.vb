Imports System.Drawing
Imports System.IO

Partial Public Class profile
    Inherits ZPageSecure
    Private m_Hash As zHashes

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim cookieName As String = FormsAuthentication.FormsCookieName
        Dim authCookie As HttpCookie = Request.Cookies(cookieName)
        Dim encryptedTicket As String = authCookie.Value
        Dim authTick As FormsAuthenticationTicket = FormsAuthentication.Decrypt(encryptedTicket)
        Dim uinfo() As String = authTick.UserData.Split("|")

        btnESig.ImageUrl = "~/e-sig.aspx?FN=" & Me.GUID

        m_Hash = New zHashes
        Dim fldTrans As Label = CType(Master.FindControl("transaction"), Label)
        If Not fldTrans Is Nothing Then fldTrans.Text = "n/a"
        'debug.Text = "Expires: " & authTick.Expiration & "<br />" & _
        '        "Issued: " & authTick.IssueDate & "<br />" & _
        '        "Data: " & authTick.UserData & "<br />"
        Master.FindControl("transaction").Visible = False

        If Not IsPostBack Then
            ' Validate initially to force the asterisks
            ' to appear before the first roundtrip.
            ' Validate()
        End If

    End Sub

    Protected Sub btnGo_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnGo.Click
        Dim oUsrTA As New dsUsersTableAdapters.userconfigTA
        Dim strMesg$ = ""
        Dim oUsrZZ As dsUsers.userconfigDataTable = oUsrTA.GetByGUID(Me.GUID)
        Dim oUsrRw As dsUsers.userconfigRow
        If oUsrZZ.Count > 0 Then
            oUsrRw = oUsrZZ(0)
        Else
            oUsrZZ.Dispose()
            oUsrTA.Dispose()
            Exit Sub
        End If
        Dim strConn$ = ConfigurationManager.ConnectionStrings("aprequestConnectionString").ConnectionString
        Dim oConn As New SqlClient.SqlConnection(strConn)

        If flub1.HasFile Then
            Dim strFolder$ = My.Settings.UploadPath
            Dim strTmpFile$ = System.IO.Path.Combine(strFolder, m_Hash.MkGUID)
            Try
                Dim img As FileUpload = flub1
                Dim imgByte As Byte()
                Dim sigImg1 As Image, sigImgFinal As Image
                Dim imgFile As HttpPostedFile = flub1.PostedFile
                imgByte = New Byte(imgFile.ContentLength - 1) {}
                imgFile.InputStream.Read(imgByte, 0, imgFile.ContentLength)
                Dim imgStream As MemoryStream
                imgStream = New MemoryStream(imgByte)
                sigImg1 = Image.FromStream(imgStream)
                If sigImg1.Width > 500 Or sigImg1.Height > 240 Then
                    Dim intW&, intH&
                    If sigImg1.Width > 500 Then
                        intW = 500
                        intH = Convert.ToInt64(500 * sigImg1.Height / sigImg1.Width)
                    Else
                        intW = sigImg1.Width
                        intH = sigImg1.Height
                    End If
                    If intH > 140 Then
                        intH = 140
                        intW = Convert.ToInt64(140 * sigImg1.Width / sigImg1.Height)
                    End If
                    sigImgFinal = New Bitmap(sigImg1, intW, intH)
                    Image2Byte(sigImgFinal, imgByte)
                End If
                ' insert signature into db
                oConn.Open()
                Dim strSQL = "UPDATE [userconfig] SET [sig]=@sig WHERE [guid]=@guid"
                Dim oCmd As New SqlClient.SqlCommand(strSQL, oConn)
                oCmd.Parameters.AddWithValue("@guid", Me.GUID)
                oCmd.Parameters.AddWithValue("@sig", imgByte)
                Dim iRet As Integer = Convert.ToInt32(oCmd.ExecuteNonQuery())
                oCmd.Dispose()
                strMesg = "Signature graphic updated" & "<br />" & vbCrLf
            Catch ex As Exception
                strMesg = "ERROR: " & ex.Message.ToString() & "<br />" & vbCrLf
            Finally
                oConn.Close()
            End Try
        Else
            ' okay, so no files were uploaded, now what?

        End If
        Dim strSig$ = sigPhrase1.Text
        If strSig.Length > 0 Then
            Dim strMD5$ = m_Hash.GetMd5Hash(strSig)
            oConn.Open()
            Dim strSQL = "UPDATE [userconfig] SET [phrase]=@sig WHERE [guid]=@guid"
            Try
                Dim oCmd As New SqlClient.SqlCommand(strSQL, oConn)
                oCmd.Parameters.AddWithValue("@guid", Me.GUID)
                oCmd.Parameters.AddWithValue("@sig", strMD5)
                Dim iRet As Integer = Convert.ToInt32(oCmd.ExecuteNonQuery())
                oCmd.Dispose()
            Catch ex As Exception
                strMesg = "ERROR: " & ex.Message.ToString() & "<br />" & vbCrLf
            Finally
                oConn.Close()
            End Try
            strMesg = "Signing phrase updated"
        End If
        oConn.Dispose()
        Response.Cookies("MSG").Value = strMesg
        Response.Redirect("~/default.aspx")
    End Sub
End Class