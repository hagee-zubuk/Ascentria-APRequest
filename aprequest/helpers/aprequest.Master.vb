Public Partial Class aprequest
    Inherits ZMasterPage

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ulink.Text = Me.DisplayName
        ulink.PostBackUrl = "~/profile.aspx?guid=" & Me.GUID
        Me.Toplinks1.GUID = Me.GUID
        Me.datedisp.Text = Format(Date.Now, "ddd MMM d, yyyy h:mmtt")
        'If Not Request.ServerVariables("SCRIPT_NAME").ToString().Contains("default") Then Me.lnkRequest.Visible = True
        'Using oUsrTA As New dsUsersTableAdapters.userconfigTA
        '    Dim oUser As dsUsers.userconfigDataTable = oUsrTA.GetByGUID(Me.GUID)
        '    If oUser.Rows.Count > 0 Then
        '        Dim oUsrInfo As dsUsers.userconfigRow = oUser(0)
        '        With oUsrInfo
        '            Me.lnkAdmin.Visible = .isadmin
        '            Me.lnkAppr.Visible = .isrequester
        '        End With
        '    End If
        '    oUser.Dispose()
        'End Using
        'uname.Text = Me.DisplayName
    End Sub

    'Protected Sub lnkLogout_Click(ByVal sender As Object, ByVal e As EventArgs) Handles lnkLogout.Click
    '    Response.Cookies(FormsAuthentication.FormsCookieName).Value = ""
    '    Response.Cookies(FormsAuthentication.FormsCookieName).Expires = DateTime.Now.AddHours(-1)
    '    ' redirect?
    '    Response.Redirect("~/logon.aspx")
    'End Sub

    'Protected Sub lnkAdmin_Click(ByVal sender As Object, ByVal e As EventArgs) Handles lnkAdmin.Click
    '    Response.Redirect("~/useradmin.aspx")
    'End Sub

    'Private Sub lnkAppr_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lnkAppr.Click
    '    Response.Redirect("~/approval.aspx")
    'End Sub

    'Private Sub lnkRequest_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lnkRequest.Click
    '    Response.Redirect("~/default.aspx")
    'End Sub

    'Private Sub lnkExport_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lnkExport.Click
    '    Response.Redirect("~/exporter.aspx")
    'End Sub
End Class