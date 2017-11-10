Imports System.IO
Imports System.Net.Mime

Partial Public Class approval
    Inherits ZPageSecure
    ' WAS: Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        'If Not Me.IsApprover Then
        '    Response.Cookies("MSG").Value = "you do not have adequate rights to that page"
        '    Response.Redirect("~/default.aspx")
        'End If
        'lnkAdmin.Visible = Me.IsAdmin
        Me.Toplinks1.GUID = Me.GUID
        Me.datedisp.Text = Format(Date.Now, "ddd MMM d, yyyy h:mmtt")
        Me.usrGUID.Value = Me.GUID
        Dim oTxns As New dsRequestDetailsTableAdapters.txnsTA
        Dim oDets As New dsRequestDetailsTableAdapters.requestdetailsTableAdapter
        Dim tblTxns As dsRequestDetails.txnsDataTable
        Dim tblDets As dsRequestDetails.requestdetailsDataTable
        Dim oTrans As dsRequestDetails.txnsRow
        Dim oDetls As dsRequestDetails.requestdetailsRow
        Dim oUtil As New ZUtils
        Dim lngCnt As Long = 0, lngDRW As Long = 0

        ulink.Text = Me.DisplayName
        ulink.PostBackUrl = "~/profile.aspx?guid=" & Me.GUID

        If Not Request.Cookies("MSG") Is Nothing Then
            errorLabel.Value = Server.HtmlEncode(Request.Cookies("MSG").Value)
            'errorLabel.Visible = True
            Response.Cookies("MSG").Value = ""
        End If

        Try
            Dim tcell As TableCell
            Dim outrow As TableRow
            Dim strLk1$, strLk2$
            oTxns.FixTransactionTotals()
            Dim blnLikeAdmin As Boolean = Me.IsAdmin Or Me.IsFinance
            If blnLikeAdmin Then
                tblTxns = oTxns.GetAllToApprove_Adm()
            Else
                tblTxns = oTxns.GetAllToApprove(Me.UserName)
            End If


            For Each oTrans In tblTxns.Rows
                Dim blnHold As Boolean = False
                blnHold = oTrans.hold
                outrow = New TableRow
                lngCnt = lngCnt + 1
                'If lngCnt > 5 Then Exit For
                tcell = New TableCell
                If oTrans.create_user = Me.UserName Or blnLikeAdmin Then
                    If blnHold Then
                        tcell.Text = "<div style=""color:#a37;"">on&nbsp;<br />hold&nbsp;</div>"
                    ElseIf oTrans.hash = "approved" Then
                        tcell.Text = "<img align=""top"" height=""16px"" id=""img_" & _
                                lngCnt & """ " & "src=""images/star.png"" alt=""ok"" " & _
                                "title=""approved!"" />"
                    Else
                        tcell.Text = "<div style=""color:#3a7;"">approval&nbsp;<br />pending&nbsp;</div>"
                    End If
                    tcell.HorizontalAlign = HorizontalAlign.Right
                    tcell.VerticalAlign = VerticalAlign.Middle
                    outrow.Cells.Add(tcell)
                ElseIf oTrans.hash = "approved" Then
                    tcell.Text = "<a href=""#"" onclick=""flag_unapprove('" & oTrans.guid & _
                            "', " & lngCnt & ")""><img align=""top"" height=""16px"" id=""img_" & _
                            lngCnt & """ " & "src=""images/star.png"" alt=""ok"" title=""approved! " & _
                            "click to disapprove"" /></a><input type=""checkbox"" id=""txn" & lngCnt & _
                            """ name=""txn" & lngCnt & """ value=""" & oTrans.guid & """ " & _
                            " style=""display: none;"" />"
                    tcell.HorizontalAlign = HorizontalAlign.Right
                    tcell.VerticalAlign = VerticalAlign.Middle
                    outrow.Cells.Add(tcell)
                Else
                    If oTrans.hold Then
                        tcell.Text = "<div style=""display: inline; font-size: 6pt;"">hold</div>"
                    Else
                        tcell.Text = "<input type=""checkbox"" id=""txn" & lngCnt & """ name=""txn" & lngCnt & """ value=""" & oTrans.guid & """ />"
                    End If
                    'tcell.CssClass = "apprmaxi"
                    tcell.HorizontalAlign = HorizontalAlign.Right
                    tcell.VerticalAlign = VerticalAlign.Middle
                    outrow.Cells.Add(tcell)
                End If

                tcell = New TableCell
                strLk1 = ""
                strLk2 = ""
                'If oTrans.hash = "approved" Then strLk1 &= "<img align=""top"" height=""16px"" src=""images/star.png"" alt=""ok"" title=""approved!"" />"
                If oTxns.GotAttachment(oTrans.guid) Then
                    strLk1 &= "<a href=""helpers/dld.aspx?guid=" & oTrans.guid & """ style=""color: maroon; font-weight: bold; text-decoration: none;"">"
                    strLk2 &= "<img align=""middle"" height=""16px"" src=""images/redpaperclip.png"" alt=""file"" title=""image attached"" /></a>"
                End If
                tcell.Text = strLk1 & "Transaction: " & oTrans.txn_no & strLk2
                tcell.ColumnSpan = 4
                tcell.VerticalAlign = VerticalAlign.Middle
                tcell.CssClass = "apprmaxismall appmini-left"
                outrow.Cells.Add(tcell)

                tcell = New TableCell
                tcell.Text = "Requested by: " & oTrans.create_user & ", on " & FormatDateTime(oTrans.create_at, DateFormat.ShortDate)
                tcell.ColumnSpan = 3
                tcell.CssClass = "apprmaxi"
                outrow.Cells.Add(tcell)

                tcell = New TableCell
                If Not oTrans.Istotal_amountNull Then
                    tcell.Text = "Amount: " & _
                            FormatNumber(oTrans.total_amount, 2, _
                            TriState.True, TriState.False, TriState.True)
                    'tcell.Text = "huh?"
                End If
                tcell.CssClass = "apprmaxi"
                tcell.HorizontalAlign = HorizontalAlign.Right
                outrow.Cells.Add(tcell)

                'tcell = New TableCell
                'tcell.CssClass = "apprmaxi"
                'tcell.Text = "&nbsp;"
                'tcell.ColumnSpan = 1
                'outrow.Cells.Add(tcell)
                'outrow.CssClass = "apprmaxi"
                'ListBox1.Items.Add(oTrans.txn_no & ": Requested by: " & oTrans.create_user & " ($" & FormatNumber(oTrans.total_amount, 2, TriState.True, TriState.False, TriState.True))

                ' now for the other control box
                tblDets = oDets.GetDataByInvoiceID(oTrans.guid)
                Dim intRwSp As Integer

                tcell = New TableCell
                tcell.CssClass = "dotborder appmini-left"
                intRwSp = tblDets.Rows.Count + 5
                tcell.RowSpan = intRwSp 'tblDets.Rows.Count + 1
                Dim strHoldBtn As String = "", strEditBtn As String = "", strDeleBtn As String = ""
                If oTrans.create_user = Me.UserName Or blnLikeAdmin Then
                    strEditBtn = "<input type=""button"" id=""btnEdit" & lngDRW & """ class=""button-secondary"" onclick=""location.href='editreq.aspx?LDTXN=" & oTrans.guid & "';"" style=""margin: 0px auto 10px auto;"" value=""edit"" /><br />"
                    strDeleBtn = "<input type=""button"" id=""btnDel" & lngDRW & """ name=""btnDel" & lngDRW & _
                            """ value=""DELETE"" class=""button-secondary"" style=""width: 30px; height: 9px; margin-top: 20px;""" & _
                            " onclick=""mkdele('" & oTrans.guid & "');"" />"
                End If
                If blnHold Then
                    strHoldBtn = "<input type=""button"" id=""btnHld" & lngDRW & """ name=""btnHld" & lngDRW & _
                            """ value=""UN-hold"" class=""button-secogreen"" style=""width: 30px; margin-top: 10px; height: 9px;""" & _
                            " onclick=""unhold('" & oTrans.guid & "');"" />"
                Else
                    strHoldBtn = "<input type=""button"" id=""btnHld" & lngDRW & """ name=""btnHld" & lngDRW & _
                            """ value=""HOLD"" class=""button-secondary"" style=""width: 30px; margin-top: 10px; height: 9px;""" & _
                            " onclick=""hold('" & oTrans.guid & "');"" />"
                End If
                tcell.Text = strEditBtn & strHoldBtn & strDeleBtn
                outrow.Cells.Add(tcell)

                tblTrans.Rows.Add(outrow)
                ' NEW FOR 2013-11-15: detail row
                outrow = New TableRow
                tcell = New TableCell
                tcell.Text = "&nbsp;"
                outrow.Cells.Add(tcell)

                tcell = New TableCell
                tcell.ColumnSpan = 3
                tcell.CssClass = "appminiheader"
                tcell.Text = "Entity"
                outrow.Cells.Add(tcell)

                If Not oTrans.ismultisite Then
                    tcell = New TableCell
                    tcell.CssClass = "appminiheader"
                    tcell.Text = "Site"
                    outrow.Cells.Add(tcell)
                End If

                tcell = New TableCell
                tcell.CssClass = "appminiheader"
                tcell.Text = "Region"
                outrow.Cells.Add(tcell)

                tcell = New TableCell
                If oTrans.ismultisite Then
                    tcell.ColumnSpan = 4
                Else
                    tcell.ColumnSpan = 3
                End If
                tcell.CssClass = "appminiheader"
                tcell.Text = "Vendor"
                outrow.Cells.Add(tcell)

                tblTrans.Rows.Add(outrow)

                outrow = New TableRow
                tcell = New TableCell
                tcell.Text = "&nbsp;"
                outrow.Cells.Add(tcell)

                tcell = New TableCell
                tcell.CssClass = "appmini-left"
                tcell.ColumnSpan = 3
                tcell.Text = oTrans.entity_id
                outrow.Cells.Add(tcell)

                If Not oTrans.ismultisite Then
                    tcell = New TableCell
                    If oTrans.Issite_idNull Then
                        tcell.Text = ""
                    Else
                        tcell.Text = oTrans.site_id
                    End If
                    outrow.Cells.Add(tcell)
                End If
                tcell = New TableCell
                tcell.Text = oTrans.region_id
                outrow.Cells.Add(tcell)

                tcell = New TableCell
                If oTrans.ismultisite Then
                    tcell.ColumnSpan = 4
                Else
                    tcell.ColumnSpan = 3
                End If
                tcell.Text = oUtil.FetchSageTitle(oTrans.vendor_id, "vendor") & _
                        " [" & oTrans.vendor_id & "]"
                outrow.Cells.Add(tcell)

                tblTrans.Rows.Add(outrow)

                'outrow = New TableRow
                'tcell = New TableCell
                'tcell.Text = "&nbsp;"
                'outrow.Cells.Add(tcell)

                'tcell = New TableCell
                'tcell.Text = "&nbsp;"
                'tcell.CssClass = "appmini-left"
                'tcell.ColumnSpan = 7
                'outrow.Cells.Add(tcell)
                'tblTrans.Rows.Add(outrow)

                ' now for the details
                If tblDets.Rows.Count > 0 Then
                    outrow = New TableRow
                    tcell = New TableCell
                    tcell.Width = 30
                    outrow.Cells.Add(tcell)
                    tcell = New TableCell
                    tcell.CssClass = "appminiheader"
                    tcell.Text = "&nbsp;"
                    outrow.Cells.Add(tcell)
                    If oTrans.ismultisite Then
                        tcell = New TableCell
                        tcell.CssClass = "appminiheader"
                        tcell.Text = "Site"
                        tcell.HorizontalAlign = HorizontalAlign.Center
                        outrow.Cells.Add(tcell)
                    End If
                    tcell = New TableCell
                    tcell.CssClass = "appminiheader"
                    tcell.Text = "Dept"
                    tcell.HorizontalAlign = HorizontalAlign.Center
                    outrow.Cells.Add(tcell)

                    tcell = New TableCell
                    tcell.CssClass = "appminiheader"
                    tcell.Text = "Service"
                    If Not oTrans.ismultisite Then tcell.ColumnSpan = "2"
                    tcell.HorizontalAlign = HorizontalAlign.Center
                    outrow.Cells.Add(tcell)
                    tcell = New TableCell
                    tcell.CssClass = "appminiheader"
                    tcell.Text = "GL Acct"
                    tcell.HorizontalAlign = HorizontalAlign.Center
                    outrow.Cells.Add(tcell)
                    tcell = New TableCell
                    tcell.CssClass = "appminiheader"
                    tcell.Text = "Payer"
                    tcell.HorizontalAlign = HorizontalAlign.Center
                    outrow.Cells.Add(tcell)
                    tcell = New TableCell
                    tcell.CssClass = "appminiheader"
                    tcell.Text = "Person"
                    tcell.HorizontalAlign = HorizontalAlign.Center
                    outrow.Cells.Add(tcell)
                    tcell = New TableCell
                    tcell.CssClass = "appminiheader"
                    tcell.Text = "Amount"
                    tcell.HorizontalAlign = HorizontalAlign.Center
                    outrow.Cells.Add(tcell)
                    'tcell = New TableCell
                    'tcell.CssClass = "appminiheader"
                    'tcell.Text = "&nbsp;"
                    'tcell.Width = "45"
                    'tcell.HorizontalAlign = HorizontalAlign.Center
                    'outrow.Cells.Add(tcell)
                    'If Not oTrans.ismultisite Then
                    '    tcell = New TableCell
                    '    tcell.CssClass = "appminiheader"
                    '    tcell.Text = ""
                    '    tcell.HorizontalAlign = HorizontalAlign.Center
                    '    outrow.Cells.Add(tcell)
                    'End If
                    tblTrans.Rows.Add(outrow)
                End If
                For Each oDetls In tblDets.Rows
                    outrow = New TableRow
                    tcell = New TableCell
                    outrow.Cells.Add(tcell)

                    tcell = New TableCell
                    lngDRW = lngDRW + 1
                    tcell.Text = "<input type=""button"" id=""btnZ" & lngDRW & """ name=""btnZ" & lngDRW & _
                            """ value=""i"" class=""button-secondary serifate"" style=""width: 9px; height: 9px;""" & _
                            " onclick=""opendetails('" & oDetls.GUID & "');"" />" & _
                            "<input type=""hidden"" id=""gidZ" & lngDRW & """ name=""gidZ" & lngDRW & """ " & _
                            "value=""" & oDetls.GUID & """ />"
                    tcell.CssClass = "redbottomborder appmini-left"
                    tcell.HorizontalAlign = HorizontalAlign.Right
                    outrow.Cells.Add(tcell)

                    If oTrans.ismultisite Then
                        tcell = New TableCell
                        tcell.CssClass = "appmini"
                        If oDetls.Issite_idNull Then
                            tcell.Text = "<p class=""ipecac"" id=""site" & lngDRW & """></p>"
                        Else
                            tcell.Text = "<p class=""ipecac"" id=""site" & lngDRW & """>" & oDetls.site_id & "</p>"
                        End If
                        outrow.Cells.Add(tcell)
                    End If

                    tcell = New TableCell
                    tcell.CssClass = "appmini"
                    tcell.Text = "<p class=""ipecac"" id=""dept" & lngDRW & """>" & oDetls.DeptTitle & "</p>"
                    outrow.Cells.Add(tcell)

                    tcell = New TableCell
                    tcell.CssClass = "appmini"
                    If Not oTrans.ismultisite Then tcell.ColumnSpan = "2"
                    tcell.Text = "<p class=""ipecac"" id=""serv" & lngDRW & """>" & oDetls.ServTitle & "</p>"
                    outrow.Cells.Add(tcell)

                    tcell = New TableCell
                    tcell.CssClass = "appmini"
                    tcell.Text = "<p class=""ipecac"" id=""glac" & lngDRW & """>" & oDetls.GLAcctTitle & "</p>"
                    outrow.Cells.Add(tcell)

                    tcell = New TableCell
                    tcell.CssClass = "appmini"
                    tcell.Text = "<p class=""ipecac"" id=""payr" & lngDRW & """>" & oDetls.PayerTitle & "</p>"
                    outrow.Cells.Add(tcell)

                    tcell = New TableCell
                    tcell.CssClass = "appmini"
                    tcell.Text = "<p class=""ipecac"" id=""pern" & lngDRW & """>" & oDetls.PersonTitle & "</p>"
                    outrow.Cells.Add(tcell)

                    tcell = New TableCell
                    tcell.CssClass = "appmini"
                    tcell.Text = "<p class=""ipecac"" id=""amnt" & lngDRW & """>" & FormatNumber(oDetls.Amount, 2, TriState.True, TriState.False, TriState.True) & "</p>"
                    tcell.HorizontalAlign = HorizontalAlign.Right
                    outrow.Cells.Add(tcell)

                    'tcell = New TableCell
                    'tcell.Text = "<input type=""button"" id=""btnSv" & lngDRW & """ name=""btnSv" & lngDRW & """ value=""save"" class=""button-secondary"" style=""display: none; height: 9px;"" onclick=""savedetails(" & lngDRW & ",'" & oDetls.GUID & "');"" />"
                    'tcell.CssClass = "appmini"
                    'outrow.Cells.Add(tcell)
                    'If Not oTrans.ismultisite Then
                    'tcell = New TableCell
                    'tcell.CssClass = "appmini"
                    'tcell.Text = ""
                    'outrow.Cells.Add(tcell)
                    'End If
                    tblTrans.Rows.Add(outrow)
                Next

                outrow = New TableRow
                tcell = New TableCell
                'tcell.CssClass = "appmini-left appmini-bottom"
                outrow.Cells.Add(tcell)
                tcell = New TableCell
                tcell.CssClass = "appmini appmini-right"
                Dim strHold$ = ""
                Dim strNote$ = "&nbsp;"
                Dim strSpNt$ = ""
                Using oHr As New dsRequestDetailsTableAdapters.holdreasonsTA
                    If (oHr.HasHoldRecords(oTrans.guid) > 0) Then
                        Dim hrRecs As dsRequestDetails.holdreasonsDataTable = oHr.GetByGUID(oTrans.guid)
                        Dim hrDtl As dsRequestDetails.holdreasonsRow
                        Dim lngC As Long = 0
                        strHold = "Hold history:<div style=""width: 100%; border-top: 1px dashed #baa;"">" & vbCrLf
                        For Each hrDtl In hrRecs.Rows
                            If Not hrDtl.IsnoteNull Then
                                lngC += 1
                                strHold &= "<div style=""text-align: right; font-size: 7pt; width: 15%; float: left; clear: left;"
                                If lngC > 1 Then strHold &= "border-top: 1px dotted #dcc;"
                                strHold &= """>" & Format(hrDtl.ts, "M/dd/yyyy H:m:s") & "<br />" & hrDtl.userid & "</div>" & _
                                        "<div style=""font: normal 8pt sans-serif; width: 80%; padding: 2px 5px 1px 10px; text-align: left; float: left; clear: right;"
                                If lngC > 1 Then strHold &= "border-top: 1px dotted #dcc;"
                                strHold &= """ > " & _
                                        "<div style=""padding: 0px; margin: 0px; text-align: left; "">" & Replace(hrDtl.note, vbCrLf, "<br />") & "</div></div>" & vbCrLf
                            End If
                        Next
                        strHold &= "</div>"
                    End If
                End Using
                If Not oTrans.IsnoteNull Then
                    strNote = "<div style=""text-align: left; padding: 2px 1px 5px 5px; overflow: auto;"">" & oTrans.note & "</div>"
                End If
                if Not oTrans.Issp_notesNull then
                    strSpNt = "<div style=""border-top: 1px dotted #cbb; text-align: left; padding: 2px 1px 5px 5px; overflow: auto;""><u>A/P NOTE</u>:" & oTrans.sp_notes & "<br style=""clear:both;"" /></div>"
                End If
                If oTrans.hash = "approved" Then
                    strNote &= oUtil.FetchApprovalInfo(oTrans.guid) & "<br />"
                End If

                tcell.Text = strNote & strHold & strSpNt 
                tcell.ColumnSpan = 8
                outrow.Cells.Add(tcell)
                tblTrans.Rows.Add(outrow)

                outrow = New TableRow
                tcell = New TableCell
                tcell.Text = "&nbsp;"
                tcell.ColumnSpan = 8
                outrow.Cells.Add(tcell)
                tblTrans.Rows.Add(outrow)
            Next
            tblTxns.Dispose()

            outrow = New TableRow
            tcell = New TableCell
            tcell.Text = "&nbsp;"
            outrow.Cells.Add(tcell)

            tcell = New TableCell
            tcell.CssClass = "apprmaxi"
            tcell.Text = "&nbsp;nothing follows"
            tcell.ColumnSpan = 8
            'outrow.CssClass = "apprmaxi"
            outrow.Cells.Add(tcell)
            tblTrans.Rows.Add(outrow)
        Catch ex As Exception
            'ListBox1.Items.Add("Exception: " & ex.Message)
            dbg.Value &= ex.Message & "|"
        Finally
            oDets.Dispose()
            oTxns.Dispose()
        End Try

        btnApprove.Visible = CBool(lngCnt > 0)
    End Sub

    'Private Sub lnkAdmin_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lnkAdmin.Click
    '    Response.Redirect("~/useradmin.aspx")
    'End Sub

    'Private Sub lnkExport_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lnkExport.Click
    '    Response.Redirect("~/exporter.aspx")
    'End Sub

    'Private Sub lnkRequest_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lnkRequest.Click
    '    Response.Redirect("~/default.aspx")
    'End Sub

    'Private Sub lnkLogout_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lnkLogout.Click
    '    Response.Cookies(FormsAuthentication.FormsCookieName).Value = ""
    '    Response.Cookies(FormsAuthentication.FormsCookieName).Expires = DateTime.Now.AddHours(-1)
    '    ' redirect?
    '    Response.Redirect("~/logon.aspx")
    'End Sub

    Private Sub btnExport_Click(ByVal sender As Object, ByVal e As System.EventArgs) 'Handles btnExport.Click
        ' do that export thing
        Dim strDLFdr$ = My.Settings.DownloadPath
        Dim oHash As New zHashes
        Dim strFileName$, strFileBase$
        strFileBase = Path.Combine(strDLFdr, oHash.MkGUID)
        strFileName = strFileBase & ".xls"
        Dim oFStream As FileStream = New FileStream(strFileName, FileMode.Create)
        Dim oTxnTA As New dsRequestDetailsTableAdapters.txnsTA
        Dim oDetTA As New dsRequestDetailsTableAdapters.requestdetailsTableAdapter

        Dim intActives As Integer = 0
        'Response.Write("STEP 1<br />")
        Try
            intActives = oTxnTA.GetActiveCount
        Catch ex As Exception
            ' ow!
            'Response.Cookies("MSG").Value = "zero active count"
        End Try
        'Response.Write("STEP 2: found " & intActives & "<br />")
        If intActives > 0 Then
            Dim intI As Integer
            Dim strTV As String, strTgt As String
            Dim oUtl As New ZUtils
            Dim oXL As New ZXLSWriter(oFStream)
            Dim intRow As Integer = 1
            oXL.BeginWrite()
            ' Begin writing headers
            oXL.WriteCell(0, 0, "Txn #")
            oXL.WriteCell(0, 1, "Inv #")
            oXL.WriteCell(0, 2, "Inv Date")
            'oXL.WriteCell(0, 4, "Total Amt")
            oXL.WriteCell(0, 3, "Entity")
            oXL.WriteCell(0, 4, "Region")
            oXL.WriteCell(0, 5, "Site")
            oXL.WriteCell(0, 6, "Vendor")
            oXL.WriteCell(0, 7, "Fiscal Yr")
            oXL.WriteCell(0, 8, "Requester")
            'oXL.WriteCell(0, 11, "Req Date")
            oXL.WriteCell(0, 9, "Department")
            oXL.WriteCell(0, 10, "Service")
            oXL.WriteCell(0, 11, "GL Acct")
            oXL.WriteCell(0, 12, "Payer")
            oXL.WriteCell(0, 13, "Person")
            oXL.WriteCell(0, 14, "Amount")
            'Response.Write("STEP 3: Header written<br />")
            For intI = 0 To intActives - 1
                strTgt = "txn" & (intI + 1)
                strTV = Request(strTgt)
                If strTV <> "" Then
                    ' get everything by GUID 
                    oDetTA.RefreshTotals(strTV)
                    Dim oTxnTbl As dsRequestDetails.txnsDataTable = oTxnTA.GetByGUID(strTV)
                    Dim oDetTbl As dsRequestDetails.requestdetailsDataTable = oDetTA.GetDataByInvoiceID(strTV)
                    Dim oTxn As dsRequestDetails.txnsRow = oTxnTbl.Rows(0)
                    'oXL.WriteCell(intRow, 0, oTxn.txn_no)       ' transaction number
                    'oXL.WriteCell(intRow, 2, oTxn.invoice_no)
                    'oXL.WriteCell(intRow, 3, oTxn.invoice_dt)
                    'If Not oTxn.Istotal_amountNull Then oXL.WriteCell(intRow, 4, oTxn.total_amount)
                    'If Not oTxn.Istotal_amountNull Then oXL.WriteCell(intRow, 18, oTxn.total_amount)
                    'If Not oTxn.Isentity_idNull Then oXL.WriteCell(intRow, 5, oUtl.FetchSageTitle(oTxn.entity_id, "entity")) ' entity id
                    'If Not oTxn.Isregion_idNull Then oXL.WriteCell(intRow, 6, oUtl.FetchSageTitle(oTxn.region_id, "region")) ' region id
                    'If Not oTxn.Issite_idNull Then oXL.WriteCell(intRow, 7, oUtl.FetchSageTitle(oTxn.site_id, "site")) ' site id
                    'If Not oTxn.Isvendor_idNull Then oXL.WriteCell(intRow, 8, oUtl.FetchSageTitle(oTxn.vendor_id, "vendor")) ' vendor id
                    'If Not oTxn.Isfy_idNull Then oXL.WriteCell(intRow, 9, oTxn.fy_id) ' fy
                    'oXL.WriteCell(intRow, 10, oTxn.create_user) ' requester
                    'oXL.WriteCell(intRow, 11, oTxn.create_at)   ' req date
                    'intRow += 1

                    If oTxnTbl.Rows.Count > 0 Then
                        Dim oDetail As dsRequestDetails.requestdetailsRow
                        For Each oDetail In oDetTbl.Rows
                            oXL.WriteCell(intRow, 0, oTxn.txn_no)
                            oXL.WriteCell(intRow, 1, oTxn.invoice_no)
                            oXL.WriteCell(intRow, 2, oTxn.invoice_dt)
                            If Not oTxn.Isentity_idNull Then oXL.WriteCell(intRow, 3, oTxn.entity_id) ' entity id
                            If Not oTxn.Isregion_idNull Then oXL.WriteCell(intRow, 4, oTxn.region_id) ' region id
                            If Not oTxn.Issite_idNull Then oXL.WriteCell(intRow, 5, oTxn.site_id) ' site id
                            If Not oTxn.Isvendor_idNull Then oXL.WriteCell(intRow, 6, oTxn.vendor_id) ' vendor id
                            If Not oTxn.Isfy_idNull Then oXL.WriteCell(intRow, 7, oTxn.fy_id) ' fy
                            oXL.WriteCell(intRow, 8, oTxn.create_user)
                            oXL.WriteCell(intRow, 9, oDetail.DeptCode)
                            oXL.WriteCell(intRow, 10, oDetail.ServCode)
                            oXL.WriteCell(intRow, 11, oDetail.GLAcctCode)
                            oXL.WriteCell(intRow, 12, oDetail.PayerCode)
                            oXL.WriteCell(intRow, 13, oDetail.Person)
                            oXL.WriteCell(intRow, 14, oDetail.Amount)
                            intRow += 1
                        Next
                    End If
                    intRow += 1
                    ' ya might want to mark this as 'exported' now...?
                End If
                ' Response.Write(strTgt & "=[" & strTV & "]<br />" & vbCrLf)
                'Response.Write("STEP 4: Line out: " & intRow & "<br />")
            Next
            oXL.EndWrite()
            'Dim xl As ZXLSWriter
            oFStream.Close()
            'Response.Write("STEP 5: Marking rows<br />")

            ' Phase 2... mark as exported
            For intI = 0 To intActives - 1
                strTgt = "txn" & (intI + 1)
                strTV = Request(strTgt)
                If strTV <> "" Then
                    oTxnTA.FlagExportByGUID(strTV)
                End If
            Next
            'Response.Write("STEP 6: Starting download<br />")

            ' perform download
            If True Then 'False Then
                Response.ContentType = "application/octet-stream"
                Dim cd As New ContentDisposition
                cd.Inline = False
                cd.FileName = "export.xls"
                Response.AppendHeader("Content-Disposition", cd.ToString())
                Dim filedata() As Byte = File.ReadAllBytes(strFileName)
                Response.OutputStream.Write(filedata, 0, filedata.Length)
                'Response.Cookies("MSG").Value = "export done"
            End If
        End If
    End Sub

    Private Sub btnApprove_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnApprove.Click
        ' approve checked items
        Dim intCnt As Integer = 0
        Using oTxnTA As New dsRequestDetailsTableAdapters.txnsTA
            Dim intActives As Integer = 0
            'Response.Write("STEP 1<br />")
            Try
                If Me.IsAdmin Then
                    intActives = oTxnTA.GetActiveCount
                Else
                    intActives = oTxnTA.GetToApproveCount(Me.UserName)
                End If
            Catch ex As Exception
                ' ow!
                'Response.Cookies("MSG").Value = "zero active count"
            End Try
            'Response.Write("STEP 2: found " & intActives & "<br />")

            If intActives > 0 Then
                Dim strTgt$, strTV$
                For intI = 0 To intActives - 1
                    strTgt = "txn" & (intI + 1)
                    strTV = Request(strTgt)
                    If strTV <> "" Then
                        oTxnTA.SetHash("approved", strTV)
                        oTxnTA.SetApproval(True, Me.UserName, strTV)
                        Try
                            LogMessage("Transaction approved (" & strTV & ")", strTV, "", Me.GUID, Request)
                        Catch ex As Exception
                            ' do nothing
                        End Try
                        intCnt += 1
                    End If
                Next
            End If
        End Using
        If intCnt > 0 Then
            Response.Redirect("~/approval.aspx")
        End If
    End Sub

End Class