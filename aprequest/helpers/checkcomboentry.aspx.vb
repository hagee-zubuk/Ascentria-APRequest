Imports System.Data
Imports System.Data.SqlClient

Partial Public Class checkcomboentry
    Inherits ZPageSecure

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim strAJX$, strDept$, strPayr$, strEnt$, strMS$, strSit$
        'Dim strServ$, strReg$
        Dim oUtl = New ZUtils

        Try
            strAJX = Request("ajx")
            strMS = Request("mul") & ""
            strEnt = Request("ent") & ""
            ' strReg = Request("reg") & ""
            strSit = Request("sit") & ""
            strDept = Request("dept") & ""
            ' strServ = Request("serv") & ""
            strPayr = Request("payr") & ""
        Catch ex As Exception
            Response.Write("{ ""error"":1," & _
                    """message"":""input error " & ex.Message & """}")
            Exit Sub
        End Try

        Dim blnRes As Boolean = True
        blnRes = oUtl.CheckComboEntry(strEnt, strSit, strPayr, strDept)
        If blnRes Then
            Response.Write("{ ""error"":0," & _
                    """message"":""ok""")
        Else
            Response.Write("{ ""error"":4," & _
                    """message"":""The account code combination is not valid.""")
        End If
        Response.Write("}")
    End Sub

End Class