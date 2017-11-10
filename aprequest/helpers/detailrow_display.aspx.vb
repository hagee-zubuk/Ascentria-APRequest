Imports System.Drawing
Imports System.Data
Imports System.Data.SqlClient

Partial Public Class detailrow_display
    Inherits System.Web.UI.Page

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
        Dim strGUID$
        Dim oDetlTA As New dsRequestDetailsTableAdapters.requestdetailsTableAdapter
        Dim tblDetl As dsRequestDetails.requestdetailsDataTable
        Try
            strGUID = Request("guid")
            If strGUID = "" Then
                Response.Write("{ ""error"":1," & _
                        """message"":""GUID not specified"" }")
                Exit Sub
            End If
            tblDetl = oDetlTA.GetByGUID(strGUID)
            If tblDetl.Rows.Count < 1 Then
                Response.Write("{ ""error"":2," & _
                        """message"":""a record with that ID was not found"" }")
                Exit Sub
            End If
            Dim oDetail As dsRequestDetails.requestdetailsRow = tblDetl.Rows(0)
            Dim strRes$ = "{ ""error"":0," & _
                    """guid"":""" & oDetail.GUID & ""","
            With oDetail
                strRes &= """deptcode"":""" & .DeptCode & ""","
                strRes &= """depttitle"":""" & .DeptTitle & ""","
                strRes &= """glacctcode"":""" & .GLAcctCode & ""","
                strRes &= """glaccttitle"":""" & .GLAcctTitle & ""","
                strRes &= """payercode"":""" & .PayerCode & ""","
                strRes &= """payertitle"":""" & .PayerTitle & ""","
                strRes &= """person"":""" & .Person & ""","
                strRes &= """persontitle"":""" & .PersonTitle & ""","
                strRes &= """servcode"":""" & .ServCode & ""","
                strRes &= """servtitle"":""" & .ServTitle & ""","
                strRes &= """amount"":""" & .Amount & ""","
            End With
            strRes &= """message"":""ok"" }"
            Response.Write(strRes)
        Catch ex As Exception

        End Try
    End Sub

End Class