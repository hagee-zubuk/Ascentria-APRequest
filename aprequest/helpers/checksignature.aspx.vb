Public Partial Class checksignature
    'Inherits System.Web.UI.Page
    Inherits ZPageSecure

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim strSig$ = Request("val")
        Dim oHash As New zHashes
        Using oUsrTA As New dsUsersTableAdapters.userconfigTA
            Dim strPhrase$ = oUsrTA.GetPhraseByGUID(Me.GUID)
            If oHash.VerifyMd5Hash(strSig, strPhrase) Then
                Response.Write("{ ""error"":0," & _
                    """message"":""ok"" }")
            Else
                Response.Write("{ ""error"":3," & _
                    """message"":""Invalid signing password"" }")
            End If
        End Using
    End Sub

End Class