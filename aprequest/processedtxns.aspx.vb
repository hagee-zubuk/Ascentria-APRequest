Imports System.Data
Imports System.Data.SqlClient

Partial Public Class ProcessedTxns
    Inherits ZPageSecure


    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Me.IsFinance Then
            Response.Cookies("MSG").Value = "you do not have adequate rights to that page (PROC)"
            Response.Redirect("~/default.aspx")
        End If
        Me.Toplinks1.GUID = Me.GUID
        Me.datedisp.Text = Format(Date.Now, "ddd MMM d, yyyy h:mmtt")
        Dim strMO = Request("mo") & ""
        Dim strYr = Request("yr") & ""
        Dim strEnty$ = Request("en") & ""
        Dim strRegn$ = Request("rn") & ""

        Dim oTxns As New dsRequestDetailsTableAdapters.txnsTA
        Dim oDets As New dsRequestDetailsTableAdapters.requestdetailsTableAdapter
        Dim tblTxns As New dsRequestDetails.txnsDataTable
        Dim tblDets As dsRequestDetails.requestdetailsDataTable
        Dim oTrans As dsRequestDetails.txnsRow
        Dim oDetls As dsRequestDetails.requestdetailsRow
        Dim lngCnt As Long = 0, lngDRW As Long = 0
        Dim oUtil As New ZUtils
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
            Dim blnAttached As Boolean
            Dim intRwSp As Integer
            'If strMO <> "" And strYr <> "" Then
            'Dim dtStart As Date, dtEnd As Date
            'dtStart = CDate(strMO & "/1/" & strYr)
            'dtEnd = DateAdd(DateInterval.Month, 1, dtStart)
            'tblTxns = oTxns.GetDoneByDate(dtStart, dtEnd)
            ''Me.errorLabel.Value = dtStart & " to " & dtEnd
            'Else
            'tblTxns = oTxns.GetAllDone()
            'End If
            Dim strSQL$ = "SELECT create_at, create_user, detailhash, due_dt" & _
                    ", entity_id, fy_id, guid, hash, hold, invoice_dt, invoice_no" & _
                    ", ismultisite, note, region_id, site_id, total_amount, txn_no" & _
                    ", vendor_id FROM transactions " & _
                    "WHERE (isexported = 1) AND (isdone = 1)"
            If strMO <> "" And strYr <> "" Then
                Dim dtStart As Date, dtEnd As Date
                dtStart = CDate(strMO & "/1/" & strYr)
                dtEnd = DateAdd(DateInterval.Month, 1, dtStart)
                strSQL &= " AND [create_at] >= '" & Format(dtStart, "yyyy-MM-dd") & _
                        "' AND [create_at] < '" & Format(dtEnd, "yyyy-MM-dd") & "'"
            End If
            If strEnty <> "" Then
                strSQL &= " AND [entity_id]='" & strEnty & "'"
            End If
            If strRegn <> "" Then
                strSQL &= " AND [region_id]='" & strRegn & "'"
            End If
            strSQL &= " ORDER BY txn_no "

            'Dim da = New System.Data.SqlClient
            'Dim ds As New System.Data.DataSet
            Try
                tblTxns = oTxns.GetDone_MOD(strSQL)
            Catch ex As Exception
                LogMessage("processedtxns.aspx: " & ex.Message, "", "", "", Request)
            End Try

            For Each oTrans In tblTxns.Rows

                outrow = New TableRow

                lngCnt = lngCnt + 1
                'tcell = New TableCell
                'tcell.Text = "<input type=""checkbox"" id=""txn" & lngCnt & """ name=""txn" & lngCnt & """ value=""" & oTrans.guid & """ />"
                'tcell.CssClass = "apprmaxi"
                'tcell.HorizontalAlign = HorizontalAlign.Right
                'tcell.VerticalAlign = VerticalAlign.Middle
                'outrow.Cells.Add(tcell)

                tcell = New TableCell

                strLk2 = ""
                blnAttached = oTxns.GotAttachment(oTrans.guid)
                'If oTrans.hash = "approved" Then strLk1 &= "<img align=""top"" height=""16px"" src=""images/star.png"" alt=""ok"" title=""approved!"" />"
                'If blnAttached Then
                '    strLk1 &= "<a href=""helpers/dld.aspx?guid=" & oTrans.guid & """ style=""color: maroon; font-weight: bold; text-decoration: none;"">"
                '    strLk2 &= "</a>"
                'End If
                strLk1 = "<input type=""button"" id=""btnZ" & lngCnt & """ name=""btnZ" & lngCnt & _
                                """ value=""i"" class=""button-secondary serifate"" style=""width: 9px; height: 9px;""" & _
                                " onclick=""opendetails('" & oTrans.guid & "');"" />" & _
                                "<input type=""hidden"" id=""gidZ" & lngCnt & """ name=""gidZ" & lngCnt & """ " & _
                                "value=""" & oTrans.guid & """ />"
                tcell.Text = strLk1 & "&nbsp;Txn:&nbsp;" & oTrans.txn_no
                tcell.ColumnSpan = 3
                tcell.VerticalAlign = VerticalAlign.Middle
                tcell.CssClass = "apprmaxismall"
                outrow.Cells.Add(tcell)

                tcell = New TableCell
                tcell.Text = oUtil.FetchSageTitle(oTrans.entity_id, "entity") & "<br />" & vbCrLf & "<span style=""font-size: 7pt;"">" & _
                        "requested by: " & oTrans.create_user & ", on " & FormatDateTime(oTrans.create_at, DateFormat.ShortDate) & "</span>"
                tcell.ColumnSpan = 3
                tcell.CssClass = "apprmaxi"
                outrow.Cells.Add(tcell)

                tcell = New TableCell
                If Not oTrans.Istotal_amountNull Then _
                        tcell.Text = "Amount: " & FormatNumber(oTrans.total_amount, _
                                2, TriState.True, TriState.False, TriState.True)
                If Not oTrans.Isvendor_idNull Then _
                        tcell.Text &= "<br /><div style=""width: 100%; text-align: right; font-size: 7pt;"">Vendor: " & _
                                oUtil.FetchSageTitle(oTrans.vendor_id, "vendor") & " [" & oTrans.vendor_id & "]</div>"
                tcell.CssClass = "apprmaxi"
                tcell.ColumnSpan = 2
                tcell.HorizontalAlign = HorizontalAlign.Right
                outrow.Cells.Add(tcell)

                '                tcell = New TableCell
                '                tcell.CssClass = "apprmaxi"
                '                tcell.Text = "&nbsp;"
                '                'tcell.ColumnSpan = 2
                '                outrow.Cells.Add(tcell)
                'outrow.CssClass = "apprmaxi"
                'ListBox1.Items.Add(oTrans.txn_no & ": Requested by: " & oTrans.create_user & " ($" & FormatNumber(oTrans.total_amount, 2, TriState.True, TriState.False, TriState.True))
                tblDets = oDets.GetDataByInvoiceID(oTrans.guid)

                tcell = New TableCell
                tcell.CssClass = "dotborder"
                intRwSp = tblDets.Rows.Count + 2
                tcell.RowSpan = intRwSp 'tblDets.Rows.Count + 1
                tcell.VerticalAlign = VerticalAlign.Top
                If blnAttached Then
                    tcell.Text = "<a href=""helpers/dld.aspx?guid=" & oTrans.guid & """" & _
                            "rel=""prettyPhoto"" ><img align=""middle"" " & _
                            "src=""helpers/atn.aspx?rs=" & intRwSp & "&guid=" & oTrans.guid & _
                            """ alt=""file"" title=""image attached"" /></a>"
                Else
                    tcell.Text = "&nbsp;"
                End If
                outrow.Cells.Add(tcell)

                tblTrans.Rows.Add(outrow)

                ' now for the details

                If tblDets.Rows.Count > 0 Then
                    outrow = New TableRow
                    tcell = New TableCell
                    tcell.Text = "<input type=""button"" id=""btnP" & _
                            lngCnt & """ name=""btnP" & lngCnt & _
                            """ value=""PDF"" class=""button-secondary serifate"" " & _
                            "style=""width: 14px; height: 9px;""" & _
                            " onclick=""dlpdf('" & oTrans.txn_no & "');"" />"
                    tcell.VerticalAlign = VerticalAlign.Middle
                    tcell.RowSpan = intRwSp - 1
                    tcell.Width = 30
                    outrow.Cells.Add(tcell)

                    tcell = New TableCell
                    tcell.CssClass = "appminiheader"
                    tcell.Text = "Dept"
                    tcell.HorizontalAlign = HorizontalAlign.Center
                    outrow.Cells.Add(tcell)
                    tcell = New TableCell
                    tcell.CssClass = "appminiheader"
                    tcell.Text = "Service"
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
                    tblTrans.Rows.Add(outrow)
                End If
                For Each oDetls In tblDets.Rows
                    outrow = New TableRow
                    'tcell = New TableCell
                    'outrow.Cells.Add(tcell)

                    'tcell = New TableCell
                    lngDRW = lngDRW + 1
                    With oDetls
                        tcell = New TableCell
                        tcell.CssClass = "appmini"
                        tcell.Text = "<p class=""ipecac"" id=""dept" & lngDRW & """>" & .DeptCode & "</p>"
                        outrow.Cells.Add(tcell)

                        tcell = New TableCell
                        tcell.CssClass = "appmini"
                        tcell.Text = "<p class=""ipecac"" id=""serv" & lngDRW & """>" & .ServCode & "</p>"
                        outrow.Cells.Add(tcell)

                        tcell = New TableCell
                        tcell.CssClass = "appmini"
                        tcell.Text = "<p class=""ipecac"" id=""glac" & lngDRW & """>" & .GLAcctCode & "</p>"
                        outrow.Cells.Add(tcell)

                        tcell = New TableCell
                        tcell.CssClass = "appmini"
                        tcell.Text = "<p class=""ipecac"" id=""payr" & lngDRW & """>" & .PayerCode & "</p>"
                        outrow.Cells.Add(tcell)

                        tcell = New TableCell
                        tcell.CssClass = "appmini"
                        tcell.Text = "<p class=""ipecac"" id=""pern" & lngDRW & """>" & .Person & "</p>"
                        outrow.Cells.Add(tcell)

                        tcell = New TableCell
                        tcell.CssClass = "appmini"
                        tcell.Text = "<p class=""ipecac"" id=""amnt" & lngDRW & """>" & FormatNumber(oDetls.Amount, 2, TriState.True, TriState.False, TriState.True) & "</p>"
                        tcell.HorizontalAlign = HorizontalAlign.Right
                        outrow.Cells.Add(tcell)
                    End With
                    'tcell = New TableCell
                    'tcell.Text = "<input type=""button"" id=""btnSv" & lngDRW & """ name=""btnSv" & lngDRW & """ value=""save"" class=""button-secondary"" style=""display: none; height: 9px;"" onclick=""savedetails(" & lngDRW & ",'" & oDetls.GUID & "');"" />"
                    'tcell.CssClass = "appmini"
                    'outrow.Cells.Add(tcell)

                    tblTrans.Rows.Add(outrow)
                Next
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
            tcell.CssClass = "apprmaxi"
            tcell.Text = "&nbsp;nothing follows"
            tcell.ColumnSpan = 8
            outrow.CssClass = "apprmaxi"
            outrow.Cells.Add(tcell)
            tblTrans.Rows.Add(outrow)
        Catch ex As Exception
            'ListBox1.Items.Add("Exception: " & ex.Message)
        Finally
            oDets.Dispose()
            oTxns.Dispose()
        End Try

    End Sub

End Class