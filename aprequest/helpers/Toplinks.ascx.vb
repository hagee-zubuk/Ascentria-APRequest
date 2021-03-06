﻿Public Partial Class Toplinks
    Inherits System.Web.UI.UserControl
    Public GUID As String

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        'Me.GUID = ""
        Me.lnkAdmin.Visible = False
        Me.lnkAppr.Visible = False
        Me.lnkDones.Visible = False
        Me.lnkExport.Visible = False
        Me.lnkRequest.Visible = True
        ShowLinksMaybe()
    End Sub

    Protected Sub lnkAdmin_Click(ByVal sender As Object, ByVal e As EventArgs) Handles lnkAdmin.Click
        Response.Redirect("~/useradmin.aspx")
    End Sub

    Private Sub lnkAppr_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lnkAppr.Click
        Response.Redirect("~/approval.aspx")
    End Sub

    Private Sub lnkDones_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lnkDones.Click
        Response.Redirect("~/pt_picker.aspx")
    End Sub

    Private Sub lnkExport_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lnkExport.Click
        Response.Redirect("~/exporter2.aspx")
        'Response.Redirect("~/ex_picker.aspx")
    End Sub

    Private Sub lnkMine_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lnkMine.Click
        Response.Redirect("~/myreqs.aspx")
    End Sub

    Protected Sub lnkLogout_Click(ByVal sender As Object, ByVal e As EventArgs) Handles lnkLogout.Click
        Response.Cookies(FormsAuthentication.FormsCookieName).Value = ""
        Response.Cookies(FormsAuthentication.FormsCookieName).Expires = DateTime.Now.AddHours(-1)
        ' redirect?
        Response.Redirect("~/logon.aspx")
    End Sub

    Private Sub lnkRequest_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lnkRequest.Click
        Response.Redirect("~/default.aspx")
    End Sub

    Private Sub ShowLinksMaybe()
        If Me.GUID <> "" Then
            Using oUsrTA As New dsUsersTableAdapters.userconfigTA
                Dim oUser As dsUsers.userconfigDataTable = oUsrTA.GetByGUID(Me.GUID)
                If oUser.Rows.Count > 0 Then
                    Dim oUsrInfo As dsUsers.userconfigRow = oUser(0)
                    With oUsrInfo
                        Me.lnkAdmin.Visible = .isadmin
                        Me.lnkAppr.Visible = .isrequester
                        Me.lnkDones.Visible = True '.isfinance
                        Me.lnkExport.Visible = .ismanager
                    End With
                End If
                oUser.Dispose()
            End Using
        End If

        Me.lnkAdmin.Visible = Me.lnkAdmin.Visible And Not Request.ServerVariables("SCRIPT_NAME").ToString().Contains("useradmin")
        Me.lnkAppr.Visible = Me.lnkAppr.Visible And Not Request.ServerVariables("SCRIPT_NAME").ToString().Contains("approval")
        Me.lnkDones.Visible = Me.lnkDones.Visible And Not ( _
            Request.ServerVariables("SCRIPT_NAME").ToString().Contains("processed") _
            Or _
            Request.ServerVariables("SCRIPT_NAME").ToString().Contains("pt_picker") _
            )
        Me.lnkExport.Visible = Me.lnkExport.Visible And Not ( _
            Request.ServerVariables("SCRIPT_NAME").ToString().Contains("export") _
            Or _
            Request.ServerVariables("SCRIPT_NAME").ToString().Contains("ex_picker") _
            )
        Me.lnkRequest.Visible = Me.lnkRequest.Visible And Not Request.ServerVariables("SCRIPT_NAME").ToString().Contains("default")

    End Sub

End Class