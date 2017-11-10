Imports System.Drawing
Imports System.Data
Imports System.Data.SqlClient
Imports System.Net.Mail
Imports System.Threading

Partial Public Class txns_add
    Inherits ZPageSecure
    'Inherits System.Web.UI.Page
    Private txnsLock As Object
    Private Shared mutInsaneTxn As New Mutex()

    Private Function GetFinalTxnNo(Optional ByVal GUID As String="ANONYMOUS") As Integer
        Dim intTxnNo As Long = -1
        Dim strCN As String = ConfigurationManager.ConnectionStrings("aprequestConnectionString").ConnectionString
        Dim strSQL= "INSERT INTO [txn_numbers] (guid) VALUES (@txnguid); SELECT SCOPE_IDENTITY()"
        Using cn As New SqlClient.SqlConnection(strCN)
            Try
                cn.Open()                        
                Dim cmd As New SqlClient.SqlCommand(strSQL, cn)
                cmd.Parameters.AddWithValue("@txnguid", GUID.Trim)
                intTxnNo = cmd.ExecuteScalar()
            Catch ex As Exception
                intTxnNo = -2
            End Try
        End Using
        if intTxnNo>0 then
            Try
                Using oSet As New dsRequestDetailsTableAdapters.apsetupTA
                    oSet.SetTxn(intTxnNo)
                End Using
            Catch ex As Exception
                ' do nothing
            End Try
        End If
        Return intTxnNo
    End Function

    Private Function GetFinalTxnNo_OLD() As Integer
        Dim intTxnNo As Long = -1

        Try
            mutInsaneTxn.WaitOne()
            Using oSet As New dsRequestDetailsTableAdapters.apsetupTA
                intTxnNo = oSet.GetLatestTransaction()
                oSet.IncrementTxn()
            End Using
        Catch ex As Exception
            LogMessage("GetFinalTxnNo Exception: " & ex.Message, "", "", "", Nothing)
        Finally
            mutInsaneTxn.ReleaseMutex()
        End Try

        Return intTxnNo
    End Function

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim CookieName As String = FormsAuthentication.FormsCookieName
        Dim authCookie As HttpCookie = Request.Cookies(CookieName)
        Dim encryptedTicket As String = authCookie.Value
        Dim authTick = FormsAuthentication.Decrypt(encryptedTicket)
        Dim Username = authTick.Name
        Dim oHash = New zHashes
        Dim strGUID = oHash.MkGUID

        Dim strAJX$, strTxn$, strCEntity$, strCRegion$, strCSiteNm$, strCVendor$
        Dim strTEntity$, strTRegion$, strTSiteNm$, strTVendor$, strNote$, strSpNt$
        Dim strFY$, strInvoice$, strDate$, strDt2$, strAmt$
        Dim lngInvoice As Long = 0, sngAmt As Single = -1
        Dim dtInvDate As Date = Convert.ToDateTime("1/1/1980")
        Dim dtDue As Date = Convert.ToDateTime("1/1/1980")
        Dim lngTxn As Long
        Try
            strGUID = Request("guid")
            strAJX = Request("ajx")
            strTxn = Request("txn")
            strCEntity = Request("code_entity")
            strCRegion = Request("code_region")
            strCSiteNm = Request("code_site")
            strCVendor = Request("vendorid")
            strFY = Request("ddFiscalYr")
            strInvoice = Request("txtInvNo")
            strDate = Request("txtInvDate")
            strDt2 = Request("txtDueDate")
            strAmt = Request("txtTotalAmount") & ""
            strAmt = strAmt.Replace(",", "").Trim()
            If strAmt.IndexOf("(") >= 0 Then
                strAmt = "-" & strAmt.Replace("(", "").Replace(")", "")
            End If
            strNote = Left(Request("txtNote") & "", 50)
            If Len(Request("txtnote")) > 50 Then strNote &= "..."
            strSpNt = Left(Request("txtAPNote") & "", 97)
            If Len(Request("txtAPNote")) > 97 Then strSpNt &= "..."

            If IsDate(strDate) Then
                dtInvDate = Convert.ToDateTime(strDate)
            End If
            If IsDate(strDt2) Then
                dtDue = Convert.ToDateTime(strDt2)
            End If
            'If IsNumeric(strInvoice) Then
            'lngInvoice = Convert.ToInt64(strInvoice)
            'End If
            If IsNumeric(strAmt) Then
                sngAmt = Convert.ToSingle(strAmt)
            End If
            If Not IsNumeric(strTxn) Then
                lngTxn = GetFinalTxnNo( strGUID )
                strTxn =lngTxn.ToString()
            Else
                lngTxn = Convert.ToInt64(strTxn)
            End If
            Dim retry as Long = 0
            Do While lngTxn <= 0 and retry < 10
                System.Threading.Thread.Sleep(100)
                lngTxn = GetFinalTxnNo( strGUID )
                retry += 1
            Loop
            strTxn =lngTxn.ToString()
            If lngTxn<0 then
                Response.Write("{ ""error"":5," & _
                    """message"":""Unable to generate a viable TRANSACTION NUMBER"" }")
                Exit Sub
            End If


            'If IsNumeric(strFY) Then
            '    Dim iTmp = Convert.ToInt32(strFY)
            '    If iTmp > 2010 And iTmp < 2100 Then
            '        strFiscalYr = Left(strFY, 4)
            '    End If
            'End If
        Catch EX As Exception
            Response.Write("{ ""error"":1," & _
                """message"":""Param fetch:\n\n" & EX.Message & """ }")
            Exit Sub
        End Try

        Dim blnErr As Boolean = False
        Dim strMesg$ = ""
        If sngAmt = 0 Then
            strMesg &= "<li>Invalid amount<br />(orig: " & strAmt & ", converted to: " & sngAmt & ")</li>"
            blnErr = True
        End If
        If strNote.Length <= 0 Then
            strMesg &= "<li>Fill in the note field</li>"
            blnErr = True
        End If
        If strInvoice.Length <= 0 Then
            strMesg &= "<li>Specify an invoice number</li>"
            blnErr = True
        End If
        'If lngInvoice. <= 0 Then
        'strMesg &= "Invalid invoice number\n"
        'blnErr = True
        'End If
        If dtInvDate <= Convert.ToDateTime("1/1/1980") Then
            strMesg &= "<li>Invalid invoice date</li>"
            blnErr = True
        End If
        ' TODO: details must be checked!
        If strCEntity = "" Then
            strMesg &= "<li>Entity value is invalid</li>"
            blnErr = True
        End If
        If strCRegion = "" Then
            strMesg &= "<li>Region value is invalid</li>"
            blnErr = True
        End If
        ' TODO: something better surely?
        ' If strCSiteNm = "" Then
        '     strMesg &= "<li>Site name is invalid</li>"
        '     blnErr = True
        ' End If
        If strCVendor = "" Then
            strMesg &= "<li>Vendor is invalid</li>"
            blnErr = True
        End If

        If blnErr Then
            Response.Write("{ ""error"":1," & _
                """message"":""The following problems were detected:<ul>" & strMesg & "</ul><br /><br />"" }")
            Exit Sub
        End If
        Try
            Dim oUtil = New ZUtils
            strTEntity = oUtil.FetchSageTitle(strCEntity, "entity")
            strTRegion = oUtil.FetchSageTitle(strCRegion, "region")
            If strCSiteNm <> "" Then strTSiteNm = oUtil.FetchSageTitle(strCSiteNm, "site")
            strTVendor = oUtil.FetchSageTitle(strCVendor, "vendor")
        Catch ex As Exception
            Response.Write("{ ""error"":3," & _
                """message"":""Sage lookup:<br />" & ex.Message & """ }")
            Exit Sub
        End Try

        Dim oRT As New dsRequestDetailsTableAdapters.txnsTA
        Dim iCnt As Integer = -1
        Try
            iCnt = oRT.TxnExists(strGUID)
            If iCnt > 0 Then
                ' it exists, so do an update instead
                LogMessage("Updating " & strTxn & " with: " & vbCrLf & _
                            "Entity: " & strCEntity & vbCrLf & _
                            "Region: " & strCRegion & vbCrLf & _
                            "Site Name: " & strCSiteNm & vbCrLf & _
                            "Vendor: " & strCVendor & vbCrLf & _
                            "Invoice: " & strInvoice & vbCrLf & _
                            "Amount: " & sngAmt & vbCrLf & _
                            "Inv Date: " & dtInvDate & vbCrLf & _
                            "Due Date: " & dtDue & vbCrLf & _
                            "FY: " & strFY, strGUID, "", "", Request)
                oRT.TxnUpdate(strTxn, strCEntity, strCRegion, strCSiteNm, strCVendor, _
                              strInvoice, sngAmt, dtInvDate, dtDue, strFY, "", Me.GUID, strNote, strSpNt, strGUID)
                'lngTxn = Convert.ToInt64(oRT.GetTxnNo(strGUID))
                'If lngTxn < 700 Then
                'lngTxn = GetFinalTxnNo() ' Convert.ToInt64(oRT.FixTransactionNumber(strGUID))
                'End If
            Else
                LogMessage("INSERTING " & strTxn & " with: " & vbCrLf & _
                        "Entity: " & strCEntity & vbCrLf & _
                        "Region: " & strCRegion & vbCrLf & _
                        "Site Name: " & strCSiteNm & vbCrLf & _
                        "Vendor: " & strCVendor & vbCrLf & _
                        "Invoice: " & strInvoice & vbCrLf & _
                        "Amount: " & sngAmt & vbCrLf & _
                        "Inv Date: " & dtInvDate & vbCrLf & _
                        "Due Date: " & dtDue & vbCrLf & _
                        "FY: " & strFY, strGUID, "", Me.GUID, Request)
                ' nonexistent, do a straight insert
                oRT.TxnInsert(strGUID, strTxn, strNote, strCEntity, strCRegion, strCSiteNm, strCVendor, _
                                strInvoice, sngAmt, dtInvDate, dtDue, strFY, Username, "", "",strSpNt)
                'lngInvoice.ToString(), sngAmt, dtInvDate, strFY,Username, "", "")
                ' now fix transaction number to a real value
                'lngTxn = Convert.ToInt64(oRT.GetTxnNo(strGUID))
            End If
        Catch ex As Exception
            Response.Write("{ ""error"":4," & _
                """message"":""DB Sync (" & iCnt & "):\n\n" & ex.Message & """ }")
            LogMessage("ERROR: (4): " & ex.Message, "", "", Me.GUID, Request)
            Exit Sub
        End Try
        strTxn = lngTxn.ToString()
        Try
            oRT.MarkDone(True, strGUID)
            oRT.SetMultiSite(Me.IsMultisite, strGUID)
            LogMessage("MULTISITE set to " & Me.IsMultisite & vbCrLf, "", "", Me.GUID, Request)
        Catch ex As Exception
            LogMessage("ERROR: MarkDone(): " & ex.Message, "", "", Me.GUID, Request)
        End Try

        ' decide whether you auto-approve this request or not
        Dim dblLimit As Double = Me.AmtLimit()
        If sngAmt <= dblLimit Then
            oRT.SetHash("approved", strGUID)
            oRT.SetApproval(True, Username, strGUID)
            LogMessage("SETTING Transaction " & strTxn & " APPROVED. (GUID: " & strGUID & ")", strGUID, "", Me.GUID, Request)
        Else
            oRT.SetHash("", strGUID)
            oRT.SetApproval(False, "", strGUID)
            Dim strMsg$ = "<p>Approval needed for transaction " & strTxn & ".</p>" ' (GUID: " & strGUID & ")" & _
            strMsg &= vbCrLf & vbCrLf & "<br /><br /><br /><br />" & _
                    "<a href=""http://webserv8/aprequest/editreq.aspx?LDTXN=" & strGUID & _
                    """>Click here to view</a>" & _
                    vbCrLf & vbCrLf & _
                    "<br /><br /><br /><br />" & _
                    "<p>If outside the organization, please use the link below:</p>" & _
                    "<a href=""https://aprequest.ascentria.org/editreq.aspx?LDTXN=" & strGUID & _
                    """>https://aprequest.ascentria.org</a><br /><br /><br /><br />" & _
                    "."
            LogMessage("RESETTING approval flag for transaction " & strTxn & ". (GUID: " & strGUID & ")", strGUID, "", Me.GUID, Request)
            SendEMailMessage(strMsg, strGUID)
        End If

        ' set approval bit to 0 if not.
        Response.Write("{ ""error"":0," & _
            """txn_no"":""" & lngTxn & """," & _
            """message"":""ok"" }")
    End Sub

End Class