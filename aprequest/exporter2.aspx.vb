﻿Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Net
Imports System.Net.Mime


Partial Public Class exporter2
    Inherits ZPageSecure
    ' WAS: Inherits System.Web.UI.Page
    Private m_strExtraSQL As String = ""

    Private Function FillEntityDropdown(ByVal strEnty$)
        Dim strCN As String = ConfigurationManager.ConnectionStrings("SAGE_DBConnectionString").ConnectionString
        Dim cn As New SqlClient.SqlConnection
        Dim ds As New DataSet
        Dim dt As New DataTable
        Me.datedisp.Text = Format(Date.Now, "ddd MMM d, yyyy h:mmtt")
        cn.ConnectionString = strCN
        Dim cmd As New SqlClient.SqlCommand
        cmd.Connection = cn
        cmd.CommandType = CommandType.Text
        Dim strSQL As String = "SELECT [sTitle], [sCodeID] FROM [tblAcctCode_1] " & _
                "ORDER BY [sTitle] ASC"
        cmd.CommandText = strSQL
        Try
            cn.Open()
            cmd.ExecuteNonQuery()
            Dim da As New SqlDataAdapter(cmd)
            da.Fill(ds)
        Catch ex As Exception
        Finally
            cn.Close()
        End Try

        dt = ds.Tables(0)
        Dim txtItems As String = ""
        For Each row As DataRow In dt.Rows
            Me.ddlFilter.Items.Add(New ListItem(row("sTitle").ToString(), row("sCodeID").ToString()))
            If row("sCodeID") = strEnty Then Me.ddlFilter.SelectedIndex = Me.ddlFilter.Items.Count - 1
        Next
        Return txtItems
    End Function

    Private Sub InitFilterDDLS(ByVal strEnty$, ByVal strRegn$)
        ' fill up the filter dropdown from database
        Me.ddlFilter.Items.Add(New ListItem("All", ""))
        FillEntityDropdown(strEnty)
        Dim strDir = Session("exDir")
        Dim strFil = Session("exFil")
        Dim strSor = Session("exSor")

        If IsPostBack Then
            strDir = Me.ddlDirection.SelectedItem.Value
            Session("exDir") = Me.ddlDirection.SelectedItem.Value
            strFil = Me.ddlFilter.SelectedItem.Value
            Session("exFil") = Me.ddlFilter.SelectedItem.Value
            strSor = Me.ddlSort.SelectedItem.Value
            Session("exSor") = Me.ddlSort.SelectedItem.Value
            Me.m_strExtraSQL = ""
        ElseIf strEnty.Length > 0 Then
            Session("exFil") = strEnty
            strSor = Me.ddlSort.SelectedItem.Value
        End If

        If strFil <> "" Then
            Me.m_strExtraSQL &= " AND entity_id='" & strFil & "' "
        ElseIf strEnty.Length > 0 Then
            Me.m_strExtraSQL &= " AND entity_id='" & strEnty & "' "
        End If
        If strSor <> "" Then
            Me.m_strExtraSQL &= " ORDER BY [" & strSor & "] "
        Else
            Me.m_strExtraSQL &= " ORDER BY [create_at] "
            strSor = "create_at"
        End If
        If strDir <> "" Then
            Me.m_strExtraSQL &= " " & strDir
        Else
            Me.m_strExtraSQL &= "ASC"
            strDir = "asc"
        End If
        Me.ddlSort.SelectedValue = strSor
        Me.ddlDirection.SelectedValue = strDir
        Me.ddlFilter.SelectedValue = strFil
        'Me.litDbg.Text = Me.m_strExtraSQL
    End Sub

    'SELECT create_at, create_user, detailhash, due_dt, entity_id, fy_id, guid, hash, hold
    ', invoice_dt, invoice_no, ismultisite, note, region_id, site_id, total_amount, txn_no
    ', vendor_id FROM transactions 
    'WHERE (isexported = 0) AND (isdone = 1) AND (hold = 0) AND (total_amount > 0)
    Private Sub btnRefresh_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRefresh.Click
        ' save filtervars into session variable
        Session("exDir") = Me.ddlDirection.SelectedItem.Value
        Session("exFil") = Me.ddlFilter.SelectedItem.Value
        Session("exSor") = Me.ddlSort.SelectedItem.Value

    End Sub


    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Me.IsManager Then
            Response.Cookies("MSG").Value = "you do not have adequate rights to that page"
            Response.Redirect("~/default.aspx")
        End If
        Server.ScriptTimeout = 600  ' 10 minutes = 600 = 10 * 60seconds
        Me.Toplinks1.GUID = Me.GUID
        Me.usrGUID.Value = Me.GUID
        'lnkAdmin.Visible = Me.IsAdmin
        Dim oTxns As New dsRequestDetailsTableAdapters.txnsTA
        Dim oDets As New dsRequestDetailsTableAdapters.requestdetailsTableAdapter
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

        Me.datedisp.Text = Format(Date.Now, "ddd MMM d, yyyy h:mmtt")
        Dim strMO = Request("mo") & ""
        Dim strYr = Request("yr") & ""
        Dim strEnty$ = Request("en") & ""
        Dim strRegn$ = Request("rn") & ""

        InitFilterDDLS(strEnty, strRegn)

        oTxns.FixTransactionTotals()
        Dim strNote$ = ""
        Dim strSQL$ = "SELECT create_at, create_user, detailhash, due_dt" & _
                        ", entity_id, fy_id, guid, hash, hold, invoice_dt, invoice_no" & _
                        ", ismultisite, note, region_id, site_id, total_amount, txn_no" & _
                        ", vendor_id, COALESCE(DATALENGTH([upload]),0) AS [AttSz]" & _
                        ", sp_notes FROM transactions " & _
                        "WHERE (isexported = 0) AND (isdone = 1) AND (hold = 0) AND (total_amount <> 0) "
        If strMO <> "" And strYr <> "" Then
            Dim dtStart As Date, dtEnd As Date
            dtStart = CDate(strMO & "/1/" & strYr)
            dtEnd = DateAdd(DateInterval.Month, 1, dtStart)
            strSQL &= " AND [create_at] >= '" & Format(dtStart, "yyyy-MM-dd") & _
                    "' AND [create_at] < '" & Format(dtEnd, "yyyy-MM-dd") & "'"
            strNote = "Displaying records from " & Format(dtStart, "MM/dd/yyyy") & _
                    " up to (but not including) " & Format(dtEnd, "MM/dd/yyyy")
        End If
        If Me.m_strExtraSQL.ToString().Length > 0 Then
            strSQL &= " " & Me.m_strExtraSQL
        Else
            If strEnty <> "" Then
                strSQL &= " AND [entity_id]='" & strEnty & "'"
                If strNote.Length > 0 Then strNote &= "; "
                strNote &= "Entity is " & strEnty
            End If
            If strRegn <> "" Then
                strSQL &= " AND [region_id]='" & strRegn & "'"
                If strNote.Length > 0 Then strNote &= "; "
                strNote &= "REGION is " & strRegn
            End If

            strSQL &= " ORDER BY txn_no "
        End If
        Me.litNote.Text = strNote
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
        Dim intRwSp As Integer

        If Not row.HasRows Then
            outrow = New TableRow
            tcell = New TableCell
            tcell.Text = "<div style=""width: 100%; clear: both; height: 50px; text-align: center;"">ERROR: " & strMesg & "&nbsp;no rows to show</div>"
            tcell.ColumnSpan = 9
            outrow.Cells.Add(tcell)
            tblTrans.Rows.Add(outrow)
            Exit Sub
        End If

        Try
            While row.Read()
                outrow = New TableRow

                lngCnt = lngCnt + 1
                tcell = New TableCell
                tcell.Text = "<input type=""checkbox"" id=""txn" & lngCnt & """ name=""txn" & lngCnt & """ value=""" & row("guid") & """ />"
                tcell.CssClass = "apprmaxi appmini-left"
                tcell.RowSpan = 2
                tcell.HorizontalAlign = HorizontalAlign.Right
                tcell.VerticalAlign = VerticalAlign.Middle
                outrow.Cells.Add(tcell)

                tcell = New TableCell
                strLk1 = ""
                strLk2 = ""
                If row("hash") = "approved" Then strLk1 &= "<div style=""float:left;display:inline-block;width:17px;height:18px;""><img align=""top"" height=""16px"" src=""images/star.png"" alt=""ok"" title=""approved!"" /></div>"
                blnAttached = CBool(Convert.ToInt32(row("AttSz")) > 0) 'oTxns.GotAttachment(row("guid"))
                If blnAttached Then
                    strLk1 &= "<a href=""helpers/dld.aspx?guid=" & row("guid") & """ style=""color: maroon; font-weight: bold; text-decoration: none;"">"
                    strLk2 &= "</a>"
                End If
                tcell.Text = strLk1 & "Transaction: " & row("txn_no") & strLk2 & "<br />" & _
                    "<div style=""display: inline; font-size: 7pt;"">Inv#&nbsp;" & row("invoice_no") & "</div>"
                tcell.ColumnSpan = 2
                tcell.VerticalAlign = VerticalAlign.Middle
                tcell.CssClass = "apprmaxismall appmini-top appmini-left"
                outrow.Cells.Add(tcell)

                tcell = New TableCell
                tcell.Text = oUtil.FetchSageTitle(row("entity_id").ToString(), "entity") & "<br />" & vbCrLf & "<span style=""font-size: 7pt;"">" & _
                        "requested by: " & row("create_user").ToString() & ", on " & FormatDateTime(row("create_at").ToString(), DateFormat.ShortDate) & "</span>"
                tcell.ColumnSpan = 3
                tcell.CssClass = "appmini-top apprmaxi"
                outrow.Cells.Add(tcell)

                tcell = New TableCell
                If Not IsDBNull(row("total_amount")) Then _
                        tcell.Text = "Amount: " & FormatNumber(row("total_amount"), _
                                2, TriState.True, TriState.False, TriState.True)
                If Not IsDBNull(row("vendor_id")) Then _
                        tcell.Text &= "<br /><div style=""width: 100%; text-align: right; font-size: 7pt;"">Vendor: " & _
                                oUtil.FetchSageTitle(row("vendor_id"), "vendor") & " [" & row("vendor_id") & "]</div>"
                tcell.CssClass = "appmini-top appmini-right"
                tcell.ColumnSpan = 2
                tcell.HorizontalAlign = HorizontalAlign.Right
                outrow.Cells.Add(tcell)

                tblDets = oDets.GetDataByInvoiceID(row("guid"))

                tcell = New TableCell
                tcell.CssClass = "dotborder"
                intRwSp = tblDets.Rows.Count + 3
                tcell.RowSpan = intRwSp 'tblDets.Rows.Count + 1
                If blnAttached Then
                    tcell.Text = "<a href=""helpers/dld.aspx?guid=" & row("guid") & """" & _
                            "rel=""prettyPhoto"" ><img align=""middle"" " & _
                            "src=""helpers/atn.aspx?rs=" & intRwSp & "&guid=" & row("guid") & _
                            """ alt=""file"" title=""image attached"" /></a>"
                    '                Else
                    '                    tcell.Text = "&nbsp;"
                End If
                tcell.Text &= "<input type=""button"" id=""btnHld" & lngDRW & """ name=""btnHld" & lngDRW & _
                        """ value=""HOLD"" class=""button-secondary"" style=""width: 30px; height: 9px;""" & _
                        " onclick=""hold('" & row("guid") & "');"" />"
                outrow.Cells.Add(tcell)

                tblTrans.Rows.Add(outrow)
                'additional detail -- entity and region
                outrow = New TableRow
                tcell = New TableCell
                tcell.Text = "Entity:"
                tcell.CssClass = "apprmaxi appmini-left"
                tcell.ColumnSpan = 3
                tcell.HorizontalAlign = HorizontalAlign.Right
                tcell.VerticalAlign = VerticalAlign.Middle
                outrow.Cells.Add(tcell)
                tcell = New TableCell
                tcell.Text = row("entity_id")
                tcell.CssClass = "apprmaxi appmini-left"
                tcell.HorizontalAlign = HorizontalAlign.Center
                tcell.VerticalAlign = VerticalAlign.Middle
                outrow.Cells.Add(tcell)
                tcell = New TableCell
                tcell.Text = "Region:"
                tcell.CssClass = "apprmaxi appmini-left"
                tcell.ColumnSpan = 2
                tcell.HorizontalAlign = HorizontalAlign.Right
                tcell.VerticalAlign = VerticalAlign.Middle
                outrow.Cells.Add(tcell)
                tcell = New TableCell
                tcell.Text = row("region_id")
                tcell.ColumnSpan = 3
                tcell.CssClass = "apprmaxi appmini-left"
                tcell.HorizontalAlign = HorizontalAlign.Center
                tcell.VerticalAlign = VerticalAlign.Middle
                outrow.Cells.Add(tcell)
                tblTrans.Rows.Add(outrow)

                ' now for the details

                If tblDets.Rows.Count > 0 Then
                    outrow = New TableRow
                    tcell = New TableCell
                    tcell.CssClass = "appmini-left"
                    tcell.Width = 30
                    outrow.Cells.Add(tcell)
                    tcell = New TableCell
                    tcell.CssClass = "appminiheader"
                    tcell.Text = "&nbsp;"
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
                    'tcell = New TableCell
                    'tcell.CssClass = "appminiheader"
                    'tcell.Text = "&nbsp;"
                    'tcell.Width = "45"
                    'tcell.HorizontalAlign = HorizontalAlign.Center
                    'outrow.Cells.Add(tcell)
                    tblTrans.Rows.Add(outrow)
                End If
                For Each oDetls In tblDets.Rows
                    outrow = New TableRow
                    tcell = New TableCell
                    tcell.CssClass = "appmini-left"
                    outrow.Cells.Add(tcell)

                    tcell = New TableCell
                    tcell.CssClass = "appmini-left"
                    lngDRW = lngDRW + 1
                    With oDetls
                        tcell.Text = "<input type=""button"" id=""btnZ" & lngDRW & """ name=""btnZ" & lngDRW & _
                                """ value=""i"" class=""button-secondary serifate"" style=""width: 9px; height: 9px;""" & _
                                " onclick=""opendetails('" & .GUID & "');"" />" & _
                                "<input type=""hidden"" id=""gidZ" & lngDRW & """ name=""gidZ" & lngDRW & """ " & _
                                "value=""" & .GUID & """ />"
                        tcell.CssClass = "redbottomborder appmini-left"
                        tcell.HorizontalAlign = HorizontalAlign.Right
                        outrow.Cells.Add(tcell)

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
                tcell.CssClass = "appmini-left appmini-bottom"
                outrow.Cells.Add(tcell)
                tcell = New TableCell
                tcell.CssClass = "appmini appmini-right"
                Dim strHold$ = ""
                strNote = "&nbsp;"
                Dim strSpNt$ = ""
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
                                        "<div style=""padding: 0px; margin: 0px; text-align: left; "">" & hrDtl.note & "</div></div>" & vbCrLf
                            End If
                        Next
                        strHold &= "</div>"
                    End If
                End Using
                If Not IsDBNull(row("note")) Then
                    strNote = "<div style=""text-align: left; padding: 0px 1px 0px 2px;"">" & row("note") & "</div>"
                End If
                if Not IsDBNull(row("sp_notes")) then
                    strSpNt = "<div style=""border-top: 1px dotted #cbb; text-align: left; padding: 2px 1px 5px 5px; overflow: auto;""><u>A/P NOTE</u>:" & row("sp_notes") & "<br style=""clear:both;"" /></div>"
                End If
                tcell.Text = "Inv #:<b>" & row("invoice_no") & "</b><br />" & strNote & strHold & strSpNt
                If row("hash") = "approved" Then
                    tcell.Text &= oUtil.FetchApprovalInfo(row("guid"))
                End If

                tcell.ColumnSpan = 8
                outrow.Cells.Add(tcell)
                tblTrans.Rows.Add(outrow)

                outrow = New TableRow
                tcell = New TableCell
                tcell.Text = "&nbsp;"
                tcell.ColumnSpan = 8
                outrow.Cells.Add(tcell)
                tblTrans.Rows.Add(outrow)
            End While

            outrow = New TableRow
            tcell = New TableCell
            tcell.CssClass = "apprmaxi"
            tcell.Text = "&nbsp;nothing follows"
            tcell.ColumnSpan = 9
            outrow.CssClass = "apprmaxi"
            outrow.Cells.Add(tcell)
            tblTrans.Rows.Add(outrow)
        Catch ex As Exception
            Debug.Print(ex.Message.ToString())
            'ListBox1.Items.Add("Exception: " & ex.Message)
        Finally
            oDets.Dispose()
            oTxns.Dispose()
            row.Close()
            cn.Close()
        End Try
        btnTest.Visible = False
        btnExport.Visible = CBool(lngCnt > 0)
    End Sub


    Private Sub btnExport_OLD_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        ' do that export thing
        Dim strDLFdr$ = My.Settings.DownloadPath
        Dim oHash As New zHashes
        Dim strFileName$, strFileBase$
        strFileBase = Path.Combine(strDLFdr, oHash.MkGUID)
        strFileName = strFileBase & ".xls"
        Dim oFStream As FileStream = New FileStream(strFileName, FileMode.Create)
        'Dim oSetTA As dsRequestDetailsTableAdapters.apsetupTA
        Dim oTxnTA As New dsRequestDetailsTableAdapters.txnsTA
        Dim oDetTA As New dsRequestDetailsTableAdapters.requestdetailsTableAdapter

        Dim intActives As Integer = 0
        'Response.Write("STEP 1<br />")
        Try
            intActives = oTxnTA.GetActiveCount
        Catch ex As Exception
            ' ow!
            Response.Cookies("MSG").Value = "zero active count"
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
            oXL.WriteCell(0, 0, "Session #")        ' A
            oXL.WriteCell(0, 1, "Doc #")            ' B
            oXL.WriteCell(0, 2, "Doc Date")         ' C
            oXL.WriteCell(0, 3, "Due Date")         ' D
            oXL.WriteCell(0, 4, "Entity")           ' E
            oXL.WriteCell(0, 5, "Region")           ' F
            oXL.WriteCell(0, 6, "Site")             ' G
            oXL.WriteCell(0, 7, "Department")       ' H
            oXL.WriteCell(0, 8, "Service")          ' I
            oXL.WriteCell(0, 9, "GL")               ' J
            oXL.WriteCell(0, 10, "Fiscal Year")     ' K
            oXL.WriteCell(0, 11, "Payer Source")    ' L
            oXL.WriteCell(0, 12, "Person Code")     ' M
            oXL.WriteCell(0, 13, "Vendor ID")       ' N
            oXL.WriteCell(0, 14, "Debit")           ' O
            oXL.WriteCell(0, 15, "Invoice #")       ' P
            oXL.WriteCell(0, 16, "Description")     ' Q
            'Response.Write("STEP 3: Header written<br />")
            Dim strPrefix$ = My.Settings.ReqPrefix.Trim()
            Dim lngReq As Long = 0
            For intI = 0 To intActives - 1
                strTgt = "txn" & (intI + 1)
                strTV = Request(strTgt)
                If strTV <> "" Then
                    'Try
                    Using oSetTA As New dsRequestDetailsTableAdapters.apsetupTA
                        lngReq = oSetTA.GetLatestRequest()
                    End Using
                    'Catch ex As Exception
                    'Response.Write("ERR: " & ex.Message)
                    'lngReq = 1000
                    'End Try
                    ' get everything by GUID 
                    oDetTA.RefreshTotals(strTV)
                    'Try
                    '    LogMessage("GUID:" & strTV & " exported", strTV, "", Me.GUID, Request)
                    'Catch ex As Exception
                    '    ' do nothing
                    'End Try

                    Dim oTxnTbl As dsRequestDetails.txnsDataTable = oTxnTA.GetByGUID(strTV)
                    Dim oDetTbl As dsRequestDetails.requestdetailsDataTable = oDetTA.GetDataByInvoiceID(strTV)
                    Dim oTxn As dsRequestDetails.txnsRow = oTxnTbl.Rows(0)

                    If oTxn.hash = "approved" Then
                        If oTxnTbl.Rows.Count > 0 Then
                            Dim oDetail As dsRequestDetails.requestdetailsRow
                            For Each oDetail In oDetTbl.Rows
                                oXL.WriteCell(intRow, 0, strPrefix & lngReq)
                                oXL.WriteCell(intRow, 1, "Transaction ID" & oTxn.txn_no)
                                oXL.WriteCell(intRow, 2, oTxn.create_at.ToString("MM/dd/yyyy"))
                                If Not oTxn.Isdue_dtNull Then oXL.WriteCell(intRow, 3, oTxn.due_dt.ToString("MM/dd/yyyy"))
                                If Not oTxn.Isentity_idNull Then oXL.WriteCell(intRow, 4, oTxn.entity_id)
                                If Not oTxn.Isregion_idNull Then oXL.WriteCell(intRow, 5, oTxn.region_id)
                                If oTxn.ismultisite Then
                                    If Not oDetail.site_id Then oXL.WriteCell(intRow, 6, oDetail.site_id)
                                Else
                                    If Not oTxn.Issite_idNull Then oXL.WriteCell(intRow, 6, oTxn.site_id)
                                End If
                                If Not oDetail.IsDeptCodeNull Then oXL.WriteCell(intRow, 7, oDetail.DeptCode)
                                If Not oDetail.IsServCodeNull Then oXL.WriteCell(intRow, 8, oDetail.ServCode)
                                If Not oDetail.IsGLAcctCodeNull Then oXL.WriteCell(intRow, 9, oDetail.GLAcctCode)
                                If Not oTxn.Isfy_idNull Then oXL.WriteCell(intRow, 10, oTxn.fy_id)
                                If Not oDetail.IsPayerCodeNull Then oXL.WriteCell(intRow, 11, oDetail.PayerCode)
                                If Not oDetail.IsPersonNull Then oXL.WriteCell(intRow, 12, oDetail.Person)
                                If Not oTxn.Isvendor_idNull Then oXL.WriteCell(intRow, 13, oTxn.vendor_id)
                                oXL.WriteCell(intRow, 14, oDetail.Amount)
                                If Not oTxn.Isinvoice_noNull Then oXL.WriteCell(intRow, 15, oTxn.invoice_no)
                                If Not oTxn.IsnoteNull Then oXL.WriteCell(intRow, 16, oTxn.note)
                                LogMessage("Exported to Req #" & lngReq, oTxn.guid, oDetail.GUID, Me.GUID, Request)
                                intRow += 1
                            Next

                        End If
                        'intRow += 1
                        ' ya might want to mark this as 'exported' now...?
                        'oTxnTA.FlagExportByGUID(strTV)
                        Using oSetTA As New dsRequestDetailsTableAdapters.apsetupTA
                            oSetTA.IncrementReq()
                        End Using
                    End If
                End If
                'Response.Write(strTgt & "=[" & strTV & "]<br />" & vbCrLf)
                'Response.Write("STEP 4: Line out: " & intRow & "<br />")
            Next
            oXL.EndWrite()
            'Dim xl As ZXLSWriter
            oFStream.Close()
            'Response.Write("STEP 5: Marking rows<br />")

            ' Phase 2... mark as exported
            'For intI = 0 To intActives - 1
            '    strTgt = "txn" & (intI + 1)
            '    strTV = Request(strTgt)
            '    If strTV <> "" Then
            '        oTxnTA.FlagExportByGUID(strTV)
            '    End If
            'Next
            'Response.Write("STEP 6: Starting download<br />")

            ' perform download
            Dim strNiceName As String = Now.ToString("yyyyMMddHHmm") & ".csv"
            If True Then 'False Then
                Response.ContentType = "application/octet-stream"
                Dim cd As New ContentDisposition
                cd.Inline = False
                cd.FileName = strNiceName '"export.xls"
                Response.AppendHeader("Content-Disposition", cd.ToString())
                Dim filedata() As Byte = File.ReadAllBytes(strFileName)
                Response.OutputStream.Write(filedata, 0, filedata.Length)
                'Response.Cookies("MSG").Value = "export done"
            End If
        End If
    End Sub

    Private Sub btnApprove_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnApprove.Click
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
            Response.Redirect("~/exporter.aspx")
        End If
    End Sub

    Private Function AppendData(ByVal IsNullVal As Boolean, ByVal myval As Object)
        If IsNullVal Then
            Return ""
        Else
            If TypeOf myval Is DateTime Then
                Dim dtTmp As DateTime = myval
                Dim dtRef As DateTime = #1/1/2012#
                If dtTmp < dtRef Then
                    Return ""
                Else
                    Return dtTmp.ToString("MM/dd/yyyy")
                End If
            Else
                Return myval
            End If
        End If
    End Function

    Private Sub btnExport_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnExport.Click
        ' do that export thing
        Dim strDLFdr$ = My.Settings.DownloadPath
        Dim oHash As New zHashes
        Dim strFileName$, strFileBase$
        Dim strNiceName As String
        Dim strFNGUID$ = oHash.MkGUID
        strFileBase = Path.Combine(strDLFdr, strFNGUID)
        strFileName = strFileBase & ".csv"
        
        Dim oTxnTA As New dsRequestDetailsTableAdapters.txnsTA
        Dim oDetTA As New dsRequestDetailsTableAdapters.requestdetailsTableAdapter

        Dim intActives As Integer = 0
        'Response.Write("STEP 1<br />")
        Try
            intActives = oTxnTA.GetActiveCount
        Catch ex As Exception
            ' ow!
            Response.Cookies("MSG").Value = "zero active count"
        End Try

        Dim blnSim As Boolean = Me.chkSim.Checked

        'Response.Write("STEP 2: found " & intActives & "<br />")
        If intActives > 0 Then
            ' ADDED 150611 -- effective date
            Dim strEffDate$
            Dim dtEffDate As DateTime
            strEffDate = Me.txtEffDate.Text & ""
            If IsDate(strEffDate) then
                dtEffDate = Convert.ToDateTime(strEffDate)
            Else
                dtEffDate = Date.Today
            End If            
            strEffDate= dtEffDate.ToString("MM/dd/yyyy") 

            strNiceName = Now.ToString("yyyyMMddHHmm") '& ".csv"
            strFileBase = Path.Combine(strDLFdr, strNiceName)
            If Not System.IO.Directory.Exists(strFileBase) Then System.IO.Directory.CreateDirectory(strFileBase)
            'strNiceName &= ".csv"
            strFileName = Path.Combine(strFileBase, strNiceName & ".csv")

            ' open the file for writing
            Dim oFWriter As New StreamWriter(strFileName)

            Dim intI As Integer
            Dim strTV As String, strTgt As String
            Dim oUtl As New ZUtils
            Dim intRow As Integer = 1

            Dim strPrefix$ = My.Settings.ReqPrefix.Trim()
            Dim lngReq As Long = 0
            Try
                Using oSetTA As New dsRequestDetailsTableAdapters.apsetupTA
                    lngReq = oSetTA.GetLatestRequest()
                    oSetTA.IncrementReq()
                End Using
            Catch ex As Exception
                Response.Write("ERR: " & ex.Message)
                'lngReq = 1000
            End Try


            For intI = 0 To intActives - 1
                strTgt = "txn" & (intI + 1)
                strTV = Request(strTgt)
                If strTV <> "" Then
                    Try
                        ' get everything by GUID 
                        oDetTA.RefreshTotals(strTV)
                        'LogMessage("GUID:" & strTV & " exported", strTV, "", Me.GUID, Request)
                    Catch ex As Exception
                        ' do nothing
                        oFWriter.WriteLine("ERR: trying to get [" & strTV & "] ERR: " & ex.Message)
                    End Try

                    Dim oTxnTbl As dsRequestDetails.txnsDataTable = oTxnTA.GetByGUID(strTV)
                    Dim oDetTbl As dsRequestDetails.requestdetailsDataTable = oDetTA.GetDataByInvoiceID(strTV)
                    Dim oTxn As dsRequestDetails.txnsRow = oTxnTbl.Rows(0)
                    Dim strTxnLine$ = ""
                    Dim strTxnNo$ = ""

                    If oTxn.hash = "approved" Then
                        If oTxnTbl.Rows.Count > 0 Then
                            Dim oDetail As dsRequestDetails.requestdetailsRow
                            For Each oDetail In oDetTbl.Rows
                                strTxnNo = oTxn.txn_no.ToString().Trim()
                                strTxnLine &= """" & strPrefix & lngReq & ""","
                                strTxnLine &= """Transaction ID" & strTxnNo & ""","
                                strTxnLine &= """" & oTxn.create_at.ToString("MM/dd/yyyy") & ""","
                                If Not oTxn.Isdue_dtNull Then
                                    strTxnLine &= """" & oTxn.due_dt.ToString("MM/dd/yyyy") & ""","
                                Else
                                    strTxnLine &= ","
                                End If
                                strTxnLine &= """" & AppendData(oTxn.Isentity_idNull, oTxn.entity_id) & ""","
                                strTxnLine &= """" & AppendData(oTxn.Isregion_idNull, oTxn.region_id) & ""","
                                'strTxnLine &= """" & AppendData(oTxn.Issite_idNull, oTxn.site_id) & ""","
                                If oTxn.ismultisite Then
                                    If Not oDetail.Issite_idNull Then
                                        strTxnLine &= """" & oDetail.site_id & ""","
                                    Else
                                        strTxnLine &= ","
                                    End If
                                Else
                                    If oTxn.Issite_idNull Then
                                        strTxnLine &= ","
                                    Else
                                        strTxnLine &= """" & AppendData(oTxn.Issite_idNull, oTxn.site_id) & ""","
                                    End If

                                End If

                                strTxnLine &= """" & AppendData(oDetail.IsDeptCodeNull, oDetail.DeptCode) & ""","
                                strTxnLine &= """" & AppendData(oDetail.IsServCodeNull, oDetail.ServCode) & ""","
                                strTxnLine &= """" & AppendData(oDetail.IsGLAcctCodeNull, oDetail.GLAcctCode) & ""","
                                If Not oTxn.Isfy_idNull Then
                                    strTxnLine &= """" & oTxn.fy_id & ""","
                                Else
                                    strTxnLine &= ","
                                End If
                                strTxnLine &= """" & AppendData(oDetail.IsPayerCodeNull, oDetail.PayerCode) & ""","
                                strTxnLine &= """" & AppendData(oDetail.IsPersonNull, oDetail.Person) & ""","
                                strTxnLine &= """" & AppendData(oTxn.Isvendor_idNull, oTxn.vendor_id) & ""","
                                strTxnLine &= oDetail.Amount & ","
                                strTxnLine &= """" & AppendData(oTxn.Isinvoice_noNull, oTxn.invoice_no) & ""","
                                strTxnLine &= """" & Now.ToString("MM/dd/yyyy") & ""","
                                strTxnLine &= """"
                                If Not oTxn.IsnoteNull Then
                                    Dim strNote$ = oTxn.note.Replace("""", "'")
                                    If strNote.Length > (60 - strTxnNo.Length + 1) Then
                                        strTxnLine &= strNote.Substring(0, 60 - (strTxnNo.Length + 1)) & " " & strTxnNo
                                    Else
                                        strTxnLine &= strNote & " " & strTxnNo
                                    End If
                                End If
                                strTxnLine &= ""","
                                If Not oTxn.Isinvoice_dtNull Then
                                    strTxnLine &= """" & oTxn.invoice_dt.ToString("MM/dd/yyyy") & """"
                                End If

                                ' added 2015-06-11 Effective date
                                strTxnLine &= ",""" & strEffDate & """"

                                LogMessage("Exported to Req #" & lngReq, oTxn.guid, oDetail.GUID, Me.GUID, Request)
                                ' actual write out to string
                                oFWriter.WriteLine(strTxnLine)
                                strTxnLine = ""
                                intRow += 1
                            Next
                            oFWriter.Flush()

                            ' ya might want to mark this as 'exported' now...?
                            If Not blnSim Then oTxnTA.FlagExportByGUID(strTV)

                            ' download a PDF record of this
                            Me.DldPDF(strFileBase, strTxnNo, oTxn.guid)

                        End If
                        'intRow += 1
                    End If
                End If
                'Response.Write(strTgt & "=[" & strTV & "]<br />" & vbCrLf)
                'Response.Write("STEP 4: Line out: " & intRow & "<br />")
            Next
            oFWriter.Close()
            'Dim xl As ZXLSWriter
            LogMessage("Export done: " & strNiceName, strFNGUID, "", Me.GUID, Request)

            ' now zip it up
            Dim strZipFile = strFileBase & ".zip"
            Using zip As New Ionic.Zip.ZipFile
                zip.AddDirectory(strFileBase)
                zip.Save(strZipFile)
            End Using

            Directory.Delete(strFileBase, True)

            ' perform download
            If True Then 'False Then
                ' redirect to something else!
                Session("NSF") = strZipFile
                Dim strFH =oHash.EncryptData( strZipFile )
                strFH = System.Web.HttpUtility.UrlEncode(strFH)
                Response.Clear()
                Response.Redirect("~/dld.aspx?FSN=" & strFH)
                
            End If
        End If
    End Sub

    Private Function DownloadFile(ByVal url As String, ByVal toLocalFile As String) As String
        Dim result() As Byte
        Dim buffer(4097) As Byte
        Dim wr As WebRequest
        Dim response As WebResponse
        Dim responseStream As Stream
        Dim memoryStream As New MemoryStream
        Dim count As Integer = 0

        Try
            wr = WebRequest.Create(url)
            response = wr.GetResponse()
            responseStream = response.GetResponseStream()

            Do
                count = responseStream.Read(buffer, 0, buffer.Length)
                memoryStream.Write(buffer, 0, count)
                If count = 0 Then Exit Do
            Loop While True

            result = memoryStream.ToArray

            Dim fs As New FileStream(toLocalFile, FileMode.OpenOrCreate, FileAccess.ReadWrite)
            fs.Write(result, 0, result.Length)
            fs.Close()

            memoryStream.Close()
            responseStream.Close()
        Catch ex As Exception
            'exception!
            Return "OOPS: " & Err.Description
        End Try

        Return "OK"
    End Function

    Private Sub DldPDF(ByVal strFolder$, ByVal strTxn$, ByVal strTxnGUID$)
        Dim strUrl As String = My.Settings.PDFurl.ToString().Trim()
        Dim strFld$, strResult$
        If IsNumeric(strTxn) Then
            strFld = Path.Combine(strFolder, strTxn & "-TXN.pdf")
            strUrl &= "?txn=" & strTxn
            strResult = DownloadFile(strUrl, strFld)
            LogMessage("Txn-" & strTxn & " export to PDF with result: " & strResult & " (to " & strFld & ")", strTxnGUID, "", Me.GUID, Request)
        Else
            LogMessage("DldPDF - ignoring input: [" & strTxn & "]", strTxnGUID, "", Me.GUID, Request)
        End If
    End Sub

    Private Sub btnTest_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnTest.Click
        Dim strUrl As String = My.Settings.PDFurl.ToString().Trim()
        Dim strFld As String = My.Settings.DownloadPath
        Dim oHash As New zHashes
        oHash.MkGUID()
        strFld = Path.Combine(strFld, "tst" & oHash.MkGUID & ".pdf")

        strUrl &= "?txn=745"
        Me.errorLabel.Value = DownloadFile(strUrl, strFld) & " --> " & strFld
    End Sub

    Private Sub lnkBack_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lnkBack.Click
        Response.Redirect("~/ex_picker.aspx")
    End Sub

    Protected Sub btnCRQ_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnCRQ.Click
        'Response.Redirect("~/chkreqqueue.aspx")
        Dim strDLFdr$ = My.Settings.DownloadPath
        Dim oHash As New zHashes
        Dim strFileName$, strFileBase$, strMesg$
        Dim strNiceName$ = "Request_Queue.csv"
        Dim strFNGUID$ = "Q_exp_" & oHash.MkGUID
        strFileBase = Path.Combine(strDLFdr, strFNGUID)
        strFileName = strFileBase & ".csv"
        
        Server.ScriptTimeout = 600
        Dim strSQL$ = "SELECT txn.[txn_no]" & _
	            ", txn.[entity_id]" & _
	            ", txn.[vendor_id]" & _
	            ", CONVERT(VARCHAR, txn.[create_at], 101) AS [request date]" & _
	            ", CONVERT(VARCHAR, txn.[due_dt], 101) AS [due date]" & _
	            ", COALESCE(ucf.[fullname], txn.[create_user]) AS [requested by]" & _
	            ", txn.[total_amount] AS [amount]" & _
	            ", txn.[hash]" & _
	            ", txn.[isdone]" & _
	            ", txn.[isexported]" & _
	            ", txn.[isapproved] " & _
                "FROM [transactions] AS txn " & _
	            "LEFT JOIN [userconfig] AS ucf ON txn.[create_user]=ucf.[username] " & _
                "WHERE [due_dt] > '2010-01-01' AND [isapproved] <> 1 AND [isexported] <> 1 AND [hash]<>'approved' " & _
                "ORDER BY [due_dt]"
        Dim strCN$ = ConfigurationManager.ConnectionStrings("aprequestConnectionString").ConnectionString
        Dim cn As New SqlClient.SqlConnection(strCN)
        cn.Open()        

        Dim cmd As New SqlClient.SqlCommand(strSQL, cn)
        cmd.CommandType = CommandType.Text
        
        Dim row As SqlDataReader = Nothing
        Try          
            row = cmd.ExecuteReader()
        Catch ex As Exception
            strMesg = ex.Message
            Response.Write("<pre>" & strMesg & "</pre>")
            Exit Sub
        End Try

        ' open the file for writing
        Dim oFWriter As New StreamWriter(strFileName)
        Dim oU As New ZUtils
        If (row.HasRows) Then
            Dim strEnt$, strVen$
            oFWriter.WriteLine("""Txn#"",""Entity ID"",""Entity"",""Vendor""" & _
                    ",""Request Date"",""Due Date"",""Requested By"",""Amount""")
            While row.Read()
                strEnt = oU.FetchSageTitle(row("entity_id"), "entity")
                strVen = oU.FetchSageTitle(row("vendor_id"), "vendor")
                oFWriter.WriteLine(row("txn_no") & "," & row("entity_id") & "," & _
                             """" & strEnt & """,""" & strVen & """," & _
                             """" & row("request date") & """,""" & row("due date") & """," & _
                             """" & row("requested by") & """," & row("amount") )
            End While
            oFWriter.Close()

            Session("NSF") = strFileName
            Dim strFH = oHash.EncryptData( strFileName )
            strFH = System.Web.HttpUtility.UrlEncode(strFH)
            Response.Clear()
            Response.Redirect("~/dld.aspx?FSN=" & strFH)
        End If
    End Sub
End Class