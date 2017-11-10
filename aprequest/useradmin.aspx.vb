Partial Public Class useradmin
    Inherits ZPageSecure
    'WAS: Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Me.IsAdmin Then
            Response.Cookies("MSG").Value = "you do not have adequate rights to that page (USR_ADM)"
            Response.Redirect("~/default.aspx")
        End If
        Me.Toplinks1.GUID = Me.GUID
        Me.datedisp.Text = Format(Date.Now, "ddd MMM d, yyyy h:mmtt")
        ulink.Text = Me.DisplayName
        ulink.PostBackUrl = "~/profile.aspx?guid=" & Me.GUID

        Dim oUtil As New ZUtils
        Dim oUsrs As New dsUsersTableAdapters.userconfigTA
        Dim oDflV As dsUsers.userconfigDataTable = oUsrs.GetDefault
        Dim oDefault As dsUsers.userconfigRow = oDflV.Item(0)

        If Me.IsPostBack Then
            Exit Sub
        End If
        'LogMessage("Load UserAdmin", "", "", "", Request)
        Dim dicAV As Dictionary(Of String, String) = oUtil.FetchUsers()
        lbAppr.DataSource = dicAV
        lbAppr.DataTextField = "Value"
        lbAppr.DataValueField = "Key"
        lbAppr.DataBind()
        ' make sure the 'right' stuff is selected
        'Try
        '    Dim lstMR As List(of String) = oUtil.FetchMgrs(
        'Catch ex As Exception
        'End Try

        Dim dicFY As Dictionary(Of String, String) = oUtil.FetchSageData("fy", True)
        ddFY.DataSource = dicFY
        ddFY.DataTextField = "Value"
        ddFY.DataValueField = "Key"
        ddFY.DataBind()
        Try
            ddFY.SelectedValue = oDefault.fiscalyr
        Catch ex As Exception
            ddFY.SelectedValue = -1
        End Try

        Dim dicEnt As Dictionary(Of String, String) = oUtil.FetchSageData("entity", True)
        ddEnt.DataSource = dicEnt
        ddEnt.DataTextField = "Value"
        ddEnt.DataValueField = "Key"
        ddEnt.DataBind()
        If oDefault.Isentity_idNull Then
            ddEnt.SelectedValue = ""
        Else
            ddEnt.SelectedValue = oDefault.entity_id
        End If

        Dim dicReg As Dictionary(Of String, String) = oUtil.FetchSageData("region", True)
        ddReg.DataSource = dicReg
        ddReg.DataTextField = "Value"
        ddReg.DataValueField = "Key"
        ddReg.DataBind()
        If oDefault.Isregion_idNull Then
            ddReg.SelectedValue = ""
        Else
            ddReg.SelectedValue = oDefault.region_id
        End If

        Dim dicSite As Dictionary(Of String, String) = oUtil.FetchSageData("site", True)
        ddSite.DataSource = dicSite
        ddSite.DataTextField = "Value"
        ddSite.DataValueField = "Key"
        ddSite.DataBind()
        If oDefault.Issite_idNull Then
            ddSite.SelectedValue = ""
        Else
            ddSite.SelectedValue = oDefault.site_id
        End If

        ' populate the table (i hope)
        Dim oDataTab As dsUsers.userconfigDataTable = oUsrs.GetAll()
        Dim oUserRow As dsUsers.userconfigRow
        Dim strDelete$
        For Each oUserRow In oDataTab.Rows
            Dim outrow As New TableRow
            Dim tcell As TableCell
            Dim strTmp As String

            tcell = New TableCell
            strDelete = "<a href=""#"" onclick=""delUsr('" & oUserRow.guid & "');"">" & _
                    "<img src=""images\recycle_bin.png"" title=""delete " & oUserRow.username & _
                    """ alt=""X"" /></a>&nbsp;"
            tcell.Text = strDelete & oUserRow.fullname
            outrow.Cells.Add(tcell)

            tcell = New TableCell
            tcell.Text = "<a href=""" & oUserRow.guid & """ class=""modal_link"">" & oUserRow.username & "</a>"
            outrow.Cells.Add(tcell)

            tcell = New TableCell
            If oUserRow.Ismgr_idNull Then
                tcell.Text = ""
            Else
                tcell.Text = oUserRow.mgr_id
            End If
            outrow.Cells.Add(tcell)

            '<th>Entity</th>
            tcell = New TableCell
            tcell.HorizontalAlign = HorizontalAlign.Center
            If oUserRow.Isentity_idNull Then
                tcell.Text = ""
            Else
                tcell.Text = oUserRow.entity_id ' oUtil.FetchSageTitle(oUserRow.entity_id, "entity")
            End If
            outrow.Cells.Add(tcell)

            '<th>Region</th>
            tcell = New TableCell
            tcell.HorizontalAlign = HorizontalAlign.Center
            If oUserRow.Isregion_idNull Then
                tcell.Text = ""
            Else
                tcell.Text = oUserRow.region_id ' oUtil.FetchSageTitle(oUserRow.region_id, "region")
            End If
            outrow.Cells.Add(tcell)

            '<th>Site</th>
            tcell = New TableCell
            tcell.HorizontalAlign = HorizontalAlign.Center
            If oUserRow.Issite_idNull Then
                tcell.Text = ""
            Else
                tcell.Text = oUserRow.site_id   ' oUtil.FetchSageTitle(oUserRow.site_id, "site")
            End If
            outrow.Cells.Add(tcell)

            '<th>Limit</th>
            tcell = New TableCell
            tcell.HorizontalAlign = HorizontalAlign.Right
            If oUserRow.IslimitNull Then
                tcell.Text = ""
            Else
                tcell.Text = oUserRow.limit
            End If
            outrow.Cells.Add(tcell)

            ' TODO: ask paul what this is!
            tcell = New TableCell
            tcell.HorizontalAlign = HorizontalAlign.Center
            strTmp = ""
            If oUserRow.isadmin Then strTmp &= " Adm "
            'If oUserRow.isapprover Then strTmp &= " App "
            If oUserRow.ismanager Then strTmp &= " Exp " ' Processor / Exporter
            If oUserRow.isfinance Then strTmp &= " Fin " ' Finance / Auditing
            If oUserRow.isrequester Then strTmp &= " Req "
            If oUserRow.ismultisite Then strTmp &= " ms "
            tcell.CssClass = "topb smalltext"
            tcell.Text = strTmp
            outrow.Cells.Add(tcell)

            Me.tblUsrs.Rows.Add(outrow)
        Next
        oDataTab.Dispose()
        oUsrs.Dispose()
    End Sub

    'Private Sub lnkRequest_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lnkRequest.Click
    '    Response.Redirect("~/default.aspx")
    'End Sub

    'Private Sub lnkLogout_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lnkLogout.Click
    '    Response.Cookies(FormsAuthentication.FormsCookieName).Value = ""
    '    Response.Cookies(FormsAuthentication.FormsCookieName).Expires = DateTime.Now.AddHours(-1)
    '    ' redirect?
    '    Response.Redirect("~/logon.aspx")
    'End Sub

    'Private Sub lnkApprove_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lnkApprove.Click
    '    Response.Redirect("~/approval.aspx")
    'End Sub

    'Private Sub lnkExport_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lnkExport.Click
    '    Response.Redirect("~/exporter.aspx")
    'End Sub

    'Private Sub lngAB_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lngAB.Click
    '    Response.Redirect("~/logviewer.aspx")
    'End Sub

End Class