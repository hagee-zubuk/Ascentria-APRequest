Public Partial Class txn_dele
    Inherits ZPageSecure

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim strGUID As String
        Try
            strGUID = Request("guid")
        Catch ex As Exception
            Response.Write("{ ""error"":1," & _
                    """message"":""GUID fetch:\n\n" & ex.Message & """ }")
            Exit Sub
        End Try
        Try
            Using oRQ As New dsRequestDetailsTableAdapters.requestdetailsTableAdapter
                oRQ.DeleteByInvID(strGUID)
            End Using
            Using oTX As New dsRequestDetailsTableAdapters.txnsTA
                oTX.DeleteByGUID(strGUID)
            End Using
            Response.Write("{ ""error"":0," & _
                    """message"":""OK"" }")
            'Exit Sub
        Catch ex As Exception
            LogMessage("Failed to set delete" & vbCrLf & ex.Message, _
                    strGUID, "", Me.GUID, Request)
            Response.Write("{ ""error"":2," & _
                    """message"":""ERROR:" & ex.Message & """ }")
        End Try

    End Sub

End Class