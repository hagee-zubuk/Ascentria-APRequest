Imports System.IO
Imports System.Net.Mime

Partial Public Class Bigexport
    Inherits ZPageSecure

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

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
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

        'Response.Write("STEP 2: found " & intActives & "<br />")
        If intActives > 0 Then
            ' open the file for writing
            Dim oFWriter As New StreamWriter(strFileName)
            Dim strTV As String
            Dim oUtl As New ZUtils
            Dim intRow As Integer = 1

            Dim strPrefix$ = My.Settings.ReqPrefix.Trim()
            Dim lngReq As Long = 0
            Using oSetTA As New dsRequestDetailsTableAdapters.txnsTA
                Dim oTxnsTbl As dsRequestDetails.txnsDataTable = oSetTA.GetData
                Dim oTxn As dsRequestDetails.txnsRow
                For Each oTxn In oTxnsTbl.Rows
                    strTV = oTxn.guid
                    Try
                        Using oPSTA As New dsRequestDetailsTableAdapters.apsetupTA
                            lngReq = oPSTA.GetLatestRequest()
                        End Using
                    Catch ex As Exception
                        oFWriter.WriteLine("ERR: " & ex.Message)
                        'lngReq = 1000
                    End Try
                    Try
                        ' get everything by GUID 
                        oDetTA.RefreshTotals(strTV)
                        'LogMessage("GUID:" & strTV & " exported", strTV, "", Me.GUID, Request)
                    Catch ex As Exception
                        ' do nothing
                        oFWriter.WriteLine("ERR: trying to get [" & strTV & "] ERR: " & ex.Message)
                    End Try

                    Dim oDetTbl As dsRequestDetails.requestdetailsDataTable = oDetTA.GetDataByInvoiceID(strTV)
                    Dim strTxnLine$ = ""
                    Dim strTxnNo$ = ""
                    If oTxn.hash = "approved" Then
                        Dim oDetail As dsRequestDetails.requestdetailsRow
                        For Each oDetail In oDetTbl.Rows
                            strTxnNo = oTxn.txn_no.ToString().Trim()
                            strTxnLine &= """" & strPrefix & lngReq & ""","
                            strTxnLine &= """Transaction ID" & strTxnNo & ""","
                            strTxnLine &= """" & oTxn.create_at.ToString("MM/dd/yyyy") & ""","
                            If oTxn.Isdue_dtNull Then
                                strTxnLine &= """"","
                            Else
                                strTxnLine &= """" & oTxn.due_dt & ""","
                            End If
                            If oTxn.Isentity_idNull Then
                                strTxnLine &= """"","
                            Else
                                strTxnLine &= """" & oTxn.entity_id & ""","
                            End If
                            If oTxn.Isregion_idNull Then
                                strTxnLine &= """"","
                            Else
                                strTxnLine &= """" & oTxn.region_id & ""","
                            End If
                            If oTxn.Issite_idNull Then
                                strTxnLine &= """"","
                            Else
                                strTxnLine &= """" & oTxn.site_id & ""","
                            End If
                            If oDetail.IsDeptCodeNull Then
                                strTxnLine &= """"","
                            Else
                                strTxnLine &= """" & oDetail.DeptCode & ""","
                            End If
                            If oDetail.IsServCodeNull Then
                                strTxnLine &= """"","
                            Else
                                strTxnLine &= """" & oDetail.ServCode & ""","
                            End If
                            If oDetail.IsGLAcctCodeNull Then
                                strTxnLine &= """"","
                            Else
                                strTxnLine &= """" & oDetail.GLAcctCode & ""","
                            End If
                            If oTxn.Isfy_idNull Then
                                strTxnLine &= """"","
                            Else
                                strTxnLine &= """" & oTxn.fy_id & ""","
                            End If
                            If oDetail.IsPayerCodeNull Then
                                strTxnLine &= """"","
                            Else
                                strTxnLine &= """" & oDetail.PayerCode & ""","
                            End If
                            If oDetail.IsPersonNull Then
                                strTxnLine &= """"","
                            Else
                                strTxnLine &= """" & oDetail.Person & ""","
                            End If
                            If oTxn.Isvendor_idNull Then
                                strTxnLine &= """"","
                            Else
                                strTxnLine &= """" & oTxn.vendor_id & ""","
                            End If
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
                            LogMessage("Exported to Req #" & lngReq, oTxn.guid, oDetail.GUID, Me.GUID, Request)
                            ' actual write out to string
                            oFWriter.WriteLine(strTxnLine)
                            strTxnLine = ""
                            intRow += 1
                        Next
                        oFWriter.Flush()
                        oTxnTA.FlagExportByGUID(strTV)
                        Using oPSTA As New dsRequestDetailsTableAdapters.apsetupTA
                            oPSTA.IncrementReq()
                        End Using
                    End If
                Next
            End Using
            strNiceName = Now.ToString("yyyyMMddHHmm") & ".csv"
            oFWriter.Close()
            'Dim xl As ZXLSWriter
            LogMessage("BIG Export done: " & strNiceName, strFNGUID, "", "", Request)

            ' perform download
            If True Then 'False Then
                Response.ContentType = "application/octet-stream"
                Dim cd As New ContentDisposition
                cd.Inline = False
                cd.FileName = strNiceName '"export.xls"
                Response.AppendHeader("Content-Disposition", cd.ToString())
                Dim filedata() As Byte = File.ReadAllBytes(strFileName)
                Response.OutputStream.Write(filedata, 0, filedata.Length)
                'Response.Cookies("MSG").Value = "export done"
                Response.Flush()
                Response.End()
                Response.Close()
            End If
        End If
    End Sub

End Class