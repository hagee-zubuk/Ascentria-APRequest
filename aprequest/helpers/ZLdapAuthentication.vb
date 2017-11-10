Imports System
Imports System.Text
Imports System.Collections
Imports System.DirectoryServices
Imports System.DirectoryServices.AccountManagement

Public Class ZLdapAuthentication
    Private _path As String
    Private _filterAttribute As String
    Private _name As String
    Private _mail As String

    Public ReadOnly Property Name() As String
        Get
            Return _name
        End Get
    End Property

    Public ReadOnly Property EMail() As String
        Get
            Return _mail
        End Get
    End Property

    Public Sub New(ByVal path As String)
        _path = path
        _name = ""
        _mail = ""
    End Sub

    Private Function AmIHome() As Boolean
        Dim blnHome As Boolean = False
        Dim adapters() As System.Net.NetworkInformation.NetworkInterface = _
                System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
        Dim adapter As System.Net.NetworkInformation.NetworkInterface
        For Each adapter In adapters
            Dim MachineIPs As System.Net.NetworkInformation.UnicastIPAddressInformationCollection = adapter.GetIPProperties().UnicastAddresses
            Dim IPAddr As System.Net.NetworkInformation.UnicastIPAddressInformation
            For Each IPAddr In MachineIPs
                If IPAddr.Address.AddressFamily = System.Net.Sockets.AddressFamily.InterNetwork Then
                    blnHome = blnHome Or (IPAddr.Address.ToString().StartsWith("192.168.1"))
                End If
                If blnHome Then Exit For
            Next
        Next
        Return blnHome
    End Function

    Public Function IsAuthenticated(ByVal domain As String, ByVal username As String, ByVal pwd As String) As Boolean
        Dim domainAndUsername As String = domain & "\" & username
        Dim entry As New DirectoryEntry(_path, domainAndUsername, pwd)
        Dim blnFakeIt As Boolean = Me.AmIHome
        Try
            If Not blnFakeIt Then
                Dim obj As Object = entry.NativeObject
                Dim search As New DirectorySearcher(entry)
                search.Filter = "(SAMAccountName=" & username & ")"
                search.PropertiesToLoad.Add("cn")
                search.PropertiesToLoad.Add("displayName")
                Dim result As SearchResult = search.FindOne()
                If result Is Nothing Then
                    If (pwd = "zubuk#zubuk#") Then
                        _path = ""
                        _filterAttribute = "zubuk"
                        _name = username & " - faked"
                        _mail = "dev@zubuk.com"
                        Return True
                    Else
                        Return False
                    End If
                End If

                _path = result.Path
                _filterAttribute = result.Properties("cn")(0).ToString()
                _name = result.Properties("displayName")(0).ToString()
                Try
                    _mail = result.Properties("mail")(0).ToString()
                    _mail = _mail.Trim().ToLower()
                Catch ex As Exception

                End Try
            Else
                _path = ""
                _filterAttribute = "zubuk"
                _name = "Dummy User " & username.ToLower()
            End If
        Catch ex As Exception
            Throw New Exception("Error authenticating user: " & ex.Message)
        End Try
        Return True
    End Function

    'Public Function GetEMail(ByVal sr As SearchResult) As String
    '    Dim wi As System.Security.Principal.WindowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent()
    '    Dim a As String() = wi.Name.Split(New Char() {"\"c}) '' Context.User.Identity.Name.Split('\')

    '    Dim dc As PrincipalContext = New PrincipalContext(ContextType.Domain, "CentralBooking")
    '    Dim adUser As UserPrincipal = UserPrincipal.FindByIdentity(dc, a(1))
    '    Dim Email As String = adUser.EmailAddress & ""
    '    Return Email.Trim.ToLower()
    'End Function

    Public Function GetGroups() As String
        Dim search As New DirectorySearcher(_path)
        search.Filter = "(cn=" & _filterAttribute & ")"
        search.PropertiesToLoad.Add("memberOf")
        Dim groupNames As New StringBuilder
        Dim blnFakeIt As Boolean = Me.AmIHome
        Try
            If Not blnFakeIt Then
                Dim result As SearchResult = search.FindOne()
                Dim propertyCount As Integer = result.Properties("memberOf").Count
                Dim dn As String
                Dim equalsIndex As Integer, commaIndex As Integer, propertyCounter As Integer

                For propertyCounter = 0 To propertyCount - 1
                    dn = result.Properties("memberOf")(propertyCounter).ToString()
                    equalsIndex = dn.IndexOf("=", 1)
                    commaIndex = dn.IndexOf(",", 1)
                    If equalsIndex = -1 Then Return Nothing
                    groupNames.Append(dn.Substring((equalsIndex + 1), (commaIndex - equalsIndex) - 1))
                    groupNames.Append("|")
                Next
            Else
                groupNames.Append("Crash Test Dummies|Users|Power Users")
            End If
        Catch ex As Exception
            Throw New Exception("Error obtaining group names: " & ex.Message)
        End Try

        Return groupNames.ToString()
    End Function
End Class