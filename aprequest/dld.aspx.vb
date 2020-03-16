Public Partial Class dld1
    Inherits System.Web.UI.Page
    'later on: Inherits ZPageSecure

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim oEnc As zHashes = New zHashes 
        Dim strFN As String = Request("FSN")        
        Dim strDc As String

        strDc = System.Web.HttpUtility.UrlDecode( strFN )
        strFN = Session("NSF")
        If Not System.IO.File.Exists(strFN) Then    
            Response.Write( "404: File could not be read: " & strDc & "|" & strFN )
            Response.End()
        Else
            Try
                Dim oFI As System.IO.FileInfo = New System.IO.FileInfo(strFN)
                oFI.Length.ToString()
                Response.ContentType = "application/octet-stream"
                'Response.AddHeader("Content-Type", "application/octet-stream")
                Response.AddHeader("Content-Transfer-Encoding", "Binary")
                Response.AddHeader("Content-Disposition", "attachment;filename=""" & oFI.Name.ToString() & """")
                Response.AddHeader("Content-Length", oFI.Length.ToString())
                
                Response.WriteFile(strFN)

            Catch ex As Exception
                Response.Write( "500: An error occured reading the file: " & strFN )
            Finally
                Response.End()
            End Try
        End If
    End Sub
    ' c6jZ%2bjtQ8zETCQf%2f4F%2b%2fbuLfEQ2YhkUQ2O6jbDi4EjNwgN9MZrpm0XzXM1fmlgRv
End Class