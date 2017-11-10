Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Net.Mime


Partial Public Class myreqs
    Inherits ZPageSecure

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Dim oUtil As New ZUtils
        ulink.Text = Me.DisplayName
        ulink.PostBackUrl = "~/profile.aspx?guid=" & Me.GUID
        Me.Toplinks1.GUID = Me.GUID
        Me.datedisp.Text = Format(Date.Now, "ddd MMM d, yyyy h:mmtt")

        Dim strSQL$ = "SELECT create_at, create_user, detailhash, due_dt" & _
                        ", entity_id, fy_id, guid, hash, hold, invoice_dt, invoice_no" & _
                        ", ismultisite, note, region_id, site_id, total_amount, txn_no" & _
                        ", vendor_id, appr_ts, approve_user, isapproved FROM transactions " & _
                        "WHERE [isdone]=1 AND [create_user] LIKE '" & Me.UserName & "'"
        strSQL &= " ORDER BY [invoice_dt] DESC"

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

        Dim tcell As TableCell
        Dim outrow As TableRow
        Dim strLk1$, strLk2$
        Dim blnAttached As Boolean
        Dim lngCnt As Long, lngDRW As Long, intRwSp As Integer
        Dim oTxns As New dsRequestDetailsTableAdapters.txnsTA
        Dim oDets As New dsRequestDetailsTableAdapters.requestdetailsTableAdapter
        Dim tblDets As dsRequestDetails.requestdetailsDataTable

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
                strLk1 = "<input type=""hidden"" id=""gidZ" & lngCnt & """ name=""gidZ" & lngCnt & """ " & _
                                "value=""" & row("guid") & """ />"
                tcell.Text = strLk1 & "&nbsp;Txn:&nbsp;" & row("txn_no")
                tcell.ColumnSpan = 3
                tcell.VerticalAlign = VerticalAlign.Middle
                tcell.CssClass = "apprmaxismall appmini-left"
                outrow.Cells.Add(tcell)

                tcell = New TableCell
                tcell.Text = oUtil.FetchSageTitle(row("entity_id"), "entity") & "<br />" & vbCrLf & "<span style=""font-size: 7pt;"">" & _
                        "on " & FormatDateTime(row("create_at"), DateFormat.ShortDate) & "</span>"
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
                    tcell.Text = "<input type=""button"" id=""btnZ" & lngCnt & """ name=""btnZ" & lngCnt & _
                                """ value=""i"" class=""button-secondary serifate"" style=""width: 9px; height: 9px;""" & _
                                " onclick=""opendetails('" & row("guid") & "');"" />"
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

                Dim strNote$ = ""
                Dim dblLimit As Double
                Using oUr As New dsUsersTableAdapters.userconfigTA
                    dblLimit = oUr.GetLimitByGUID(Me.GUID)
                End Using
                If (row("total_amount") > dblLimit) Then
                    strNote &= "<div style=""text-align: right; font-size: 7pt; width: 15%; float: left; clear: left;"">"
                    If Not IsDBNull(row("appr_ts")) Then strNote &= Format(row("appr_ts"), "M/dd/yyyy H:m:s") & "<br />"
                    strNote &= row("approve_user") & "</div>" & _
                            "<div style=""font: normal 8pt sans-serif; width: 80%; padding: 2px 5px 1px 10px; text-align: left; float: left; clear: right;"" > " & _
                            "<div style=""padding: 0px; margin: 0px; text-align: left; "">"
                    If (Convert.ToBoolean(row("isapproved"))) Then
                        strNote &= "APPROVED"
                    Else
                        strNote &= "not approved"
                    End If
                    strNote &= "<br /></div></div>" & vbCrLf
                End If
                Dim strHold$ = ""
                Using oHr As New dsRequestDetailsTableAdapters.holdreasonsTA
                    If (oHr.HasHoldRecords(row("guid")) > 0) Then
                        Dim hrRecs As dsRequestDetails.holdreasonsDataTable = oHr.GetByGUID(row("guid"))
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
                        strNote &= strHold
                    End If
                End Using
                If strNote.Length > 1 Then
                    outrow = New TableRow
                    tcell = New TableCell
                    tcell.CssClass = "apprmaxi appmini-left appmini-right appmini-bottom"
                    tcell.Text = strNote
                    tcell.ColumnSpan = 9
                    outrow.Cells.Add(tcell)
                    tblTrans.Rows.Add(outrow)
                End If

                outrow = New TableRow
                tcell = New TableCell
                tcell.Text = "&nbsp;"
                tcell.ColumnSpan = 9
                outrow.Cells.Add(tcell)
                tblTrans.Rows.Add(outrow)
            End While
            'tblTxns.Dispose()

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
            Me.errorLabel.Value = ex.Message
        Finally
            oDets.Dispose()
            oTxns.Dispose()
            row.Close()
            cn.Close()
        End Try

    End Sub

End Class