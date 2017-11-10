Public Partial Class editreq
    Inherits ZPageSecure
    Private m_Hash As zHashes
    Private m_GUID As String
    Private m_TxnNo As Integer
    Public strDetailTable$

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim cookieName As String = FormsAuthentication.FormsCookieName
        Dim authCookie As HttpCookie = Request.Cookies(cookieName)
        Dim encryptedTicket As String = authCookie.Value
        Dim authTick As FormsAuthenticationTicket = FormsAuthentication.Decrypt(encryptedTicket)
        Dim uinfo() As String = authTick.UserData.Split("|")

        m_Hash = New zHashes
        m_GUID = Request("LDTXN")
        Dim oTxnTA As New dsRequestDetailsTableAdapters.txnsTA
        Dim oTxnDT As dsRequestDetails.txnsDataTable = oTxnTA.GetByGUID(m_GUID)
        If oTxnDT.Count <= 0 Then
            Response.Cookies("MSG").Value = "That request was not found."
            If Request.ServerVariables("ReferrerURL") Is Nothing Then
                Response.Redirect("~/default.aspx")
            Else
                Response.Redirect(Request.ServerVariables("ReferrerURL"))
            End If
        End If
        Dim oTxnInfo As dsRequestDetails.txnsRow = oTxnDT(0)

        Dim blnMgr As Boolean = Me.IsManager
        Dim blnAdm As Boolean = Me.IsAdmin
        Dim blnApv As Boolean = Me.IsApprover
        If Not blnApv Then ' see if i can approve it anyway?
            blnApv = Me.CanApprove(oTxnInfo.create_user)
        End If
        If Not (oTxnInfo.create_user = Me.UserName Or blnMgr Or blnAdm Or blnApv) Then 'me.isrequester
            Response.Cookies("MSG").Value = "You [" & Me.GUID & "] do NOT have adequate rights to that page: " & _
                    "MGR: " & blnMgr & ", ADM: " & blnAdm & ", APV: " & blnApv
            Response.Redirect("~/logon.aspx")
        End If

        Me.btnSubmit.Visible = Me.IsRequester Or Me.IsAdmin Or Me.IsManager

        If Not Request.Cookies("MSG") Is Nothing Then
            errorLabel.Value = Server.HtmlEncode(Request.Cookies("MSG").Value)
            'errorLabel.Visible = True
            Response.Cookies("MSG").Value = ""
        End If

        ' TODO: update transaction number
        ' m_TxnNo = CInt(Math.Ceiling(Rnd() * 1000))
        Me.txnNumber.Value = oTxnInfo.txn_no ' 
        m_TxnNo = oTxnInfo.txn_no ' 
        Dim fldTrans As Label = CType(Master.FindControl("transaction"), Label)
        If Not fldTrans Is Nothing Then fldTrans.Text = m_TxnNo

        If oTxnInfo.ismultisite Then
            Me.litHeader.Text = "<th style=""width: 80px;"" align=""center"" scope=""col"">Site</th>"
            Me.litBody.Text = "<td style=""width: 80px;"" align=""center"">" & _
                    "<input name=""txtASite"" type=""text"" id=""txtASite"" " & _
                    "class=""addbox"" clientidmode=""Static"" autocomplete=""off"" />" & _
                    "</td>"
        Else
            Me.BigSiteName.Text = "<tr><td align=""right"">Site Name:</td>" & _
                    "<td colspan=""2""><input type=""text"" " & _
                    "name=""txtSiteCode"" id=""txtSiteCode"" " & _
                    "style=""width: 100%;""/></td><td><input " & _
                    "type=""text"" name=""code_site"" id=""code_site"" " & _
                    "readonly class=""align-center"" style=""width: 80px;"" " & _
                    "tabindex=""-1"" /></td></tr>"
        End If

        Dim oUtil As New ZUtils
        Dim dicFY As Dictionary(Of String, String) = oUtil.FetchSageData("fy")
        ddFiscalYr.DataSource = dicFY
        ddFiscalYr.DataTextField = "Value"
        ddFiscalYr.DataValueField = "Key"
        ddFiscalYr.DataBind()

        If oTxnInfo.hash = "approved" Then
            Me.btnApprove.Visible = False
            litApproved.Text = oUtil.FetchApprovalInfo(m_GUID)
        Else
            Me.btnApprove.Visible = (Me.UserName <> oTxnInfo.create_user) And (Me.IsManager Or Me.IsAdmin Or Me.IsApprover)
        End If


        Dim strDefaultsHash$ = "{""result"":0"
        'If Not oUserDefault Is Nothing Then
        With oTxnInfo
            strDefaultsHash = "{""result"":1"
            ' txtEntiCode, code_entity
            strDefaultsHash &= ",""entitycode"":""" & .entity_id & """"
            strDefaultsHash &= ",""entitytitle"":""" & oUtil.FetchSageTitle(.entity_id, "entity") & """"
            strDefaultsHash &= ",""regioncode"":""" & .region_id & """"
            strDefaultsHash &= ",""regiontitle"":""" & oUtil.FetchSageTitle(.region_id, "region") & """"
            If .Issite_idNull Then
                strDefaultsHash &= ",""sitecode"":"""""
                strDefaultsHash &= ",""sitetitle"":"""""
            Else
                strDefaultsHash &= ",""sitecode"":""" & .site_id & """"
                strDefaultsHash &= ",""sitetitle"":""" & oUtil.FetchSageTitle(.site_id, "site") & """"
            End If
            strDefaultsHash &= ",""vendortitle"":""" & oUtil.FetchSageTitle(.vendor_id, "vendor") & """"
            strDefaultsHash &= ",""vendorcode"":""" & .vendor_id & """"
            strDefaultsHash &= ",""invoice_no"":""" & .invoice_no & """"
            strDefaultsHash &= ",""invoice_dt"":""" & Format(.invoice_dt, "MM/dd/yyyy") & """"
            strDefaultsHash &= ",""due_dt"":""" & Format(.due_dt, "MM/dd/yyyy") & """"
            If .IsnoteNull Then
                strDefaultsHash &= ",""note"":"""""
            Else
                strDefaultsHash &= ",""note"":""" & .note & """"
            End If
            If .Issp_notesNull then
                strDefaultsHash &= ",""spnote"":"""""
            Else
                strDefaultsHash &= ",""spnote"":""" & .sp_notes & """"
            End If

            strDefaultsHash &= ",""totalamnt"":"""
            If Not .Istotal_amountNull Then
                strDefaultsHash &= FormatNumber(.total_amount, 2, TriState.True, TriState.False, TriState.False)
            Else
                strDefaultsHash &= "0"
            End If
            strDefaultsHash &= """}"
            If Not .Isfy_idNull Then
                ddFiscalYr.SelectedValue = .fy_id
                Me.FiscalYr.Value = .fy_id  'Date.Today.Year
            End If
            txnGUID.Value = .guid
            txnNumber.Value = .txn_no
        End With
        ' NOW LOAD THE DETAIL ROWS, FOCRYIN' OUT LOUD!
        strDetailTable = ""
        Using oTxnDetailTA As New dsRequestDetailsTableAdapters.requestdetailsTableAdapter
            Dim oDetDT As dsRequestDetails.requestdetailsDataTable = oTxnDetailTA.GetDataByInvoiceID(m_GUID)
            If oDetDT.Count > 0 Then
                Dim oDetRow As dsRequestDetails.requestdetailsRow
                For Each oDetRow In oDetDT
                    '<tr class="gridRow"><th style="width: 60px;">&nbsp;</th>
                    strDetailTable &= "<tr id=""trow_" & oDetRow.Ordinal & """ class=""gridRow"">" & _
                            "<td style=""text-align: center;"">"
                    strDetailTable &= "<input type=""hidden"" id=""drg_" & oDetRow.Ordinal & """ value=""" & _
                            oDetRow.GUID & """><img src=""images/recycle_bin.png"" alt=""del"" " & _
                            "title=""delete this detail"" id=""del_" & oDetRow.Ordinal & """ " & _
                            "onclick=""hndDelRow(this)""; /></th>"
                    If oTxnInfo.ismultisite Then
                        strDetailTable &= "<td>" & oUtil.FetchSageTitle(oDetRow.site_id, "site") & "</td>"
                    End If
                    strDetailTable &= "<td>" & oDetRow.DeptTitle & "</td>" & _
                            "<td>" & oDetRow.ServTitle & "</td>" & _
                            "<td>" & oDetRow.GLAcctTitle & "</td>" & _
                            "<td>" & oDetRow.PayerTitle & "</td>" & _
                            "<td>" & oDetRow.PersonTitle & "</td>" & _
                            "<td align=""right"">" & oDetRow.Amount & "</td>" & _
                            "<td>&nbsp;</td></tr>"
                Next
            End If
        End Using
        Me.txnGUID.Value = m_GUID

        Me.DetailsTable.Text = strDetailTable
        btnSubmit.Visible = True ' not me.hasphrase
        defaulthash.Value = strDefaultsHash
    End Sub


    Private Sub btnApprove_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnApprove.Click
        Using oTxnTA As New dsRequestDetailsTableAdapters.txnsTA
            If Me.txnGUID.Value = m_GUID Then
                oTxnTA.SetHash("approved", m_GUID)
                oTxnTA.SetApproval(True, Me.UserName, m_GUID)
                Me.btnApprove.Visible = False
                Dim oUtil As New ZUtils
                litApproved.Text = oUtil.FetchApprovalInfo(m_GUID)
            End If
        End Using
    End Sub
End Class