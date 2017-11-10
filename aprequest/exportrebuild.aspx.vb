Imports System.Drawing
Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Net
Imports System.Net.Mime

Public Partial Class exportrebuild
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Private Sub btnGo_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnGo.Click
        Dim strCN As String = ConfigurationManager.ConnectionStrings("aprequestConnectionString").ConnectionString
        Dim cn As New SqlClient.SqlConnection(strCN)
        Dim strReq$ = Me.txtReqNo.Text.Trim()
        If strReq.Length<2 then 
            Response.Write("<div class=""errormessage"">Invalid Req#</div>")
            Exit Sub
        End If
        If Not IsNumeric(strReq) then
            Response.Write("<div class=""errormessage"">Expecting a number</div>")
            Exit Sub
        End If
        Dim strSQL As String = "SELECT [txn_guid], [effdate], [ts] FROM [exportlog] WHERE [reqno]=" & strReq
        Dim cmd As New SqlClient.SqlCommand(strSQL, cn)
        Dim ds As New DataSet

        Try
            cn.Open()
            cmd.ExecuteNonQuery()
            Dim da As New SqlDataAdapter(cmd)
            da.Fill(ds)
        Catch ex As Exception
        Finally
            cn.Close()
        End Try

        Dim dt As New DataTable
        dt = ds.Tables(0)

        Dim strDLFdr$ = My.Settings.DownloadPath
        Dim oHash As New zHashes
        Dim strFileName$, strFileBase$
        Dim strTxGUID, strNiceName As String
        Dim strFNGUID$ = oHash.MkGUID
        Dim lngIX As long = 0
        strFileBase = Path.Combine(strDLFdr, strFNGUID)

        Dim oTxnTA As New dsRequestDetailsTableAdapters.txnsTA
        Dim oDetTA As New dsRequestDetailsTableAdapters.requestdetailsTableAdapter
        Dim strEffDate$
        strEffDate= Date.Today.ToString("MM/dd/yyyy")   'dtEffDate.ToString("MM/dd/yyyy") 
        Dim oFWriter As StreamWriter = Nothing

        Dim intI As Integer = 0
        Dim oUtl As New ZUtils
        Dim intRow As Integer = 1

        strNiceName = DateTime.Now().ToString("yyyyMMddHHmm-RB")
        For Each row As DataRow In dt.Rows
            strTxGuid = row("txn_guid").ToString()
            strEffDate = Convert.ToDateTime(row("effdate")).ToString("MM/dd/yyyy")
            If lngIX=0 then
                If row("ts").ToString().Length > 0 then
                    strNiceName = Convert.ToDateTime(row("ts")).ToString("yyyyMMddHHmm-RB")
                End If
                strFileBase = Path.Combine(strDLFdr, strNiceName)
                'Response.Write ("Create folder: " & strFileBase & "<br />")
                If Not System.IO.Directory.Exists(strFileBase) Then System.IO.Directory.CreateDirectory(strFileBase)
                strFileName = Path.Combine(strFileBase, strNiceName & ".csv")
                oFWriter = New StreamWriter(strFileName)
            End If
            'Response.Write("Txn: " & strTxGUID & "<br />")
            lngIX = lngIX + 1

            Dim strPrefix$ = My.Settings.ReqPrefix.Trim()
            Dim lngReq As Long = Convert.ToInt64(strReq)

            Dim oTxnTbl As dsRequestDetails.txnsDataTable = oTxnTA.GetByGUID(strTxGUID)
            Dim oDetTbl As dsRequestDetails.requestdetailsDataTable = oDetTA.GetDataByInvoiceID(strTxGUID)
            Dim oTxn As dsRequestDetails.txnsRow = oTxnTbl.Rows(0)
            Dim strTxnLine$ = ""
            Dim strTxnNo$ = ""

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

                    'LogMessage("Exported to Req #" & lngReq, oTxn.guid, oDetail.GUID, "", Request)
                    ' actual write out to string
                    oFWriter.WriteLine(strTxnLine)
                    strTxnLine = ""
                    intRow += 1
                Next
                oFWriter.Flush()

                ' ya might want to mark this as 'exported' now...?
                ' If Not blnSim Then oTxnTA.FlagExportByGUID(strTV)

                ' download a PDF record of this
                Me.DldPDF(strFileBase, strTxnNo, oTxn.guid)

            End If 'If oTxnTbl.Rows.Count > 0 
                'Response.Write(strTgt & "=[" & strTV & "]<br />" & vbCrLf)
                'Response.Write("STEP 4: Line out: " & intRow & "<br />")
            Response.Flush()
        Next
        oFWriter.Close()
        ' Dim xl As ZXLSWriter
        ' LogMessage("Export done: " & strNiceName, strFNGUID, "", Me.GUID, Request)
        ' now zip it up
        Dim strZipFile = strFileBase & ".zip"
        Using zip As New Ionic.Zip.ZipFile
            zip.AddDirectory(strFileBase)
            zip.Save(strZipFile)
        End Using

        'Directory.Delete(strFileBase, True)

         ' perform download
         If True Then 'False Then
            Response.ContentType = "application/octet-stream"
            Dim cd As New ContentDisposition
            cd.Inline = False
            cd.FileName = strNiceName & ".zip" '"export.xls"
            Response.AppendHeader("Content-Disposition", cd.ToString())
            Dim filedata() As Byte = File.ReadAllBytes(strZipFile)
            Response.OutputStream.Write(filedata, 0, filedata.Length)
            'Response.Cookies("MSG").Value = "export done"
            Response.Flush()
            Response.End()
            Response.Close()
        End If

        ' Response.Write("<hr /><br />DL Folder: " & strDLFdr & "<br />" & _
        '         "FileBase: " & strFileBase & "<br />" & _
        '         "FNGUID: " & strFNGUID & "<br />" & _
        '         "Nice Name: " & strNiceName & "<br />" )
    End Sub


#Region "ConvenienceFunctions"
    
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
            Response.Write(">>  PDF export result: " & strResult & "<br />" & _
                           ">>  " & strUrl & "<br />")
            'LogMessage("Txn-" & strTxn & " export to PDF with result: " & strResult & " (to " & strFld & ")", strTxnGUID, "", "rebuild", Request)
        Else
            Response.Write(">>  DldPDF - ignoring input: [" & strTxn & "]<br />")
            'LogMessage("DldPDF - ignoring input: [" & strTxn & "]", strTxnGUID, "", "rebuild", Request)
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
#End Region
End Class