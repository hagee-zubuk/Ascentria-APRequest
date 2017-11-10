Partial Public Class detailrow_del
    Inherits ZPageSecure
    'Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        'Dim CookieName As String = FormsAuthentication.FormsCookieName
        'Dim authCookie As HttpCookie = Request.Cookies(CookieName)
        'Dim encryptedTicket As String = authCookie.Value
        'Dim authTick = FormsAuthentication.Decrypt(encryptedTicket)
        'Dim Username = authTick.Name
        Dim strGUID As String

        Try
            strGUID = Request("guid")
        Catch ex As Exception
            Response.Write("{ ""error"":1," & _
                    """message"":""GUID fetch:\n\n" & ex.Message & """ }")
            Exit Sub
        End Try
        Dim oRD As New dsRequestDetailsTableAdapters.requestdetailsTableAdapter
        Try
            oRD.DeleteByGUID(strGUID)
        Catch ex As Exception
            Response.Write("{ ""error"":2," & _
                    """message"":""DB Sync:\n\n" & ex.Message & """ }")
            Exit Sub
        End Try
        Try
            LogMessage("Detail deleted(" & strGUID & ")", "", strGUID, Me.GUID, Request)
        Catch ex As Exception
            ' do nothing
        End Try

        Response.Write("{ ""error"":0," & _
        """message"":""delete OK:" & strGUID & """ }")
    End Sub

End Class