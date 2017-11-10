Public Partial Class detailrow_updateamount
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim CookieName As String = FormsAuthentication.FormsCookieName
        Dim authCookie As HttpCookie = Request.Cookies(CookieName)
        Dim encryptedTicket As String = authCookie.Value
        Dim authTick = FormsAuthentication.Decrypt(encryptedTicket)
        Dim Username = authTick.Name
        Dim strGUID$, strLastVal$, strVal$
        Dim strIVGuid$, strResponse$

        strGUID = Request("guid")
        strVal = Request("amnt").Replace(",", "")
        strLastVal = ""
        strIVGuid = ""
        If strGUID = "" Then Response.Write("")

        Dim oRQTA As New dsRequestDetailsTableAdapters.requestdetailsTableAdapter
        Try
            Dim oRqD As dsRequestDetails.requestdetailsDataTable = oRQTA.GetByGUID(strGUID)
            Dim oReqDetail As dsRequestDetails.requestdetailsRow = oRqD.Rows(0)
            strLastVal = FormatNumber(oReqDetail.Amount, 2, TriState.True, TriState.False, TriState.True)
            strIVGuid = oReqDetail.Inv_ID
        Catch ex As Exception
            Response.Write(strLastVal)
        End Try
        Try
            oRQTA.UpdateAmntByGUID(Convert.ToDecimal(strVal), strGUID)
            oRQTA.RefreshTotals(strIVGuid)
            strResponse = FormatNumber(strVal, 2, TriState.True, TriState.False, TriState.True)
        Catch ex As Exception
            strResponse = strLastVal
        End Try
        Response.Write(strResponse)
    End Sub

End Class