Public Partial Class _default
    'Inherits System.Web.UI.Page
    'http://www.codeproject.com/Articles/417693/Insert-Update-Delete-in-ASP-NET-Gridview-DataSourc
    'Icons from:
    'http://dryicons.com/free-icons/preview/iconika-red-icons/
    Inherits ZPageSecure
    Private m_Hash As zHashes
    Private m_TxnNo As Integer

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim cookieName As String = FormsAuthentication.FormsCookieName
        Dim authCookie As HttpCookie = Request.Cookies(cookieName)
        Dim encryptedTicket As String = authCookie.Value
        Dim authTick As FormsAuthenticationTicket = FormsAuthentication.Decrypt(encryptedTicket)
        Dim uinfo() As String = authTick.UserData.Split("|")

        Me.Title = "New Request"
        m_Hash = New zHashes
        Me.txnGUID.Value = m_Hash.MkGUID.ToLower()
        Me.FiscalYr.Value = Date.Today.Year

        If Not Me.isRequester Then
            Response.Cookies("MSG").Value = "you do not have adequate rights to that page (REQ)"
            'Response.Redirect("~/logon.aspx")
            Response.Redirect("~/approval.aspx")
        End If

        If Me.IsMultisite Then
            Me.litHeader.Text = "<th style=""width: 80px;"" align=""center"" scope=""col"">Site</th>"
            Me.litBody.Text = "<td style=""width: 80px;"" align=""center"">" & _
                    "<input name=""txtASite"" type=""text"" id=""txtASite"" " & _
                    "class=""addbox"" clientidmode=""Static"" autocomplete=""off"" />" & _
                    "</td>"
            Me.blnMultiSite.Value = True
        Else
            Me.BigSiteName.Text = "<tr><td align=""right"">Site Name:</td>" & _
                    "<td colspan=""2""><input type=""text"" " & _
                    "name=""txtSiteCode"" id=""txtSiteCode"" " & _
                    "style=""width: 100%;""/></td><td><input " & _
                    "type=""text"" name=""code_site"" id=""code_site"" " & _
                    "readonly class=""align-center"" style=""width: 80px;"" " & _
                    "tabindex=""-1"" /></td></tr>"
            Me.blnMultiSite.Value = False
        End If
        If Not Request.Cookies("MSG") Is Nothing Then
            errorLabel.Value = Server.HtmlEncode(Request.Cookies("MSG").Value)
            'errorLabel.Visible = True
            Response.Cookies("MSG").Value = ""
        End If
        Using oReqTA As New dsRequestDetailsTableAdapters.txnsTA
            m_TxnNo = -1 'oReqTA.GetNxTxn()
        End Using
        ' TODO: update transaction number
        ' m_TxnNo = CInt(Math.Ceiling(Rnd() * 1000))
        If m_TxnNo < 0 Then
            Me.txnNumber.Value = "---"
        Else
            Me.txnNumber.Value = m_TxnNo
        End If
        Dim fldTrans As Label = CType(Master.FindControl("transaction"), Label)
        If Not fldTrans Is Nothing Then fldTrans.Text = Me.txnNumber.Value ' m_TxnNo

        ' Load the defaults for this user.
        Dim oUserDefault As dsUsers.userconfigRow = Nothing
        Using oUsTA As New dsUsersTableAdapters.userconfigTA
            Dim oUsDT As dsUsers.userconfigDataTable = oUsTA.GetDefaultsByGUID(Me.GUID)
            If oUsDT.Rows.Count > 0 Then
                oUserDefault = oUsDT(0)
            End If
        End Using
        Dim oUtil As New ZUtils
        Dim strDefaultsHash$ = "{""result"":0"
        If Not oUserDefault Is Nothing Then
            strDefaultsHash = "{""result"":1"
            'txtEntiCode, code_entity
            strDefaultsHash &= ",""entitycode"":""" & oUserDefault.entity_id & """"
            strDefaultsHash &= ",""entitytitle"":""" & oUtil.FetchSageTitle(oUserDefault.entity_id, "entity") & """"
            strDefaultsHash &= ",""regioncode"":""" & oUserDefault.region_id & """"
            strDefaultsHash &= ",""regiontitle"":""" & oUtil.FetchSageTitle(oUserDefault.region_id, "region") & """"
            strDefaultsHash &= ",""sitecode"":""" & oUserDefault.site_id & """"
            strDefaultsHash &= ",""sitetitle"":""" & oUtil.FetchSageTitle(oUserDefault.site_id, "site") & """"
        End If
        strDefaultsHash &= "}"
        Dim strFY$ = oUserDefault.fiscalyr

        Dim dicFY As Dictionary(Of String, String) = oUtil.FetchSageData("fy")
        ddFiscalYr.DataSource = dicFY
        ddFiscalYr.DataTextField = "Value"
        ddFiscalYr.DataValueField = "Key"
        ddFiscalYr.DataBind()
        ddFiscalYr.SelectedValue = strFY

        btnESig.ImageUrl = "~/e-sig.aspx?FN=" & Me.GUID

        btnSubmit.Visible = True ' not me.hasphrase
        lblInstru.Visible = False
        btnESig.Visible = True 'Not Me.HasPhrase
        sigPhrase1.Visible = Me.HasPhrase
        lblPhrase1.Visible = Me.HasPhrase
        defaulthash.Value = strDefaultsHash
    End Sub

End Class