Public Partial Class userdelete
    Inherits ZPageSecure

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim strGUID As String = Request("guid") & ""
        If strGUID = "" Then
            Response.Write("{ ""error"":1," & _
                """message"":""user id unspecified"" }")
            Exit Sub
        End If
        Dim intRet As Integer = -1
        Try
            Using oUsrs As New dsUsersTableAdapters.userconfigTA
                intRet = oUsrs.DeleteByGUID(strGUID)
            End Using
            Response.Write("{ ""error"":0," & _
                    """retval"":""" & intRet & """," & _
                    """message"":""delete successful: " & intRet & """ }")
        Catch ex As Exception
            Response.Write("{ ""error"":2," & _
                    """retval"":""" & intRet & """," & _
                    """message"":""" & ex.Message & """ }")
            Exit Sub
        End Try

    End Sub

End Class