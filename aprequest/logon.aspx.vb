Imports System.Net.NetworkInformation

Partial Public Class logon
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        txtDomain.Text = My.Settings.Domain
        If Not Request.Cookies("MSG") Is Nothing Then
            errorLabel.Text = Server.HtmlEncode(Request.Cookies("MSG").Value)
            Response.Cookies("MSG").Value = ""
 '       Else
        '    Dim adapters() As NetworkInterface = NetworkInterface.GetAllNetworkInterfaces()
            'Dim strAddr$
            'Try
            '    strAddr = Request.UserHostAddress
            'Catch ex As Exception
            '    strAddr = "-"
            'End Try
        '    For Each adp As NetworkInterface In adapters
        '        Dim aProp As IPInterfaceProperties = adp.GetIPProperties()
        '        Dim uniCast As UnicastIPAddressInformationCollection = aProp.UnicastAddresses
        '        If uniCast.Count > 0 Then
        '            For Each uni As UnicastIPAddressInformation In uniCast
        '                strAddr &= uni.Address.ToString()
        '            Next
        '        End If
        '    Next
'            errorLabel.Text = "Detected connection from: " & strAddr
        End If
    End Sub

    Private Function AmILocal() As Boolean
        Dim boolRes As Boolean = False
        Dim adapters() As NetworkInterface = NetworkInterface.GetAllNetworkInterfaces()
        For Each adp As NetworkInterface In adapters
            Dim aProp As IPInterfaceProperties = adp.GetIPProperties()
            Dim uniCast As UnicastIPAddressInformationCollection = aProp.UnicastAddresses
            If uniCast.Count > 0 Then
                For Each uni As UnicastIPAddressInformation In uniCast
                    boolRes = boolRes Or uni.Address.ToString().StartsWith("192.168.111")
                Next
            End If
            If boolRes Then Exit For
        Next
        Try
            If Request.UserHostAddress.Trim().StartsWith("127.0.0.1") Or _
                    Request.UserHostAddress.Trim().StartsWith("::1") Then boolRes = True
        Catch ex As Exception

        End Try

        Return boolRes
    End Function

    Protected Sub AttemptLogon(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnLogin.Click
        Dim adPath As String = My.Settings.LDAP_Path
        Dim adAuth As New ZLdapAuthentication(adPath)
        If AmILocal() Then
            ' WARNING!!! everything Zubuk-local (192.168.111.x) will be authenticated
            ' TODO: fix this someday
            Dim authTicket As New FormsAuthenticationTicket(1, _
                    txtUsername.Text, _
                    DateTime.Now, _
                    DateTime.Now.AddMinutes(60), _
                    False, txtUsername.Text & "|Faker|Users|Power Users")
            Dim encryptedTicket As String = FormsAuthentication.Encrypt(authTicket)
            Dim authCookie As New HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
            Response.Cookies.Add(authCookie)
            LogMessage("logon", "", "", "", Request)
            Response.Redirect(FormsAuthentication.GetRedirectUrl(txtUsername.Text, False))
        Else
            Try
                If (adAuth.IsAuthenticated(txtDomain.Text, txtUsername.Text, txtPassword.Text)) Then
                    Dim groups As String = adAuth.GetGroups()
                    'Dim isCookiePersistent As Boolean = False
                    Dim authTicket As New FormsAuthenticationTicket(1, _
                            txtUsername.Text, _
                            DateTime.Now, _
                            DateTime.Now.AddMinutes(60), _
                            False, adAuth.Name & "|" & groups)
                    Dim encryptedTicket As String = FormsAuthentication.Encrypt(authTicket)
                    Dim authCookie As New HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                    'If (isCookiePersistent = True) Then authCookie.Expires = authTicket.Expiration
                    Response.Cookies.Add(authCookie)
                    LogMessage("logon", "", "", "", Request)
                    Dim groupCookie As New HttpCookie("X-GroupMem", groups)
                    Response.Cookies.Add(groupCookie)
                    Response.Redirect(FormsAuthentication.GetRedirectUrl(txtUsername.Text, False))
                Else
                    errorLabel.Text = "Authentication did not succeed. Try again."
                End If
            Catch ex As Exception
                errorLabel.Text = "Authentication error: " & ex.Message
            End Try
        End If
    End Sub

End Class