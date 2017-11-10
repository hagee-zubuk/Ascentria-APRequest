Imports System.Drawing
Imports System.Data
Imports System.Data.SqlClient

Partial Public Class detailrow_display2
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim strGUID$
        Dim oDetlTA As New dsRequestDetailsTableAdapters.requestdetailsTableAdapter
        Dim oUtil As New ZUtils
        Try
            strGUID = Request("guid")
            If strGUID = "" Then
                Response.Write("{ ""error"":1," & _
                        """message"":""GUID not specified"" }")
                Exit Sub
            End If
            Dim oTxns As New dsRequestDetailsTableAdapters.txnsTA
            Dim tblTxns As dsRequestDetails.txnsDataTable = oTxns.GetByGUID(strGUID)
            If tblTxns.Rows.Count < 1 Then
                Response.Write("{ ""error"":2," & _
                        """message"":""a record with that ID was not found"" }")
                Exit Sub
            End If
            ' Response.Write("{ ""error"":3, ""message"":""test"" }")
            ' Exit Sub
            Dim oTxnRec As dsRequestDetails.txnsRow = tblTxns.Rows(0)
            Dim strRes$ = "{ ""error"":0," & _
                    """guid"":""" & oTxnRec.guid & ""","
            With oTxnRec
                If .Isentity_idNull Then
                    strRes &= """enticode"":""""," & _
                            """entititle"":"""","
                Else
                    strRes &= """enticode"":""" & .entity_id & """," & _
                            """entititle"":""" & oUtil.FetchSageTitle(.entity_id, "entity") & ""","
                End If
                If .Isregion_idNull Then
                    strRes &= """regncode"":""""," & _
                            """regntitle"":"""","
                Else
                    strRes &= """regncode"":""" & .region_id & """," & _
                            """regntitle"":""" & oUtil.FetchSageTitle(.region_id, "region") & ""","
                End If
                If .Issite_idNull Then
                    strRes &= """sitecode"":""""," & _
                            """sitetitle"":"""","
                Else
                    strRes &= """sitecode"":""" & .site_id & """," & _
                            """sitetitle"":""" & oUtil.FetchSageTitle(.site_id, "site") & ""","
                End If
                If .Isvendor_idNull Then
                    strRes &= """vendcode"":""""," & _
                            """vendtitle"":"""","
                Else
                    strRes &= """vendcode"":""" & .vendor_id & """," & _
                            """vendtitle"":""" & oUtil.FetchSageTitle(.vendor_id, "vendor") & ""","
                End If
                If .Isfy_idNull Then
                    strRes &= """fyr"":"""","
                Else
                    strRes &= """fyr"":""" & .fy_id & ""","
                End If
                If .Istotal_amountNull Then
                    strRes &= """total"":""0.00"","
                Else
                    strRes &= """total"":""" & FormatNumber(.total_amount, 2, TriState.True, TriState.False, TriState.True) & ""","
                End If
                If .Isinvoice_noNull Then
                    strRes &= """inv_no"":""-"","
                Else
                    strRes &= """inv_no"":""" & .invoice_no & ""","
                End If
                If .Isinvoice_dtNull Then
                    strRes &= """inv_dt"":"""","
                Else
                    strRes &= """inv_dt"":""" & .invoice_dt.ToString("MM/dd/yyyy") & ""","
                End If
                If .Isdue_dtNull Then
                    strRes &= """due_dt"":"""","
                Else
                    strRes &= """due_dt"":""" & .due_dt.ToString("MM/dd/yyyy") & ""","
                End If
            End With
            Dim tblDets As dsRequestDetails.requestdetailsDataTable = oDetlTA.GetDataByInvoiceID(oTxnRec.guid)
            Dim strTable$
            Dim lngRwCnt& = 0
            If tblDets.Rows.Count > 0 Then
                strTable = "<table class='apprmini' border='1' style='width: 100%;'><tr><th>Dept</th><th>Service</th><th>GL Acct</th><th>Payer</th><th>Person</th><th>Amount</th></tr>"
                For Each oDetls As dsRequestDetails.requestdetailsRow In tblDets.Rows
                    With oDetls
                        strTable &= "<tr><td>" & .DeptTitle & " [" & .DeptCode & "]</td>"
                        strTable &= "<td>" & .ServTitle & " [" & .ServCode & "]</td>"
                        strTable &= "<td>" & .GLAcctTitle & " [" & .GLAcctCode & "]</td>"
                        strTable &= "<td>" & .PayerTitle & " [" & .PayerCode & "]</td>"
                        strTable &= "<td>" & .PersonTitle & " [" & .Person & "]</td>"
                        strTable &= "<td align='right'>" & FormatNumber(.Amount, 2, TriState.True, TriState.False, TriState.True) & "</td></tr>"
                        'strTable &= vbCrLf
                        lngRwCnt += 1
                    End With
                Next
                strTable &= "</table>"
                strRes &= """detblock"":""" & strTable & ""","
            Else
                strRes &= """detblock"":"""","
            End If
            strRes &= """message"":""ok"" }"
            Response.Write(strRes)
        Catch ex As Exception
            Response.Write("{ ""error"":14, ""message"":""an exception occurred"" }")
        End Try
    End Sub

End Class