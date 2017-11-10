Imports System.Data
Imports System.Data.SqlClient
Imports System.Drawing
Imports System.IO
Imports System.IO.File
Imports System.Net
Imports System.Net.Mail
Imports System.Net.Mime
Imports System.Security.Cryptography

Module modUtils

    Function GetTxnExtent()
        Dim cn As New SqlClient.SqlConnection
        Dim ds As New DataSet
        Dim dt As New DataTable

        Dim strCN As String = ConfigurationManager.ConnectionStrings("aprequestConnectionString").ConnectionString
        cn.ConnectionString = strCN
        Dim cmd As New SqlClient.SqlCommand
        cmd.Connection = cn
        cmd.CommandType = CommandType.Text
        Dim strSQL As String = "SELECT DISTINCT ( " & _
                "CAST (YEAR([create_at])AS CHAR(4)) + " & _
                "RIGHT('00' + CONVERT(NVARCHAR(2)," & _
                "DATEPART(MONTH, [create_at])),2) " & _
                ") AS [aggr] " & _
                ", MONTH([create_at]) AS [mo] " & _
                ", YEAR([create_at]) AS [yr] " & _
                "FROM [transactions] ORDER BY [aggr] DESC"
        cmd.CommandText = strSQL
        Try
            cn.Open()
            cmd.ExecuteNonQuery()
            Dim da As New SqlDataAdapter(cmd)
            da.Fill(ds)
        Catch ex As Exception
        Finally
            cn.Close()
        End Try

        dt = ds.Tables(0)
        Dim dict As New Dictionary(Of String, String)
        For Each row As DataRow In dt.Rows
            dict.Add(row("aggr"), MonthName(row("mo")) & " " & row("yr"))
        Next
        Return dict

    End Function

    Function LogMessage(ByVal msg$, ByVal txn_guid$, ByVal dtl_guid$, ByVal usr_guid$, ByRef objRq As System.Web.HttpRequest, Optional ByVal level As Byte = 255) As Boolean
        ' check if message should be logged in the first place
        Dim logset As Byte = My.Settings.LogLevel
        If level < logset Then Return False

        ' okay, figure out the user cookie goodness...
        Dim CookieName As String = FormsAuthentication.FormsCookieName
        Dim strSess$, strUname$, strRaw$, strAgent$
        strUname = "unknown"
        strRaw = ""
        strAgent = ""
        Try
            If Not objRq Is Nothing Then
                Dim authCookie As HttpCookie = objRq.Cookies(CookieName)
                If authCookie Is Nothing Then
                    strUname = "unknwn"
                    strRaw = ""
                    strAgent = ""
                Else
                    Dim encryptedTicket As String = authCookie.Value
                    Dim authTick As System.Web.Security.FormsAuthenticationTicket = FormsAuthentication.Decrypt(encryptedTicket)
                    ' Me._Username = authTick.Name
                    strUname = Microsoft.VisualBasic.Left(authTick.Name, 60).ToLower()
                    strRaw = objRq.RawUrl
                    strAgent = objRq.UserAgent
                    ' Dim uinfo() As String = authTick.UserData.Split("|")
                    ' DisplayName is uinfo(0)
                End If
            End If
        Catch ex As Exception
            ' strUname = "unknown"
            ' strRaw = ""
            ' strAgent = ""
        End Try
        strSess = GetMd5Hash(msg)

        ' now write it out
        Dim FILE_NAME As String = My.Settings.DownloadPath & "\activity.log"
        Try
            Dim objWriter As New System.IO.StreamWriter(FILE_NAME, True)
            objWriter.WriteLine(Format(Date.Now, "yyMMddHHmmss") & ": " & strUname & ": " & txn_guid & ": " & dtl_guid & ": " & usr_guid & ": ")
            objWriter.WriteLine(vbTab & vbTab & msg)
            objWriter.WriteLine(vbTab & vbTab & strRaw & strAgent)
            objWriter.Close()
        Catch ex As Exception
            'aw crap
        End Try

        Try
            Using objLO As New dsSessionsTableAdapters.activity_logTA
                objLO.LogAdd(objRq.UserHostAddress, _
                        strSess, _
                        strUname, _
                        strRaw & "|" & strAgent, _
                        msg, _
                        txn_guid, _
                        dtl_guid, _
                        usr_guid)
            End Using
        Catch ex As Exception
            'Dim evtlog As New EventLog
            'evtlog.Source = "APREQUEST Web App"
            'evtlog.WriteEntry("Write to SQL Activity Log failed: " + ex.Message, EventLogEntryType.FailureAudit)
            'evtlog.Dispose()
            Return False
        End Try

        Return True
    End Function

    Public Function GetMd5Hash(ByVal input As String) As String
        ' Create a new Stringbuilder to collect the bytes 
        ' and create a string. 
        Dim sBuilder As New StringBuilder()
        Using md5Hash As MD5 = MD5.Create()
            ' Convert the input string to a byte array and compute the hash. 
            Dim data As Byte() = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input))
            ' Loop through each byte of the hashed data  
            ' and format each one as a hexadecimal string. 
            Dim i As Integer
            For i = 0 To data.Length - 1
                sBuilder.Append(data(i).ToString("x2"))
            Next i
        End Using
        ' Return the hexadecimal string. 
        Return sBuilder.ToString()
    End Function 'GetMd5Hash

    ' Verify a hash against a string. 
    Public Function VerifyMd5Hash(ByVal input As String, ByVal hash As String) As Boolean
        ' Hash the input. 
        hash = Trim(hash)
        Dim hashOfInput As String = GetMd5Hash(input)
        ' Create a StringComparer an compare the hashes. 
        Dim comparer As StringComparer = StringComparer.OrdinalIgnoreCase
        Return CBool(hashOfInput = hash)
    End Function 'Verify

    Public Sub Image2Byte(ByRef NewImage As Image, ByRef ByteArr() As Byte)
        Dim ImageStream As MemoryStream
        Try
            ReDim ByteArr(0)
            If NewImage IsNot Nothing Then
                ImageStream = New MemoryStream
                NewImage.Save(ImageStream, Imaging.ImageFormat.Jpeg)
                ReDim ByteArr(CInt(ImageStream.Length - 1))
                ImageStream.Position = 0
                ImageStream.Read(ByteArr, 0, CInt(ImageStream.Length))
            End If
        Catch ex As Exception

        End Try
    End Sub

    Public Function GetStringWidth(ByVal sTmp$, ByVal sFnt$, ByVal iSz As Long) As Single
        Dim objBitmap As New Bitmap(500, 200)
        Dim objGraphics As Graphics

        objGraphics = Graphics.FromImage(objBitmap)
        Dim strSize As SizeF = objGraphics.MeasureString(sTmp, New Font(sFnt, iSz))
        objBitmap.Dispose()
        objGraphics.Dispose()

        Return strSize.Width
    End Function

    Public Function FitString(ByVal strZ$, ByVal sFnt$, ByVal iSz As Long, ByVal sSiz As Single) As String
        Dim strTmp As String
        Dim lngJ As Long = strZ.Length
        Dim sWid As Single

        Do
            strTmp = Left(strZ, lngJ)
            If lngJ < strZ.Length Then strTmp &= ".."
            sWid = GetStringWidth(strTmp, sFnt, iSz)
            lngJ -= 1
        Loop While sWid > sSiz And lngJ > 0
        Return strTmp
    End Function

    Public Function SendReqEMailMessage(ByVal strMesg$, ByVal strGUID$, Optional ByVal strSubj$ = "Approval Required")
        ' send e-mail to managers of a txn guid=strGUID
        Dim strEMAddr$ = ""
        Dim strCN As String = ConfigurationManager.ConnectionStrings("aprequestConnectionString").ConnectionString
        Dim strSQL As String = "SELECT DISTINCT u2.[email] AS [reqstr], u1.[email] AS [email]  " & _
                "FROM [userconfig] AS u1 " & _
                "INNER JOIN [managers] AS m ON u1.[username] = m.[manager] " & _
                "INNER JOIN [transactions] AS t ON m.[username] = t.[create_user] " & _
                "INNER JOIN [userconfig] AS u2 ON t.[create_user] = u2.[username] " & _
                "WHERE t.[guid]='" & strGUID & "'"

        Using cn As New SqlConnection(strCN)
            cn.Open()
            Dim command As New SqlCommand(strSQL, cn)
            Try
                Dim reader As SqlDataReader = command.ExecuteReader()
                Dim strETmp$ = ""
                While reader.Read()
                    If strETmp.Length <= 0 Then strETmp = CType(reader, IDataRecord)(0).ToString().Trim()
                    strETmp = CType(reader, IDataRecord)(1).ToString().Trim()
                    If strETmp.Length > 0 Then strEMAddr &= CType(reader, IDataRecord)(0).ToString() & ","
                End While
            Catch ex As Exception
                ' meh
            End Try
        End Using
        'strEMAddr &= "hagee@argao.net"

        Try
            Dim Smtp_Server As New SmtpClient
            Dim e_mail As New MailMessage()
            Smtp_Server.UseDefaultCredentials = False
            Smtp_Server.Credentials = New Net.NetworkCredential( _
                    My.Settings.SMTPUsername, _
                    My.Settings.SMTPPassword)
            Smtp_Server.Port = Convert.ToInt32(My.Settings.SMTPPort) ' 587
            Smtp_Server.EnableSsl = False
            Smtp_Server.Host = My.Settings.SMTPServer '"70.90.105.91"

            e_mail = New MailMessage()
            e_mail.From = New MailAddress(My.Settings.SMTPSender)
            e_mail.Subject = My.Settings.SMTPSubjectPrefix & " " & strSubj ' Approval Required"
            e_mail.To.Add(strEMAddr)
            e_mail.IsBodyHtml = True
            e_mail.Body = strMesg
            Smtp_Server.Send(e_mail)
            LogMessage("Message sent to: " & strEMAddr & vbCrLf & vbCrLf & strMesg, strGUID, "", "", Nothing)
        Catch error_t As Exception
            LogMessage("Message sending failed: " & strEMAddr & vbCrLf & strMesg, strGUID, "", "", Nothing)
            LogMessage("FAILED: Message sent to: " & strEMAddr & vbCrLf & vbCrLf & _
                       strMesg & vbCrLf & vbCrLf & _
                       error_t.Message, strGUID, "", "", Nothing)
            Err.Clear()
        End Try
        Return True
    End Function

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
                Dim strETmp$ = ""
                Dim reader As SqlDataReader = command.ExecuteReader()
                While reader.Read()
                    strETmp = CType(reader, IDataRecord)(0).ToString().Trim()
                    If strETmp.Length > 0 Then strEMAddr &= CType(reader, IDataRecord)(0).ToString() & ","
                End While
            Catch ex As Exception
                ' meh
            End Try
        End Using
        'strEMAddr &= "hagee@argao.net"

        Try
            Dim Smtp_Server As New SmtpClient
            Dim e_mail As New MailMessage()
            Smtp_Server.UseDefaultCredentials = False
            Smtp_Server.Credentials = New Net.NetworkCredential( _
                    My.Settings.SMTPUsername, _
                    My.Settings.SMTPPassword)
            Smtp_Server.Port = Convert.ToInt32(My.Settings.SMTPPort) ' 587
            Smtp_Server.EnableSsl = False
            Smtp_Server.Host = My.Settings.SMTPServer '"70.90.105.91"

            e_mail = New MailMessage()
            e_mail.From = New MailAddress(My.Settings.SMTPSender)
            e_mail.Subject = My.Settings.SMTPSubjectPrefix & " " & strSubj ' Approval Required"
            e_mail.To.Add(strEMAddr)
            e_mail.IsBodyHtml = True
            e_mail.Body = strMesg
            Smtp_Server.Send(e_mail)
            LogMessage("Message sent to: " & strEMAddr & vbCrLf & vbCrLf & strMesg, strGUID, "", "", Nothing)
        Catch error_t As Exception
            LogMessage("Message sent to: " & strEMAddr & vbCrLf & vbCrLf & _
                       strMesg & vbCrLf & vbCrLf & _
                       error_t.Message, strGUID, "", "", Nothing)
            ' Message sending failed
            'Err.Clear()
        End Try
        Return True
    End Function

End Module
