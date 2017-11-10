Imports System.Guid
Imports System.Security
Imports System.Security.Cryptography

Public Enum AccessLevel
    Guest = 0
    Requester = 1
    Approver = 2
    AP_Manager = 4
    Admin = 8
End Enum

Public Class ZPageSecure
    Inherits System.Web.UI.Page
    Private _authTick As FormsAuthenticationTicket
    Private _GUID As String
    Private _HasPhrase As Boolean
    Private _Username As String
    Private _DisplayName As String

    Public ReadOnly Property UserName() As String
        Get
            Return Me._Username
        End Get
    End Property

    Public ReadOnly Property DisplayName() As String
        Get
            Return Me._DisplayName
        End Get
    End Property

    Public ReadOnly Property GUID() As String
        Get
            Return Me._GUID
        End Get
    End Property

    Public ReadOnly Property AmtLimit() As Double
        Get
            Using oUsrTA As New dsUsersTableAdapters.userconfigTA
                Return (oUsrTA.GetLimitByGUID(Me._GUID))
            End Using
        End Get
    End Property

    Public ReadOnly Property IsAdmin() As Boolean
        Get
            If Me._GUID Is Nothing Then Return False
            Using oUsrTA As New dsUsersTableAdapters.userconfigTA
                Return (oUsrTA.IsGUIDAdmin(Me._GUID))
            End Using
        End Get
    End Property

    Public ReadOnly Property IsApprover() As Boolean
        Get
            If Me._GUID Is Nothing Then Return False
            Using oUsrTA As New dsUsersTableAdapters.userconfigTA
                Return (oUsrTA.IsGUIDApprover(Me._GUID))
            End Using
        End Get
    End Property

    Public ReadOnly Property IsExporter() As Boolean
        Get
            Using oUsrTA As New dsUsersTableAdapters.userconfigTA
                Return (oUsrTA.IsGUIDManager(Me._GUID))
            End Using
        End Get
    End Property

    Public ReadOnly Property IsFinance() As Boolean
        Get
            If Me._GUID Is Nothing Then Return False
            Using oUsrTA As New dsUsersTableAdapters.userconfigTA
                Return (oUsrTA.IsGUIDFinance(Me._GUID))
            End Using
        End Get
    End Property

    Public ReadOnly Property IsManager() As Boolean
        ' This now means, "EXPORTER"/"PROCESSOR"...
        Get
            If Me._GUID Is Nothing Then Return False
            Using oUsrTA As New dsUsersTableAdapters.userconfigTA
                Return (oUsrTA.IsGUIDManager(Me._GUID))
            End Using
        End Get
    End Property

    Public ReadOnly Property IsRequester() As Boolean
        Get
            If Me._GUID Is Nothing Then Return False
            Using oUsrTA As New dsUsersTableAdapters.userconfigTA
                Return (oUsrTA.IsGUIDRequester(Me._GUID))
            End Using
        End Get
    End Property

    Public ReadOnly Property IsMultisite() As Boolean
        Get
            If Me._GUID Is Nothing Then Return False
            Using oUsrTA As New dsUsersTableAdapters.userconfigTA
                Return (oUsrTA.IsGUIDMultisite(Me._GUID))
            End Using
        End Get
    End Property

    Public ReadOnly Property HasPhrase() As Boolean
        Get
            Return Me._HasPhrase
        End Get
    End Property

    Public Function CanApprove(ByVal uname As String) as Boolean
        Dim strSQL As String = "SELECT COUNT([manager]) AS cnt " & _
                "FROM [managers] AS m " & _
                "INNER JOIN [userconfig] AS u ON u.[username]=m.[manager] " & _
                "WHERE u.[guid]='" & Me._GUID & "' " & _
                "AND m.[username]='" & uname & "'"
        Dim ds As New DataSet
        Dim cn As Sqlclient.SqlConnection
        Try
            Dim strCN$ = ConfigurationManager.ConnectionStrings("aprequestConnectionString").ConnectionString
            cn = New SqlClient.SqlConnection(strCN)
            Dim cmd As New SqlClient.SqlCommand(strSQL, cn)
            cn.Open()
            cmd.ExecuteNonQuery()
            Dim da As New System.Data.SqlClient.SqlDataAdapter(cmd)
            da.Fill(ds)
        Catch ex As Exception
        Finally
            cn.Close()
        End Try

        If ds.Tables(0).Rows.Count>0 then
            Dim row As DataRow = ds.Tables(0).Rows(0)
            If CLng(row("cnt")) > 0 Then
                Return true
            End If
        End If

        Return False
    End Function

    Public Sub New()
        MyBase.New()
    End Sub

    Private Sub Page_PreLoad(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreLoad
        Dim CookieName As String = FormsAuthentication.FormsCookieName
        Dim authCookie As HttpCookie = Request.Cookies(CookieName)
        Dim encryptedTicket As String = authCookie.Value
        Me._authTick = FormsAuthentication.Decrypt(encryptedTicket)
        Dim uinfo() As String = Me._authTick.UserData.Split("|")
        Me._Username = Me._authTick.Name
        Me._DisplayName = uinfo(0)
        'uinfo.CopyTo(Me._Groups, 1)
        Using oUsrTA As New dsUsersTableAdapters.userconfigTA
            Me._GUID = oUsrTA.GetGUIDByUsername(Me._Username)
            Me._HasPhrase = False 'CBool(oUsrTA.HasPhrase(Me._GUID) > 0)
        End Using
        If Me._GUID Is Nothing Then ' login again!
            Response.Cookies(CookieName).Expires = DateAdd(DateInterval.Day, 1, Now)
            Response.Cookies("MSG").Value = "User validation cookie invalid. Please re-authenticate."
            Response.Redirect("~/logon.aspx")
        End If
    End Sub
End Class
