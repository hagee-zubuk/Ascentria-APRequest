Imports System.Drawing
Imports System.Data
Imports System.Data.SqlClient

Public Structure FY_INFO
    Public FY_Title As String
    Public FY_ID As String
End Structure

Public Class ZUtils
    Private Function GetStringWidth(ByVal sTmp$, ByVal sFnt$, ByVal iSz As Long) As Single
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


    Public Function FetchSageTitle(ByVal strCode As String, ByVal strTable As String) As String
        Dim strType As String = strTable.ToLower()
        Dim strPref As String = strCode
        Dim strRet As String
        Select Case strType
            Case "entity"
                strRet = FetchTableData(strPref, "tblAcctCode_1")
            Case "region"
                strRet = FetchTableData(strPref, "tblAcctCode_2")
            Case "site"
                strRet = FetchTableData(strPref, "tblAcctCode_10")
            Case "vendor"
                strRet = FetchTableData(strPref, "tblAPVendor", "sName", "sVendorID")
            Case "dept"
                strRet = FetchTableData(strPref, "tblAcctCode_5")
            Case "serv"
                strRet = FetchTableData(strPref, "tblAcctCode_4")
            Case "glac"
                strRet = FetchTableData(strPref, "tblAcctCode_0")
            Case "payr"
                strRet = FetchTableData(strPref, "tblAcctCode_3")
            Case "pern"
                strRet = FetchTableData(strPref, "tblAcctCode_9")
            Case "fy"
                strRet = FetchTableData(strPref, "tblAcctCode_7")
            Case Else
                strRet = ""
        End Select
        Return (strRet)
    End Function

    Public Function FetchVendorTitle(ByVal strCode as string) As String
        Dim cn As New SqlClient.SqlConnection
        Dim ds As New DataSet
        Dim dt As New DataTable

        Dim strCN As String = ConfigurationManager.ConnectionStrings("SAGE_DBConnectionString").ConnectionString
        cn.ConnectionString = strCN
        Dim cmd As New SqlClient.SqlCommand
        cmd.Connection = cn
        cmd.CommandType = CommandType.Text
        Dim strSQL As String = "SELECT [sName], [sVendorID], [sMainPrintedAddress] FROM [tblAPVendor] " & _
                "WHERE [sVendorID] LIKE @code"
        cmd.CommandText = strSQL
        cmd.Parameters.AddWithValue("@code", strCode)
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
        Dim txtItem As String = ""
        For Each row As DataRow In dt.Rows
            If row("sMainPrintedAddress").ToString().Length > 2 Then
                txtItem &= row("sName").ToString() & vbCrLf & _
                        row("sMainPrintedAddress").ToString()
            Else
                txtItem &= row("sName").ToString() & vbCrLf 
            end if

            'txtItem = txtItem.Trim()
            If txtItem <> "" Then Exit For
        Next
        Return txtItem
    End Function

    Private Function FetchTableData(ByVal prefixText As String, _
            ByVal tblName As String, _
            Optional ByVal fld1 As String = "sTitle", _
            Optional ByVal fld2 As String = "sCodeID") As String
        Dim cn As New SqlClient.SqlConnection
        Dim ds As New DataSet
        Dim dt As New DataTable

        Dim strCN As String = ConfigurationManager.ConnectionStrings("SAGE_DBConnectionString").ConnectionString
        cn.ConnectionString = strCN
        Dim cmd As New SqlClient.SqlCommand
        cmd.Connection = cn
        cmd.CommandType = CommandType.Text
        Dim strSQL As String = "SELECT [" & fld1 & "] FROM [" & tblName & "] " & _
                "WHERE [" & fld2 & "] LIKE @code"
        cmd.CommandText = strSQL
        cmd.Parameters.AddWithValue("@code", prefixText)
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
        Dim txtItem As String = ""
        For Each row As DataRow In dt.Rows
            txtItem = row(fld1).ToString().Trim()
            If txtItem <> "" Then Exit For
        Next
        Return txtItem
    End Function

    Public Function FetchFiscalYears() As List(Of FY_INFO)
        Dim tmpFY As New List(Of FY_INFO)
        Dim cn As New SqlClient.SqlConnection
        Dim ds As New DataSet
        Dim dt As New DataTable

        Dim strCN As String = ConfigurationManager.ConnectionStrings("SAGE_DBConnectionString").ConnectionString
        cn.ConnectionString = strCN
        Dim cmd As New SqlClient.SqlCommand
        cmd.Connection = cn
        cmd.CommandType = CommandType.Text
        Dim strSQL As String = "SELECT [sCodeID], [sTitle] FROM [tblAcctCode_7] WHERE [sStatus]='A' ORDER BY [sTitle] ASC"
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
        For Each row As DataRow In dt.Rows
            Dim fyi As FY_INFO
            fyi.FY_ID = row("scodeid").ToString().Trim()
            fyi.FY_Title = row("stitle").ToString().Trim()
            tmpFY.Add(fyi)
        Next
        Return tmpFY
    End Function

    Public Function FetchSageData(ByVal strTable As String, Optional ByVal blnZ As Boolean = False) As Dictionary(Of String, String)
        Dim strType As String = strTable.ToLower()
        Dim dicRet As Dictionary(Of String, String)
        Select Case strType
            Case "entity"
                dicRet = FetchSageTableData("tblAcctCode_1", , , blnZ)
            Case "region"
                dicRet = FetchSageTableData("tblAcctCode_2", , , blnZ)
            Case "site"
                dicRet = FetchSageTableData("tblAcctCode_10", , , blnZ)
            Case "vendor"
                dicRet = FetchSageTableData("tblAPVendor", "sName", "sVendorID", blnZ)
            Case "dept"
                dicRet = FetchSageTableData("tblAcctCode_5", , , blnZ)
            Case "serv"
                dicRet = FetchSageTableData("tblAcctCode_4", , , blnZ)
            Case "glac"
                dicRet = FetchSageTableData("tblAcctCode_0", , , blnZ)
            Case "payr"
                dicRet = FetchSageTableData("tblAcctCode_3", , , blnZ)
            Case "pern"
                dicRet = FetchSageTableData("tblAcctCode_9", , , blnZ)
            Case "fy"
                dicRet = FetchSageTableData("tblAcctCode_7", , , blnZ)
            Case Else
                dicRet = Nothing
        End Select
        Return (dicRet)
    End Function

    Public Function FetchMgrs(ByVal strUser As String) As List(Of String)
        Dim strCN As String = ConfigurationManager.ConnectionStrings("aprequestConnectionString").ConnectionString
        Dim cn As New SqlClient.SqlConnection(strCN)
        Dim strSQL As String = "SELECT DISTINCT [manager] FROM [managers] WHERE [username]='" & _
                strUser & "' ORDER BY [manager] ASC"
        Dim cmd As New SqlClient.SqlCommand(strSQL, cn)
        Dim ds As New DataSet
        Try
            cn.Open()
            cmd.ExecuteNonQuery()
            Dim da As New SqlDataAdapter(cmd)
            da.Fill(ds)
        Catch ex As Exception
        Finally
            cn.Close()
        End Try

        Dim dt As New DataTable
        dt = ds.Tables(0)
        Dim dict As New List(Of String)
        For Each row As DataRow In dt.Rows
            Try
                dict.Add(row("manager"))
            Catch ex As Exception

            End Try
        Next
        Return dict
    End Function

    Public Function FetchUsers() As Dictionary(Of String, String)
        Dim strCN As String = ConfigurationManager.ConnectionStrings("aprequestConnectionString").ConnectionString
        Dim cn As New SqlClient.SqlConnection(strCN)
        Dim strSQL As String = "SELECT [fullname], [username] FROM [userconfig] WHERE LEN([guid])>11 ORDER BY [fullname] ASC"
        Dim cmd As New SqlClient.SqlCommand(strSQL, cn)
        Dim ds As New DataSet
        Try
            cn.Open()
            cmd.ExecuteNonQuery()
            Dim da As New SqlDataAdapter(cmd)
            da.Fill(ds)
        Catch ex As Exception
        Finally
            cn.Close()
        End Try

        Dim dt As New DataTable
        dt = ds.Tables(0)
        Dim dict As New Dictionary(Of String, String)
        For Each row As DataRow In dt.Rows
            Try
                dict.Add(row("username"), row("fullname"))
            Catch ex As Exception

            End Try
        Next
        Return dict
    End Function

    Public Function FetchApprovalInfo(ByVal guid$) As String
        Dim strCN$ = ConfigurationManager.ConnectionStrings("aprequestConnectionString").ConnectionString
        Dim cn As New SqlClient.SqlConnection(strCN)
        Dim strSQL As String = "SELECT [isapproved], [approve_user], [appr_ts], [fullname] FROM [transactions] " & _
                "INNER JOIN [userconfig] ON [transactions].[approve_user] = [userconfig].[username] " & _
                "WHERE [transactions].[guid]='" & guid & "'"
        Dim cmd As New SqlClient.SqlCommand(strSQL, cn)
        Dim ds As New DataSet
        Try
            cn.Open()
            cmd.ExecuteNonQuery()
            Dim da As New SqlDataAdapter(cmd)
            da.Fill(ds)
        Catch ex As Exception
        Finally
            cn.Close()
        End Try
        Dim strTmp$ = "Not approved"
        If ds.Tables(0).Rows.Count>0 then
            Dim row As DataRow = ds.Tables(0).Rows(0)
            If CBool(row("isapproved")) Then
                strTmp = "Approved by " & row("fullname")
                If Not IsDBNull(row("appr_ts")) Then
                    strTmp &= " on " & Format(Convert.ToDateTime(row("appr_ts")), "MM/dd/yyyy HH:mm")
                End If
            End If
        End If
        Return strTmp
    End Function

    Private Function FetchSageTableData(ByVal tblName As String, _
            Optional ByVal fld1 As String = "sTitle", _
            Optional ByVal fld2 As String = "sCodeID", _
            Optional ByVal blnShow As Boolean = False) As Dictionary(Of String, String)
        Dim cn As New SqlClient.SqlConnection
        Dim ds As New DataSet
        Dim dt As New DataTable

        Dim strCN As String = ConfigurationManager.ConnectionStrings("SAGE_DBConnectionString").ConnectionString
        cn.ConnectionString = strCN
        Dim cmd As New SqlClient.SqlCommand
        cmd.Connection = cn
        cmd.CommandType = CommandType.Text
        Dim strSQL As String = "SELECT ISNULL([" & fld1 & "],'') AS [" & fld1 & "], " & _
                "ISNULL([" & fld2 & "],'') AS [" & fld2 & "] FROM [" & tblName & "] " & _
                "ORDER BY [" & fld1 & "] ASC"
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
            If blnShow Then
                dict.Add(row(fld2), "[" & row(fld2) & "] " & row(fld1))
            Else
                dict.Add(row(fld2), row(fld1))
            End If
        Next
        Return dict
    End Function

    Public Function CheckComboEntry(ByVal strEnt$, ByVal strSit$, _
            ByVal strPayr$, ByVal strDept$) As Boolean
        If My.Settings.AcceptAllCombos Then Return True
        Dim ds As New DataSet
        Try
            Dim strCN As String = ConfigurationManager.ConnectionStrings("SAGE_DBConnectionString").ConnectionString
            Dim cn As New SqlClient.SqlConnection(strCN)
            Dim cmd As New SqlClient.SqlCommand
            cmd.Connection = cn
            cmd.CommandType = CommandType.Text
            Dim strSQL As String = "SELECT [ctrID] FROM [tblComboEdit] " & _
                    "WHERE [sCodeIDf_1] LIKE @ent AND " & _
                    "([sCodeIDf_3] LIKE @payr OR [sCodeIDf_3]='999999') AND " & _
                    "[sCodeIDf_5] LIKE @dept AND " & _
                    "[sCodeIDf_10] LIKE @sit "
            cmd.CommandText = strSQL
            cmd.Parameters.AddWithValue("@ent", strEnt)
            cmd.Parameters.AddWithValue("@sit", strSit)
            cmd.Parameters.AddWithValue("@payr", strPayr)
            cmd.Parameters.AddWithValue("@dept", strDept)
            cn.Open()
            cmd.ExecuteNonQuery()
            Dim da As New SqlDataAdapter(cmd)
            da.Fill(ds)
        Catch ex As Exception
            Return False
        End Try
        Dim dt As New DataTable
        dt = ds.Tables(0)
        If dt.Rows.Count > 0 Then
            Return True
        Else
            Return False
        End If
    End Function
    'Public Function CheckComboEntry(ByVal strEnt$, ByVal strReg$, ByVal strSit$, _
    '        ByVal strPayr$, ByVal strServ$, ByVal strDept$) As Boolean
    '    If My.Settings.AcceptAllCombos Then Return True
    '    Dim ds As New DataSet
    '    Try
    '        Dim strCN As String = ConfigurationManager.ConnectionStrings("SAGE_DBConnectionString").ConnectionString
    '        Dim cn As New SqlClient.SqlConnection(strCN)
    '        Dim cmd As New SqlClient.SqlCommand
    '        cmd.Connection = cn
    '        cmd.CommandType = CommandType.Text
    '        Dim strSQL As String = "SELECT [ctrID] FROM [tblComboEdit] " & _
    '                "WHERE [sCodeIDf_1] LIKE @ent AND " & _
    '                "[sCodeIDf_2] LIKE @reg AND " & _
    '                "([sCodeIDf_3] LIKE @payr OR [sCodeIDf_2]='999999') AND " & _
    '                "[sCodeIDf_4] LIKE @serv AND " & _
    '                "[sCodeIDf_5] LIKE @dept AND " & _
    '                "[sCodeIDf_10] LIKE @sit "
    '        cmd.CommandText = strSQL
    '        cmd.Parameters.AddWithValue("@ent", strEnt)
    '        cmd.Parameters.AddWithValue("@reg", strReg)
    '        cmd.Parameters.AddWithValue("@sit", strSit)
    '        cmd.Parameters.AddWithValue("@payr", strPayr)
    '        cmd.Parameters.AddWithValue("@serv", strServ)
    '        cmd.Parameters.AddWithValue("@dept", strDept)
    '        cn.Open()
    '        cmd.ExecuteNonQuery()
    '        Dim da As New SqlDataAdapter(cmd)
    '        da.Fill(ds)
    '    Catch ex As Exception
    '        Return False
    '    End Try
    '    Dim dt As New DataTable
    '    dt = ds.Tables(0)
    '    If dt.Rows.Count > 0 Then
    '        Return True
    '    Else
    '        Return False
    '    End If
    'End Function
End Class
