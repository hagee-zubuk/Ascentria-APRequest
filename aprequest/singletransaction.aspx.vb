Public Partial Class singletransaction
    Inherits System.Web.UI.Page

    Private _BarcodeURL As String = "http://www.argao.net/barcoder/index.php?code=" 

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim strGUID$, strTrxn$
        Dim blnTxn As Boolean = False
        Dim oDetlTA As New dsRequestDetailsTableAdapters.requestdetailsTableAdapter
        Dim oUserTA As New dsUsersTableAdapters.userconfigTA
        Dim oUtil As New ZUtils
        Try
            strTrxn = Request("txn")
            strGUID = Request("guid")
            If strTrxn <> "" Then
                blnTxn = True
            Else
                If strGUID = "" Then
                    Me.lblMesg.Text = "GUID not specified"
                    Exit Sub
                End If
            End If
        Catch ex As Exception
            Exit Sub
        End Try

        If My.Settings.BarcodeURL.Length > 7 then Me._BarcodeURL = My.Settings.BarcodeURL

        Dim oTxns As New dsRequestDetailsTableAdapters.txnsTA
        Dim oTxnRec As dsRequestDetails.txnsRow
        Dim strApprover$ = ""
        Try
            Dim tblTxns As dsRequestDetails.txnsDataTable
            If blnTxn Then
                tblTxns = oTxns.GetByTxn(strTrxn)
            Else
                tblTxns = oTxns.GetByGUID(strGUID)
            End If

            If tblTxns.Rows.Count < 1 Then
                Me.lblMesg.Text = "A record with that ID was not found"
                Exit Sub
            End If
            oTxnRec = tblTxns.Rows(0)
            Dim strRes$ = "{ ""error"":0," & _
                    """guid"":""" & oTxnRec.guid & ""","
            With oTxnRec
                If .ismultisite Then
                    Me.hdSite.Visible = True
                Else
                    Me.hdSite.Visible = False
                    Me.BigSiteName.Text = "<tr><td align=""right"">Site:</td>" & _
                            "<td><input name=""txtSiteTitle"" type=""text"" " & _
                            "value="""
                    If Not .Issite_idNull Then Me.BigSiteName.Text &= oUtil.FetchSageTitle(.site_id, "site")
                    Me.BigSiteName.Text &= """ readonly=""readonly"" id=""txtRegnTitle"" " & _
                            "style=""width:500px;""/></td><td><input name=""txtSiteCode"" " & _
                            "type=""text"" value="""
                    If Not .Issite_idNull Then Me.BigSiteName.Text &= oUtil.FetchSageTitle(.site_id, "site")
                    Me.BigSiteName.Text &= """ readonly=""readonly"" " & _
                            "id=""txtSiteCode"" /></td></tr>"
                End If
                strApprover = oTxns.GetApprover(.guid).ToString()
                Me.Title = "Transaction " & .txn_no
                '. approval date needed
                Me.imgBC.ImageUrl = me._BarcodeURL & .txn_no.ToString().Trim()
                If Not .Isentity_idNull Then
                    Me.txtEntiCode.Text = .entity_id
                    Me.txtEntiTitle.Text = oUtil.FetchSageTitle(.entity_id, "entity")
                End If
                If Not .Isregion_idNull Then
                    Me.txtRegnCode.Text = .region_id
                    Me.txtRegnTitle.Text = oUtil.FetchSageTitle(.region_id, "region")
                End If
                If Not .Isvendor_idNull Then
                    Me.txtVendCode.Text = .vendor_id
                    Dim strVendTitle As string = oUtil.FetchVendorTitle(.vendor_id)
                    Dim intLns As Integer
                    intLns = strVendTitle.Split(vbCr).GetUpperBound(0)
                    If intLns > 0 then
                        Me.txtVendTitle.TextMode=TextBoxMode.MultiLine
                    else
                        Me.txtVendTitle.TextMode=TextBoxMode.SingleLine
                    End If
                    Me.txtVendTitle.Text = strvendtitle
                    Me.txtVendTitle.Rows = 1 + intLns
'FetchSageTitle(.vendor_id, "vendor")
                End If
                If Not .Isfy_idNull Then
                    Me.txtFiscCode.Text = .fy_id
                    Me.txtFiscTitle.Text = oUtil.FetchSageTitle(.fy_id, "fy")
                End If
                If Not .Istotal_amountNull Then Me.txtAmount.Text = .total_amount.ToString("#,##0.00")
                If Not .Isinvoice_noNull Then Me.txtInvNo.Text = .invoice_no
                If Not .Isinvoice_dtNull Then Me.txtInvDt.Text = .invoice_dt.ToString("MM/dd/yyyy")
                If Not .Isdue_dtNull Then Me.txtDueDt.Text = .due_dt.ToString("MM/dd/yyyy")
                Me.txtReqst.Text = oUserTA.GetNameByUsername(.create_user)
                Me.txtApprv.Text = oUtil.FetchApprovalInfo(.guid)   '  oUserTA.GetNameByUsername(strApprover)
                If Not .Issp_notesNull Then
                    Me.litSp_Notes.Text = "<div style=""padding: 5px 10px; font-size: 11pt; border: 1px dotted #abb; width: 750px; margin: 10px auto;"">" & _
                            "<u><b>Note&nbsp;to&nbsp;A/P</b></u>:&nbsp;" & _
                            .sp_notes.Replace(vbCrLf,"<br />") & _
                            "</div>"
                End If
            End With
        Catch ex As Exception
            Me.lblMesg.Text = "An exception occurred (1): " & ex.Message
            Exit Sub
        End Try

        Dim tblDets As dsRequestDetails.requestdetailsDataTable = oDetlTA.GetDataByInvoiceID(oTxnRec.guid)
        'Dim strTable$
        Dim lngRwCnt& = 0
        If tblDets.Rows.Count > 0 Then
            Try
                For Each oDetls As dsRequestDetails.requestdetailsRow In tblDets.Rows
                    With oDetls
                        Dim tcell As TableCell
                        Dim outrow As TableRow

                        outrow = New TableRow
                        If oTxnRec.ismultisite Then
                            tcell = New TableCell
                            tcell.CssClass = "detamaxi"
                            If Not .Issite_idNull Then tcell.Text = oUtil.FetchSageTitle(.site_id, "site") & " [" & .site_id & "]"
                            tcell.HorizontalAlign = HorizontalAlign.Center
                            tcell.VerticalAlign = VerticalAlign.Middle
                            outrow.Cells.Add(tcell)
                        End If

                        tcell = New TableCell
                        tcell.CssClass = "detamaxi"
                        tcell.Text = oUtil.FetchSageTitle(.DeptCode, "dept") & " [" & .DeptCode & "]"
                        tcell.HorizontalAlign = HorizontalAlign.Center
                        tcell.VerticalAlign = VerticalAlign.Middle
                        outrow.Cells.Add(tcell)

                        tcell = New TableCell
                        tcell.CssClass = "detamaxi"
                        tcell.Text = .ServTitle & " [" & .ServCode & "]"
                        tcell.HorizontalAlign = HorizontalAlign.Center
                        tcell.VerticalAlign = VerticalAlign.Middle
                        outrow.Cells.Add(tcell)

                        tcell = New TableCell
                        tcell.CssClass = "detamaxi"
                        tcell.Text = .GLAcctTitle & " [" & .GLAcctCode & "]"
                        tcell.HorizontalAlign = HorizontalAlign.Center
                        tcell.VerticalAlign = VerticalAlign.Middle
                        outrow.Cells.Add(tcell)

                        tcell = New TableCell
                        tcell.CssClass = "detamaxi"
                        tcell.Text = .PayerTitle & " [" & .PayerCode & "]"
                        tcell.HorizontalAlign = HorizontalAlign.Center
                        tcell.VerticalAlign = VerticalAlign.Middle
                        outrow.Cells.Add(tcell)

                        tcell = New TableCell
                        tcell.CssClass = "detamaxi"
                        tcell.Text = .PersonTitle & " [" & .Person.Trim() & "]"
                        tcell.HorizontalAlign = HorizontalAlign.Center
                        tcell.VerticalAlign = VerticalAlign.Middle
                        outrow.Cells.Add(tcell)

                        tcell = New TableCell
                        tcell.CssClass = "detamaxi"
                        tcell.Text = FormatNumber(.Amount, 2, TriState.True, TriState.False, TriState.True)
                        tcell.HorizontalAlign = HorizontalAlign.Right
                        tcell.VerticalAlign = VerticalAlign.Middle
                        outrow.Cells.Add(tcell)

                        tblTrans.Rows.Add(outrow)
                        lngRwCnt += 1
                    End With
                Next
            Catch ex As Exception
                Me.lblMesg.Text = "Exception occurred. (2)"
            End Try
        End If
        Me.lblMesg.Text = "Transaction&nbsp;" & oTxnRec.txn_no.ToString().Trim()
    End Sub

End Class