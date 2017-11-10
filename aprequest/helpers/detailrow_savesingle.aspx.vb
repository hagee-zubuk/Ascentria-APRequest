Imports System.Drawing
Imports System.Data
Imports System.Data.SqlClient


Partial Public Class detailrow_savesingle
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
        Dim CookieName As String = FormsAuthentication.FormsCookieName
        Dim authCookie As HttpCookie = Request.Cookies(CookieName)
        Dim encryptedTicket As String = authCookie.Value
        Dim authTick = FormsAuthentication.Decrypt(encryptedTicket)
        Dim Username = authTick.Name
        Dim strAJX$, strGUID$, strType$, strIDElem$, strTitle$, strZTitle$
        Dim strResponse$ = "{ ""error"":2," & _
                """message"":""General error"" }"
        Dim Utl As New ZUtils
        Try
            strAJX = Request("ajx")
            strGUID = Request("guid")
            strType = Request("type")
            strTitle = Request("title")
            strIDElem = Request("id")

            strZTitle = Utl.FetchSageTitle(strIDElem, strType)
            If strGUID = "" Then
                Throw New System.Exception("Unknown GUID")
            End If
        Catch ex As Exception
            Response.Write("{ ""error"":4," & _
                """message"":""" & ex.Message & """ }")
            Exit Sub
        End Try
        If strZTitle <> "" Then
            Dim oRQTA As New dsRequestDetailsTableAdapters.requestdetailsTableAdapter
            Try
                Select Case (strType)
                    Case "dept"
                        oRQTA.UpdateDeptByGUID(strIDElem, strZTitle, strGUID)
                    Case "serv"
                        oRQTA.UpdateServByGUID(strIDElem, strZTitle, strGUID)
                    Case "glac"
                        oRQTA.UpdateGlacByGUID(strIDElem, strZTitle, strGUID)
                    Case "payr"
                        oRQTA.UpdatePayrByGUID(strIDElem, strZTitle, strGUID)
                    Case "pern"
                        oRQTA.UpdatePernByGUID(strIDElem, strZTitle, strGUID)
                    Case Else
                        Throw New System.Exception("Unknown DB Request Type")
                End Select
                strResponse = "{ ""error"":0," & _
                    """message"":""ok""," & _
                    """title"":""" & FitString(strZTitle, "Calibri", 10, 90) & """ }"
            Catch ex As Exception
                strResponse = "{ ""error"":3," & _
                        """message"":""" & ex.Message & """ }"
            End Try
        Else
            Response.Write("{ ""error"":1," & _
                    """message"":""Input error: invalid request type"" }")
        End If
        Response.Write(strResponse)
    End Sub

End Class