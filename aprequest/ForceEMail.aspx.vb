Imports System.Data
Imports System.Data.SqlClient
Imports System.Net.Mail

Partial Public Class ForceEMail
    Inherits ZPageSecure

    Public Function SendEMailMessage(ByVal strMesg$, ByVal strGUID$, Optional ByVal strSubj$ = "Approval Required")
        ' send e-mail to managers of a txn guid=strGUID
        Dim strEMAddr$ = ""
        Dim strCN As String = ConfigurationManager.ConnectionStrings("aprequestConnectionString").ConnectionString
        Dim strSQL As String = "SELECT DISTINCT u1.[email] AS [email]  " & _
                "FROM [userconfig] AS u1 " & _
                "INNER JOIN [managers] AS m ON u1.[username] = m.[manager] " & _
                "INNER JOIN [transactions] AS t ON m.[username] = t.[create_user] " & _
                "WHERE t.[guid]='" & strGUID & "'"

        Using cn As New SqlConnection(strCN)
            cn.Open()
            Dim command As New SqlCommand(strSQL, cn)
            Try
                Dim reader As SqlDataReader = command.ExecuteReader()
                While reader.Read()
                    strEMAddr &= CType(reader, IDataRecord)(0).ToString() & ","
                End While
            Catch ex As Exception
                ' meh
            End Try
        End Using
        'strEMAddr &= "hagee@argao.net"

        Try
            Dim Smtp_Server As New SmtpClient
            Dim e_mail As New MailMessage()
            'Smtp_Server.UseDefaultCredentials = False
            'Smtp_Server.Credentials = New Net.NetworkCredential( _
            '        My.Settings.SMTPUsername, _
            '        My.Settings.SMTPPassword)
            'Smtp_Server.Port = Convert.ToInt32(My.Settings.SMTPPort) ' 587
            'Smtp_Server.EnableSsl = False
            'Smtp_Server.Host = My.Settings.SMTPServer '"70.90.105.91"

            e_mail = New MailMessage()
            e_mail.From = New MailAddress(My.Settings.SMTPSender)
            e_mail.Subject = My.Settings.SMTPSubjectPrefix & " " & strSubj ' Approval Required"
            e_mail.To.Add(strEMAddr)
            e_mail.IsBodyHtml = True
            e_mail.Body = strMesg
            Smtp_Server.Send(e_mail)
            LogMessage("Message sent to: " & strEMAddr & vbCrLf & vbCrLf & strMesg, strGUID, "", "", Nothing)
        Catch error_t As Exception
            'LogMessage("Message sending failed: " & strEMAddr & vbCrLf & strMesg, strGUID, "", "", Nothing)
            ' Message sending failed
            'Err.Clear()
        End Try
        Return True
    End Function


    Private Sub GetTxns()
        Dim strCN As String = ConfigurationManager.ConnectionStrings("aprequestConnectionString").ConnectionString
        Dim strSQL As String = "SELECT TOP 30 [guid], [create_user], [txn_no], [total_amount] " & _
                "FROM [transactions] " & _
                "ORDER BY [txn_no] DESC"
        Dim objLI As ListItem
        Dim strTmp$
        Using cn As New SqlConnection(strCN)
            cn.Open()
            Dim command As New SqlCommand(strSQL, cn)
            Try
                Dim reader As SqlDataReader = command.ExecuteReader()
                While reader.Read()
                    strTmp = reader.Item(2) & " (" & reader.Item(3) & ") " & reader.Item(1)
                    objLI = New ListItem(strTmp, reader.Item(0))
                    ddTxns.Items.Add(objLI)
                End While
                Me.litResult1.Text = "ready"
            Catch ex As Exception
                ' meh
                Me.litResult1.Text = "<h1>ERR (Exception)</h1><p>" & ex.Message & "</p>"
            End Try
        End Using
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        GetTxns()
        Me.txtGUID.Text = Me.ddTxns.SelectedValue
    End Sub

    Protected Sub btnLookup_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnLookup.Click
        Dim strCN As String = ConfigurationManager.ConnectionStrings("aprequestConnectionString").ConnectionString
        Dim strEMAddr$, strGUID$, strTmp$
        strGUID = Me.ddTxns.SelectedValue
        strTmp = "<p>Fetch: " & Me.ddTxns.SelectedValue & "</p>"

        Dim strSQL As String = "SELECT DISTINCT u1.[email] AS [email], u1.[fullname] AS [fullname] " & _
                "FROM [userconfig] AS u1 " & _
                "INNER JOIN [managers] AS m ON u1.[username] = m.[manager] " & _
                "INNER JOIN [transactions] AS t ON m.[username] = t.[create_user] " & _
                "WHERE t.[guid]='" & strGUID & "'"

        strTmp &= "<div>" & strSQL & "</div>"

        strEMAddr = vbCrLf
        Using cn As New SqlConnection(strCN)
            cn.Open()
            Dim command As New SqlCommand(strSQL, cn)
            Try
                Dim strETmp$
                Dim reader As SqlDataReader = command.ExecuteReader()
                While reader.Read()
                    strETmp = CType(reader, IDataRecord)(0).ToString().Trim()
                    If strETmp.Length > 0 Then
                        strEMAddr &= strETmp & " (" & CType(reader, IDataRecord)(1).ToString() & ")," & vbCrLf
                    End If
                End While
            Catch ex As Exception
                ' meh
                strTmp &= "<p><b>ERR:</b> " & ex.Message & "</p>"
            End Try
        End Using
        strTmp &= "<h1>Email addressees</h1><pre>" & strEMAddr & "</pre>"

        ' finally...
        Me.litResult1.Text = strTmp
    End Sub

    Protected Sub btnSend_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSend.Click
        Dim strCN As String = ConfigurationManager.ConnectionStrings("aprequestConnectionString").ConnectionString
        Dim strEMAddr$, strGUID$, strTmp$
        strGUID = Me.ddTxns.SelectedValue
        strTmp = "<p>Fetch: " & Me.ddTxns.SelectedValue & "</p>"

        Dim strSQL As String = "SELECT DISTINCT u1.[email] AS [email], u1.[fullname] AS [fullname] " & _
                "FROM [userconfig] AS u1 " & _
                "INNER JOIN [managers] AS m ON u1.[username] = m.[manager] " & _
                "INNER JOIN [transactions] AS t ON m.[username] = t.[create_user] " & _
                "WHERE t.[guid]='" & strGUID & "'"

        strTmp &= "<pre>" & strSQL & "</pre>"

        strEMAddr = ""
        strTmp &= "<h1>Email addressees</h1><pre>"
        Using cn As New SqlConnection(strCN)
            cn.Open()
            Dim command As New SqlCommand(strSQL, cn)
            Try
                Dim strETmp$
                Dim reader As SqlDataReader = command.ExecuteReader()
                While reader.Read()
                    strETmp = CType(reader, IDataRecord)(0).ToString().Trim()
                    If strETmp.Length > 0 Then
                        strTmp &= strETmp & " (" & CType(reader, IDataRecord)(1).ToString() & ")," & vbCrLf
                        strEMAddr &= strETmp & ","
                    End If
                End While
            Catch ex As Exception
                ' meh
                strTmp &= "<p><b>ERR:</b> " & ex.Message & "</p>"
            End Try
        End Using
        strTmp &= "</pre>"


        Try
            Dim Smtp_Server As New SmtpClient
            Dim e_mail As New MailMessage()
            'Smtp_Server.UseDefaultCredentials = False
            'Smtp_Server.Credentials = New Net.NetworkCredential( _
            '        My.Settings.SMTPUsername, _
            '        My.Settings.SMTPPassword)
            'Smtp_Server.Port = Convert.ToInt32(My.Settings.SMTPPort) ' 587
            'Smtp_Server.EnableSsl = False
            'Smtp_Server.Host = My.Settings.SMTPServer '"70.90.105.91"

            e_mail = New MailMessage()
            e_mail.From = New MailAddress(My.Settings.SMTPSender)
            e_mail.Subject = "TEST-ONLY Approval message"
            e_mail.To.Add(strEMAddr)
            e_mail.Bcc.Add("hagee@zubuk.com")
            e_mail.IsBodyHtml = True
            e_mail.Body = "This is a debug message to trace mail problems in the AP check request app." & _
                    vbCrLf & vbCrLf & "..." & vbCrLf & vbCrLf & _
                    "<a href=""http://www.argao.net/zubuk_ip.html"">Click here for DL IP</a>" & _
                    vbCrLf & vbCrLf & "..." & vbCrLf & vbCrLf & _
                    "Pellentesque bibendum tempor nulla vel venenatis. Aliquam erat volutpat. Sed id " & _
                    "tristique turpis, vitae bibendum quam. Suspendisse condimentum, magna vitae vulp" & _
                    "utate faucibus, mi sapien dictum massa, eu hendrerit erat nisl vel lacus. In viv" & _
                    "erra elementum ipsum, a pellentesque nunc luctus sit amet." & vbCrLf & "Nullam a" & _
                    "uctor eros vel efficitur rutrum. Nulla et neque eros. Ut ac orci nec nisl dictum" & _
                    " convallis. Donec mollis neque a tortor tempor, nec dictum dolor gravida. Fusce " & _
                    "lacinia magna tortor, vel volutpat lorem sagittis eu. Nullam egestas lacinia aug" & _
                    "ue, nec tempor leo posuere sit amet. Nullam accumsan cursus tortor, in ullamcorp" & _
                    "er nunc mollis eget. Nam tempor sit amet orci eu bibendum. Phasellus et ligula q" & _
                    "uis orci sollicitudin semper. Proin a nibh ipsum. Donec nec nibh eros." & vbCrLf & _
                    "Etiam nec convallis massa. Vestibulum velit libero, tristique eget hendrerit ege" & _
                    "t, eleifend sit amet nunc. Vivamus eget nisl tristique, aliquam risus eget, temp" & _
                    "or urna. Pellentesque tempor lorem nec quam aliquet, scelerisque cursus est rhon" & _
                    "cus. Fusce sed quam iaculis, tristique felis eu, iaculis diam. Etiam orci sem, l" & _
                    "acinia et condimentum vel, pharetra sit amet nisi. Aliquam nec lacus dapibus, au" & _
                    "ctor nisl non, scelerisque dolor. Donec risus enim, interdum quis dolor vitae, c" & _
                    "ondimentum laoreet nunc. Nullam pellentesque ultrices laoreet." & vbCrLf & vbCrLf & _
                    "Fusce id tristique tortor. Nullam aliquam nisl neque, eget lacinia ante imperdie" & _
                    "t in."
            Smtp_Server.Send(e_mail)
            strTmp &= "<p>Mail sent</p>"
        Catch error_t As Exception
            strTmp &= "<p><b>ERR:</b> " & error_t.Message & "</p>"
        End Try

        ' finally...
        Me.litResult1.Text = strTmp
    End Sub
End Class