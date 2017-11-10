Partial Public Class unapprove
    Inherits ZPageSecure
    'Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim strGUID$ = ""
        Dim strIX$ = ""
        Try
            strGUID = Request("guid")
            strIX = Request("ix")
        Catch ex As Exception
            Response.Write("{ ""error"":1," & _
                    """message"":""GUID fetch:\n\n" & ex.Message & """ }")
            Exit Sub
        End Try
        Using oRT As New dsRequestDetailsTableAdapters.txnsTA
            oRT.SetHash("", strGUID)
            oRT.SetApproval(False, "", strGUID)
        End Using
        Try
            LogMessage("Approval revoked (" & strGUID & ")", strGUID, "", Me.GUID, Request)
        Catch ex As Exception
            ' do nothing
        End Try

        Response.Write("{ ""error"":0," & _
                """target"":" & strIX & "," & _
                """guid"":""" & strGUID & """," & _
                """message"":""Revoke OK:" & strGUID & """ }")
    End Sub

End Class