Public Partial Class pt_cal
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim tcell As TableCell
        Dim outrow As TableRow
        Dim lngCnt As Long
        ' Figure out the target

        outrow = New TableRow

        lngCnt = lngCnt + 1
        tcell = New TableCell
        tcell.Text = ""
        tcell.CssClass = "apprmaxi"
        tcell.HorizontalAlign = HorizontalAlign.Right
        tcell.VerticalAlign = VerticalAlign.Middle
        outrow.Cells.Add(tcell)


    End Sub

End Class