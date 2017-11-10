Imports System.Net.Mail
Imports System.Web.SessionState
Imports System.Web.Security
Imports System.Security.Principal

Public Class Global_asax
    Inherits System.Web.HttpApplication

    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when the application is started
    End Sub

    Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when the session is started
    End Sub

    Sub Application_BeginRequest(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires at the beginning of each request
    End Sub

    Sub Application_AuthenticateRequest(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires upon attempting to authenticate the user
        Dim cookieName As String = FormsAuthentication.FormsCookieName
        Dim authCookie As HttpCookie = Context.Request.Cookies(cookieName)
        If authCookie Is Nothing Then Exit Sub
        Dim authTicket As FormsAuthenticationTicket = Nothing
        Try
            authTicket = FormsAuthentication.Decrypt(authCookie.Value)
        Catch ex As Exception
            ' write exception to a logfile somewhere
            Exit Sub
        End Try
        If authTicket Is Nothing Then Exit Sub
        ' when the ticket was created, userdata property was assigned a pipe-delimited string of group names
        Dim groups() As String = authTicket.UserData.Split("|")
        Dim id As GenericIdentity = New GenericIdentity(authTicket.Name, "LdapAuthentication")
        Dim principal As New GenericPrincipal(id, groups)
        Context.User = principal
    End Sub

    Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when an error occurs
        'Dim ctx As HttpContext = HttpContext.Current
        'Dim exception As Exception = ctx.Server.GetLastError()
        'Dim errorInfo = "<br />URL: " & ctx.Request.Url.ToString() & _
        '        "<br />Source: " & exception.Source & _
        '        "<br />Message: " & exception.Message & _
        '        "<br />Stack trace: " & exception.StackTrace
        'ctx.Response.Write(errorInfo)
        'ctx.Server.ClearError()
        'Try
        '    Dim mm As New MailMessage
        '    mm.From = New MailAddress(My.Settings.MailSender)
        '    mm.To.Add(New MailAddress(My.Settings.DebugRecpt))
        '    mm.Subject = "General Error"
        '    mm.Body = errorInfo
        '    mm.IsBodyHtml = True
        '    mm.Priority = MailPriority.Normal
        '    Dim smtp = New SmtpClient
        '    smtp.Send(mm)
        'Catch ex As Exception
        'End Try
    End Sub

    Sub Session_End(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when the session ends
    End Sub

    Sub Application_End(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when the application ends
    End Sub

End Class