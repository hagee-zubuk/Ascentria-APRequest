Imports System.Drawing
Imports System.Data
Imports System.Data.SqlClient


Partial Public Class detailrow_add
    'Inherits System.Web.UI.Page
    Inherits ZPageSecure

    Private Function GetStringWidth(ByVal sTmp$, ByVal sFnt$, ByVal iSz As Long) As Single
        Dim objBitmap As New Bitmap(500, 200)
        Dim objGraphics As Graphics

        objGraphics = Graphics.FromImage(objBitmap)
        Dim strSize As SizeF = objGraphics.MeasureString(sTmp, New Font(sFnt, iSz))
        objBitmap.Dispose()
        objGraphics.Dispose()

        Return strSize.Width
    End Function

    Private Function FitString(ByVal strZ$, ByVal sFnt$, ByVal iSz As Long, ByVal sSiz As Single) As String
        Dim strTmp As String
        Dim lngJ As Long = strZ.Length
        Dim sWid As Single

        Do
            strTmp = Left(strZ, lngJ)
            If lngJ < strZ.Length Then strTmp &= ".."
            sWid = GetStringWidth(strTmp, sFnt, iSz)
            lngJ -= 1
        Loop While sWid > sSiz And lngJ > 0
        Return strTmp
    End Function

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim CookieName As String = FormsAuthentication.FormsCookieName
        Dim authCookie As HttpCookie = Request.Cookies(CookieName)
        Dim encryptedTicket As String = authCookie.Value
        Dim authTick = FormsAuthentication.Decrypt(encryptedTicket)
        Dim Username = authTick.Name

        'var xxx = new String('ajx=' + ajx + '&txn=' + $('<%= txnGUID.ClientID %>').value);
        'xxx = xxx + '&dept=' + $('<%= txtADept.ClientID %>').value;
        'xxx = xxx + '&serv=' + $('<%= txtAServ.ClientID %>').value;
        'xxx = xxx + '&glac=' + $('<%= txtAGlac.ClientID %>').value;
        'xxx = xxx + '&payr=' + $('<%= txtAPayr.ClientID %>').value;
        'xxx = xxx + '&pern=' + $('<%= txtAPern.ClientID %>').value;
        'xxx = xxx + '&amnt=' + $('<%= txtAAmnt.ClientID %>').value;
        Dim strAJX$, strTxn$, strDept$, strServ$, strSite$, strGLAc$, strPayr$, strPern$, strAmnt$
        Dim strDeptTitle$, strServTitle$, strSiteTitle$, strGLAcTitle$, strPayrTitle$, strPernTitle$
        Dim oHash = New zHashes
        Dim strGUID = oHash.MkGUID                                    ' [transactions].[guid]
        Dim intOrd As Integer = CInt(Math.Ceiling(Rnd() * 1000)).ToString()
        Dim oRD As New dsRequestDetailsTableAdapters.requestdetailsTableAdapter

        ' step 1 -- verify that the transaction exists, 
        ' if not, create the damn thing, so we can track it down later on.

        ' step 2 -- figure out the ordinal for this row
        Try
            strAJX = Request("ajx")
            strTxn = Request("txn") ' this is a GUID, not an integer!
            Dim vntOrd = oRD.GetCurrentOrdinalForInvoice(strTxn)
            If IsDBNull(vntOrd) Or vntOrd Is Nothing Then
                intOrd = 1
            Else
                intOrd = Convert.ToInt32(vntOrd)
                If intOrd <= 0 Then
                    intOrd = 1
                Else
                    intOrd += 1
                End If
            End If
        Catch ex As Exception
            Response.Write("{ ""error"":1," & _
                """message"":""Ord fetch:\n\n" & ex.Message & """ }")
            Exit Sub
        End Try
        Dim blnErr As Boolean = False
        Dim strMesg$ = ""
        Try
            strSite = Request("site") & ""
            If strSite <> "" Then
                strSiteTitle = FetchSageTitle(strSite, "site")
            Else
                strSiteTitle = ""
            End If
            strDept = Request("dept")
            strDeptTitle = FetchSageTitle(strDept, "dept")
            strServ = Request("serv")
            strServTitle = FetchSageTitle(strServ, "serv")
            strGLAc = Request("glac")
            strGLAcTitle = FetchSageTitle(strGLAc, "glac")
            strPayr = Request("payr")
            strPayrTitle = FetchSageTitle(strPayr, "payr")
            strPern = Request("pern")
            strPernTitle = FetchSageTitle(strPern, "pern")
            strAmnt = Request("amnt").ToString().Replace("$", "").Replace(",", "")
            If strAmnt.IndexOf("(") >= 0 then
                strAmnt = strAmnt.Replace("(", "").Replace(")", "")
                strAmnt = "-" & strAmnt
            End If
        Catch ex As Exception
            Response.Write("{ ""error"":2," & _
                """message"":""Param fetch:\n\n" & ex.Message & """ }")
            Exit Sub
        End Try
        Dim dAmnt As Decimal = Convert.ToDecimal(strAmnt)
        If strDeptTitle.Trim = "" Then
            strMesg &= "<li>Department is invalid</li>"
            blnErr = True
        End If
        If strServTitle.Trim = "" Then
            strMesg &= "<li>Service is invalid</li>"
            blnErr = True
        End If
        If strGLAcTitle.Trim = "" Then
            strMesg &= "<li>GL Account is invalid</li>"
            blnErr = True
        End If
        If strPayrTitle.Trim = "" Then
            strMesg &= "<li>Payer is invalid</li>"
            blnErr = True
        End If
        If strPernTitle.Trim = "" Then
            strMesg &= "<li>Person is invalid</li>"
            blnErr = True
        End If
        If Me.IsMultisite Then
            If strSite = "" Then
                strMesg &= "<li>Site code is invalid</li>"
                blnErr = True
            End If
        End If
        If dAmnt = 0 Then
            strMesg &= "<li>Amount is invalid</li>"
            blnErr = True
        End If
        If blnErr Then
            Response.Write("{ ""error"":5," & _
                """message"":""<ul>" & strMesg & "</ul>"" }")
            Exit Sub
        End If

        Dim dTotl As Decimal = 0
        Try
            If Me.IsMultisite Then
                oRD.usp_AddDetailMS(strGUID, strTxn, intOrd, dAmnt, strDept, strServ, strGLAc, strPayr, strPern, _
                        strDeptTitle, strServTitle, strGLAcTitle, strPayrTitle, strPernTitle, Username, strSite)
            Else
                oRD.usp_AddDetail(strGUID, strTxn, intOrd, dAmnt, strDept, strServ, strGLAc, strPayr, strPern, _
                        strDeptTitle, strServTitle, strGLAcTitle, strPayrTitle, strPernTitle, Username)
            End If
            dTotl = oRD.GetCurrentTotalForInvoice(strTxn)
        Catch ex As Exception
            Response.Write("{ ""error"":4," & _
                """message"":""DB Sync:\n\n" & ex.Message & """ }")
            Exit Sub
        End Try

        Try
            If Me.IsMultisite Then
                LogMessage("Detail added (" & intOrd & ") for GUID:" & strTxn & " with: " & vbCrLf & _
                                    "Amt: " & dAmnt & vbCrLf & _
                                    "Site: " & strSite & vbCrLf & _
                                    "Dept: " & strDept & vbCrLf & _
                                    "Serv: " & strServ & vbCrLf & _
                                    "GL Acct: " & strGLAc & vbCrLf & _
                                    "Payor: " & strPayr & vbCrLf & _
                                    "Person: " & strPern, strTxn, strGUID, Me.GUID, Request)
            Else
                LogMessage("Detail added (" & intOrd & ") for GUID:" & strTxn & " with: " & vbCrLf & _
                                    "Amt: " & dAmnt & vbCrLf & _
                                    "Dept: " & strDept & vbCrLf & _
                                    "Serv: " & strServ & vbCrLf & _
                                    "GL Acct: " & strGLAc & vbCrLf & _
                                    "Payor: " & strPayr & vbCrLf & _
                                    "Person: " & strPern, strTxn, strGUID, Me.GUID, Request)
            End If
        Catch ex As Exception
            ' do nothing
        End Try

        Response.Write("{ ""error"":0," & _
                """message"":""ok""," & _
                """guid"":""" & strGUID & """," & _
                """cid"":""" & intOrd & """," & _
                """Site"":""" & FitString(strSiteTitle, "Calibri", 10, 100) & """," & _
                """Dept"":""" & FitString(strDeptTitle, "Calibri", 10, 100) & """," & _
                """Serv"":""" & FitString(strServTitle, "Calibri", 10, 100) & """," & _
                """GLAc"":""" & FitString(strGLAcTitle, "Calibri", 10, 100) & """," & _
                """Payr"":""" & FitString(strPayrTitle, "Calibri", 10, 100) & """," & _
                """Pern"":""" & FitString(strPernTitle, "Calibri", 10, 100) & """," & _
                """Amnt"":""" & FormatNumber(dAmnt, 2, TriState.True, TriState.True, TriState.True) & """," & _
                """Total"":""" & FormatNumber(dTotl, 2, TriState.True, TriState.True, TriState.True) & """," & _
                """damt"":" & dAmnt & "," & _
                """dtot"":" & dTotl & "}")
    End Sub


    Private Function FetchSageTitle(ByVal strCode As String, ByVal strTable As String) As String
        Dim strType As String = strTable.ToLower()
        Dim strPref As String = strCode
        Dim strRet As String
        Select Case strType
            Case "entity"
                strRet = FetchTableData(strPref, "tblAcctCode_1")
            Case "region"
                strRet = FetchTableData(strPref, "tblAcctCode_2")
            Case "site"
                strRet = FetchTableData(strPref, "tblAcctCode_10")
            Case "vendor"
                strRet = FetchTableData(strPref, "tblAPVendor", "sName", "sVendorID")
            Case "dept"
                strRet = FetchTableData(strPref, "tblAcctCode_5")
            Case "serv"
                strRet = FetchTableData(strPref, "tblAcctCode_4")
            Case "glac"
                strRet = FetchTableData(strPref, "tblAcctCode_0")
            Case "payr"
                strRet = FetchTableData(strPref, "tblAcctCode_3")
            Case "pern"
                strRet = FetchTableData(strPref, "tblAcctCode_9")
            Case Else
                strRet = ""
        End Select
        Return (strRet)
    End Function


    Private Function FetchTableData(ByVal prefixText As String, _
            ByVal tblName As String, _
            Optional ByVal fld1 As String = "sTitle", _
            Optional ByVal fld2 As String = "sCodeID") As String
        Dim cn As New SqlClient.SqlConnection
        Dim ds As New DataSet
        Dim dt As New DataTable

        Dim strCN As String = ConfigurationManager.ConnectionStrings("SAGE_DBConnectionString").ConnectionString
        cn.ConnectionString = strCN
        Dim cmd As New SqlClient.SqlCommand
        cmd.Connection = cn
        cmd.CommandType = CommandType.Text
        Dim strSQL As String = "SELECT [" & fld1 & "] FROM [" & tblName & "] " & _
                "WHERE [" & fld2 & "] LIKE @code"
        cmd.CommandText = strSQL
        cmd.Parameters.AddWithValue("@code", prefixText)
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
        Dim txtItem As String = ""
        For Each row As DataRow In dt.Rows
            txtItem = row(fld1).ToString().Trim()
            If txtItem <> "" Then Exit For
        Next
        Return txtItem
    End Function

End Class