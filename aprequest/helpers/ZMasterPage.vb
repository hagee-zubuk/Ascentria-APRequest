Imports System.Guid
Imports System.Security
Imports System.Security.Cryptography

Public Class ZMasterPage
    Inherits System.Web.UI.MasterPage

    Private _authTick As FormsAuthenticationTicket
    Private _GUID As String
    Private _Username As String
    Private _DisplayName As String

    Public Sub New()
        MyBase.New()
    End Sub

    Public ReadOnly Property GUID() As String
        Get
            Return _GUID
        End Get
    End Property

    Public ReadOnly Property DisplayName() As String
        Get
            Return _DisplayName
        End Get
    End Property

    Private Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim CookieName As String = FormsAuthentication.FormsCookieName
        Dim authCookie As HttpCookie = Request.Cookies(CookieName)
        Dim encryptedTicket As String = authCookie.Value
        Me._authTick = FormsAuthentication.Decrypt(encryptedTicket)
        Dim data() As String = Me._authTick.UserData.Split("|")
        Me._Username = Me._authTick.Name
        Me._DisplayName = data(0)
        'uinfo.CopyTo(Me._Groups, 1)

        Using oUsrTA As New dsUsersTableAdapters.userconfigTA
            Me._GUID = oUsrTA.GetGUIDByUsername(Me._Username)
        End Using
    End Sub
End Class
