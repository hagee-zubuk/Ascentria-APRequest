Public Partial Class sethold
    Inherits ZPageSecure

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim strGUID As String
        Dim strUsrID As String
        Dim strReasn As String = ""
        Dim strTxnN As String = ""
        Dim strMesg As String = ""
        Dim strUser As String = ""
        Dim blnHold As Boolean = False
        Try
            strGUID = Request("guid")
            strUsrID = Request("uid")
            strReasn = Request("reason")
            blnHold = CBool(CStr(Request("hold") & "").Trim())
        Catch ex As Exception
            Response.Write("{ ""error"":1," & _
                    """message"":""GUID fetch:\n\n" & ex.Message & """ }")
            Exit Sub
        End Try

        Try
            Using oTX As New dsRequestDetailsTableAdapters.txnsTA
                LogMessage("Set HOLD to " & blnHold, strGUID, "", Me.GUID, Request)
                oTX.SetHoldState(blnHold, strGUID)
                If blnHold Then
                    Dim strApprv$ = oTX.GetHash(strGUID)
                    If strApprv.ToUpper = "APPROVED" Then
                        oTX.SetHash("", strGUID)
                        oTX.SetApproval(False, "", strGUID)
                    End If
                End If
                Dim txnsDT As dsRequestDetails.txnsDataTable = oTX.GetByGUID(strGUID)
                If txnsDT.Rows.Count > 0 Then
                    strTxnN = "Txn: " & txnsDT(0).txn_no.ToString()
                End If
            End Using
            Using oUX As New dsUsersTableAdapters.userconfigTA
                Dim oUDT As dsUsers.userconfigDataTable = oUX.GetByGUID(strUsrID)
                If oUDT.Rows.Count > 0 Then
                    strUser = oUDT(0).fullname
                End If
            End Using
            If blnHold Then
                strMesg = "<p>" & strTxnN & " put <b>on HOLD</b> by " & strUser & "</p>"
                SendReqEMailMessage(strMesg, strGUID, strTxnN & " on HOLD")
                If strReasn <> "" Then
                    Using oHr As New dsRequestDetailsTableAdapters.holdreasonsTA
                        oHr.Insert(strGUID, strUser, Date.Now, "ON HOLD:" & vbCrLf & strReasn)
                    End Using
                End If
            Else
                strMesg = "<p>" & strTxnN & " -- HOLD <i>removed</i> by " & strUser & "</p>"
                SendReqEMailMessage(strMesg, strGUID, strTxnN & " removed from  HOLD")
                Using oHr As New dsRequestDetailsTableAdapters.holdreasonsTA
                    oHr.Insert(strGUID, strUser, Date.Now, "HOLD removed")
                End Using
            End If

            Response.Write("{ ""error"":0," & _
                    """message"":""OK"" }")
            Exit Sub
        Catch ex As Exception
            LogMessage("Failed to set HOLD state (should be: " & blnHold & ")" & vbCrLf & ex.Message, _
                    strGUID, "", Me.GUID, Request)
            Response.Write("{ ""error"":2," & _
                    """message"":""ERROR:" & ex.Message & """ }")
        End Try
    End Sub

End Class