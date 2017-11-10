Imports System
Imports System.Guid
Imports System.Text
Imports System.Security.Cryptography

Public Class zHashes
    Private Const m_PASSPHRASE As String = "someWhere70aCcent79Deferring19halfhearTed47chaotic76eNdowed"
    Public Const HASH_BYTES As Integer = 24
    Public Const ITERATION_INDEX As Integer = 0
    Public Const SALT_INDEX As Integer = 1
    Public Const SALT_BYTES As Integer = 24
    Public Const PBKDF2_INDEX As Integer = 2
    Public Const PBKDF2_ITERATIONS As Integer = 1000

    '<summary>
    'Computes the PBKDF2-SHA1 hash of a password.
    '</summary>
    '<param name="password">The password to hash.</param>
    '<param name="salt">The salt.</param>
    '<param name="iterations">The PBKDF2 iteration count.</param>
    '<param name="outputBytes">The length of the hash to generate, in bytes.</param>
    '<returns>A hash of the password.</returns>
    Private Function PBKDF2(ByVal password As String, _
                            ByVal salt() As Byte, _
                            ByVal iterations As Integer, _
                            ByVal outputBytes As Integer) As Byte()
        Dim dbpbkdf2 As New Rfc2898DeriveBytes(password, salt)
        dbpbkdf2.IterationCount = iterations
        Return dbpbkdf2.GetBytes(outputBytes)
    End Function

    '<summary>
    'Compares two byte arrays in length-constant time. This comparison
    'method is used so that password hashes cannot be extracted from
    'on-line systems using a timing attack and then attacked off-line.
    '</summary>
    '<param name="a">The first byte array.</param>
    '<param name="b">The second byte array.</param>
    '<returns>True if both byte arrays are equal. False otherwise.</returns>
    Private Function SlowEquals(ByVal a As Byte(), ByVal b As Byte()) As Boolean
        Dim diff As Boolean = CBool(a.Length = b.Length)
        Dim i As Integer = 0
        Dim blnZZ As Boolean = CBool(i < a.Length And i < b.Length)
        '         for (int i = 0; i < a.Length && i < b.Length; i++)
        While blnZZ
            diff = diff And (a(i) = b(i))
            i += 1
            blnZZ = CBool(i < a.Length And i < b.Length)
        End While
        Return diff
    End Function

    '<summary>
    'Validates a password given a hash of the correct one.
    '</summary>
    '<param name="password">The password to check.</param>
    '<param name="goodHash">A hash of the correct password.</param>
    '<returns>True if the password is correct. False otherwise.</returns>
    Public Function ValidatePassword(ByVal password As String, ByVal goodHash As String) As Boolean
        Dim delimeter() As Char = (":")
        Dim hashes() As String = goodHash.Split(delimeter)
        Dim iterations As Integer = Int32.Parse(hashes(ITERATION_INDEX))        ' 0
        Dim salt() As Byte = Convert.FromBase64String(hashes(SALT_INDEX))       ' 1
        Dim hash() As Byte = Convert.FromBase64String(hashes(PBKDF2_INDEX))     ' 2

        Dim testHash() As Byte = PBKDF2(password, salt, iterations, hash.Length)
        Return SlowEquals(hash, testHash)
    End Function

    '<summary>
    'Creates a salted PBKDF2 hash of the password.
    '</summary>
    '<param name="password">The password to hash.</param>
    '<returns>The hash of the password.</returns>
    Public Function CreateHash(ByVal password As String) As String
        Dim csprng As New RNGCryptoServiceProvider
        Dim salt(SALT_BYTES) As Byte
        ' Generate a random salt
        csprng.GetBytes(salt)
        ' Hash the password and encode the parameters
        Dim hashish() As Byte = PBKDF2(password, salt, PBKDF2_ITERATIONS, HASH_BYTES)
        'Return Convert.ToBase64String(hashish)
        Return PBKDF2_ITERATIONS & ":" & _
               Convert.ToBase64String(salt) & ":" & _
               Convert.ToBase64String(hashish)
    End Function

    Public Function CreateHash2(ByVal password As String) As String()
        Dim csprng As New RNGCryptoServiceProvider
        Dim salt(SALT_BYTES) As Byte
        ' Generate a random salt
        csprng.GetBytes(salt)
        ' Hash the password and encode the parameters
        Dim hashish() As Byte = PBKDF2(password, salt, PBKDF2_ITERATIONS, HASH_BYTES)
        Dim VV() As String = {PBKDF2_ITERATIONS.ToString(), Convert.ToBase64String(salt), Convert.ToBase64String(hashish)}
        'Return Convert.ToBase64String(hashish)
        Return VV
    End Function

    Public Function DecryptData(ByVal Message As String) As String
        Dim DataToDecrypt() As Byte
        Try
            DataToDecrypt = Convert.FromBase64String(Message)
        Catch ex As Exception
            Return ""
        End Try

        Dim Results() As Byte
        Dim UTF8 As New UTF8Encoding
        Dim HashProvider As New MD5CryptoServiceProvider
        Dim TDESKey() As Byte = HashProvider.ComputeHash(UTF8.GetBytes(m_PASSPHRASE))
        Dim TDESAlgorithm As New TripleDESCryptoServiceProvider
        TDESAlgorithm.Key = TDESKey
        TDESAlgorithm.Mode = CipherMode.ECB
        TDESAlgorithm.Padding = PaddingMode.PKCS7

        Try
            Dim Decryptor = TDESAlgorithm.CreateDecryptor()
            Results = Decryptor.TransformFinalBlock(DataToDecrypt, 0, DataToDecrypt.Length)
        Catch ex As Exception
            Results = UTF8.GetBytes("")
        Finally
            TDESAlgorithm.Clear()
            HashProvider.Clear()
        End Try
        Return UTF8.GetString(Results)
    End Function

    Public Function EncryptData(ByVal Message As String) As String
        Dim Results() As Byte
        Dim UTF8 As New System.Text.UTF8Encoding
        Dim HashProvider = New MD5CryptoServiceProvider
        Dim TDESKey() As Byte = HashProvider.ComputeHash(UTF8.GetBytes(m_PASSPHRASE))
        Dim TDESAlgorithm As New TripleDESCryptoServiceProvider
        TDESAlgorithm.Key = TDESKey
        TDESAlgorithm.Mode = CipherMode.ECB
        TDESAlgorithm.Padding = PaddingMode.PKCS7
        Dim DataToEncrypt() As Byte = UTF8.GetBytes(Message)
        Try
            Dim Encryptor As ICryptoTransform = TDESAlgorithm.CreateEncryptor()
            Results = Encryptor.TransformFinalBlock(DataToEncrypt, 0, DataToEncrypt.Length)
        Catch ex As Exception
            Results = UTF8.GetBytes("")
        Finally
            TDESAlgorithm.Clear()
            HashProvider.Clear()
        End Try
        Return Convert.ToBase64String(Results)
    End Function

    Public Function MkGUID() As String
        Return NewGuid().ToString().Replace("-", "")
    End Function

    Public Function NewSessionID(ByVal UserName As String) As String
        Dim strIP As String
        Try
            strIP = HttpContext.Current.Request.UserHostAddress
        Catch ex As Exception
            strIP = "127.0.0.1"
        End Try

        Dim oSDS As New dsSessionsTableAdapters.sessionsTableAdapter
        Dim strGUID = Me.MkGUID()
        oSDS.InsertQuery(strGUID, UserName.ToLower(), strIP, Date.Now)

        Return strGUID
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
End Class
