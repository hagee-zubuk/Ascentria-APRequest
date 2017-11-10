Imports System.IO

Partial Public Class attachfile
    'Inherits System.Web.UI.Page
    Inherits ZPageSecure

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim strTxnID As String = Request("txnid")
        If strTxnID Is Nothing Or strTxnID = "" Then
            btnOK.Visible = True
            Exit Sub
        End If

        txnid.Value = strTxnID
        btnGO.Visible = True
        file1.Visible = True

        btnOK.Visible = False
    End Sub

    Private Sub btnGO_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnGO.Click
        Dim upldFolder As String = My.Settings.UploadPath
        Dim strTxnID As String = Request("txnid").ToLower()
        Using oRT As New dsRequestDetailsTableAdapters.txnsTA
            Dim iCnt& = oRT.TxnExists(strTxnID)
            If iCnt <= 0 Then
                oRT.TxnInsert(strTxnID, 0, "", "", "", "", "", _
                                "", 0, "1/1/1980", "1/1/1980", "", Me.UserName, "", "","")
            End If
        End Using

        If file1.HasFile Then
            Dim lngI& = 0
            Dim f_name As String = file1.FileName
            Dim f_extn As String = Path.GetExtension(f_name)

            Dim imgFile As HttpPostedFile = file1.PostedFile
            Dim imgByte As Byte() = New Byte(imgFile.ContentLength - 1) {}
            imgFile.InputStream.Read(imgByte, 0, imgFile.ContentLength)

            Dim strConn$ = ConfigurationManager.ConnectionStrings("aprequestConnectionString").ConnectionString
            Dim oConn As New SqlClient.SqlConnection(strConn)

            oConn.Open()
            Dim strSQL = "UPDATE [transactions] SET [upload]=@uld, [ofn]=@fnm WHERE [guid]=@guid"
            Dim oCmd As New SqlClient.SqlCommand(strSQL, oConn)
            oCmd.Parameters.AddWithValue("@guid", strTxnID) ' Me.GUID)
            oCmd.Parameters.AddWithValue("@uld", imgByte)
            oCmd.Parameters.AddWithValue("@fnm", f_name)
            Dim iRet As Integer = Convert.ToInt32(oCmd.ExecuteNonQuery())
            oCmd.Dispose()

        End If

        file1.Visible = False
        btnGO.Visible = False
        btnOK.Visible = True
    End Sub
End Class