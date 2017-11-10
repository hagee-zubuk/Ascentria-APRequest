Imports System.Data
Imports System.Data.SqlClient

Partial Public Class ProcessedTxns2
    Inherits ZPageSecure


    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Me.IsFinance Then
            Response.Cookies("MSG").Value = "you do not have adequate rights to that page"
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

        Dim tcell As TableCell
        Dim outrow As TableRow
        Dim strLk1$, strLk2$
        Dim blnAttached As Boolean
        Dim intRwSp As Integer

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

        Dim strCN As String = ConfigurationManager.ConnectionStrings("aprequestConnectionString").ConnectionString
        Dim cn As New SqlClient.SqlConnection
        Dim ds As New DataSet
        Dim dt As New DataTable
        cn.ConnectionString = strCN
        Dim cmd As New SqlClient.SqlCommand
        cmd.Connection = cn
        cmd.CommandType = CommandType.Text
        cmd.CommandText = strSQL
        Dim strMesg$ = ""
        Dim row As SqlDataReader = Nothing
        Try
            cn.Open()
            row = cmd.ExecuteReader()
        Catch ex As Exception
            strMesg = ex.Message
            Response.Write("<pre>" & strMesg & "</pre>")
            Exit Sub
        End Try

        If Not row.HasRows Then
            outrow = New TableRow
            tcell = New TableCell
            tcell.Text = "ERROR: " & strMesg
            tcell.ColumnSpan = 7
            outrow.Cells.Add(tcell)
            tblTrans.Rows.Add(outrow)
            Exit Sub
        End If

        Try
            While row.Read()
                outrow = New TableRow
                lngCnt = lngCnt + 1
                tcell = New TableCell

                strLk2 = ""

                blnAttached = oTxns.GotAttachment(row("guid"))
                strLk1 = "<input type=""button"" id=""btnZ" & lngCnt & """ name=""btnZ" & lngCnt & _
                                """ value=""i"" class=""button-secondary serifate"" style=""width: 9px; height: 9px;""" & _
                                " onclick=""opendetails('" & row("guid") & "');"" />" & _
                                "<input type=""hidden"" id=""gidZ" & lngCnt & """ name=""gidZ" & lngCnt & """ " & _
                                "value=""" & row("guid") & """ />"
                tcell.Text = strLk1 & "&nbsp;Txn:&nbsp;" & row("txn_no")
                tcell.ColumnSpan = 3
                tcell.VerticalAlign = VerticalAlign.Middle
                tcell.CssClass = "apprmaxismall appmini-left"
                outrow.Cells.Add(tcell)

                tcell = New TableCell
                tcell.Text = oUtil.FetchSageTitle(row("entity_id"), "entity") & "<br />" & vbCrLf & "<span style=""font-size: 7pt;"">" & _
                        "requested by: " & row("create_user") & ", on " & FormatDateTime(row("create_at"), DateFormat.ShortDate) & "</span>"
                tcell.ColumnSpan = 3
                tcell.CssClass = "apprmaxi"
                outrow.Cells.Add(tcell)

                tcell = New TableCell
                If Not IsDBNull(row("total_amount")) Then _
                        tcell.Text = "Amount: " & FormatNumber(row("total_amount"), _
                                2, TriState.True, TriState.False, TriState.True)
                If Not IsDBNull(row("vendor_id")) Then _
                        tcell.Text &= "<br /><div style=""width: 100%; text-align: right; font-size: 7pt;"">Vendor: " & _
                                oUtil.FetchSageTitle(row("vendor_id"), "vendor") & " [" & row("vendor_id") & "]</div>"
                tcell.CssClass = "apprmaxi"
                tcell.ColumnSpan = 2
                tcell.HorizontalAlign = HorizontalAlign.Right
                outrow.Cells.Add(tcell)
                tblDets = oDets.GetDataByInvoiceID(row("guid"))

                tcell = New TableCell
                tcell.CssClass = "dotborder"
                intRwSp = tblDets.Rows.Count + 2
                tcell.RowSpan = intRwSp 'tblDets.Rows.Count + 1
                tcell.VerticalAlign = VerticalAlign.Top
                If blnAttached Then
                    tcell.Text = "<a href=""helpers/dld.aspx?guid=" & row("guid") & """" & _
                            "rel=""prettyPhoto"" ><img align=""middle"" " & _
                            "src=""helpers/atn.aspx?rs=" & intRwSp & "&guid=" & row("guid") & _
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
                            " onclick=""dlpdf('" & row("txn_no") & "');"" />"
                    tcell.VerticalAlign = VerticalAlign.Middle
                    tcell.RowSpan = intRwSp - 1
                    tcell.CssClass = "appmini-left appmini-bottom"
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
                    tblTrans.Rows.Add(outrow)
                Next
                outrow = New TableRow
                tcell = New TableCell
                tcell.Text = "&nbsp;"
                tcell.ColumnSpan = 8
                outrow.Cells.Add(tcell)
                tblTrans.Rows.Add(outrow)
            End While
            tblTxns.Dispose()

            outrow = New TableRow
            tcell = New TableCell
            tcell.CssClass = "apprmaxi"
            tcell.Text = "&nbsp;nothing follows"
            tcell.ColumnSpan = 9
            outrow.CssClass = "apprmaxi"
            outrow.Cells.Add(tcell)
            tblTrans.Rows.Add(outrow)
        Catch ex As Exception
            'ListBox1.Items.Add("Exception: " & ex.Message)
        Finally
            oDets.Dispose()
            oTxns.Dispose()
            row.Close()
            cn.Close()
        End Try

    End Sub

End Class